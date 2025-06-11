using Microsoft.AspNetCore.Mvc;
using SQL_SERVER.Services;
using System;
using System.Threading.Tasks;

namespace SQL_SERVER.Controllers
{
    [ApiController]
    [Route("api/tools")]
    public class RecognizeAslController : ControllerBase
    {
        private readonly OpenAIHelper _openAi;

        public RecognizeAslController(OpenAIHelper openAi)
        {
            _openAi = openAi ?? throw new ArgumentNullException(nameof(openAi));
        }

        [HttpPost("recognize-asl")]
        public async Task<IActionResult> RecognizeASL([FromHeader(Name = "Authorization")] string token, [FromBody] dynamic body)
        {
            var user = TranslationService.ValidateSessionAndExtract(token);
            if (user == null)
                return Unauthorized(new { error = "Invalid session." });

            if (body == null || body.imageBase64 == null)
                return BadRequest(new { error = "Image data is required." });

            try
            {
                // Add ASL recognition functionality to OpenAIHelper
                string result = await RecognizeASLFromImage(body.imageBase64);
                return Ok(new { text = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to recognize ASL gesture.", details = ex.Message });
            }
        }

        private async Task<string> RecognizeASLFromImage(string imageBase64)
        {
            // This is a placeholder for actual ASL recognition
            // In your OpenAIHelper, add a method similar to this:

            /* 
            public async Task<string> RecognizeASLFromImage(string base64Image)
            {
                var payload = new
                {
                    model = "gpt-4o",
                    messages = new object[]
                    {
                        new { role = "user", content = new object[]
                            {
                                new { type = "text", text = "Interpret the American Sign Language (ASL) gesture in this image and provide the corresponding English word or phrase." },
                                new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{base64Image}" } }
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
            */

            // For now, use the OpenAI helper's existing image recognition method
            return await _openAi.RecognizeTextFromImage(imageBase64);
        }
    }
}