namespace перенос_бд_на_Web.Models
{
    public class NedostovernayaTM
    {
        public Guid ID { get; set; }
        public double IndexTm { get; set; }
        public double CorrTm { get; set; }
        public string Status { get; set; }
        public double MaxLagranj { get; set; }
        public double AvgLagranj { get; set; }

    }

}
