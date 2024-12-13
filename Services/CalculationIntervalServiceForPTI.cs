using перенос_бд_на_Web.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace перенос_бд_на_Web.Services
{
    public class CalculationIntervalServiceForPTI
    {
        private readonly ApplicationContext _dbContext;

        public CalculationIntervalServiceForPTI(ApplicationContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Сохранение интервала
        public async Task SaveCalculationInterval(DateTime startDate, DateTime endDate)
        {
            Console.WriteLine($"Вызываю процедуру с параметрами: {startDate} - {endDate}");

            await _dbContext.Database.ExecuteSqlRawAsync(
                "CALL AddOrUpdateCalculationInterval(@startDate, @endDate)",
                new NpgsqlParameter("startDate", startDate),
                new NpgsqlParameter("endDate", endDate)
            );
        }

        // Получение последнего интервала
        public async Task<CalculationIntervalForPTI> GetLastCalculationInterval()
        {
            return await _dbContext.calculationIntervalForPTIs
                .FromSqlRaw("SELECT \"startDate\" AS \"startDate\", \"endDate\" AS \"endDate\" FROM CalculationIntervals ORDER BY \"createDat\" DESC LIMIT 1")
                .FirstOrDefaultAsync();
        }
    }
}
