using Kasko.DataAccess;
using Kasko.DataAccess.Repositories.Abstract;
using Kasko.Entities.Concrete;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace Kasko.IntegrationTests;

public class PricingRuleVersioningTests
{
    [Theory]
    [InlineData(2026, 8, 15, 1, "0.0200")]
    [InlineData(2026, 8, 31, 1, "0.0200")]
    [InlineData(2026, 9, 1, 2, "0.0215")]
    [InlineData(2026, 9, 15, 2, "0.0215")]
    [InlineData(2026, 10, 31, 2, "0.0215")]
    [InlineData(2026, 11, 1, 3, "0.0230")]
    [InlineData(2026, 11, 15, 3, "0.0230")]
    public async Task GetApplicableRuleAsync_ShouldSelectCorrectVersionByEffectiveDate(
        int year,
        int month,
        int day,
        int expectedVersion,
        string expectedValue)
    {
        await using var factory =
            new WebApplicationFactory<Program>();

        await using var scope =
            factory.Services.CreateAsyncScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<KaskoContext>();

        var code = $"VERSIONING_TEST_{Guid.NewGuid():N}";

        context.PricingRules.AddRange(
            CreateVersion(code, 1, 0.0200m, new DateTime(2026, 1, 1), new DateTime(2026, 9, 1).AddTicks(-1)),
            CreateVersion(code, 2, 0.0215m, new DateTime(2026, 9, 1), new DateTime(2026, 11, 1).AddTicks(-1)),
            CreateVersion(code, 3, 0.0230m, new DateTime(2026, 11, 1), null));

        await context.SaveChangesAsync();

        var repository =
            scope.ServiceProvider
                .GetRequiredService<IPricingRuleRepository>();

        var rule =
            await repository.GetApplicableRuleAsync(
                code,
                new DateTime(year, month, day));

        Assert.NotNull(rule);
        Assert.Equal(expectedVersion, rule!.Version);
        Assert.Equal(decimal.Parse(expectedValue, System.Globalization.CultureInfo.InvariantCulture), rule.Value);
    }

    private static PricingRule CreateVersion(
        string code,
        int version,
        decimal value,
        DateTime effectiveFrom,
        DateTime? effectiveUntil)
    {
        return new PricingRule
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = "Versioning Test Rule",
            Value = value,
            Version = version,
            IsActive = true,
            EffectiveFrom = effectiveFrom,
            EffectiveUntil = effectiveUntil,
            CreatedDate = DateTime.UtcNow
        };
    }
}
