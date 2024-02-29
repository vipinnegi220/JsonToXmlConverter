// Program.cs
using System;
using JsonToXmlConverter;

class Program
{
    static async Task Main()
    {
        string apiUrl = "http://localhost:5170/surveys/1/questions";
        string outputXmlFilePath = "output.xml";

        await JsonToXmlConverter.JsonToXmlConverter.ConvertJsonArrayToXml(apiUrl, outputXmlFilePath);
    }
}
