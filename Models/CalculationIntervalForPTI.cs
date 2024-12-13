using System.ComponentModel.DataAnnotations;

namespace перенос_бд_на_Web.Models
{
    public class CalculationIntervalForPTI
    {
        //[Key]
        //public Guid id { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        //public DateTime createDat { get; set; }= DateTime.Now;
    }
}
