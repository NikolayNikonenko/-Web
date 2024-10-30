using Microsoft.EntityFrameworkCore;
using перенос_бд_на_Web.Models;

namespace перенос_бд_на_Web.Services
{
    public class DataFilterService
    {
        private readonly ApplicationContext _context;
        private readonly SliceService _sliceService;

        public DataFilterService(ApplicationContext context, SliceService sliceService)
        {
            _context = context;
            _sliceService = sliceService;
        }

        // Основной метод для применения фильтра по временному интервалу и прореживания
        public async Task<List<TMValues>> ApplyTimeIntervalFilterAsync(DateTime? startDate, DateTime? endDate, bool isThinning = false, int thinningInterval = 5)
        {
            List<TMValues> filteredData;

            if (startDate.HasValue && endDate.HasValue)
            {
                filteredData = await FilterByTimeIntervalAsync(startDate.Value, endDate.Value, isThinning, thinningInterval);
            }
            else
            {
                filteredData = await RetrieveAllDataAsync(isThinning, thinningInterval);
            }

            return filteredData;
        }

        // Метод для фильтрации данных по временному интервалу
        private async Task<List<TMValues>> FilterByTimeIntervalAsync(DateTime startDate, DateTime endDate, bool isThinning, int thinningInterval)
        {
            var filePaths = await _sliceService.GetFilePathsInRangeAsync(startDate, endDate);

            if (!filePaths.Any())
            {
                Console.WriteLine("Нет найденных путей файлов для заданного временного интервала.");
                return new List<TMValues>();
            }

            var sliceIdsInRange = await _context.slices
                .Where(s => filePaths.Contains(s.SlicePath))
                .Select(s => s.SliceID)
                .ToListAsync();

            if (!sliceIdsInRange.Any())
            {
                Console.WriteLine("Нет идентификаторов срезов для заданных путей файлов.");
                return new List<TMValues>();
            }

            var filteredTMValues = await _context.TMValues
                .Where(t => sliceIdsInRange.Contains(t.SliceID))
                .ToListAsync();

            if (!filteredTMValues.Any())
            {
                Console.WriteLine("filteredTMValues пуст после фильтрации.");
                return new List<TMValues>();
            }

            // Применяем прореживание, если включено
            return isThinning ? ApplyThinning(filteredTMValues, thinningInterval) : filteredTMValues;
        }

        // Метод для получения всех данных с опциональным прореживанием
        private async Task<List<TMValues>> RetrieveAllDataAsync(bool isThinning, int thinningInterval)
        {
            var allData = await _context.TMValues.ToListAsync();
            return isThinning ? ApplyThinning(allData, thinningInterval) : allData;
        }


        // Метод для прореживания данных: выборка каждого n-го элемента (например, каждая 5-я запись)
        private List<TMValues> ApplyThinning(List<TMValues> data, int step)
        {
            return data.Where((item, index) => index % step == 0).ToList();
        }
    }
}
