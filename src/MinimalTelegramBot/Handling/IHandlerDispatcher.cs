namespace MinimalTelegramBot.Handling;

public interface IHandlerDispatcher
{
    IServiceProvider ServiceProvider { get; }
    ICollection<HandlerSource> HandlerSources { get; }
    ICollection<Func<BotRequestContext, Func<BotRequestContext, ValueTask<bool>>>> FilterFactories { get; }
}

public abstract class HandlerSource
{
    public abstract IReadOnlyCollection<Handler> Handlers { get; }
}