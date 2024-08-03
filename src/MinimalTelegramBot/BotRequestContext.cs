using MinimalTelegramBot.Localization.Abstractions;
using MinimalTelegramBot.StateMachine.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MinimalTelegramBot;

public class BotRequestContext
{
    public ITelegramBotClient Client { get; set; } = null!;
    public Update Update { get; set; } = null!;
    public long ChatId { get; set; }
    public string? MessageText { get; set; }
    public string? CallbackData { get; set; }
    public Locale? UserLocale { get; set; }
    public State? UserState { get; set; }
    public IServiceProvider Services { get; set; } = null!;
    public IStateMachine StateMachine { get; set; } = null!;
    public ILocalizer? Localizer { get; set; }
    public IDictionary<string, object?> Data { get; } = new Dictionary<string, object?>();
    internal bool UpdateHandlingStarted { get; set; }
}