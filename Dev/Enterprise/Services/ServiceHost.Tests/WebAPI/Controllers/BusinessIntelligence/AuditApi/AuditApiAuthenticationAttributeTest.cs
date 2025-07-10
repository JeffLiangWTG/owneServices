using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using CargoWise.Async;
using Enterprise.Services.ServiceHost.Test;
using WTG.OAuth2.Token.TestFramework;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.Test
{
	class AuditApiAuthenticationAttributeTest : CW1IdentityBasicAuthenticationAttributeTest
	{
		protected override IAuthenticationFilter GetAuthenticationAttribute()
		{
			return new AuditApiAuthenticationAttribute();
		}

		public void TestBearerAuthenticationFailsWithInvalidToken()
		{
			using var identityServer = new MockOpenIDIdentityServer();
			var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
			var token = "Invalid-Token";

			var attribute = new AuditApiAuthenticationAttribute();

			var request = new HttpRequestMessage
			{
				Method = HttpMethod.Get,
				RequestUri = new Uri("http://testserver/api/replication/change-summary?afterLsn=0x000000000000000ff000&format=JSON"),
				Headers =
				{
					{ "Authorization", $"Bearer {token}" }
				}
			};

			var actionContext = new HttpActionContext
			{
				ControllerContext = new HttpControllerContext
				{
					Request = request
				}
			};

			var context = new HttpAuthenticationContext(actionContext, null);

			using var cancellationTokenSource = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));
			AsyncHelper.RunTask(() => attribute.AuthenticateAsync(context, cancellationTokenSource.Token), cancellationTokenSource.Token, "Hit Endpoint").GetAwaiter().GetResult().Wait(cancellationTokenSource.Token);

			AssertType<AuthenticationFailureResult>(context.ErrorResult);
			var failureResult = (AuthenticationFailureResult)context.ErrorResult;

			AssertEquals("Invalid Bearer token", failureResult.ReasonPhrase);
		}

		public void TestValidateExtraClaims_ReturnsTrue_WhenAzpClaimMatchesDdcApplicationId()
		{
			var attribute = new AuditApiAuthenticationAttribute();

			var validToken = new JwtSecurityToken(
				claims: new[]
				{
				new System.Security.Claims.Claim("azp", "d04c4c51-4ac4-4e27-b4b2-f1529abb443b")
				});

			var result = attribute.ValidateExtraClaims(validToken);

			AssertEquals(true, result);
		}

		public void TestValidateExtraClaims_ReturnsFalse_WhenAzpClaimDoesNotMatchDdcApplicationId()
		{
			var attribute = new AuditApiAuthenticationAttribute();

			var invalidToken = new JwtSecurityToken(
				claims: new[]
				{
				new System.Security.Claims.Claim("azp", "00000000-0000-0000-0000-000000000000")
				});

			var result = attribute.ValidateExtraClaims(invalidToken);

			AssertEquals(false, result);
		}

		public void TestValidateExtraClaims_ReturnsFalse_WhenAzpClaimIsMissing()
		{
			var attribute = new AuditApiAuthenticationAttribute();

			var tokenWithoutAzp = new JwtSecurityToken();

			var result = attribute.ValidateExtraClaims(tokenWithoutAzp);

			AssertEquals(false, result);
		}

		public void TestConstructor_SetsAuthorityUrlCorrectly()
		{
			var attribute = new AuditApiAuthenticationAttribute();
			AssertEquals(attribute.AuthorityUrl, "https://login.microsoftonline.com/1b20b87e-cebd-43cc-97bd-bdd41a2f5cf1/v2.0");
		}

		public void TestConstructor_SetsAudienceCorrectly()
		{
			var attribute = new AuditApiAuthenticationAttribute();
			AssertEquals(attribute.Audience, "9a6ebfef-9638-4fdd-95bb-5394926c0422");
		}
	}
}
