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

    public DisplayItemFile(DisplayItemCollection collection, FileInfo info) : base()
    {
        this.info = info;
    }
}


