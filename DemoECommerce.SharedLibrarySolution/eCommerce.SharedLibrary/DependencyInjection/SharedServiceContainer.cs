using eCommerce.SharedLibrary.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace eCommerce.SharedLibrary.DependencyInjection
{
   
    public static class SharedServiceContainer
    {
        // This class is responsible for registering shared services across the application.
        // I added file name parameter to allow for configuration from different files if needed.
        public static IServiceCollection AddSharedServices<TContext>(this IServiceCollection services,
            IConfiguration configuration, string fileName) where TContext : DbContext
        {
            services.AddDbContext<TContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("eCommerceConnection"),
                sqlserverOption => sqlserverOption.EnableRetryOnFailure()) // Enable retry on failure for transient faults
                );

            // Configure Serilog logging. Used AI to generate this configuration, so I can focus more on API development.
            // This configuration writes logs to Debug, Console, and rolling files with a specific output template.
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Debug()
                .WriteTo.Console()
                .WriteTo.File(
                    path: $"{fileName}-.txt",
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // Add JWT authentication scheme. This is a basic setup and
            // can be further customized based on the application's requirements.
            JWTAuthenticationScheme.AddJWTAuthenticationScheme(services, configuration);
            return services;
        }

        // This method is responsible for applying shared policies across the application.
        public static IApplicationBuilder UsedSharedPolicies (this IApplicationBuilder app)
        {
            // Use Global exception handling middleware.
            // This middleware will catch unhandled exceptions and return a standardized error response.
            app.UseMiddleware<GlobalException>();

            // Register middleware to block all outsiders API calls
            app.UseMiddleware<AllowApiGatewayOnly>();

            return app;
        }

    }

}
