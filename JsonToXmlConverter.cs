using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JsonToXmlConverter
{
    public class JsonToXmlConverter
    {
        public static async Task ConvertJsonFileToXml(string jsonFilePath, string outputXmlFilePath)
        {
            try
            {
                string jsonData = File.ReadAllText(jsonFilePath);

                JsonDocument jsonDocument = JsonDocument.Parse(jsonData);

                if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
                {
                    XDocument xmlDocument = new();

                    XElement rootElement = new XElement("survey");

                    foreach (var element in jsonDocument.RootElement.EnumerateArray())
                    {
                        try
                        {
                            XElement? question = GetQuestionTypeElement(element);

                            if (question != null)
                            {
                                question.Add(new XElement("title", element.GetProperty("title").GetString().Trim()));
                                question.Add(new XElement("comment", "Enter a number"));

                                AddQuestionOptions(element, question);

                                rootElement.Add(question);
                            }
                            else
                            {
                                Console.WriteLine("Error: Unable to process a JSON element. Check the JSON structure.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing JSON element: {ex.Message}");
                            Console.WriteLine($"Problematic JSON element: {element}");
                        }
                    }

                    xmlDocument.Add(rootElement);
                    xmlDocument.Save(outputXmlFilePath);

                    Console.WriteLine($"Conversion completed. Check {outputXmlFilePath} for the result.");
                }
                else
                {
                    Console.WriteLine("JSON data is not an array. Unable to convert.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during conversion: {ex.Message}");
            }
        }

        private static XElement? GetQuestionTypeElement(JsonElement question)
        {
            try
            {
                XElement? questionType = null;

                var questionTypeProperty = question.GetProperty("questionType");
                var typeProperty = questionTypeProperty.ValueKind == JsonValueKind.Object ? questionTypeProperty.GetProperty("type") : default;

                switch (typeProperty.GetString()?.Trim())
                {
                    case "number":
                        questionType = new XElement("number",
                            new XAttribute("label", question.GetProperty("label").GetString().Trim()),
                            new XAttribute("optional", "0"),
                            new XAttribute("size", question.GetProperty("size").GetString().Trim()));
                        break;
                    default:
                        Console.WriteLine($"No matching question type found for {typeProperty.GetString()?.Trim()}");
                        break;
                }

                return questionType;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetQuestionTypeElement: {ex.Message}");
                return null;
            }
        }

        private static void AddQuestionOptions(JsonElement question, XElement? xElement)
        {
            try
            {
                var questionOptions = question.GetProperty("options");

                if (questionOptions.ValueKind == JsonValueKind.Object)
                {
                    foreach (var option in questionOptions.EnumerateObject())
                    {
                        try
                        {
                            XElement element = AddRow(question, option);

                            xElement?.Add(element);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing option: {ex.Message}");
                            Console.WriteLine($"Problematic option: {option}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddQuestionOptions: {ex.Message}");
            }
        }

        private static XElement AddRow(JsonElement question, JsonProperty option)
        {
            try
            {
                var optionValue = option.Value.ToString().Trim();
                bool isRow = option.Value.ValueKind == JsonValueKind.Object &&
                              optionValue.StartsWith("{") &&
                              optionValue.EndsWith("}");

                if (isRow)
                {
                    JsonObject optionObject = JsonNode.Parse(optionValue).AsObject();

                    string rowValue = optionObject.ContainsKey("value") ? optionObject["value"].ToString().Trim() : null;

                    return new XElement("row",
                        new XAttribute("label", option.Name),
                        new XAttribute("value", rowValue),
                        rowValue);
                }
                else
                {
                    return new XElement("col",
                        new XAttribute("label", option.Name),
                        new XAttribute("value", optionValue),
                        optionValue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddRow: {ex.Message}");
                return new XElement("error", "Error processing row/col element");
            }
        }
    }
}
