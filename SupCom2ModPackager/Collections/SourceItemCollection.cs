using System.Collections.ObjectModel;
using SupCom2ModPackager.Models;
using static System.Net.WebRequestMethods;

namespace SupCom2ModPackager.Collections
{
    public class SourceItemCollection : ObservableCollection<SourceItem>
    {
        public static readonly SourceItemCollection Empty = new SourceItemCollection();
        public SourceItemCollection()
        {
            Add(new SourceItem
            {
                SourceType = SourceType.GoogleDrive,
                FriendlyName = "Super Secret",
                SourcePath = "https://drive.google.com/drive/u/0/folders/1cRHLoBGw5f5eDX3vc-ptDgt7SCLVyeVJ"
            });
            Add(new SourceItem
            {
                SourceType = SourceType.GoogleDrive,
                FriendlyName = "SupCom2Mods",
                SourcePath = "https://drive.google.com/drive/folders/13YFudVkPF9Id0mdn2JCtaTPwSCbv5jp7?usp=sharing"
            });
        }
    }
}
