using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace JsonToXmlConverter
{
    public class JsonToXmlConverter
    {
        public static async Task ConvertJsonArrayToXml(string apiUrl, string outputXmlFilePath)
        {
            try
            {
                string? jsonData = await GetJsonDataFromApi(apiUrl);

                if (jsonData != null)
                {
                    JsonDocument jsonDocument = JsonDocument.Parse(jsonData);

                    if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        XDocument xmlDocument = new();

                        XElement rootElementXml = new("root");

                        foreach (var question in jsonDocument.RootElement.EnumerateArray())
                        {
                            XElement? xElement = GetQuestionTypeElement(question);

                            xElement?.Add(new XElement("title", question.GetProperty("title").GetString().Trim()));

                            AddInstructionText(question, xElement);

                            AddQuestionOptions(question, xElement);

                            rootElementXml.Add(xElement);

                            // Add <suspend> element after each <radio> element
                            rootElementXml.Add(new XElement("suspend"));
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

        private static void AddQuestionOptions(JsonElement question, XElement? xElement)
        {
            var questionOptions = question.GetProperty("questionOptions");

            if (questionOptions.ValueKind == JsonValueKind.Array)
            {
                foreach (var option in questionOptions.EnumerateArray())
                {
                    XElement element = AddRow(option);

                    xElement?.Add(element);
                }
            }
        }

        private static XElement AddRow(JsonElement option)
        {
            JsonObject obj = JsonNode.Parse(option.ToString()).AsObject();

            bool isRow = (bool)obj["isRow"];

            return new(isRow ? "row" : "col", new XAttribute("label", option.GetProperty("label").GetString().Trim()))
            {
                Value = option.GetProperty("optionText").GetString().Trim()
            };
        }

        private static void AddInstructionText(JsonElement question, XElement? xElement)
        {
            string commentText = question.GetProperty("instructionText").GetString().Trim();

            XElement commentElement = new("comment");

            if (!string.IsNullOrEmpty(commentText))
            {
                commentElement.Add(new XElement("em"));

                commentElement.Value = commentText;
            }

            xElement?.Add(commentElement);
        }

        private static XElement? GetQuestionTypeElement(JsonElement item)
        {
            XElement? questionType = null;

            switch (item.GetProperty("questionType").GetProperty("type").GetString().Trim())
            {
                case QuestionType.Radio:
                    questionType = new("radio", new XAttribute("label", item.GetProperty("label").GetString().Trim()));
                    break;
                case QuestionType.Checkbox:
                    questionType = new("checkbox", new XAttribute("label", item.GetProperty("label").GetString().Trim()));
                    break;
                case QuestionType.Select:
                    questionType = new("select", new XAttribute("label", item.GetProperty("label").GetString().Trim()));
                    break;
                default:
                    Console.WriteLine("No matching question type found");
                    break;
            }

            return questionType;
        }

        private static async Task<string?> GetJsonDataFromApi(string apiUrl)
        {
            try
            {
                using HttpClient httpClient = new();
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }
    }
}
