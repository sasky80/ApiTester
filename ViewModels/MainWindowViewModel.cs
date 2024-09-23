namespace ApiTester.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net.Http;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ApiTester.Builders;
    using ApiTester.Models;
    using Newtonsoft.Json;
    using ReactiveUI;
    using ApiTester.Services;

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
            set
            {
                if (value)
                {
                    this.RaiseAndSetIfChanged(ref _selectedContentType, "application/json");
                }
            }
        }

        public bool AppXmlEnabled
        {
            get => _selectedContentType == "application/xml";
            set
            {
                if (value)
                {
                    this.RaiseAndSetIfChanged(ref _selectedContentType, "application/xml");
                }
            }
        }

        public bool TextPlainEnabled
        {
            get => _selectedContentType == "text/plain";
            set
            {
                if (value)
                {
                    this.RaiseAndSetIfChanged(ref _selectedContentType, "text/plain");
                }
            }
        }

        private string _url;
        public string Url
        {
            get => _url;
            set => this.RaiseAndSetIfChanged(ref _url, value);
        }

        private string _requestBody = string.Empty;
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

        private int _messageCount = 1;
        public int MessageCount
        {
            get => _messageCount;
            set => this.RaiseAndSetIfChanged(ref _messageCount, value);
        }

        private bool _sendInParallel = false;
        public bool SendInParallel
        {
            get => _sendInParallel;
            set => this.RaiseAndSetIfChanged(ref _sendInParallel, value);
        }

        private int _numberOfThreads = 1;
        private readonly IPersistenceService _persistenceService;

        public int NumberOfThreads
        {
            get => _numberOfThreads;
            set => this.RaiseAndSetIfChanged(ref _numberOfThreads, value);
        }

        public ReactiveCommand<Unit, Unit> SendCommand { get; }
        public ReactiveCommand<Unit, Unit> FormatCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }

        public ReactiveCommand<Unit, Unit> LoadCommand { get; }

        public ObservableCollection<HttpRequestResult> HttpRequestResults { get; }

        public MainWindowViewModel(IPersistenceService persistenceService)
        {
            SendCommand = ReactiveCommand.CreateFromTask(SendRequestsInParallel);
            FormatCommand = ReactiveCommand.Create(Format);
            HttpRequestResults = new ObservableCollection<HttpRequestResult>();
            SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
            LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
            _persistenceService = persistenceService;
        }

        public async Task SendRequestsInParallel()
        {
            Format();

            var semaphore = new SemaphoreSlim(NumberOfThreads);
            var tasks = Enumerable.Range(0, MessageCount).Select(async i =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var result = await SendHttpRequestAsync(i + 1);
                    lock (HttpRequestResults)
                    {
                        HttpRequestResults.Add(result);
                    }
                }
                catch (Exception ex)
                {
                    var errResult = new HttpRequestResult
                    {
                        RequestNumber = i + 1,
                        ResponseCode = 0,
                        ResponseContent = ex.Message,
                        RequestDuration = TimeSpan.Zero
                    };
                    lock (HttpRequestResults)
                    {
                        HttpRequestResults.Add(errResult);
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }

        private async Task<HttpRequestResult> SendHttpRequestAsync(int requestNumber)
        {
            using (var client = new HttpClient())
            {
                var request = MessageBuilder.BuildRequest(Url, HttpMethod, ContentType, RequestBody);
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await client.SendAsync(request);
                stopwatch.Stop();

                return new HttpRequestResult
                {
                    RequestNumber = requestNumber,
                    ResponseCode = (int)response.StatusCode,
                    ResponseContent = (await response.Content.ReadAsStringAsync())?.Substring(0, 100) ?? string.Empty,
                    RequestDuration = stopwatch.Elapsed
                };
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
            if (string.IsNullOrWhiteSpace(json))
            {
                return string.Empty;
            }
            object? parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }

        private HttpRequestPersistentDataModel ToPersistentDataModel()
        {
            return new HttpRequestPersistentDataModel
            {
                HttpMethod = HttpMethod,
                Url = Url,
                AppJsonEnabled = AppJsonEnabled,
                AppXmlEnabled = AppXmlEnabled,
                TextPlainEnabled = TextPlainEnabled,
                MessageCount = MessageCount,
                SendInParallel = SendInParallel,
                NumberOfThreads = NumberOfThreads,
                RequestBody = RequestBody
            };
        }

        private void FromPersistentDataModel(HttpRequestPersistentDataModel data)
        {
            HttpMethod = data.HttpMethod;
            Url = data.Url;
            AppJsonEnabled = data.AppJsonEnabled;
            AppXmlEnabled = data.AppXmlEnabled;
            TextPlainEnabled = data.TextPlainEnabled;
            MessageCount = data.MessageCount;
            SendInParallel = data.SendInParallel;
            NumberOfThreads = data.NumberOfThreads;
            RequestBody = data.RequestBody;
        }

        private async Task SaveAsync()
        {
            await _persistenceService.SaveAsync(ToPersistentDataModel());
        }

        private async Task LoadAsync()
        {
            var data = await _persistenceService.LoadAsync();

            if (data != null)
            {
                FromPersistentDataModel(data);
            }
        }
    }
}
