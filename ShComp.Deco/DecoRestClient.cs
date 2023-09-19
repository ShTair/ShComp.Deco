using RestSharp;

namespace ShComp.Deco;

internal class DecoRestClient : IDisposable
{
    private readonly RestClient _client;

    public DecoRestClient(string host)
    {
        _client = new RestClient(new Uri($"http://{host}/cgi-bin/luci/"));
        _client.AddDefaultHeader("Content-Type", "application/json");
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    public async Task<string> PostAsync(string path, string from, string body)
    {
        var request = new RestRequest($"{path}?form={from}");
        request.AddBody(body);

        var result = await _client.PostAsync(request);
        return result.Content!;
    }

    public async Task<T> PostAsync<T>(string path, string from, string body)
    {
        var content = await PostAsync(path, from, body);
        return JsonSerializer.Deserialize<T>(content)!;
    }

    public Task<TResult> PostAsync<TBody, TResult>(string path, string from, TBody body)
    {
        var bodyJson = JsonSerializer.Serialize(body);
        return PostAsync<TResult>(path, from, bodyJson);
    }
}
