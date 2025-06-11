using Microsoft.Data.SqlClient;
using SQL_SERVER.Models;
using TranslationHubServer;

namespace SQL_SERVER.Services
{
    public class TranslationService
    {
        private static readonly DBHandler _db = new DBHandler();

        // ✅ SIMPLE FIX: Use the exact same JWT key as your appsettings.json
        // Fix your TranslationService ValidateSessionAndExtract method
        public static User? ValidateSessionAndExtract(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            string cleanToken = token.StartsWith("Bearer ") ? token.Substring(7) : token;

            try
            {
                Console.WriteLine($"[TranslationService] Validating token...");

                // ✅ FIXED: Use exact same key and expiration as your appsettings.json
                var jwtService = new JwtService("K9mP2qL8j5vX4rY7n6zB3wT8sF1dG4hJ7kL0pQ3uR6tY9eW2iO5aS8fH1jK4mN7vC2xZ5bV8nM1qP4rT7yU0wE3", 80000);
                var principal = jwtService.ValidateToken(cleanToken);

                if (principal == null)
                {
                    Console.WriteLine($"[TranslationService] JWT validation failed");
                    return null;
                }

                var userIdClaim = principal.FindFirst("id")?.Value;
                // ✅ FIXED: Use the correct email claim name (same as AuthService)
                var emailClaim = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

                Console.WriteLine($"[TranslationService] Claims found - UserId: {userIdClaim}, Email: {emailClaim}");

                if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(emailClaim))
                {
                    Console.WriteLine($"[TranslationService] Missing claims in token");
                    return null;
                }

                Console.WriteLine($"[TranslationService] Validation successful for user: {emailClaim}");

                var user = new User();
                user.SetId(Guid.Parse(userIdClaim));
                user.SetEmail(emailClaim);
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TranslationService] Validation error: {ex.Message}");
                return null;
            }
        }

        public static User? OptionalValidateSession(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            return ValidateSessionAndExtract(token);
        }

        // ✅ KEEP ALL YOUR EXISTING METHODS UNCHANGED
        public void SaveTextTranslation(Guid userId, string fromLang, string toLang, string original, string translated, string type)
        {
            _db.SaveTextTranslation(userId, fromLang, toLang, original, translated, type);
        }

        public void SaveVoiceTranslation(Guid userId, string fromLang, string toLang, string original, string translated, string type)
        {
            _db.SaveVoiceTranslation(userId, fromLang, toLang, original, translated, type);
        }

        // Fix your TranslationService GetTextTranslations method - use correct column names
        // Fix your TranslationService GetTextTranslations method to return lowercase field names
        public List<object> GetTextTranslations(Guid userId)
        {
            Console.WriteLine($"[TranslationService] 📖 Getting text translations for user: {userId}");

            SqlDataReader reader = _db.GetTextTranslations(userId);
            List<object> list = new List<object>();

            try
            {
                while (reader.Read())
                {
                    Console.WriteLine($"[TranslationService] 📖 Reading translation row...");

                    list.Add(new
                    {
                        id = reader["Id"],                          // ✅ FIXED: lowercase 'id'
                        from_lang = reader["from_lang"],            // ✅ FIXED: snake_case
                        to_lang = reader["to_lang"],                // ✅ FIXED: snake_case  
                        original_text = reader["original_text"],    // ✅ FIXED: snake_case
                        translated_text = reader["translated_text"], // ✅ FIXED: snake_case
                        type = reader["type"],                      // ✅ FIXED: lowercase
                        created_at = reader["created_at"]           // ✅ FIXED: snake_case
                    });
                }

                Console.WriteLine($"[TranslationService] 📖 Successfully read {list.Count} translations");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TranslationService] 📖 Error reading translations: {ex.Message}");
                throw;
            }
            finally
            {
                reader.Close();
            }

            return list;
        }

        // Also fix your GetVoiceTranslations method the same way
        public List<object> GetVoiceTranslations(Guid userId)
        {
            Console.WriteLine($"[TranslationService] 🎙️ Getting voice translations for user: {userId}");

            SqlDataReader reader = _db.GetVoiceTranslations(userId);
            List<object> list = new List<object>();

            try
            {
                while (reader.Read())
                {
                    Console.WriteLine($"[TranslationService] 🎙️ Reading voice translation row...");

                    list.Add(new
                    {
                        id = reader["Id"],                          // ✅ FIXED: lowercase 'id'
                        from_lang = reader["from_lang"],            // ✅ FIXED: snake_case
                        to_lang = reader["to_lang"],                // ✅ FIXED: snake_case
                        original_text = reader["original_text"],    // ✅ FIXED: snake_case
                        translated_text = reader["translated_text"], // ✅ FIXED: snake_case
                        type = reader["type"],                      // ✅ FIXED: lowercase
                        created_at = reader["created_at"]           // ✅ FIXED: snake_case
                    });
                }

                Console.WriteLine($"[TranslationService] 🎙️ Successfully read {list.Count} voice translations");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TranslationService] 🎙️ Error reading voice translations: {ex.Message}");
                throw;
            }
            finally
            {
                reader.Close();
            }

            return list;
        }

        public void DeleteTranslation(Guid userId, Guid translationId)
        {
            _db.DeleteTranslation(userId, translationId);
        }

        public void ClearTranslations(Guid userId)
        {
            _db.ClearTranslations(userId);
        }

        public List<object> GetStatistics(Guid userId)
        {
            SqlDataReader reader = _db.GetLanguageStatistics(userId);
            List<object> list = new List<object>();

            while (reader.Read())
            {
                list.Add(new
                {
                    FromLang = reader["FromLang"],
                    ToLang = reader["ToLang"],
                    Count = reader["Count"]
                });
            }
            reader.Close();
            return list;
        }

        public List<object> GetAuditLogs(Guid userId)
        {
            SqlDataReader reader = _db.GetAuditLogs(userId);
            List<object> list = new List<object>();

            while (reader.Read())
            {
                list.Add(new
                {
                    Action = reader["Action"],
                    Timestamp = reader["Timestamp"],
                    Details = reader["Details"]
                });
            }
            reader.Close();
            return list;
        }

        public static List<object> SearchLanguages(string query)
        {
            List<object> langs = new List<object>
            {
                new { code = "af", name = "Afrikaans" }, new { code = "ar", name = "Arabic" },
                new { code = "hy", name = "Armenian" }, new { code = "az", name = "Azerbaijani" },
                new { code = "be", name = "Belarusian" }, new { code = "bs", name = "Bosnian" },
                new { code = "bg", name = "Bulgarian" }, new { code = "ca", name = "Catalan" },
                new { code = "zh", name = "Chinese" }, new { code = "hr", name = "Croatian" },
                new { code = "cs", name = "Czech" }, new { code = "da", name = "Danish" },
                new { code = "nl", name = "Dutch" }, new { code = "en", name = "English" },
                new { code = "et", name = "Estonian" }, new { code = "fi", name = "Finnish" },
                new { code = "fr", name = "French" }, new { code = "gl", name = "Galician" },
                new { code = "de", name = "German" }, new { code = "el", name = "Greek" },
                new { code = "he", name = "Hebrew" }, new { code = "hi", name = "Hindi" },
                new { code = "hu", name = "Hungarian" }, new { code = "is", name = "Icelandic" },
                new { code = "id", name = "Indonesian" }, new { code = "it", name = "Italian" },
                new { code = "ja", name = "Japanese" }, new { code = "kn", name = "Kannada" },
                new { code = "kk", name = "Kazakh" }, new { code = "ko", name = "Korean" },
                new { code = "lv", name = "Latvian" }, new { code = "lt", name = "Lithuanian" },
                new { code = "mk", name = "Macedonian" }, new { code = "ms", name = "Malay" },
                new { code = "mr", name = "Marathi" }, new { code = "mi", name = "Maori" },
                new { code = "ne", name = "Nepali" }, new { code = "no", name = "Norwegian" },
                new { code = "fa", name = "Persian" }, new { code = "pl", name = "Polish" },
                new { code = "pt", name = "Portuguese" }, new { code = "ro", name = "Romanian" },
                new { code = "ru", name = "Russian" }, new { code = "sr", name = "Serbian" },
                new { code = "sk", name = "Slovak" }, new { code = "sl", name = "Slovenian" },
                new { code = "es", name = "Spanish" }, new { code = "sw", name = "Swahili" },
                new { code = "sv", name = "Swedish" }, new { code = "tl", name = "Tagalog" },
                new { code = "ta", name = "Tamil" }, new { code = "th", name = "Thai" },
                new { code = "tr", name = "Turkish" }, new { code = "uk", name = "Ukrainian" },
                new { code = "ur", name = "Urdu" }, new { code = "vi", name = "Vietnamese" },
                new { code = "cy", name = "Welsh" }
            };

            if (string.IsNullOrWhiteSpace(query) || query == "*")
                return langs;

            string q = query.ToLower();
            return langs.FindAll(lang =>
                lang.GetType().GetProperty("name")?.GetValue(lang)?.ToString()?.ToLower().Contains(q) == true ||
                lang.GetType().GetProperty("code")?.GetValue(lang)?.ToString()?.ToLower().Contains(q) == true);
        }
    }
}