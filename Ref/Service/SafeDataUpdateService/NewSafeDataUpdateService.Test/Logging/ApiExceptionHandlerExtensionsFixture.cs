using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.ErrorReporting;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Common.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	class ApiExceptionHandlerExtensionsFixture
	{
		[Test]
		public async Task LogError()
		{
			var log = new Mock<ILog>();
			var logWrapper = new Mock<ILogWrapper>();
			var errorRportWrapper = new Mock<ErrorReportingClientWrapper>(null);
			logWrapper.Setup(x => x.GetLog<ExceptionHandlerMiddleware>()).Returns(log.Object);
			logWrapper.Setup(x => x.GetLog<TrackDurationAttribute>()).Returns(log.Object);
			using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.AddSingleton(SetupRepoMock().Object);
					services.AddSingleton(logWrapper.Object);
					services.AddSingleton(errorRportWrapper.Object);
				});
			}))
			{
				var uri = new Uri("http://localhost/odata/RefAccTaxRateUpdate");

				using (var client = factory.CreateClient())
				{
					client.DefaultRequestHeaders.Add("ReportIssue", "true");
					client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
					using (var response = await client.GetAsync(uri))
					{
						var contentString = await response.Content.ReadAsStringAsync();
						log.Verify(x => x.Error(
							$"Exception in /odata/RefAccTaxRateUpdate - User {userId}",
							It.Is<Exception>(e => e.Message == "Operation is not valid."))
						);
						errorRportWrapper.Verify(x => x.PostCrashReport(It.IsAny<InvalidOperationException>(), "Safe Update Service", null));
						Assert.AreEqual("System.InvalidOperationException: Operation is not valid.\r\n", contentString);
					}
				}
			}
		}

		[Test]
		public async Task LogError_ShouldNotReport()
		{
			var log = new Mock<ILog>();
			var logWrapper = new Mock<ILogWrapper>();
			var errorRportWrapper = new Mock<ErrorReportingClientWrapper>(null);
			logWrapper.Setup(x => x.GetLog<ExceptionHandlerMiddleware>()).Returns(log.Object);
			logWrapper.Setup(x => x.GetLog<TrackDurationAttribute>()).Returns(log.Object);
			using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.AddSingleton(SetupRepoMock().Object);
					services.AddSingleton(logWrapper.Object);
					services.AddSingleton(errorRportWrapper.Object);
				});
			}))
			{
				var uri = new Uri("http://localhost/odata/RefAccTaxRateUpdate");

				using (var client = factory.CreateClient())
				{
					client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
					using (var response = await client.GetAsync(uri))
					{
						var contentString = await response.Content.ReadAsStringAsync();
						log.Verify(x => x.Error(
							$"Exception in /odata/RefAccTaxRateUpdate - User {userId}",
							It.Is<Exception>(e => e.Message == "Operation is not valid."))
						);
						Assert.AreEqual("System.InvalidOperationException: Operation is not valid.\r\n", contentString);
						errorRportWrapper.Verify(x => x.PostCrashReport(It.IsAny<Exception>(), "Safe Update Service", null), Times.Never);
					}
				}
			}
		}


		[Test]
		public async Task ThrowInvalidOperationException_WhenUseExceptionHandler()
		{
			var log = new Mock<ILog>();
			var logger = new Mock<ILogger>();
			var logWrapper = new Mock<ILogWrapper>();
			var errorRportWrapper = new Mock<ErrorReportingClientWrapper>(null);
			var loggerProvider = new Mock<ILoggerProvider>();
			loggerProvider.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
			using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.AddSingleton(SetupRepoMock().Object);
					services.AddSingleton(logWrapper.Object);
					services.AddSingleton(errorRportWrapper.Object);
					services.AddLogging(config => config.AddProvider(loggerProvider.Object));
				});
			}))
			{
				var uri = new Uri("http://localhost/odata/RefAccTaxRateUpdate");

				using (var client = factory.CreateClient())
				using (var response = await client.GetAsync(uri))
				{
					var contentString = await response.Content.ReadAsStringAsync();
					Assert.IsTrue(logger.Invocations.Any(x => x.ToString().Contains("System.InvalidOperationException: Fail to get Log.")));
				}
			}
		}

		[Test]
		public async Task UseApiExceptionHandlerBeforeOdata()
		{
			var log = new Mock<ILog>();
			var logWrapper = new Mock<ILogWrapper>();
			var errorRportWrapper = new Mock<ErrorReportingClientWrapper>(null);
			logWrapper.Setup(x => x.GetLog<ExceptionHandlerMiddleware>()).Returns(log.Object);
			using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.AddSingleton(SetupRepoMock().Object);
					services.AddSingleton(logWrapper.Object);
					services.AddSingleton(errorRportWrapper.Object);
				});
			}))
			{
				var uri = new Uri("http://localhost/odata/$batch");

				using (var client = factory.CreateClient())
				{
					client.DefaultRequestHeaders.Add("ReportIssue", "true");
					using (var httpContent = new StringContent(@"{}"))
					{
						httpContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);
						using (var response = await client.PostAsync(uri, httpContent))
						{
							_ = await response.Content.ReadAsStringAsync();
							log.Verify(x => x.Error(
								"Exception in /odata/$batch",
								It.Is<Exception>(e => e.Source == "Microsoft.OData.Core")
								));
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
			var errorRportWrapper = new Mock<ErrorReportingClientWrapper>(null);
			logWrapper.Setup(x => x.GetLog<ExceptionHandlerMiddleware>()).Returns(log.Object);
			logWrapper.Setup(x => x.GetLog<TrackDurationAttribute>()).Returns(log.Object);
			using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.AddSingleton(SetupRepoMock().Object);
					services.AddSingleton(logWrapper.Object);
					services.AddSingleton(errorRportWrapper.Object);
				});
			}))
			{
				var uri = new Uri("http://localhost/odata/$batch");

				using (var client = factory.CreateClient())
				{
					client.DefaultRequestHeaders.Add("ReportIssue", "true");
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
							log.Verify(x => x.Error(
									"Exception in /odata/RefAccTaxRateUpdate",
									It.Is<Exception>(e => e.Message == "Operation is not valid."))
							);
						}
					}
				}
			}
		}

		static Mock<IReferenceDataRepository> SetupRepoMock()
		{
			var repo = new Mock<IReferenceDataRepository>();
			repo.Setup(x => x.Get<RefAccTaxRate>()).Throws(new InvalidOperationException("Operation is not valid."));
			return repo;
		}

		const string userId = "42CFADE3-F079-45C0-B778-9D3B2BA9CA21";
		readonly string accessToken = AccessTokenGenerator.GenerateS2SAccessToken(userId, DateTime.Now);
	}
}
