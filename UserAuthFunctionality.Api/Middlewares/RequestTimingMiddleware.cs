//using System.Diagnostics;

//namespace UserAuthFunctionality.Api.Middlewares
//{
//    public class RequestTimingMiddleware
//    {
//        private readonly RequestDelegate _next;
//        private readonly ILogger<RequestTimingMiddleware> _logger;

//        public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
//        {
//            _next = next;
//            _logger = logger;
//        }

//        public async Task Invoke(HttpContext context)
//        {
//            var stopwatch = Stopwatch.StartNew(); // Start timing

//            await _next(context); // Call the next middleware (execute the request)

//            stopwatch.Stop(); // Stop timing

//            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

//            _logger.LogInformation($"[{context.Request.Method}] {context.Request.Path} took {elapsedMilliseconds}ms");

//            // Optionally, add the execution time to the response header
//            context.Response.Headers["X-Execution-Time-ms"] = elapsedMilliseconds.ToString();
//        }
//    }
//}
