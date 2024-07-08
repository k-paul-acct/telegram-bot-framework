namespace MinimalTelegramBot.Localization.Abstractions.Providers;

public interface IUserLocaleProvider
{
    Task<Locale> GetUserLocaleAsync(long userId);
}