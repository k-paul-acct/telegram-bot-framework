using MinimalTelegramBot.Localization.Abstractions;

namespace MinimalTelegramBot.Localization;

/// <inheritdoc />
public class InMemoryLocaleStringSetRepository : ILocaleStringSetRepository
{
    private readonly Dictionary<string, IReadOnlyDictionary<string, string>> _translates = new();

    /// <inheritdoc />
    public string GetString(string key, string locale)
    {
        if (!_translates.TryGetValue(locale, out var stringSet) || !stringSet.TryGetValue(key, out var result))
        {
            throw new Exception($"No string with key {key} found for locale {locale}");
        }

        return result;
    }

    /// <inheritdoc />
    public void AddLocaleStringSet(LocaleStringSet localeStringSet)
    {
        _translates[localeStringSet.Locale.ToString()] = localeStringSet.Values;
    }
}