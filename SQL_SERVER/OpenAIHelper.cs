using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SQL_SERVER.Services
{
    public class OpenAIHelper
    {
        private readonly HttpClient _httpClient;

        public OpenAIHelper(string apiKey)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<string> DetectLanguage(string text)
        {
            var payload = new
            {
                model = "gpt-4o",
                messages = new[]
                {
                    new { role = "system", content = "Detect the primary language of the following text and return only the ISO code (e.g., 'en', 'he'). If unknown, return 'unknown'." },
                    new { role = "user", content = text }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString()
                      .Trim();
        }

        public async Task<string> TranslateText(string sourceLang, string targetLang, string text)
        {
            var payload = new
            {
                model = "gpt-4o",
                messages = new[]
                {
                    new { role = "system", content = $"Translate from {sourceLang} to {targetLang}. Use transliteration if needed." },
                    new { role = "user", content = text }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString()
                      .Trim();
        }

        public async Task<string> RecognizeTextFromImage(string base64Image)
        {
            var payload = new
            {
                model = "gpt-4o",
                messages = new object[]
                {
                    new { role = "system", content = "Extract visible text from this image. Return only raw text." },
                    new {
                        role = "user",
                        content = new object[]
                        {
                            new {
                                type = "image_url",
                                image_url = new { url = $"data:image/jpeg;base64,{base64Image}" }
                            }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString()
                      .Trim();
        }

        public async Task<byte[]> TextToSpeech(string text)
        {
            var payload = new
            {
                model = "tts-1",
                voice = "alloy",
                input = text,
                response_format = "mp3"
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/audio/speech", content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<string> SpeechToText(byte[] audioBytes, string fileName)
        {
            using var form = new MultipartFormDataContent();
            var audioContent = new ByteArrayContent(audioBytes);
            audioContent.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");
            form.Add(audioContent, "file", fileName);
            form.Add(new StringContent("whisper-1"), "model");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/audio/transcriptions", form);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.GetProperty("text").GetString();
        }
    }
}
