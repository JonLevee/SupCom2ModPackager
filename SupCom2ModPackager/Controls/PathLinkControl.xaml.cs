﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace SupCom2ModPackager.Controls
{
    /// <summary>
    /// Interaction logic for PathLinkControl.xaml
    /// </summary>
    public partial class PathLinkControl : UserControl
    {
        private string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                SetPath(_path);
            }
        }

        public string GetParentPath()
        {
            return (string)((Button)PathPanel.Children[PathPanel.Children.Count - 1]).Tag;
        }

        public event EventHandler<string>? PathChanged;

        public PathLinkControl()
        {
            InitializeComponent();
            PathPanel.Children.Clear();
            _path = string.Empty;
        }


        private void SetPath(string newPath)
        {
            var sb = new StringBuilder();
            var changed = false;
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
                    changed = true;
                } else
                {
                    var button = (Button)PathPanel.Children[i];
                    var content = (string)button.Content;
                    if (pathPart != content)
                    {
                        button.Content = pathPart;
                        button.Tag = sb.ToString();
                        changed = true;
                    }
                }
            }

            while (pathParts.Length < PathPanel.Children.Count)
            {
                var button = (Button)PathPanel.Children[PathPanel.Children.Count - 1];
                button.Click -= PathButtonClicked;
                PathPanel.Children.RemoveAt(PathPanel.Children.Count - 1);
                changed = true;
            }

            if (changed)
            {
                PathChanged?.Invoke(this, newPath);
            }
        }

        private void PathButtonClicked(object sender, RoutedEventArgs e)
        {
            var newPath = (string)((Button)sender).Tag;
            SetPath(newPath);
        }

    }
}
