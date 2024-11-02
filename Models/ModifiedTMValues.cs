using System.ComponentModel.DataAnnotations;

namespace перенос_бд_на_Web.Models
{
    public class ModifiedTMValues
    {
        [Key]
        public Guid id_tm_value_after_modified { get; set; }
        public string id_tm_after_modified { get; set; }
        public double izmer_tm_value_after_modified { get; set; }
        public double ocen_tm_value_after_modified { get; set; }
        public double lagranj_tm_value_after_modified { get; set; }
        public Guid id_file_after_modified { get; set; }
    }
}
