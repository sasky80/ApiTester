namespace ApiTester.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Reactive;
    using System.Threading;
    using System.Threading.Tasks;
    using ApiTester.Builders;
    using ApiTester.Models;
    using ApiTester.Extensions;
    using Newtonsoft.Json;
    using ReactiveUI;
    using ApiTester.Services;
    using ApiTester.Views;
    using System.ComponentModel;
    using System.Collections.Specialized;
    using Avalonia.Controls.ApplicationLifetimes;
    using Microsoft.Extensions.DependencyInjection;

    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IPersistenceService _persistenceService;
        private readonly IFormatterService _formatterService;
        private readonly IServiceProvider _serviceProvider;

        // Constants for content types
        private const string ContentTypeJson = "application/json";
        private const string ContentTypeXml = "application/xml";
        private const string ContentTypeText = "text/plain";
        private const string ContentTypeNone = "None";

        public ObservableCollection<string> HttpMethods { get; } = new ObservableCollection<string> { "GET", "POST", "PUT", "DELETE" };

        private string _selectedHttpMethod = "GET";
        public string HttpMethod
        {
            get => _selectedHttpMethod;
            set => this.RaiseAndSetIfChanged(ref _selectedHttpMethod, value);
        }

        private string _selectedContentType = ContentTypeJson;
        public string ContentType
        {
            get => _selectedContentType;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedContentType, value);
                UpdateContentTypeHeader();
                NotifyContentTypeChanged();
            }
        }

        public bool AppJsonEnabled
        {
            get => _selectedContentType == ContentTypeJson;
            set
            {
                if (value)
                {
                    this.RaiseAndSetIfChanged(ref _selectedContentType, ContentTypeJson);
                    UpdateContentTypeHeader();
                    NotifyContentTypeChanged();
                }
            }
        }

        public bool NoneEnabled
        {
            get => string.Equals(_selectedContentType, ContentTypeNone, StringComparison.OrdinalIgnoreCase);
            set
            {
                if (value)
                {
                    this.RaiseAndSetIfChanged(ref _selectedContentType, ContentTypeNone);
                    UpdateContentTypeHeader();
                    NotifyContentTypeChanged();
                }
            }
        }

        public bool AppXmlEnabled
        {
            get => _selectedContentType == ContentTypeXml;
            set
            {
                if (value)
                {
                    this.RaiseAndSetIfChanged(ref _selectedContentType, ContentTypeXml);
                    UpdateContentTypeHeader();
                    NotifyContentTypeChanged();
                }
            }
        }

        public bool TextPlainEnabled
        {
            get => _selectedContentType == ContentTypeText;
            set
            {
                if (value)
                {
                    this.RaiseAndSetIfChanged(ref _selectedContentType, ContentTypeText);
                    UpdateContentTypeHeader();
                    NotifyContentTypeChanged();
                }
            }
        }

        private void NotifyContentTypeChanged()
        {
            // Derived boolean properties need explicit notifications
            this.RaisePropertyChanged(nameof(AppJsonEnabled));
            this.RaisePropertyChanged(nameof(AppXmlEnabled));
            this.RaisePropertyChanged(nameof(TextPlainEnabled));
            this.RaisePropertyChanged(nameof(NoneEnabled));
            // Other is true when ContentType is not one of the knowns
            var isOther = !(
                string.Equals(ContentType, ContentTypeJson, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(ContentType, ContentTypeXml, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(ContentType, ContentTypeText, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(ContentType, ContentTypeNone, StringComparison.OrdinalIgnoreCase)
            );

            // Update backing field without invoking the setter to avoid recursion
            this.RaiseAndSetIfChanged(ref _otherEnabled, isOther);
        }

        private string _url = "https://cat-fact.herokuapp.com/facts";
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

        private string _validationStatus = string.Empty;
        public string ValidationStatus
        {
            get => _validationStatus;
            set => this.RaiseAndSetIfChanged(ref _validationStatus, value);
        }

        private string _copyStatusMessage = string.Empty;
        public string CopyStatusMessage
        {
            get => _copyStatusMessage;
            set => this.RaiseAndSetIfChanged(ref _copyStatusMessage, value);
        }

        private int _messageCount = 1;
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

        private int _numberOfThreads = 1;
        public int NumberOfThreads
        {
            get => _numberOfThreads;
            set => this.RaiseAndSetIfChanged(ref _numberOfThreads, value);
        }

        public ReactiveCommand<Unit, Unit> SendCommand { get; }
        public ReactiveCommand<Unit, Unit> FormatCommand { get; }
        public ReactiveCommand<Unit, Unit> CopyAsCurlCommand { get; }
        public ReactiveCommand<Unit, Unit> CopyAsPowerShellCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }

        public ReactiveCommand<HttpRequestResult, Unit> ShowDetailsCommand { get; }


        public ReactiveCommand<Unit, Unit> LoadCommand { get; }

        public ObservableCollection<HttpRequestResult> HttpRequestResults { get; }

        public ObservableCollection<string> ApiKeyLocations { get; } = new ObservableCollection<string> { "Header", "Query" };

        public ObservableCollection<HeaderEntry> Headers { get; } = new();
        private HeaderEntry? _selectedHeader;
        public HeaderEntry? SelectedHeader
        {
            get => _selectedHeader;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedHeader, value);
                // cannot remove a fixed header (Content-Type when present)
                CanRemoveHeader = _selectedHeader != null && !_selectedHeader.IsFixed;
                CanClearHeaders = Headers.Count > 0 && Headers.Any(h => !h.IsFixed);
            }
        }

        public ReactiveCommand<Unit, Unit> AddHeaderRowCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveHeaderRowCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearHeadersCommand { get; }

        private string _headersValidationMessage = string.Empty;
        public string HeadersValidationMessage
        {
            get => _headersValidationMessage;
            set => this.RaiseAndSetIfChanged(ref _headersValidationMessage, value);
        }

        private string _customContentType = string.Empty;
        public string CustomContentType
        {
            get => _customContentType;
            set
            {
                this.RaiseAndSetIfChanged(ref _customContentType, value);
                // If Other is selected, update ContentType to match manual entry
                if (OtherEnabled)
                {
                    ContentType = value;
                }
            }
        }

        private bool _otherEnabled;
        public bool OtherEnabled
        {
            get => _otherEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _otherEnabled, value);
                if (value)
                {
                    // activate other: set ContentType to the custom value (or empty) so the predefined radio buttons become false
                    ContentType = string.IsNullOrWhiteSpace(CustomContentType) ? string.Empty : CustomContentType;
                }
            }
        }

        private bool _canRemoveHeader;
        public bool CanRemoveHeader
        {
            get => _canRemoveHeader;
            set => this.RaiseAndSetIfChanged(ref _canRemoveHeader, value);
        }

        private bool _canClearHeaders = true;
        public bool CanClearHeaders
        {
            get => _canClearHeaders;
            set => this.RaiseAndSetIfChanged(ref _canClearHeaders, value);
        }

        public MainWindowViewModel(IPersistenceService persistenceService, IFormatterService formatterService, IServiceProvider serviceProvider)
        {
            SendCommand = ReactiveCommand.CreateFromTask(SendRequestsInParallel);
            FormatCommand = ReactiveCommand.Create(Format);
            HttpRequestResults = new ObservableCollection<HttpRequestResult>();
            SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
            LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);

            ShowDetailsCommand = ReactiveCommand.CreateFromTask<HttpRequestResult>(ShowDetailsAsync);
            CopyAsCurlCommand = ReactiveCommand.CreateFromTask(CopyAsCurlAsync);
            CopyAsPowerShellCommand = ReactiveCommand.CreateFromTask(CopyAsPowerShellAsync);
            _persistenceService = persistenceService;
            _formatterService = formatterService;
            _serviceProvider = serviceProvider;

            // Initialize headers: add Content-Type only when a content type is selected
            if (Headers.Count == 0 && !string.Equals(ContentType, ContentTypeNone, StringComparison.OrdinalIgnoreCase))
            {
                Headers.Add(new HeaderEntry { Name = "Content-Type", Value = ContentType, IsFixed = true });
            }

            UpdateContentTypeHeader();
            AttachToFirstHeader();

            AddHeaderRowCommand = ReactiveCommand.Create(() => Headers.Add(new HeaderEntry()));
            RemoveHeaderRowCommand = ReactiveCommand.Create(RemoveSelectedHeader);
            ClearHeadersCommand = ReactiveCommand.Create(ClearHeaders);

            Headers.CollectionChanged += Headers_CollectionChanged;
        }

        private async Task CopyAsCurlAsync()
        {
            try
            {
                // Build curl
                var sb = new System.Text.StringBuilder();
                sb.Append("curl");
                sb.Append(" -X ").Append(HttpMethod);

                // Prepare headers list (avoid modifying original collection)
                var headerPairs = Headers
                    .Where(h => !string.IsNullOrWhiteSpace(h?.Name))
                    .Select(h => new KeyValuePair<string, string>(h.Name!, h.Value ?? string.Empty))
                    .ToList();

                var url = Url;
                ApplyAuthentication(headerPairs, ref url);

                // Headers
                foreach (var h in headerPairs)
                {
                    sb.Append(" -H ")
                      .Append('"')
                      .Append(h.Key)
                      .Append(": ")
                      .Append(h.Value?.Replace("\"", "\\\"") ?? string.Empty)
                      .Append('"');
                }

                // Body
                if (!string.IsNullOrEmpty(RequestBody))
                {
                    sb.Append(" -d ")
                      .Append('"')
                      .Append(RequestBody.Replace("\"", "\\\""))
                      .Append('"');
                }

                sb.Append(' ').Append(url);

                var curl = sb.ToString();

                // Copy to clipboard via Avalonia (preferred cross-platform API)
                await CopyToClipboardAsync(curl, "Copied curl to clipboard");
            }
            catch
            {
                // ignore
            }
        }

        private async Task CopyAsPowerShellAsync()
        {
            try
            {
                // Build a PowerShell Invoke-RestMethod snippet
                var headersPairs = Headers.Where(h => !string.IsNullOrWhiteSpace(h?.Name)).ToDictionary(h => h.Name!, h => h.Value ?? string.Empty, StringComparer.OrdinalIgnoreCase);

                var uri = Url;
                ApplyAuthentication(headersPairs.ToList(), ref uri);

                // Build headers hashtable literal
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("$headers = @{");
                foreach (var kv in headersPairs)
                {
                    var val = kv.Value.Replace("`", "``").Replace("\"", "\"\"");
                    sb.AppendLine($"    '{kv.Key}' = '{val}'");
                }
                sb.AppendLine("}");
                sb.AppendLine($"$body = @'\n{RequestBody}\n'@");
                sb.AppendLine($"Invoke-RestMethod -Uri '{uri}' -Method {HttpMethod} -Headers $headers -Body $body");

                var psSnippet = sb.ToString();

                // Copy to clipboard
                await CopyToClipboardAsync(psSnippet, "Copied PowerShell snippet to clipboard");
            }
            catch
            {
                // ignore
            }
        }

        // Authentication
        private string _authScheme = "None"; // None, Basic, Bearer, ApiKey
        public string AuthScheme
        {
            get => _authScheme;
            set => this.RaiseAndSetIfChanged(ref _authScheme, value);
        }

        // Basic
        private string _basicUsername = string.Empty;
        public string BasicUsername
        {
            get => _basicUsername;
            set => this.RaiseAndSetIfChanged(ref _basicUsername, value);
        }

        private string _basicPassword = string.Empty;
        public string BasicPassword
        {
            get => _basicPassword;
            set => this.RaiseAndSetIfChanged(ref _basicPassword, value);
        }

        // Bearer
        private string _bearerToken = string.Empty;
        public string BearerToken
        {
            get => _bearerToken;
            set => this.RaiseAndSetIfChanged(ref _bearerToken, value);
        }

        // ApiKey
        private string _apiKey = string.Empty;
        public string ApiKey
        {
            get => _apiKey;
            set => this.RaiseAndSetIfChanged(ref _apiKey, value);
        }

        private string _apiKeyLocation = "Header"; // Header or Query
        public string ApiKeyLocation
        {
            get => _apiKeyLocation;
            set => this.RaiseAndSetIfChanged(ref _apiKeyLocation, value);
        }

        private string _apiKeyName = "X-API-KEY";
        public string ApiKeyName
        {
            get => _apiKeyName;
            set => this.RaiseAndSetIfChanged(ref _apiKeyName, value);
        }

        private void UpdateContentTypeHeader()
        {
            // If 'None' is selected, remove any Content-Type header rows
            if (string.Equals(ContentType, ContentTypeNone, StringComparison.OrdinalIgnoreCase))
            {
                var existing = Headers.FirstOrDefault(h => string.Equals(h.Name, "Content-Type", StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    Headers.Remove(existing);
                }
            }
            else
            {
                // Ensure first row exists and represents Content-Type
                var first = Headers.FirstOrDefault();
                if (first == null || !string.Equals(first.Name, "Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    // If a Content-Type exists elsewhere, move it to the front
                    var existing = Headers.FirstOrDefault(h => string.Equals(h.Name, "Content-Type", StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        Headers.Remove(existing);
                        existing.Value = ContentType;
                        existing.IsFixed = true;
                        Headers.Insert(0, existing);
                    }
                    else
                    {
                        Headers.Insert(0, new HeaderEntry { Name = "Content-Type", Value = ContentType, IsFixed = true });
                    }
                }
                else
                {
                    first.Name = "Content-Type";
                    first.Value = ContentType;
                    first.IsFixed = true;
                }
            }

            // Cannot remove fixed headers (the Content-Type when present)
            CanRemoveHeader = SelectedHeader != null && !_selectedHeader?.IsFixed == true;
            CanClearHeaders = Headers.Any(h => !h.IsFixed);

            // Ensure we are listening to first header changes
            AttachToFirstHeader();
        }

        private void Headers_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ValidateHeaders();
            AttachToFirstHeader();
        }

        private HeaderEntry? _observedFirstHeader;

        private void AttachToFirstHeader()
        {
            try
            {
                if (_observedFirstHeader != null)
                {
                    _observedFirstHeader.PropertyChanged -= FirstHeader_PropertyChanged;
                    _observedFirstHeader = null;
                }

                var first = Headers.FirstOrDefault();
                if (first != null)
                {
                    _observedFirstHeader = first;
                    _observedFirstHeader.PropertyChanged += FirstHeader_PropertyChanged;
                }
            }
            catch
            {
                // swallow any subscription errors
            }
        }

        private void FirstHeader_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not HeaderEntry hdr) return;

            if (string.Equals(e.PropertyName, nameof(HeaderEntry.Value), StringComparison.OrdinalIgnoreCase))
            {
                // Only react if this is the Content-Type row
                if (string.Equals(hdr.Name, "Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    // Avoid recursion: only update if different
                    if (!string.Equals(ContentType, hdr.Value, StringComparison.Ordinal))
                    {
                        var newCt = hdr.Value ?? string.Empty;
                        ContentType = newCt;

                        // If the new content-type is not one of the known types, ensure Other is enabled and CustomContentType shows it
                        var isKnown = string.Equals(newCt, "application/json", StringComparison.OrdinalIgnoreCase)
                                      || string.Equals(newCt, "application/xml", StringComparison.OrdinalIgnoreCase)
                                      || string.Equals(newCt, "text/plain", StringComparison.OrdinalIgnoreCase);

                        if (!isKnown)
                        {
                            // set backing field to avoid re-entering the OtherEnabled setter
                            this.RaiseAndSetIfChanged(ref _otherEnabled, true);
                            CustomContentType = newCt;
                            this.RaisePropertyChanged(nameof(OtherEnabled));
                        }
                    }
                }
            }
            else if (string.Equals(e.PropertyName, nameof(HeaderEntry.Name), StringComparison.OrdinalIgnoreCase))
            {
                // Ensure the first header keeps the Content-Type name
                if (hdr == Headers.FirstOrDefault())
                {
                    if (!string.Equals(hdr.Name, "Content-Type", StringComparison.OrdinalIgnoreCase))
                    {
                        hdr.Name = "Content-Type";
                    }
                }
            }
        }

        private void RemoveSelectedHeader()
        {
            if (SelectedHeader != null && SelectedHeader != Headers.FirstOrDefault())
            {
                Headers.Remove(SelectedHeader);
            }

            if (Headers.Count == 0)
            {
                Headers.Add(new HeaderEntry());
            }

            ValidateHeaders();
            UpdateContentTypeHeader();
        }

        private void ClearHeaders()
        {
            // Keep the first Content-Type row, remove others
            var contentTypeRow = Headers.FirstOrDefault();
            Headers.Clear();
            if (contentTypeRow != null)
            {
                Headers.Add(new HeaderEntry { Name = "Content-Type", Value = ContentType });
            }
            else
            {
                Headers.Add(new HeaderEntry { Name = "Content-Type", Value = ContentType });
            }

            ValidateHeaders();
            UpdateContentTypeHeader();
        }

        private void ValidateHeaders()
        {
            // Duplicate name check (case-insensitive), ignoring empty names
            var duplicates = Headers
                .Where(h => !string.IsNullOrWhiteSpace(h.Name))
                .GroupBy(h => h.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            HeadersValidationMessage = duplicates.Count > 0
                ? $"Duplicate header name(s): {string.Join(", ", duplicates)}"
                : string.Empty;
        }

        public async Task SendRequestsInParallel()
        {
            if (!TryFormatRequestBody())
            {
                return;
            }

            HttpRequestResults.Clear();

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
                var request = MessageBuilder.BuildRequest(Url, HttpMethod, ContentType, RequestBody, Headers, AuthScheme, BasicUsername, BasicPassword, BearerToken, ApiKey, ApiKeyLocation, ApiKeyName);
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await client.SendAsync(request);
                stopwatch.Stop();

                return new HttpRequestResult
                {
                    RequestNumber = requestNumber,
                    ResponseCode = (int)response.StatusCode,
                    ResponseContent = (await response.Content.ReadAsStringAsync()),
                    RequestDuration = stopwatch.Elapsed,
                    HttpResponseMessage = response
                };
            }
        }

        private void Format()
        {
            _ = TryFormatRequestBody();
        }

        private bool TryFormatRequestBody()
        {
            ValidationStatus = string.Empty;

            if (string.IsNullOrWhiteSpace(RequestBody))
            {
                return true;
            }

            try
            {
                RequestBody = ContentTypeHelper.IsJson(ContentType)
                    ? _formatterService.FormatJson(RequestBody)
                    : ContentTypeHelper.IsXml(ContentType)
                        ? _formatterService.FormatXml(RequestBody)
                        : RequestBody;
                return true;
            }
            catch (JsonReaderException ex)
            {
                ValidationStatus = $"Request body is not valid JSON: {ex.Message}";
            }
            catch (System.Xml.XmlException ex)
            {
                ValidationStatus = $"Request body is not valid XML: {ex.Message}";
            }
            catch (Exception ex)
            {
                ValidationStatus = $"Formatting failed: {ex.Message}";
            }

            return false;
        }

        private HttpRequestPersistentDataModel ToPersistentDataModel()
        {
            return new HttpRequestPersistentDataModel
            {
                HttpMethod = HttpMethod,
                Url = Url,
                MessageCount = MessageCount,
                SendInParallel = SendInParallel,
                NumberOfThreads = NumberOfThreads,
                RequestBody = RequestBody,
                Headers = Headers.Select(h => new SerializableHeader { Name = h.Name, Value = h.Value }).ToList()
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
            // Load headers and derive ContentType from Content-Type header (if present)
            Headers.Clear();
            if (data.Headers != null && data.Headers.Count > 0)
            {
                // Prefer Content-Type header and place it as the first row
                var ct = data.Headers.FirstOrDefault(h => string.Equals(h.Name, "Content-Type", StringComparison.OrdinalIgnoreCase));
                if (ct != null)
                {
                    Headers.Add(new HeaderEntry { Name = "Content-Type", Value = ct.Value, IsFixed = true });
                }

                foreach (var h in data.Headers.Where(h => !string.Equals(h.Name, "Content-Type", StringComparison.OrdinalIgnoreCase)))
                {
                    Headers.Add(new HeaderEntry { Name = h.Name, Value = h.Value, IsFixed = false });
                }
            }
            else
            {
                // No saved headers: keep a default Content-Type row
                Headers.Add(new HeaderEntry { Name = "Content-Type", Value = ContentType, IsFixed = true });
            }

            // If a Content-Type header was present, set ContentType from it
            var first = Headers.FirstOrDefault();
            if (first != null && string.Equals(first.Name, "Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                // If persisted value is empty, interpret as None
                ContentType = string.IsNullOrWhiteSpace(first.Value) ? "None" : first.Value;
            }

            // If the loaded content type is custom (Other), populate the CustomContentType textbox
            if (OtherEnabled)
            {
                CustomContentType = ContentType;
            }

            ValidateHeaders();
            UpdateContentTypeHeader();
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

        private async Task ShowDetailsAsync(HttpRequestResult httpRequestResults)
        {

            var details = new HttpRequestResultWindow();

            var viewModel = _serviceProvider.GetRequiredService<HttpRequestResultViewModel>();

            await viewModel.SetDetails(httpRequestResults);

            details.DataContext = viewModel;

            details.Show();
        }

        private async Task CopyToClipboardAsync(string text, string successMessage)
        {
            try
            {
                var lifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                var mainWindow = lifetime?.MainWindow;
                var clipboard = mainWindow?.Clipboard;
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync(text);
                    CopyStatusMessage = successMessage;
                    // Clear message after a short delay
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(3000);
                        CopyStatusMessage = string.Empty;
                    });
                }
            }
            catch
            {
                // ignore clipboard errors
            }
        }

        private void ApplyAuthentication(List<KeyValuePair<string, string>> headers, ref string url)
        {
            var hasAuthorizationHeader = headers.Any(h => string.Equals(h.Key, "Authorization", StringComparison.OrdinalIgnoreCase));

            // Basic auth
            if (string.Equals(AuthScheme, "Basic", StringComparison.OrdinalIgnoreCase) && !hasAuthorizationHeader)
            {
                try
                {
                    var credBytes = System.Text.Encoding.UTF8.GetBytes($"{BasicUsername}:{BasicPassword}");
                    var b64 = Convert.ToBase64String(credBytes);
                    headers.Add(new KeyValuePair<string, string>("Authorization", $"Basic {b64}"));
                }
                catch
                {
                    // ignore encoding issues
                }
            }

            // Bearer token
            if (string.Equals(AuthScheme, "Bearer", StringComparison.OrdinalIgnoreCase) && !hasAuthorizationHeader && !string.IsNullOrEmpty(BearerToken))
            {
                headers.Add(new KeyValuePair<string, string>("Authorization", $"Bearer {BearerToken}"));
            }

            // ApiKey
            var apiKeyLocation = string.IsNullOrWhiteSpace(ApiKeyLocation) ? "Header" : ApiKeyLocation;
            if (string.Equals(AuthScheme, "ApiKey", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(ApiKey))
            {
                var name = string.IsNullOrWhiteSpace(ApiKeyName) ? "X-API-KEY" : ApiKeyName;
                if (string.Equals(apiKeyLocation, "Header", StringComparison.OrdinalIgnoreCase))
                {
                    // Find existing header (case-insensitive). If present, replace its value; otherwise add it.
                    var idx = headers.FindIndex(h => string.Equals(h.Key, name, StringComparison.OrdinalIgnoreCase));
                    if (idx >= 0)
                    {
                        headers[idx] = new KeyValuePair<string, string>(name, ApiKey);
                    }
                    else
                    {
                        headers.Add(new KeyValuePair<string, string>(name, ApiKey));
                    }
                }
                else if (string.Equals(apiKeyLocation, "Query", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var sep = url.Contains("?") ? "&" : "?";
                        url = url + sep + Uri.EscapeDataString(name) + "=" + Uri.EscapeDataString(ApiKey);
                    }
                    catch
                    {
                        // ignore URL encoding issues
                    }
                }
            }
        }
    }
}
