using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using FortifyScan.Models;
using Renci.SshNet;
using YamlDotNet.Serialization.NamingConventions;

public class RunCommand
{
    public static async Task Run(RunArgs args)
    {
        // Read Global Config
        var globalConfig = ReadGlobalConfig(args);

        // Read Project Config
        var projectConfig = ReadProjectConfig(args);

        // Send Sftp file
        UploadProjectFiles(globalConfig, projectConfig);

        // Call Forty API
        await SendFortifyScanRequest(globalConfig, projectConfig);
    }

    private static ProjectConfig ReadProjectConfig(RunArgs args)
    {
        var projectConfigYaml = File.ReadAllText(args.Path);
        var projectConfig = new YamlDotNet.Serialization.DeserializerBuilder().Build()
            .Deserialize<ProjectConfig>(projectConfigYaml);
        return projectConfig;
    }

    private static GlobalConfig ReadGlobalConfig(RunArgs args)
    {
        var globalConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.yml");
        GlobalConfig globalConfig;
        if (File.Exists(globalConfigPath))
        {
            var globalConfigYaml = File.ReadAllText(globalConfigPath);
            globalConfig = new YamlDotNet.Serialization.DeserializerBuilder()
                // .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build()
                .Deserialize<GlobalConfig>(globalConfigYaml);
        }
        else
            globalConfig = new GlobalConfig();

        if (!string.IsNullOrEmpty(args.HostName))
            globalConfig.HostName = args.HostName;
        if (!string.IsNullOrEmpty(args.EmployId))
            globalConfig.EmployId = args.EmployId;
        if (!string.IsNullOrEmpty(args.Token))
            globalConfig.Token = args.Token;
        if (args.SshPort > 0)
            globalConfig.SshPort = args.SshPort;
        if (!string.IsNullOrEmpty(args.KeyPath))
            globalConfig.KeyPath = args.KeyPath;
        return globalConfig;
    }

    private static async Task UploadProjectFiles(GlobalConfig globalConfig, ProjectConfig projectConfig)
    {
        using var client = new SftpClient(
            new ConnectionInfo(
                globalConfig.HostName, 22,
                globalConfig.EmployId,
                new PrivateKeyAuthenticationMethod(globalConfig.EmployId, new PrivateKeyFile(globalConfig.KeyPath))
            )
        );
        client.Connect();
        Console.WriteLine("SFTP Login");

        if (client.Exists(projectConfig.ProjectDirectoryName))
        {
            client.DeleteDirectory(projectConfig.ProjectDirectoryName, true);
            Console.WriteLine("SFTP Delete Old File");
        }

        client.CreateDirectory(projectConfig.ProjectDirectoryName);
        Console.WriteLine("SFTP Create Folder");
        client.ChangeDirectory(projectConfig.ProjectDirectoryName);
        Console.WriteLine("SFTP Change Folder");

        Console.WriteLine("==========================================");
        var rootPath = new DirectoryInfo(projectConfig.ProjectPath).FullName;
        var ignoreRegexs = projectConfig.GetIgnoreFileRegexs();
        foreach (var fileSystemInfo in EnumerableFileFilter(rootPath, ignoreRegexs))
        {
            var path = Path.GetRelativePath(rootPath, fileSystemInfo.FullName)
                .Replace("\\", "/");
            if (fileSystemInfo is DirectoryInfo dir)
            {
                client.CreateDirectory(path);
            }
            else if (fileSystemInfo is FileInfo file)
            {
                var fileStream = file.OpenRead();
                
                client.UploadFile(fileStream, path, true);
                Console.WriteLine($"Upload {path}");
                
                await fileStream.DisposeAsync();
            }
        }

        client.Disconnect();
    }

    private static IEnumerable<FileSystemInfo> EnumerableFileFilter(string projectConfigProjectPath, Regex[] projectConfigIgnoreFiles)
    {
        var directoryInfo = new DirectoryInfo(projectConfigProjectPath);
        var infos = directoryInfo.EnumerateFileSystemInfos("*.*", SearchOption.TopDirectoryOnly);

        foreach (var info in infos)
        {
            var infoFullName = info.FullName;
            if (info is DirectoryInfo dir)
            {
                if (projectConfigIgnoreFiles.All(i => !i.IsMatch(dir.Name)))
                {
                    // Console.WriteLine(info.FullName);
                    yield return info;

                    foreach (var fileSystemInfo in EnumerableFileFilter(info.FullName, projectConfigIgnoreFiles))
                        yield return fileSystemInfo;
                }
            }
            else if (info is FileInfo file)
            {
                if (projectConfigIgnoreFiles.All(i => !i.IsMatch(file.Name)))
                {
                    // Console.WriteLine(info.FullName);
                    yield return info;
                }
            }
        }
    }

    private static async Task SendFortifyScanRequest(GlobalConfig globalConfig, ProjectConfig projectConfig)
    {
        try
        {
            Console.WriteLine("==========================================");
            /*
            curl  --data $(addTaskSample)  
            -H "X-idp:keycloak" 
            -H "X-Authorization: Bearer $(token)" 
            -H "Origin:  $(origin)"   
            -H "Content-Type: application/json" -k -o /dev/null -s -w "%{http_code}\n" 
            --request POST  https://$(ip)/main/api/task/add  
            */
            var payload = projectConfig.ToPayload();
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri($"https://{globalConfig.HostName}"),
            };
            httpClient.DefaultRequestHeaders.Add("X-idp", "keycloak");
            httpClient.DefaultRequestHeaders.Add("X-Authorization", $"bearer {globalConfig.Token.Split(new char[]{'\r', '\n'})[0]}");
            httpClient.DefaultRequestHeaders.Add("Origin", $"https://{globalConfig.HostName}");
            Console.WriteLine("Send Fortify Scan Request");
            var result = await httpClient.PostAsJsonAsync("main/api/task/add", payload);
            Console.WriteLine(result.StatusCode);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}