using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SQL_SERVER.Services;
using SQL_SERVER.Helpers;


namespace SQL_SERVER.Controllers
{
    [ApiController]
    [Route("api/tools")]
    public class ExtractTextController : ControllerBase
    {
        [HttpPost("extract-text")]
        public IActionResult Extract([FromHeader(Name = "Authorization")] string token, [FromBody] dynamic body)
        {
            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null)
            {
                return Unauthorized(new { error = "Login required", code = "AUTH_REQUIRED" });
            }

            string uri = body?.uri;
            if (string.IsNullOrEmpty(uri))
            {
                return BadRequest(new { error = "File URI is required" });
            }

            try
            {
                byte[] buffer = Convert.FromBase64String(uri);
                string result = ExtractTextFromBuffer(buffer, uri);
                if (result == null)
                {
                    return BadRequest(new { error = "Unsupported file type" });
                }

                return Ok(new { text = result });
            }
            catch
            {
                return StatusCode(500, new { error = "Failed to extract text" });
            }
        }

        private string ExtractTextFromBuffer(byte[] buffer, string base64String)
        {
            string prefix = base64String.Substring(0, Math.Min(50, base64String.Length));
            if (prefix.Contains("application/pdf") || prefix.StartsWith("%PDF"))
            {
                return PDFHelper.ExtractTextFromPdf(buffer); // You implement this using iTextSharp or PdfPig
            }
            if (prefix.Contains("application/vnd.openxmlformats-officedocument.wordprocessingml.document") || prefix.Contains("UEsDB"))
            {
                return DocxHelper.ExtractTextFromDocx(buffer); // You implement this using OpenXml
            }
            if (prefix.Contains("text/plain"))
            {
                return Encoding.UTF8.GetString(buffer);
            }
            return null;
        }
    }
}
