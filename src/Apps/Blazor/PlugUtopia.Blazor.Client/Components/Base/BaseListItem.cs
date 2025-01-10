namespace PlugUtopia.Blazor.Client.Components.Base;

public class BaseListItem<TItem> 
{
    public BaseListItem(TItem item)
    {
        Item = item;
    }
    public TItem Item { get; set; }
    public bool ShowDetails { get; set; }
}