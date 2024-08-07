using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

public class XmlProcessor
{
    private const string _addressObjectXPath = "OBJECT";
    private const string _isActiveAttribute = "ISACTIVE";

    public void ProcessExtractedFiles(string extractPath)
    {
        var files = GetFiles(extractPath);

        if (files.Length == 0)
        {
            Console.WriteLine("Файлы AS_ADDR_OBJ не найдены.");
            return;
        }

        var addressObjects = new List<AddressObject>();

        foreach (var file in files)
        {
            Console.WriteLine($"Обработка файла: {file}");
            ProcessFile(file, addressObjects);
        }

        if (addressObjects.Count == 0)
        {
            Console.WriteLine("Нет активных адресных объектов.");
            return;
        }

        GenerateReport(addressObjects);
    }

    private string[] GetFiles(string path)
    {
        return Directory.GetFiles(path, "AS_ADDR_OBJ_*.xml", SearchOption.AllDirectories);
    }

    private void ProcessFile(string filePath, List<AddressObject> addressObjects)
    {
        try
        {
            XDocument doc = XDocument.Load(filePath);

            var objects = from e in doc.Descendants(_addressObjectXPath)
                          where (string)e.Attribute(_isActiveAttribute) == "1"
                          select new AddressObject
                          {
                              Name = (string)e.Attribute("NAME"),
                              ShortName = (string)e.Attribute("TYPENAME"),
                              Level = (int?)e.Attribute("LEVEL") ?? 0
                          };

            foreach (var obj in objects)
            {
                addressObjects.Add(obj);
            }

            Console.WriteLine($"Количество объектов в файле: {objects.Count()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обработки файла {filePath}: {ex.Message}");
        }
    }

    private void GenerateReport(List<AddressObject> addressObjects)
    {
        var groupedByLevel = addressObjects.GroupBy(a => a.Level);

        StringBuilder reportBuilder = new StringBuilder();
        reportBuilder.AppendLine($"Дата изменений: {DateTime.Now:dd.MM.yyyy}, {DateTime.UtcNow}");
        reportBuilder.AppendLine();

        foreach (var group in groupedByLevel)
        {
            reportBuilder.AppendLine($"Уровень: {group.Key}");
            reportBuilder.AppendLine("| Краткое наименование типа объекта | Наименование объекта |");
            reportBuilder.AppendLine("|-----------------------------------|-----------------------|");

            foreach (var obj in group.OrderBy(a => a.Name))
            {
                reportBuilder.AppendLine($"| {obj.ShortName.PadRight(35)} | {obj.Name.PadRight(23)} |");
            }

            reportBuilder.AppendLine();
        }

        File.WriteAllText("report.txt", reportBuilder.ToString());
        Console.WriteLine("Отчет успешно создан и сохранен в report.txt.");
    }
}
