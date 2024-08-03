using System.Globalization;
using MinimalTelegramBot.Handling;
using Telegram.Bot.Types.ReplyMarkups;

namespace UsageExample.CallbackModels;

public class PaginationModel : ICallbackDataParser<PaginationModel>
{
    public const string CallbackPrefix = "page";

    public int PageNumber { get; init; }

    public static PaginationModel Parse(string callbackData)
    {
        return Parse(callbackData.AsSpan());
    }

    public InlineKeyboardMarkup? GetPageKeyboard(string backText, string backCallback)
    {
        if (PageNumber == -1)
        {
            return null;
        }

        var navigation = new List<InlineKeyboardButton>(3);

        if (PageNumber > 1)
        {
            navigation.Add(InlineKeyboardButton.WithCallbackData("<", $"{CallbackPrefix}_{PageNumber - 1}"));
        }

        navigation.Add(InlineKeyboardButton.WithCallbackData(PageNumber.ToString(CultureInfo.InvariantCulture),
            $"{CallbackPrefix}_-1"));

        navigation.Add(InlineKeyboardButton.WithCallbackData(">",
            $"{CallbackPrefix}_{PageNumber + 1}"));

        return new InlineKeyboardMarkup(
        [
            navigation,
            [InlineKeyboardButton.WithCallbackData(backText, backCallback),],
        ]);
    }

    private static PaginationModel Parse(ReadOnlySpan<char> callbackData)
    {
        var index = callbackData.IndexOf('_');
        var sNumber = callbackData[(index + 1)..];
        var number = int.Parse(sNumber, CultureInfo.InvariantCulture);
        return new PaginationModel { PageNumber = number, };
    }
}