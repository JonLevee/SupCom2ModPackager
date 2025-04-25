using SupCom2ModPackager.Collections;
using SupCom2ModPackager.Extensions;
using System.ComponentModel;
using System.DirectoryServices;
using System.IO;

namespace SupCom2ModPackager.Models;

public class DisplayItemDirectoryParent : DisplayItem
{
    public static new readonly DisplayItemDirectoryParent Empty = new(DisplayItemCollection.Empty, null!);
    private readonly DirectoryInfo info;

    public override string Name => "...";
    public override string NameSort => SortDirection == ListSortDirection.Ascending ? " " : "zzz";
    public override string FullPath => info.FullName;
    public override DateTime Modified => info.LastWriteTime;
    public override DateTime ModifiedSort => SortDirection == ListSortDirection.Ascending ? DateTime.MinValue : DateTime.MaxValue;
    public override bool Exists => info.Exists;


    public ListSortDirection SortDirection { get; set; } = ListSortDirection.Ascending;

    public DisplayItemDirectoryParent(DisplayItemCollection collection, DirectoryInfo info) : base(collection)
    {
        this.info = info;
    }
}

