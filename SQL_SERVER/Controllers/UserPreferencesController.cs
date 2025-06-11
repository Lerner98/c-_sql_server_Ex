using Microsoft.AspNetCore.Mvc;
using SQL_SERVER.Services;
using System;
using TranslationHubServer;

namespace SQL_SERVER.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PreferencesController : ControllerBase
    {
        private readonly DBHandler _dbHandler;

        public PreferencesController(DBHandler dbHandler)
        {
            _dbHandler = dbHandler;
        }

        [HttpPost]
        public IActionResult UpdatePreferences([FromHeader(Name = "Authorization")] string token, [FromBody] dynamic body)
        {
            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null)
                return Unauthorized(new { error = "Invalid session." });

            try
            {
                string defaultFromLang = body.defaultFromLang;
                string defaultToLang = body.defaultToLang;

                _dbHandler.UpdateUserPreferences(user.GetId(), defaultFromLang, defaultToLang);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to update preferences.", details = ex.Message });
            }
        }
    }
}