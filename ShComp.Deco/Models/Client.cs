using System.Text;

namespace ShComp.Deco.Models;

internal class Client
{
    [JsonPropertyName("mac")]
    public string? Mac { get; set; }

    [JsonPropertyName("up_speed")]
    public int UpSpeed { get; set; }

    [JsonPropertyName("down_speed")]
    public int DownSpeed { get; set; }

    [JsonPropertyName("wire_type")]
    public string? WireType { get; set; }

    [JsonPropertyName("access_host")]
    public string? AccessHost { get; set; }

    [JsonPropertyName("connection_type")]
    public string? ConnectionType { get; set; }

    [JsonPropertyName("space_id")]
    public string? SpaceId { get; set; }

    [JsonPropertyName("ip")]
    public string? IP { get; set; }

    [JsonPropertyName("client_mesh")]
    public bool ClientMesh { get; set; }

    [JsonPropertyName("online")]
    public bool Online { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("enable_priority")]
    public bool EnablePriority { get; set; }

    [JsonPropertyName("remain_time")]
    public int RemainTime { get; set; }

    [JsonPropertyName("owner_id")]
    public string? OwnerId { get; set; }

    [JsonPropertyName("client_type")]
    public string? ClientType { get; set; }

    [JsonPropertyName("interface")]
    public string? Interface { get; set; }


    public string? DecodedName => Name is null ? null : Encoding.UTF8.GetString(Convert.FromBase64String(Name));

    public override string ToString() => $"{Mac} {DecodedName}";
}
