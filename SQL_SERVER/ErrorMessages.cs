using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationHubServer.Helpers
{
    public static class ErrorMessages
    {
        public const string EMAIL_PASSWORD_REQUIRED = "Email and password are required.";
        public const string INVALID_CREDENTIALS = "Invalid email or password.";
        public const string FAILED_TO_REGISTER = "Failed to register user.";
        public const string FAILED_TO_LOGIN = "Failed to login.";
        public const string TOKEN_REQUIRED = "Authorization token is required.";
        public const string INVALID_SESSION = "Session is invalid or expired.";
        public const string INVALID_TOKEN = "Invalid or expired token.";
        public const string FAILED_TO_LOGOUT = "Failed to logout.";
        public const string FAILED_TO_UPDATE_PREFERENCES = "Failed to update preferences.";
        public const string TEXT_TARGETLANG_REQUIRED = "Text and target language are required.";
        public const string IMAGE_DATA_REQUIRED = "Image data is required.";
        public const string AUDIO_FILE_REQUIRED = "Audio file is required.";
        public const string INVALID_FILE_PATH = "Invalid file path.";
        public const string INVALID_AUDIO_FORMAT = "Invalid audio format.";
        public const string FAILED_TO_TRANSCRIBE = "Failed to transcribe audio.";
        public const string TEXT_LANGUAGE_REQUIRED = "Text and language are required.";
        public const string FAILED_TO_GENERATE_SPEECH = "Failed to generate speech.";
        public const string FAILED_TO_RECOGNIZE_TEXT = "Failed to recognize text from image.";
        public const string FAILED_TO_RECOGNIZE_ASL = "Failed to recognize ASL gesture.";
        public const string FILE_URI_REQUIRED = "File URI is required.";
        public const string UNSUPPORTED_FILE_TYPE = "Unsupported file type.";
        public const string FAILED_TO_EXTRACT_TEXT = "Failed to extract text.";
        public const string TEXT_REQUIRED = "Text is required.";
        public const string FAILED_TO_GENERATE_DOC = "Failed to generate document.";
        public const string FAILED_TO_SAVE_TEXT_TRANSLATION = "Failed to save text translation.";
        public const string FAILED_TO_FETCH_TEXT_TRANSLATIONS = "Failed to fetch text translations.";
        public const string FAILED_TO_SAVE_VOICE_TRANSLATION = "Failed to save voice translation.";
        public const string FAILED_TO_FETCH_VOICE_TRANSLATIONS = "Failed to fetch voice translations.";
        public const string FAILED_TO_DELETE_TRANSLATION = "Failed to delete translation.";
        public const string FAILED_TO_CLEAR_TRANSLATIONS = "Failed to clear translations.";
        public const string FAILED_TO_FETCH_STATISTICS = "Failed to fetch statistics.";
        public const string FAILED_TO_FETCH_AUDIT_LOGS = "Failed to fetch audit logs.";
        public const string LOGIN_REQUIRED = "Login required to access this resource.";
        public const string QUERY_PARAM_REQUIRED = "Query parameter is required.";
        public const string FAILED_TO_SEARCH_LANGUAGES = "Failed to search languages.";
    }
}

