using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using SupCom2ModPackager.DisplayItemClass;
using SupCom2ModPackager.Utility;
using System.Windows.Input;
using SupCom2ModPackager.Extensions;
using SupCom2ModPackager.Models;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;

namespace SupCom2ModPackager.Controls
{
    /// <summary>
    /// Interaction logic for DisplayItemsControl.xaml
    /// </summary>
    public partial class DisplayItemsControl : UserControl
    {
        private readonly DisplayItemCollection _items;
        private readonly SC2ModPackager _modPackager;
        public ICommand ActionCommand { get; }


        public DisplayItemsControl()
        {
            _items = ServiceLocator.GetService<DisplayItemCollection>() ?? DisplayItemCollection.Empty;
            _modPackager = ServiceLocator.GetService<SC2ModPackager>() ?? SC2ModPackager.Empty;
            ActionCommand = new RelayCommand(ExecuteActionCommand, CanExecuteActionCommand);

            InitializeComponent();
            ExtractionProgress.Visibility = System.Windows.Visibility.Hidden;
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
                ExtractionProgress.Visibility = System.Windows.Visibility.Visible;
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
                ExtractionProgress.Visibility = System.Windows.Visibility.Hidden;
            }
        }



        private void PathDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {

        }

    }
}

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _execute(parameter);

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
