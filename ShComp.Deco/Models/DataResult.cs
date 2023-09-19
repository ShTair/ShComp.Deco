using System.Text.Json.Serialization;

namespace ShComp.Deco.Models;

internal class DataResult
{
    [JsonPropertyName("data")]
    public string? Data { get; set; }
}
