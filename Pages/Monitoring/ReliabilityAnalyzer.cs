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
                Console.WriteLine($"Загружен файл {filePath}, Результат ОС - {Convert.ToString(oc1)}");
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

        public Dictionary<string, string> LoadDataFromCsv(string filePath)
        {
            var dataDictionary = new Dictionary<string, string>();

            try
            {
                // Считываем строки из файла CSV
                var lines = File.ReadAllLines(filePath);

                // Перебираем строки, начиная со второй (первая строка — заголовки)
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(';'); // Предполагается, что разделитель — запятая

                    if (values.Length >= 2)
                    {
                        string key = values[0]?.Trim();  // Первый столбец
                        string value = values[1]?.Trim(); // Второй столбец

                        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                        {
                            dataDictionary[key] = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении CSV файла: {ex.Message}");
            }

            return dataDictionary;
        }

        public string GetParameterValue(string parameterName, ApplicationContext context)
        {
            var parameter = context.configuration_parameters
            .FirstOrDefault(p => p.parameter_name == parameterName);

            if (parameter == null)
            {
                throw new Exception($"Параметр с именем {parameterName} не найден в таблице configuration_parameters");
            }
            return parameter.parameter_value;
        }


        public async Task<(int successfulCount, int totalCount)> AnalyzeNewReliabilityData(
           ApplicationContext context,
           DateTime startDateTime,
           DateTime endDateTime,
           List<string> filePaths,
           Action<int> progressCallback,
           CancellationToken cancellationToken)
        {

            int totalCount = 0;
            int successfulCount = 0;
            int processedCount = 0;

            // Путь к документу
            string csvFilePath = GetParameterValue("EOTMDataPath", context);

            var tmDictionary = LoadDataFromCsv(csvFilePath);

            if (tmDictionary.Count == 0 || filePaths.Count == 0)
            {
                Console.WriteLine("Нет данных для обработки.");
                return (successfulCount, totalCount);
            }

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
                        int progress = (int)(double)processedCount*100*2 / totalCount;
                        progressCallback(progress);
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