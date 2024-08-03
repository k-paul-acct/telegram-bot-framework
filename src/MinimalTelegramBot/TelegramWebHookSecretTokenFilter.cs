namespace MinimalTelegramBot;

internal sealed class TelegramWebHookSecretTokenFilter : IEndpointFilter
{
    private const string SecretTokenHeaderName = "X-Telegram-Bot-Api-Secret-Token";

    private readonly string? _secretToken;

    public TelegramWebHookSecretTokenFilter(string? secretToken)
    {
        _secretToken = secretToken;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (_secretToken is null)
        {
            return await next(context);
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(SecretTokenHeaderName, out var header))
        {
            return Microsoft.AspNetCore.Http.Results.StatusCode(StatusCodes.Status401Unauthorized);
        }

        if (!header.Equals(_secretToken))
        {
            return Microsoft.AspNetCore.Http.Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        return await next(context);
    }
}