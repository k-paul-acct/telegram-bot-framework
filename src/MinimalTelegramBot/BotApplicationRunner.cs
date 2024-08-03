using Microsoft.AspNetCore.Http.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using JsonSerializerOptionsProvider = Telegram.Bot.Serialization.JsonSerializerOptionsProvider;

namespace MinimalTelegramBot;

internal static class BotApplicationRunner
{
    public static void Run(BotApplication app)
    {
        var appBuilder = (IBotApplicationBuilder)app;

        appBuilder.Build();

        var isWebhook = appBuilder.Properties.ContainsKey("__WebhookEnabled");

        appBuilder.Properties.TryGetValue("__DeleteWebhook", out var deleteWebhook);

        if (deleteWebhook is true)
        {
            DeleteWebhook(app).GetAwaiter().GetResult();
        }

        var host = isWebhook ? SetupWebhooks(app).GetAwaiter().GetResult() : app.Host;

        if (!isWebhook)
        {
            app.StartPolling();
        }

        app.InitBot(isWebhook);

        if (isWebhook)
        {
            ((WebApplication)host).Run();
        }
        else
        {
            host.Run();
        }
    }

    private static async Task DeleteWebhook(BotApplication app)
    {
        await app.Client.DeleteWebhookAsync(app.Options.ReceiverOptions?.DropPendingUpdates ?? false);
    }

    private static async Task<WebApplication> SetupWebhooks(BotApplication app)
    {
        var options = app.Options.WebhookOptions ??
                      throw new Exception("No webhook options was specified to use webhook");

        await app.Client.SetWebhookAsync(options.Url, options.Certificate, options.IpAddress, options.MaxConnections,
            app.Options.ReceiverOptions?.AllowedUpdates, app.Options.ReceiverOptions?.DropPendingUpdates ?? false,
            options.SecretToken);

        var webAppBuilder = WebApplication.CreateSlimBuilder(app.Options.Args ?? []);

        webAppBuilder.Services.Configure<JsonOptions>(o =>
        {
            JsonSerializerOptionsProvider.Configure(o.SerializerOptions);
        });

        webAppBuilder.Services.AddHttpClient("tgwebhook")
            .RemoveAllLoggers()
            .AddTypedClient(httpClient => new TelegramBotClient(app.Options.Token, httpClient));

        var webApp = webAppBuilder.Build();

        webApp.MapPost("/", async (Update update, TelegramBotClient bot) =>
        {
            await app.HandleUpdateInBackground(bot, update);
            return Microsoft.AspNetCore.Http.Results.StatusCode(StatusCodes.Status200OK);
        }).AddEndpointFilter(new TelegramWebHookSecretTokenFilter(options.SecretToken));

        return webApp;
    }
}