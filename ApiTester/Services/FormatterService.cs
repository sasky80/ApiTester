using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <inheritdoc/>
public class FormatterService : IFormatterService
{
    /// <inheritdoc/>
    public string FormatJson(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var json = JToken.Parse(input).ToString(Formatting.Indented);
        return json;
    }

    public string FormatXml(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var document = XDocument.Parse(input);
        return document.ToString(SaveOptions.None);
    }

    /// <inheritdoc />
    public bool TryFormatJson(string input, out string output)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            output = string.Empty;
            return false;
        }

        try
        {
            output = FormatJson(input);
            return true;
        }
        catch (JsonReaderException)
        {
            output = input;
            return false;
        }
    }

    public bool TryFormatXml(string input, out string output)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            output = string.Empty;
            return false;
        }

        try
        {
            output = FormatXml(input);
            return true;
        }
        catch (System.Xml.XmlException)
        {
            output = input;
            return false;
        }
    }
}