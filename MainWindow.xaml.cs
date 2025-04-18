using SupCom2ModPackager.Extensions;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SupCom2ModPackager;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private DisplayItemCollection _items = new DisplayItemCollection();
    public MainWindow()
    {
        InitializeComponent();
        PathDataGrid.ItemsSource = _items;
        PathDataGrid.CanUserSortColumns = true;
        PathLink.PathChanged += PathLink_PathChanged;
        PathLink.Path = @"C:\SC2Mods\Testing";
    }

    private void PathLink_PathChanged(object? sender, string newPath)
    {
        _items.Clear();
        _items.Add(new DisplayItem(DisplayItemType.Directory, newPath, "..."));
        foreach (var dir in Directory.GetDirectories(newPath))
        {
            _items.Add(new DisplayItem(DisplayItemType.Directory, dir));
        }
        foreach (var file in Directory.GetFiles(newPath))
        {
            _items.Add(new DisplayItem(DisplayItemType.File, file));
        }
    }

    private void ActionClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DataGrid myGrid = (DataGrid)sender;
        // Get the mouse position relative to the DataGrid
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

        var row = (DataGridRow)((TextBlock)element).BindingGroup.Owner;
        var item = (DisplayItem)row.Item;
        if (item.Action == "Unpack")
        {
             var directory = Path.ChangeExtension(item.Path, null);
            if (Directory.Exists(directory))
            {
                var result = MessageBox.Show("Overwrite directory?", "Unpack will overwrite existing directory", MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
                Directory.Delete(directory, true);
            }
            // Handle unpack action
            using (var zipFile = ZipFile.OpenRead(item.Path))
            {

            }
            return;
        }
    }
}