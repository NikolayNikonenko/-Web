using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace перенос_бд_на_Web.Controllers
{

    [ApiController]
    [Route("api/document")]
    public class DocumentController : ControllerBase
    {
        [HttpGet("about")]
        public IActionResult GetAboutDocument()
        {
            // путь к документу
            var filePath = "C:\\Users\\User\\Desktop\\учеба\\магистратура\\5 семак\\диплом по ИТ\\страница о программе\\Руководство пользователя.docx";

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            var fileName = "Руководство пользователя.docx";

            return PhysicalFile(filePath, contentType, fileName);
        }

    }
}