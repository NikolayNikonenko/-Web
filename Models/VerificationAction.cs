namespace перенос_бд_на_Web.Models
{
    public class VerificationAction
    {
        public int TelemetryId { get; set; }        // Идентификатор телеметрии
        public int Id1 { get; set; }                // Номер узла
        public string Privyazka { get; set; }
        public string ActionName { get; set; }      // Название выбранного действия
        public DateTime StartDate { get; set; }     // Дата начала временного интервала
        public DateTime EndDate { get; set; }       // Дата окончания временного интервала
    }
}
