using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;

namespace PlatformServiceTemplate.Api.IntegrationTests;

public sealed class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Production")).CreateClient();
    }

    [Fact]
    public async Task LiveHealth_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/health/live");
        Assert.True(response.IsSuccessStatusCode);
    }
}
