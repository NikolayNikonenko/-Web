using ASTRALib;
using Microsoft.Extensions.Options;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using перенос_бд_на_Web.Models;

namespace перенос_бд_на_Web.Services
{
    public class ActionService: IDisposable
    {

        private readonly ISliceService _sliceService;
        private readonly IRastr _rastr;
        private readonly string _fullSaveDirectory;
        private readonly ApplicationContext _context;

        public ActionService(ISliceService sliceService, ApplicationContext context)
        {
            _sliceService = sliceService;
            _context = context;
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
                        await SaveFilePathToDatabase(path);
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

        private void SaveSlice(string path)
        {
            var pathParts = path.Split('\\');
            if (pathParts.Length >= 3)
            {
                string subFolder1 = pathParts[^3];
                string subFolder2 = pathParts[^2];
                string saveDirectory = System.IO.Path.Combine(_fullSaveDirectory, subFolder1, subFolder2);

                System.IO.Directory.CreateDirectory(saveDirectory);
                string saveFilePath = System.IO.Path.Combine(saveDirectory, $"{subFolder2}.rg2");

                _rastr.rgm("");
                _rastr.opf("s");
                _rastr.Save(saveFilePath, "");
                Console.WriteLine($"Срез сохранен в: {saveFilePath}");
            }
        }

        private async Task SaveFilePathToDatabase(string path)
        {
            var experimentFile = new ExperimentFiles
            {
                Id_file = Guid.NewGuid(),
                path_experiment_file = path,
                Id_experiment = Guid.NewGuid() // Можно указать существующий Id эксперимента, если он известен
            };

            _context.experiment_file.Add(experimentFile);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Путь к файлу сохранен в БД: {path}");
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
