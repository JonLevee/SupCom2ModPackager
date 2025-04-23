using System;
using System.Collections.Generic;
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
using SupCom2ModPackager.Collections;
using SupCom2ModPackager.Utility;

namespace SupCom2ModPackager.Controls
{
    /// <summary>
    /// Interaction logic for SourcesViewControl.xaml
    /// </summary>
    public partial class SourcesViewControl : UserControl
    {
        private SourceItemCollection Items;
        public SourcesViewControl()
        {
            Items = ServiceLocator.GetService<SourceItemCollection>() ?? SourceItemCollection.Empty;
            foreach (var item in Items)
            {
                //string text = GoogleDriveFolderLister.GetFolderContentsAsJson(item.SourcePath).Result;
            }
            InitializeComponent();
        }
    }
}
