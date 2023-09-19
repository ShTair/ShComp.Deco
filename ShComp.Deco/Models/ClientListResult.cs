namespace ShComp.Deco.Models;

internal class ClientListResult
{
    [JsonPropertyName("client_list")]
    public Client[]? Clients { get; set; }
}
