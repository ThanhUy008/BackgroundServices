namespace Shared.ConfigurateObjects;

public class KafkaConsumerSettings
{
    public string BootstrapServers { get; set; }
    public int MessageMaxBytes { get; set; }
    public string ConsumerGroupId { get; set; }
    public string AutoOffsetReset { get; set; }
    public bool AutoCommit { get; set; }
    public int FetchMaxBytes { get; set; }
    public int MaxPollRecords { get; set; }
    public bool AutoOffsetStore { get; set; }
}
