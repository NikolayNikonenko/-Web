using System.ComponentModel.DataAnnotations;

namespace перенос_бд_на_Web.Models
{
    public class ExperimentFiles
    {
        [Key]
        public Guid id_file_after_modified { get; set; }
        public string path_experiment_file { get; set; }
    }
}
