using MinimalTelegramBot.Handling;

namespace UsageExample.CommandModels;

public class AddCommandModel : ICommandParser<AddCommandModel>
{
    public double A { get; set; }
    public double B { get; set; }
    public bool IsError { get; set; }
    public double Result => A + B;

    private static AddCommandModel Error => new() { IsError = true, };

    public static AddCommandModel Parse(string command, IFormatProvider? formatProvider = null)
    {
        var parts = command.Split(' ');

        if (parts.Length < 3 ||
            !double.TryParse(parts[1], formatProvider, out var a) ||
            !double.TryParse(parts[2], formatProvider, out var b))
        {
            return Error;
        }

        return new AddCommandModel { A = a, B = b, IsError = false, };
    }
}