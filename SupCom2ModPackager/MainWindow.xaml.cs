using SupCom2ModPackager.DisplayItemClass;
using SupCom2ModPackager.Extensions;
using SupCom2ModPackager.Utility;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace SupCom2ModPackager;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private DisplayItemCollection _items;
    private readonly SC2ModPackager _modPackager;

    public MainWindow(
        DisplayItemCollection items,
        SC2ModPackager modPackager)
    {
        _items = items;
        _modPackager = modPackager;
        InitializeComponent();

        ExtractionProgress.Visibility = Visibility.Hidden;
        ExtractionProgressBar.Value = -1;
        ExtractionProgressBar.Minimum = 0;
        ExtractionProgressBar.Maximum = 0;
        CurrentFileTextBlock.Text = string.Empty;

        PathDataGrid.ItemsSource = _items;
        PathDataGrid.CanUserSortColumns = true;
        PathLink.PathChanged += PathLink_PathChanged;
        // Automatically sort the DataGrid by Name
        var collectionView = CollectionViewSource.GetDefaultView(PathDataGrid.ItemsSource);
        collectionView.SortDescriptions.Clear();
        collectionView.SortDescriptions.Add(new SortDescription(nameof(DisplayItem.Name), ListSortDirection.Ascending));
        collectionView.Refresh();


        var path = GeneralExtensions
            .GetValidDrives()
            .Select(name => @$"{name}SC2Mods\Testing")
            .First(Directory.Exists);
        PathLink.Path = path;
    }

    private void PathLink_PathChanged(object? sender, string newPath)
    {
        _items.Clear();
        _items.AddParent(new(newPath));
        foreach (var dir in Directory.GetDirectories(newPath))
        {
            _items.Add(new DirectoryInfo(dir));
        }
        foreach (var file in Directory.GetFiles(newPath))
        {
            _items.Add(new FileInfo(file));
        }
    }

    private async void ActionClicked(object sender, MouseButtonEventArgs e)
    {
        if (!((DataGrid)sender).TryGetDisplayItem(e, out string column, out DisplayItem item))
            return;

        if (item.Action == "UnpackAsync")
        {
            var fileItem = (DisplayItemFile)item;

            ExtractionProgressBar.Value = -1;
            ExtractionProgressBar.Maximum = 0;
            CurrentFileTextBlock.Text = string.Empty;

            // Show the progress overlay
            ExtractionProgress.Visibility = Visibility.Visible;
            // Create a progress reporter
            var progress = new Progress<PackProgressArgs>(args =>
            {
                if (args.Text != null)
                    CurrentFileTextBlock.Text = args.Text;
                if (args.Value != null)
                    ExtractionProgressBar.Value = args.Value.Value;
                else
                    ++ExtractionProgressBar.Value;
                if (args.Maximum != null)
                    ExtractionProgressBar.Maximum = args.Maximum.Value;
            });


            var overWrite = false;
            if (Directory.Exists(fileItem.UnpackDirectoryPath))
            {
                //var result = MessageBox.Show("Overwrite directory?", "UnpackAsync will overwrite existing directory", MessageBoxButton.OKCancel);
                //if (result == MessageBoxResult.OK)
                //{
                overWrite = true;
                //}

            }

            await _modPackager.UnpackAsync(fileItem, overWrite, progress);


            // Reset UI after completion
            ExtractionProgress.Visibility = Visibility.Hidden;
        }
    }



    private void PathDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
    {

    }
}
