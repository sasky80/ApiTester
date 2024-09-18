namespace ApiTester.Models;
using System;

public class HttpRequestResult
{
    public int RequestNumber { get; set; }
    public int ResponseCode { get; set; }
    public string ResponseContent { get; set; }
    public TimeSpan RequestDuration { get; set; }
}
