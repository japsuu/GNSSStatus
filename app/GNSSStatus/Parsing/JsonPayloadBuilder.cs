using System.Net;
using System.Text;
using System.Text.Json;
using GNSSStatus.Configuration;
using GNSSStatus.Utils;

namespace GNSSStatus.Parsing;

public class JsonPayloadBuilder
{
    private readonly List<string> _jsonPayloads = new();
    
    
    public JsonPayloadBuilder AddPayload<T>(T obj)
    {
        string json = JsonSerializer.Serialize(obj);
        _jsonPayloads.Add(json);
        
        return this;
    }
    
    
    public string Build(bool encode)
    {
        if (_jsonPayloads.Count > ConfigManager.MAX_COMBINED_PAYLOAD_COUNT)
        {
            Logger.LogWarning($"Combined payload count exceeds max supported count ({ConfigManager.MAX_COMBINED_PAYLOAD_COUNT}). Returning empty payload.");
            return string.Empty;
        }
        
        StringBuilder sb = new();
        
        for (int i = 0; i < _jsonPayloads.Count; i++)
        {
            string payload = CreatePayload(_jsonPayloads[i], encode);
            
            sb.Append($"field{i + 1}={payload}");
            if (i < _jsonPayloads.Count - 1)
                sb.Append('&');
        }
        
        return sb.ToString();
    }
    
    
    private static string CreatePayload(string json, bool encode)
    {
        if (string.IsNullOrEmpty(json))
        {
            Logger.LogWarning("An empty payload was generated.");
            return string.Empty;
        }
            
        // Percent-encode the payload.
        if (encode)
            json = WebUtility.UrlEncode(json);

        if (json.Length > ConfigManager.MAX_JSON_PAYLOAD_LENGTH)
        {
            Logger.LogWarning($"Payload exceeds max supported character count ({ConfigManager.MAX_JSON_PAYLOAD_LENGTH}). Returning empty payload.");
            return string.Empty;
        }
        
        Logger.LogDebug($"payload length: {json.Length}");
        return json;
    }
}