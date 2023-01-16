using System.Text.Json.Serialization;

namespace FortifyScan.Models;

public class FortifyScanPayload
{
    [JsonPropertyName("pCode")]
    public string PCode { get; set; }
    
    [JsonPropertyName("artifact")]
    public string Artifact { get; set; }
    
    [JsonPropertyName("projectId")]
    public string ProjectId { get; set; }
    
    [JsonPropertyName("emailToOthers")]
    public string EmailToOthers { get; set; }
}