using System.Net;

public class FileDownloader
{
    public void DownloadFile(string url, string filePath)
    {
        var client = new WebClient();
        client.DownloadFile(url, filePath);
    }
}