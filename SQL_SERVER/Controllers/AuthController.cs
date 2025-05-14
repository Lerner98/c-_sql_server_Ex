using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using SQL_SERVER.Models;
using SQL_SERVER.Services;
using System;

namespace SQL_SERVER.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { error = "Email and password are required." });

            try
            {
                bool success = _authService.RegisterUser(request.Email, request.Password);
                if (success)
                    return Ok(new { success = true });

                return BadRequest(new { error = "Registration failed or email already exists." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { error = "Email and password are required." });

            try
            {
                var result = _authService.LoginUser(request.Email, request.Password);
                if (result == null)
                    return Unauthorized(new { error = "Invalid credentials." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromHeader(Name = "Authorization")] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { error = "Token is required." });

            try
            {
                _authService.Logout(token);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("validate-session")]
        public IActionResult ValidateSession([FromHeader(Name = "Authorization")] string token)
        {
            try
            {
                var user = _authService.ValidateSession(token);
                if (user == null)
                    return Unauthorized(new { error = "Invalid session." });

                return Ok(new { success = true, user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
