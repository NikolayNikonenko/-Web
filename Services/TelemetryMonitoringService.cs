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
            try
            {
                Console.WriteLine("Получаем последний эксперимент...");

                // Загружаем все метки "Эксперимент" в память
                var allExperiments = await _context.TMValues
                    .Where(tm => tm.experiment_label.StartsWith("Эксперимент"))
                    .Select(tm => tm.experiment_label)
                    .ToListAsync();

                // Извлекаем числовую часть, сортируем и находим последний номер
                var lastExperimentNumber = allExperiments
                    .Select(label => int.Parse(label.Split(' ')[1])) // Извлекаем число из метки
                    .OrderByDescending(num => num) // Сортируем по убыванию
                    .FirstOrDefault();

                Console.WriteLine($"Последний эксперимент: {lastExperimentNumber}");

                // Если данных нет, начинаем с "Эксперимент 1"
                if (lastExperimentNumber == 0)
                {
                    Console.WriteLine("Данных о последнем эксперименте нет.");
                    return "Эксперимент 1";
                }

                // Возвращаем следующий номер эксперимента
                return $"Эксперимент {lastExperimentNumber + 1}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в GetNextExperimentLabelAsync: {ex.Message}");
                throw;
            }
        }

        // метод для выборки набора содежащего все уникальные элементы из filteredTMValues
        public async Task<List<TMValues>> MonitorAllTMAsync(List<TMValues> filteredTMValues)
        {
            var result = filteredTMValues
      .GroupBy(tm => new { tm.IndexTM, tm.Id1, tm.Privyazka })
      .Select(g => g.FirstOrDefault())
      .ToList();

            return result;
        }
        // метод для выборки набора содежащего все уникальные элементы из filteredTMValues, статус которых либо "Недостоверная" либо "Сомнительная"
        public async Task<List<TMValues>> MonitorUnreliableAndQuestionableTMAsync(List<TMValues> filteredTMValues)
        {
            var result = (from tmValue in filteredTMValues
                          join tm in _context.tm
                          on new { IndexTM = (int)tmValue.IndexTM, tmValue.Id1 }
                          equals new { IndexTM = (int)tm.IndexTm, tm.Id1 }
                          where tm.Status == "Недостоверная" || tm.Status == "Сомнительная"
                          group tmValue by new { tmValue.IndexTM, tmValue.Id1, tmValue.Privyazka } into grouped
                          select grouped.FirstOrDefault())
                          .ToList();

            return result;
        }
        // метод для выборки набора содежащего все уникальные элементы из filteredTMValues, "Недостоверная"
        public async Task<List<TMValues>> MonitorUnreliableTMAsync(List<TMValues> filteredTMValues)
        {
            var result = (from tmValue in filteredTMValues
                          join unreliable in _context.tm
                          on new { IndexTM = (int)tmValue.IndexTM, tmValue.Id1 } equals new { IndexTM = (int)unreliable.IndexTm, unreliable.Id1 }
                          where unreliable.Status == "Недостоверная"
                          group tmValue by new { tmValue.IndexTM, tmValue.Id1, tmValue.Privyazka } into grouped
                          select grouped.FirstOrDefault())
                          .ToList();

            return result;
        }
        // метод для выборки набора содежащего все уникальные элементы для которых проводилась достоверизация
        public async Task<List<TMValues>> MonitorVerifiedTMAsync(List<(int TelemetryId, int Id1)> telemetryKeys, List<TMValues> filteredTMValues)
        {
            var result = filteredTMValues
        .Where(tmValue => telemetryKeys.Any(key =>
            (int)tmValue.IndexTM == key.TelemetryId && tmValue.Id1 == key.Id1))
        .GroupBy(tmValue => new { tmValue.IndexTM, tmValue.Id1, tmValue.Privyazka })
        .Select(group => group.FirstOrDefault()) // Берем первый уникальный элемент из группы
        .ToList();

            return result;
        }
        // метод для выборки набора содежащего все уникальные элементы номера ТМ которых были введены пользователем вручную
        public async Task<List<TMValues>> ManualMonitorSetupAsync(List<int> tmNumbers, List<TMValues> filteredTMValues)
        {
            // Преобразуем числа tmNumbers в тип double
            var tmNumbersAsDouble = tmNumbers.Select(n => (double)n).ToList();

            // Фильтруем список filteredTMValues
            var result = filteredTMValues
                .Where(tm => tmNumbersAsDouble.Contains(tm.IndexTM))
                .GroupBy(tm => new { tm.IndexTM, tm.Id1, tm.Privyazka })
                .Select(g => g.First())
                .ToList();

            // Возвращаем результат
            return result;
        }
        public async Task<List<TMValues>> CombineMonitoringSetsAsync(
        List<TMValues> selectedTMValues, List<TMValues> manualTMValues)
        {
            var combined = selectedTMValues
                .Concat(manualTMValues) // Объединяем два списка
                .GroupBy(tm => new { tm.IndexTM, tm.Id1, tm.Privyazka }) // Группируем по уникальным ключам
                .Select(g => g.First()) // Берем первый элемент каждой группы
                .ToList();

            return await Task.FromResult(combined); // Возвращаем объединенный список
        }




        public async Task<bool> CheckIfTMExistsAsync(int indexTM)
        {
            // Предполагаем, что `TMValues` — это таблица в вашем `ApplicationContext`, содержащая номера телеметрии
            return await _context.TMValues.AnyAsync(tm => tm.IndexTM == indexTM);
        }



    }
}
