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

                        XElement rootSurveyElement = GetRootSurveyElement();

                        foreach (var element in jsonDocument.RootElement.EnumerateArray())
                        {
                            XElement? question = GetQuestionTypeElement(element);

                            question?.Add(new XElement("title", new XElement("strong", element.GetProperty("title").GetString().Trim())));

                            AddInstructionText(element, question);

                            AddQuestionOptions(element, question);

                            rootSurveyElement.Add(question);

                            rootSurveyElement.Add(new XElement("suspend"));
                        }

                        xmlDocument.Add(rootSurveyElement);
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

        private static XElement GetRootSurveyElement()
        {
            XElement rootElement = new("survey");

            XAttribute autoSaveKey = new("autosaveKey", "uid");
            XAttribute builderCompatible = new("builderCompatible", "1");
            XAttribute mobileDevices = new("mobileDevices", "smartphone,tablet,desktop");
            XAttribute secure = new("secure", "1");
            XAttribute disableBackButton = new("ss:disableBackButton", "1");
            XAttribute enableNavigation = new("ss:enableNavigation", "0");
            XAttribute hideProgressBar = new("ss:hideProgressBar", "0");
            XAttribute version = new("version", "3");

            rootElement.Add(autoSaveKey);
            rootElement.Add(builderCompatible);
            rootElement.Add(mobileDevices);
            rootElement.Add(secure);
            rootElement.Add(disableBackButton);
            rootElement.Add(enableNavigation);
            rootElement.Add(hideProgressBar);
            rootElement.Add(version);

            return rootElement;
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
            string optionValue = (string)obj["value"];

            if (optionValue != null)
            {
                if (isRow)
                {
                    return new("row", new XAttribute("label", option.GetProperty("label").GetString().Trim()), new XAttribute("value", option.GetProperty("value").GetString().Trim()))
                    {
                        Value = option.GetProperty("optionText").GetString().Trim()
                    };
                }
                else
                {
                    return new("col", new XAttribute("label", option.GetProperty("label").GetString().Trim()))
                    {
                        Value = option.GetProperty("optionText").GetString().Trim()
                    };
                }
            }

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
                commentElement.Add(new XElement("em", commentText));

                //commentElement.Value = commentText;
            }

            xElement?.Add(commentElement);
        }

        private static XElement? GetQuestionTypeElement(JsonElement question)
        {
            XElement? questionType = null;

            switch (question.GetProperty("questionType").GetProperty("type").GetString().Trim())
            {
                case QuestionType.Radio:
                    questionType = new(QuestionType.Radio, new XAttribute("label", question.GetProperty("label").GetString().Trim()));
                    break;
                case QuestionType.Checkbox:
                    questionType = new(QuestionType.Checkbox, new XAttribute("label", question.GetProperty("label").GetString().Trim()));
                    break;
                case QuestionType.Select:
                    questionType = new(QuestionType.Select, new XAttribute("label", question.GetProperty("label").GetString().Trim()));
                    break;
                default:
                    Console.WriteLine("No matching question type found");
                    break;
            }

            var questionOptions = question.GetProperty("questionOptions");

            if (questionOptions.ValueKind == JsonValueKind.Array && questionType is not null && question.GetProperty("questionType").GetProperty("type").GetString().Trim() == "radio")
            {
                bool customValueExists = questionOptions.EnumerateArray().Any(CustomValueExists);

                if (customValueExists)
                {
                    XAttribute attribute = new("values", "order");

                    questionType.Add(attribute);
                }
            }

            return questionType;
        }

        private static bool CustomValueExists(JsonElement element)
        {
            JsonObject obj = JsonNode.Parse(element.ToString()).AsObject();

            string optionValue = (string)obj["value"];

            return optionValue != null;
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
