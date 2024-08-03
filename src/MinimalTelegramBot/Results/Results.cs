using MinimalTelegramBot.Results.Extensions;
using Telegram.Bot.Types.ReplyMarkups;

namespace MinimalTelegramBot.Results;

public static class Results
{
    public static IResult Empty => new MessageResult(string.Empty);

    public static IResult Message(string message, IReplyMarkup? keyboard = null)
    {
        return new MessageResult(message, keyboard);
    }

    public static IResult MessageReply(string message)
    {
        return new MessageResult(message, reply: true);
    }

    public static IResult MessageEdit(string message, IReplyMarkup? keyboard = null)
    {
        return new MessageResult(message, keyboard, edit: true);
    }

    public static IResult CallbackAnswer()
    {
        return new CallbackAnswerResult();
    }

    public static IResult Photo(string photoName, string? caption = null)
    {
        return new PhotoResult(photoName, caption);
    }

    public static IResult Photo(Stream photoStream, string? caption = null)
    {
        return new PhotoResult(photoStream, caption);
    }

    public static IResult Document(string documentName, string? caption = null)
    {
        return new DocumentResult(documentName, caption);
    }

    public static IResult Document(Stream documentStream, string? caption = null)
    {
        return new DocumentResult(documentStream, caption);
    }
}