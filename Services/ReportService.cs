using System.IO;
using System.Threading.Tasks;
using Aspose.Words;
using Aspose.Words.Tables;
using System;
using System.Collections.Generic;

namespace перенос_бд_на_Web.Services
{
    public class ReportService
    {
        private class ReportData
        {
            public string FirstSetLabel { get; set; }
            public string SecondSetLabel { get; set; }
            public double FirstMaxActivePowerImbalance { get; set; }
            public double SecondMaxActivePowerImbalance { get; set; }
            public double FirstMaxReactivePowerImbalance { get; set; }
            public double SecondMaxReactivePowerImbalance { get; set; }
            public double FirstAverageTotalActivePowerImbalance { get; set; }
            public double SecondAverageTotalActivePowerImbalance { get; set; }
            public double FirstAverageTotalReactivePowerImbalance { get; set; }
            public double SecondAverageTotalReactivePowerImbalance { get; set; }
            public double FirstAverageDeviation { get; set; }
            public double SecondAverageDeviation { get; set; }
            public double FirstStandardDeviation { get; set; }
            public double SecondStandardDeviation { get; set; }
            public double FirstSuccessRate { get; set; }
            public double SecondSuccessRate { get; set; }
            public DateTime? StartDateTime { get; set; }
            public DateTime? EndDateTime { get; set; }
        }

        private readonly List<ReportData> _reportDataList = new List<ReportData>();

        public async Task AddToReport(string firstSetLabel, string secondSetLabel,
                               double firstMaxActivePowerImbalance, double secondMaxActivePowerImbalance,
                               double firstMaxReactivePowerImbalance, double secondMaxReactivePowerImbalance,
                               double firstAverageTotalActivePowerImbalance, double secondAverageTotalActivePowerImbalance,
                               double firstAverageTotalReactivePowerImbalance, double secondAverageTotalReactivePowerImbalance,
                               double firstAverageDeviation, double secondAverageDeviation,
                               double firstStandardDeviation, double secondStandardDeviation,
                               double firstSuccessRate, double secondSuccessRate,
                               DateTime? startDateTime, DateTime? endDateTime)
        {
            var data = new ReportData
            {
                FirstSetLabel = firstSetLabel,
                SecondSetLabel = secondSetLabel,
                FirstMaxActivePowerImbalance = firstMaxActivePowerImbalance,
                SecondMaxActivePowerImbalance = secondMaxActivePowerImbalance,
                FirstMaxReactivePowerImbalance = firstMaxReactivePowerImbalance,
                SecondMaxReactivePowerImbalance = secondMaxReactivePowerImbalance,
                FirstAverageTotalActivePowerImbalance = firstAverageTotalActivePowerImbalance,
                SecondAverageTotalActivePowerImbalance = secondAverageTotalActivePowerImbalance,
                FirstAverageTotalReactivePowerImbalance = firstAverageTotalReactivePowerImbalance,
                SecondAverageTotalReactivePowerImbalance = secondAverageTotalReactivePowerImbalance,
                FirstAverageDeviation = firstAverageDeviation,
                SecondAverageDeviation = secondAverageDeviation,
                FirstStandardDeviation = firstStandardDeviation,
                SecondStandardDeviation = secondStandardDeviation,
                FirstSuccessRate = firstSuccessRate,
                SecondSuccessRate = secondSuccessRate,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime
            };

            _reportDataList.Add(data);
        }

        public async Task SaveReportAsync()
        {
            if (_reportDataList.Count == 0)
                throw new InvalidOperationException("Нет данных для сохранения отчета.");

            string filePath = Path.Combine("D:\\учеба\\магистратура\\3 курс\\диплом ит\\мое\\тест отчеты",
                            $"Отчет_{DateTime.Now:yyyyMMdd_HHmmss}.docx");

            // Создание документа
            Document document = new Document();
            DocumentBuilder builder = new DocumentBuilder(document);

            // Заголовок
            builder.Font.Size = 16;
            builder.Font.Bold = true;
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            builder.Writeln("Отчет по надежности");
            builder.ParagraphFormat.ClearFormatting();

            foreach (var data in _reportDataList)
            {
                // Подзаголовок
                builder.Font.Size = 12;
                builder.Font.Bold = true;
                builder.Writeln($"Расчетный интервал: от {data.StartDateTime} до {data.EndDateTime}");
                builder.ParagraphFormat.ClearFormatting();

                // Создание таблицы
                Table table = builder.StartTable();

                // Заголовок таблицы
                AddCellToTable(builder, "Показатель", true);
                AddCellToTable(builder, "Единицы измерения", true);
                AddCellToTable(builder, data.FirstSetLabel, true);
                AddCellToTable(builder, data.SecondSetLabel, true);
                builder.EndRow();

                // Заполнение строк
                AddTableRow(builder, "Максимальный небаланс по активной мощности", "МВт", data.FirstMaxActivePowerImbalance, data.SecondMaxActivePowerImbalance);
                AddTableRow(builder, "Максимальный небаланс по реактивной мощности", "МВАр", data.FirstMaxReactivePowerImbalance, data.SecondMaxReactivePowerImbalance);
                AddTableRow(builder, "Усредненный суммарный небаланс по активной мощности", "МВт", data.FirstAverageTotalActivePowerImbalance, data.SecondAverageTotalActivePowerImbalance);
                AddTableRow(builder, "Усредненный суммарный небаланс по реактивной мощности", "МВАр", data.FirstAverageTotalReactivePowerImbalance, data.SecondAverageTotalReactivePowerImbalance);
                AddTableRow(builder, "Среднее математическое ожидание отклонения измеренного от оцененного", "(о.е.)", data.FirstAverageDeviation, data.SecondAverageDeviation);
                AddTableRow(builder, "Среднеквадратичное отклонение измеренных значений", "(о.е.)", data.FirstStandardDeviation, data.SecondStandardDeviation);
                AddTableRow(builder, "Успешность ОС", "%", data.FirstSuccessRate, data.SecondSuccessRate);

                builder.EndTable();
                builder.Writeln();
            }

            document.Save(filePath);

            await Task.CompletedTask;
        }

        private void AddCellToTable(DocumentBuilder builder, string text, bool isHeader)
        {
            if (isHeader)
            {
                builder.Font.Bold = true;
            }

            builder.InsertCell();
            builder.Write(text);
            builder.Font.Bold = false;
        }

        private void AddTableRow(DocumentBuilder builder, string indicator, string unit, double firstValue, double secondValue)
        {
            AddCellToTable(builder, indicator, false);
            AddCellToTable(builder, unit, false);
            AddCellToTable(builder, firstValue.ToString("F3"), false);
            AddCellToTable(builder, secondValue.ToString("F3"), false);
            builder.EndRow();
        }
    }
}
