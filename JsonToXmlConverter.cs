// JsonToXmlConverter.cs
using System.Text.Json;
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
                        XDocument xmlDocument = new();
                        XElement rootElementXml = new("root");

                        foreach (var arrayItem in jsonDocument.RootElement.EnumerateArray())
                        {
                            XElement radioElement = new("radio", new XAttribute("label", arrayItem.GetProperty("label").GetString().Trim()));
                            radioElement.Add(new XElement("title", arrayItem.GetProperty("title").GetString().Trim()));

                            // Ensure both opening and closing tags for <comment>
                            string commentText = arrayItem.GetProperty("instructionText").GetString().Trim();

                            XElement commentElement = new("comment");

                            if (!string.IsNullOrEmpty(commentText))
                            {
                                commentElement.Value = commentText;
                            }

                            radioElement.Add(commentElement);

                            var questionOptions = arrayItem.GetProperty("questionOptions");
                            if (questionOptions.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var option in questionOptions.EnumerateArray())
                                {
                                    XElement rowElement = new("row", new XAttribute("label", option.GetProperty("label").GetString().Trim()))
                                    {
                                        Value = option.GetProperty("optionText").GetString().Trim()
                                    };

                                    radioElement.Add(rowElement);
                                }
                            }

                            rootElementXml.Add(radioElement);

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

        private static async Task<string> GetJsonDataFromApi(string apiUrl)
        {
            try
            {
                using (HttpClient httpClient = new())
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
    }
}
