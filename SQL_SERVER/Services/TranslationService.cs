using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using SQL_SERVER.Models;
using TranslationHubServer;

namespace SQL_SERVER.Services
{
    public class TranslationService
    {
        private static readonly DBHandler _db = new DBHandler();

        public static User? ValidateSessionAndExtract(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            string cleanToken = token.StartsWith("Bearer ") ? token.Substring(7) : token;

            User? user = _db.ValidateSession(cleanToken);
            return user;
        }


        public static User? OptionalValidateSession(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            string cleanToken = token.StartsWith("Bearer ") ? token.Substring(7) : token;

            return _db.ValidateSession(cleanToken);  
        }


        public void SaveTextTranslation(Guid userId, string fromLang, string toLang, string original, string translated, string type)
        {
            _db.SaveTextTranslation(userId, fromLang, toLang, original, translated, type);
        }

        public List<object> GetTextTranslations(Guid userId)
        {
            SqlDataReader reader = _db.GetTextTranslations(userId);
            List<object> list = new List<object>();

            while (reader.Read())
            {
                list.Add(new
                {
                    Id = reader["Id"],
                    FromLang = reader["FromLang"],
                    ToLang = reader["ToLang"],
                    OriginalText = reader["OriginalText"],
                    TranslatedText = reader["TranslatedText"],
                    Type = reader["Type"],
                    CreatedAt = reader["CreatedAt"]
                });
            }
            reader.Close();
            return list;
        }

        public void SaveVoiceTranslation(Guid userId, string fromLang, string toLang, string original, string translated, string type)
        {
            _db.SaveVoiceTranslation(userId, fromLang, toLang, original, translated, type);
        }

        public List<object> GetVoiceTranslations(Guid userId)
        {
            SqlDataReader reader = _db.GetVoiceTranslations(userId);
            List<object> list = new List<object>();

            while (reader.Read())
            {
                list.Add(new
                {
                    Id = reader["Id"],
                    FromLang = reader["FromLang"],
                    ToLang = reader["ToLang"],
                    OriginalText = reader["OriginalText"],
                    TranslatedText = reader["TranslatedText"],
                    Type = reader["Type"],
                    CreatedAt = reader["CreatedAt"]
                });
            }
            reader.Close();
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

            string q = query.ToLower();
            return langs.FindAll(lang =>
                lang.GetType().GetProperty("name").GetValue(lang).ToString().ToLower().Contains(q) ||
                lang.GetType().GetProperty("code").GetValue(lang).ToString().ToLower().Contains(q));
        }

    }
}
