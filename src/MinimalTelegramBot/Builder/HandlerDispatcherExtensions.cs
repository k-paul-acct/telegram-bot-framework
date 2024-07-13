using MinimalTelegramBot.Handling;

namespace MinimalTelegramBot.Builder;

public static class HandlerDispatcherExtensions
{
    public static HandlerGroupBuilder HandleGroup(this IHandlerDispatcher handlerDispatcher)
    {
        throw new NotImplementedException();
    }

    public static HandlerBuilder Handle(this IHandlerDispatcher handlerDispatcher, Delegate handler)
    {
        throw new NotImplementedException();
    }

    public static HandlerBuilder Handle(this IHandlerDispatcher handlerDispatcher, Func<BotRequestContext, Task> handler)
    {
        throw new NotImplementedException();
    }
}