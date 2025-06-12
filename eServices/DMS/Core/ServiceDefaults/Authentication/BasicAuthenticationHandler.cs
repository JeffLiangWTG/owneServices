using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace eServices.Dms.Core.ServiceDefaults.Authentication;

public class BasicAuthenticationHandler<T>(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, SignInManager<T> signInManager)
	: AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder) where T : IdentityUser
{
	protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		try
		{
			if (!Request.Headers.ContainsKey("Authorization"))
				return AuthenticateResult.NoResult();

			if (!AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out var authHeader)
				|| string.IsNullOrWhiteSpace(authHeader.Parameter))
				return AuthenticateResult.Fail("Invalid Authorization header.");

			var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(":", 2);
			if (credentials.Length == 2)
			{
				var username = credentials[0];
				var password = credentials[1];

				var user = await signInManager.UserManager.FindByNameAsync(username);
				if (user is null)
					return AuthenticateResult.Fail("Invalid credentials.");

				var signIn = await signInManager.PasswordSignInAsync(user, password, true, false);
				if (!signIn.Succeeded)
					return AuthenticateResult.Fail("Invalid credentials.");

				var claims = new List<Claim>
				{
					new Claim(ClaimTypes.NameIdentifier, username),
					new Claim(ClaimTypes.Name, username),
				};

				var roles = await signInManager.UserManager.GetRolesAsync(user);
				foreach (var role in roles)
				{
					claims.Add(new Claim(ClaimTypes.Role, role));
				}

				var identity = new ClaimsIdentity(claims, Scheme.Name);
				var principal = new ClaimsPrincipal(identity);

				var ticket = new AuthenticationTicket(principal, Scheme.Name);

				return AuthenticateResult.Success(ticket);
			}
			else
			{
				return AuthenticateResult.Fail("Invalid credentials.");
			}
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Authentication error");
			return AuthenticateResult.Fail("Authentication error. Contact support.");
		}
	}
}
