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
        public string NameTM { get; set; }
        public string recomendedActions { get; set; } = "Достоверизация"; // Значение по умолчанию
        public string experiment_label { get; set; }
        public int Id1 { get; set; }

    }

}
