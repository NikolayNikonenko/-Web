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

    public class ReliabilityAnalyzer
    {
       
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

            // Размер пакета для обработки
            int batchSize = 10;

            var fileBatches = filePaths
                .Select((file, index) => new { file, index })
                .GroupBy(x => x.index / batchSize)
                .Select(group => group.Select(x => x.file).ToList())
                .ToList();

            foreach (var batch in fileBatches)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Создаем объект Rastr для текущего пакета
                IRastr rastr = new Rastr();

                foreach (var filePath in batch)
                {
                    try
                    {
                        bool isSuccessful = AnalyzeFile(filePath, rastr);

                        if (isSuccessful)
                        {
                            successfulCount++;
                        }
                        processedCount++;

                        int progress = (int)((double)processedCount / totalCount * 100);
                        progressCallback(progress);
                    }
                    catch (Exception ex)
                    {
                    }
                }

                // Освобождаем объект Rastr после обработки пакета
                rastr = null;
            }

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
                return false;
            }
        }
    }
}