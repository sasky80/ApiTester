using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiTester.Extensions;
using ApiTester.Models;
using ApiTester.Services;

namespace ApiTester.ViewModels
{
    public class HttpRequestResultViewModel(IFormatterService formatterService) : ViewModelBase
    {

        private readonly IFormatterService _formatterService = formatterService;

        private string _responseBody = string.Empty;

        public required HttpRequestResult HttpRequestResult { get; set; }

        public async Task SetDetails(HttpRequestResult httpRequestResult)
        {
            HttpRequestResult = httpRequestResult;

            if (HttpRequestResult == null || HttpRequestResult.HttpResponseMessage == null || HttpRequestResult.HttpResponseMessage.Content == null)
            {
                _responseBody = string.Empty;
                return;
            }

            var body = await HttpRequestResult.HttpResponseMessage.Content.ReadAsStringAsync();
            var contentType = HttpRequestResult.HttpResponseMessage.Content.Headers.ContentType?.MediaType;

            if (ContentTypeHelper.IsJson(contentType))
            {
                _formatterService.TryFormatJson(body, out var formattedBody);
                _responseBody = formattedBody;
            }
            else if (ContentTypeHelper.IsXml(contentType))
            {
                _formatterService.TryFormatXml(body, out var formattedBody);
                _responseBody = formattedBody;
            }
            else
            {
                _responseBody = body;
            }
        }


        public int StatusCode => HttpRequestResult?.ResponseCode ?? 0;

        public string ResponseBody => _responseBody;

        public IEnumerable<KeyValuePair<string, string>> Headers => HttpRequestResult?.HttpResponseMessage?.Headers != null
            ? HttpRequestResult.HttpResponseMessage.Headers.ToList().Select(h => new KeyValuePair<string, string>(h.Key, string.Join("\n", h.Value)))
            : [];

    }
}