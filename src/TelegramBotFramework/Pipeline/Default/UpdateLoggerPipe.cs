namespace TelegramBotFramework.Pipeline.Default;

public class UpdateLoggerPipe : IPipe
{
    private readonly ILogger<BotApplication> _logger;

    public UpdateLoggerPipe(ILogger<BotApplication> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(BotRequestContext ctx, BotRequestDelegate next)
    {
        _logger.LogInformation(1, "Received update with ID = {UpdateId}", ctx.Update.Id);
        await next(ctx);

        if (ctx.Items.TryGetValue(PipelineBuilder.RequestUnhandledKey, out var unhandled) && (bool)unhandled!)
        {
            _logger.LogInformation(2, "Update with ID = {UpdateId} is not handled", ctx.Update.Id);
        }
        else
        {
            _logger.LogInformation(3, "Update with ID = {UpdateId} is handled", ctx.Update.Id);
        }
    }
}