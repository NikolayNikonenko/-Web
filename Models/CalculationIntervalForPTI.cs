using System.ComponentModel.DataAnnotations;

namespace перенос_бд_на_Web.Models
{
    public class CalculationIntervalForPTI
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
