using SupCom2ModPackager.Collections;
using SupCom2ModPackager.Extensions;
using System.IO;

namespace SupCom2ModPackager.Models;

public class DisplayItemDirectory : DisplayItem
{
    public static readonly DisplayItemDirectory Bogus = new(DisplayItemCollection.Empty, new DirectoryInfo(@"C:\Bogus"));
    private readonly DirectoryInfo info;

    public override string Name => info.Name;
    public override string NameSort => info.Name + "\t";
    public override string FullPath => info.FullName;
    public override DateTime Modified => info.LastWriteTime;
    public override bool Exists => info.Exists;

    public DisplayItemDirectory(DisplayItemCollection collection, DirectoryInfo info) : base(collection)
    {
        this.info = info;
    }
}

