using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using SupCom2ModPackager.Models;
using SupCom2ModPackager.Utility;

namespace SupCom2ModPackager.Collections;
public class DisplayItemCollection : ObservableCollection<IDisplayItem>
{
    private readonly SharedData sharedData = ServiceLocator.GetRequiredService<SharedData>();
    private readonly FileSystemWatcher _fileSystemWatcher;

    public DisplayItemCollection()
    {
        _fileSystemWatcher = new FileSystemWatcher
        {
            IncludeSubdirectories = false,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
            Filter = "*",
            EnableRaisingEvents = false
        };
        _fileSystemWatcher.Deleted += (o, e) =>
        {
            var item = this.FirstOrDefault(i => i.FullPath == e.FullPath);
            if (item != null)
            {
                Application.Current.Dispatcher.Invoke(() => Remove(item));
            }

        };
        _fileSystemWatcher.Created += (o, e) =>
        {
            if (File.Exists(e.FullPath))
            {
                Application.Current.Dispatcher.Invoke(() => Add(new DisplayItemFile(this, new FileInfo(e.FullPath))));
                return;
            }
            if (Directory.Exists(e.FullPath))
            {
                Application.Current.Dispatcher.Invoke(() => Add(new DisplayItemDirectory(this, new DirectoryInfo(e.FullPath))));
                return;
            }
            throw new InvalidOperationException($"File system watcher created event for {e.FullPath} but it is not a file or directory");
        };

        _fileSystemWatcher.EnableRaisingEvents = false;

    }

    public IEnumerable<DisplayItemFile> Files => this.Where(item => item is DisplayItemFile).Cast<DisplayItemFile>();
    public IEnumerable<DisplayItemDirectory> Directories => this
        .Where(item => item is DisplayItemDirectory && !(item is DisplayItemDirectoryParent))
        .Cast<DisplayItemDirectory>();

    public void Load()
    {
        if (string.IsNullOrEmpty(sharedData.CurrentPath) || !Directory.Exists(sharedData.CurrentPath))
            return;
        _fileSystemWatcher.EnableRaisingEvents = false;

        Clear();
        var directoryInfo = new DirectoryInfo(sharedData.CurrentPath);
        Add(new DisplayItemDirectoryParent(this, directoryInfo));

        foreach (var file in directoryInfo.GetFiles())
        {
            Add(new DisplayItemFile(this, file));
        }
        foreach (var directory in directoryInfo.GetDirectories())
        {
            Add(new DisplayItemDirectory(this, directory));
        }
        _fileSystemWatcher.Path = sharedData.CurrentPath;
        _fileSystemWatcher.EnableRaisingEvents = true;
    }
}

