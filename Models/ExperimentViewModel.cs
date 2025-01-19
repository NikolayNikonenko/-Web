namespace перенос_бд_на_Web.Models
{
    public class ExperimentViewModel
    {
        public string ExperimentNumber { get; set; }
        public DateTime? DateExperiment { get; set; }
        public string CalculationInterval { get; set; }
        public bool ApplyFGO { get; set; }
        public double TelemetryNumber { get; set; }
        public string RecommendedAction { get; set; }
    }
}
