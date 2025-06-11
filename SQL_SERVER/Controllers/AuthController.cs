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
                // Clean the token (remove "Bearer " prefix if present)
                string cleanToken = token?.StartsWith("Bearer ") == true ? token.Substring(7) : token;
                _authService.Logout(cleanToken);
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
            Console.WriteLine($"[AuthController] ValidateSession called with token length: {token?.Length}");

            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    Console.WriteLine("[AuthController] No token provided");
                    return BadRequest(new { error = "Token is required." });
                }

                // Clean the token (remove "Bearer " prefix if present)
                string cleanToken = token?.StartsWith("Bearer ") == true ? token.Substring(7) : token;
                Console.WriteLine($"[AuthController] Clean token length: {cleanToken?.Length}");
                Console.WriteLine($"[AuthController] Token starts with: {cleanToken?.Substring(0, Math.Min(20, cleanToken?.Length ?? 0))}...");

                var user = _authService.ValidateSession(cleanToken);

                if (user == null)
                {
                    Console.WriteLine("[AuthController] ValidateSession returned null");
                    return Unauthorized(new { error = "Invalid session." });
                }

                Console.WriteLine("[AuthController] Validation successful");
                return Ok(new { success = true, user });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthController] Validation exception: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}