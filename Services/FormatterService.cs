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
}