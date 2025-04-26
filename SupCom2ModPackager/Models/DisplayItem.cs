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
    Visibility StatusTextVisible { get; set; }
    Visibility ProgressVisible { get; set; }
    Visibility ColumnsVisible { get; set; }
    string StatusText { get; set; }
    double ProgressMaximum { get; set; }
    double ProgressValue { get; set; }
}

[DebuggerDisplay("{Name} Exists:{Exists}")]
public class DisplayItem : IDisplayItem
{
    private string statusText = string.Empty;
    private Visibility statusTextVisible = Visibility.Collapsed;
    private Visibility progressVisible = Visibility.Collapsed;
    private Visibility columnsVisible = Visibility.Collapsed;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected readonly DisplayItemCollection collection = DisplayItemCollection.Empty;

    public virtual string Name => throw new NotImplementedException();
    public virtual string NameSort => Name;
    public virtual string FullPath => throw new NotImplementedException();
    public virtual DateTime Modified => throw new NotImplementedException();
    public virtual DateTime ModifiedSort => Modified;
    public virtual bool Exists => throw new NotImplementedException();

    public Visibility StatusTextVisible
    {
        get => statusTextVisible;
        set => SetValue(value, ref statusTextVisible);
    }
    public Visibility ProgressVisible
    {
        get => progressVisible;
        set => SetValue(value, ref progressVisible);
    }

    public Visibility ColumnsVisible
    {
        get => columnsVisible;
        set => SetValue(value, ref columnsVisible);
    }
    public string StatusText
    {
        get => statusText;
        set => SetValue(value, ref statusText);
    }

    private double progressMaximum = 100;
    public double ProgressMaximum
    {
        get => progressMaximum;
        set => SetValue(value, ref progressMaximum);
    }

    private double progressValue = 0;
    public double ProgressValue
    {
        get => progressValue;
        set => SetValue(value, ref progressValue);
    }

    private DisplayItem() { }

    protected DisplayItem(DisplayItemCollection collection)
    {
        this.collection = collection;
    }

    protected void SetValue<T>(T value, ref T backingField, [CallerMemberName] string memberName = null!)
    {
        if (!EqualityComparer<T>.Default.Equals(backingField, value))
        {
            backingField = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
        }
    }

}


