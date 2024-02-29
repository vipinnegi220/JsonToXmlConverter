// JsonToXmlConverter.cs
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JsonToXmlConverter
{
    public class JsonToXmlConverter
    {
        public static async Task ConvertJsonArrayToXml(string apiUrl, string outputXmlFilePath)
        {
            try
            {
                string jsonData = await GetJsonDataFromApi(apiUrl);

                if (jsonData != null)
                {
                    JsonDocument jsonDocument = JsonDocument.Parse(jsonData);

                    if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        XDocument xmlDocument = new XDocument();
                        XElement rootElementXml = new XElement("Root"); 

                        foreach (var arrayItem in jsonDocument.RootElement.EnumerateArray())
                        {
                            XElement itemElement = new XElement("Item"); 
                            ConvertJsonToXml(itemElement, arrayItem);
                            rootElementXml.Add(itemElement);
                        }

                        xmlDocument.Add(rootElementXml);
                        xmlDocument.Save(outputXmlFilePath);

                        Console.WriteLine($"Conversion completed. Check {outputXmlFilePath} for the result.");
                    }
                    else
                    {
                        Console.WriteLine("JSON data is not an array. Unable to convert.");
                    }
                }
                else
                {
                    Console.WriteLine("Unable to fetch JSON data from the API.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during conversion: {ex.Message}");
            }
        }

        private static async Task<string> GetJsonDataFromApi(string apiUrl)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        private static void ConvertJsonToXml(XElement parentXmlElement, JsonElement jsonElement)
        {
            foreach (var property in jsonElement.EnumerateObject())
            {
                XElement xmlElement = new XElement(property.Name);

                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    ConvertJsonToXml(xmlElement, property.Value);
                    parentXmlElement.Add(xmlElement);
                }
                else if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var arrayItem in property.Value.EnumerateArray())
                    {
                        XElement arrayElement = new XElement(property.Name);
                        ConvertJsonToXml(arrayElement, arrayItem);
                        parentXmlElement.Add(arrayElement);
                    }
                }
                else
                {
                    xmlElement.Value = property.Value.ToString();
                    parentXmlElement.Add(xmlElement);
                }
            }
        }
    }
}
