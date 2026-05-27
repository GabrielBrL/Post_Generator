using PostGenerator.Shared.Request;
using PostGenerator.Shared.Response;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PostGenerator.Service.Services;

public class OllamaService(IHttpClientFactory clientFactory)
{
    private readonly HttpClient _httpClient = clientFactory.CreateClient();
    private const string ProxyUrl = "http://localhost:8000"; // <-- YOUR PYTHON PROXY URL

    // This method handles the entire streaming communication
    public async Task<TopicResponse?> StreamChatAsync(TopicRequest topic)
    {
        _httpClient.BaseAddress = new Uri(ProxyUrl);

        var request = await _httpClient.PostAsJsonAsync("/generate-topic", topic);
        string content = await request.Content.ReadAsStringAsync();
        var result = await request.Content.ReadFromJsonAsync<TopicResponse>();
        return result;
    }

    public async Task<string> GeneratePostAsync(PostRequest postRequest)
    {
        _httpClient.BaseAddress = new Uri(ProxyUrl);
        _httpClient.Timeout = TimeSpan.FromMinutes(5);

        var request = await _httpClient.PostAsJsonAsync("/generate-post", postRequest);
        var result = await request.Content.ReadAsStringAsync();

        return result.Replace("\n", "\\n");
    }
}