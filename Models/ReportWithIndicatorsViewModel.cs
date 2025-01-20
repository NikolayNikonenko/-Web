namespace перенос_бд_на_Web.Models
{
    public class ReportWithIndicatorsViewModel
    {
        public Guid Id { get; set; } // Уникальный идентификатор отчета
        public DateTime? ReportDate { get; set; } // Дата отчета в формате строки
        public List<ReliabilityAndValidityIndicators> Indicators { get; set; } // Список индикаторов (если требуется)
    }
}
