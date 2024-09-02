using System.ComponentModel.DataAnnotations;

namespace перенос_бд_на_Web.Models
{
    public class Slices
    {
        [Key]
        public Guid SliceID { get; set; }
        public string SliceName { get; set; }
        public string SlicePath { get; set; }
    }
}
