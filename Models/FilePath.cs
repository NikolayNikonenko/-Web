namespace перенос_бд_на_Web.Models
{
    public class FilePath
    {
        public int id { get; set; } // Первичный ключ
        public string path { get; set; } // Путь до файла
        public DateTime updatedAt { get; set; } // Дата и время обновления
    }
}
