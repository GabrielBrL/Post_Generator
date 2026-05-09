using PostGenerator.Shared.Response;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PostGenerator.Service.Utils;

public class JsonTreatment
{
    public static T? Parse<T>(string raw) where T : class
    {
        var clean = raw.Replace("```json", "").Replace("```", "").Trim();
        return JsonSerializer.Deserialize<T>(clean) ?? null;
    }
}
