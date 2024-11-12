using Microsoft.EntityFrameworkCore;
using System.Linq;
using перенос_бд_на_Web.Models;

namespace перенос_бд_на_Web.Services
{
    public class TelemetryMonitoringService
    {
        private readonly ApplicationContext _context;

        public TelemetryMonitoringService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<string> GetNextExperimentLabelAsync()
        {
            var lastExperiment = await _context.TMValues
                .OrderByDescending(tm => tm.experiment_label)
                .Select(tm => tm.experiment_label)
                .FirstOrDefaultAsync();

            if (lastExperiment != null && lastExperiment.StartsWith("Эксперимент"))
            {
                var experimentNumber = int.Parse(lastExperiment.Split(' ')[1]);
                return $"Эксперимент {experimentNumber + 1}";
            }

            return "Эксперимент 1";
        }


        public async Task<List<TMValues>> MonitorAllTMAsync()
        {
            var result = await _context.TMValues
       .GroupBy(tm => new { tm.IndexTM, tm.Id1 })
       .Select(g => g.First())
       .ToListAsync();

            // Здесь можно проверить содержимое groupedResult в режиме отладки

            return result;
        }

        public async Task<List<TMValues>> MonitorUnreliableAndQuestionableTMAsync()
        {
            // Получаем список всех IndexTm, соответствующих статусам "Недостоверная" или "Сомнительная"
            var result = await (from tmValues in _context.TMValues
                                join tm in _context.tm on tmValues.IndexTM equals tm.IndexTm
                                where tm.Status == "Недостоверная" || tm.Status == "Сомнительная"
                                select tmValues)
                        .GroupBy(tm => new { tm.IndexTM, tm.Id1 })
                        .Select(g => g.First())
                        .ToListAsync();

            return result;

        }

        public async Task<List<TMValues>> MonitorUnreliableTMAsync()
        {
            // Выполняем JOIN между таблицами TMValues и tm для получения только "Недостоверных" записей
            var result = await (from tmValue in _context.TMValues
                                join tm in _context.tm on tmValue.IndexTM equals tm.IndexTm
                                where tm.Status == "Недостоверная"
                                select tmValue)
                        .GroupBy(tm => new { tm.IndexTM, tm.Id1 })
                        .Select(g => g.First())
                        .ToListAsync();

            return result;
        }

        public async Task<List<TMValues>> MonitorVerifiedTMAsync(List<int> telemetryIds)
        {
            // Запрашиваем записи из базы данных для выбранных идентификаторов
            var result = await _context.TMValues
                                       .Where(tm => telemetryIds.Contains((int)tm.IndexTM))
                                       .Distinct()
                                       .ToListAsync();

            return result;
        }

        public async Task<List<TMValues>> ManualMonitorSetupAsync(List<int> tmNumbers)
        {
            var tmNumbersAsDouble = tmNumbers.Select(n => (double)n).ToList();
            return await _context.TMValues
                .Where(tm => tmNumbersAsDouble.Contains(tm.IndexTM))
                .GroupBy(tm => new { tm.IndexTM, tm.Id1 })
                .Select(g => g.First())
                .ToListAsync();
        }

        public async Task<bool> CheckIfTMExistsAsync(int indexTM)
        {
            // Предполагаем, что `TMValues` — это таблица в вашем `ApplicationContext`, содержащая номера телеметрии
            return await _context.TMValues.AnyAsync(tm => tm.IndexTM == indexTM);
        }

    }
}
