using System.IO;
using System.IO.Compression;

public class ZipExtractor
{
    public void ExtractZipFile(string zipFilePath, string extractPath)
    {
        if (Directory.Exists(extractPath))
        {
            Directory.Delete(extractPath, true);
        }

        ZipFile.ExtractToDirectory(zipFilePath, extractPath);
    }
}