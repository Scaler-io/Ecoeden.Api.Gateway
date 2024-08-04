
using Ecoeden.Api.Gateway.Services.Subscription;

namespace Ecoeden.Api.Gateway.Middlewares;

public class SubscriptionValidationMiddlewar(ILogger logger, ISubscriptionValidationService validator) : IMiddleware
{
    private readonly ILogger _logger = logger;
    private readonly ISubscriptionValidationService _validator = validator;
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var subscriptionKey = context.Request.Headers["ocp-apim-subscriptionkey"].FirstOrDefault();
        var apiPath = GetApiPath(context.Request.Path.Value);

        if(apiPath!="*" && subscriptionKey == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(HandleInvalidResponse(StatusCodes.Status401Unauthorized));
            return;
        }

        if (apiPath != "*" && !await _validator.CheckSubscriptionValidity(subscriptionKey, apiPath))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(HandleInvalidResponse(StatusCodes.Status403Forbidden));
            return;
        }

        await next(context);
    }

    private string GetApiPath(string requestPath)
    {
        if (requestPath == "/") return "*";
        var segments = requestPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length > 0 ? "/" + segments[0] : string.Empty;
    }

    private object HandleInvalidResponse(int status) => status switch
    {
        StatusCodes.Status401Unauthorized => new
        {
            ErrorCode = StatusCodes.Status403Forbidden,
            Message = "Forbidden: No subscription provided"
        },
        StatusCodes.Status403Forbidden => new
        {
            ErrorCode = StatusCodes.Status403Forbidden,
            Message = $"Forbidden: Invalid subscription key provided"
        },
        _ => throw new NotImplementedException()
    };
}
