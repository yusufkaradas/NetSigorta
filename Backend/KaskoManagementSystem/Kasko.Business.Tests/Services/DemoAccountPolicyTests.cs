using Kasko.Business.Exceptions;
using Kasko.Business.Security;

namespace Kasko.Business.Tests.Services;

public class DemoAccountPolicyTests
{
    [Theory]
    [InlineData("customer@test.com")]
    [InlineData("  CUSTOMER@test.com ")]
    public void IsProtected_ShouldMatchCaseAndWhitespaceInsensitive(string email)
    {
        var policy = new DemoAccountPolicy(new[] { "Customer@Test.com" });

        Assert.True(policy.IsProtected(email));
        Assert.Throws<BadRequestException>(() => policy.EnsureNotProtected(email));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("elif@ornek.com")]
    public void IsProtected_ShouldBeFalseForOtherOrEmptyEmails(string? email)
    {
        var policy = new DemoAccountPolicy(new[] { "customer@test.com", " " });

        Assert.False(policy.IsProtected(email));
        policy.EnsureNotProtected(email);
    }

    [Fact]
    public void None_ShouldProtectNothing()
    {
        Assert.False(DemoAccountPolicy.None.IsProtected("customer@test.com"));
    }
}
