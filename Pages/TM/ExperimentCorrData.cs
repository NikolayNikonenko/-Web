using Microsoft.EntityFrameworkCore;
using перенос_бд_на_Web.Models;

namespace перенос_бд_на_Web.Pages.TM
{
    public class ExperimentCorrData : CorrData
    {
        public ExperimentCorrData(ApplicationContext db) : base(db) { }

        public async Task CalculationCorrelationWithExperimentLabel(
        List<TMValues> originalTMValues,
        string latestExperimentLabel,
        Action<int> progressCallback,
        Action<bool> setStatusBarVisible,
        CancellationToken cancellationToken = default)
        {
            setStatusBarVisible(true); // Отображаем статусбар
            cancellationToken.ThrowIfCancellationRequested();

            if (!originalTMValues.Any())
            {
                Console.WriteLine("Нет данных для расчёта корреляции по указанной метке эксперимента.");
                setStatusBarVisible(true);
                return;
            }
            try 
            {

                // Получаем уникальные пары (IndexTM, Id1, Privyazka)
                var uniqueKeys = originalTMValues
                    .Select(tm => new { tm.IndexTM, tm.Id1, tm.Privyazka })
                    .Distinct()
                    .OrderBy(key => key.IndexTM)
                    .ThenBy(key => key.Id1)
                    .ToList();

                int totalIterations = uniqueKeys.Count;
                int processedCount = 0;
                var newRecords = new List<NedostovernayaTM>();

                foreach (var key in uniqueKeys)
                {
                    var tmValuesForKey = originalTMValues
                        .Where(tm => tm.IndexTM == key.IndexTM && tm.Id1 == key.Id1 && tm.Privyazka == key.Privyazka)
                        .OrderBy(tm => tm.NumberOfSrez)
                        .ToList();

                    // Измеренные, оцененные и лагранжевые значения
                    var allIzmerTM = tmValuesForKey.Select(tm => tm.IzmerValue).ToList();
                    var allOcenTM = tmValuesForKey.Select(tm => tm.OcenValue).ToList();
                    var lagranjValues = tmValuesForKey.Select(tm => tm.Lagranj).ToList();

                    if (!allIzmerTM.Any() || !allOcenTM.Any()) continue;

                    // Расчёт корреляции
                    double avgMeasured = allIzmerTM.Average();
                    double avgEstimated = allOcenTM.Average();

                    double covariance = 0, varianceMeasured = 0, varianceEstimated = 0;

                    for (int i = 0; i < allIzmerTM.Count; i++)
                    {
                        double measuredDiff = allIzmerTM[i] - avgMeasured;
                        double estimatedDiff = allOcenTM[i] - avgEstimated;
                        covariance += measuredDiff * estimatedDiff;
                        varianceMeasured += measuredDiff * measuredDiff;
                        varianceEstimated += estimatedDiff * estimatedDiff;
                    }

                    double denominator = Math.Sqrt(varianceMeasured * varianceEstimated);
                    double correlation = denominator != 0 ? covariance / denominator : 0;

                    // Статус корреляции
                    string status = DetermineStatus(correlation, _correlation_Context);

                    // Максимальный лагранж
                    double maxPositiveLagrange = lagranjValues.Where(x => x > 0).DefaultIfEmpty(0).Max();
                    double maxNegativeLagrange = lagranjValues.Where(x => x < 0).DefaultIfEmpty(0).Min();
                    double avgLagrange = lagranjValues.Any() ? lagranjValues.Average() : 0;
                    double maxAbsoluteLagrange = Math.Abs(maxPositiveLagrange) > Math.Abs(maxNegativeLagrange)
                        ? maxPositiveLagrange
                        : maxNegativeLagrange;

                    newRecords.Add(new NedostovernayaTM
                    {
                        IndexTm = key.IndexTM,
                        Id1 = key.Id1,
                        NameTM = tmValuesForKey.FirstOrDefault()?.NameTM ?? string.Empty,
                        CorrTm = correlation,
                        Status = status,
                        MaxLagranj = maxAbsoluteLagrange,
                        AvgLagranj = avgLagrange,
                        experiment_label = latestExperimentLabel
                    });

                    processedCount++;
                    int progress = (int)((double)processedCount / totalIterations * 100);

                    //Обновление прогресса на каждой итерации
                    progressCallback(progress);
                }

                await _correlation_Context.tm.AddRangeAsync(newRecords);
                await _correlation_Context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Ошибка при сохранении данных: {ex.Message}");
            }
            finally
            {
                setStatusBarVisible(false); // Скрываем статусбар после завершения

            }
        }
    }
}
