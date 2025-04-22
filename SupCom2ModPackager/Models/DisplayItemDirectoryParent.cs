using SupCom2ModPackager.DisplayItemClass;
using SupCom2ModPackager.Extensions;
using System.IO;

namespace SupCom2ModPackager.Models;

public class DisplayItemDirectoryParent : DisplayItemDirectory
{
    public new static readonly DisplayItemDirectoryParent Empty = new(DisplayItemCollection.Empty, new DirectoryInfo(GeneralExtensions.GetValidDrives().First()));
    private readonly DirectoryInfo info;

    public override string Name => "...";

    public DisplayItemDirectoryParent(DisplayItemCollection collection, DirectoryInfo info) : base(collection, info)
    {
        this.info = info;
    }

    protected override string GetAction()
    {
        return string.Empty;
    }
}

