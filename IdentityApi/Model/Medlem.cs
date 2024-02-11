using System.Text.Json.Serialization;

namespace IdentityApi.Model;

/// <summary>
/// 
/// </summary>
public class Medlem
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="identitetsnoegle"></param>
    /// <param name="cprnummer"></param>
    public Medlem(uint identitetsnoegle, string? cprnummer)
    {
        Identitetsnoegle = identitetsnoegle;
        Cprnummer = cprnummer;
    }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("identitetsnoegle")]
    public uint Identitetsnoegle { get; init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("cprnummer")]
    public string? Cprnummer { get; init; }
}