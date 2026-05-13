using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace eCommerce.SharedLibrary.DependencyInjection
{
    public static class JWTAuthenticationScheme
    {
        // Adds JWT authentication configuration to the application.
        // Uses IConfiguration to read values from appsettings.json
        // and IServiceCollection to register authentication services
        // into Dependency Injection.
        public static IServiceCollection AddJWTAuthenticationScheme(
            this IServiceCollection services,
            IConfiguration config)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var key = Encoding.UTF8.GetBytes(
                        config["Authentication:Key"]!);

                    var issuer = config["Authentication:Issuer"];
                    var audience = config["Authentication:Audience"];

                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = false, // // Can enable later when implementing refresh tokens
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = issuer,
                        ValidAudience = audience,

                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });

            return services;
        }
    }
}