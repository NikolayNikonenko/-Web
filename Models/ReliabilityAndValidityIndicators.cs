using System.ComponentModel.DataAnnotations;

namespace перенос_бд_на_Web.Models
{
    public class ReliabilityAndValidityIndicators
    {
        [Key]
        public Guid id_indicator { get; set; }
        public int count_nos { get; set; }
        public int total_number_of_os { get; set; }
        public double max_neb_p { get; set; }
        public double max_neb_q { get; set; }
        public double avg_neb_p { get; set; }
        public double avg_neb_q { get; set; }
        public double avg_mo { get; set; }
        public double sko { get; set; }
        public Guid id_report { get; set; }
        public string set_name { get; set; }
        public double success_rate { get; set; }
    }
}
