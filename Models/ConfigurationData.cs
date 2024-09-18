namespace ApiTester.Models
{
    public class ConfigurationData
    {
        public string HttpMethod { get; set; }
        public string Url { get; set; }
        public bool AppJsonEnabled { get; set; }
        public bool AppXmlEnabled { get; set; }
        public bool TextPlainEnabled { get; set; }
        public int MessageCount { get; set; }
        public bool SendInParallel { get; set; }
        public int NumberOfThreads { get; set; }
        public string RequestBody { get; set; }
    }
}