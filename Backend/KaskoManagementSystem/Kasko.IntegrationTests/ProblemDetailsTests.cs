using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Kasko.IntegrationTests;

public class ProblemDetailsTests
{
    [Fact]
    public async Task NonExistingCustomer_ShouldReturnProblemDetails()
    {
        await using var factory =
            new WebApplicationFactory<Program>();

        var user =
            await IntegrationTestHelper.SeedUserAsync(
                factory,
                "Admin");

        using var client =
            await IntegrationTestHelper.LoginAsync(
                factory,
                user);

        var customerId = Guid.NewGuid();

        var response =
            await client.GetAsync(
                $"/api/Customer/{customerId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        var contentType =
            response.Content.Headers.ContentType?.MediaType;

        Assert.Equal(
            "application/problem+json",
            contentType);

        var body =
            await response.Content.ReadAsStringAsync();

        Assert.False(
            string.IsNullOrWhiteSpace(body));

        Console.WriteLine("PROBLEM DETAILS RESPONSE:");
        Console.WriteLine(body);
    }
    [Fact]
    public async Task DuplicateCustomerEmail_ShouldReturnBadRequestProblemDetails()
    {
        await using var factory =
            new WebApplicationFactory<Program>();

        var user =
            await IntegrationTestHelper.SeedUserAsync(
                factory,
                "Admin");

        using var client =
            await IntegrationTestHelper.LoginAsync(
                factory,
                user);

        var firstCustomer =
            await IntegrationTestHelper.BuildCustomerUserAsync(
                client,
                $"duplicate.{Guid.NewGuid():N}@test.local");

        var firstResponse =
            await client.PostAsJsonAsync(
                "/api/User",
                firstCustomer);

        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode);

        var secondCustomer =
            await IntegrationTestHelper.BuildCustomerUserAsync(
                client,
                firstCustomer.Email);

        var secondResponse =
            await client.PostAsJsonAsync(
                "/api/User",
                secondCustomer);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            secondResponse.StatusCode);

        Assert.Equal(
            "application/problem+json",
            secondResponse.Content.Headers.ContentType?.MediaType);

        var json =
            await secondResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            400,
            json.GetProperty("status").GetInt32());

        Assert.Equal(
            "Bad Request",
            json.GetProperty("title").GetString());

        Assert.True(
            json.TryGetProperty(
                "traceId",
                out _));
    }
}