using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            builder.Services.AddControllers();

            //  Dependencies
            builder.Services.AddScoped<DBHandler>();
            builder.Services.AddScoped<JwtService>(provider =>
            {
                var jwtConfig = configuration.GetSection("Jwt");
                string key = jwtConfig["Key"];
                int expirationSeconds = int.Parse(jwtConfig["ExpirationSeconds"]);
                return new JwtService(key, expirationSeconds); 
            });
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<TranslationService>();

            //  OpenAIHelper – Singleton with injected apiKey
            builder.Services.AddSingleton<OpenAIHelper>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var apiKey = config["OpenAI:ApiKey"];
                return new OpenAIHelper(apiKey);
            });

            //  Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            //  Middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
