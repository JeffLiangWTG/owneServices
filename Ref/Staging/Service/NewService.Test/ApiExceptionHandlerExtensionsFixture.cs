using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Common.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WTG.OpenIDConnect.Token;

namespace CargoWise.RefDbRepo.Staging.NewService.Test
{
	[TestFixture]
	class ApiExceptionHandlerExtensionsFixture
	{
		[Test]
		public async Task HandleExceptionWithoutAuthentication()
		{
			var log = new Mock<ILog>();
			var logWrapper = new Mock<ILogWrapper>();
			logWrapper.Setup(x => x.GetLog<ExceptionHandlerMiddleware>()).Returns(log.Object);
			logWrapper.Setup(x => x.GetLog<TrackDurationAttribute>()).Returns(log.Object);
			var stagingRepo = new Mock<IStagingRepository>();
			using (var factory = IntegrationTestHelper.GetWebAppFactory(stagingRepo.Object, null).WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.AddSingleton(SetupRepoMock().Object)
					.AddSingleton(logWrapper.Object);
				});
			}))
			{
				var errorMessage = "Could not find a property named 'RefCusCodeTypeLanguage' on type 'CargoWise.RefDbRepo.Staging.Schema_New.RefCusCodeType'.";
				var uri = new Uri("http://localhost/odata/RefCusCodeTypeUpdate?$expand=RefCusCodeTypeLanguage");
				using (var client = factory.CreateClient())
				{
					using (var response = await client.GetAsync(uri))
					{
						var contentString = await response.Content.ReadAsStringAsync();
						log.Verify(x => x.Error("Exception in /odata/RefCusCodeTypeUpdate?$expand=RefCusCodeTypeLanguage", It.Is<Exception>(e => e.Message == errorMessage)));
						Assert.AreEqual("An unexpected server error occurred.", contentString);
					}
				}
			}
		}

		[Test]
		public async Task HandleExceptionWithAuthentication()
		{
			var log = new Mock<ILog>();
			var logWrapper = new Mock<ILogWrapper>();
			logWrapper.Setup(x => x.GetLog<ExceptionHandlerMiddleware>()).Returns(log.Object);
			logWrapper.Setup(x => x.GetLog<TrackDurationAttribute>()).Returns(log.Object);
			var tokenValidationHelper = new Mock<ITokenValidationHelper>();
			tokenValidationHelper.Setup(x => x.ShouldHandle(accessToken)).Returns(true);
			tokenValidationHelper.Setup(x => x.VerifyAccessTokenAsync(accessToken, It.IsAny<ConcurrentDictionary<string, IConfigurationManagerWithLock>>(), It.IsAny<ILogger>(), CancellationToken.None)).Returns(Task.FromResult(new JwtSecurityToken(accessToken)));
			var stagingRepo = new Mock<IStagingRepository>();
			var refDbRepoCrypto = new Mock<IRefDbRepoCrypto>();
			using (var factory = IntegrationTestHelper.GetWebAppFactory(stagingRepo.Object, refDbRepoCrypto.Object).WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.AddSingleton(SetupRepoMock().Object)
					.AddSingleton(logWrapper.Object)
					.AddScoped(sp => tokenValidationHelper.Object);
				});
			}))
			{
				var errorMessage = "Could not find a property named 'RefCusCodeTypeLanguage' on type 'CargoWise.RefDbRepo.Staging.Schema_New.RefCusCodeType'.";
				var innerErrorMessage = "Microsoft.OData.ODataException: Could not find a property named 'RefCusCodeTypeLanguage' on type 'CargoWise.RefDbRepo.Staging.Schema_New.RefCusCodeType'.";
				var uri = new Uri("http://localhost/odata/RefCusCodeTypeUpdate?$expand=RefCusCodeTypeLanguage");
				using (var client = factory.CreateClient())
				{
					client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
					using (var response = await client.GetAsync(uri))
					{
						var contentString = await response.Content.ReadAsStringAsync();
						log.Verify(x => x.Error($"Exception in /odata/RefCusCodeTypeUpdate?$expand=RefCusCodeTypeLanguage - User {userId}", It.Is<Exception>(e => e.Message == errorMessage)));
						Assert.AreEqual($"System.ArgumentException: {errorMessage}\r\n{innerErrorMessage}\r\n", contentString);
					}
				}
			}
		}

		[Test]
		public async Task UseApiExceptionHandlerBeforeOdata()
		{
			var log = new Mock<ILog>();
			var logWrapper = new Mock<ILogWrapper>();
			logWrapper.Setup(x => x.GetLog<ExceptionHandlerMiddleware>()).Returns(log.Object);
			logWrapper.Setup(x => x.GetLog<TrackDurationAttribute>()).Returns(log.Object);
			var stagingRepo = new Mock<IStagingRepository>();
			using (var factory = IntegrationTestHelper.GetWebAppFactory(stagingRepo.Object, null).WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.AddSingleton(SetupRepoMock().Object);
					services.AddSingleton(logWrapper.Object);
				});
			}))
			{
				var uri = new Uri("http://localhost/odata/$batch");

				using (var client = factory.CreateClient())
				{
					using (var httpContent = new StringContent(@"{}"))
					{
						httpContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);
						using (var response = await client.PostAsync(uri, httpContent))
						{
							_ = await response.Content.ReadAsStringAsync();
							log.Verify(x => x.Error("Exception in /odata/$batch", It.Is<Exception>(e => e.Source == "Microsoft.OData.Core")));
						}
					}
				}
			}
		}

		[Test]
		public async Task UseApiExceptionHandlerBeforeMapControllers()
		{
			var log = new Mock<ILog>();
			var logWrapper = new Mock<ILogWrapper>();
			logWrapper.Setup(x => x.GetLog<ExceptionHandlerMiddleware>()).Returns(log.Object);
			logWrapper.Setup(x => x.GetLog<TrackDurationAttribute>()).Returns(log.Object);
			var stagingRepo = new Mock<IStagingRepository>();
			using (var factory = IntegrationTestHelper.GetWebAppFactory(stagingRepo.Object, null).WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.AddSingleton(SetupRepoMock().Object);
					services.AddSingleton(logWrapper.Object);
				});
			}))
			{
				var uri = new Uri("http://localhost/odata/$batch");

				using (var client = factory.CreateClient())
				{
					using (var httpContent = new StringContent(@"
					{
						""requests"":[
							{
								""id"":""1"",
								""method"":""Get"",
								""url"":""http://localhost/odata/RefAccTaxRateUpdate""
							}
						]
					}"))
					{
						httpContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);
						using (var response = await client.PostAsync(uri, httpContent))
						{
							_ = await response.Content.ReadAsStringAsync();
							log.Verify(x => x.Error("Exception in /odata/RefAccTaxRateUpdate", It.Is<Exception>(e => e.Message == "Operation is not valid.")));
						}
					}
				}
			}
		}

		static Mock<IStagingRepository> SetupRepoMock()
		{
			var repo = new Mock<IStagingRepository>();
			repo.Setup(x => x.Get<RefAccTaxRate>()).Throws(new InvalidOperationException("Operation is not valid."));
			return repo;
		}

		const string userId = "42CFADE3-F079-45C0-B778-9D3B2BA9CA21";
		readonly string accessToken = AccessTokenGenerator.GenerateS2SAccessToken(userId, DateTime.Now);
	}
}
