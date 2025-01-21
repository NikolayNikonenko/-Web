using Npgsql;
using Microsoft.EntityFrameworkCore;
using перенос_бд_на_Web.Models;

namespace перенос_бд_на_Web.Services
{
    public class CalculationIntervalServiceForCorrCalc
    {
        private readonly ApplicationContext _dbContext;

        public CalculationIntervalServiceForCorrCalc(ApplicationContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Сохранение интервала
        public async Task SaveCalculationInterval(DateTime startDate, DateTime endDate)
        {
            Console.WriteLine($"Вызываю процедуру с параметрами: {startDate} - {endDate}");

            await _dbContext.Database.ExecuteSqlRawAsync(
                "CALL AddOrUpdateCalculationIntervalForCalcCorr(@startDate, @endDate)",
                new NpgsqlParameter("startDate", startDate),
                new NpgsqlParameter("endDate", endDate)
            );
        }

        // Получение последнего интервала
        public async Task<IntervalForCorrCalc> GetLastCalculationInterval()
        {
            return await _dbContext.interval_for_corr_calc
                .FromSqlRaw("SELECT \"startDate\" AS \"startDate\", \"endDate\" AS \"endDate\" FROM interval_for_corr_calc ORDER BY \"createDat\" DESC LIMIT 1")
                .FirstOrDefaultAsync();
        }
    }
}
