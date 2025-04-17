using System;
using SharpHook;

namespace CustomKey.Common;

public static class Keyboard
{
    // Événement déclenché lorsque l'état de Shift change
    public static event Action? IsShiftChanged;

    public static bool _isShift;
    public static bool _inputOn = true;

    public static bool IsShift
    {
        get => _isShift;
        set
        {
            if (_isShift != value)
            {
                _isShift = value;
                IsShiftChanged?.Invoke(); // Notifie les abonnés que l'état a changé
            }
        }
    }
    
    public static void CharInput(String text)
    {
        IEventSimulator outputInjector = new EventSimulator();
        outputInjector.SimulateTextEntry(text);
    }
}