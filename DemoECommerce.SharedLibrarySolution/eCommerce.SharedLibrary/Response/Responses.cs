using System;
using System.Collections.Generic;
using System.Text;

namespace eCommerce.SharedLibrary.Responses
{
    // Used in all services to return a response with a success status, message, and optional data.
    public record Response(bool Success = false, string Message = "");

}
