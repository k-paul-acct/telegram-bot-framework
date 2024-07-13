using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using MinimalTelegramBot;
using MinimalTelegramBot.Builder;
using MinimalTelegramBot.Extensions;
using MinimalTelegramBot.Localization.Abstractions;
using MinimalTelegramBot.Localization.Extensions;
using MinimalTelegramBot.Pipeline;
using MinimalTelegramBot.Settings;
using MinimalTelegramBot.StateMachine.Extensions;
using UsageExample.Localization;
using UsageExample.Services;
using Results = MinimalTelegramBot.Results.Results;

// TODO: Group filtering.
// TODO: Generic typed filters.
// TODO: More Result templates.
// TODO: More efficient handler resolving.
// TODO: Different state models.
// TODO: Notifications.
// TODO: Bot info after startup.
// TODO: Different persistence and builder options in builders (Locale Service, State Machine, Notification Service).

var builder = BotApplication.CreateBuilder(new BotApplicationBuilderOptions
{
    Args = args,
    ReceiverOptions = new ReceiverOptions
    {
        AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery,],
        DropPendingUpdates = true,
    },
});

builder.HostBuilder.Services.AddStateMachine();
builder.HostBuilder.Services.AddLocalizer<UserLocaleProvider>(x =>
{
    x.EnrichFromFile("Localization/ru.yaml", new Locale("ru"));
});

builder.HostBuilder.Services.AddScoped<WeatherService>();

builder.SetTokenFromConfiguration("BotToken");

var app = builder.Build();

app.UsePolling();
// app.UseWebhook(new WebhookOptions { Url = "", });

app.UseCallbackAutoAnswering();

var group = app.HandleGroup().Filter(x => ValueTask.FromResult(x.MessageText == "Hi!"));
    
group.Handle(() => Results.MessageReply("Hey there!"));

app.Handle((ILocalizer localizer) =>
{
    var helloText = localizer["HelloText"];
    var helloButton = localizer["Button.Hello"];
    var catButton = localizer["Button.Cat"];
    var keyboard = new ReplyKeyboardMarkup(new []
    {
        new KeyboardButton(helloButton),
        new KeyboardButton(catButton),
    });

    return Results.Message(helloText, keyboard);
}).FilterCommand("/start");

app.Handle(() =>
{
    var keyboard = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Send photo!", "Photo"));
    return Results.MessageEdit("Button pressed", keyboard);
}).FilterCallbackData(x => x == "Hello");

app.Handle(() => Results.Photo("cat.jpeg", "Little cat")).FilterCallbackData(x => x == "Photo");

app.Handle(() => Results.MessageReply("I'm replied!"))
    .FilterText(x => x.Equals("reply", StringComparison.OrdinalIgnoreCase));

app.Handle(async (string messageText, WeatherService weatherService) =>
{
    var weather = await weatherService.GetWeather();
    var keyboard = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Hello", "Hello"));
    return ($"Hello, {messageText}, weather is {weather}", keyboard);
}).FilterUpdateType(x => x == UpdateType.Message);

app.Run();