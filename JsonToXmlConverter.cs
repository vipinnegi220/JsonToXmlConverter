using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

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

                    XDocument xmlDocument = new XDocument();

                    switch (jsonDocument.RootElement.GetProperty("questionType").GetProperty("type").GetString().Trim())
                    {
                        case QuestionType.Radio:
                            XElement radioElement = GetRadioElement(jsonDocument.RootElement);
                            xmlDocument.Add(radioElement);
                            break;
                        case QuestionType.Checkbox:
                            XElement checkboxElement = GetCheckboxElement(jsonDocument.RootElement);
                            xmlDocument.Add(checkboxElement);
                            break;
                        case QuestionType.Select:
                            XElement selectElement = GetSelectElement(jsonDocument.RootElement);
                            xmlDocument.Add(selectElement);
                            break;
                        case QuestionType.Grid:
                            XElement gridElement = GetGridElement(jsonDocument.RootElement);
                            xmlDocument.Add(gridElement);
                            break;
                        default:
                            Console.WriteLine("No matching question type found");
                            break;
                    }

                    xmlDocument.Save(outputXmlFilePath);
                    Console.WriteLine($"Conversion completed. Check {outputXmlFilePath} for the result.");
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

        private static XElement GetRadioElement(JsonElement question)
        {
            XElement radioElement = new XElement(QuestionType.Radio, new XAttribute("label", question.GetProperty("label").GetString().Trim()));
            // Add radio-specific attributes or elements here
            return radioElement;
        }

        private static XElement GetCheckboxElement(JsonElement question)
        {
            XElement checkboxElement = new XElement(QuestionType.Checkbox, new XAttribute("label", question.GetProperty("label").GetString().Trim()));
            // Add checkbox-specific attributes or elements here
            return checkboxElement;
        }

        private static XElement GetSelectElement(JsonElement question)
        {
            XElement selectElement = new XElement(QuestionType.Select, new XAttribute("label", question.GetProperty("label").GetString().Trim()));
            // Add select-specific attributes or elements here
            return selectElement;
        }

        private static XElement GetGridElement(JsonElement question)
        {
            XElement gridElement = new XElement("grid");
            // Add any attributes or elements specific to the grid-type question
            AddGridOptions(question, gridElement);
            return gridElement;
        }

        private static void AddGridOptions(JsonElement question, XElement? xElement)
        {
            var questionOptions = question.GetProperty("questionOptions");

            if (questionOptions.ValueKind == JsonValueKind.Array)
            {
                foreach (var option in questionOptions.EnumerateArray())
                {
                    XElement element = AddGridRow(question, option);
                    xElement?.Add(element);
                }
            }
        }

        private static XElement AddGridRow(JsonElement question, JsonElement option)
        {
            return new XElement("gridRow",
                new XAttribute("label", option.GetProperty("label").GetString().Trim()),
                new XAttribute("value", option.GetProperty("value").GetString().Trim()))
            {
                Value = option.GetProperty("optionText").GetString().Trim()
            };
        }

        private static async Task<string?> GetJsonDataFromApi(string apiUrl)
        {
            try
            {
                using HttpClient httpClient = new HttpClient();
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
