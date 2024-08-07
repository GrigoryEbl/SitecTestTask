﻿class Program
{
    private const string _archiveUrl = "https://fias-file.nalog.ru/downloads/2024.08.06/gar_delta_xml.zip";
    private const string _archivePath = "gar_delta_xml.zip";
    private const string _extractPath = "extracted";

    static void Main(string[] args)
    {
        var downloader = new FileDownloader();
        var extractor = new ZipExtractor();
        var processor = new XmlProcessor();

        downloader.DownloadFile(_archiveUrl, _archivePath);

        extractor.ExtractZipFile(_archivePath, _extractPath);

        processor.ProcessExtractedFiles(_extractPath);
    }
}
