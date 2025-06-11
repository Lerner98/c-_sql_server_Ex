using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQL_SERVER.Services;
using Microsoft.Data.SqlClient;
using System.Text.Json; // ✅ ADDED: For JsonElement support

namespace SQL_SERVER.Controllers
{
    [ApiController]
    [Route("api/translations")]
    public class TranslationController : ControllerBase
    {
        private readonly TranslationService _translationService;

        public TranslationController()
        {
            _translationService = new TranslationService();
        }

        [HttpPost("text")]
        public IActionResult SaveTextTranslation([FromHeader(Name = "Authorization")] string token, [FromBody] JsonElement body)
        {
            Console.WriteLine("[TranslationController] 💾 SaveTextTranslation endpoint hit");
            Console.WriteLine($"[TranslationController] 💾 Token received: {(string.IsNullOrEmpty(token) ? "NO TOKEN" : "TOKEN EXISTS")}");
            Console.WriteLine($"[TranslationController] 💾 Body received: {(body.ValueKind == JsonValueKind.Undefined ? "NULL BODY" : "BODY EXISTS")}");

            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null)
            {
                Console.WriteLine("[TranslationController] 💾 Authentication failed");
                return Unauthorized(new { error = "Invalid session." });
            }

            Console.WriteLine($"[TranslationController] 💾 Authentication successful for user: {user.GetEmail()}");

            try
            {
                // ✅ FIXED: Proper JSON parsing instead of dynamic
                if (body.ValueKind == JsonValueKind.Undefined)
                {
                    Console.WriteLine("[TranslationController] 💾 Missing request body");
                    return BadRequest(new { error = "Request body is required." });
                }

                // Extract required fields with proper error handling
                if (!body.TryGetProperty("fromLang", out var fromLangElement) ||
                    !body.TryGetProperty("toLang", out var toLangElement) ||
                    !body.TryGetProperty("original_text", out var originalTextElement) ||
                    !body.TryGetProperty("translated_text", out var translatedTextElement) ||
                    !body.TryGetProperty("type", out var typeElement))
                {
                    Console.WriteLine("[TranslationController] 💾 Missing required fields in request body");
                    return BadRequest(new { error = "Missing required fields: fromLang, toLang, original_text, translated_text, type" });
                }

                string fromLang = fromLangElement.GetString() ?? "";
                string toLang = toLangElement.GetString() ?? "";
                string originalText = originalTextElement.GetString() ?? "";
                string translatedText = translatedTextElement.GetString() ?? "";
                string type = typeElement.GetString() ?? "";

                Console.WriteLine($"[TranslationController] 💾 Translation data - From: {fromLang}, To: {toLang}, Type: {type}");
                Console.WriteLine($"[TranslationController] 💾 Original: '{originalText.Substring(0, Math.Min(50, originalText.Length))}...'");

                _translationService.SaveTextTranslation(user.GetId(), fromLang, toLang, originalText, translatedText, type);

                Console.WriteLine("[TranslationController] 💾 Text translation saved successfully");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TranslationController] 💾 SaveTextTranslation error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("text")]
        public IActionResult GetTextTranslations([FromHeader(Name = "Authorization")] string token)
        {
            Console.WriteLine("[TranslationController] 📖 GetTextTranslations endpoint hit");

            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null)
            {
                Console.WriteLine("[TranslationController] 📖 Authentication failed");
                return Unauthorized(new { error = "Invalid session." });
            }

            try
            {
                var result = _translationService.GetTextTranslations(user.GetId());
                Console.WriteLine($"[TranslationController] 📖 Retrieved {result.Count} text translations");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TranslationController] 📖 GetTextTranslations error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("voice")]
        public IActionResult SaveVoiceTranslation([FromHeader(Name = "Authorization")] string token, [FromBody] JsonElement body)
        {
            Console.WriteLine("[TranslationController] 🎤 SaveVoiceTranslation endpoint hit");

            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null)
            {
                Console.WriteLine("[TranslationController] 🎤 Authentication failed");
                return Unauthorized(new { error = "Invalid session." });
            }

            try
            {
                // ✅ FIXED: Proper JSON parsing instead of dynamic
                if (body.ValueKind == JsonValueKind.Undefined)
                {
                    Console.WriteLine("[TranslationController] 🎤 Missing request body");
                    return BadRequest(new { error = "Request body is required." });
                }

                // Extract required fields with proper error handling
                if (!body.TryGetProperty("fromLang", out var fromLangElement) ||
                    !body.TryGetProperty("toLang", out var toLangElement) ||
                    !body.TryGetProperty("original_text", out var originalTextElement) ||
                    !body.TryGetProperty("translated_text", out var translatedTextElement) ||
                    !body.TryGetProperty("type", out var typeElement))
                {
                    Console.WriteLine("[TranslationController] 🎤 Missing required fields in request body");
                    return BadRequest(new { error = "Missing required fields: fromLang, toLang, original_text, translated_text, type" });
                }

                string fromLang = fromLangElement.GetString() ?? "";
                string toLang = toLangElement.GetString() ?? "";
                string originalText = originalTextElement.GetString() ?? "";
                string translatedText = translatedTextElement.GetString() ?? "";
                string type = typeElement.GetString() ?? "";

                _translationService.SaveVoiceTranslation(user.GetId(), fromLang, toLang, originalText, translatedText, type);

                Console.WriteLine("[TranslationController] 🎤 Voice translation saved successfully");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TranslationController] 🎤 SaveVoiceTranslation error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("voice")]
        public IActionResult GetVoiceTranslations([FromHeader(Name = "Authorization")] string token)
        {
            Console.WriteLine("[TranslationController] 🎙️ GetVoiceTranslations endpoint hit");

            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null)
            {
                Console.WriteLine("[TranslationController] 🎙️ Authentication failed");
                return Unauthorized(new { error = "Invalid session." });
            }

            try
            {
                var result = _translationService.GetVoiceTranslations(user.GetId());
                Console.WriteLine($"[TranslationController] 🎙️ Retrieved {result.Count} voice translations");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TranslationController] 🎙️ GetVoiceTranslations error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("delete/{id}")]
        [HttpPost("delete/{id}")]
        public IActionResult DeleteTranslation([FromHeader(Name = "Authorization")] string token, Guid id)
        {
            Console.WriteLine($"[TranslationController] 🗑️ DeleteTranslation endpoint hit for ID: {id}");

            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null)
            {
                Console.WriteLine("[TranslationController] 🗑️ Authentication failed");
                return Unauthorized(new { error = "Invalid session." });
            }

            try
            {
                _translationService.DeleteTranslation(user.GetId(), id);
                Console.WriteLine("[TranslationController] 🗑️ Translation deleted successfully");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TranslationController] 🗑️ DeleteTranslation error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete]
        public IActionResult ClearAllTranslations([FromHeader(Name = "Authorization")] string token)
        {
            Console.WriteLine("[TranslationController] 🧹 ClearAllTranslations endpoint hit");

            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null)
            {
                Console.WriteLine("[TranslationController] 🧹 Authentication failed");
                return Unauthorized(new { error = "Invalid session." });
            }

            try
            {
                _translationService.ClearTranslations(user.GetId());
                Console.WriteLine("[TranslationController] 🧹 All translations cleared successfully");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TranslationController] 🧹 ClearAllTranslations error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public IActionResult GetStatistics([FromHeader(Name = "Authorization")] string token)
        {
            Console.WriteLine("[TranslationController] 📊 GetStatistics endpoint hit");

            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null)
            {
                Console.WriteLine("[TranslationController] 📊 Authentication failed");
                return Unauthorized(new { error = "Invalid session." });
            }

            try
            {
                var stats = _translationService.GetStatistics(user.GetId());
                Console.WriteLine("[TranslationController] 📊 Statistics retrieved successfully");
                return Ok(stats);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TranslationController] 📊 GetStatistics error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("audit-logs")]
        public IActionResult GetAuditLogs([FromHeader(Name = "Authorization")] string token)
        {
            Console.WriteLine("[TranslationController] 📋 GetAuditLogs endpoint hit");

            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null)
            {
                Console.WriteLine("[TranslationController] 📋 Authentication failed");
                return Unauthorized(new { error = "Invalid session." });
            }

            try
            {
                var logs = _translationService.GetAuditLogs(user.GetId());
                Console.WriteLine("[TranslationController] 📋 Audit logs retrieved successfully");
                return Ok(logs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TranslationController] 📋 GetAuditLogs error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}