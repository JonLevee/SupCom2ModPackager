using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SupCom2ModPackager.DisplayItemClass;

[DebuggerDisplay("{Name} Exists:{Exists}")]
public class DisplayItem : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public static readonly DisplayItem Empty = new DisplayItem();
    protected readonly DisplayItemCollection collection = DisplayItemCollection.Empty;

    public virtual string Name => throw new NotImplementedException();
    public virtual string FullPath => throw new NotImplementedException();
    public virtual DateTime Modified => throw new NotImplementedException();
    public string Action => GetAction();
    public virtual bool Exists => false;

    private DisplayItem() { }

    protected DisplayItem(DisplayItemCollection collection)
    {
        this.collection = collection;
    }

    protected virtual string GetAction() => throw new NotImplementedException();

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }


}


