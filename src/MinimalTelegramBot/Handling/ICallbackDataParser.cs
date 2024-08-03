namespace MinimalTelegramBot.Handling;

public interface ICallbackDataParser<out TModel>
{
    static abstract TModel Parse(string callbackData);
}