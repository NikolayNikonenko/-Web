using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using перенос_бд_на_Web.Services;
namespace перенос_бд_на_Web.Services
{
    public class ValidationService
    {
        private readonly TelemetryMonitoringService _monitoringService;

        public ValidationService(TelemetryMonitoringService monitoringService)
        {
            _monitoringService = monitoringService;
        }
        public async Task<ValidationResult> ValidateTMInputAsync(string tmInput)
        {
            var result = new ValidationResult();

            // Проверка на допустимые символы
            if (!Regex.IsMatch(tmInput, @"^[-\d, ]+$"))
            {
                result.Errors.Add("Введены недопустимые символы. Вводите только номера ТМ через запятую.");
                return result;
            }

            // Разбиваем ввод на отдельные номера ТМ и удаляем пробелы
            var tmNumbers = tmInput.Split(',')
                                   .Select(s => s.Trim())
                                   .Where(s => !string.IsNullOrEmpty(s))
                                   .ToList();

            var nonExistentTM = new List<string>();

            // Проверяем существование номеров в базе данных
            foreach (var tmNumber in tmNumbers)
            {
                if (int.TryParse(tmNumber, out int parsedNumber))
                {
                    var exists = await _monitoringService.CheckIfTMExistsAsync(parsedNumber);
                    if (!exists)
                    {
                        nonExistentTM.Add(tmNumber);
                    }
                }
            }

            // Добавляем сообщение об ошибке, если есть несуществующие номера ТМ
            if (nonExistentTM.Any())
            {
                result.Errors.Add($"Следующие номера ТМ не существуют в базе данных: {string.Join(", ", nonExistentTM)}");
            }
            else
            {
                result.IsValid = true;
                result.TMNumbers = tmNumbers;
            }

            return result;
        }
    }
    public class ValidationResult
    {
        public bool IsValid { get; set; } = false;
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> TMNumbers { get; set; } = new List<string>();
    }


}
