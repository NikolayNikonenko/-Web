using ASTRALib;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using перенос_бд_на_Web.Models;
using перенос_бд_на_Web.Pages.TM;
using Microsoft.EntityFrameworkCore;
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
        public async Task ExecuteAction(
            List<VerificationAction> actions,
            List<TMValues> tmValues,
            Action<int> progressCallback,
            Action<bool> setStatusBarVisible
            )
        {
            setStatusBarVisible(true); // Отображаем статус-бар
            try
            {
                // Создаём scope на уровне метода
                using var scope = _scopeFactory.CreateScope();
                var serviceProvider = scope.ServiceProvider;
                var context = serviceProvider.GetRequiredService<ApplicationContext>();
                var telemetryMonitoringService = serviceProvider.GetRequiredService<TelemetryMonitoringService>();
                // Подключаем сервис для работы с TM
                var corrDataService = serviceProvider.GetRequiredService<ExperimentCorrData>();

                // Генерируем метку эксперимента один раз
                var experimentLabel = await telemetryMonitoringService.GetNextExperimentLabelAsync();

                var filePathsInRange = await _sliceService.GetFilePathsInRangeAsync(
                actions.Min(a => a.StartDate),
                actions.Max(a => a.EndDate)
                );

                // Всего операций: обработка файлов + расчет корреляции
                int totalOperations = filePathsInRange.Count;
                int completedOperations = 0;

                int orderIndex = 0;

                foreach (var path in filePathsInRange)
                {
                    try
                    {
                        _rastr.Load(RG_KOD.RG_REPL, path, "");
                        bool hasChanges = await ProcessFile(path, orderIndex, tmValues,  actions, context, telemetryMonitoringService, experimentLabel);

                        if (hasChanges)
                        {
                            completedOperations++;
                            int progress = (int)((double)completedOperations / totalOperations * 100);
                            progressCallback(progress);
                            orderIndex++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при обработке файла {path}: {ex.Message}");
                    }
                }

                // Расчет корреляции
                tmValues = await context.TMValues
                    .Where(tm => tm.experiment_label == experimentLabel)
                    .ToListAsync();

                await corrDataService.CalculationCorrelationWithExperimentLabel(
                    tmValues,
                    experimentLabel, 
                    progress=>
                    {
                    },
                    setStatusBarVisible,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при расчете корреляции: {ex.Message}");
            }
            finally
            {
                setStatusBarVisible(false); // Скрываем статус-бар после завершения
            }
        }

        private async Task<bool> ProcessFile(
        string path,
        int orderIndex,
        List<TMValues> tMValues,
        List<VerificationAction> actions,
        ApplicationContext context,
        TelemetryMonitoringService telemetryMonitoringService,
        string experimentLabel)
        {
            // Реализация обработки файла и возврат флага изменений
            bool hasChanges = false;

            foreach (var actionGroup in actions.GroupBy(a => new { a.TelemetryId, a.Id1 }))
            {
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
                    }
                }
            }

            if (hasChanges)
            {
                await SaveSlice(path, orderIndex, tMValues, context, telemetryMonitoringService, experimentLabel);
            }

            return hasChanges;
        }

        private async Task SaveSlice(
            string path,
            double orderIndex,
            List<TMValues> tmValues,
            ApplicationContext context,
            TelemetryMonitoringService telemetryMonitoringService,
            string experimentLabel
            )
        {
            try
            {

                // Парсим путь и создаем директорию
                var (saveFilePath, subFolder2) = PrepareSaveDirectory(path, experimentLabel);

                // Оценка состояния перед сохранением данных
                _rastr.opf("s");

                // Сохраняем срез
                _rastr.Save(saveFilePath, "");
                Console.WriteLine($"Срез сохранен в: {saveFilePath}");

                // Начинаем транзакцию
                await using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // Получаем следующую метку эксперимента
                    //var nextExperimentLabel = await telemetryMonitoringService.GetNextExperimentLabelAsync();

                    // Сохраняем путь в таблицу Slices
                    var experimentFileId = await SaveFilePathToSlicesTable(saveFilePath, subFolder2, context, experimentLabel);

                    if (experimentFileId != Guid.Empty)
                    {
                        // Сохраняем модифицированные значения TMValues
                        await SaveModifiedTMValues(
                            experimentFileId,
                            subFolder2,
                            orderIndex,
                            context,
                            experimentLabel,
                            tmValues
                        );
                    }

                    // Подтверждаем транзакцию
                    await transaction.CommitAsync();
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

        private (string saveFilePath, string subFolder2) PrepareSaveDirectory(string path, string experimentLabel)
        {
            var pathParts = path.Split('\\');
            if (pathParts.Length < 3)
                throw new ArgumentException("Некорректный путь: недостаточно частей в пути.", nameof(path));

            string subFolder1 = pathParts[^3];
            string subFolder2 = pathParts[^2];

            // Формируем текущую дату и время
            string currentDate = DateTime.Now.ToString("dd_MM_yy");


            string saveDirectory = Path.Combine(
         _fullSaveDirectory,                     
         currentDate,
         experimentLabel,
         subFolder2                              
         );


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
            await context.SaveChangesAsync();

            Console.WriteLine($"Путь к файлу сохранен в таблицу Slices с меткой эксперимента: {nextExperimentLabel}");

            return sliceRecord.SliceID;
        }

        // Метод для заполнения таблицы ModifiedTMValues
        private async Task SaveModifiedTMValues(
        Guid idFileAfterModified,
        string sliceName,
        double orderIndex,
        ApplicationContext context,
        string nextExperimentLabel,
        List<TMValues> tmValues)
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

            // Преобразуем список tmValues в HashSet для быстрого поиска
            var tmValuesSet = tmValues
                .Select(tv => new { tv.IndexTM, tv.Id1, tv.Privyazka })
                .ToHashSet();

            var newValues = new List<TMValues>();

            // Перебираем каждое уникальное значение из входного списка
            foreach (var tmValue in tmValues.DistinctBy(tv => new { tv.IndexTM, tv.Id1, tv.Privyazka }))
            {
                // Фильтруем строки таблицы по текущему IndexTM
                tableTIChannel.SetSel($"Num={(int)tmValue.IndexTM}");
                int n = tableTIChannel.FindNextSel[-1];

                while (n != -1)
                {
                    // Считываем значения полей из текущей строки
                    var currentCombination = new
                    {
                        IndexTM = Convert.ToDouble(numCol.get_ZN(n)),
                        Id1 = Convert.ToInt32(id1Col.get_ZN(n)),
                        Privyazka = Convert.ToString(privyazka.get_ZN(n))
                    };

                    // Проверяем, совпадает ли сочетание с текущей записью из tmValues
                    if (!tmValuesSet.Contains(currentCombination))
                    {
                        n = tableTIChannel.FindNextSel[n];
                        continue;
                    }

                    // Проверяем, что запись ещё не добавлена
                    if (newValues.Any(v =>
                        v.IndexTM == currentCombination.IndexTM &&
                        v.Id1 == currentCombination.Id1 &&
                        v.Privyazka == currentCombination.Privyazka))
                    {
                        n = tableTIChannel.FindNextSel[n];
                        continue;
                    }

                    // Проверяем дополнительные условия
                    if (!IsRelevantTM(typeTM, cod_v_OC, n))
                    {
                        n = tableTIChannel.FindNextSel[n];
                        continue;
                    }

                    // Создаём новую запись для сохранения
                    var modifiedValue = new TMValues
                    {
                        ID = Guid.NewGuid(),
                        IndexTM = currentCombination.IndexTM,
                        IzmerValue = Convert.ToDouble(izmZnach.get_ZN(n)),
                        OcenValue = Convert.ToDouble(ocenZnach.get_ZN(n)),
                        Privyazka = currentCombination.Privyazka,
                        Id1 = currentCombination.Id1,
                        NameTM = Convert.ToString(NameTm.get_ZN(n)),
                        NumberOfSrez = sliceName,
                        OrderIndex = orderIndex,
                        DeltaOcenIzmer = Convert.ToDouble(Delta.get_ZN(n)),
                        SliceID = idFileAfterModified,
                        Lagranj = Convert.ToDouble(lagrZnach.get_ZN(n)),
                        experiment_label = nextExperimentLabel
                    };

                    newValues.Add(modifiedValue);

                    // Переходим к следующей строке
                    n = tableTIChannel.FindNextSel[n];
                }
            }

            // Сохраняем найденные записи одним запросом
            if (newValues.Any())
            {
                await context.TMValues.AddRangeAsync(newValues);
                await context.SaveChangesAsync();
            }

            Console.WriteLine($"Сохранено {newValues.Count} новых записей в таблицу TMValues.");
        }

        private static bool IsRelevantTM(ICol typeTM, ICol cod_v_OC, int numTm)
        {
            return (((int)typeTM.get_ZN(numTm) == 0) || ((int)typeTM.get_ZN(numTm) == 2));
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
