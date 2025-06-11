using System;
using System.IO;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using SQL_SERVER.Services;

namespace SQL_SERVER.Controllers
{
    [ApiController]
    [Route("api/tools")]
    public class GenerateDocxController : ControllerBase
    {
        [HttpPost("generate-docx")]
        public IActionResult GenerateDocx([FromHeader(Name = "Authorization")] string token, [FromBody] dynamic body)
        {
            // Optional authentication - allow guests
            var user = TranslationService.OptionalValidateSession(token);

            if (body == null || body.text == null)
            {
                return BadRequest(new { error = "Text is required." });
            }

            try
            {
                string text = body.text;
                var docBytes = GenerateWordDocument(text);

                return File(docBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "translated.docx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to generate document", details = ex.Message });
            }
        }

        private byte[] GenerateWordDocument(string text)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (WordprocessingDocument doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = doc.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    Paragraph para = body.AppendChild(new Paragraph());
                    Run run = para.AppendChild(new Run());

                    // Split text by newlines to preserve formatting
                    string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                    for (int i = 0; i < lines.Length; i++)
                    {
                        run.AppendChild(new Text(lines[i]));

                        // Add line breaks between lines (except for the last line)
                        if (i < lines.Length - 1)
                        {
                            run.AppendChild(new Break());
                        }
                    }
                }

                return ms.ToArray();
            }
        }
    }
}