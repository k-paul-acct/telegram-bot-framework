using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace MinimalTelegramBot.Results.Extensions;

public class CallbackAnswerResult : IResult
{
    public async Task ExecuteAsync(BotRequestContext context)
    {
        if (context.Update.Type == UpdateType.CallbackQuery && !context.Data.ContainsKey("__CallbackAnswered"))
        {
            context.Data["__CallbackAnswered"] = true;
            await context.Client.AnswerCallbackQueryAsync(context.Update.CallbackQuery!.Id);
        }
    }
}