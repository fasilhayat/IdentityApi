using System.Text.Json.Serialization;

namespace IdentityApi.Model;

/// <summary>
/// 
/// </summary>
public class Noeglering
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="identitetsnoegle"></param>
    /// <param name="cprnummer"></param>
    public Noeglering(string? identitetsnoegle, string? cprnummer)
    {
        Identitetsnoegle = identitetsnoegle;
        Cprnummer = cprnummer;
    }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("identitetsnoegle")]
    public string? Identitetsnoegle { get; init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("cprnummer")]
    public string? Cprnummer { get; init; }
}