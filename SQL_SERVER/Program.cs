using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SQL_SERVER.Helpers;
using SQL_SERVER.Services;
using TranslationHubServer;

namespace SQL_SERVER
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;

            // Controllers
            builder.Services.AddControllers()
                .AddJsonOptions(options => {
                    // Configure JSON serialization options to match Node.js behavior
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });

            // ✅ CORS Setup
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // Dependencies
            builder.Services.AddScoped<DBHandler>();
            builder.Services.AddScoped<JwtService>(provider =>
            {
                var jwtConfig = configuration.GetSection("Jwt");
                string key = jwtConfig["Key"] ?? throw new InvalidOperationException("JWT Key not found");
                int expirationSeconds = int.Parse(jwtConfig["ExpirationSeconds"] ?? throw new InvalidOperationException("JWT ExpirationSeconds not found"));
                return new JwtService(key, expirationSeconds);
            });
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<TranslationService>();

            // OpenAIHelper – Singleton with injected apiKey
            builder.Services.AddSingleton<OpenAIHelper>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var apiKey = config["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API Key not found");
                return new OpenAIHelper(apiKey);
            });

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // No HTTPS redirection to match Node.js server behavior
            // app.UseHttpsRedirection(); 

            // ✅ Enable CORS Policy
            app.UseCors("AllowAll");

            app.UseAuthorization();

            // Map controllers with professional API structure
            app.MapControllers();

            // Auth routes (keeping existing patterns for compatibility)
            app.MapControllerRoute(
                name: "register",
                pattern: "register",
                defaults: new { controller = "Auth", action = "Register" });

            app.MapControllerRoute(
                name: "login",
                pattern: "login",
                defaults: new { controller = "Auth", action = "Login" });

            app.MapControllerRoute(
                name: "logout",
                pattern: "logout",
                defaults: new { controller = "Auth", action = "Logout" });

            // ✅ FIXED: Session validation route - correctly map to SessionController
            // RootLayout calls ApiService.get('/validate-session', token)
            // This should hit SessionController.ValidateSession, NOT AuthController
            app.MapGet("/validate-session", async (HttpContext context, AuthService authService) =>
            {
                Console.WriteLine("[Custom Route] /validate-session called");

                // Get Authorization header
                if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    Console.WriteLine("[Custom Route] No Authorization header");
                    return Results.BadRequest(new { error = "Token is required." });
                }

                string? token = authHeader.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(token))
                {
                    Console.WriteLine("[Custom Route] Empty Authorization header");
                    return Results.BadRequest(new { error = "Token is required." });
                }

                try
                {
                    // Clean the token (remove "Bearer " prefix if present)
                    string cleanToken = token.StartsWith("Bearer ") ? token.Substring(7) : token;
                    Console.WriteLine($"[Custom Route] Validating token: {cleanToken?.Substring(0, Math.Min(10, cleanToken.Length))}...");

                    var user = authService.ValidateSession(cleanToken);
                    if (user == null)
                    {
                        Console.WriteLine("[Custom Route] Session validation failed - user is null");
                        return Results.Unauthorized();
                    }

                    Console.WriteLine("[Custom Route] Session validation successful");
                    return Results.Ok(new { success = true, user });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Custom Route] Session validation error: {ex.Message}");
                    return Results.Problem(ex.Message);
                }
            });

            // Preferences route
            app.MapControllerRoute(
                name: "preferences",
                pattern: "api/preferences",
                defaults: new { controller = "Preferences", action = "UpdatePreferences" });

            // Tools routes - Professional API structure
            app.MapControllerRoute(
                name: "translate",
                pattern: "api/tools/translate",
                defaults: new { controller = "Tools", action = "Translate" });

            app.MapControllerRoute(
                name: "recognize-text",
                pattern: "api/tools/recognize-text",
                defaults: new { controller = "Tools", action = "RecognizeText" });

            app.MapControllerRoute(
                name: "speech-to-text",
                pattern: "api/tools/speech-to-text",
                defaults: new { controller = "Tools", action = "SpeechToText" });

            app.MapControllerRoute(
                name: "text-to-speech",
                pattern: "api/tools/text-to-speech",
                defaults: new { controller = "Tools", action = "TextToSpeech" });

            app.MapControllerRoute(
                name: "recognize-asl",
                pattern: "api/tools/recognize-asl",
                defaults: new { controller = "RecognizeAsl", action = "RecognizeASL" });

            app.MapControllerRoute(
                name: "extract-text",
                pattern: "api/tools/extract-text",
                defaults: new { controller = "ExtractText", action = "Extract" });

            app.MapControllerRoute(
                name: "generate-docx",
                pattern: "api/tools/generate-docx",
                defaults: new { controller = "GenerateDocx", action = "GenerateDocx" });

            // Translation routes - Professional API structure
            app.MapControllerRoute(
                name: "translations-text-post",
                pattern: "api/translations/text",
                defaults: new { controller = "Translation", action = "SaveTextTranslation" });

            app.MapControllerRoute(
                name: "translations-text-get",
                pattern: "api/translations/text",
                defaults: new { controller = "Translation", action = "GetTextTranslations" });

            app.MapControllerRoute(
                name: "translations-voice-post",
                pattern: "api/translations/voice",
                defaults: new { controller = "Translation", action = "SaveVoiceTranslation" });

            app.MapControllerRoute(
                name: "translations-voice-get",
                pattern: "api/translations/voice",
                defaults: new { controller = "Translation", action = "GetVoiceTranslations" });

            app.MapControllerRoute(
                name: "translations-delete",
                pattern: "api/translations/delete/{id}",
                defaults: new { controller = "Translation", action = "DeleteTranslation" });

            app.MapControllerRoute(
                name: "translations-clear",
                pattern: "api/translations",
                defaults: new { controller = "Translation", action = "ClearAllTranslations" });

            app.MapControllerRoute(
                name: "statistics",
                pattern: "api/statistics",
                defaults: new { controller = "Translation", action = "GetStatistics" });

            app.MapControllerRoute(
                name: "audit-logs",
                pattern: "api/audit-logs",
                defaults: new { controller = "Translation", action = "GetAuditLogs" });

            // Languages route (already correct)
            app.MapControllerRoute(
                name: "languages",
                pattern: "languages",
                defaults: new { controller = "Languages", action = "SearchLanguages" });

            app.Run();
        }
    }
}