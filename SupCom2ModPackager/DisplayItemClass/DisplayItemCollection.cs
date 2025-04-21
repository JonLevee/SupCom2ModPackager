using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace SupCom2ModPackager.DisplayItemClass;

public class DisplayItemCollection : ObservableCollection<DisplayItem>
{
    public static readonly DisplayItemCollection Empty = new();


    public DisplayItemCollection()
    {
    }

    public DisplayItemFile Add(FileInfo info)
    {
        var item = new DisplayItemFile(this, info);
        Add(item);
        return item;
    }

    public DisplayItemDirectory Add(DirectoryInfo info)
    {
        var item = new DisplayItemDirectory(this, info);
        Add(item);
        return item;
    }

    public DisplayItemDirectoryParent AddParent(DirectoryInfo info)
    {
        var item = new DisplayItemDirectoryParent(this, info);
        Add(item);
        return item;
    }
}


