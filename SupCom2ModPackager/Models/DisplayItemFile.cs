using System.IO;
using SupCom2ModPackager.Collections;

namespace SupCom2ModPackager.Models;

public class DisplayItemFile : DisplayItem
{
    private readonly FileInfo info;

    public override string Name => info.Name;
    public override string NameSort => Path.GetFileNameWithoutExtension(Name) + "\t" + Path.GetExtension(Name);

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
            return targetDirectory ?? DisplayItemDirectory.Bogus;
        }
    }


    public DisplayItemFile(DisplayItemCollection collection, FileInfo info) : base(collection)
    {
        this.info = info;
    }
}


