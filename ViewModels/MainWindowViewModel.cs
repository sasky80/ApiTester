namespace ApiTester.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive;
using System.Windows.Input;
using ApiTester.Builders;
using Newtonsoft.Json;
using ReactiveUI;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<string> HttpMethods { get; } = new ObservableCollection<string> { "GET", "POST", "PUT", "DELETE" };

    private string _selectedHttpMethod = "GET";
    public string HttpMethod
    {
        get => _selectedHttpMethod;
        set => this.RaiseAndSetIfChanged(ref _selectedHttpMethod, value);
    }

    private string _selectedContentType = "application/json";

    public string ContentType
    {
        get => _selectedContentType;
        set => this.RaiseAndSetIfChanged(ref _selectedContentType, value);
    }

    public bool AppJsonEnabled
    {
        get => _selectedContentType == "application/json";
        set => this.RaiseAndSetIfChanged(ref _selectedContentType, "application/json");
    }

    public bool AppXmlEnabled
    {
        get => _selectedContentType == "application/xml";
        set => this.RaiseAndSetIfChanged(ref _selectedContentType, "application/xml");
    }

    public bool TextPlainEnabled
    {
        get => _selectedContentType == "text/plain";
        set => this.RaiseAndSetIfChanged(ref _selectedContentType, "text/plain");
    }

    private string _url;
    public string Url
    {
        get => _url;
        set => this.RaiseAndSetIfChanged(ref _url, value);
    }

    private string _requestBody;
    public string RequestBody
    {
        get => _requestBody;
        set => this.RaiseAndSetIfChanged(ref _requestBody, value);
    }

    private string _validationStatus;

    public string ValidationStatus
    {
        get => _validationStatus;
        set => this.RaiseAndSetIfChanged(ref _validationStatus, value);
    }

    private int _messageCount;
    public int MessageCount
    {
        get => _messageCount;
        set => this.RaiseAndSetIfChanged(ref _messageCount, value);
    }

    private bool _sendInParallel;
    public bool SendInParallel
    {
        get => _sendInParallel;
        set => this.RaiseAndSetIfChanged(ref _sendInParallel, value);
    }

    public ReactiveCommand<Unit, Unit> SendCommand { get; }
    public ReactiveCommand<Unit, Unit> FormatCommand { get; }

    public MainWindowViewModel()
    {
        SendCommand = ReactiveCommand.Create(Send);
        FormatCommand = ReactiveCommand.Create(Format);
    }

    private async void Send()
    {
        Format();
        using (var client = new HttpClient())
        {
            try
            {
                var requests = MessageBuilder.BuildRequest(Url, HttpMethod, ContentType, RequestBody);

                var request = new HttpRequestMessage(new HttpMethod(HttpMethod), Url)
                {
                    Content = new StringContent(RequestBody)
                };

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                ValidationStatus = "Request sent successfully.";
            }
            catch (Exception ex)
            {
                ValidationStatus = $"Error: {ex.Message}";
            }
        }
    }
    private void Format()
    {
        ValidationStatus = string.Empty;
        try
        {
            RequestBody = FormatJson(RequestBody);
        }
        catch (Exception ex)
        {
            ValidationStatus = ex.Message;
        }
    }

    private static string FormatJson(string json)
    {
        object? parsedJson = JsonConvert.DeserializeObject(json);

        return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
    }
}
