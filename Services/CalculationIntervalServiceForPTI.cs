using перенос_бд_на_Web.Models;
using Microsoft.EntityFrameworkCore;
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
            await _dbContext.Database.ExecuteSqlRawAsync(
                "CALL AddOrUpdateCalculationInterval({0}, {1})",
                startDate, endDate
            );
        }

        // Получение последнего интервала
        public async Task<CalculationIntervalForPTI> GetLastCalculationInterval()
        {
            return await _dbContext.calculationIntervalForPTIs
                .FromSqlRaw("SELECT * FROM GetLastCalculationInterval()")
                .FirstOrDefaultAsync();
        }
    }
}
