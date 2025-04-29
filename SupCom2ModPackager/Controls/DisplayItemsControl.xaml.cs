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
        private readonly SC2ModPackager _modPackager = ServiceLocator.GetRequiredService<SC2ModPackager>();
        private readonly DocumentToHtmlConverter documentToHtmlConverter = ServiceLocator.GetRequiredService<DocumentToHtmlConverter>();
        private readonly SharedData sharedData = ServiceLocator.GetRequiredService<SharedData>();
        private readonly SteamInfo steamInfo = ServiceLocator.GetRequiredService<SteamInfo>();

        private readonly DisplayItemCollection _items = ServiceLocator.GetRequiredService<DisplayItemCollection>();
        public DisplayItemCollection Items { get => _items; }

        public DisplayItemsControl()
        {

            InitializeComponent();
            //PathDataGrid.ItemsSource = _items;
            var collectionView = CollectionViewSource.GetDefaultView(PathDataGrid.ItemsSource);
            if (collectionView != null)
            {
                collectionView.SortDescriptions.Clear();
                collectionView.SortDescriptions.Add(new SortDescription(nameof(IDisplayItem.NameSort), ListSortDirection.Ascending));
                collectionView.Refresh();
            }

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

        private void PathDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (PathDataGrid.SelectedItem is IDisplayItem selectedItem)
            {
                switch (e.Key)
                {
                    case Key.Delete:
                        HandleDelete(selectedItem);
                        e.Handled = true;
                        break;

                    case Key.X when Keyboard.Modifiers.HasFlag(ModifierKeys.Control):
                        HandleCut(selectedItem);
                        e.Handled = true;
                        break;

                    case Key.C when Keyboard.Modifiers.HasFlag(ModifierKeys.Control):
                        HandleCopy(selectedItem);
                        e.Handled = true;
                        break;

                    case Key.V when Keyboard.Modifiers.HasFlag(ModifierKeys.Control):
                        HandlePaste();
                        e.Handled = true;
                        break;
                }
            }
        }

        private void HandleDelete(IDisplayItem selectedItem)
        {
            if (MessageBox.Show($"Are you sure you want to delete '{selectedItem.Name}'?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (File.Exists(selectedItem.FullPath))
                {
                    File.Delete(selectedItem.FullPath);
                }
                else if (Directory.Exists(selectedItem.FullPath))
                {
                    Directory.Delete(selectedItem.FullPath, true);
                }

                // Refresh the grid
                Items.Load();
            }
        }

        private IDisplayItem? _cutItem;

        private void HandleCut(IDisplayItem selectedItem)
        {
            _cutItem = selectedItem;
        }

        private void HandleCopy(IDisplayItem selectedItem)
        {
            Clipboard.SetFileDropList(new System.Collections.Specialized.StringCollection { selectedItem.FullPath });
        }

        private void HandlePaste()
        {
            if (_cutItem != null)
            {
                var targetPath = Path.Combine(sharedData.CurrentPath, Path.GetFileName(_cutItem.FullPath));

                if (File.Exists(_cutItem.FullPath))
                {
                    File.Move(_cutItem.FullPath, targetPath);
                }
                else if (Directory.Exists(_cutItem.FullPath))
                {
                    Directory.Move(_cutItem.FullPath, targetPath);
                }

                _cutItem = null; // Clear the cut item
            }
            else if (Clipboard.ContainsFileDropList())
            {
                var files = Clipboard.GetFileDropList();
                foreach (string file in files)
                {
                    var targetPath = Path.Combine(sharedData.CurrentPath, Path.GetFileName(file));

                    if (File.Exists(file))
                    {
                        File.Copy(file, targetPath, overwrite: false);
                    }
                    else if (Directory.Exists(file))
                    {
                        CopyDirectory(file, targetPath);
                    }
                }
            }

            // Refresh the grid
            Items.Load();
        }

        private void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), overwrite: false);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                CopyDirectory(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
            }
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

        private void PathDataGrid_DragEnter(object sender, DragEventArgs e)
        {
            // Check if the data being dragged is a file or folder
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy; // Show the copy cursor
            }
            else
            {
                e.Effects = DragDropEffects.None; // Show the "not allowed" cursor
            }
        }

        private void PathDataGrid_Drop(object sender, DragEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // Get the dropped files/folders
                    var droppedPaths = (string[])e.Data.GetData(DataFormats.FileDrop);

                    foreach (var path in droppedPaths)
                    {
                        if (File.Exists(path))
                        {
                            var target = Path.Combine(sharedData.CurrentPath, Path.GetFileName(path));
                            if (!File.Exists(target))
                            {
                                // Add the file to the collection
                                File.Copy(path, Path.Combine(sharedData.CurrentPath, Path.GetFileName(path)), false);
                            }
                        }
                        else if (Directory.Exists(path))
                        {
                            // Add the directory to the collection
                            void RecursiveDirectoryCopy(string source, string target)
                            {
                                if (Directory.Exists(source))
                                {
                                    Directory.CreateDirectory(target);
                                    foreach (var file in Directory.GetFiles(source))
                                    {
                                        var targetFile = Path.Combine(target, Path.GetFileName(file));
                                        if (!File.Exists(targetFile))
                                            // Copy the file to the target directory
                                            File.Copy(file, Path.Combine(target, Path.GetFileName(file)), false);
                                    }
                                    foreach (var directory in Directory.GetDirectories(source))
                                    {
                                        RecursiveDirectoryCopy(directory, Path.Combine(target, Path.GetFileName(directory)));
                                    }
                                }
                            }
                            RecursiveDirectoryCopy(path, Path.Combine(sharedData.CurrentPath, Path.GetFileName(path)));
                        }
                    }
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void PathDataGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Get the selected item
                if (PathDataGrid.SelectedItem is IDisplayItem selectedItem)
                {
                    // Start a drag-and-drop operation
                    var data = new DataObject(DataFormats.FileDrop, new[] { selectedItem.FullPath });
                    DragDrop.DoDragDrop(PathDataGrid, data, DragDropEffects.Copy);
                }
            }
        }


        private void SetPath()
        {
            SetCurrentFolderBar();
            Items.Load();
        }

        private void SetCurrentFolderBar()
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
        }
    }

}
