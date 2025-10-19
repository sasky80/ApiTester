namespace ApiTester.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using ApiTester.Models;

    public static class MessageBuilder
    {
        // BuildRequest method
        // This method is used to build an HttpRequestMessage object
        // based on the provided parameters.
        // The method accepts a URL, method, content type, and body as parameters.
        // The method creates a new HttpRequestMessage object with the provided method and URL.
        // If the method is POST or PUT, the method creates a new StringContent object
        // with the provided body and content type.
        // The method sets the content of the HttpRequestMessage object to the created StringContent object
        // and sets the content type header of the HttpRequestMessage object to the provided content type.
        // The method returns the created HttpRequestMessage object.

        public static HttpRequestMessage BuildRequest(string url, string method, string contentType, string body, IEnumerable<HeaderEntry>? headers = null, string? authScheme = null, string? basicUser = null, string? basicPass = null, string? bearerToken = null, string? apiKey = null, string? apiKeyLocation = null, string? apiKeyName = null)
        {
            var request = new HttpRequestMessage(new HttpMethod(method), url);

            if (method.Equals("POST", StringComparison.OrdinalIgnoreCase) || method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(body))
                {
                    request.Content = new StringContent(body);
                    // Only set Content-Type header if a real content type was selected (not 'None')
                    if (!string.IsNullOrWhiteSpace(contentType) && !string.Equals(contentType, "None", StringComparison.OrdinalIgnoreCase))
                    {
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                    }
                }
            }

            if (headers != null)
            {
                foreach (var h in headers)
                {
                    if (!string.IsNullOrWhiteSpace(h?.Name))
                    {
                        // Use TryAddWithoutValidation to allow any header names/values
                        request.Headers.TryAddWithoutValidation(h.Name, h?.Value ?? string.Empty);
                    }
                }
            }

            // Apply authentication (if not already present in headers)
            if (!string.IsNullOrWhiteSpace(authScheme))
            {
                var scheme = authScheme?.Trim();
                if (string.Equals(scheme, "Basic", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(basicUser))
                {
                    var creds = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{basicUser}:{basicPass}"));
                    if (!request.Headers.Contains("Authorization"))
                    {
                        request.Headers.TryAddWithoutValidation("Authorization", $"Basic {creds}");
                    }
                }
                else if (string.Equals(scheme, "Bearer", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(bearerToken))
                {
                    if (!request.Headers.Contains("Authorization"))
                    {
                        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {bearerToken}");
                    }
                }
                else if (string.Equals(scheme, "ApiKey", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(apiKey))
                {
                    if (string.Equals(apiKeyLocation, "Header", StringComparison.OrdinalIgnoreCase))
                    {
                        var headerName = string.IsNullOrWhiteSpace(apiKeyName) ? "X-API-KEY" : apiKeyName;
                        request.Headers.TryAddWithoutValidation(headerName, apiKey);
                    }
                    else // Query
                    {
                        var uriBuilder = new UriBuilder(request.RequestUri!);
                        var q = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
                        q["api_key"] = apiKey;
                        uriBuilder.Query = q.ToString();
                        request.RequestUri = uriBuilder.Uri;
                    }
                }
            }

            return request;
        }
    }
}
