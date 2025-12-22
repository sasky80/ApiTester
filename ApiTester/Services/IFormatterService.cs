/// <summary>
/// Provides methods to format input strings.
/// </summary>
public interface IFormatterService
{
    /// <summary>
    /// Formats a JSON string with indentation.
    /// </summary>
    /// <param name="input">The JSON string to format.</param>
    /// <returns>The formatted JSON string, or an empty string if the input is null or whitespace.</returns>
    /// <exception cref="JsonReaderException">Thrown when the input is not a valid JSON string.</exception>
    string FormatJson(string input);

    /// <summary>
    /// Attempts to format a JSON string with indentation.
    /// </summary>
    /// <param name="input">The JSON string to format.</param>
    /// <param name="output">The formatted JSON string, or the original input if formatting fails.</param>
    /// <returns>True if the input was successfully formatted; otherwise, false.</returns>
    bool TryFormatJson(string input, out string output);
}