using Microsoft.AspNetCore.Mvc;
using SQL_SERVER.Services;
using System;

namespace SQL_SERVER.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Change this to respond to /api/languages
    public class LanguagesController : ControllerBase
    {
        [HttpGet]
        public IActionResult SearchLanguages([FromQuery] string query = "")
        {
            try
            {
                var result = TranslationService.SearchLanguages(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to search languages.", details = ex.Message });
            }
        }
    }
}