using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

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

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                // Добавляем основной документ
                MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document(new Body());

                Body body = mainPart.Document.Body;

                // Заголовок
                AddParagraph(body, "Отчет по надежности", true, 16);

                foreach (var data in _reportDataList)
                {
                    // Подзаголовок
                    AddParagraph(body, $"Расчетный интервал: от {data.StartDateTime} до {data.EndDateTime}", true, 12);

                    // Таблица
                    Table table = new Table();
                    table.AppendChild(CreateTableProperties());

                    // Заголовки таблицы
                    TableRow headerRow = new TableRow();
                    headerRow.Append(CreateTableCell("Показатель", true));
                    headerRow.Append(CreateTableCell("Единицы измерения", true));
                    headerRow.Append(CreateTableCell(data.FirstSetLabel, true));
                    headerRow.Append(CreateTableCell(data.SecondSetLabel, true));
                    table.Append(headerRow);

                    // Добавление строк с данными
                    table.Append(CreateDataRow("Максимальный небаланс по активной мощности", "МВт", data.FirstMaxActivePowerImbalance, data.SecondMaxActivePowerImbalance));
                    table.Append(CreateDataRow("Максимальный небаланс по реактивной мощности", "МВАр", data.FirstMaxReactivePowerImbalance, data.SecondMaxReactivePowerImbalance));
                    table.Append(CreateDataRow("Усредненный суммарный небаланс по активной мощности", "МВт", data.FirstAverageTotalActivePowerImbalance, data.SecondAverageTotalActivePowerImbalance));
                    table.Append(CreateDataRow("Усредненный суммарный небаланс по реактивной мощности", "МВАр", data.FirstAverageTotalReactivePowerImbalance, data.SecondAverageTotalReactivePowerImbalance));
                    table.Append(CreateDataRow("Среднее математическое ожидание отклонения измеренного от оцененного", "(о.е.)", data.FirstAverageDeviation, data.SecondAverageDeviation));
                    table.Append(CreateDataRow("Среднеквадратичное отклонение измеренных значений", "(о.е.)", data.FirstStandardDeviation, data.SecondStandardDeviation));
                    table.Append(CreateDataRow("Успешность ОС", "%", data.FirstSuccessRate, data.SecondSuccessRate));

                    body.Append(table);
                }

                mainPart.Document.Save();
            }

            await Task.CompletedTask;
        }

        private void AddParagraph(Body body, string text, bool bold, int fontSize)
        {
            Paragraph paragraph = new Paragraph();
            Run run = new Run();
            RunProperties runProperties = new RunProperties();

            if (bold)
                runProperties.Append(new Bold());
            runProperties.Append(new FontSize { Val = (fontSize * 2).ToString() });

            run.Append(runProperties);
            run.Append(new Text(text));
            paragraph.Append(run);
            body.Append(paragraph);
        }

        private TableCell CreateTableCell(string text, bool bold)
        {
            TableCell cell = new TableCell();
            Paragraph paragraph = new Paragraph();
            Run run = new Run();
            if (bold)
                run.Append(new RunProperties(new Bold()));

            run.Append(new Text(text));
            paragraph.Append(run);
            cell.Append(paragraph);
            return cell;
        }

        private TableRow CreateDataRow(string indicator, string unit, double firstValue, double secondValue)
        {
            TableRow row = new TableRow();
            row.Append(CreateTableCell(indicator, false));
            row.Append(CreateTableCell(unit, false));
            row.Append(CreateTableCell(firstValue.ToString("F3"), false));
            row.Append(CreateTableCell(secondValue.ToString("F3"), false));
            return row;
        }

        private TableProperties CreateTableProperties()
        {
            return new TableProperties(
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 4 },
                    new BottomBorder { Val = BorderValues.Single, Size = 4 },
                    new LeftBorder { Val = BorderValues.Single, Size = 4 },
                    new RightBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }));
        }
    }
}
