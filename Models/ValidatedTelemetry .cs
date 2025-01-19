using System.ComponentModel.DataAnnotations;

namespace перенос_бд_на_Web.Models
{
    public class ValidatedTelemetry
    {
        [Key]
        public Guid id_validated_telemetry { get; set; }
        public Guid id_experiment { get; set; }
        public double index_tm { get; set; }
        public string recomended_action { get; set; }

        // Навигационное свойство для связи с таблицей experiment
        public Experiment Experiment { get; set; }
    }
}
