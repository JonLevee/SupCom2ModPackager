using SupCom2ModPackager.Extensions;
using System.IO;

namespace SupCom2ModPackager.DisplayItemClass;

public class DisplayItemDirectoryParent : DisplayItem
{
    public new static readonly DisplayItemDirectoryParent Empty = new(DisplayItemCollection.Empty, new DirectoryInfo(GeneralExtensions.GetValidDrives().First()));
    private readonly DirectoryInfo info;

    public override string Name => "...";
    public override string FullPath => info.FullName;
    public override DateTime Modified => info.LastWriteTime;
    public override bool Exists => info.Exists;

    public DisplayItemDirectoryParent(DisplayItemCollection collection, DirectoryInfo info) : base(collection)
    {
        this.info = info;
    }

    protected override string GetAction()
    {
        return string.Empty;
    }
}

