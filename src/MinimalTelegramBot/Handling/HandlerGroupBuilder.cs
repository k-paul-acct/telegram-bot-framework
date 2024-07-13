namespace MinimalTelegramBot.Handling;

public class HandlerGroupBuilder : IHandlerDispatcher, IHandlerBuilder
{
    private readonly IHandlerDispatcher _outerDispatcher;
    private readonly List<HandlerSource> _handlerSources = [];
    private readonly List<Action<IHandlerBuilder>> _conventions = [];
    private readonly List<Func<BotRequestContext, Func<BotRequestContext, ValueTask<bool>>>> _filterFactories = [];

    internal HandlerGroupBuilder(IHandlerDispatcher outerDispatcher)
    {
        _outerDispatcher = outerDispatcher;
        _outerDispatcher.HandlerSources.Add(new HandlerGroupSource(this));
    }

    IServiceProvider IHandlerDispatcher.ServiceProvider => _outerDispatcher.ServiceProvider;
    ICollection<HandlerSource> IHandlerDispatcher.HandlerSources => _outerDispatcher.HandlerSources;
    ICollection<Func<BotRequestContext, Func<BotRequestContext, ValueTask<bool>>>> IHandlerDispatcher.FilterFactories => _filterFactories;

    void IHandlerBuilder.AddConvention(Action<IHandlerBuilder> convention)
    {
        _conventions.Add(convention);
    }

    private class HandlerGroupSource : HandlerSource
    {
        private readonly HandlerGroupBuilder _groupBuilder;

        public HandlerGroupSource(HandlerGroupBuilder groupBuilder)
        {
            _groupBuilder = groupBuilder;
        }

        public override IReadOnlyCollection<Handler> Handlers => GetHandlers();

        private IReadOnlyList<Handler> GetHandlers()
        {
            var handlers = new List<Handler>();
            handlers.AddRange(_groupBuilder._handlerSources);
        }
    }
}