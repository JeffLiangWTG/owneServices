using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
	class AuthenticationFixture
	{
		[Test]
		public void TokenAuthentication()
		{
			var stagingRepo = new Mock<IStagingRepository>();
			stagingRepo.Setup(x => x.Get<RefAccTaxRate>()).Returns(new[] { taxRate }.AsQueryable());
			var accessToken = AccessTokenGenerator.GenerateS2SAccessToken("370B7728-1DC7-4931-9F77-080CB88E58F6", DateTime.Now);
			var tokenValidationHelper = new Mock<ITokenValidationHelper>();
			tokenValidationHelper.Setup(x => x.ShouldHandle(accessToken)).Returns(true);
			tokenValidationHelper.Setup(x => x.VerifyAccessTokenAsync(accessToken, It.IsAny<ConcurrentDictionary<string, IConfigurationManagerWithLock>>(), It.IsAny<ILogger>(), CancellationToken.None)).Returns(Task.FromResult(new JwtSecurityToken(accessToken)));
			var authorizationHelper = new Mock<IAuthorizationHelper>();
			authorizationHelper.Setup(x => x.IsAuthorized(It.IsAny<RefAccTaxRate>(), It.IsAny<string>())).Returns(true);
			var refDbRepoCrypto = new Mock<IRefDbRepoCrypto>();

			using (var factory = IntegrationTestHelper.GetWebAppFactory(stagingRepo.Object, refDbRepoCrypto.Object).WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll<ITokenValidationHelper>();
					services.AddScoped(sp => authorizationHelper.Object)
						.AddScoped(sp => tokenValidationHelper.Object);

					services.Configure<TestServerOptions>(options =>
					{
						options.AllowSynchronousIO = true;
					});
				});
			}))

			using (var client = factory.CreateClient())
			{
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
				foreach (var request in GetHttpRequestMessages())
				{
					HttpResponseMessage response = null;
					Assert.DoesNotThrowAsync(async () => response = await client.SendAsync(request));
					response.EnsureSuccessStatusCode();
				}
			}
		}

		[Test]
		public async Task InvalidAuthenticationReturn401()
		{
			var stagingRepo = new Mock<IStagingRepository>();
			var tokenValidationHelper = new Mock<ITokenValidationHelper>();
			var refDbRepoCrypto = new Mock<IRefDbRepoCrypto>();
			using (var factory = IntegrationTestHelper.GetWebAppFactory(stagingRepo.Object, refDbRepoCrypto.Object).WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.AddScoped(sp => tokenValidationHelper.Object);

					services.Configure<TestServerOptions>(options =>
					{
						options.AllowSynchronousIO = true;
					});
				});
			}))

			using (var client = factory.CreateClient())
			{
				foreach (var request in GetHttpRequestMessages())
				{
					HttpResponseMessage response = null;
					Assert.DoesNotThrowAsync(async () => response = await client.SendAsync(request));
					Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
					var resultContent = await response.Content.ReadAsStringAsync();
					Assert.AreEqual("Unauthorized: Invalid access token", resultContent);
				}
			}
		}

		HttpRequestMessage[] GetHttpRequestMessages()
		{
			var postRequest = CreateHttpRequestMessage(HttpMethod.Post, "http://localhost/odata/RefAccTaxRateUpdate");
			var putRequest = CreateHttpRequestMessage(HttpMethod.Put, "http://localhost/odata/RefAccTaxRateUpdate(9B79A9DB-3BCB-4757-9338-B0DD5EF2CAF2)");
			var patchRequest = CreateHttpRequestMessage(HttpMethod.Patch, "http://localhost/odata/RefAccTaxRateUpdate(9B79A9DB-3BCB-4757-9338-B0DD5EF2CAF2)");
			var deleteRequest = CreateHttpRequestMessage(HttpMethod.Delete, "http://localhost/odata/RefAccTaxRateUpdate(9B79A9DB-3BCB-4757-9338-B0DD5EF2CAF2)");

			return new[] { postRequest, putRequest, patchRequest, deleteRequest };
		}

		HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string url)
		{
			var request = new HttpRequestMessage(method, new Uri(url));
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json", 0.9D));
			request.Content = new StringContent(JsonSerializer.Serialize(taxRate).Replace("T00:00:00Z", ""), Encoding.UTF8, "application/json");
			return request;
		}

		readonly RefAccTaxRate taxRate = new RefAccTaxRate
		{
			ZAT_PK = Guid.Parse("9B79A9DB-3BCB-4757-9338-B0DD5EF2CAF2"),
			ZAT_RN_NKCountry = "ZZ",
			ZAT_ReferenceRateType = "TEST",
			ZAT_StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			ZAT_EndDate = new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc),
		};
	}
}
