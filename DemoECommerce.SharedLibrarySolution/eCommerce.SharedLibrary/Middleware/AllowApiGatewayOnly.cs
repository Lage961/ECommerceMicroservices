using Microsoft.AspNetCore.Http;

namespace eCommerce.SharedLibrary.Middleware
{
    // This middleware is designed to restrict access to certain endpoints,
    // allowing only requests that come through an API Gateway.
    public class AllowApiGatewayOnly
    {
        private readonly RequestDelegate? next;

        public AllowApiGatewayOnly(RequestDelegate _next)
        {
            this.next = _next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Extract specific header from the incoming request, which is expected to be set by the API Gateway.
            var signedHeader = context.Request.Headers["Api-Gateway"];

            // Check if the header is present and has the expected value or if is NULL/Empty
            if(signedHeader.FirstOrDefault() is null || signedHeader.Count == 0)
            {
                // If the header is missing or does not have the expected value, return a 503 Service Unavailable
                // response, indicating that the service is temporarily unavailable.
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync("Service Unavailable: Access is not available. This endpoint can only be accessed through the API Gateway.");
                return; // Stop further processing of the request.
            }
            else
            {
                await next(context); // If the header is present and valid, continue processing the request.
            }

        }
    }
}
