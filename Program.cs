using System;

class Program
{
    static void Main()
    {
        // Specify the path to your JSON file
        string jsonFilePath = "JsonFile.json";

        // Specify the path for the output XML file
        string xmlOutputPath = "output.xml";

        // Convert JSON to XML
        JsonToXmlConverter.JsonToXmlConverter.ConvertJsonFileToXml(jsonFilePath, xmlOutputPath);

        Console.WriteLine($"Conversion completed. XML file saved at: {xmlOutputPath}");
    }
}
