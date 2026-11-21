using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Hive.Tests.Integration;

namespace Hive.Tests.Features.Authentication;

public sealed class AuthenticationApiIntegrationTests
{
    [Fact]
    public async Task Register_ValidRequest_ReturnsCreatedWithoutPasswordOrHash()
    {
        using var factory = new HiveApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                fullName = "Integration Test User",
                email = "integration@example.com",
                password = "StrongPassword123"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.True(json.RootElement.TryGetProperty("userId", out _));
        Assert.Equal(
            "integration@example.com",
            json.RootElement.GetProperty("email").GetString());

        Assert.DoesNotContain("password", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        using var factory = new HiveApiFactory();
        using var client = factory.CreateClient();

        var request = new
        {
            fullName = "Integration Test User",
            email = "duplicate@example.com",
            password = "StrongPassword123"
        };

        var first = await client.PostAsJsonAsync("/api/auth/register", request);
        var second = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                request.fullName,
                email = "DUPLICATE@example.com",
                request.password
            });

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Theory]
    [InlineData("", "valid@example.com", "StrongPassword123")]
    [InlineData("A", "valid@example.com", "StrongPassword123")]
    [InlineData("Valid Name", "not-an-email", "StrongPassword123")]
    [InlineData("Valid Name", "valid@example.com", "short")]
    public async Task Register_InvalidRequest_ReturnsBadRequest(
        string fullName,
        string email,
        string password)
    {
        using var factory = new HiveApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { fullName, email, password });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_TwoConcurrentRequestsForSameEmail_OnlyOneSucceeds()
    {
        using var factory = new HiveApiFactory();
        using var client = factory.CreateClient();

        var requests = Enumerable.Range(0, 2)
            .Select(_ => client.PostAsJsonAsync(
                "/api/auth/register",
                new
                {
                    fullName = "Concurrent User",
                    email = "race@example.com",
                    password = "StrongPassword123"
                }));

        var responses = await Task.WhenAll(requests);

        Assert.Single(responses, response =>
            response.StatusCode == HttpStatusCode.Created);

        Assert.Single(responses, response =>
            response.StatusCode == HttpStatusCode.Conflict);
    }
}
