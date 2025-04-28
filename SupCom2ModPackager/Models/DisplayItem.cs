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
    string StatusText { get; set; }
    Visibility ProgressVisible { get; }
    double ProgressValue { get; set; }
    double ProgressMaximum { get; set; }
    Visibility ColumnsVisible { get; }
}

[DebuggerDisplay("{Name} Exists:{Exists}")]
public class DisplayItem : IDisplayItem
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual string Name => throw new NotImplementedException();
    public virtual string NameSort => throw new NotImplementedException();
    public virtual string FullPath => throw new NotImplementedException();
    public virtual DateTime Modified => throw new NotImplementedException();
    public virtual DateTime ModifiedSort => Modified;
    public virtual bool Exists => throw new NotImplementedException();

    private double progressMaximum = 100;
    public double ProgressMaximum
    {
        get => progressMaximum;
        set => SetProperty(ref progressMaximum, value);
    }

    private double progressValue = 0;
    public double ProgressValue
    {
        get => progressValue;
        set => SetProperty(ref progressValue, value);
    }

    private Visibility statusTextVisible = Visibility.Collapsed;
    public Visibility StatusTextVisible
    {
        get => statusTextVisible;
        set => SetProperty(ref statusTextVisible, value);
    }
    private string statusText = string.Empty;
    public string StatusText
    {
        get => statusText;
        set => SetProperty(ref statusText, value);
    }

    private Visibility progressVisible = Visibility.Collapsed;
    public Visibility ProgressVisible
    {
        get => progressVisible;
        set => SetProperty(ref progressVisible, value);
    }
    private Visibility columnsVisible = Visibility.Visible;
    public Visibility ColumnsVisible
    {
        get => columnsVisible;
        set => SetProperty(ref columnsVisible, value);
    }

    protected DisplayItem()
    {
    }

    protected void SetProperty<T>(ref T backingField, T value, [CallerMemberName] string memberName = null!)
    {
        if (!EqualityComparer<T>.Default.Equals(backingField, value))
        {
            backingField = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
        }
    }

}


