using System;

namespace ApiTester.Extensions
{
    internal static class ContentTypeHelper
    {
        internal static bool IsJson(string? contentType)
        {
            return MatchesMediaType(contentType, "application/json")
                   || MatchesMediaType(contentType, "text/json")
                   || HasStructuredSyntaxSuffix(contentType, "+json");
        }

        internal static bool IsXml(string? contentType)
        {
            return MatchesMediaType(contentType, "application/xml")
                   || MatchesMediaType(contentType, "text/xml")
                   || HasStructuredSyntaxSuffix(contentType, "+xml");
        }

        private static bool MatchesMediaType(string? contentType, string expected)
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return false;
            }

            var mediaType = ExtractMediaType(contentType);
            return string.Equals(mediaType, expected, StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasStructuredSyntaxSuffix(string? contentType, string suffix)
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return false;
            }

            var mediaType = ExtractMediaType(contentType);
            return mediaType.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
        }

        private static string ExtractMediaType(string contentType)
        {
            var separatorIndex = contentType.IndexOf(';');
            var mediaType = separatorIndex >= 0 ? contentType[..separatorIndex] : contentType;
            return mediaType.Trim();
        }
    }
}
