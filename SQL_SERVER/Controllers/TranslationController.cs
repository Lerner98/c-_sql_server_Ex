using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQL_SERVER.Services;
using Microsoft.Data.SqlClient;


namespace SQL_SERVER.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationController : ControllerBase
    {
        private readonly TranslationService _translationService;

        public TranslationController()
        {
            _translationService = new TranslationService();
        }

        [HttpPost("text")]
        public IActionResult SaveTextTranslation([FromHeader(Name = "Authorization")] string token, [FromBody] dynamic body)
        {
            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null) return Unauthorized(new { error = "Invalid session." });

            try
            {
                _translationService.SaveTextTranslation(user.GetId(),
                    (string)body.fromLang,
                    (string)body.toLang,
                    (string)body.original_text,
                    (string)body.translated_text,
                    (string)body.type);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("text")]
        public IActionResult GetTextTranslations([FromHeader(Name = "Authorization")] string token)
        {
            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null) return Unauthorized(new { error = "Invalid session." });

            try
            {
                var result = _translationService.GetTextTranslations(user.GetId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("voice")]
        public IActionResult SaveVoiceTranslation([FromHeader(Name = "Authorization")] string token, [FromBody] dynamic body)
        {
            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null) return Unauthorized(new { error = "Invalid session." });

            try
            {
                _translationService.SaveVoiceTranslation(user.GetId(),
                    (string)body.fromLang,
                    (string)body.toLang,
                    (string)body.original_text,
                    (string)body.translated_text,
                    (string)body.type);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("voice")]
        public IActionResult GetVoiceTranslations([FromHeader(Name = "Authorization")] string token)
        {
            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null) return Unauthorized(new { error = "Invalid session." });

            try
            {
                var result = _translationService.GetVoiceTranslations(user.GetId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("delete/{id}")]
        [HttpPost("delete/{id}")]
        public IActionResult DeleteTranslation([FromHeader(Name = "Authorization")] string token, Guid id)
        {
            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null) return Unauthorized(new { error = "Invalid session." });

            try
            {
                _translationService.DeleteTranslation(user.GetId(), id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete]
        public IActionResult ClearAllTranslations([FromHeader(Name = "Authorization")] string token)
        {
            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null) return Unauthorized(new { error = "Invalid session." });

            try
            {
                _translationService.ClearTranslations(user.GetId());
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("/statistics")]
        public IActionResult GetStatistics([FromHeader(Name = "Authorization")] string token)
        {
            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null) return Unauthorized(new { error = "Invalid session." });

            try
            {
                var stats = _translationService.GetStatistics(user.GetId());
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("/audit-logs")]
        public IActionResult GetAuditLogs([FromHeader(Name = "Authorization")] string token)
        {
            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null) return Unauthorized(new { error = "Invalid session." });

            try
            {
                var logs = _translationService.GetAuditLogs(user.GetId());
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
