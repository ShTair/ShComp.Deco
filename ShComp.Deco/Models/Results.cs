namespace ShComp.Deco.Models;

internal class PostResult<T>
{
    [JsonPropertyName("result")]
    public T? Result { get; set; }

    [JsonPropertyName("error_code")]
    public int ErrorCode { get; set; }
}

internal class KeysResult
{
    [JsonPropertyName("username")]
    public string? UserName { get; set; }

    [JsonPropertyName("password")]
    public string[]? Password { get; set; }
}

internal class SessionResult
{
    [JsonPropertyName("key")]
    public string[]? Key { get; set; }

    [JsonPropertyName("seq")]
    public int Seq { get; set; }
}

internal class LoginResult
{
    [JsonPropertyName("logined_host")]
    public string? LoginedHost { get; set; }

    [JsonPropertyName("stok")]
    public string? Stok { get; set; }

    [JsonPropertyName("logined_ip")]
    public string? LoginedIp { get; set; }

    [JsonPropertyName("logined_remote")]
    public bool? LoginedRemote { get; set; }

    [JsonPropertyName("logined_mac")]
    public string? LoginedMac { get; set; }

    [JsonPropertyName("logined_user")]
    public string? LoginedUser { get; set; }
}
