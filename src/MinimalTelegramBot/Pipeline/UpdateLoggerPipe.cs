using System.Diagnostics;

namespace MinimalTelegramBot.Pipeline;

public class UpdateLoggerPipe : IPipe
{
    private readonly ILogger<BotApplication> _logger;

    public UpdateLoggerPipe(ILogger<BotApplication> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(BotRequestContext ctx, Func<BotRequestContext, Task> next)
    {
        _logger.LogInformation(1, "Received update with ID = {UpdateId}", ctx.Update.Id);

        var stopWatch = new Stopwatch();

        stopWatch.Start();

        await next(ctx);

        stopWatch.Stop();

        if (ctx.Data.TryGetValue(PipelineBuilder.RequestUnhandledKey, out var unhandled) && (bool)unhandled!)
        {
            _logger.LogInformation(2, "Update with ID = {UpdateId} is not handled", ctx.Update.Id);
        }
        else
        {
            _logger.LogInformation(3, "Update with ID = {UpdateId} is handled. Duration {Milliseconds} ms",
                ctx.Update.Id, stopWatch.ElapsedMilliseconds);
        }
    }
}