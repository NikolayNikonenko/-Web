namespace перенос_бд_на_Web.Models
{
    public class Experiment
    {
        public Guid id_experiment { get; set; }
        public DateTime? date_experiment { get; set; }
        public DateTime date_experiment_interval { get; set; }
        public bool apply_fgo { get; set; }
        public Guid id_report { get; set; }
        public string experiment_label { get; set; }
    }
}
