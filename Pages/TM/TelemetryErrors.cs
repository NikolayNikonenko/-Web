using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using перенос_бд_на_Web.Models;

public class CorrData
{
    public ApplicationContext _correlation_Context;

    public CorrData(ApplicationContext db)
    {
        _correlation_Context = db;
    }

    public async Task CalculationCorrelation(
        List<TMValues> filteredTMValues,
        Action<int> progressCallback,
        Action<bool> setStatusBarVisible,
        string originalDataSet,
        DateTime? startTime = null,
        DateTime? endTime = null,
        CancellationToken cancellationToken = default
        )
    {
        setStatusBarVisible(true); // Отображаем статусбар один раз в начале
        cancellationToken.ThrowIfCancellationRequested();

        if ((filteredTMValues == null || !filteredTMValues.Any()) && (startTime.HasValue && endTime.HasValue))
        {
            var allTMValues = await _correlation_Context.TMValues
                .AsNoTracking()
                .Where(t => t.experiment_label == originalDataSet)
                .ToListAsync();

            filteredTMValues = allTMValues
                .Where(t => DateTime.TryParseExact(t.NumberOfSrez, "HH_mm_ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime numberOfSrezTime) &&
                            numberOfSrezTime >= startTime.Value && numberOfSrezTime <= endTime.Value)
                .ToList();
        }
        else if (filteredTMValues == null || !filteredTMValues.Any())
        {
            filteredTMValues = await _correlation_Context.TMValues
                .AsNoTracking()
                .Where(t => t.experiment_label == originalDataSet)
                .Select(t => new TMValues
                {
                    ID = t.ID,
                    IndexTM = t.IndexTM,
                    IzmerValue = t.IzmerValue,
                    OcenValue = t.OcenValue,
                    Id1 = t.Id1,
                    NameTM = t.NameTM,
                    Lagranj = t.Lagranj

                })
                .ToListAsync(cancellationToken);
        }

        if (!filteredTMValues.Any())
        {
            setStatusBarVisible(false); // Скрываем статусбар, если нет данных
            return;
        }

        var uniquePairsTM = filteredTMValues
            .Select(s => new { s.IndexTM, s.Id1, s.NameTM })
            .Distinct()
            .OrderBy(x => x.IndexTM)
            .ThenBy(x => x.Id1)
            .ToList();

        int totalIterations = uniquePairsTM.Count;
        int processedCount = 0;
        var newRecords = new List<NedostovernayaTM>();

        foreach (var uniquePairTM in uniquePairsTM)
        {
            var tmValuesForPair = filteredTMValues
                .Where(t => t.IndexTM == uniquePairTM.IndexTM && t.Id1 == uniquePairTM.Id1)
                .OrderBy(e => e.NumberOfSrez)
                .ToList();

            var allIzmerTM = tmValuesForPair.Select(s => s.IzmerValue).ToList();
            var allOcenTM = tmValuesForPair.Select(s => s.OcenValue).ToList();
            var lagranjValues = tmValuesForPair.Select(s => s.Lagranj).ToList();

            if (!allIzmerTM.Any() || !allOcenTM.Any()) continue;

            double avgMeasured = allIzmerTM.Average();
            double avgEstimated = allOcenTM.Average();

            double covariance = 0;
            double varianceMeasured = 0;
            double varianceEstimated = 0;

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

            string status = DetermineStatus(correlation);

            double maxPositiveLagrange = lagranjValues.Where(x => x > 0).DefaultIfEmpty(0).Max();
            double maxNegativeLagrange = lagranjValues.Where(x => x < 0).DefaultIfEmpty(0).Min();
            double avgLagrange = lagranjValues.Any() ? lagranjValues.Average() : 0;
            double maxAbsoluteLagrange = Math.Abs(maxPositiveLagrange) > Math.Abs(maxNegativeLagrange)
                ? maxPositiveLagrange
                : maxNegativeLagrange;

            newRecords.Add(new NedostovernayaTM
            {
                IndexTm = uniquePairTM.IndexTM,
                NameTM = uniquePairTM.NameTM,
                Id1 = uniquePairTM.Id1,
                CorrTm = correlation,
                Status = status,
                MaxLagranj = maxAbsoluteLagrange,
                AvgLagranj = avgLagrange,
                experiment_label = "Входные данные"
            });

            processedCount++;
            int progress = (int)((double)processedCount / totalIterations * 100);

            if (processedCount % 5 == 0 || processedCount == totalIterations) // Увеличена частота обновления
            {
                await Task.Run(() => progressCallback(progress));
            }
        }

        try
        {
            await _correlation_Context.tm.AddRangeAsync(newRecords);
            await _correlation_Context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"Ошибка при сохранении данных: {ex.Message}");
        }

        setStatusBarVisible(false); // Скрываем статусбар после завершения
    }

    public string DetermineStatus(double correlation)
    {
        if (correlation >= -1 && correlation < -0.5) return "Недостоверная";
        if (correlation >= -0.5 && correlation < 0.5) return "Сомнительная";
        if (correlation >= 0.5 && correlation <= 1) return "Достоверная";
        return "Неопределено";
    }
}




