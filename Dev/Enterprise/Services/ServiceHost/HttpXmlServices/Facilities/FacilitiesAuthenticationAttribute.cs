using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;
using WTG.OpenIDConnect.Token;

namespace Enterprise.Services.ServiceHost
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "Inherited for testing")]
	[AttributeUsage(AttributeTargets.All)]
	public class FacilitiesAuthenticationAttribute : Attribute, IAuthenticationFilter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Scheme name")]
		const string AuthenticatonScheme = "Bearer";

		public bool AllowMultiple => false;

		// the heavy lifting of authentication is handled by the OAuth2AUthenticationAttribute
		// the authentication provided in this method provides further validation specific to the Facilities team product
		public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
		{
			if (!IsFacilitiesGateWebServiceEnabled())
			{
				context.ErrorResult = new AuthenticationFailureResult((NoResString)"Facilities Gate Web Service is disabled. You can enable it in Registry -> Freight -> Gate Booking -> Enable Facilities Gate Web Service", context.Request);
				return Task.CompletedTask;
			}

			if (!IsAzpClaimValid(context, out var invalidAuthorisationMessage))
			{
				context.ErrorResult = new AuthenticationFailureResult(invalidAuthorisationMessage, context.Request);
				return Task.CompletedTask;
			}

			return Task.CompletedTask;
		}

		bool IsAzpClaimValid(HttpAuthenticationContext context, out string errorMessage)
		{
			errorMessage = string.Empty;
			if (!TryGetAzpClaimFromAccessToken(context, out var azpClaim))
			{
				errorMessage = (NoResString)"Invalid authorization data";
			}
			else if (!WarehouseDataRegistry.Instance.GateManagementInboundOAuthClientIDs.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Contains(azpClaim))
			{
				errorMessage = (NoResString)"Invalid access token";
			}

			return string.IsNullOrEmpty(errorMessage);
		}

		bool TryGetAzpClaimFromAccessToken(HttpAuthenticationContext context, out string azpClaim)
		{
			azpClaim = null;
			var authHeader = context.Request.Headers?.Authorization;
			if (AuthenticatonScheme.Equals(authHeader?.Scheme, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(authHeader?.Parameter))
			{
				var accessToken = authHeader.Parameter;
				var jwtToken = GetJwtToken(accessToken);
				azpClaim = jwtToken?.Claims?.FirstOrDefault(x => x.Type == (NoResString)"azp")?.Value;
			}
			return !string.IsNullOrEmpty(azpClaim);
		}

		protected virtual JwtSecurityToken GetJwtToken(string accessToken)
		{
			return TokenValidator.ReadJwtToken(accessToken, new TokenValidationLogger(new SimpleLogger()));
		}

		static bool IsFacilitiesGateWebServiceEnabled()
		{
			return WarehouseDataRegistry.Instance.EnableFacilitiesGateWebService.Value;
		}

		public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
