using System;
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
            var reader = _dbHandler.FindUserByEmail(email);
            if (!reader.Read()) return null;

            string storedHash = reader["PasswordHash"].ToString();
            Guid userId = Guid.Parse(reader["UserId"].ToString());
            string defaultFromLang = reader["DefaultFromLang"]?.ToString();
            string defaultToLang = reader["DefaultToLang"]?.ToString();

            reader.Close();

            if (!VerifyPassword(password, storedHash)) return null;

            string token = _jwtService.GenerateToken(userId, email);
            Guid sessionId = Guid.NewGuid();
            DateTime expiresAt = DateTime.UtcNow.AddHours(24);
            _dbHandler.CreateSession(userId, sessionId, expiresAt, token);

            return new
            {
                success = true,
                user = new
                {
                    id = userId,
                    email,
                    defaultFromLang,
                    defaultToLang,
                    signed_session_id = token
                },
                token
            };
        }

        public void Logout(string token)
        {
            _dbHandler.Logout(token);
        }


        public object? ValidateSession(string signedSessionId)
        {
            var user = _dbHandler.ValidateSession(signedSessionId);
            if (user == null)
                return null;

            return new
            {
                id = user.GetId(),
                email = user.GetEmail(),
                defaultFromLang = user.GetDefaultFromLang(),
                defaultToLang = user.GetDefaultToLang(),
                signed_session_id = signedSessionId
            };
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
