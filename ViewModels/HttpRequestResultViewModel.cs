using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiTester.Models;

namespace ApiTester.ViewModels
{
    public class HttpRequestResultViewModel : ViewModelBase
    {

        private readonly IFormatterService _formatterService;

        private string _responseBody = string.Empty;

        public HttpRequestResultViewModel(IFormatterService formatterService)
        {
            _formatterService = formatterService;
        }

        private HttpRequestResult _httpRequestResult;
        public HttpRequestResult HttpRequestResult
        {
            get => _httpRequestResult;
            set
            {
                _httpRequestResult = value;

            }
        }

        public async Task SetDetails(HttpRequestResult httpRequestResult)
        {
            HttpRequestResult = httpRequestResult;

            if (HttpRequestResult == null || HttpRequestResult.HttpResponseMessage == null || HttpRequestResult.HttpResponseMessage.Content == null)
            {
                _responseBody = string.Empty;
                return;
            }

            var body = await HttpRequestResult.HttpResponseMessage.Content.ReadAsStringAsync();
            _formatterService.TryFormatJson(body, out var formattedBody);

            _responseBody = formattedBody;
        }


        public int StatusCode => HttpRequestResult?.ResponseCode ?? 0;

        public string ResponseBody => _responseBody;

        public IEnumerable<KeyValuePair<string, string>> Headers => HttpRequestResult?.HttpResponseMessage?.Headers != null
            ? HttpRequestResult.HttpResponseMessage.Headers.ToList().Select(h => new KeyValuePair<string, string>(h.Key, string.Join("\n", h.Value)))
            : [];

    }
}