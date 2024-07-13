namespace MinimalTelegramBot.Handling;

public class HandlerBuilder : IHandlerBuilder
{
    private readonly ICollection<Action<IHandlerBuilder>> _conventions;

    public HandlerBuilder(ICollection<Action<IHandlerBuilder>> conventions)
    {
        _conventions = conventions;
    }

    void IHandlerBuilder.AddConvention(Action<IHandlerBuilder> convention)
    {
        _conventions.Add(convention);
    }
}