using System.ComponentModel.DataAnnotations;

namespace перенос_бд_на_Web.Models
{
    public class ValidatedTelemetry
    {
        [Key]
        public Guid id_validated_telemetry { get; set; }
        public Guid id_experiment { get; set; }
        public double index_tm { get; set; }
        public string recommended_action { get; set; }
    }
}
