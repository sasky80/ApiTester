using System;
using System.Xml;
using Newtonsoft.Json;
using Xunit;

namespace ApiTester.Tests
{
    public class FormatterServiceTests
    {
        private readonly FormatterService _formatter = new();

        [Fact]
        public void FormatJson_WithValidJson_ReturnsIndented()
        {
            var result = _formatter.FormatJson("{\"foo\":1}");
            Assert.Contains(Environment.NewLine, result);
        }

        [Fact]
        public void FormatJson_WithInvalidJson_Throws()
        {
            Assert.Throws<JsonReaderException>(() => _formatter.FormatJson("not-json"));
        }

        [Fact]
        public void FormatJson_WithWhitespace_ReturnsEmpty()
        {
            var result = _formatter.FormatJson("   ");
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void FormatXml_WithValidXml_ReturnsIndented()
        {
            var xml = "<root><child id=\"1\">value</child></root>";
            var result = _formatter.FormatXml(xml);
            Assert.Contains(Environment.NewLine, result);
        }

        [Fact]
        public void FormatXml_WithInvalidXml_Throws()
        {
            Assert.Throws<XmlException>(() => _formatter.FormatXml("not-xml"));
        }

        [Fact]
        public void FormatXml_WithWhitespace_ReturnsEmpty()
        {
            var result = _formatter.FormatXml("   ");
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void TryFormatJson_Invalid_ReturnsFalse()
        {
            var success = _formatter.TryFormatJson("not-json", out var formatted);
            Assert.False(success);
            Assert.Equal("not-json", formatted);
        }

        [Fact]
        public void TryFormatXml_Invalid_ReturnsFalse()
        {
            var success = _formatter.TryFormatXml("not-xml", out var formatted);
            Assert.False(success);
            Assert.Equal("not-xml", formatted);
        }

        [Fact]
        public void TryFormatXml_Valid_ReturnsTrue()
        {
            var success = _formatter.TryFormatXml("<root />", out var formatted);
            Assert.True(success);
            Assert.False(string.IsNullOrWhiteSpace(formatted));
        }
    }
}
