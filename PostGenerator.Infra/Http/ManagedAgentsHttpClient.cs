using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace PostGenerator.Infra.Http;

public class ManagedAgentsHttpClient(HttpClient http)
{
    public async Task<JsonElement> PostAsync(string path, object body, CancellationToken ct)
    {
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var resp = await http.PostAsync(path, content, ct);
        resp.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync(ct));
    }

    public async Task<JsonElement> UpdateAsync(string path, object body, CancellationToken ct)
    {
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var resp = await http.PatchAsync(path, content, ct);
        string result = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
            Console.WriteLine(result);
        resp.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<JsonElement>(result);
    }

    public async Task<JsonElement> DeleteAsync(string path, CancellationToken ct)
    {
        var resp = await http.DeleteAsync(path, ct);
        resp.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync(ct));
    }

    public async IAsyncEnumerable<JsonElement> StreamAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, path);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        using var resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        resp.EnsureSuccessStatusCode();
        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:")) continue;

            var data = line["data:".Length..].Trim();
            if (data == "[DONE]") yield break;

            JsonElement evt;
            try { evt = JsonSerializer.Deserialize<JsonElement>(data); }
            catch { continue; }

            yield return evt;
        }
    }
}
