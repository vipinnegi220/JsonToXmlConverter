using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.Json;

namespace JsonToXmlConverter
{
    public class JsonToXmlConverter
    {
        public static async Task ConvertJsonArrayToXml(string jsonFilePath, string outputXmlFilePath)
        {
            try
            {
                string? jsonData = await GetJsonDataFromFile(jsonFilePath);

                if (jsonData != null)
                {
                    JsonDocument jsonDocument = JsonDocument.Parse(jsonData);

                    if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        XDocument xmlDocument = new XDocument(); // Create a new XDocument

                        // Create a root element for the XML document
                        XElement surveyElement = new XElement("survey");

                        foreach (JsonElement questionElement in jsonDocument.RootElement.EnumerateArray())
                        {
                            XElement questionXml = GetQuestionXml(questionElement);
                            surveyElement.Add(questionXml);
                        }

                        // Add the root element to the XML document
                        xmlDocument.Add(surveyElement);

                        xmlDocument.Save(outputXmlFilePath);
                        Console.WriteLine($"Conversion completed. Check {outputXmlFilePath} for the result.");
                    }
                    else
                    {
                        Console.WriteLine("The JSON data should be an array of objects.");
                    }
                }
                else
                {
                    Console.WriteLine("Unable to read JSON data from the file.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during conversion: {ex.Message}");
            }
        }

        private static XElement GetQuestionXml(JsonElement question)
        {
            string questionType = question.GetProperty("type").GetString().Trim();
            XElement questionXml = new XElement(questionType);

            // Add common attributes for all question types
            questionXml.Add(new XAttribute("label", question.GetProperty("label").GetString().Trim()));

            // Add type-specific attributes or elements
            switch (questionType)
            {
                case "text":
                    questionXml.Add(
                        new XAttribute("optional", question.GetProperty("optional").GetString().Trim()),
                        new XAttribute("verify", question.GetProperty("verify").GetString().Trim()),
                        new XElement("title", new XElement("strong", question.GetProperty("title").GetString().Trim())),
                        new XElement("comment", new XElement("em", question.GetProperty("comment").GetString().Trim()))
                    );
                    break;

                case "number":
                    questionXml.Add(
                        new XAttribute("optional", question.GetProperty("optional").GetString().Trim()),
                        new XAttribute("verify", question.GetProperty("verify").GetString().Trim()),
                        new XElement("title", new XElement("strong", question.GetProperty("title").GetString().Trim())),
                        new XElement("comment", new XElement("em", question.GetProperty("comment").GetString().Trim()))
                    );
                    break;

                case "textarea":
                    questionXml.Add(
                        new XElement("title", new XElement("strong", question.GetProperty("title").GetString().Trim())),
                        new XElement("validate", question.GetProperty("validate").GetString().Trim())
                    );
                    break;

                // Add additional cases for other question types if needed

                default:
                    Console.WriteLine($"Unknown question type: {questionType}");
                    break;
            }

            return questionXml;
        }

        private static async Task<string?> GetJsonDataFromFile(string jsonFilePath)
        {
            try
            {
                return System.IO.File.ReadAllText(jsonFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading JSON data from the file: {ex.Message}");
                return null;
            }
        }
    }
}
