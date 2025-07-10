using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.OpenIDConnect.Token;

namespace Enterprise.Services.ServiceHost
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public abstract class S2STAndBasicAuthenticationAttribute : Attribute, IAuthenticationFilter
	{
		readonly CW1IdentityBasicAuthenticationAttribute basicAuthAttribute = new();

		public S2STAndBasicAuthenticationAttribute(string authorityUrl, string audienceExtension)
		{
			AuthorityUrl = authorityUrl ?? throw new ArgumentNullException(nameof(authorityUrl));
			this.audienceExtension = audienceExtension;
		}

		readonly string audienceExtension;
		public string AuthorityUrl { get; }
		public string Audience => $"{SystemDataRegistry.Instance.EDIClientID.Value}{audienceExtension}";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant for the scheme type")]
		const string Bearer = "Bearer";
		public bool AllowMultiple => false;

		public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
		{
			var authHeader = context.Request.Headers.Authorization;

			if (Bearer.Equals(authHeader?.Scheme, StringComparison.OrdinalIgnoreCase))
			{
				var jwtToken = await ValidateBearerTokenAsync(authHeader.Parameter, cancellationToken);
				if (jwtToken != null)
				{
					context.Principal = CreatePrincipal(jwtToken);
				}
				else
				{
					SetUnauthorizedResponse(context, Res.GetString("b3771c3e-4260-45a9-b574-76752a58144f", "Invalid Bearer token"));
				}
			}
			else
			{
				await basicAuthAttribute.AuthenticateAsync(context, cancellationToken);
			}
		}

		public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
		{
			return basicAuthAttribute.ChallengeAsync(context, cancellationToken);
		}

		async Task<JwtSecurityToken> ValidateBearerTokenAsync(string token, CancellationToken cancellationToken)
		{
			try
			{
				var jwtToken = await VerifyAccessTokenAsync(token, cancellationToken);

				if (!ValidateExtraClaims(jwtToken) && jwtToken != null)
				{
					jwtToken = null;
				}

				return jwtToken;
			}
			catch
			{
				return null;
			}
		}

		async Task<JwtSecurityToken> VerifyAccessTokenAsync(string accessToken, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(accessToken))
			{
				return null;
			}

			return await TokenValidator.ValidateAccessToken(
				AuthorityUrl,
				Audience,
				accessToken,
				ConfigurationHelper.ConfigurationManagerCache,
				new TokenValidationLogger(new SimpleLogger()),
				cancellationToken
			);
		}

		internal abstract bool ValidateExtraClaims(JwtSecurityToken token);

		void SetUnauthorizedResponse(HttpAuthenticationContext context, string reason)
		{
			context.ErrorResult = new AuthenticationFailureResult(reason, context.Request);
		}

		ClaimsPrincipal CreatePrincipal(JwtSecurityToken jwtToken)
		{
			var identity = new ClaimsIdentity(jwtToken.Claims, Bearer);

			return new ClaimsPrincipal(identity);
		}
	}
}
