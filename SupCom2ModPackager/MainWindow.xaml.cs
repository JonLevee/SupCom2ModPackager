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
    private readonly ProgressReporter progressReporter;

    public MainWindow(
        DisplayItemCollection items,
        SC2ModPackager modPackager,
        ProgressReporter progressReporter)
    {
        _items = items;
        _modPackager = modPackager;
        this.progressReporter = progressReporter;
        InitializeComponent();

        progressReporter.SetVisibilityProperty(() => ExtractionProgress.Visibility);
        progressReporter.SetMinimumProperty(() => ExtractionProgressBar.Minimum);
        progressReporter.SetMaximumProperty(() => ExtractionProgressBar.Maximum);
        progressReporter.SetValueProperty(() => ExtractionProgressBar.Value);
        progressReporter.SetTextProperty(() => CurrentFileTextBlock.Text);

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
            using (progressReporter.CreateReporter())
            {
                await _modPackager.Unpack(item);
            }

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


            // Reset UI after completion
            ExtractionProgress.Visibility = Visibility.Hidden;
        }
    }



    private void PathDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
    {

    }
}
