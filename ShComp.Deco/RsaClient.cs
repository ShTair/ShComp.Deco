using ShComp.Deco.Models;
using System.Buffers;
using System.Security.Cryptography;
using System.Text;

namespace ShComp.Deco;

internal class RsaClient : IDisposable
{
    private readonly string _readBody = JsonSerializer.Serialize(new { operation = "read" });

    private readonly UTF8Encoding _utf8 = new(false);

    private readonly DecoRestClient _client;
    private readonly string _passwordHash;

    private readonly string _aesKeyIV;
    private readonly Aes _aes;

    private RSA? _rsa;
    private int _sequence;

    public RsaClient(DecoRestClient client, string password)
    {
        _client = client;
        _passwordHash = MD5.HashData(_utf8.GetBytes($"admin{password}")).ToXString();

        var r = new Random();
        static string Int64(Random r) => r.NextInt64(10000000000000000).ToString("0000000000000000");
        var aesKey = Int64(r);
        var aesIV = Int64(r);
        _aesKeyIV = $"k={aesKey}&i={aesIV}";

        _aes = Aes.Create();
        _aes.Key = _utf8.GetBytes(aesKey);
        _aes.IV = _utf8.GetBytes(aesIV);
    }

    public async Task InitializeAsync()
    {
        var sessionKey = await _client.PostAsync<PostResult<SessionResult>>(";stok=/login", "auth", _readBody);
        _rsa = CreateRsa(sessionKey.Result!.Key!);
        _sequence = sessionKey.Result!.Seq;
    }

    private static RSA CreateRsa(string[] parameters)
    {
        var rsaParameters = new RSAParameters
        {
            Exponent = parameters[1].FromXString(),
            Modulus = parameters[0].FromXString(),
        };

        return RSA.Create(rsaParameters);
    }

    public async Task<T?> EncryptedPostAsync<T>(string path, string from, string body)
    {
        var encryptedBody = Convert.ToBase64String(_aes.EncryptCbc(_utf8.GetBytes(body), _aes.IV));

        var length = _sequence + encryptedBody.Length;

        var sign = $"h={_passwordHash}&s={length}";
        if (from == "login") sign = $"{_aesKeyIV}&{sign}";

        string Encrypt(in ReadOnlySpan<char> source)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(64);
            var span = buffer.AsSpan();

            try
            {
                var count1 = _utf8.GetBytes(source, span);
                var count2 = _rsa!.Encrypt(span[..count1], span, RSAEncryptionPadding.Pkcs1);
                return span[..count2].ToXString();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
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

    public Task<T?> EncryptedPostAsync<T>(string path, string from, object body)
    {
        var bodyJson = JsonSerializer.Serialize(body);
        return EncryptedPostAsync<T>(path, from, bodyJson);
    }

    public void Dispose()
    {
        _rsa?.Dispose();
    }
}
