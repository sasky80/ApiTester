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

        public static HttpRequestMessage BuildRequest(string url, string method, string contentType, string body, IEnumerable<HeaderEntry>? headers = null)
        {
            var request = new HttpRequestMessage(new HttpMethod(method), url);

            if (method.Equals("POST", StringComparison.OrdinalIgnoreCase) || method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(body))
                {
                    request.Content = new StringContent(body);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
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

            return request;
        }
    }
}
