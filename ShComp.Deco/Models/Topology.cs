namespace ShComp.Deco.Models;

internal class Topology
{
    [JsonPropertyName("auto")]
    public bool Auto { get; set; }

    [JsonPropertyName("device_id")]
    public string? DeviceId { get; set; }
}
