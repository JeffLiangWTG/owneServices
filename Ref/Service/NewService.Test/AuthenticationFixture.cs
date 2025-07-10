using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WTG.OpenIDConnect.Token;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class AuthenticationFixture
	{
		[Test]
		public void BasicAuthentication()
		{
			var authenticationHelper = new Mock<IAuthenticationHelper>();
			ClaimsPrincipal principal = new ClaimsPrincipal(new GenericIdentity("abc", AuthType.BasicAuth));
			authenticationHelper.Setup(x => x.GetClaimsPrincipal(It.IsAny<string>())).Returns(principal);
			var tokenValidationHelper = new Mock<ITokenValidationHelper>();

			using (var factory = WebApplicationFactoryHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.AddScoped(sp => repo.Object)
					.AddScoped(sp => readOnlyRepo.Object)
					.AddScoped(sp => authenticationHelper.Object)
					.AddScoped(sp => tokenValidationHelper.Object);

					services.Configure<TestServerOptions>(options =>
					{
						options.AllowSynchronousIO = true;
					});
				});
			}))
			using (var client = factory.CreateClient())
			{
				HttpResponseMessage response = null;
				Assert.DoesNotThrowAsync(async () => response = await client.GetAsync(uri));
				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			}
		}

		[Test]
		public void TokenAuthentication()
		{
			var authenticationHelper = new Mock<IAuthenticationHelper>();
			ClaimsPrincipal principal = null;
			var accessToken = AccessTokenGenerator.GenerateS2SAccessToken("A780C457-39F2-40E4-82DC-D74F39CFD2F7", DateTime.Now);
			authenticationHelper.Setup(x => x.GetClaimsPrincipal(It.IsAny<string>())).Returns(principal);
			var tokenValidationHelper = new Mock<ITokenValidationHelper>();
			tokenValidationHelper.Setup(x => x.ShouldHandle(accessToken)).Returns(true);
			tokenValidationHelper.Setup(x => x.VerifyAccessTokenAsync(accessToken, It.IsAny<ConcurrentDictionary<string, IConfigurationManagerWithLock>>(), It.IsAny<ILogger>(), CancellationToken.None)).Returns(Task.FromResult(new JwtSecurityToken(accessToken)));

			using (var factory = WebApplicationFactoryHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll<ITokenValidationHelper>();
					services.AddScoped(sp => repo.Object)
					.AddScoped(sp => readOnlyRepo.Object)
					.AddScoped(sp => authenticationHelper.Object)
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
				HttpResponseMessage response = null;
				Assert.DoesNotThrowAsync(async () => response = await client.GetAsync(uri));
				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			}
		}

		[Test]
		public async Task InvalidAuthenticationReturn401()
		{
			var authenticationHelper = new Mock<IAuthenticationHelper>();
			ClaimsPrincipal principal = null;
			authenticationHelper.Setup(x => x.GetClaimsPrincipal(It.IsAny<string>())).Returns(principal);
			var tokenValidationHelper = new Mock<ITokenValidationHelper>();
			using (var factory = WebApplicationFactoryHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.AddScoped(sp => repo.Object)
					.AddScoped(sp => readOnlyRepo.Object)
					.AddScoped(sp => authenticationHelper.Object)
					.AddScoped(sp => tokenValidationHelper.Object);

					services.Configure<TestServerOptions>(options =>
					{
						options.AllowSynchronousIO = true;
					});
				});
			}))
			using (var client = factory.CreateClient())
			{
				HttpResponseMessage response = null;
				Assert.DoesNotThrowAsync(async () => response = await client.GetAsync(uri));
				Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
				var resultContent = await response.Content.ReadAsStringAsync();
				Assert.AreEqual("Unauthorized request", resultContent);
			}
		}

		[SetUp]
		public void Setup()
		{
			repo = new Mock<IReferenceDataRepository>();
			readOnlyRepo = new Mock<IReadOnlyReferenceDataRepository>();
			readOnlyRepo.Setup(x => x.Get<RefDataSetInformation>()).Returns(new[] { dataSetInformation }.AsQueryable());
		}

		Mock<IReferenceDataRepository> repo;
		Mock<IReadOnlyReferenceDataRepository> readOnlyRepo;
		readonly Uri uri = new Uri("http://localhost/RefAccTaxRate/GetServerTimestamp");
		readonly RefDataSetInformation dataSetInformation = new RefDataSetInformation
		{
			RDS_PK = Guid.NewGuid(),
			RDS_DataSetId = 1,
			RDS_DataSetName = "RefAccTaxRate",
			RDS_TableName = "RefAccTaxRate",
			RDS_DataSetTableCode = "ZAT",
			RDS_LastUpdatedUTC = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
		};
	}
}
