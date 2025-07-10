using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web.Auth;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WTG.OpenIDConnect.Token;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	class InternalDataSetsTestFixture
	{
		[TestCase("UserAuthorization")]
		[TestCase("RefDataSetInformation")]
		[TestCase("DataPushSubscription")]
		[TestCase("RefClient")]
		[TestCase("RefDataSetInformationDefinition")]
		public async Task TestGetInternalDataSetsUnauthorized(string dataSet)
		{
			using (var factory = IntegrationTestHelper.WebAppFactory)
			using (var client = factory.CreateClient())
			using (var response = await client.GetAsync(new Uri($"http://localhost/odata/{dataSet}Update")))
			{
				Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			}
		}

		[TestCase("UserAuthorization")]
		[TestCase("RefDataSetInformation")]
		[TestCase("DataPushSubscription")]
		[TestCase("RefClient")]
		[TestCase("RefDataSetInformationDefinition")]
		public async Task TestGetInternalDataSetsAuthorized(string dataSet)
		{
			var accessToken = AccessTokenGenerator.GenerateS2SAccessToken("42CFADE3-F079-45C0-B778-9D3B2BA9CA21", DateTime.Now);
			var tokenValidationHelper = new Mock<ITokenValidationHelper>();
			tokenValidationHelper.Setup(x => x.ShouldHandle(accessToken)).Returns(true);
			var jwtSecurityToken = new JwtSecurityToken("S2S Issuer", "S2S Audience", new[] { new Claim(AuthClaimType.Azp, "42CFADE3-F079-45C0-B778-9D3B2BA9CA21") });
			tokenValidationHelper.Setup(x => x.VerifyAccessTokenAsync(accessToken,
				It.IsAny<ConcurrentDictionary<string, IConfigurationManagerWithLock>>(), It.IsAny<ILogger>(),
				It.IsAny<CancellationToken>())).ReturnsAsync(jwtSecurityToken);
			var mockAuthorizationHelper = new Mock<IAuthorizationHelper>();
			mockAuthorizationHelper.Setup(x => x.IsAuthorized(It.IsAny<Type>(), "42CFADE3-F079-45C0-B778-9D3B2BA9CA21")).Returns(true);
			using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
					{
						builder.ConfigureTestServices(services =>
						{
							services.RemoveAll<ITokenValidationHelper>();
							services.RemoveAll<IAuthorizationHelper>();
							services.AddScoped(sp => tokenValidationHelper.Object)
								.AddScoped(sp => mockAuthorizationHelper.Object);
						});
					}))
			using (var client = factory.CreateClient())
			{
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
				using (var response = await client.GetAsync(new Uri($"http://localhost/odata/{dataSet}Update")))
				{
					response.EnsureSuccessStatusCode();
				}
			}
		}
	}
}
