using Microsoft.Extensions.Configuration;

namespace ShComp.Deco.Test;

public class DecoClientTest
{
    private readonly IConfiguration _configuration;

    public DecoClientTest()
    {
        _configuration = new ConfigurationBuilder().AddUserSecrets<DecoClientTest>().Build();
    }

    private async Task<DecoClient> CreateAndLoginAsync()
    {
        var host = _configuration["Host"]!;
        var password = _configuration["Password"]!;

        var client = new DecoClient(host);
        await client.LoginAsync(password);

        return client;
    }

    [Fact]
    public async Task LoginTest()
    {
        using var client = await CreateAndLoginAsync();
    }

    [Fact]
    public async Task GetDevicesTest()
    {
        using var client = await CreateAndLoginAsync();

        var devices = await client.GetDevicesAsync();
        Assert.NotEmpty(devices);
    }

    [Fact]
    public async Task GetClientsTest()
    {
        using var client = await CreateAndLoginAsync();

        var clients = await client.GetClientsAsync();
        Assert.NotEmpty(clients);
    }

    [Fact]
    public async Task GetDecoClientsTest()
    {
        using var client = await CreateAndLoginAsync();

        var devices = await client.GetDevicesAsync();

        var clients = await client.GetClientsAsync();
        var decoClients = await client.GetClientsAsync(devices[0]!.Mac!);

        Assert.NotEmpty(clients);
        Assert.NotEmpty(decoClients);
        Assert.NotEqual(clients.Length, decoClients.Length);
    }

    [Fact]
    public async Task RenameDeviceTest()
    {
        using var client = await CreateAndLoginAsync();

        var devices = await client.GetDevicesAsync();
        var device = devices[0];

        await client.RenameDeviceAsync(device.DeviceId!, device.Nickname!);
    }

    [Fact]
    public async Task DoubleLoginTest()
    {
        using var client1 = await CreateAndLoginAsync();
        using var client2 = await CreateAndLoginAsync();

        await Assert.ThrowsAnyAsync<Exception>(() => client1.GetDevicesAsync());
    }
}
