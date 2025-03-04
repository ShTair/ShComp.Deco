using ShComp.Deco.Models;
using System.Security.Cryptography;
using System.Text;

namespace ShComp.Deco;

public sealed class DecoClient(string host) : IDisposable
{
    private readonly UTF8Encoding _utf8 = new(false);
    private readonly string _readBody = JsonSerializer.Serialize(new { operation = "read" });

    private readonly DecoRestClient _client = new(host);

    private string? _stok;

    private RsaClient? _rsaClient;

    public void Dispose()
    {
        _rsaClient?.Dispose();
        _client.Dispose();
    }

    public async Task LoginAsync(string password)
    {
        _rsaClient = new RsaClient(_client, password);
        await _rsaClient.InitializeAsync();

        var encryptedPassword = await EncryptPasswordAsync(password);
        var loginRequestBody = new { @params = new { password = encryptedPassword }, operation = "login" };
        var loginResult = await _rsaClient.EncryptedPostAsync<LoginResult>(";stok=/login", "login", loginRequestBody);
        if (loginResult is not { Stok: { } stok }) throw new Exception("ログイン失敗");

        _stok = stok;
    }

    private async Task<string> EncryptPasswordAsync(string password)
    {
        var passwordKey = await _client.PostAsync<PostResult<KeysResult>>(";stok=/login", "keys", _readBody);
        using (var passwordRsa = CreateRsa(passwordKey.Result!.Password!))
        {
            return passwordRsa.Encrypt(_utf8.GetBytes(password), RSAEncryptionPadding.Pkcs1).ToXString();
        }
    }

    public async Task<Device[]> GetDevicesAsync()
    {
        var result = await _rsaClient!.EncryptedPostAsync<DeviceListResult>($";stok={_stok}/admin/device", "device_list", _readBody);
        return result?.Devices ?? [];
    }

    public async Task<Client[]> GetClientsAsync(string deviceMac = "default")
    {
        var body = new { operation = "read", @params = new { device_mac = deviceMac } };
        var result = await _rsaClient!.EncryptedPostAsync<ClientListResult>($";stok={_stok}/admin/client", "client_list", body);
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

    #endregion
}
