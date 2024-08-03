namespace MinimalTelegramBot.Handling;

public interface ICommandParser<out TModel>
{
    static abstract TModel Parse(string command, IFormatProvider? formatProvider = null);
}