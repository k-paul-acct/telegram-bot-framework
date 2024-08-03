using MinimalTelegramBot.Handling;
using MinimalTelegramBot.Localization.Abstractions;
using MinimalTelegramBot.Pipeline;
using MinimalTelegramBot.Services;
using MinimalTelegramBot.Settings;
using MinimalTelegramBot.StateMachine.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MinimalTelegramBot;

public class BotApplication : IBotApplicationBuilder, IHandlerDispatcher
{
    private readonly IHandlerBuilder _handlerBuilder;
    private readonly IHandlerDispatcher _handlerDispatcher;
    private readonly PipelineBuilder _pipelineBuilder = new();
    private readonly List<HandlerSource> _handlerSources = [];
    private readonly List<Func<BotRequestContext, Func<BotRequestContext, ValueTask<bool>>>> _filterFactories = [];

    private Func<BotRequestContext, Task>? _pipeline;

    internal readonly TelegramBotClient Client;
    internal readonly BotApplicationOptions Options;

    internal BotApplication(IHost host, TelegramBotClient client, BotApplicationOptions options,
        IHandlerBuilder handlerBuilder)
    {
        Host = host;
        Client = client;
        Options = options;
        _handlerBuilder = handlerBuilder;
        _handlerDispatcher = new HandlerGroupBuilder(this);

        UseDefaultOuterPipes();
    }

    public IHost Host { get; }
    public IServiceProvider ServiceProvider => Host.Services;
    public ICollection<HandlerSource> HandlerSources => _handlerSources;
    public ICollection<Func<BotRequestContext, Func<BotRequestContext, ValueTask<bool>>>> FilterFactories => _filterFactories;

    IDictionary<string, object?> IBotApplicationBuilder.Properties => _pipelineBuilder.Properties;

    public IBotApplicationBuilder Use(Func<Func<BotRequestContext, Task>, Func<BotRequestContext, Task>> pipe)
    {
        _pipelineBuilder.Use(pipe);
        return this;
    }

    Func<BotRequestContext, Task> IBotApplicationBuilder.Build()
    {
        _pipeline = _pipelineBuilder.Build();
        return _pipeline;
    }

    public Handler Handle(Delegate handlerDelegate)
    {
        return _handlerDispatcher.Handle(handlerDelegate);
    }

    public Handler Handle(Func<BotRequestContext, Task> func)
    {
        return _handlerBuilder.Handle(func);
    }

    Handler? IHandlerBuilder.TryResolveHandler(BotRequestContext ctx)
    {
        return _handlerBuilder.TryResolveHandler(ctx);
    }

    internal void InitBot(bool isWebhook)
    {
        using var scope = Host.Services.CreateScope();
        var botInitService = scope.ServiceProvider.GetRequiredService<BotInitService>();
        botInitService.InitBot(isWebhook).GetAwaiter().GetResult();
    }

    public static BotApplicationBuilder CreateBuilder()
    {
        return new BotApplicationBuilder(args: null);
    }

    public static BotApplicationBuilder CreateBuilder(string[] args)
    {
        return new BotApplicationBuilder(args);
    }

    public static BotApplicationBuilder CreateBuilder(BotApplicationBuilderOptions options)
    {
        return new BotApplicationBuilder(options);
    }

    public void Run()
    {
        if (_pipelineBuilder.Properties.ContainsKey("__CallbackAutoAnsweringAdded"))
        {
            this.UsePipe<CallbackAutoAnsweringPipe>();
        }

        this.UsePipe<HandlerResolverPipe>();
        BotApplicationRunner.Run(this);
    }

    internal void StartPolling()
    {
        Client.StartReceiving(UpdateHandler, PollingErrorHandler, Options.ReceiverOptions);
    }

    private Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        _ = Task.Run(() => HandleUpdateInBackground(client, update), cancellationToken);
        return Task.CompletedTask;
    }

    internal async Task HandleUpdateInBackground(ITelegramBotClient client, Update update)
    {
        using var scope = Host.Services.CreateScope();

        var context = new BotRequestContext();
        var contextAccessor = scope.ServiceProvider.GetRequiredService<IBotRequestContextAccessor>();

        contextAccessor.BotRequestContext = context;

        var chatId = update.Message?.Chat.Id ??
                     update.CallbackQuery?.Message?.Chat.Id ??
                     update.EditedMessage?.Chat.Id ??
                     update.ChannelPost?.Chat.Id ??
                     update.EditedChannelPost?.Chat.Id ??
                     update.MessageReaction?.Chat.Id ??
                     update.MessageReactionCount?.Chat.Id ??
                     update.ChatBoost?.Chat.Id ??
                     update.RemovedChatBoost?.Chat.Id ??
                     0;

        var messageText = update.Message?.Text;
        var callbackData = update.CallbackQuery?.Data;

        var stateMachine = scope.ServiceProvider.GetRequiredService<IStateMachine>();
        var state = chatId != 0 ? stateMachine.GetState(chatId) : null;
        var localizer = scope.ServiceProvider.GetService<ILocalizer>();
        var localeService = chatId != 0 ? scope.ServiceProvider.GetService<IUserLocaleService>() : null;
        var locale = localeService is null
            ? null
            : await localeService.GetFromRepositoryOrUpdateWithProviderAsync(chatId);

        context.Client = client;
        context.Update = update;
        context.ChatId = chatId;
        context.MessageText = messageText;
        context.CallbackData = callbackData;
        context.UserLocale = locale;
        context.Services = scope.ServiceProvider;
        context.StateMachine = stateMachine;
        context.Localizer = localizer;
        context.UserState = state;

        await _pipeline!(context);
    }

    private Task PollingErrorHandler(ITelegramBotClient client, Exception e, CancellationToken cancellationToken)
    {
        using var scope = Host.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<BotApplication>>();
        logger.LogError(500, e, "Bot error: {Error}", e.Message);
        return Task.CompletedTask;
    }

    private void UseDefaultOuterPipes()
    {
        this.UsePipe<ExceptionHandlerPipe>();
        this.UsePipe<UpdateLoggerPipe>();
    }
}