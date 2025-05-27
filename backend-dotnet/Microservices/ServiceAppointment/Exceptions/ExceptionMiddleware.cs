using System.Net;

namespace ServiceAppointment.API.Exceptions
{
    public class ExceptionMiddleware
    {   
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);  // המשך בביצוע הבקשה
            }
            catch (Exception ex)
            {
                // לוג של השגיאה
                _logger.LogError(ex, "Unhandled exception occurred");

                // הגדרת סטטוס שגיאה 500
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // שלח תשובה אחידה
                await httpContext.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred. Please try again later." });
            }
        }
    }
}
