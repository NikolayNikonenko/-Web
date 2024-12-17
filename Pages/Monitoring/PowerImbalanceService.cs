using Microsoft.EntityFrameworkCore;
using перенос_бд_на_Web.Models;
using System.Threading.Tasks;
using System.Linq;

namespace перенос_бд_на_Web.Services
{
    public class PowerImbalanceService
    {
        //private readonly ApplicationContext _context;

        private readonly IDbContextFactory<ApplicationContext> _contextFactory;

        public PowerImbalanceService(IDbContextFactory<ApplicationContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public class PowerImbalanceMetrics
        {
            public double MaxActivePowerImbalance { get; set; }
            public double MaxReactivePowerImbalance { get; set; }
            public double AverageTotalActivePowerImbalance { get; set; }
            public double AverageTotalReactivePowerImbalance { get; set; }
        }

        // Метод для расчета метрик одного набора
        public async Task<PowerImbalanceMetrics> CalculateMetricsAsync(IEnumerable<string> slicePaths)
        {
            await using var _context = _contextFactory.CreateDbContext();
            // Получаем все SliceID для переданных путей
            var sliceIds = await _context.slices
                .Where(s => slicePaths.Contains(s.SlicePath))
                .Select(s => s.SliceID) // Извлекаем только SliceID
                .ToListAsync();

            // Фильтрация данных активной мощности
            var activePowerData = await _context.active_power_imbalance
                .Where(api => sliceIds.Contains(api.SliceID_p)) // Сравнение с SliceID_p
                .ToListAsync();

            // Фильтрация данных реактивной мощности
            var reactivePowerData = await _context.reactive_power_imbalance
                .Where(rpi => sliceIds.Contains(rpi.SliceID_q)) // Сравнение с SliceID_q
                .ToListAsync();

            // Максимальное отклонение активной мощности
            var maxActivePowerImbalance = activePowerData
                .OrderByDescending(api => Math.Abs(api.p_neb_p))
                .Select(api => Math.Abs(api.p_neb_p))
                .FirstOrDefault();

            // Максимальное отклонение реактивной мощности
            var maxReactivePowerImbalance = reactivePowerData
                .OrderByDescending(rpi => Math.Abs(rpi.q_neb_q))
                .Select(rpi => Math.Abs(rpi.q_neb_q))
                .FirstOrDefault();

            // Среднее отклонение активной мощности
            var activeSliceCount = activePowerData
                .Select(api => api.SliceID_p)
                .Distinct()
                .Count();

            var totalActiveImbalance = activePowerData
                .GroupBy(api => api.SliceID_p)
                .Select(g => g.Sum(api => api.p_neb_p))
                .Sum();

            double averageActiveImbalance = activeSliceCount > 0
                ? totalActiveImbalance / activeSliceCount
                : 0;

            // Среднее отклонение реактивной мощности
            var reactiveSliceCount = reactivePowerData
                .Select(rpi => rpi.SliceID_q)
                .Distinct()
                .Count();

            var totalReactiveImbalance = reactivePowerData
                .GroupBy(rpi => rpi.SliceID_q)
                .Select(g => g.Sum(rpi => rpi.q_neb_q))
                .Sum();

            double averageReactiveImbalance = reactiveSliceCount > 0
                ? totalReactiveImbalance / reactiveSliceCount
                : 0;

            // Возврат метрик
            return new PowerImbalanceMetrics
            {
                MaxActivePowerImbalance = maxActivePowerImbalance,
                MaxReactivePowerImbalance = maxReactivePowerImbalance,
                AverageTotalActivePowerImbalance = averageActiveImbalance,
                AverageTotalReactivePowerImbalance = averageReactiveImbalance
            };
        }
    }
}