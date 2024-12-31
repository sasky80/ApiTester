using System;
using System.Net.Http;

namespace ApiTester.Models
{
    public class HttpRequestResult
    {
        public int RequestNumber { get; set; }
        public int ResponseCode { get; set; }
        public string ResponseContent { get; set; }
        public TimeSpan RequestDuration { get; set; }
        public HttpResponseMessage HttpResponseMessage { get; set; }
    }
}