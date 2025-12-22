namespace ApiTester.Models
{
    public class HttpRequestPersistentDataModel
    {
        public string HttpMethod { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public bool AppJsonEnabled { get; set; }
        public bool AppXmlEnabled { get; set; }
        public bool TextPlainEnabled { get; set; }
        public int MessageCount { get; set; }
        public bool SendInParallel { get; set; }
        public int NumberOfThreads { get; set; }
        public string RequestBody { get; set; } = string.Empty;
        public System.Collections.Generic.List<SerializableHeader> Headers { get; set; } = new();
    }
}