using YamlDotNet.Serialization;

namespace FortifyScan.Models;

public class GlobalConfig
{
    public string HostName { get; set; } = "";
    public int SshPort { get; set; } = 22;
    public string EmployId { get; set; } = "";
    public string Token { get; set; } = "";
    public string KeyPath { get; set; } = "id_rsa";
}