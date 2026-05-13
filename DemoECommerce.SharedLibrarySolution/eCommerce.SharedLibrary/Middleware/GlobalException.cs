using eCommerce.SharedLibrary.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace eCommerce.SharedLibrary.Middleware
{
    // This class is intended to be used as a global exception handler in the middleware pipeline
    // of an ASP.NET Core application. It catches any unhandled exceptions that occur during the
    // processing of HTTP requests
    public class GlobalException
    {
        private readonly RequestDelegate next;

        public GlobalException(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string message = "Internal server error ocurred. Please try again";
            int statusCode = StatusCodes.Status500InternalServerError;
            string title = "Error";

            try
            {
                await next(context);

                //Check if Response is too many request // 429 status code
                if(context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    message = "Too many requests. Please try again later.";
                    statusCode = StatusCodes.Status429TooManyRequests;
                    title = "Too Many Requests";
                    await ModifyHeader(context, message, statusCode, title);
                }

                //Check if Response is Unauthorized // 401 status code
                if(context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    message = "Unauthorized. Please provide valid credentials.";
                    statusCode = StatusCodes.Status401Unauthorized;
                    title = "Unauthorized";
                    await ModifyHeader(context, message, statusCode, title);
                }

                //Check if Response is Not Found // 404 status code
                if(context.Response.StatusCode == StatusCodes.Status404NotFound)
                {
                    message = "Resource not found. Please check the URL and try again.";
                    statusCode = StatusCodes.Status404NotFound;
                    title = "Not Found";
                    await ModifyHeader(context, message, statusCode, title);
                }

                //check id Reponse is Forbidden // 403 status code
                if(context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    message = "Forbidden. You don't have permission to access this resource.";
                    statusCode = StatusCodes.Status403Forbidden;
                    title = "Forbidden";
                    await ModifyHeader(context, message, statusCode, title);
                }
            }
            catch (Exception ex) 
            {
                // Log Original Exception / Console log, file log, database log, etc.
                LogException.LogExceptions(ex);

                // Check if Exception is TimeoutException
                if(ex is TimeoutException || ex is TaskCanceledException)
                {
                    message = "The request timed out. Please try again later.";
                    statusCode = StatusCodes.Status408RequestTimeout;
                    title = "Request Timeout";
                }

                // None of the above, return Internal Server Error
                await ModifyHeader(context, message, statusCode, title);
            }
        }


        //This method standardizes API error responses so every error returns clean JSON instead
        //of ugly server exceptions.
        private static async Task ModifyHeader(HttpContext context, string message, int statusCode, string title)
        {
            // Display scary-free error message to the user, without exposing sensitive information about
            // the server or the application.
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails(){
                Status = statusCode,
                Title = title,
                Detail = message
            }), CancellationToken.None);
            return;
        }
    }
}
