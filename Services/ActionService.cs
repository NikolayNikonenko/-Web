using ASTRALib;
using Microsoft.Extensions.Options;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using перенос_бд_на_Web.Models;
using static System.Formats.Asn1.AsnWriter;

namespace перенос_бд_на_Web.Services
{
    public class ActionService: IDisposable
    {

        private readonly ISliceService _sliceService;
        private readonly IRastr _rastr;
        private readonly string _fullSaveDirectory;
        private readonly IServiceScopeFactory _scopeFactory;

        public ActionService(ISliceService sliceService, IServiceScopeFactory scopeFactory)
        {
            _sliceService = sliceService;
            _scopeFactory = scopeFactory;
            _rastr = new Rastr(); // Инициализация IRastr
            _fullSaveDirectory = "D:\\учеба\\магистратура\\3 курс\\диплом ит\\мое\\тесты сохранения файлов";
        }

        // Метод для выполнения действия
        public async Task ExecuteAction(List<VerificationAction> actions)
        {
            // Получаем файлы в диапазоне для всех действий сразу
            var startDate = actions.Min(a => a.StartDate);
            var endDate = actions.Max(a => a.EndDate);
            var filePathsInRange = await GetFilePathsInRangeAsync(startDate, endDate);

            foreach (var path in filePathsInRange)
            {
                try
                {
                    _rastr.Load(RG_KOD.RG_REPL, path, "");
                    bool hasChanges = false;

                    foreach (var actionGroup in actions.GroupBy(a => a.ActionName))
                    {
                        switch (actionGroup.Key)
                        {
                            case "Изменить знак ТМ":
                                hasChanges |= await ChangeSign(actionGroup.ToList());
                                break;

                            case "Создать дорасчет":
                                hasChanges |= await CreateRecalculation(actionGroup.ToList());
                                break;

                            case "Исключить из ОС":
                                hasChanges |= await ExcludeFromOS(actionGroup.ToList());
                                break;

                            default:
                                Console.WriteLine("Не выбрано действие для выполнения.");
                                break;
                        }
                    }

                    if (hasChanges)
                    {
                        SaveSlice(path);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обработке файла {path}: {ex.Message}");
                }
            }
        }

        private async Task<List<string>> GetFilePathsInRangeAsync(DateTime startTime, DateTime endTime)
        {
            return await _sliceService.GetFilePathsInRangeAsync(startTime, endTime);
        }

        private async Task SaveSlice(string path)
        {

            var pathParts = path.Split('\\');
            if (pathParts.Length >= 3)
            {
                string subFolder1 = pathParts[^3];
                string subFolder2 = pathParts[^2];
                string saveDirectory = System.IO.Path.Combine(_fullSaveDirectory, subFolder1, subFolder2);

                System.IO.Directory.CreateDirectory(saveDirectory);
                string saveFilePath = System.IO.Path.Combine(saveDirectory, $"{subFolder2}.rg2");

                // Оценка состояния перед сохранением данных
                _rastr.opf("s");

                // Сохраняем срез
                _rastr.Save(saveFilePath, "");
                Console.WriteLine($"Срез сохранен в: {saveFilePath}");

                // Сохраняем путь к файлу в базе данных
                var experimentFileId = await SaveFilePathToDatabase(saveFilePath);

                if (experimentFileId != Guid.Empty)
                {
                    // Заполняем таблицу ModifiedTMValues
                    await SaveModifiedTMValues(experimentFileId);
                }
            }
        }

        private async Task<Guid> SaveFilePathToDatabase(string path)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            var experimentFile = new ExperimentFiles
            {
                id_file_after_modified = Guid.NewGuid(),
                path_experiment_file = path,
            };

            context.experiment_file.Add(experimentFile);
            await context.SaveChangesAsync();
            Console.WriteLine($"Путь к файлу сохранен в БД: {path}");
            return experimentFile.id_file_after_modified;
        }

        // Метод для заполнения таблицы ModifiedTMValues
        private async Task SaveModifiedTMValues(Guid idFileAfterModified)
        {
            ITable tableTIChannel = (ITable)_rastr.Tables.Item("ti");
            ICol numCol = (ICol)tableTIChannel.Cols.Item("Num");
            ICol izmZnach = (ICol)tableTIChannel.Cols.Item("ti_val");
            ICol ocenZnach = (ICol)tableTIChannel.Cols.Item("ti_ocen");
            ICol lagrZnach = (ICol)tableTIChannel.Cols.Item("lagr");
            ICol id1Col = (ICol)tableTIChannel.Cols.Item("id1");
            ICol typeTM = (ICol)tableTIChannel.Cols.Item("type");
            ICol cod_v_OC = (ICol)tableTIChannel.Cols.Item("cod_oc");
            int rowCount = tableTIChannel.Count;

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            for (int i = 0; i < rowCount; i++)
            {
                var numberValue = Convert.ToInt32(numCol.get_ZN(i));
                var id1Value = Convert.ToInt32(id1Col.get_ZN(i));

                if (!IsRelevantTM(typeTM, cod_v_OC, i))
                {
                    continue; // Пропускаем строки, не соответствующие условию
                }

                var modifiedValue = new ModifiedTMValues
                {
                    id_tm_value_after_modified = Guid.NewGuid(),
                    id_tm_after_modified = $"{numberValue}_{id1Value}", // Формируем составной ключ
                    izmer_tm_value_after_modified = Convert.ToDouble(izmZnach.get_ZN(i)),
                    ocen_tm_value_after_modified = Convert.ToDouble(ocenZnach.get_ZN(i)),
                    lagranj_tm_value_after_modified = Convert.ToDouble(lagrZnach.get_ZN(i)),
                    id_file_after_modified = idFileAfterModified,

                };

                context.tm_values_after_verification.Add(modifiedValue);
            }

            await context.SaveChangesAsync();
            Console.WriteLine("Данные сохранены в таблицу ModifiedTMValues.");
        }

        private static bool IsRelevantTM(ICol typeTM, ICol cod_v_OC, int numTm)
        {
            return (((int)typeTM.get_ZN(numTm) == 0) || ((int)typeTM.get_ZN(numTm) == 2)) && ((int)cod_v_OC.get_ZN(numTm) == 1);
        }


        // Пример реализации методов для каждого действия
        private async Task<bool> ChangeSign(List<VerificationAction> actions)
        {
            Console.WriteLine("Изменение знака для ТМ");
            bool hasChanges = false;

            ITable tableTIChannel = (ITable)_rastr.Tables.Item("ti");
            ICol izmZnach = (ICol)tableTIChannel.Cols.Item("ti_val");

            foreach (var action in actions)
            {
                tableTIChannel.SetSel($"Num={action.TelemetryId}");
                int n = tableTIChannel.FindNextSel[-1];
                if (n >= 0)
                {
                    double znachdoKorr = (double)izmZnach.get_ZN(n);
                    izmZnach.set_ZN(n, znachdoKorr * -1);
                    hasChanges = true;
                    Console.WriteLine($"Знак изменен для ТМ {action.TelemetryId}");
                }
            }

            return hasChanges;
        }

        private async Task<bool> CreateRecalculation(List<VerificationAction> actions)
        {
            Console.WriteLine("Создание дорасчета для ТМ");
            bool hasChanges = false;

            foreach (var action in actions)
            {
                Console.WriteLine($"Создан дорасчет для ТМ {action.TelemetryId}");
                hasChanges = true; // Фиксируем изменение для вызова SaveSlice
            }

            return hasChanges;
        }


        private async Task<bool> ExcludeFromOS(List<VerificationAction> actions)
        {
            bool hasChanges = false;

            ITable tableTIChannel = (ITable)_rastr.Tables.Item("ti");
            ICol status = (ICol)tableTIChannel.Cols.Item("sta");

            foreach (var action in actions)
            {
                tableTIChannel.SetSel($"Num={action.TelemetryId}");
                int n = tableTIChannel.FindNextSel[-1];
                if (n >= 0)
                {
                    status.set_ZN(n, true);
                    Console.WriteLine($"Исключен из ОС для ТМ {action.TelemetryId}");
                    hasChanges = true; // Фиксируем изменение для вызова SaveSlice
                }
            }

            return hasChanges;
        }

        public void Dispose()
        {
            if (_rastr != null)
            {
                Marshal.ReleaseComObject(_rastr);
                Console.WriteLine("IRastr освобожден.");
            }
        }
    }
}
