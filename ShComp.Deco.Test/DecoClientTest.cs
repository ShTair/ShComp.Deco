using Microsoft.Extensions.Configuration;

namespace ShComp.Deco.Test;

public class DecoClientTest
{
    private readonly IConfiguration _configuration;

    public DecoClientTest()
    {
        _configuration = new ConfigurationBuilder().AddUserSecrets<DecoClientTest>().Build();
    }

    [Fact]
    public async Task LoginTest()
    {
        var host = _configuration["Host"]!;
        var password = _configuration["Password"]!;

        using var client = new DecoClient(host);
        await client.LoginAsync(password);
    }

    [Fact]
    public async Task GetDevicesTest()
    {
        var host = _configuration["Host"]!;
        var password = _configuration["Password"]!;

        using var client = new DecoClient(host);
        await client.LoginAsync(password);

        var devices = await client!.GetDevicesAsync();
        Assert.NotEmpty(devices);
    }
}
