using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;

namespace PlatformServiceTemplate.Api.FunctionalTests;

public sealed class OpenApiEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OpenApiEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Production")).CreateClient();
    }

    [Fact]
    public async Task OpenApiEndpoint_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        Assert.True(response.IsSuccessStatusCode);
    }
}
