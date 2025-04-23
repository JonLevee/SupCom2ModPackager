using System.Collections.ObjectModel;
using SupCom2ModPackager.Models;

internal class SourceItemView
{
    private readonly SourceItem item;

    public string FriendlyName { get; set; }
    public string SourcePath { get; set; }

    public SourceItemView(SourceItem item)
    {
        FriendlyName = item.FriendlyName;
        SourcePath = item.SourcePath;
        this.item = item;
    }
}

internal class SourceItemCollectionView : ObservableCollection<SourceItemView>
{
    public static readonly SourceItemCollectionView Empty = new SourceItemCollectionView();
    public SourceItemCollectionView()
    {
    }
    public SourceItemCollectionView(IEnumerable<SourceItem> items) : base(items.Select(i => new SourceItemView(i)))
    {
    }
}
