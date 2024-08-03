using Telegram.Bot;

namespace MinimalTelegramBot.Services;

internal sealed class BotInitService
{
    private readonly TelegramBotClient _client;
    private readonly ILogger<BotApplication> _logger;

    public BotInitService(ILogger<BotApplication> logger, TelegramBotClient client)
    {
        _logger = logger;
        _client = client;
    }

    private static string Fullname(string firstname, string? lastname)
    {
        return lastname is null ? firstname : $"{firstname} {lastname}";
    }

    public async Task InitBot(bool isWebhook)
    {
        var bot = await _client.GetMeAsync();
        var message = isWebhook ? "Getting updates via webhook" : "Polling";

        _logger.LogInformation(message + " started for bot @{BotUsername} ({BotFullname}) with ID = {BotId}",
            bot.Username, Fullname(bot.FirstName, bot.LastName), bot.Id);
    }
}