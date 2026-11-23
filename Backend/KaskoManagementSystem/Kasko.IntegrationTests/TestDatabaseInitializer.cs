using Kasko.DataAccess;
using Kasko.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace Kasko.IntegrationTests;

internal static class TestDatabaseInitializer
{
    public static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("KASKO_TEST_CONNECTION")
        ?? "Server=.;Database=KaskoManagementDb_Test;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=60;";

    [ModuleInitializer]
    internal static void Initialize()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", ConnectionString);

        var options = new DbContextOptionsBuilder<KaskoContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        using var context = new KaskoContext(options);
        context.Database.Migrate();

        DisableQuoteAutoApproval(context);
    }

    private static void DisableQuoteAutoApproval(KaskoContext context)
    {
        const string code = "AUTO_APPROVE_MAX_PREMIUM";

        if (context.PricingRules.Any(x => x.Code == code && !x.IsDeleted))
        {
            return;
        }

        context.PricingRules.Add(new PricingRule
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = "Otomatik onay üst prim limiti (entegrasyon testi)",
            Value = 0m,
            Version = 1,
            IsActive = true,
            EffectiveFrom = new DateTime(2000, 1, 1),
            CreatedDate = DateTime.UtcNow
        });

        context.SaveChanges();
    }
}
