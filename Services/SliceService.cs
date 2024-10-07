using перенос_бд_на_Web.Models;
using Microsoft.EntityFrameworkCore;

namespace перенос_бд_на_Web.Services
{
    public class SliceService : ISliceService
    {
        private readonly ApplicationContext _context;

        public SliceService(ApplicationContext context)
        {
            _context = context;
        }

        // Метод для получения путей файлов в указанном временном интервале
        public async Task<List<string>> GetFilePathsInRangeAsync(DateTime startDateTime, DateTime endDateTime)
        {
            string startDateStr = startDateTime.ToString("yyyy_MM_dd");
            string endDateStr = endDateTime.ToString("yyyy_MM_dd");
            string startTimeStr = startDateTime.ToString("HH_mm_ss");
            string endTimeStr = endDateTime.ToString("HH_mm_ss");

            var allSlices = await _context.slices.ToListAsync();

            return allSlices
                .Where(s =>
                {
                    var pathParts = s.SlicePath.Split(Path.DirectorySeparatorChar);
                    if (pathParts.Length < 2) return false;

                    var datePart = pathParts[^3];
                    var timePart = pathParts[^2];

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
