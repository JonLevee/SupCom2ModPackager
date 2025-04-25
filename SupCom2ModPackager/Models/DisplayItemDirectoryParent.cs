using SupCom2ModPackager.Collections;
using SupCom2ModPackager.Extensions;
using System.ComponentModel;
using System.DirectoryServices;
using System.IO;

namespace SupCom2ModPackager.Models;

public class DisplayItemDirectoryParent : DisplayItemDirectory
{
    public new static readonly DisplayItemDirectoryParent Empty = new(DisplayItemCollection.Empty, new DirectoryInfo(GeneralExtensions.GetValidDrives().First()));
    private readonly DirectoryInfo info;

    public override string Name => "...";
    public override string NameSort => SortDirection == ListSortDirection.Ascending ? " " : "zzz";
    public override DateTime ModifiedSort => SortDirection == ListSortDirection.Ascending ? DateTime.MinValue : DateTime.MaxValue;
    public override string ActionSort => SortDirection == ListSortDirection.Ascending ? " " : "zzz";

    public ListSortDirection SortDirection { get; set; } = ListSortDirection.Ascending;

    public DisplayItemDirectoryParent(DisplayItemCollection collection, DirectoryInfo info) : base(collection, info)
    {
        this.info = info;
    }

    protected override string GetAction()
    {
        return string.Empty;
    }
}

