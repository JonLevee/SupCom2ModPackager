using System.Windows.Controls;
using SupCom2ModPackager.Utility;
using System.Windows.Input;
using SupCom2ModPackager.Extensions;
using SupCom2ModPackager.Models;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;
using SupCom2ModPackager.Collections;
using Vis = System.Windows.Visibility;
using System.Windows.Media;
using System.Windows;
using System.Diagnostics;
using System.Text;

namespace SupCom2ModPackager.Controls
{
    /// <summary>
    /// Interaction logic for DisplayItemsControl.xaml
    /// </summary>
    public partial class DisplayItemsControl : UserControl
    {

        private readonly DisplayItemCollection _items = ServiceLocator.GetRequiredService<DisplayItemCollection>();
        private readonly SC2ModPackager _modPackager = ServiceLocator.GetRequiredService<SC2ModPackager>();
        private readonly SupCom2ModPackagerSettings settings = ServiceLocator.GetRequiredService<SupCom2ModPackagerSettings>();
        private readonly DocumentToHtmlConverter documentToHtmlConverter = ServiceLocator.GetRequiredService<DocumentToHtmlConverter>();
        private readonly SharedData sharedData = ServiceLocator.GetRequiredService<SharedData>();
        private readonly SteamInfo steamInfo = ServiceLocator.GetRequiredService<SteamInfo>();

        public DisplayItemsControl()
        {

            InitializeComponent();


            PathDataGrid.ItemsSource = _items;
            var collectionView = CollectionViewSource.GetDefaultView(PathDataGrid.ItemsSource);
            collectionView.SortDescriptions.Clear();
            collectionView.SortDescriptions.Add(new SortDescription(nameof(IDisplayItem.NameSort), ListSortDirection.Ascending));
            collectionView.Refresh();

            PathDataGrid.SelectionChanged += CommandButtonsVisibilityHandler;
            PathDataGrid.SelectionChanged += ContentDisplayHandler;
            CommandUnpack.Visibility = Vis.Collapsed;
            CommandPack.Visibility = Vis.Collapsed;

            sharedData.CurrentPathChanged += (sender, e) =>
            {
                SetPath();
            };
        }


        private async void CommandUnpack_Click(object sender, RoutedEventArgs e)
        {
            var fileItem = (DisplayItemFile)PathDataGrid.SelectedItem;
            var progress = new Progress<string>(text =>
            {
                if (text != null)
                {
                    fileItem.StatusTextVisible = Vis.Visible;
                    fileItem.ColumnsVisible = Vis.Collapsed;
                    fileItem.ProgressVisible = Vis.Collapsed;
                    fileItem.StatusText = text;
                    return;
                }
                ++fileItem.ProgressValue;
            });

            await _modPackager.UnpackAsync(fileItem, true, progress);
        }

        private void CommandPack_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void CommandButtonsVisibilityHandler(object sender, SelectionChangedEventArgs e)
        {
            CommandUnpack.Visibility = Vis.Collapsed;
            CommandPack.Visibility = Vis.Collapsed;
            if (PathDataGrid.SelectedItem is DisplayItemFile fileItem)
            {
                CommandUnpack.Visibility = fileItem.FullPath.IsCompressedFile() ? Vis.Visible : Vis.Collapsed;
            }
            else if (PathDataGrid.SelectedItem is DisplayItemDirectoryParent directoryItem)
            {
                CommandPack.Visibility = directoryItem.FullPath.IsSupCom2Directory() ? Vis.Visible : Vis.Collapsed;
            }
        }

        private void ContentDisplayHandler(object sender, SelectionChangedEventArgs e)
        {
            var updatedContent = "<html/>";
            if (PathDataGrid.SelectedItem is DisplayItemFile fileItem)
            {
                if (documentToHtmlConverter.TryConvert(Path.GetExtension(fileItem.Name), () => File.ReadAllText(fileItem.FullPath), out var convertedContent))
                {
                    updatedContent = convertedContent;
                }
            }
            ContentDisplay.NavigateToString(updatedContent);
        }

        private void PathDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            var collectionView = CollectionViewSource.GetDefaultView(PathDataGrid.ItemsSource);

            var sortDescription = collectionView.SortDescriptions.Single();
            var sortDescriptionDirection = sortDescription.Direction;

            var newSortDirection = sortDescription.Direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

            var parentDirectory = PathDataGrid.ItemsSource.Cast<IDisplayItem>().Single(x => x is DisplayItemDirectoryParent) as DisplayItemDirectoryParent;
            parentDirectory!.SortDirection = newSortDirection;

            collectionView.SortDescriptions.Clear();

            collectionView.SortDescriptions.Add(new SortDescription(e.Column.SortMemberPath, newSortDirection));
            collectionView.Refresh();
        }

        private void ActionClicked(object sender, MouseButtonEventArgs e)
        {
            //if (!((DataGrid)sender).TryGetDisplayItem(e, out string column, out DisplayItem item))
            //    return;

            //if (item.Action == "UnpackAsync_delete")
            //{
            //    var fileItem = (DisplayItemFile)item;

            //    ExtractionProgressBar.Value = -1;
            //    ExtractionProgressBar.Maximum = 0;
            //    CurrentFileTextBlock.Text = string.Empty;

            //    // Show the progress overlay
            //    ExtractionProgress.Visibility = Vis.Visible;
            //    // Create a progress reporter
            //    var progress = new Progress<PackProgressArgs>(args =>
            //    {
            //        if (args.Text != null)
            //            CurrentFileTextBlock.Text = args.Text;
            //        if (args.Value != null)
            //            ExtractionProgressBar.Value = args.Value.Value;
            //        else
            //            ++ExtractionProgressBar.Value;
            //        if (args.Maximum != null)
            //            ExtractionProgressBar.Maximum = args.Maximum.Value;
            //    });


            //    var overWrite = false;
            //    if (Directory.Exists(fileItem.UnpackDirectoryPath))
            //    {
            //        //var result = MessageBox.Show("Overwrite directory?", "UnpackAsync_delete will overwrite existing directory", MessageBoxButton.OKCancel);
            //        //if (result == MessageBoxResult.OK)
            //        //{
            //        overWrite = true;
            //        //}

            //    }

            //    await _modPackager.UnpackAsync_delete(fileItem, overWrite, progress);


            //    // Reset UI after completion
            //    ExtractionProgress.Visibility = Vis.Hidden;
            //}
        }

        private void OpenCurrentDirectoryButton(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", sharedData.CurrentPath);
        }

        private void OpenGamedataButton(object sender, RoutedEventArgs e)
        {
            var root = steamInfo.GetRoot("Supreme Commander 2");
            var gamedata = Path.Combine(root, "gamedata");
            Process.Start("explorer.exe", gamedata);
        }

        private void PathButtonClicked(object sender, RoutedEventArgs e)
        {
            sharedData.CurrentPath = (string)((Button)sender).Tag;
        }

        private void SetPath()
        {
            var sb = new StringBuilder();
            var pathParts = sharedData.CurrentPath.Split(Path.DirectorySeparatorChar);
            PathPanel.Children.Cast<TextBlock>().ForEach(child => child.MouseLeftButtonUp -= PathButtonClicked);
            foreach (FrameworkElement child in PathPanel.Children)
            {
                if (child.Tag != null)
                {
                    child.MouseLeftButtonUp -= PathButtonClicked;
                }
            }

            PathPanel.Children.Clear();
            for (int i = 0; i < pathParts.Length; i++)
            {
                var pathPart = pathParts[i];
                if (i == 0)
                {
                    sb.Append(pathPart);
                }
                else
                {
                    PathPanel.Children.Add(new TextBlock { Text = ">" });
                    sb.Append(Path.DirectorySeparatorChar);
                    sb.Append(pathPart);
                }
                PathPanel.Children.Add(new TextBlock { Text = pathPart, Tag = sb.ToString() });
            }
            _items.Load();
        }
    }

}
