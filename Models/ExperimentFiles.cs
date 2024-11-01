using System.ComponentModel.DataAnnotations;

namespace перенос_бд_на_Web.Models
{
    public class ExperimentFiles
    {
        [Key]
        public Guid Id_file { get; set; }
        public string path_experiment_file { get; set; }
        public Guid Id_experiment { get; set; }
    }
}
