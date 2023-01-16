using Renci.SshNet;
using Renci.SshNet.Sftp;

public static class SftpClientEx
{
    public static void DeleteDirectory(this SftpClient client, string path, bool force)
    {
        if (!force)
            client.DeleteDirectory(path);
        
        foreach (SftpFile file in client.ListDirectory(path))
        {
            if ((file.Name != ".") && (file.Name != ".."))
            {
                if (file.IsDirectory)
                {
                    client.DeleteDirectory(file.FullName, true);
                }
                else
                {
                    client.DeleteFile(file.FullName);
                }
            }
        }

        client.DeleteDirectory(path);
    }
}