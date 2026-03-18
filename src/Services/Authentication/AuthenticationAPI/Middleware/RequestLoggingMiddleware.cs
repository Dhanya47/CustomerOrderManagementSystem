namespace AuthService.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var traceId = context.TraceIdentifier;
            var method = context.Request.Method;
            var path = context.Request.Path;
            var startTime = DateTime.UtcNow;

            _logger.LogInformation(
                "Request START  | TraceId: {TraceId} | {Method} {Path} | Time: {Time}",
                traceId, method, path, startTime);

            await _next(context);

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.LogInformation(
                "Request END    | TraceId: {TraceId} | {Method} {Path} | Status: {Status} | Duration: {Duration}ms",
                traceId, method, path, context.Response.StatusCode, duration);
        }
    }
}