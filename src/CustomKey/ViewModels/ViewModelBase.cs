using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CustomKey.Common;

namespace CustomKey.ViewModels;

public class ViewModelBase : ObservableObject
{
    public new event PropertyChangedEventHandler? PropertyChanged;
    
    protected bool RaiseAndSetIfChanged<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
        return false;
    }

    public string this[string id]
    {
        get
        {
            // KeyID Bindings
            if (id.StartsWith("Key")) return LayoutLoader.GetChar(id);

            // Translations Bindings
            if (Translator.TradBinding.TryGetValue(id, out var value)) return value;
            
            return $"[undefined:{id}]";
        }
    }
}
