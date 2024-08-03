using MinimalTelegramBot.StateMachine.Abstractions;

namespace UsageExample.States;

public class EnteringNameStates : State
{
    public EnteringNameStates(int stateId) : base(stateId)
    {
    }

    public static State EnteringText => new EnteringNameStates(1);
}