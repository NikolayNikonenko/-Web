using перенос_бд_на_Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace перенос_бд_на_Web.Services
{
    public class SliceService : ISliceService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public SliceService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        // Метод для получения путей файлов в указанном временном интервале
        public async Task<List<string>> GetFilePathsInRangeAsync(DateTime startDateTime, DateTime endDateTime, string experimentalKit)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var scopedContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

                string startDateStr = startDateTime.ToString("yyyy_MM_dd");
                string endDateStr = endDateTime.ToString("yyyy_MM_dd");
                string startTimeStr = startDateTime.ToString("HH_mm_ss");
                string endTimeStr = endDateTime.ToString("HH_mm_ss");

                // Загружаем только срезы с нужной меткой
                var filteredSlices = await scopedContext.slices
                    .Where(s => s.experiment_label == experimentalKit)
                    .ToListAsync();

                return filteredSlices
                    .Where(s =>
                    {
                        var pathParts = s.SlicePath.Split(Path.DirectorySeparatorChar);

                        // Проверяем минимальную длину пути для обоих случаев
                        if (pathParts.Length < 5) return false;

                        // Извлекаем части пути для проверки
                        var datePart = experimentalKit == "Входные данные" ? pathParts[^3] : pathParts[^3]; // Дата
                        var timePart = experimentalKit == "Входные данные" ? pathParts[^2] : pathParts[^2]; // Время

                        if (!DateTime.TryParseExact(datePart, "yyyy_MM_dd", null, System.Globalization.DateTimeStyles.None, out DateTime fileDate))
                            return false;

                        if (!DateTime.TryParseExact(timePart, "HH_mm_ss", null, System.Globalization.DateTimeStyles.None, out DateTime fileTime))
                            return false;

                        var fileDateTime = fileDate.Add(fileTime.TimeOfDay);
                        return fileDateTime >= startDateTime && fileDateTime <= endDateTime;
                    })
                    .Select(s => s.SlicePath)
                    .OrderBy(p => p)
                    .ToList();
            }
        }
    }
}
