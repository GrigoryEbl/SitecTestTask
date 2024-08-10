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

        var levelsDictionary = LoadObjectLevels(extractPath);
       
        var addressObjects = new List<AddressObject>();
        DateTime? updateDate = null;

        foreach (var file in files)
        {
            ProcessFile(file, addressObjects, ref updateDate, levelsDictionary);
        }

        if (addressObjects.Count == 0)
        {
            Console.WriteLine("Нет активных адресных объектов.");
            return;
        }

        GenerateReport(addressObjects, updateDate ?? DateTime.Now);
    }
     
    private string[] GetFiles(string path)
    {
        string pattern = "AS_ADDR_OBJ_*.xml";
        return Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
    }

    private void ProcessFile(string filePath, List<AddressObject> addressObjects, ref DateTime? updateDate, Dictionary<int, string> levelsDictionary)
    {
        try
        {
            XDocument doc = XDocument.Load(filePath);

            var dateAttribute = doc.Descendants(_addressObjectXPath)
                                   .FirstOrDefault()
                                   ?.Attribute("UPDATEDATE");

            if (dateAttribute != null && DateTime.TryParse(dateAttribute.Value, out DateTime parsedDate))
            {
                if (!updateDate.HasValue || parsedDate > updateDate.Value)
                {
                    updateDate = parsedDate;
                }
            }

            var objects = from e in doc.Descendants(_addressObjectXPath)
                          where (string)e.Attribute(_isActiveAttribute) == "1"
                          select new AddressObject
                          {
                              Name = (string)e.Attribute("NAME"),
                              ShortName = (string)e.Attribute("TYPENAME"),
                              Level = levelsDictionary.TryGetValue((int?)e.Attribute("LEVEL") ?? 0, out var levelName) ? levelName : "Неизвестный уровень"
                          };

            addressObjects.AddRange(objects);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обработки файла {filePath}: {ex.Message}");
        }
    }

    private Dictionary<int, string> LoadObjectLevels(string extractPath)
    {
        string pattern = "AS_OBJECT_LEVELS_*.xml";
        string filePath = Directory.GetFiles(extractPath, pattern, SearchOption.TopDirectoryOnly).FirstOrDefault();

        if (string.IsNullOrEmpty(filePath))
        {
            Console.WriteLine("Файл уровней объектов не найден.");
            return new Dictionary<int, string>();
        }

        var levels = new Dictionary<int, string>();

        try
        {
            XDocument doc = XDocument.Load(filePath);
            var levelElements = doc.Descendants("OBJECTLEVEL");

            foreach (var levelElement in levelElements)
            {
                var levelAttr = levelElement.Attribute("LEVEL");
                var nameAttr = levelElement.Attribute("NAME");

                if (levelAttr == null || nameAttr == null)
                {
                    Console.WriteLine($"Ошибка: отсутствуют необходимые атрибуты в элементе {levelElement}");
                    continue;
                }

                if (int.TryParse(levelAttr.Value, out int level))
                {
                    levels[level] = nameAttr.Value;
                }
                else
                {
                    Console.WriteLine($"Ошибка: некорректное значение уровня '{levelAttr.Value}' в элементе {levelElement}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке уровней объектов: {ex.Message}");
        }

        return levels;
    }


    private void GenerateReport(List<AddressObject> addressObjects, DateTime updateDate)
    {
        StringBuilder reportBuilder = new StringBuilder();
        var groupedByLevel = addressObjects.GroupBy(a => a.Level);

        reportBuilder.AppendLine();
        reportBuilder.AppendLine($"Дата изменений: {updateDate:dd.MM.yyyy}");
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
