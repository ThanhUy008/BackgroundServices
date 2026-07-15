namespace Shared.Messages;

[Channel("item-changes")]
public class ItemMessage
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedOn { get; set; }
}
