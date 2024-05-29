using System.Security.Claims;

namespace AnotherBlogEngine.Web.Shared;

// Add properties to this class and update the server and client AuthenticationStateProviders
// to expose more information about the authenticated user to the client.
public sealed class UserInfo
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;


    public const string UserIdClaimType = "sub";

    public const string EmailClaimType = "email";

    public static UserInfo FromClaimsPrincipal(ClaimsPrincipal principal) =>
        new()
        {
            UserId = GetRequiredClaim(principal, UserIdClaimType),
            Email = GetRequiredClaim(principal, EmailClaimType),
        };

    public ClaimsPrincipal ToClaimsPrincipal() =>
        new(new ClaimsIdentity(
            claims: new List<Claim>{ new(UserIdClaimType, UserId), new(EmailClaimType, Email) },
            authenticationType: nameof(UserInfo),
            nameType: EmailClaimType,
            roleType: null));

    private static string GetRequiredClaim(ClaimsPrincipal principal, string claimType) =>
        principal.FindFirst(claimType)?.Value ?? throw new InvalidOperationException($"Could not find required '{claimType}' claim.");
}
