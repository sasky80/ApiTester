namespace ApiTester.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ApiTester.Builders;
using ApiTester.Models;
using Xunit;

public class MessageBuilderTests
{
    [Fact]
    public async Task BuildRequest_PostWithBody_SetsContentAndContentType()
    {
        var body = "{\"hello\":\"world\"}";

        var request = MessageBuilder.BuildRequest(
            url: "https://example.com/api",
            method: "POST",
            contentType: "application/json",
            body: body);

        Assert.NotNull(request.Content);
        Assert.Equal("application/json", request.Content!.Headers.ContentType!.MediaType);
        Assert.Equal(body, await request.Content.ReadAsStringAsync());
    }

    [Fact]
    public void BuildRequest_AddsCustomHeaders()
    {
        var headers = new List<HeaderEntry>
        {
            new() { Name = "X-Test", Value = "abc" },
            new() { Name = "X-Number", Value = "123" }
        };

        var request = MessageBuilder.BuildRequest(
            url: "https://example.com/resource",
            method: "GET",
            contentType: "None",
            body: string.Empty,
            headers: headers);

        Assert.True(request.Headers.TryGetValues("X-Test", out var testHeader));
        Assert.Equal("abc", testHeader.Single());
        Assert.True(request.Headers.TryGetValues("X-Number", out var numberHeader));
        Assert.Equal("123", numberHeader.Single());
    }

    [Fact]
    public void BuildRequest_ApiKeyInQuery_AppendsQueryParameter()
    {
        var request = MessageBuilder.BuildRequest(
            url: "https://example.com/data",
            method: "GET",
            contentType: "None",
            body: string.Empty,
            authScheme: "ApiKey",
            apiKey: "token-value",
            apiKeyLocation: "Query",
            apiKeyName: "custom_key");

        var query = System.Web.HttpUtility.ParseQueryString(request.RequestUri!.Query);
        Assert.Equal("token-value", query["custom_key"]);
    }

    [Fact]
    public void BuildRequest_BasicAuth_AddsAuthorizationHeader()
    {
        var request = MessageBuilder.BuildRequest(
            url: "https://example.com/secure",
            method: "GET",
            contentType: "None",
            body: string.Empty,
            authScheme: "Basic",
            basicUser: "user",
            basicPass: "pass");

        Assert.True(request.Headers.TryGetValues("Authorization", out var values));
        var expected = Convert.ToBase64String(Encoding.UTF8.GetBytes("user:pass"));
        Assert.Equal($"Basic {expected}", values.Single());
    }
}

