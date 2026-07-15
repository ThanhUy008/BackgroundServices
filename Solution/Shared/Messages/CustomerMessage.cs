namespace Shared.Messages;

[Channel("customer-changes")]
public class CustomerMessage
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedOn { get; set; }
    public object[] Items { get; set; }
}
