using SupCom2ModPackager.Extensions;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SupCom2ModPackager;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private DisplayItemCollection _items;
    private readonly SC2ModPackager _modPackager;

    public MainWindow(DisplayItemCollection items, SC2ModPackager modPackager)
    {
        _items = items;
        _modPackager = modPackager;
        InitializeComponent();
        //ExtractionProgress.Visibility = Visibility.Visible;
        PathDataGrid.ItemsSource = _items;
        PathDataGrid.CanUserSortColumns = true;
        PathLink.PathChanged += PathLink_PathChanged;
        // Automatically sort the DataGrid by Name
        var collectionView = CollectionViewSource.GetDefaultView(PathDataGrid.ItemsSource);
        collectionView.SortDescriptions.Clear();
        collectionView.SortDescriptions.Add(new SortDescription(nameof(DisplayItem.Name), ListSortDirection.Ascending));
        collectionView.Refresh();


        var path = DriveInfo
            .GetDrives()
            .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
            .Select(d => @$"{d.Name}SC2Mods\Testing")
            .First(Directory.Exists);
        PathLink.Path = path;
    }

    private void PathLink_PathChanged(object? sender, string newPath)
    {
        _items.Clear();
        _items.Add(DisplayItemType.Directory, newPath, "...");
        foreach (var dir in Directory.GetDirectories(newPath))
        {
            _items.Add(DisplayItemType.Directory, dir);
        }
        foreach (var file in Directory.GetFiles(newPath))
        {
            _items.Add(DisplayItemType.File, file);
        }
    }

    private async void ActionClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DataGrid myGrid = (DataGrid)sender;
        Point mousePos = e.GetPosition(myGrid);

        var element = myGrid.InputHitTest(mousePos) as Visual;
        var cell = element?.GetAncestorOfType<DataGridCell>();
        if (cell == null)
        {
            return;
        }

        var cellHeader = myGrid.Columns[cell.Column.DisplayIndex];
        if (cellHeader.Header.ToString() != "Action")
        {
            return;
        }

        var item = (DisplayItem)((DataGridRow)cell.BindingGroup.Owner).Item;
        if (item.Action == "Unpack")
        {
            ExtractionProgress.Visibility = Visibility.Visible;
            ExtractionProgressBar.Value = -1;
            // Show the progress overlay
            // Create a progress reporter
            var progress = new Progress<string>(text =>
            {
                ++ExtractionProgressBar.Value;
                CurrentFileTextBlock.Text = text;
            });

            await _modPackager.Unpack(item, progress);

            var directory = Path.ChangeExtension(item.Path, null);
            if (Directory.Exists(directory))
            {
                //var result = MessageBox.Show("Overwrite directory?", "Unpack will overwrite existing directory", MessageBoxButton.OKCancel);
                //if (result != MessageBoxResult.OK)
                //{
                //    return;
                //}
                ((IProgress<string>)progress).Report("Removing folder ...");
                await Task.Run(() => Directory.Delete(directory, true));

            }
            var targetDirectory = _items.FirstOrDefault(dirItem => string.Equals(directory, dirItem.Path, StringComparison.OrdinalIgnoreCase));
            if (targetDirectory == null)
            {
                Directory.CreateDirectory(directory);
                targetDirectory = _items.Add(DisplayItemType.Directory, directory);

                await Task.CompletedTask;
            }


            ((IProgress<string>)progress).Report("Scanning ...");
            ExtractionProgressBar.Maximum = await Task.Run(() => GetCountForAllEntries(item.Path));

            // Perform the extraction
            await Task.Run(() => UnpackAsync(item.Path, progress));

            // Reset UI after completion
            ExtractionProgress.Visibility = Visibility.Hidden;
        }
    }

    private static async Task<int> GetCountForAllEntries(string path)
    {
        if (!path.IsCompressedFile())
        {
            return await Task.FromResult(0); // Use Task.FromResult to return a completed task with a result
        }

        return await Task.Run(() => // Use Task.Run to perform CPU-bound work on a background thread
        {
            using var zipFile = ZipFile.OpenRead(path);
            return GetCountForAllEntries(zipFile);
        });
    }

    private static async Task<int> GetCountForAllEntries(ZipArchive archive)
    {
        int count = 0;
        foreach (var entry in archive.Entries)
        {
            if (entry.FullName.EndsWith("/"))
            {
                continue;
            }
            ++count;

            // Check if the entry is a nested zip file
            if (entry.FullName.IsCompressedFile())
            {
                using var nestedStream = entry.Open();
                using var nestedArchive = new ZipArchive(nestedStream, ZipArchiveMode.Read);
                count += await GetCountForAllEntries(nestedArchive);
            }
        }
        return count;
    }

    private static async Task UnpackAsync(string path, IProgress<string> progress)
    {
        if (!path.IsCompressedFile())
        {
            return;
        }

        var directory = Path.ChangeExtension(path, null);
        Directory.CreateDirectory(directory);

        using (var zipFile = ZipFile.OpenRead(path))
        {
            foreach (var entry in zipFile.Entries)
            {
                if (entry.FullName.EndsWith("/"))
                {
                    continue;
                }

                var filePath = Path.Combine(directory, entry.FullName);
                var fileDirectory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(fileDirectory) && !Directory.Exists(fileDirectory))
                {
                    Directory.CreateDirectory(fileDirectory);
                }

                // Extract the file
                entry.ExtractToFile(filePath, overwrite: true);

                progress.Report($"Extracting {entry.FullName}");

                // Recursively unpack nested archives
                await UnpackAsync(filePath, progress);
            }
        }
    }

    private void PathDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
    {

    }
}
