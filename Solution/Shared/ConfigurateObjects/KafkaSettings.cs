namespace Shared.ConfigurateObjects;

public class KafkaSettings
{
    public string BootstrapServers { get; set; }
    public int MessageMaxBytes { get; set; }
    public string Ack { get; set; }
    public bool EnableIdempotence { get; set; }
    public int MessageSendMaxRetries { get; set; }
    public int MessageTimeoutMs { get; set; }
}
