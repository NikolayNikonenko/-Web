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
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

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

        private bool AnalyzeTMInFile(KeyValuePair<string, string> tmEntry, IRastr rastr, ref int telemetryCount, string filePath)
        {
            rastr.Load(RG_KOD.RG_REPL, filePath, "");
            // Обращение к таблице ТИ:каналы
            ITable tableTIChannel = (ITable)rastr.Tables.Item("ti");

            // Обращение к колонке номера ТМ
            ICol numberTMColumn = (ICol)tableTIChannel.Cols.Item("Num");

            // Обращение к колонке названия телеметрии
            ICol nameTMColumn = (ICol)tableTIChannel.Cols.Item("name");

            // Обращение к колонке статуса ТМ
            ICol status = (ICol)tableTIChannel.Cols.Item("sta");

            bool found = false;

            bool successfulState = false;
            // Ищем ТМ по комбинации "Номер ТМ" и "Наименование ТМ"
            tableTIChannel.SetSel($"Num={tmEntry.Key} AND name='{tmEntry.Value}'");

            // Начинаем поиск
            int n = tableTIChannel.FindNextSel[-1];
            while (n != -1)
            {
                found = true;

                telemetryCount++; 

                // Отключение ТМ
                status.set_ZN(n, true);
                Console.WriteLine($"Найдена строка ТМ: Номер ТМ = {tmEntry.Key}, Наименование ТМ = {tmEntry.Value}, Индекс строки = {n}");


                // Выполняем ОС и получаем результат
                var result = rastr.opf("s");

                if (result == 0) // Проверяем результат ОС
                {
                    successfulState = true;
                    Console.WriteLine($"ОС успешная для ТМ: Номер ТМ = {tmEntry.Key}");
                }
                else
                {
                    Console.WriteLine($"ОС неуспешная для ТМ: Номер ТМ = {tmEntry.Key}");
                }

                // Включаем ТМ обратно
                status.set_ZN(n, false);
                
                // Ищем следующую строку с заданными параметрами
                n = tableTIChannel.FindNextSel[n];
            }
            // Если не найдено, выводим сообщение
            if (!found)
            {
                Console.WriteLine($"ТМ не найдена: Номер ТМ = {tmEntry.Key}, Наименование ТМ = {tmEntry.Value}");
            }
            return successfulState;
        }

        public Dictionary<string, string> LoadDataFromExcel(string filePath)
        {
            var dataDictionary = new Dictionary<string, string>();

            Application excelApp = new Application();
            Workbook workBook = null;
            Worksheet workSheet = null;

            try
            {
                // Открываем Excel файл
                workBook = excelApp.Workbooks.Open(filePath);
                workSheet = workBook.Sheets[1]; // Работаем с первым листом

                // Указываем полный путь к Range из Excel
                Microsoft.Office.Interop.Excel.Range usedRange = workSheet.UsedRange;

                // Перебираем строки в диапазоне, начиная со второй (первая строка — заголовки)
                for (int row = 2; row <= usedRange.Rows.Count; row++)
                {
                    string key = usedRange.Cells[row, 1]?.Value2?.ToString(); // Столбец A
                    string value = usedRange.Cells[row, 2]?.Value2?.ToString(); // Столбец B

                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                    {
                        dataDictionary[key] = value;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении Excel файла: {ex.Message}");
            }
            finally
            {
                // Освобождение ресурсов
                if (workSheet != null) Marshal.ReleaseComObject(workSheet);
                if (workBook != null)
                {
                    workBook.Close(false);
                    Marshal.ReleaseComObject(workBook);
                }
                if (excelApp != null)
                {
                    excelApp.Quit();
                    Marshal.ReleaseComObject(excelApp);
                }
            }

            return dataDictionary;
        }


        public async Task<(int successfulCount, int totalCount)> AnalyzeNewReliabilityData(
           DateTime startDateTime,
           DateTime endDateTime,
           List<string> filePaths,
           Action<int> progressCallback,
           CancellationToken cancellationToken)
        {

            int totalCount = 0;
            int successfulCount = 0;
            int processedCount = 0;


            // Путь к Excel документу
            string excelFilePath = @"C:\Users\User\Desktop\учеба\магистратура\5 семак\диплом по ИТ\300 ТМ для отключения\300 ТМ для ЕОТМ.xlsx";

            var tmDictionary = LoadDataFromExcel(excelFilePath);

            // Общее количество операций (файлы * записи в tmDictionary)
            totalCount = filePaths.Count * tmDictionary.Count;
            // Создаем объект Rastr
            IRastr rastr = new Rastr();
            try
            {
                foreach (var filePath in filePaths)
                {

                    cancellationToken.ThrowIfCancellationRequested();
                    foreach (var kvp in tmDictionary)
                    {
                        Console.WriteLine($"Номер ТМ: {kvp.Key}, Наименование ТМ: {kvp.Value}");
                        // Анализируем ТМ в файле
                        if (AnalyzeTMInFile(kvp, rastr, ref totalCount, filePath))
                        {
                            successfulCount++; // Увеличиваем счетчик успешных ОС
                        }
                        processedCount++;
                        int progress = (processedCount * 100) / totalCount;
                        progressCallback?.Invoke(progress);
                    }

                }

            }
            finally
            {
                // Освобождаем объект Rastr
                rastr = null;
            }


            return (successfulCount, totalCount);
        }
    }
}