using System.Runtime.CompilerServices;
using Telegram.Bot.Types.ReplyMarkups;

namespace MinimalTelegramBot.Results;

internal sealed class ResultHelper
{
    private static readonly Type MessageInlineKeyboardPair = typeof(ValueTuple<string, InlineKeyboardMarkup>);
    private static readonly Type MessageReplyKeyboardPair = typeof(ValueTuple<string, ReplyKeyboardMarkup>);

    public static Func<T, IResult> FromType<T>()
    {
        if (typeof(T) == typeof(string))
        {
            return x => Results.Message((x as string)!);
        }

        if (typeof(T) == MessageReplyKeyboardPair || typeof(T) == MessageInlineKeyboardPair)
        {
            return x =>
            {
                var t = (ITuple)x!;
                return Results.Message((string)t[0]!, (IReplyMarkup)t[1]!);
            };
        }

        return x =>
        {
            var message = x?.ToString();
            return message is null ? Results.Empty : Results.Message(message);
        };
    }
}