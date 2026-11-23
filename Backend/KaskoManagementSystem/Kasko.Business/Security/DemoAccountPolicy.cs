using Kasko.Business.Exceptions;

namespace Kasko.Business.Security;

public class DemoAccountPolicy
{
    public const string BlockedMessage =
        "Bu bir demo hesabıdır; şifre, e-posta ve hesap bilgileri değiştirilemez.";

    private readonly HashSet<string> _protectedEmails;

    public DemoAccountPolicy(IEnumerable<string>? protectedEmails)
    {
        _protectedEmails = new HashSet<string>(
            (protectedEmails ?? Enumerable.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(Normalize),
            StringComparer.Ordinal);
    }

    public static DemoAccountPolicy None { get; } = new(null);

    public bool IsProtected(string? email)
    {
        return !string.IsNullOrWhiteSpace(email) && _protectedEmails.Contains(Normalize(email));
    }

    public void EnsureNotProtected(string? email)
    {
        if (IsProtected(email))
        {
            throw new BadRequestException(BlockedMessage);
        }
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
