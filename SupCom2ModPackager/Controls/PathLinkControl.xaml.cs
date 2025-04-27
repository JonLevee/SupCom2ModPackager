using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SupCom2ModPackager.Extensions;
using SupCom2ModPackager.Utility;

namespace SupCom2ModPackager.Controls
{
    /// <summary>
    /// Interaction logic for PathLinkControl.xaml
    /// </summary>
    public partial class PathLinkControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly SharedData sharedData;

        private string _path = string.Empty;
        public string Path
        {
            get => _path;
            set
            {
                if (!EqualityComparer<string>.Default.Equals(_path, value))
                {
                    _path = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Path)));
                    SetPath(value);
                }
            }
        }

        public string GetParentPath()
        {
            return (string)((Button)PathPanel.Children[PathPanel.Children.Count - 1]).Tag;
        }

        public PathLinkControl()
        {
            sharedData = ServiceLocator.GetRequiredService<SharedData>();
            sharedData.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(sharedData.CurrentPath))
                {
                    Path = sharedData.CurrentPath;
                }
            };
            InitializeComponent();
            PathPanel.Children.Clear();
        }


        private void SetPath(string newPath)
        {
            _path = newPath;
            var sb = new StringBuilder();
            var pathParts = newPath.Split(System.IO.Path.DirectorySeparatorChar);
            for (int i = 0; i < pathParts.Length; i++)
            {
                var pathPart = i == 0 ? pathParts[i] : "\\" + pathParts[i];
                sb.Append(pathPart);
                if (i >= PathPanel.Children.Count)
                {
                    var button = new Button
                    {
                        Content = pathPart,
                        Tag = sb.ToString(),
                    };
                    button.Click += PathButtonClicked;
                    PathPanel.Children.Add(button);
                }
                else
                {
                    var button = (Button)PathPanel.Children[i];
                    var content = (string)button.Content;
                    if (pathPart != content)
                    {
                        button.Content = pathPart;
                        button.Tag = sb.ToString();
                    }
                }
            }

            while (pathParts.Length < PathPanel.Children.Count)
            {
                var button = (Button)PathPanel.Children[PathPanel.Children.Count - 1];
                button.Click -= PathButtonClicked;
                PathPanel.Children.RemoveAt(PathPanel.Children.Count - 1);
            }
        }

        private void PathButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Path = (string)((Button)sender).Tag;
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) // Check for double-click
            {
                Process.Start("explorer.exe", Path);
            }
        }
    }
}
