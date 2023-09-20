using ShComp.Deco.Models;
using System.Security.Cryptography;
using System.Text;

namespace ShComp.Deco;

public sealed class DecoClient : IDisposable
{
    private readonly UTF8Encoding _utf8 = new(false);
    private readonly string _readBody = JsonSerializer.Serialize(new { operation = "read" });

    private readonly DecoRestClient _client;

    private readonly string _aesKeyIV;
    private readonly Aes _aes;

    private string? _passwordHash;
    private RSA? _rsa;
    private int _sequence;
    private string? _stok;

    public DecoClient(string host)
    {
        _client = new DecoRestClient(host);

        var r = new Random();
        static string Int64(Random r) => r.NextInt64(10000000000000000).ToString("0000000000000000");
        var aesKey = Int64(r);
        var aesIV = Int64(r);
        _aesKeyIV = $"k={aesKey}&i={aesIV}";

        _aes = Aes.Create();
        _aes.Key = _utf8.GetBytes(aesKey);
        _aes.IV = _utf8.GetBytes(aesIV);
    }

    public void Dispose()
    {
        _rsa?.Dispose();
        _client.Dispose();
    }

    public async Task LoginAsync(string password)
    {
        _passwordHash = ToXString(MD5.HashData(_utf8.GetBytes($"admin{password}")));

        var passwordKey = await _client.PostAsync<PostResult<KeysResult>>(";stok=/login", "keys", _readBody);
        string encryptedPassword;
        using (var passwordRsa = CreateRsa(passwordKey.Result!.Password!))
        {
            encryptedPassword = ToXString(passwordRsa.Encrypt(_utf8.GetBytes(password), RSAEncryptionPadding.Pkcs1));
        }

        var sessionKey = await _client.PostAsync<PostResult<SessionResult>>(";stok=/login", "auth", _readBody);
        _rsa = CreateRsa(sessionKey.Result!.Key!);
        _sequence = sessionKey.Result!.Seq;

        var loginRequestBody = new { @params = new { password = encryptedPassword }, operation = "login" };
        var loginResult = await EncryptedPostAsync<LoginResult>(";stok=/login", "login", loginRequestBody);
        if (loginResult is not { Stok: { } stok }) throw new Exception("ログイン失敗");

        _stok = stok;
    }

    public async Task<Device[]> GetDevicesAsync()
    {
        var result = await EncryptedPostAsync<DeviceListResult>($";stok={_stok}/admin/device", "device_list", _readBody);
        return result?.Devices ?? Array.Empty<Device>();
    }

    public async Task<Client[]> GetClientsAsync(string deviceMac = "default")
    {
        var body = new { operation = "read", @params = new { device_mac = deviceMac } };
        var result = await EncryptedPostAsync<ClientListResult>($";stok={_stok}/admin/client", "client_list", body);
        return result?.Clients ?? Array.Empty<Client>();
    }

    #region Utils

    private static RSA CreateRsa(string[] parameters)
    {
        var rsaParameters = new RSAParameters
        {
            Exponent = FromXString(parameters[1]),
            Modulus = FromXString(parameters[0]),
        };

        return RSA.Create(rsaParameters);
    }

    private static string ToXString(byte[] buffer) => string.Concat(buffer.Select(t => t.ToString("x2")));

    private static byte[] FromXString(string s) => Enumerable.Range(0, s.Length / 2).Select(i => byte.Parse(s.AsSpan(i * 2, 2), System.Globalization.NumberStyles.HexNumber)).ToArray();

    private async Task<T?> EncryptedPostAsync<T>(string path, string from, string body)
    {
        var encryptedBody = Convert.ToBase64String(_aes.EncryptCbc(_utf8.GetBytes(body), _aes.IV));

        var length = _sequence + encryptedBody.Length;

        var sign = $"h={_passwordHash}&s={length}";
        if (from == "login") sign = $"{_aesKeyIV}&{sign}";

        if (sign!.Length > 53)
        {
            var f1 = ToXString(_rsa!.Encrypt(_utf8.GetBytes(sign[..53]), RSAEncryptionPadding.Pkcs1));
            var f2 = ToXString(_rsa!.Encrypt(_utf8.GetBytes(sign[53..]), RSAEncryptionPadding.Pkcs1));
            sign = f1 + f2;
        }
        else
        {
            sign = ToXString(_rsa!.Encrypt(_utf8.GetBytes(sign), RSAEncryptionPadding.Pkcs1));
        }

        var postBody = $"sign={sign}&data={Uri.EscapeDataString(encryptedBody)}";
        var result = await _client.PostAsync<DataResult>(path, from, postBody);
        if (string.IsNullOrEmpty(result.Data)) throw new Exception("ログインの期限切れ");

        var data = Convert.FromBase64String(result.Data);
        var json = _utf8.GetString(_aes.DecryptCbc(data, _aes.IV));
        try
        {
            var decryptedResult = JsonSerializer.Deserialize<PostResult<T>>(json);
            return decryptedResult!.Result!;
        }
        catch
        {
            return default;
        }
    }

    private Task<T?> EncryptedPostAsync<T>(string path, string from, object body)
    {
        var bodyJson = JsonSerializer.Serialize(body);
        return EncryptedPostAsync<T>(path, from, bodyJson);
    }

    #endregion
}
