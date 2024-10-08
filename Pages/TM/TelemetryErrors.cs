using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using перенос_бд_на_Web.Models;
using Microsoft.EntityFrameworkCore;
using перенос_бд_на_Web.Pages.TM;
namespace перенос_бд_на_Web.Pages.TM
{

    public class SomeDataTM : PageModel
    {
        private readonly ApplicationContext _telemetry_Context;

        public List<NedostovernayaTM> Tm { get; set; }
        //public SomeDataTM(ApplicationContext db)
        //{
            //_telemetry_Context = db;
        //}
        public void OnGet()
        {
            Tm = _telemetry_Context.tm.AsNoTracking().ToList();
        }
    }
    
    public class CorrData 
        {
        
        private readonly ApplicationContext _correlation_Context;
        public List<TMValues> TMValues { get; set; }

        public CorrData(ApplicationContext db)
        {
            _correlation_Context = db;
        }


        public async Task CalculationCorrelation(List<TMValues> filteredTMValues)
        {
            var a = filteredTMValues;
            // Извлекаем все уникальные индексы телеметрии
            var uniqueOrdersTM = filteredTMValues
                    .Select(s => s.IndexTM)
                    .Distinct()
                    .OrderBy(indexTM => indexTM)
                    .ToList();

            var answer = new Dictionary<int, double>();

            foreach (var uniqueOrderTM in uniqueOrdersTM)
            {
                // Получаем все измеренные и оценочные значения для данного индекса телеметрии
                var allIzmerTM = filteredTMValues
                    .Where(t => t.IndexTM == uniqueOrderTM)
                    .OrderBy(e => e.NumberOfSrez)
                    .Select(s => s.IzmerValue)
                    .ToList();

                var allOcenTM = filteredTMValues
                    .Where(t => t.IndexTM == uniqueOrderTM)
                    .OrderBy(e => e.NumberOfSrez)
                    .Select(s => s.OcenValue)
                    .ToList();

                // Получаем данные по Lagranj
                var lagranjValues = filteredTMValues
                    .Where(t => t.IndexTM == uniqueOrderTM)
                    .Select(s => s.Lagranj) // Убедитесь, что у вас есть свойство Lagranj в классе TMValues
                    .ToList();

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
                double correlation = covariance / denominator;
                answer[(int)uniqueOrderTM] = correlation;

                // Определяем статус на основе корреляции
                string status = DetermineStatus(correlation);

                // Рассчитываем максимальные и средние значения Лагранжа
                double maxPositiveLagrange = lagranjValues.Where(x => x > 0).DefaultIfEmpty(0).Max();
                double maxNegativeLagrange = lagranjValues.Where(x => x < 0).DefaultIfEmpty(0).Min();
                double avgLagrange = lagranjValues.Any() ? lagranjValues.Average() : 0; // Проверка на пустую коллекцию
                double maxAbsoluteLagrange = Math.Abs(maxPositiveLagrange) > Math.Abs(maxNegativeLagrange) ? maxPositiveLagrange : maxNegativeLagrange;


                // Извлекаем значение NameTM для текущего уникального индекса TM
                var nameTM = filteredTMValues
                    .Where(t => t.IndexTM == uniqueOrderTM)
                    .Select(t => t.NameTM)
                    .FirstOrDefault();

                // Запись статуса в базу данных для телеметрии с данным индексом
                var correlation_context = await _correlation_Context.tm
                    .Where(z => z.IndexTm == uniqueOrderTM)
                    .ToListAsync();

                var newRecord = new NedostovernayaTM
                {
                    IndexTm = uniqueOrderTM,
                    CorrTm = correlation,
                    Status = status,
                    NameTM = nameTM,
                    MaxLagranj = maxAbsoluteLagrange, // Записываем максимальный Лагранж
                    AvgLagranj = avgLagrange // Записываем средний Лагранж

                };

                _correlation_Context.tm.Add(newRecord);
                try
                {
                    // Сохраняем изменения в базе данных
                    await _correlation_Context.SaveChangesAsync();
                    Console.WriteLine($"Телеметрия под номером {uniqueOrderTM} успешно сохранена в БД");
                }
                catch (DbUpdateException ex) 
                {
                    Console.WriteLine($"Ошибка при сохранении данных: { ex.Message}");
                }
                
            }
            Console.WriteLine("Усе");
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




