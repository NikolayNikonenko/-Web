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
            var filePath = "D:\\учеба\\магистратура\\3 курс\\диплом ит\\мое\\Руководство пользователя\\Руководство пользователя.docx";

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