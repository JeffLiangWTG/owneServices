using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using CargoWise.Async;
using Enterprise.Registry.Business;
using Enterprise.Services.ServiceHost.Test;
using NUnit.Framework;
using WTG.OAuth2.Token.TestFramework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class S2STAndBasicAuthenticationAttributeTest
	{
		sealed class TestS2STAndBasicAuthAttribute : S2STAndBasicAuthenticationAttribute
		{
			public JwtSecurityToken CapturedToken { get; private set; }
			readonly bool ExtraValidationResult;

			public TestS2STAndBasicAuthAttribute(string authorityUrl, bool extraValidationResult)
				: base(authorityUrl, "/audit")
			{
				ExtraValidationResult = extraValidationResult;
			}

			internal override bool ValidateExtraClaims(JwtSecurityToken token)
			{
				CapturedToken = token;
				return ExtraValidationResult;
			}
		}

		class InheritedTest : CW1IdentityBasicAuthenticationAttributeTest
		{
			protected override IAuthenticationFilter GetAuthenticationAttribute()
			{
				return new TestS2STAndBasicAuthAttribute("https://authority-not-needed.com", true);
			}
		}

		class OidcTest : TestCase
		{
			const string MockClientId = "d04c4c51-4ac4-4e27-b4b2-f1529abb443b";
			static MockOpenIDIdentityServer MockIdentityServer;
			static string AuthorityUrl;

			protected override void SetUp()
			{
				MockIdentityServer = new MockOpenIDIdentityServer();
				AuthorityUrl = $"https://{MockIdentityServerBase.HostName}:{MockIdentityServer.Port}";
			}

			protected override void TearDown()
			{
				MockIdentityServer?.Dispose();
			}

			public void TestValidateBearerToken_ReturnsFalse_WhenTokenIsEmpty()
			{
				var attribute = new TestS2STAndBasicAuthAttribute(AuthorityUrl, extraValidationResult: false);
				var context = CreateAuthenticationContextWithBearerToken(null);

				using var cancellationTokenSource = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));
				CallAuthenticateAsync(attribute, context, cancellationTokenSource.Token);

				AssertType<AuthenticationFailureResult>(context.ErrorResult);
				var failureResult = (AuthenticationFailureResult)context.ErrorResult;

				AssertEquals("Invalid Bearer token", failureResult.ReasonPhrase);
			}

			public void TestValidateBearerToken_ReturnsFalse_WhenTokenIsInvalidButClaimsAreValid()
			{
				var attribute = new TestS2STAndBasicAuthAttribute(AuthorityUrl, extraValidationResult: false);
				var context = CreateAuthenticationContextWithBearerToken("invalid-token");

				using var cancellationTokenSource = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));
				CallAuthenticateAsync(attribute, context, cancellationTokenSource.Token);

				AssertType<AuthenticationFailureResult>(context.ErrorResult);
				var failureResult = (AuthenticationFailureResult)context.ErrorResult;

				AssertEquals("Invalid Bearer token", failureResult.ReasonPhrase);
			}

			public void TestValidateBearerToken_ReturnsTrue_WhenTokenIsValidAndClaimsPass()
			{
				var token = MockJwtTokenProvider.GenerateClientAccessToken(
					AuthorityUrl,
					MockClientId,
					SystemDataRegistry.Instance.EDIClientID.Value + "/audit");

				var attribute = new TestS2STAndBasicAuthAttribute(AuthorityUrl, extraValidationResult: true);
				var context = CreateAuthenticationContextWithBearerToken(token);

				using var cancellationTokenSource = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));
				CallAuthenticateAsync(attribute, context, cancellationTokenSource.Token);

				AssertEquals(true, context.Principal.Identity.IsAuthenticated);
				AssertEquals("Bearer", context.Principal.Identity.AuthenticationType);

				var claimsPrincipal = context.Principal as ClaimsPrincipal;
				AssertNotNull(claimsPrincipal);

				var azpClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "azp")?.Value;
				AssertEquals(MockClientId, azpClaim);
			}

			public void TestValidateBearerToken_ReturnsFalse_WhenTokenIsValidButClaimsFail()
			{
				var token = MockJwtTokenProvider.GenerateClientAccessToken(
					AuthorityUrl,
					"wrong-claim",
					SystemDataRegistry.Instance.EDIClientID.Value + "/audit");

				var attribute = new TestS2STAndBasicAuthAttribute(AuthorityUrl, extraValidationResult: false);
				var context = CreateAuthenticationContextWithBearerToken(token);

				using var cancellationTokenSource = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));
				CallAuthenticateAsync(attribute, context, cancellationTokenSource.Token);

				AssertType<AuthenticationFailureResult>(context.ErrorResult);
				var failureResult = (AuthenticationFailureResult)context.ErrorResult;

				AssertNotNull(attribute.CapturedToken);

				AssertEquals(token, attribute.CapturedToken.RawData);
			}

			public void TestValidateBearerToken_ReturnsFalse_WhenNoAuthorizationHeader()
			{
				var request = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri("http://testserver/api/test")
				};
				var actionContext = new HttpActionContext
				{
					ControllerContext = new HttpControllerContext
					{ Request = request }
				};

				var context = new HttpAuthenticationContext(actionContext, null);
				var attribute = new TestS2STAndBasicAuthAttribute(AuthorityUrl, extraValidationResult: false);

				using var cancellationTokenSource = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));
				CallAuthenticateAsync(attribute, context, cancellationTokenSource.Token);

				AssertType<AuthenticationFailureResult>(context.ErrorResult);
				var failureResult = (AuthenticationFailureResult)context.ErrorResult;

				AssertEquals("Unauthorized", failureResult.ReasonPhrase);
			}

			public void TestValidateBearerToken_ReturnsFalse_WhenUnrecognizedAuthorizationHeader()
			{
				var request = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri("http://testserver/api/test"),
					Headers = { { "Authorization", $"not-recognised header" } }
				};
				var actionContext = new HttpActionContext
				{
					ControllerContext = new HttpControllerContext
					{ Request = request }
				};

				var context = new HttpAuthenticationContext(actionContext, null);
				var attribute = new TestS2STAndBasicAuthAttribute(AuthorityUrl, extraValidationResult: false);

				using var cancellationTokenSource = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));

				CallAuthenticateAsync(attribute, context, cancellationTokenSource.Token);

				AssertType<AuthenticationFailureResult>(context.ErrorResult);
				var failureResult = (AuthenticationFailureResult)context.ErrorResult;

				AssertEquals("Unauthorized", failureResult.ReasonPhrase);
			}

			public void TestValidateBasicToken_ReturnsFalse()
			{
				const string testUsername = "oliver";
				const string testPassword = "thePassword";
				var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.GetEncoding("iso-8859-1").GetBytes($"{testUsername}:{testPassword}")));

				var token = authHeader.ToString();
				var context = CreateAuthenticationContextWithBasicToken(token);

				var attribute = new TestS2STAndBasicAuthAttribute(AuthorityUrl, extraValidationResult: true);

				using var cancellationTokenSource = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));
				CallAuthenticateAsync(attribute, context, cancellationTokenSource.Token);

				AssertType<AuthenticationFailureResult>(context.ErrorResult);
				var failureResult = (AuthenticationFailureResult)context.ErrorResult;

				AssertEquals("You must enter a correct user name and/or password. Please try again.", failureResult.ReasonPhrase);
			}

			public void TestChallengeAsync_SetsSchemeCorrectly()
			{
				var attribute = new TestS2STAndBasicAuthAttribute(AuthorityUrl, extraValidationResult: true);
				var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
				var token = "TestTokenSinceItsJustAssertingChallengeAsync";
				var actionContext = new HttpActionContext
				{
					ControllerContext = new HttpControllerContext
					{
						Request = new HttpRequestMessage
						{
							Method = HttpMethod.Get,
							RequestUri = new Uri("http://testserver/api/test"),
							Headers = { { "Authorization", $"Basic {token}" } }
						}
					},
					Response = response
				};
				var context = new HttpAuthenticationChallengeContext(
					actionContext,
					new ResponseMessageResult(response)
				);

				var cancellationTokenSource = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));

				attribute.ChallengeAsync(context, cancellationTokenSource.Token).GetAwaiter().GetResult();

				AssertNotNull(context.Result);

				var result = (AddChallengeOnUnauthorizedResult)context.Result;
				AssertNotNull(result.Challenge);
				AssertEquals("Basic", result.Challenge.Scheme);
			}

			HttpAuthenticationContext CreateAuthenticationContextWithBearerToken(string token)
			{
				var request = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri("http://url-doesnt-matter/api/test"),
					Headers = { { "Authorization", $"Bearer {token}" } }
				};

				var actionContext = new HttpActionContext
				{
					ControllerContext = new HttpControllerContext
					{ Request = request }
				};

				return new HttpAuthenticationContext(actionContext, null);
			}

			HttpAuthenticationContext CreateAuthenticationContextWithBasicToken(string token)
			{
				var request = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri("http://url-doesnt-matter/api/test"),
				};

				request.Headers.Authorization = AuthenticationHeaderValue.Parse(token);

				var actionContext = new HttpActionContext
				{
					ControllerContext = new HttpControllerContext
					{ Request = request }
				};

				return new HttpAuthenticationContext(actionContext, null);
			}

			void CallAuthenticateAsync(S2STAndBasicAuthenticationAttribute attribute, HttpAuthenticationContext context, CancellationToken token)
			{
				AsyncHelper.RunTask(() => attribute.AuthenticateAsync(context, token), token, "Hit Endpoint").GetAwaiter().GetResult().Wait(token);
			}
		}
	}
}
