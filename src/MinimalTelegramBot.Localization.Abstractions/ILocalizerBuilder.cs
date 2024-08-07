namespace MinimalTelegramBot.Localization.Abstractions;

public interface ILocalizerBuilder
{
    ILocalizerBuilder EnrichFromFile(string fileName, Locale locale);
    ILocaleStringSetRepository Build();
}