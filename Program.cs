using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        string inputJsonFilePath = "JsonFile.json";
        string outputXmlFilePath = "output.xml";

        await JsonToXmlConverter.JsonToXmlConverter.ConvertJsonArrayToXml(inputJsonFilePath, outputXmlFilePath);

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
