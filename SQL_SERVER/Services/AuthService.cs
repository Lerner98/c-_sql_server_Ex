using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SQL_SERVER.Models;
using TranslationHubServer;

namespace SQL_SERVER.Services
{
    public class AuthService
    {
        private readonly DBHandler _dbHandler;
        private readonly JwtService _jwtService;

        public AuthService(DBHandler dbHandler, JwtService jwtService)
        {
            _dbHandler = dbHandler;
            _jwtService = jwtService;
        }

        public bool RegisterUser(string email, string password)
        {
            string hashedPassword = HashPassword(password);
            return _dbHandler.CreateUser(email, hashedPassword);
        }

        public object LoginUser(string email, string password)
        {
            Console.WriteLine($"Attempting to login user: {email}");

            string storedHash;
            Guid userId;
            string defaultFromLang;
            string defaultToLang;

            try
            {
                using (var reader = _dbHandler.FindUserByEmail(email))
                {
                    Console.WriteLine("Reader obtained, checking if data exists...");

                    if (!reader.Read())
                    {
                        Console.WriteLine("No user found with email: " + email);
                        return null;
                    }

                    Console.WriteLine("User found, reading data...");
                    Console.WriteLine($"Available columns: {string.Join(", ", Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i)))}");

                    storedHash = reader["password_hash"]?.ToString();
                    userId = Guid.Parse(reader["UserId"].ToString());
                    defaultFromLang = reader["default_from_lang"]?.ToString();
                    defaultToLang = reader["default_to_lang"]?.ToString();

                    Console.WriteLine($"User data read successfully. UserId: {userId}");
                }

                Console.WriteLine("Verifying password...");
                if (!VerifyPassword(password, storedHash))
                {
                    Console.WriteLine("Password verification failed");
                    return null;
                }

                Console.WriteLine("Password verified, generating token...");
                string token = _jwtService.GenerateToken(userId, email);

                // Generate a separate short session ID for database storage
                string shortSessionId = Guid.NewGuid().ToString("N")[..32]; // 32 characters, fits in 64-char limit

                Guid sessionId = Guid.NewGuid();
                DateTime expiresAt = DateTime.UtcNow.AddHours(24);

                Console.WriteLine("Creating session...");
                // Store the short session ID in database
                _dbHandler.CreateSession(userId, sessionId, expiresAt, shortSessionId);

                Console.WriteLine("Login successful");
                return new
                {
                    success = true,
                    user = new
                    {
                        id = userId,
                        email,
                        defaultFromLang,
                        defaultToLang,
                        signed_session_id = token // Still return the JWT token to client
                    },
                    token
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LoginUser: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public void Logout(string token)
        {
            // For logout, we need to find the session by JWT validation first
            var principal = _jwtService.ValidateToken(token);
            if (principal != null)
            {
                // Since we don't store JWT tokens in DB anymore, we can't use DB logout
                // JWT tokens will naturally expire, so logout is essentially a client-side operation
                Console.WriteLine("Logout processed (JWT tokens expire naturally)");
            }
        }

        // Add this debug logging to your AuthService ValidateSession method
        // Fix your AuthService ValidateSession method - use correct claim names!
        public object? ValidateSession(string signedSessionId)
        {
            Console.WriteLine($"[AuthService] ValidateSession called with token: {signedSessionId?.Substring(0, Math.Min(20, signedSessionId?.Length ?? 0))}...");

            try
            {
                var principal = _jwtService.ValidateToken(signedSessionId);
                Console.WriteLine($"[AuthService] JWT validation result: {(principal != null ? "SUCCESS" : "FAILED")}");

                if (principal == null)
                {
                    Console.WriteLine("[AuthService] JWT validation returned null - token is invalid");
                    return null;
                }

                // ✅ FIXED: Use correct claim names that .NET JWT library generates
                var userIdClaim = principal.FindFirst("id")?.Value;
                var emailClaim = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

                Console.WriteLine($"[AuthService] Claims found - UserId: {userIdClaim}, Email: {emailClaim}");

                if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(emailClaim))
                {
                    Console.WriteLine("[AuthService] Missing required claims in token");
                    return null;
                }

                Console.WriteLine("[AuthService] Token validation successful, returning user data");
                return new
                {
                    id = Guid.Parse(userIdClaim),
                    email = emailClaim,
                    defaultFromLang = (string?)null,
                    defaultToLang = (string?)null,
                    signed_session_id = signedSessionId
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthService] ValidateSession exception: {ex.Message}");
                Console.WriteLine($"[AuthService] Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}