using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace SupCom2ModPackager;

public enum DisplayItemType
{
    Directory,
    File
}

public class DisplayItemCollection : ObservableCollection<DisplayItem>
{
    public DisplayItemCollection()
    {
    }
}
public class DisplayItem : INotifyPropertyChanged
{
    private readonly DisplayItemType _displayType;
    public string Name { get; private set; }
    public string Path { get; private set; }

    private DateTime _lastModified;
    public DateTime LastModified
    {
        get => _lastModified;
        set
        {
            if (_lastModified != value)
            {
                _lastModified = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastModified)));
            }
        }
    }


    public string Action
    {
        get => _displayType switch
        {
            DisplayItemType.File => GetFileAction(),
            DisplayItemType.Directory => GetDirectoryAction(),
            _ => "Unknown"
        };
    }

    public DisplayItem(DisplayItemType displayType, string path, string? name = null)
    {
        _displayType = displayType;
        Path = path;
        this.Name = name ?? System.IO.Path.GetFileName(path);
        _lastModified = displayType == DisplayItemType.File
            ? new FileInfo(path).LastWriteTime
            : new DirectoryInfo(path).LastWriteTime;
    }

    private string GetFileAction()
    {
        return string.Equals(".sc2", System.IO.Path.GetExtension(this.Path), StringComparison.OrdinalIgnoreCase)
            ? "Unpack"
            : string.Empty;
    }

    private string GetDirectoryAction()
    {
        return string.Empty;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
