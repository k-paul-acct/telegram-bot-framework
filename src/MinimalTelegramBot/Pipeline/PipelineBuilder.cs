namespace MinimalTelegramBot.Pipeline;

internal sealed class PipelineBuilder : IBotApplicationBuilder
{
    public const string RequestUnhandledKey = "__RequestUnhandled";

    private readonly List<Func<Func<BotRequestContext, Task>, Func<BotRequestContext, Task>>> _pipes = [];

    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();

    public IBotApplicationBuilder Use(Func<Func<BotRequestContext, Task>, Func<BotRequestContext, Task>> pipe)
    {
        _pipes.Add(pipe);
        return this;
    }

    public Func<BotRequestContext, Task> Build()
    {
        Func<BotRequestContext, Task> pipeline = context =>
        {
            if (!context.UpdateHandlingStarted)
            {
                context.Data[RequestUnhandledKey] = true;
            }

            return Task.CompletedTask;
        };

        for (var i = _pipes.Count - 1; i >= 0; --i)
        {
            pipeline = _pipes[i](pipeline);
        }

        return pipeline;
    }
}