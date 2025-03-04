using ShComp.Deco.Models;
using System.Buffers;
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
        _passwordHash = MD5.HashData(_utf8.GetBytes($"admin{password}")).ToXString();

        var passwordKey = await _client.PostAsync<PostResult<KeysResult>>(";stok=/login", "keys", _readBody);
        string encryptedPassword;
        using (var passwordRsa = CreateRsa(passwordKey.Result!.Password!))
        {
            encryptedPassword = passwordRsa.Encrypt(_utf8.GetBytes(password), RSAEncryptionPadding.Pkcs1).ToXString();
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
        return result?.Devices ?? [];
    }

    public async Task<Client[]> GetClientsAsync(string deviceMac = "default")
    {
        var body = new { operation = "read", @params = new { device_mac = deviceMac } };
        var result = await EncryptedPostAsync<ClientListResult>($";stok={_stok}/admin/client", "client_list", body);
        return result?.Clients ?? [];
    }

    #region Utils

    private static RSA CreateRsa(string[] parameters)
    {
        var rsaParameters = new RSAParameters
        {
            Exponent = parameters[1].FromXString(),
            Modulus = parameters[0].FromXString(),
        };

        return RSA.Create(rsaParameters);
    }

    private async Task<T?> EncryptedPostAsync<T>(string path, string from, string body)
    {
        var encryptedBody = Convert.ToBase64String(_aes.EncryptCbc(_utf8.GetBytes(body), _aes.IV));

        var length = _sequence + encryptedBody.Length;

        var sign = $"h={_passwordHash}&s={length}";
        if (from == "login") sign = $"{_aesKeyIV}&{sign}";

        string Encrypt(in ReadOnlySpan<char> source)
        {
            var buffer1 = ArrayPool<byte>.Shared.Rent(64);
            var span1 = buffer1.AsSpan();

            var buffer2 = ArrayPool<byte>.Shared.Rent(128);
            var span2 = buffer1.AsSpan();

            try
            {
                var count1 = _utf8.GetBytes(source, span1);
                var count2 = _rsa!.Encrypt(span1[..count1], span2, RSAEncryptionPadding.Pkcs1);
                return span2[..count2].ToXString();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer1);
                ArrayPool<byte>.Shared.Return(buffer2);
            }
        }

        if (sign.Length <= 53) sign = Encrypt(sign);
        else sign = Encrypt(sign.AsSpan(0, 53)) + Encrypt(sign.AsSpan(54));

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
