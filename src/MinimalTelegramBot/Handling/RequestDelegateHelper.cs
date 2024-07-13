using IResult = MinimalTelegramBot.Results.IResult;

namespace MinimalTelegramBot.Handling;

internal static class RequestDelegateHelper
{
    public static Func<BotRequestContext, Task<IResult>> VoidDelegateWrapper(Action<BotRequestContext> delegateInvocation)
    {
        return context =>
        {
            delegateInvocation(context);
            return Task.FromResult(Results.Results.Empty);
        };
    }

    public static Func<BotRequestContext, Task<IResult>> ResultDelegateWrapper(
        Func<BotRequestContext, IResult> delegateInvocation)
    {
        return context => Task.FromResult(delegateInvocation(context));
    }

    public static Func<BotRequestContext, Task<IResult>> GenericDelegateWrapper<T>(
        Func<BotRequestContext, T> delegateInvocation, Func<T, IResult> resultHandler)
    {
        return context =>
        {
            var invocationResult = delegateInvocation(context);
            return Task.FromResult(resultHandler(invocationResult));
        };
    }

    public static Func<BotRequestContext, Task<IResult>> TaskDelegateWrapper(
        Func<BotRequestContext, Task> delegateInvocation)
    {
        return async context =>
        {
            await delegateInvocation(context);
            return Results.Results.Empty;
        };
    }

    public static Func<BotRequestContext, Task<IResult>> GenericTaskDelegateWrapper<T>(
        Func<BotRequestContext, Task<T>> delegateInvocation, Func<T, IResult> resultHandler)
    {
        return async context =>
        {
            var invocationResult = await delegateInvocation(context);
            return resultHandler(invocationResult);
        };
    }

    public static Func<BotRequestContext, Task<IResult>> ValueTaskDelegateWrapper(Func<BotRequestContext,
        ValueTask> delegateInvocation)
    {
        return async context =>
        {
            await delegateInvocation(context);
            return Results.Results.Empty;
        };
    }

    public static Func<BotRequestContext, Task<IResult>> ResultValueTaskDelegateWrapper(Func<BotRequestContext,
        ValueTask<IResult>> delegateInvocation)
    {
        return async context => await delegateInvocation(context);
    }

    public static Func<BotRequestContext, Task<IResult>> GenericValueTaskDelegateWrapper<T>(Func<BotRequestContext,
        ValueTask<T>> delegateInvocation, Func<T, IResult> resultHandler)
    {
        return async context =>
        {
            var invocationResult = await delegateInvocation(context);
            return resultHandler(invocationResult);
        };
    }
}