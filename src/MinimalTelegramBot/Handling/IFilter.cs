namespace MinimalTelegramBot.Handling;

public interface IFilter
{
    ValueTask<bool> Filter(BotRequestContext context);
}