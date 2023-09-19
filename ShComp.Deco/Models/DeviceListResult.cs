namespace ShComp.Deco.Models;

internal class DeviceListResult
{
    [JsonPropertyName("device_list")]
    public Device[]? Devices { get; set; }
}
