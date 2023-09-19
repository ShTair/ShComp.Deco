namespace ShComp.Deco.Models;

internal class SignalLevel
{
    [JsonPropertyName("band2_4")]
    public string? Band2_4 { get; set; }

    [JsonPropertyName("band5")]
    public string? Band5 { get; set; }

    [JsonPropertyName("band6")]
    public string? Band6 { get; set; }
}
