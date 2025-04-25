using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Shapes;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.VisualBasic;
using SupCom2ModPackager.Extensions;
using SupCom2ModPackager.Models;
using SupCom2ModPackager.Utility;

namespace SupCom2ModPackager.Collections;

public class DisplayItemCollection : ObservableCollection<IDisplayItem>, IDisposable
{
    public static readonly DisplayItemCollection Empty = new();


    public string Path
    {
        get => this.GetSyncValue<string>();
        set
        {
            if (this.SetSyncValue(value))
                Load();
        }
    }

    public DisplayItemCollection()
    {
    }

    public IEnumerable<DisplayItemFile> Files => this.Where(item => item is DisplayItemFile).Cast<DisplayItemFile>();
    public IEnumerable<DisplayItemDirectory> Directories => this
        .Where(item => item is DisplayItemDirectory && !(item is DisplayItemDirectoryParent))
        .Cast<DisplayItemDirectory>();


    public DisplayItemFile Add(FileInfo info)
    {
        var item = new DisplayItemFile(this, info);
        Add(item);
        return item;
    }

    public DisplayItemDirectory Add(DirectoryInfo info)
    {
        var item = new DisplayItemDirectory(this, info);
        Add(item);
        return item;
    }

    public DisplayItemDirectoryParent AddParent(DirectoryInfo info)
    {
        var item = new DisplayItemDirectoryParent(this, info);
        Add(item);
        return item;
    }

    public bool TryRemove(string path, out IDisplayItem? item)
    {
        item = this.FirstOrDefault(i => i.FullPath == path);
        if (item != null)
        {
            Remove(item);
            return true;
        }
        return false;
    }

    public void Load()
    {
        if (string.IsNullOrEmpty(Path) || !Directory.Exists(Path))
            return;
        Clear();
        var directoryInfo = new DirectoryInfo(Path);
        AddParent(directoryInfo);
        foreach (var file in directoryInfo.GetFiles())
        {
            Add(file);
        }
        foreach (var directory in directoryInfo.GetDirectories())
        {
            Add(directory);
        }
    }

    public void Dispose()
    {
        PropertySyncManager.Remove(this);
    }

}

