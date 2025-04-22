using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SupCom2ModPackager.Models;

namespace SupCom2ModPackager.ViewModels
{
    internal class DisplayItemCollectionViewModel : ObservableCollection<DisplayItemView>
    {
        public static readonly DisplayItemCollectionViewModel Empty = new DisplayItemCollectionViewModel();
        public DisplayItemCollectionViewModel()
        {
        }
        public DisplayItemCollectionViewModel(IEnumerable<DisplayItem> items) : base(items.Select(i => new DisplayItemView(i)))
        {
        }
    }

}
internal class DisplayItemView
{
    public string Name { get; set; }
    public string FullPath { get; set; }
    public DateTime Modified { get; set; }

    public DisplayItemView(DisplayItem item)
    {
        Name = item.Name;
        FullPath = item.FullPath;
        Modified = item.Modified;
    }
}
