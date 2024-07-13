namespace MinimalTelegramBot.Handling;

public interface IHandlerBuilder
{
    void AddConvention(Action<IHandlerBuilder> convention);
}