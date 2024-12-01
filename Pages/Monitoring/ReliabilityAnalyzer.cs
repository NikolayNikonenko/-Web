using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using перенос_бд_на_Web.Models;
using Microsoft.EntityFrameworkCore;
using ASTRALib;

namespace перенос_бд_на_Web
{
    public class SlicesService
    {
        private readonly ApplicationContext _context;

        public SlicesService(ApplicationContext context)
        {
            _context = context;
        }

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

    public class ReliabilityAnalyzer
    {
        private readonly SlicesService _slicesService;
        private readonly ILogger<ReliabilityAnalyzer> _logger;

        public ReliabilityAnalyzer(SlicesService slicesService, ILogger<ReliabilityAnalyzer> logger)
        {
            _slicesService = slicesService;
            _logger = logger;
        }

        public async Task<(int successfulCount, int totalCount)> AnalyzeReliabilityData(
            DateTime startDateTime,
            DateTime endDateTime,
            List<string> filePaths,
            Action<int> progressCallback,
            CancellationToken cancellationToken)
        {
            int totalCount = filePaths.Count;
            int successfulCount = 0;
            int processedCount = 0;
            object lockObj = new object();
            // Создаем объект Rastr до начала цикла
            IRastr rastr = new Rastr();

            await Task.Run(() =>
            {
                Parallel.ForEach(filePaths, new ParallelOptions { CancellationToken = cancellationToken }, filePath =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        _logger.LogInformation($"Processing file: {filePath}");

                        bool isSuccessful = AnalyzeFile(filePath, rastr);

                        lock (lockObj)
                        {
                            if (isSuccessful)
                            {
                                successfulCount++;
                            }
                            processedCount++;
                        }

                        int progress = (int)((double)processedCount / totalCount * 100);
                        progressCallback(progress);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Ошибка при обработке файла {filePath}: {ex.Message}");
                        _logger.LogError($"StackTrace: {ex.StackTrace}");
                    }
                });
            });
            // Освобождаем ресурсы после завершения работы
            rastr = null;

            return (successfulCount, totalCount);
        }

        public bool AnalyzeFile(string filePath, IRastr rastr)
        {
            try
            {
                //IRastr Rastr = new Rastr();
                rastr.Load(RG_KOD.RG_REPL, filePath, "");
                // Задержка для имитации реального времени загрузки
                Thread.Sleep(100);

                var oc1 = rastr.opf("s");
                return (int)oc1 == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при анализе файла {filePath}: {ex.Message}");
                _logger.LogError($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }
    }
}