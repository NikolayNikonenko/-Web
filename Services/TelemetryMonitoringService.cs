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
            return await _context.TMValues.ToListAsync();
        }

        public async Task<List<TMValues>> MonitorUnreliableAndQuestionableTMAsync()
        {
            // Получаем список всех IndexTm, соответствующих статусам "Достоверная" или "Сомнительная"
            var result = await (from tmValues in _context.TMValues
                                join tm in _context.tm on tmValues.IndexTM equals tm.IndexTm
                                where tm.Status == "Достоверная" || tm.Status == "Сомнительная"
                                select tmValues).ToListAsync();

            return result;

        }

        public async Task<List<TMValues>> MonitorUnreliableTMAsync()
        {
            // Выполняем JOIN между таблицами TMValues и tm для получения только "Недостоверных" записей
            var result = await (from tmValue in _context.TMValues
                                join tm in _context.tm on tmValue.IndexTM equals tm.IndexTm
                                where tm.Status == "Недостоверная"
                                select tmValue)
                               .ToListAsync();

            return result;
        }

        public async Task<List<TMValues>> MonitorVerifiedTMAsync()
        {
            // Выполняем JOIN между таблицами TMValues и tm для получения только "Достоверных" записей
            var result = await (from tmValue in _context.TMValues
                                join tm in _context.tm on tmValue.IndexTM equals tm.IndexTm
                                where tm.Status == "Достоверная"
                                select tmValue)
                               .ToListAsync();

            return result;
        }

        public async Task<List<TMValues>> ManualMonitorSetupAsync(List<int> tmNumbers)
        {
            var tmNumbersAsDouble = tmNumbers.Select(n => (double)n).ToList();
            return await _context.TMValues
                .Where(tm => tmNumbersAsDouble.Contains(tm.IndexTM))
                .ToListAsync();
        }

        public async Task<bool> CheckIfTMExistsAsync(int indexTM)
        {
            // Предполагаем, что `TMValues` — это таблица в вашем `ApplicationContext`, содержащая номера телеметрии
            return await _context.TMValues.AnyAsync(tm => tm.IndexTM == indexTM);
        }

    }
}
