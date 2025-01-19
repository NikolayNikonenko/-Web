using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using перенос_бд_на_Web.Models;

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
            public List<ExperimentViewModel> ExperimentData { get; set; }
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
                               DateTime? startDateTime, DateTime? endDateTime,
                               List<ExperimentViewModel> experimentData)
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
                EndDateTime = endDateTime,
                ExperimentData = experimentData // Инициализация данных экспериментов
            };

            _reportDataList.Add(data);
        }

        public async Task SaveReportAsync(string filePath)
        {
            if (_reportDataList.Count == 0)
                throw new InvalidOperationException("Нет данных для сохранения отчета.");

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                // Добавляем основной документ
                MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document(new Body());

                Body body = mainPart.Document.Body;

                // Заголовок
                AddParagraph(body, "Отчет по надежности и достоверности", bold: true, fontSize: 14, alignment: JustificationValues.Center);

                foreach (var data in _reportDataList)
                {
                    // Подзаголовок
                    AddParagraph(body, $"Расчетный интервал: от {data.StartDateTime} до {data.EndDateTime}", bold: false, fontSize: 12, alignment: JustificationValues.Left);

                    // Таблица с ключевыми показателями
                    Table mainTable = new Table();
                    mainTable.AppendChild(CreateTableProperties());

                    // Заголовки таблицы
                    TableRow headerRow = new TableRow();
                    headerRow.Append(CreateTableCell("Показатель", true));
                    headerRow.Append(CreateTableCell("Единицы измерения", true));
                    headerRow.Append(CreateTableCell(data.FirstSetLabel, true));
                    headerRow.Append(CreateTableCell(data.SecondSetLabel, true));
                    mainTable.Append(headerRow);

                    // Добавление строк с данными
                    mainTable.Append(CreateDataRow("Максимальный небаланс по активной мощности", "МВт", data.FirstMaxActivePowerImbalance, data.SecondMaxActivePowerImbalance));
                    mainTable.Append(CreateDataRow("Максимальный небаланс по реактивной мощности", "МВАр", data.FirstMaxReactivePowerImbalance, data.SecondMaxReactivePowerImbalance));
                    mainTable.Append(CreateDataRow("Усредненный суммарный небаланс по активной мощности", "МВт", data.FirstAverageTotalActivePowerImbalance, data.SecondAverageTotalActivePowerImbalance));
                    mainTable.Append(CreateDataRow("Усредненный суммарный небаланс по реактивной мощности", "МВАр", data.FirstAverageTotalReactivePowerImbalance, data.SecondAverageTotalReactivePowerImbalance));
                    mainTable.Append(CreateDataRow("Среднее математическое ожидание отклонения измеренного от оцененного", "(о.е.)", data.FirstAverageDeviation, data.SecondAverageDeviation));
                    mainTable.Append(CreateDataRow("Среднеквадратичное отклонение измеренных значений", "(о.е.)", data.FirstStandardDeviation, data.SecondStandardDeviation));
                    mainTable.Append(CreateDataRow("Успешность ОС", "%", data.FirstSuccessRate, data.SecondSuccessRate));

                    body.Append(mainTable);

                    // Создание таблицы для ExperimentViewModel
                    AddParagraph(body, "Детали эксперимента", bold: false, fontSize: 12, alignment: JustificationValues.Left);
                    Table experimentTable = new Table();
                    experimentTable.AppendChild(CreateTableProperties());

                    // Заголовки таблицы эксперимента
                    TableRow experimentHeaderRow = new TableRow();
                    experimentHeaderRow.Append(CreateTableCell("Номер эксперимента", true));
                    experimentHeaderRow.Append(CreateTableCell("Дата проведения эксперимента", true));
                    experimentHeaderRow.Append(CreateTableCell("Расчетный интервал", true));
                    experimentHeaderRow.Append(CreateTableCell("Использование ФГО", true));
                    experimentHeaderRow.Append(CreateTableCell("Номер телеизмерений", true));
                    experimentHeaderRow.Append(CreateTableCell("Действие по достоверизации", true));
                    experimentTable.Append(experimentHeaderRow);

                    // Группировка данных по указанным колонкам
                    var groupedData = data.ExperimentData
                        .GroupBy(e => new { e.ExperimentNumber, e.DateExperiment, e.CalculationInterval, e.ApplyFGO })
                        .ToList();

                    // Заполнение таблицы с объединением ячеек
                    foreach (var group in groupedData)
                    {
                        bool isFirstRow = true; // Флаг для первой строки группы

                        foreach (var experiment in group)
                        {
                            TableRow row = new TableRow();

                            if (isFirstRow)
                            {
                                // Для первой строки группы добавляем объединенные ячейки
                                row.Append(CreateMergedCell(group.Key.ExperimentNumber.ToString(), group.Count(), true));
                                row.Append(CreateMergedCell(group.Key.DateExperiment.ToString(), group.Count(), true));
                                row.Append(CreateMergedCell(group.Key.CalculationInterval.ToString(), group.Count(), true));
                                row.Append(CreateMergedCell(group.Key.ApplyFGO ? "Да" : "Нет", group.Count(), true));

                                isFirstRow = false; // Сбрасываем флаг
                            }
                            else
                            {
                                // Для всех последующих строк добавляем продолжение объединенных ячеек
                                row.Append(CreateMergedCell("", group.Count(), false)); // Пустая ячейка для первой
                                row.Append(CreateMergedCell("", group.Count(), false)); // Пустая ячейка для второй
                                row.Append(CreateMergedCell("", group.Count(), false)); // Пустая ячейка для третьей
                                row.Append(CreateMergedCell("", group.Count(), false)); // Пустая ячейка для четвёртой
                            }

                            // Добавляем уникальные данные для этой строки (оставшиеся столбцы)
                            row.Append(CreateTableCell(experiment.TelemetryNumber.ToString(), false));
                            row.Append(CreateTableCell(experiment.RecommendedAction.ToString(), false));

                            // Добавляем строку в таблицу
                            experimentTable.Append(row);
                        }
                    }

                    body.Append(experimentTable);
                }

                mainPart.Document.Save();
            }

            await Task.CompletedTask;
        }

        private TableCell CreateEmptyCell()
        {
            return CreateTableCell("", false); // Пустая ячейка
        }


        private TableCell CreateMergedCell(string text, int rowSpan, bool isFirstRow)
        {
            TableCell cell = CreateTableCell(text, false);

            if (rowSpan > 1)
            {
                // Устанавливаем свойство VerticalMerge для объединения ячеек
                TableCellProperties cellProperties = new TableCellProperties();
                cellProperties.Append(new VerticalMerge { Val = isFirstRow ? MergedCellValues.Restart : MergedCellValues.Continue });
                cell.Append(cellProperties);
            }

            return cell;
        }

        private void AddParagraph(Body body, string text, bool bold, int fontSize, JustificationValues alignment)
        {
            Paragraph paragraph = new Paragraph();

            // Добавляем выравнивание (центрирование или по левому краю)
            ParagraphProperties paragraphProperties = new ParagraphProperties();
            paragraphProperties.Justification = new Justification() { Val = alignment };

            Run run = new Run();
            RunProperties runProperties = new RunProperties();

            if (bold)
                runProperties.Append(new Bold());

            runProperties.Append(new FontSize { Val = (fontSize * 2).ToString() });

            run.Append(runProperties);
            run.Append(new Text(text));

            paragraph.Append(paragraphProperties);
            paragraph.Append(run);

            body.Append(paragraph);
        }

        private TableCell CreateTableCell(string text, bool bold)
        {
            TableCell cell = new TableCell();

            // Добавляем выравнивание по центру для параграфа
            Paragraph paragraph = new Paragraph();
            ParagraphProperties paragraphProperties = new ParagraphProperties();
            paragraphProperties.Justification = new Justification() { Val = JustificationValues.Center };

            Run run = new Run();
            if (bold)
                run.Append(new RunProperties(new Bold()));

            run.Append(new Text(text));

            paragraph.Append(paragraphProperties); // Применяем выравнивание по центру
            paragraph.Append(run);

            cell.Append(paragraph);

            // Дополнительно выравниваем содержимое ячейки по вертикали по центру
            TableCellProperties cellProperties = new TableCellProperties();
            cellProperties.Append(new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center });
            cell.Append(cellProperties);

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
