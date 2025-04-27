using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Shapes;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.VisualBasic;
using RtfPipe.Tokens;
using SupCom2ModPackager.Extensions;
using SupCom2ModPackager.Models;
using SupCom2ModPackager.Utility;

namespace SupCom2ModPackager.Collections;

public class DisplayItemCollection : ObservableCollection<IDisplayItem>
{
    public static readonly DisplayItemCollection Empty = new(null!);
    private readonly SharedData sharedData;
    private readonly FileSystemWatcher _fileSystemWatcher;

    private string path = string.Empty;
    public string Path
    {
        get => path;
        set
        {
            if (!EqualityComparer<string>.Default.Equals(path, value))
            {
                path = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Path)));
                Load();
            }
        }
    }

    public DisplayItemCollection(SharedData sharedData)
    {
        _fileSystemWatcher = null!;
        this.sharedData = null!;
        if (sharedData == null)
            return;
        this.sharedData = sharedData;
        sharedData.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(sharedData.CurrentPath))
            {
                Path = sharedData.CurrentPath;
            }
        };
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
        _path = path;
        Clear();
        _fileSystemWatcher.Path = sharedData.CurrentPath;
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
        _fileSystemWatcher.EnableRaisingEvents = true;
    }
}

