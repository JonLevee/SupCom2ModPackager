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
using Microsoft.Extensions.FileSystemGlobbing;

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
        private readonly FileSystemWatcher _fileSystemWatcher;
        public ICommand ActionCommand { get; }


        public DisplayItemsControl()
        {
            ActionCommand = new RelayCommand(ExecuteActionCommand, CanExecuteActionCommand);

            InitializeComponent();

            _fileSystemWatcher = new FileSystemWatcher
            {
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
                Filter = "*",
                EnableRaisingEvents = false
            };
            _fileSystemWatcher.Created += OnCreated;
            _fileSystemWatcher.Deleted += OnDeleted;
            _fileSystemWatcher.EnableRaisingEvents = false;

            PropertySyncManager.Sync(_items, PathLink, x => x.Path, x => x.Path);

            ExtractionProgress.Visibility = Vis.Hidden;
            ExtractionProgressBar.Value = -1;
            ExtractionProgressBar.Minimum = 0;
            ExtractionProgressBar.Maximum = 0;
            CurrentFileTextBlock.Text = string.Empty;

            PathDataGrid.ItemsSource = _items;
            var collectionView = CollectionViewSource.GetDefaultView(PathDataGrid.ItemsSource);
            collectionView.SortDescriptions.Clear();
            collectionView.SortDescriptions.Add(new SortDescription(nameof(IDisplayItem.NameSort), ListSortDirection.Ascending));
            collectionView.Refresh();

            PathDataGrid.SelectionChanged += PathDataGrid_SelectionChanged;
            CommandUnpack.Visibility = Vis.Collapsed;
            CommandPack.Visibility = Vis.Collapsed;

            _items.Path = settings.InstalledModsFolder;
        }

        private void PathDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void PathDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true; // Prevent default sorting behavior


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

        private void ExecuteActionCommand(object? parameter)
        {
            // Handle the command logic here
            //MessageBox.Show($"ActionCommand executed with parameter: {parameter}");
        }

        private bool CanExecuteActionCommand(object? parameter)
        {
            // Return true if the command can execute, otherwise false
            return true;
        }

        private async void ActionClicked(object sender, MouseButtonEventArgs e)
        {
            //if (!((DataGrid)sender).TryGetDisplayItem(e, out string column, out DisplayItem item))
            //    return;

            //if (item.Action == "UnpackAsync")
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
            //        //var result = MessageBox.Show("Overwrite directory?", "UnpackAsync will overwrite existing directory", MessageBoxButton.OKCancel);
            //        //if (result == MessageBoxResult.OK)
            //        //{
            //        overWrite = true;
            //        //}

            //    }

            //    await _modPackager.UnpackAsync(fileItem, overWrite, progress);


            //    // Reset UI after completion
            //    ExtractionProgress.Visibility = Vis.Hidden;
            //}
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            _items.TryRemove(e.FullPath, out _);
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (File.Exists(e.FullPath))
            {
                _items.Add(new FileInfo(e.FullPath));
                return;
            }
            if (Directory.Exists(e.FullPath))
            {
                _items.Add(new DirectoryInfo(e.FullPath));
                return;
            }
            throw new InvalidOperationException($"File system watcher created event for {e.FullPath} but it is not a file or directory");
        }

        private void CommandUnpack_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Guard.Requires(PathDataGrid.SelectedItem is DisplayItemFile);
            var fileItem = (DisplayItemFile)PathDataGrid.SelectedItem;
            /*
             * add check if directory already exists ... no unpack button?
             * add action delete on file and directory
             * add progress bar control
             */
            foo
        }

        private void CommandPack_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
