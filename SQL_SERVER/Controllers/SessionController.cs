using Microsoft.AspNetCore.Mvc;
using SQL_SERVER.Services;
using System;

namespace SQL_SERVER.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly AuthService _authService;

        public SessionController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("validate")]
        public IActionResult ValidateSession([FromHeader(Name = "Authorization")] string? token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return BadRequest(new { error = "Token is required." });

                // Clean the token (remove "Bearer " prefix if present)
                string cleanToken = token.StartsWith("Bearer ") ? token.Substring(7) : token;
                var user = _authService.ValidateSession(cleanToken);
                if (user == null)
                    return Unauthorized(new { error = "Invalid session." });
                return Ok(new { success = true, user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromHeader(Name = "Authorization")] string? token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return BadRequest(new { error = "Token is required." });

                // Clean the token (remove "Bearer " prefix if present)
                string cleanToken = token.StartsWith("Bearer ") ? token.Substring(7) : token;
                _authService.Logout(cleanToken);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}