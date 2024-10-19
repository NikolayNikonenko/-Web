using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using перенос_бд_на_Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace перенос_бд_на_Web.Pages.TM
{
    public class SomeDataTM : PageModel
    {
        private readonly ApplicationContext _telemetry_Context;
        private readonly CorrData _correlation; // Включаем CorrData для расчета корреляции

        public List<NedostovernayaTM> Tm { get; set; }
        public bool IsCalculating { get; set; } // Поле для отслеживания статуса загрузки
        private bool isStatusBarVisible; // Объявляем переменную для статусбара

        // Если у вас используется Dependency Injection, то конструктор может быть следующим:
        public SomeDataTM(ApplicationContext db)
        {
            _telemetry_Context = db;
            _correlation = new CorrData(db); // Инициализация CorrData
        }

        public void OnGet()
        {
            Tm = _telemetry_Context.tm.AsNoTracking().ToList();
            IsCalculating = false; // Изначально расчет не выполняется
        }

        // Метод для запуска расчета
        public async Task<IActionResult> OnPostCalculateCorrelation()
        {
            IsCalculating = true; // Устанавливаем статус выполнения

            // Объявляем переменные для фильтрации (по необходимости можно изменить)
            DateTime? startTime = null; // Установите значения при необходимости
            DateTime? endTime = null;

            // Запускаем расчет корреляции и передаем прогресс в виде колбэка
            await _correlation.CalculationCorrelation(
                null, // Здесь можно передать отфильтрованные значения, если нужно
                progress => {
                    // Обновление прогресса в UI можно реализовать через SignalR или другие механизмы
                    Console.WriteLine($"Прогресс: {progress}%"); // Логируем прогресс
                },
                SetStatusBarVisible, // Метод для управления видимостью статусбара
                startTime,
                endTime);

            IsCalculating = false; // Устанавливаем статус завершения

            return Page(); // Обновляем страницу после завершения расчета
        }
        private void SetStatusBarVisible(bool isVisible)
        {
            isStatusBarVisible = isVisible; // Устанавливаем видимость статусбара
        }

    }

    public class CorrData
    {
        private readonly ApplicationContext _correlation_Context;

        // Добавим поля для хранения данных
        public List<TMValues> TMValues { get; set; } = new List<TMValues>();
        private bool wasFilteringPerformed = false; // Флаг, был ли выполнен фильтр или прореживание

        public CorrData(ApplicationContext db)
        {
            _correlation_Context = db;
        }

        // Метод для установки флага прореживания
        public void SetFilteringPerformed(bool performed)
        {
            wasFilteringPerformed = performed;
        }

        public async Task CalculationCorrelation(
            List<TMValues> filteredTMValues,
            Action<int> progressCallback,
            Action<bool> setStatusBarVisible, 
            DateTime? startTime = null,
            DateTime? endTime = null,
            bool useThinning = false,
            int thinningStep = 1)

        {
            // Показываем статусбар
            setStatusBarVisible(true);
            // Проверяем, есть ли отфильтрованные данные
            if ((filteredTMValues == null || !filteredTMValues.Any()) && (startTime.HasValue && endTime.HasValue))
            {
                // Если нет, то загружаем данные из базы, отфильтрованные по времени
                var allTMValues = await _correlation_Context.TMValues.AsNoTracking().ToListAsync();

                filteredTMValues = allTMValues
                    .Where(t => DateTime.TryParseExact(t.NumberOfSrez, "HH_mm_ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime numberOfSrezTime) &&
                                numberOfSrezTime >= startTime.Value && numberOfSrezTime <= endTime.Value)
                    .ToList();

                Console.WriteLine("Данные загружены из базы данных по заданному интервалу времени.");
            }
            else if (filteredTMValues == null || !filteredTMValues.Any())
            {
                // Если нет данных и время не задано, загружаем все данные
                filteredTMValues = await _correlation_Context.TMValues.AsNoTracking().ToListAsync();

                Console.WriteLine("Данные загружены из базы данных без фильтрации по времени.");
            }

            if (!filteredTMValues.Any())
            {
                Console.WriteLine("Нет данных для расчета корреляции.");
                return;
            }

            // Извлекаем все уникальные индексы телеметрии
            var uniqueOrdersTM = filteredTMValues
                .Select(s => s.IndexTM)
                .Distinct()
                .OrderBy(indexTM => indexTM)
                .ToList();

            var answer = new Dictionary<int, double>();
            int totalIterations = uniqueOrdersTM.Count;
            int processedCount = 0;

            foreach (var uniqueOrderTM in uniqueOrdersTM)
            {
                // Получаем данные для текущего индекса телеметрии
                var tmValuesForIndex = filteredTMValues
                    .Where(t => t.IndexTM == uniqueOrderTM)
                    .OrderBy(e => e.NumberOfSrez)
                    .ToList();

                // Получаем измеренные и оценочные значения
                var allIzmerTM = tmValuesForIndex.Select(s => s.IzmerValue).ToList();
                var allOcenTM = tmValuesForIndex.Select(s => s.OcenValue).ToList();

                // Получаем данные по Лагранжу
                var lagranjValues = tmValuesForIndex.Select(s => s.Lagranj).ToList();

                if (useThinning)
                {
                    // Применяем прореживание
                    allIzmerTM = allIzmerTM.Where((value, index) => index % thinningStep == 0).ToList();
                    allOcenTM = allOcenTM.Where((value, index) => index % thinningStep == 0).ToList();
                    lagranjValues = lagranjValues.Where((value, index) => index % thinningStep == 0).ToList();
                }

                if (allIzmerTM.Count == 0 || allOcenTM.Count == 0)
                    continue; // Пропускаем, если нет данных для корреляции

                // Рассчитываем средние значения
                double avgMeasured = allIzmerTM.Average();
                double avgEstimated = allOcenTM.Average();

                double covariance = 0;
                double varianceMeasured = 0;
                double varianceEstimated = 0;

                // Рассчитываем ковариацию и дисперсию
                for (int i = 0; i < allIzmerTM.Count; i++)
                {
                    double measuredDiff = allIzmerTM[i] - avgMeasured;
                    double estimatedDiff = allOcenTM[i] - avgEstimated;
                    covariance += measuredDiff * estimatedDiff;
                    varianceMeasured += measuredDiff * measuredDiff;
                    varianceEstimated += estimatedDiff * estimatedDiff;
                }

                double denominator = Math.Sqrt(varianceMeasured * varianceEstimated);

                // Проверяем на ноль, чтобы избежать деления на ноль
                double correlation = denominator != 0 ? covariance / denominator : 0;
                answer[(int)uniqueOrderTM] = correlation;

                // Определяем статус на основе корреляции
                string status = DetermineStatus(correlation);

                // Рассчитываем максимальные и средние значения Лагранжа
                double maxPositiveLagrange = lagranjValues.Where(x => x > 0).DefaultIfEmpty(0).Max();
                double maxNegativeLagrange = lagranjValues.Where(x => x < 0).DefaultIfEmpty(0).Min();
                double avgLagrange = lagranjValues.Any() ? lagranjValues.Average() : 0;
                double maxAbsoluteLagrange = Math.Abs(maxPositiveLagrange) > Math.Abs(maxNegativeLagrange)
                    ? maxPositiveLagrange
                    : maxNegativeLagrange;

                // Извлекаем значение NameTM для текущего индекса TM
                var nameTM = tmValuesForIndex
                    .Select(t => t.NameTM)
                    .FirstOrDefault();

                // Проверяем, есть ли уже запись в базе данных
                var existingRecord = await _correlation_Context.tm
                    .FirstOrDefaultAsync(z => z.IndexTm == uniqueOrderTM);

                if (existingRecord != null)
                {
                    // Обновляем существующую запись
                    existingRecord.CorrTm = correlation;
                    existingRecord.Status = status;
                    existingRecord.NameTM = nameTM;
                    existingRecord.MaxLagranj = maxAbsoluteLagrange;
                    existingRecord.AvgLagranj = avgLagrange;
                }
                else
                {
                    // Создаем новую запись
                    var newRecord = new NedostovernayaTM
                    {
                        IndexTm = uniqueOrderTM,
                        CorrTm = correlation,
                        Status = status,
                        NameTM = nameTM,
                        MaxLagranj = maxAbsoluteLagrange,
                        AvgLagranj = avgLagrange
                    };

                    _correlation_Context.tm.Add(newRecord);
                }

                try
                {
                    // Сохраняем изменения в базе данных
                    await _correlation_Context.SaveChangesAsync();
                    Console.WriteLine($"Телеметрия под номером {uniqueOrderTM} успешно сохранена в БД");
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"Ошибка при сохранении данных: {ex.Message}");
                }
                processedCount++;
                int progress = (int)((double)processedCount / totalIterations * 100);
                progressCallback(progress);
            }
            setStatusBarVisible(false);
            Console.WriteLine("Расчет корреляции завершен.");
        }

        string DetermineStatus(double correlation)
        {
            if (correlation >= -1 && correlation < -0.5) return "Недостоверная";
            if (correlation >= -0.5 && correlation < 0.5) return "Сомнительная";
            if (correlation >= 0.5 && correlation <= 1) return "Достоверная";
            return "Неопределено";
        }
    }
}




