using System.IO;
using SupCom2ModPackager.Extensions;

namespace SupCom2ModPackager.DisplayItemClass;

public class DisplayItemFile : DisplayItem
{
    public new static readonly DisplayItemFile Empty = new(DisplayItemCollection.Empty, new FileInfo(GeneralExtensions.GetValidDrives().First()));
    private readonly FileInfo info;

    public override string Name => info.Name;
    public override string FullPath => info.FullName;
    public override DateTime Modified => info.LastWriteTime;
    public override bool Exists => info.Exists;

    public string UnpackDirectoryPath => Path.ChangeExtension(FullPath, null);

    public DisplayItemDirectory UnpackDirectory
    {
        get
        {
            var unpackDirectoryPath = UnpackDirectoryPath;
            var targetDirectory = collection
                .Where(item => item is DisplayItemDirectory)
                .Cast<DisplayItemDirectory>()
                .FirstOrDefault(dirItem => string.Equals(unpackDirectoryPath, dirItem.FullPath, StringComparison.OrdinalIgnoreCase));
            return targetDirectory ?? DisplayItemDirectory.Empty;
        }
    }


    public DisplayItemFile(DisplayItemCollection collection, FileInfo info) : base(collection)
    {
        this.info = info;
    }

    protected override string GetAction()
    {
        return string.Equals(".sc2", System.IO.Path.GetExtension(this.FullPath), StringComparison.OrdinalIgnoreCase)
            ? "UnpackAsync"
            : string.Empty;
    }
}


