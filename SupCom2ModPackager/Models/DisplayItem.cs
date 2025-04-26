using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using SupCom2ModPackager.Collections;

namespace SupCom2ModPackager.Models;

public interface IDisplayItem : INotifyPropertyChanged
{
    string Name { get; }
    string NameSort { get; }
    string FullPath { get; }
    DateTime Modified { get; }
    DateTime ModifiedSort { get; }
    bool Exists { get; }
    Visibility StatusTextVisible { get; }
    Visibility ProgressVisible { get; }
    Visibility ColumnsVisible { get; }
}

[DebuggerDisplay("{Name} Exists:{Exists}")]
public class DisplayItem : IDisplayItem
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public static readonly DisplayItem Empty = new DisplayItem();
    protected readonly DisplayItemCollection collection = DisplayItemCollection.Empty;

    public virtual string Name => throw new NotImplementedException();
    public virtual string NameSort => Name;
    public virtual string FullPath => throw new NotImplementedException();
    public virtual DateTime Modified => throw new NotImplementedException();
    public virtual DateTime ModifiedSort => Modified;
    public virtual bool Exists => false;

    public Visibility StatusTextVisible { get; set; } = Visibility.Collapsed;
    public Visibility ProgressVisible { get; set; } = Visibility.Collapsed;
    public Visibility ColumnsVisible { get; set; } = Visibility.Visible;
    private DisplayItem() { }

    protected DisplayItem(DisplayItemCollection collection)
    {
        this.collection = collection;
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}


