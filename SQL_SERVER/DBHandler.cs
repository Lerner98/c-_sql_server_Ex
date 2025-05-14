using System;
using System.Collections.Generic;
using SQL_SERVER.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TranslationHubServer
{
    public class DBHandler
    {
        private readonly string connectionString;

        public DBHandler()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            connectionString = config.GetConnectionString("DefaultConnection");
        }

        public bool CreateUser(string email, string passwordHash)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("spRegisterUser", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

                conn.Open();
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return reader["Message"].ToString() == "Registration successful";
                }
                return false;
            }
        }

        public SqlDataReader FindUserByEmail(string email)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("spLoginUser", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Email", email);
            conn.Open();
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public void CreateSession(Guid userId, Guid sessionId, DateTime expiresAt, string signedSessionId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("spCreateSession", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@SessionId", sessionId);
                cmd.Parameters.AddWithValue("@ExpiresAt", expiresAt);
                cmd.Parameters.AddWithValue("@SignedSessionId", signedSessionId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public User? ValidateSession(string signedSessionId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("spValidateSession", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SignedSessionId", signedSessionId);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (reader.Read())
                    {
                        User user = new User();
                        user.SetId((Guid)reader["UserId"]);
                        user.SetEmail((string)reader["Email"]);
                        user.SetDefaultFromLang((string)reader["DefaultFromLang"]);
                        user.SetDefaultToLang((string)reader["DefaultToLang"]);
                        return user;
                    }
                }
            }

            return null;
        }




        public void Logout(string signedSessionId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("spLogoutUser", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SignedSessionId", signedSessionId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateUserPreferences(Guid userId, string fromLang, string toLang)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("spUpdateUserPreferences", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@DefaultFromLang", fromLang);
                cmd.Parameters.AddWithValue("@DefaultToLang", toLang);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void SaveTextTranslation(Guid userId, string fromLang, string toLang, string originalText, string translatedText, string type)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("spSaveTextTranslation", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@FromLang", fromLang);
                cmd.Parameters.AddWithValue("@ToLang", toLang);
                cmd.Parameters.AddWithValue("@OriginalText", originalText);
                cmd.Parameters.AddWithValue("@TranslatedText", translatedText);
                cmd.Parameters.AddWithValue("@Type", type);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public SqlDataReader GetTextTranslations(Guid userId)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("spGetUserTextTranslations", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);
            conn.Open();
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public void SaveVoiceTranslation(Guid userId, string fromLang, string toLang, string originalText, string translatedText, string type)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("spSaveVoiceTranslation", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@FromLang", fromLang);
                cmd.Parameters.AddWithValue("@ToLang", toLang);
                cmd.Parameters.AddWithValue("@OriginalText", originalText);
                cmd.Parameters.AddWithValue("@TranslatedText", translatedText);
                cmd.Parameters.AddWithValue("@Type", type);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public SqlDataReader GetVoiceTranslations(Guid userId)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("spGetUserVoiceTranslations", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);
            conn.Open();
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public void ClearTranslations(Guid userId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("spClearTranslations", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteTranslation(Guid userId, Guid translationId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("spDeleteTranslation", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@TranslationId", translationId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateLanguageStatistics(Guid userId, string fromLang, string toLang)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("spUpdateLanguageStatistics", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@FromLang", fromLang);
                cmd.Parameters.AddWithValue("@ToLang", toLang);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public SqlDataReader GetLanguageStatistics(Guid userId)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("spGetLanguageStatistics", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);
            conn.Open();
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public SqlDataReader GetAuditLogs(Guid userId)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("spGetAuditLogs", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);
            conn.Open();
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public User GetUserById(Guid userId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("spGetUserById", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (reader.Read())
                    {
                        User user = new User();
                        user.SetId((Guid)reader["UserId"]);
                        user.SetEmail((string)reader["Email"]);
                        user.SetDefaultFromLang((string)reader["DefaultFromLang"]);
                        user.SetDefaultToLang((string)reader["DefaultToLang"]);
                        return user;
                    }
                }
            }

            return null;
        }
    }
}
