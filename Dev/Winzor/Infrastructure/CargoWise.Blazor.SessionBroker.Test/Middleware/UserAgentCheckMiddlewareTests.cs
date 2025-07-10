using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CargoWise.Blazor.Client.Integration;
using CargoWise.Blazor.SessionBroker.Middleware;
using CargoWise.Blazor.SessionBroker.StaticFiles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test.Middleware
{
	public class UserAgentCheckMiddlewareTests
	{
		[TestCase("POST", "/")]
		[TestCase("GET", "/_blazor")]
		[TestCase("POST", "/_blazor")]
		[TestCase("GET", "/_blazor/disconnect")]
		[TestCase("POST", "/_blazor/disconnect")]
		[TestCase("GET", "/_sessionbroker/auth")]
		public async Task ClientAppUserAgentCheckMiddleware_AllowedRouteWithNoUserAgent_ShouldLogInformationAndBypassUserAgentCheck(string httpMethod, string path)
		{
			var logger = new Mock<ILogger<UserAgentCheckMiddleware>>();
			using var factory = new CustomWebApplicationFactory<Startup>() { AddDefaultUserAgentHeader = false };
			using var client = factory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					services.AddSingleton(_ => logger.Object);
				});
			}).CreateClient();
			var response = httpMethod switch
			{
				"GET" => await client.GetAsync(path),
				"POST" => await client.PostAsync(path, null),
				_ => throw new NotSupportedException($"HTTP method {httpMethod} is not supported.")
			};

			logger.Verify(l => l.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((value, _) => value.ToString().Equals($"Skipping UserAgent check for request http://localhost{path}.")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>())
			, Times.Once);
		}

		[Test]
		public async Task ClientAppUserAgentCheckMiddleware_MainRouteWithNoUserAgent_ShouldForwardToBrowserLaunchPad()
		{
			var logger = new Mock<ILogger<UserAgentCheckMiddleware>>();
			using var factory = new CustomWebApplicationFactory<Startup>() { AddDefaultUserAgentHeader = false };
			using var client = factory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					services.AddSingleton(_ => logger.Object);
				});
			}).CreateClient();
			var response = await client.GetAsync("/");

			logger.Verify(l => l.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((value, _) => value.ToString().Equals($"The {RequestHeaders.ClientAppUserAgent} header was not provided for the request http://localhost/. Forwarding to BrowserLaunchPad.")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>())
			, Times.Once);

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("Please wait while CargoWise is launched..."));
		}

		[Test]
		public async Task ClientAppUserAgentCheckMiddleware_MainRouteWithInvalidUserAgent_ShouldForwardToBrowserLaunchPad()
		{
			var logger = new Mock<ILogger<UserAgentCheckMiddleware>>();
			using var factory = new CustomWebApplicationFactory<Startup>() { AddDefaultUserAgentHeader = false };
			using var client = factory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					services.AddSingleton(_ => logger.Object);
				});
			}).CreateClient();
			client.DefaultRequestHeaders.Add(RequestHeaders.ClientAppUserAgent, "invalid-header");
			var response = await client.GetAsync("/");

			logger.Verify(l => l.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((value, _) => value.ToString().Equals($"The {RequestHeaders.ClientAppUserAgent} header was not provided for the request http://localhost/. Forwarding to BrowserLaunchPad.")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>())
			, Times.Once);

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("Please wait while CargoWise is launched..."));
		}

		[TestCase("/backchannel")]
		[TestCase("/?Command=ShowEditForm")]
		public async Task ClientAppUserAgentCheckMiddleware_RestrictedRouteWithNoUserAgent_ShouldLogErrorAndShowDialog(string path)
		{
			var logger = new Mock<ILogger<UserAgentCheckMiddleware>>();
			using var factory = new CustomWebApplicationFactory<Startup>() { AddDefaultUserAgentHeader = false };
			using var client = factory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					services.AddSingleton(_ => logger.Object);
				});
			}).CreateClient();
			var response = await client.GetAsync(path);

			logger.Verify(l => l.Log(
				LogLevel.Warning,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((value, _) => value.ToString().Equals($"The {RequestHeaders.ClientAppUserAgent} header was not provided for the request http://localhost{path}.")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>())
			, Times.Once);

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
		}

		[TestCase("/backchannel")]
		[TestCase("/?Command=ShowEditForm")]
		public async Task ClientAppUserAgentCheckMiddleware_RestrictedRouteWithInvalidUserAgent_ShouldLogErrorAndShowDialog(string path)
		{
			var logger = new Mock<ILogger<UserAgentCheckMiddleware>>();
			using var factory = new CustomWebApplicationFactory<Startup>() { AddDefaultUserAgentHeader = false };
			using var client = factory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					services.AddSingleton(_ => logger.Object);
				});
			}).CreateClient();
			client.DefaultRequestHeaders.Add(RequestHeaders.ClientAppUserAgent, "invalid-header");
			var response = await client.GetAsync(path);

			logger.Verify(l => l.Log(
				LogLevel.Warning,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((value, _) => value.ToString().Equals($"The {RequestHeaders.ClientAppUserAgent} value invalid-header was invalid for the request http://localhost{path}.")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>())
			, Times.Once);

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
		}

		[Obsolete]
		[Test]
		public async Task ClientAppUserAgentCheckMiddleware_WithOnlyLegacyUserAgent_ShouldLogWarningAndShowDialog()
		{
			var logger = new Mock<ILogger<UserAgentCheckMiddleware>>();
			using var factory = new CustomWebApplicationFactory<Startup>() { AddDefaultUserAgentHeader = false };
			using var client = factory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					services.AddSingleton(_ => logger.Object);
				});
			}).CreateClient();
			client.DefaultRequestHeaders.Add(RequestHeaders.UserAgentKeyObsolete, RequestHeaders.ClientAppUserAgentPrefix);
			var response = await client.GetAsync("/");

			logger.Verify(l => l.Log(
				LogLevel.Warning,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((value, _) => value.ToString().Equals($"The {RequestHeaders.ClientAppUserAgent} header was not provided for the request http://localhost/. Only the legacy header {RequestHeaders.UserAgentKeyObsolete} was provided.")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>())
			, Times.Once);

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("<title>Automatic update failed</title>"));
		}

		[TestCase("CargoWiseClient/invalid-version")]
		[TestCase("CargoWiseClient/1111")]
		public async Task ClientAppUserAgentCheckMiddleware_WithInvalidUserAgentVersion_ShouldLogErrorAndShowDialog(string invalidHeaderValue)
		{
			var logger = new Mock<ILogger<UserAgentCheckMiddleware>>();
			using var factory = new CustomWebApplicationFactory<Startup>() { AddDefaultUserAgentHeader = false };
			using var client = factory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					services.AddSingleton(_ => logger.Object);
				});
			}).CreateClient();
			client.DefaultRequestHeaders.Add(RequestHeaders.ClientAppUserAgent, invalidHeaderValue);
			var response = await client.GetAsync("/");

			logger.Verify(l => l.Log(
				LogLevel.Error,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((value, _) => value.ToString().Equals($"The {RequestHeaders.ClientAppUserAgent} value was invalid. {invalidHeaderValue}")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>())
			, Times.Once);

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("<title>Automatic update failed</title>"));
		}

		[Test]
		public async Task ClientAppUserAgentCheckMiddleware_WithOutdatedUserAgentVersion_ShouldLogErrorAndShowDialog()
		{
			var logger = new Mock<ILogger<UserAgentCheckMiddleware>>();
			var appInstallerHandler = new Mock<IAppInstallerHandler>();
			appInstallerHandler.Setup(h => h.GetAppInstallerInfo()).Returns(new AppInstallerInfo("TestPackage", "TestPublisher", new Version(1, 2, 3, 5)));

			using var factory = new CustomWebApplicationFactory<Startup>() { AddDefaultUserAgentHeader = false };
			using var client = factory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					services.AddSingleton(_ => logger.Object);
					services.AddSingleton(appInstallerHandler.Object);
				});
			}).CreateClient();
			client.DefaultRequestHeaders.Add(RequestHeaders.ClientAppUserAgent, "CargoWiseClient/1.2.3.4");
			var response = await client.GetAsync("/");

			logger.Verify(l => l.Log(
				LogLevel.Error,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((value, _) => value.ToString().Equals($"The {RequestHeaders.ClientAppUserAgent} version provided is not supported by the server. Required Version: 1.2.3.5 Provided Version: 1.2.3.4")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>())
			, Times.Once);

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("<title>Automatic update failed</title>"));
		}

		[Test]
		public async Task ClientAppUserAgentCheckMiddleware_WithSameUserAgentVersion_ShouldSucceedAndContinue()
		{
			var logger = new Mock<ILogger<UserAgentCheckMiddleware>>();
			var appInstallerHandler = new Mock<IAppInstallerHandler>();
			appInstallerHandler.Setup(h => h.GetAppInstallerInfo()).Returns(new AppInstallerInfo("TestPackage", "TestPublisher", new Version(1, 2, 3, 4)));

			using var factory = new CustomWebApplicationFactory<Startup>() { AddDefaultUserAgentHeader = false };
			using var client = factory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					services.AddSingleton(_ => logger.Object);
					services.AddSingleton(appInstallerHandler.Object);
				});
			}).CreateClient();
			client.DefaultRequestHeaders.Add(RequestHeaders.ClientAppUserAgent, "CargoWiseClient/1.2.3.4");
			var response = await client.GetAsync("/");

			logger.Verify(l => l.Log(
				LogLevel.Error,
				It.IsAny<EventId>(),
				It.IsAny<It.IsAnyType>(),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>())
			, Times.Never);

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(await response.Content.ReadAsStringAsync(), Does.Not.Contain("<title>Automatic update failed</title>"));
		}

		[Test]
		public async Task ClientAppUserAgentCheckMiddleware_WithNewerUserAgentVersion_ShouldSucceedAndContinue()
		{
			var logger = new Mock<ILogger<UserAgentCheckMiddleware>>();
			var appInstallerHandler = new Mock<IAppInstallerHandler>();
			appInstallerHandler.Setup(h => h.GetAppInstallerInfo()).Returns(new AppInstallerInfo("TestPackage", "TestPublisher", new Version(1, 2, 3, 4)));

			using var factory = new CustomWebApplicationFactory<Startup>() { AddDefaultUserAgentHeader = false };
			using var client = factory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					services.AddSingleton(_ => logger.Object);
					services.AddSingleton(appInstallerHandler.Object);
				});
			}).CreateClient();
			client.DefaultRequestHeaders.Add(RequestHeaders.ClientAppUserAgent, "CargoWiseClient/1.2.3.5");
			var response = await client.GetAsync("/");

			logger.Verify(l => l.Log(
				LogLevel.Error,
				It.IsAny<EventId>(),
				It.IsAny<It.IsAnyType>(),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>())
			, Times.Never);

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(await response.Content.ReadAsStringAsync(), Does.Not.Contain("<title>Automatic update failed</title>"));
		}
	}
}
