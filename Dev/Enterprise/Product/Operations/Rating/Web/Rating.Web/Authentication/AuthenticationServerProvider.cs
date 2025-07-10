using System.Security.Claims;
using System.Threading.Tasks;
using AuthenticationService.Client.Models;
using CargoWise.Data;
using Enterprise.Security;
using Microsoft.Owin.Security.OAuth;

namespace Enterprise.Rating.Web.Authentication
{
	class AuthenticationServerProvider : OAuthAuthorizationServerProvider
	{
		const string GrantTypeParameterName = "grant_type";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Password type")]
		const string PasswordGrantType = "password";
		public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
		{
			var grantType = context.Parameters.Get(GrantTypeParameterName);
			if (PasswordGrantType.Equals(grantType, System.StringComparison.InvariantCultureIgnoreCase))
			{
				context.Validated();
			}
			else
			{
				context.Rejected();
				context.SetError("unsupported_grant_type");
			}
			return Task.CompletedTask;
		}

		public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
		{
			// Request specification is described here : https://datatracker.ietf.org/doc/html/rfc6749#section-4.1.3
			using (Db.DisposableActionForDbConnection())
			{
				var loginController = new UserLoginController();

				var loginResult = loginController.ValidateUserLoginAndPassword(context.UserName, context.Password);
				if (loginResult.IsOK)
				{
					var identity = new ClaimsIdentity(context.Options.AuthenticationType);
					identity.AddClaim(new Claim(WTGClaimTypes.UserCode, context.UserName));
					context.Validated(identity);
				}
				else
				{
					context.SetError("invalid_grant", Res.GetString("7FB26FF8-94DB-4794-9C46-3508B100B684", "Login failed with status: {0}", loginResult.State));
				}
			}

			return Task.CompletedTask;
		}
	}
}
