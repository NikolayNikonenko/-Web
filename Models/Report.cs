namespace перенос_бд_на_Web.Models
{
    public class Report
    {
        public Guid id_report { get; set; }
        public DateTime report_date { get; set; }
        public string report_path { get; set; }
    }
}
