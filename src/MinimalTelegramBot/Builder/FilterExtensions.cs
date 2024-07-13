using MinimalTelegramBot.Handling;
using MinimalTelegramBot.StateMachine.Abstractions;
using Telegram.Bot.Types.Enums;

namespace MinimalTelegramBot.Builder;

public static class FilterExtensions
{
    public const string CommandArgs = "__CommandArgs__";
    public const string CallbackDataArgs = "__CallbackDataArgs__";

    public static Handler FilterText(this Handler handler, Func<string, bool> filter)
    {
        return handler.Filter(ctx => ctx.MessageText is not null && filter(ctx.MessageText));
    }

    public static Handler FilterTextWithLocalizer(this Handler handler, string key)
    {
        return handler.Filter(ctx => ctx.Localizer![ctx.UserLocale!, key] == ctx.MessageText);
    }

    public static Handler FilterCommand(this Handler handler, string command)
    {
        return handler.Filter(ctx =>
        {
            if (ctx.MessageText is null)
            {
                return false;
            }

            var parts = ctx.MessageText.Split(' ');
            ctx.Data[CommandArgs] = parts[1..];

            return parts[0] == command;
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
        return handler.Filter(ctx =>
        {
            if (ctx.CallbackData is null)
            {
                return false;
            }

            var parts = ctx.CallbackData.Split(' ');
            ctx.Data[CallbackDataArgs] = parts[1..];

            return filter(ctx.CallbackData);
        });
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