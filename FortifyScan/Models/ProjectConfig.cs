using System.Text.RegularExpressions;

namespace FortifyScan.Models;

public class ProjectConfig
{
    public string ProjectDirectoryName => Path.GetFileName(ProjectPath);
    public string ProjectName { get; set; } = "";

    public string ProjectCode { get; set; } = "";
    // public string OwnerId { get; set; }
    // public string Owner { get; set; }
    public string[] NotifyEmail { get; set; } = new string[] { "" };
    
    // public string EmployId { get; set; } = "";

    public string[] IgnoreFiles { get; set; } = new string[] { };
    
    public string ProjectPath { get; set; } = @"";

    public FortifyScanPayload ToPayload()
    {
        return new FortifyScanPayload()
        {
            PCode = this.ProjectCode,
            Artifact = this.ProjectDirectoryName,
            ProjectId = this.ProjectName,
            EmailToOthers = string.Join(",", this.NotifyEmail),
        };
    }

    public Regex[] GetIgnoreFileRegexs()
    {
        var regexes = this.IgnoreFiles
            .Select(p => FindFilesPatternToRegex.Convert(p))
            .ToArray();
        return regexes;
    }
}