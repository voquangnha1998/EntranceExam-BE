using EntranceExam.Services.Services;

namespace EntranceExam.Middleware
{
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenBlacklistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IBlacklistTokenService blacklistService)
        {
            var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                await _next(context);
                return;
            }
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                var isBlacklisted = await blacklistService.IsBlacklistedAsync(token);
                if (isBlacklisted)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Token is blacklisted");
                    return;
                }
            }

            await _next(context);
        }
    }
}
