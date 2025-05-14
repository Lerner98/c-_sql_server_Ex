using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SQL_SERVER.Services;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SQL_SERVER.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToolsController : ControllerBase
    {
        private readonly OpenAIHelper _openAi;

        public ToolsController(OpenAIHelper openAi)
        {
            _openAi = openAi ?? throw new ArgumentNullException(nameof(openAi));
        }


        [HttpPost("translate")]
        public async Task<IActionResult> Translate([FromHeader(Name = "Authorization")] string token, [FromBody] dynamic body)
        {
            var user = TranslationService.OptionalValidateSession(token);
            if (body == null || body.text == null || body.targetLang == null)
                return BadRequest(new { error = "Text and target language are required." });

            string text = body.text;
            string targetLang = body.targetLang;
            string sourceLang = body.sourceLang ?? "auto";

            try
            {
                if (sourceLang == "auto")
                {
                    sourceLang = await _openAi.DetectLanguage(text);
                    if (sourceLang == "unknown")
                        sourceLang = "en";
                }

                if (sourceLang == targetLang)
                    return Ok(new { translatedText = text, detectedLang = sourceLang });

                var translatedText = await _openAi.TranslateText(sourceLang, targetLang, text);
                return Ok(new { translatedText, detectedLang = sourceLang });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to translate", details = ex.Message });
            }
        }

        [HttpPost("recognize-text")]
        public async Task<IActionResult> RecognizeText([FromHeader(Name = "Authorization")] string token, [FromBody] dynamic body)
        {
            var user = TranslationService.ValidateSessionAndExtract(token);
            if (body == null || body.imageBase64 == null)
                return BadRequest(new { error = "Image data is required." });

            try
            {
                string base64 = body.imageBase64;
                string result = await _openAi.RecognizeTextFromImage(base64);
                return Ok(new { text = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to recognize text", details = ex.Message });
            }
        }

        [HttpPost("speech-to-text")]
        public async Task<IActionResult> SpeechToText([FromHeader(Name = "Authorization")] string token)
        {
            var user = TranslationService.ValidateSessionAndExtract(token);
            if (!Request.Form.Files.Any())
                return BadRequest(new { error = "Audio file is required." });

            var file = Request.Form.Files[0];
            if (!file.ContentType.StartsWith("audio"))
                return BadRequest(new { error = "Invalid audio format." });

            try
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                var result = await _openAi.SpeechToText(ms.ToArray(), file.FileName);
                return Ok(new { text = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to transcribe", details = ex.Message });
            }
        }

        [HttpPost("text-to-speech")]
        public async Task<IActionResult> TextToSpeech([FromHeader(Name = "Authorization")] string token, [FromBody] dynamic body)
        {
            var user = TranslationService.ValidateSessionAndExtract(token);
            if (body == null || body.text == null)
                return BadRequest(new { error = "Text is required." });

            try
            {
                string text = body.text;
                byte[] audio = await _openAi.TextToSpeech(text);
                return File(audio, "audio/mpeg", "speech.mp3");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to generate speech", details = ex.Message });
            }
        }

        [HttpGet("languages")]
        public IActionResult SearchLanguages([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { error = "Query parameter is required." });

            var result = TranslationService.SearchLanguages(query);
            return Ok(result);
        }
    }
}
