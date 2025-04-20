
namespace SupCom2ModPackager;

public class SC2ModPackager
{
    private readonly DisplayItemCollection items;

    public SC2ModPackager(DisplayItemCollection items)
    {
        this.items = items;
    }

    public async Task Unpack(DisplayItem item, IProgress<string> progress)
    {
        throw new NotImplementedException();
    }
}