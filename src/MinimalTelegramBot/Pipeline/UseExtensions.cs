namespace MinimalTelegramBot.Pipeline;

public static class UseExtensions
{
    public static IBotApplicationBuilder UseCallbackAutoAnswering(this IBotApplicationBuilder app)
    {
        app.Properties["__CallbackAutoAnsweringAdded"] = true;
        return app;
    }

    public static IBotApplicationBuilder Use(this IBotApplicationBuilder app, Func<BotRequestContext, Func<Task>, Task> pipe)
    {
        return app.Use(next => context => pipe(context, () => next(context)));
    }

    public static IBotApplicationBuilder Use(this IBotApplicationBuilder app, Func<BotRequestContext, Func<BotRequestContext, Task>, Task> pipe)
    {
        return app.Use(next => context => pipe(context, next));
    }
}