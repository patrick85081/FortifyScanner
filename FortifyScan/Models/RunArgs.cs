using System.Net.Http.Headers;
using CommandLine;

[Verb("run", true, new string[]{"run"})]
public class RunArgs
{
    [Value(0, Required = true, HelpText = "專案送掃設定檔案 (*.yml)")]
    public string Path { get; set; }
    
    [Option('h', "host", HelpText = "Host Name")]
    public string HostName { get; set; } 
    
    [Option('p', "port", HelpText = "Ssh Port")]
    public int SshPort { get; set; } 
    
    [Option('i', "id", HelpText = "Employ Id")]
    public string EmployId { get; set; } 
    
    [Option('k', "Key", HelpText = "SSH Private Key")]
    public string KeyPath { get; set; }
    
    [Option('t', "token", HelpText = "Fortify Token")]
    public string Token { get; set; } 
}

/*
{
    "artifact":"cas-fortify", 
    "projectId":"協助中華電信進行CAS 3.0 欠費與帳務查詢模組雲端客製化開發_(開口合約)", 
    "emailToOthers":"sherry.wu@iisigroup.com,patrick.shih@iisigroup.com,albert.fj@iisigroup.com,eileen.chen@iisigroup.com,hank.chen@iisigroup.com,baron.huang@iisigroup.com,jasmine.chen@iisigroup.com,hz.tseng@iisigroup.com", 
    "pCode":"D220130"
}
*/