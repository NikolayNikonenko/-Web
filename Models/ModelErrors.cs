namespace перенос_бд_на_Web.Models
{
    public class ModelErrors
    {
        public Guid ID { get; set; }
        public double IndexTm { get; set; }
        public string ErrorType { get; set; }
        public string error_status { get; set; } = "Ошибка";
    }
}
