using ASTRALib;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task ExecuteAction(List<VerificationAction> actions, List<TMValues> tmValues)
        {

            // Получаем файлы в диапазоне для всех действий сразу
            var startDate = actions.Min(a => a.StartDate);
            var endDate = actions.Max(a => a.EndDate);
            var filePathsInRange = await GetFilePathsInRangeAsync(startDate, endDate);
            int orderIndex = 0;

            foreach (var path in filePathsInRange)
            {
                try
                {
                    _rastr.Load(RG_KOD.RG_REPL, path, "");
                    bool hasChanges = false;

                    // Группируем действия по комбинации TelemetryId и Id1
                    foreach (var actionGroup in actions.GroupBy(a => new { a.TelemetryId, a.Id1 }))
                    {
                        var key = actionGroup.Key;  // Получаем ключ (TelemetryId, Id1)

                        // Здесь можно использовать key.TelemetryId и key.Id1 для сравнения
                        foreach (var action in actionGroup)
                        {
                            switch (action.ActionName)
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
                    }

                    if (hasChanges)
                    {
                        orderIndex++;
                        SaveSlice(path, orderIndex, tmValues);
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

        private async Task SaveSlice(string path, double orderIndex, List<TMValues> tmValues)
        {
            try
            {
                // Парсим путь и создаем директорию
                var (saveFilePath, subFolder2) = PrepareSaveDirectory(path);

                // Оценка состояния перед сохранением данных
                _rastr.opf("s");

                // Сохраняем срез
                _rastr.Save(saveFilePath, "");
                Console.WriteLine($"Срез сохранен в: {saveFilePath}");

                // Создаем scope для работы с контекстом
                using var scope = _scopeFactory.CreateScope();
                var serviceProvider = scope.ServiceProvider;

                var context = serviceProvider.GetRequiredService<ApplicationContext>();
                var telemetryMonitoringService = serviceProvider.GetRequiredService<TelemetryMonitoringService>();

                // Начинаем транзакцию
                await using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // Получаем следующую метку эксперимента
                    var nextExperimentLabel = await telemetryMonitoringService.GetNextExperimentLabelAsync();

                    // Сохраняем путь в таблицу Slices
                    var experimentFileId = await SaveFilePathToSlicesTable(saveFilePath, subFolder2, context, nextExperimentLabel);

                    if (experimentFileId != Guid.Empty)
                    {
                        // Сохраняем модифицированные значения TMValues
                        await SaveModifiedTMValues(
                            experimentFileId,
                            subFolder2,
                            orderIndex,
                            context,
                            nextExperimentLabel,
                            tmValues
                        );
                    }

                    // Подтверждаем транзакцию
                    await transaction.CommitAsync();
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Откатываем транзакцию при ошибке
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Ошибка при сохранении данных в транзакции: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Общая ошибка в SaveSlice: {ex.Message}");
                throw;
            }
        }

        private (string saveFilePath, string subFolder2) PrepareSaveDirectory(string path)
        {
            var pathParts = path.Split('\\');
            if (pathParts.Length < 3)
                throw new ArgumentException("Некорректный путь: недостаточно частей в пути.", nameof(path));

            string subFolder1 = pathParts[^3];
            string subFolder2 = pathParts[^2];
            string saveDirectory = Path.Combine(_fullSaveDirectory, subFolder1, subFolder2);

            // Создаем директорию только если она не существует
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            string saveFilePath = Path.Combine(saveDirectory, $"{subFolder2}.rg2");
            return (saveFilePath, subFolder2);
        }

        private async Task<Guid> SaveFilePathToSlicesTable(string path, string sliceName, ApplicationContext context, string nextExperimentLabel)
        {
            // Создаем запись в таблице Slices
            var sliceRecord = new Slices
            {
                SliceID = Guid.NewGuid(),
                SliceName = sliceName,
                SlicePath = path,
                experiment_label = nextExperimentLabel
            };

            // Добавляем и сохраняем запись в таблице Slices
            context.slices.Add(sliceRecord);
            //await context.SaveChangesAsync();

            Console.WriteLine($"Путь к файлу сохранен в таблицу Slices с меткой эксперимента: {nextExperimentLabel}");

            return sliceRecord.SliceID;
        }

        // Метод для заполнения таблицы ModifiedTMValues
        private async Task SaveModifiedTMValues(Guid idFileAfterModified, string sliceName, double orderIndex, ApplicationContext context, string nextExperimentLabel, List<TMValues> tmValues)
        {
            ITable tableTIChannel = (ITable)_rastr.Tables.Item("ti");
            ICol numCol = (ICol)tableTIChannel.Cols.Item("Num");
            ICol izmZnach = (ICol)tableTIChannel.Cols.Item("ti_val");
            ICol ocenZnach = (ICol)tableTIChannel.Cols.Item("ti_ocen");
            ICol privyazka = (ICol)tableTIChannel.Cols.Item("prv_num");
            ICol lagrZnach = (ICol)tableTIChannel.Cols.Item("lagr");
            ICol id1Col = (ICol)tableTIChannel.Cols.Item("id1");
            ICol NameTm = (ICol)tableTIChannel.Cols.Item("name");
            ICol Delta = (ICol)tableTIChannel.Cols.Item("dif_oc");
            ICol typeTM = (ICol)tableTIChannel.Cols.Item("type");
            ICol cod_v_OC = (ICol)tableTIChannel.Cols.Item("cod_oc");
            int rowCount = tableTIChannel.Count;

            var relevantCombination = tmValues
            .Select(tv => new { tv.IndexTM, tv.Id1, tv.Privyazka })
            .ToHashSet();

            foreach (var tmValue in tmValues)
            {
                // Устанавливаем фильтр для текущего элемента
                tableTIChannel.SetSel($"Num={(int)tmValue.IndexTM}");
                // Ищем первую подходящую строку
                int n = tableTIChannel.FindNextSel[-1];
                while (n != -1)
                {
                    // Пропускаем строки, не соответствующие дополнительным условиям
                    //if (!IsRelevantTM(typeTM, cod_v_OC, n))
                    //{
                    //    n = tableTIChannel.FindNextSel[n];
                    //    continue;
                    //}
                    // Создаем объект TMValues для сохранения
                    var modifiedValue = new TMValues
                    {
                        ID = Guid.NewGuid(),
                        IndexTM = Convert.ToDouble(numCol.get_ZN(n)),
                        IzmerValue = Convert.ToDouble(izmZnach.get_ZN(n)),
                        OcenValue = Convert.ToDouble(ocenZnach.get_ZN(n)),
                        Privyazka = Convert.ToString(privyazka.get_ZN(n)),
                        Id1 = Convert.ToInt32(id1Col.get_ZN(n)),
                        NameTM = Convert.ToString(NameTm.get_ZN(n)),
                        NumberOfSrez = sliceName,
                        OrderIndex = orderIndex,
                        DeltaOcenIzmer = Convert.ToDouble(Delta.get_ZN(n)),
                        SliceID = idFileAfterModified,
                        Lagranj = Convert.ToDouble(lagrZnach.get_ZN(n)),
                        experiment_label = nextExperimentLabel
                    };

                    context.TMValues.Add(modifiedValue);

                    // Ищем следующую строку
                    n = tableTIChannel.FindNextSel[n];
                }
            }
            //try
            //{
            //    Console.WriteLine("Начинаем сохранение изменений...");
            //    var affectedRows = await context.SaveChangesAsync();

            //    if (affectedRows > 0)
            //    {
            //        Console.WriteLine($"Успешно сохранено {affectedRows} записей.");
            //    }
            //    else
            //    {
            //        Console.WriteLine("Изменения не были сохранены. Проверьте данные для записи.");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Ошибка при сохранении данных: {ex.Message}");
            //}

            //// Проверка содержимого контекста после добавления данных
            //if (!context.TMValues.Any())
            //{
            //    Console.WriteLine("Контекст не содержит записей TMValues после добавления. Проверьте исходные данные и настройки контекста.");
            //}

            //Console.WriteLine("Данные сохранены в таблицу ModifiedTMValues.");
        }

        //private static bool IsRelevantTM(ICol typeTM, ICol cod_v_OC, int numTm)
        //{
        //    return (((int)typeTM.get_ZN(numTm) == 0) || ((int)typeTM.get_ZN(numTm) == 2)) && ((int)cod_v_OC.get_ZN(numTm) == 1);
        //}


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
