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
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WTG.OpenIDConnect.Token;

namespace CargoWise.RefDbRepo.Staging.NewService.Test
{
	[TestFixture]
	class InternalDataSetsTestFixture
	{
		[TestCase("RefApplicationAttribute")]
		[TestCase("RefApplicationAttributeDefault")]
		[TestCase("RefClient")]
		public async Task TestGetInternalDataSetsUnauthorized(string dataSet)
		{
			var stagingRepo = new Mock<IStagingRepository>().Object;
			var refDbRepoCrypto = new Mock<IRefDbRepoCrypto>();
			using (var factory = IntegrationTestHelper.GetWebAppFactory(stagingRepo, refDbRepoCrypto.Object))
			using (var client = factory.CreateClient())
			using (var response = await client.GetAsync(new Uri($"http://localhost/odata/{dataSet}Update")))
			{
				Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			}
		}

		[TestCase("RefApplicationAttribute")]
		[TestCase("RefClient")]
		public async Task TestGetInternalDataSetsAuthorized(string dataSet)
		{
			var accessToken = AccessTokenGenerator.GenerateOIDCAccessToken("test_name", DateTime.Now.AddDays(1));
			var tokenValidationHelper = new Mock<ITokenValidationHelper>();
			tokenValidationHelper.Setup(x => x.ShouldHandle(accessToken)).Returns(true);
			var jwtSecurityToken = new JwtSecurityToken("OIDC Issuer", "OIDC Audience", new[] { new Claim(AuthClaimType.UniqueName, "test_name") });
			tokenValidationHelper.Setup(x => x.VerifyAccessTokenAsync(accessToken,
				It.IsAny<ConcurrentDictionary<string, IConfigurationManagerWithLock>>(), It.IsAny<ILogger>(),
				It.IsAny<CancellationToken>())).ReturnsAsync(jwtSecurityToken);
			var stagingRepository = new Mock<IStagingRepository>();
			var refDbRepoCrypto = new Mock<IRefDbRepoCrypto>();
			var mockAuthorizationHelper = new Mock<IAuthorizationHelper>();
			mockAuthorizationHelper.Setup(x => x.IsAuthorized(It.IsAny<Type>(), "test_name")).Returns(true);
			using (var factory = IntegrationTestHelper.GetWebAppFactory(stagingRepository.Object, refDbRepoCrypto.Object).WithWebHostBuilder(builder =>
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
