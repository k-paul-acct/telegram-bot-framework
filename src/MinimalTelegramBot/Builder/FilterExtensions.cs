using MinimalTelegramBot.Handling;
using MinimalTelegramBot.StateMachine.Abstractions;
using Telegram.Bot.Types.Enums;

namespace MinimalTelegramBot.Builder;

public static class FilterExtensions
{
    public static Handler FilterText(this Handler handler, Func<string, bool> filter)
    {
        return handler.Filter(ctx => ctx.MessageText is not null && filter(ctx.MessageText));
    }

    public static Handler FilterTextWithLocalizer(this Handler handler, string key)
    {
        return handler.Filter(ctx => ctx.MessageText is not null &&
                                     ctx.Localizer![ctx.UserLocale ?? Locale.Default, key] == ctx.MessageText);
    }

    public static Handler FilterCommand(this Handler handler, string command)
    {
        return handler.Filter(ctx =>
        {
            if (ctx.MessageText is null)
            {
                return false;
            }

            var span = ctx.MessageText.AsSpan();

            if (span.Length < 2 || span[0] != '/')
            {
                return false;
            }

            var commandEnd = 1;

            foreach (var letter in span[1..])
            {
                var uLetter = (uint)letter;

                if (uLetter - 'a' > 'z' - 'a' && uLetter - '0' > '9' - '0' && uLetter != '_')
                {
                    break;
                }

                commandEnd += 1;
            }

            if (commandEnd < 2)
            {
                return false;
            }

            return span[..commandEnd].SequenceEqual(command);
        });
    }

    public static Handler FilterState(this Handler handler, State state)
    {
        return handler.Filter(ctx => ctx.UserState == state);
    }

    public static Handler FilterState(this Handler handler, Func<State, bool> filter)
    {
        return handler.Filter(ctx => ctx.UserState is not null && filter(ctx.UserState));
    }

    public static Handler FilterCallbackData(this Handler handler, Func<string, bool> filter)
    {
        return handler.Filter(ctx => ctx.CallbackData is not null && filter(ctx.CallbackData));
    }

    public static Handler FilterUpdateType(this Handler handler, UpdateType updateType)
    {
        return handler.Filter(ctx => ctx.Update.Type == updateType);
    }

    public static Handler FilterUpdateType(this Handler handler, Func<UpdateType, bool> filter)
    {
        return handler.Filter(ctx => filter(ctx.Update.Type));
    }

    public static TBuilder Filter<TBuilder>(this TBuilder handler, Func<BotRequestContext, ValueTask<bool>> filter)
        where TBuilder : IHandlerBuilder
    {
        return handler.Filter(ctx => ctx.MessageText is not null && filter(ctx.MessageText));
    }

    // TODO:
    public static TBuilder FilterWithFactory<TBuilder>(this TBuilder handler, Func<string, bool> filter)
    {
        return handler.Filter(ctx => ctx.MessageText is not null && filter(ctx.MessageText));
    }
}