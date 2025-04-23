using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Microsoft.Extensions.FileSystemGlobbing;
using SupCom2ModPackager.Models;

namespace SupCom2ModPackager.Collections;

public class DisplayItemCollection : ObservableCollection<IDisplayItem>
{
    public static readonly DisplayItemCollection Empty = new();

    public DisplayItemCollection()
    {
    }

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
}

public class DisplayItemCollectionMonitor : IDisposable
{
    private readonly string path;
    private readonly DisplayItemCollection collection;
    private readonly FileSystemWatcher watcher;

    public DisplayItemCollectionMonitor(string path, DisplayItemCollection collection)
    {
        this.path = path;
        this.collection = collection;
        watcher = new FileSystemWatcher(path)
        {
            IncludeSubdirectories = false,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
            Filter = "*"
        };
        watcher.Created += OnCreated;
        watcher.Deleted += OnDeleted;
        watcher.EnableRaisingEvents = false;

    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        throw new NotImplementedException();
    }

    public void Load()
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            return;
        var directoryInfo = new DirectoryInfo(path);
        foreach (var file in directoryInfo.GetFiles())
        {
            collection.Add(file);
        }
        foreach (var directory in directoryInfo.GetDirectories())
        {
            collection.Add(directory);
        }
    }

    public void Dispose()
    {
        watcher.EnableRaisingEvents = false;
        watcher.Created -= OnCreated;
        watcher.Deleted -= OnDeleted;
        watcher.Dispose();
    }
}

