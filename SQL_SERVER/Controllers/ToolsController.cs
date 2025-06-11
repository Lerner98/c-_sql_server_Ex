using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SQL_SERVER.Services;
using SQL_SERVER.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;

namespace SQL_SERVER.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToolsController : ControllerBase
    {
        private readonly OpenAIHelper _openAi;
        private readonly TranslationService _translationService;

        // ✅ UPDATED: Add TranslationService to constructor
        public ToolsController(OpenAIHelper openAi, TranslationService translationService)
        {
            _openAi = openAi ?? throw new ArgumentNullException(nameof(openAi));
            _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        }

        [HttpPost("translate")]
        public async Task<IActionResult> Translate([FromHeader(Name = "Authorization")] string? token, [FromBody] TranslateRequest request)
        {
            Console.WriteLine("[ToolsController] Translate called");

            // ✅ FIXED: Use instance method instead of static
            var user = TranslationService.OptionalValidateSession(token);
            Console.WriteLine($"[ToolsController] User validation: {(user != null ? "authenticated" : "guest")}");

            if (request == null || string.IsNullOrEmpty(request.Text) || string.IsNullOrEmpty(request.TargetLang))
                return BadRequest(new { error = "Text and target language are required." });

            string text = request.Text;
            string targetLang = request.TargetLang;
            string sourceLang = request.SourceLang ?? "auto";

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
                Console.WriteLine($"[ToolsController] Translation successful");
                return Ok(new { translatedText, detectedLang = sourceLang });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ToolsController] Translation error: {ex.Message}");
                return StatusCode(500, new { error = "Failed to translate", details = ex.Message });
            }
        }
        // In your ToolsController.cs, fix the RecognizeText method:

        [HttpPost("recognize-text")]
        public async Task<IActionResult> RecognizeText([FromHeader(Name = "Authorization")] string? token, [FromBody] JsonElement body)
        {
            Console.WriteLine("[ToolsController] 📷 RecognizeText endpoint hit");
            Console.WriteLine($"[ToolsController] 📷 Token received: {(string.IsNullOrEmpty(token) ? "NO TOKEN" : "TOKEN EXISTS")}");
            Console.WriteLine($"[ToolsController] 📷 Body received: {(body.ValueKind == JsonValueKind.Undefined ? "NULL BODY" : "BODY EXISTS")}");

            // ✅ FIXED: Use instance method and check for null
            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null)
            {
                Console.WriteLine("[ToolsController] 📷 Authentication failed - user is null");
                return Unauthorized(new { error = "Authentication required." });
            }

            Console.WriteLine($"[ToolsController] 📷 Authentication successful for user: {user.GetEmail()}");

            // ✅ FIXED: Proper JSON parsing instead of dynamic
            if (body.ValueKind == JsonValueKind.Undefined || !body.TryGetProperty("imageBase64", out var imageElement))
            {
                Console.WriteLine("[ToolsController] 📷 Missing imageBase64 in request body");
                return BadRequest(new { error = "Image data is required." });
            }

            string base64 = imageElement.GetString() ?? "";

            if (string.IsNullOrWhiteSpace(base64))
            {
                Console.WriteLine("[ToolsController] 📷 Empty or invalid base64 image data");
                return BadRequest(new { error = "Valid image data is required." });
            }

            Console.WriteLine($"[ToolsController] 📷 Image data length: {base64.Length} characters");

            try
            {
                Console.WriteLine("[ToolsController] 📷 Calling OpenAI RecognizeTextFromImage...");
                string result = await _openAi.RecognizeTextFromImage(base64);
                Console.WriteLine($"[ToolsController] 📷 Text recognition result: '{result?.Substring(0, Math.Min(100, result?.Length ?? 0))}...'");

                if (string.IsNullOrEmpty(result))
                {
                    Console.WriteLine("[ToolsController] 📷 Warning: Empty text recognition result");
                    return Ok(new { text = "" });
                }

                Console.WriteLine("[ToolsController] 📷 Text recognition successful, returning result");
                return Ok(new { text = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ToolsController] 📷 RecognizeText error: {ex.Message}");
                Console.WriteLine($"[ToolsController] 📷 Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = "Failed to recognize text", details = ex.Message });
            }
        }

        [HttpPost("speech-to-text")]
        public async Task<IActionResult> SpeechToText([FromHeader(Name = "Authorization")] string? token)
        {
            Console.WriteLine("[ToolsController] 🎤 SpeechToText endpoint hit");
            Console.WriteLine($"[ToolsController] 🎤 Token received: {(string.IsNullOrEmpty(token) ? "NO TOKEN" : "TOKEN EXISTS")}");
            Console.WriteLine($"[ToolsController] 🎤 Token length: {token?.Length ?? 0}");
            Console.WriteLine($"[ToolsController] 🎤 Files count: {Request.Form.Files.Count}");
            Console.WriteLine($"[ToolsController] 🎤 Content-Type: {Request.ContentType}");

            try
            {
                // ✅ FIXED: Use instance method and check for null
                var user = TranslationService.ValidateSessionAndExtract(token);
                if (user == null)
                {
                    Console.WriteLine("[ToolsController] 🎤 Authentication failed - user is null");
                    return Unauthorized(new { error = "Authentication required." });
                }

                Console.WriteLine($"[ToolsController] 🎤 Authentication successful for user: {user.GetEmail()}");

                if (!Request.Form.Files.Any())
                {
                    Console.WriteLine("[ToolsController] 🎤 No audio file provided");
                    return BadRequest(new { error = "Audio file is required." });
                }

                var file = Request.Form.Files[0];
                Console.WriteLine($"[ToolsController] 🎤 File received - Name: {file.FileName}, Size: {file.Length}, ContentType: {file.ContentType}");

                if (!file.ContentType.StartsWith("audio"))
                {
                    Console.WriteLine($"[ToolsController] 🎤 Invalid content type: {file.ContentType}");
                    return BadRequest(new { error = "Invalid audio format." });
                }

                // ✅ DEBUG: Check if sourceLang is provided
                var sourceLang = Request.Form["sourceLang"].ToString();
                Console.WriteLine($"[ToolsController] 🎤 Source language: {sourceLang}");

                Console.WriteLine("[ToolsController] 🎤 Processing audio file...");
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                Console.WriteLine($"[ToolsController] 🎤 Audio copied to memory stream, size: {ms.Length} bytes");

                Console.WriteLine("[ToolsController] 🎤 Calling OpenAI SpeechToText...");
                var result = await _openAi.SpeechToText(ms.ToArray(), file.FileName);
                Console.WriteLine($"[ToolsController] 🎤 SpeechToText result: '{result?.Substring(0, Math.Min(50, result?.Length ?? 0))}...'");

                if (string.IsNullOrEmpty(result))
                {
                    Console.WriteLine("[ToolsController] 🎤 Warning: Empty transcription result");
                    return Ok(new { text = "" });
                }

                Console.WriteLine($"[ToolsController] 🎤 SpeechToText successful, returning result");
                return Ok(new { text = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ToolsController] 🎤 SpeechToText error: {ex.Message}");
                Console.WriteLine($"[ToolsController] 🎤 Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = "Failed to transcribe", details = ex.Message });
            }
        }
        [HttpPost("text-to-speech")]
        public async Task<IActionResult> TextToSpeech([FromHeader(Name = "Authorization")] string? token, [FromBody] JsonElement body)
        {
            Console.WriteLine("[ToolsController] 🔊 TextToSpeech endpoint hit");
            Console.WriteLine($"[ToolsController] 🔊 Token received: {(string.IsNullOrEmpty(token) ? "NO TOKEN" : "TOKEN EXISTS")}");
            Console.WriteLine($"[ToolsController] 🔊 Token length: {token?.Length ?? 0}");
            Console.WriteLine($"[ToolsController] 🔊 Body received: {(body.ValueKind == JsonValueKind.Undefined ? "NULL BODY" : "BODY EXISTS")}");
            Console.WriteLine($"[ToolsController] 🔊 Content-Type: {Request.ContentType}");

            try
            {
                // ✅ FIXED: Use instance method and check for null
                var user = TranslationService.ValidateSessionAndExtract(token);
                if (user == null)
                {
                    Console.WriteLine("[ToolsController] 🔊 Authentication failed - user is null");
                    return Unauthorized(new { error = "Authentication required." });
                }

                Console.WriteLine($"[ToolsController] 🔊 Authentication successful for user: {user.GetEmail()}");

                // ✅ FIXED: Proper JSON parsing instead of dynamic
                if (body.ValueKind == JsonValueKind.Undefined || !body.TryGetProperty("text", out var textElement))
                {
                    Console.WriteLine("[ToolsController] 🔊 Missing text in request body");
                    return BadRequest(new { error = "Text is required." });
                }

                string text = textElement.GetString() ?? "";
                string language = "en"; // Default language

                if (body.TryGetProperty("language", out var languageElement))
                {
                    language = languageElement.GetString() ?? "en";
                }

                Console.WriteLine($"[ToolsController] 🔊 Text to convert: '{text.Substring(0, Math.Min(100, text.Length))}...'");
                Console.WriteLine($"[ToolsController] 🔊 Language: {language}");
                Console.WriteLine($"[ToolsController] 🔊 Text length: {text.Length} characters");

                if (string.IsNullOrWhiteSpace(text))
                {
                    Console.WriteLine("[ToolsController] 🔊 Empty or whitespace text provided");
                    return BadRequest(new { error = "Text cannot be empty." });
                }

                Console.WriteLine("[ToolsController] 🔊 Calling OpenAI TextToSpeech...");
                byte[] audio = await _openAi.TextToSpeech(text);
                Console.WriteLine($"[ToolsController] 🔊 TextToSpeech successful, generated {audio.Length} bytes");

                if (audio == null || audio.Length == 0)
                {
                    Console.WriteLine("[ToolsController] 🔊 Warning: Empty audio result from OpenAI");
                    return StatusCode(500, new { error = "Failed to generate audio - empty result" });
                }

                Console.WriteLine($"[ToolsController] 🔊 Returning audio file with {audio.Length} bytes");
                return File(audio, "audio/mpeg", "speech.mp3");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ToolsController] 🔊 TextToSpeech error: {ex.Message}");
                Console.WriteLine($"[ToolsController] 🔊 Stack trace: {ex.StackTrace}");
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