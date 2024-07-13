using IResult = MinimalTelegramBot.Results.IResult;

namespace MinimalTelegramBot.Handling;

public class Handler
{
    public readonly Dictionary<object, object?> Metadata;

    // TODO:
    // private readonly List<Func<BotRequestContext, bool>> _filterDelegates = [];

    public readonly Func<BotRequestContext, Task<IResult>> HandlerDelegate;

    public Handler(Func<BotRequestContext, Task<IResult>> handlerDelegate, Dictionary<object, object?> metadata)
    {
        HandlerDelegate = handlerDelegate;
        Metadata = metadata;
    }

    public async Task Handle(BotRequestContext context)
    {
        var result = await HandlerDelegate(context);
        await result.ExecuteAsync(context);
    }
}