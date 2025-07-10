using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CargoWise.Blazor.Client.Integration;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.Common.Testing.Auth;
using CargoWise.Blazor.SessionBroker.Authentication;
using CargoWise.Blazor.SessionBroker.Helpers;
using CargoWise.Blazor.SessionBroker.Pages;
using CargoWise.Blazor.Testing.Common;
using CargoWiseNext.Infrastructure.Authentication;
using CargoWiseNext.Infrastructure.Authentication.Test;
using Enterprise.Core.Environment.ZA;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.OAuth2.Token.TestFramework;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.SessionAffinity;

namespace CargoWise.Blazor.SessionBroker.Test
{
	using static TestHelpers;

	[KillBlazorAppProcesses]
	public class AffinityFailurePolicyTests : TestWithDatabase
	{
		Mock<IDataProtectionProvider> dataProtectionProvider;

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			dataProtectionProvider = new Mock<IDataProtectionProvider>();
			var mockDataProtector = new Mock<IDataProtector>();
			dataProtectionProvider.Setup(x => x.CreateProtector("Yarp.ReverseProxy.SessionAffinity.CookieSessionAffinityPolicy")).Returns(mockDataProtector.Object);
		}

		[Test]
		public async Task RequestWithNoSessionCookieOrClientTokenShouldRedirectAuthPage()
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			using var client = factory.CreateClient();
			var response = await client.GetAsync("/");
			Assert.That(response.RequestMessage.RequestUri.AbsolutePath, Is.EqualTo("/_sessionbroker/auth"));
		}

		[Test]
		public async Task InvalidTokenGets401()
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			using var client = factory.CreateClient();
			var response = await client.GetAsync($"/?{QueryParameters.ClientToken}=wrong");

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
		}

		[Test]
		public async Task ValidTokenLaunchesCargoWiseBlazorAndSetsAffinityCookieWithParamsRemoved()
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			var cargoWiseOptions = factory.Services.GetRequiredService<IOptions<CargoWiseOptions>>();
			var tokenGenerator = new DebugClientTokenGenerator(cargoWiseOptions);
			var token = tokenGenerator.GenerateClientToken();
			using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
			{
				AllowAutoRedirect = false
			});
			var response = await client.GetAsync($"/?{QueryParameters.ClientToken}={token}&persist=test");
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
			// ClientToken and Persist parameters should be removed from the redirected URL
			Assert.That(response.Headers.Location.OriginalString, Is.EqualTo("/"));

			var redirectedRequest = await client.GetAsync(response.Headers.Location);
			Assert.That(GetAffinityCookie(redirectedRequest, factory.Server.BaseAddress), Is.Not.Empty);
		}

		[Test]
		public void TestDbConfig_NoDbConnection_WillThrowException()
		{
			using var f = new CustomWebApplicationFactory<Startup>();
			using var factory = f.WithWebHostBuilder(builder =>
			{
				builder.ConfigureAppConfiguration(
				(_, configBuilder) =>
				{
					configBuilder.Sources.Clear();
					configBuilder.AddInMemoryCollection(
						new Dictionary<string, string>
						{
						{ "CargoWiseOptions:DbServerName", "" },
						{ "CargoWiseOptions:DatabaseName", "" }
						});
				});
			});

			Assert.That(() => factory.CreateClient(), Throws.InstanceOf<InvalidDatabaseConfigurationException>().With.Message.EqualTo("No database connection information available."));
		}

		[TestCase(null, null)]
		[TestCase("?loginPrompt=login", "?loginPrompt=login")]
		[TestCase("?persist=TestPersistForm", "?persist=TestPersistForm")]
		[TestCase("?netcore", "?netcore")]
		[TestCase("?netcore&loginPrompt=login&persist=TestPersistForm", "?netcore&loginPrompt=login&persist=TestPersistForm")]
		[TestCase("?netcore&loginPrompt=login&persist=TestPersistForm&someOtherParameters", "?netcore&loginPrompt=login&persist=TestPersistForm")]
		public async Task TestQueryStringWhenRedirectToAuthEndpoint(string requestQuery, string redirectQuery)
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			var cargoWiseOptions = factory.Services.GetRequiredService<IOptions<CargoWiseOptions>>();

			var context = new DefaultHttpContext();
			var config = new TestConfiguration();
			var reverseProxyConfig = new Mock<IProxyConfigProvider>();
			var testAppServerProcess = TestAppServerProcess.FromTestContext(config);
			var appInstanceService = new AppInstanceService(
				reverseProxyConfigProvider: reverseProxyConfig.Object,
				logger: NullLogger<AppInstanceService>.Instance,
				appServerProcess: testAppServerProcess,
				secureSecretGenerator: new SecureSecretGenerator(),
				sessionSecretStore: new SessionSecretStore(),
				cargoWiseOptions: cargoWiseOptions);
			var cookieSessionAffinityProviderOptions = Options.Create(new AffinityCookieOptionsProvider());
			using var memoryCache = new MemoryCache(new MemoryCacheOptions());
			var policy = new AuthenticatingAffinityFailurePolicy(
				cargoWiseOptions: cargoWiseOptions,
				appInstanceService: appInstanceService,
				affinityCookieProviderOptions: cookieSessionAffinityProviderOptions,
				dataProtectionProvider: dataProtectionProvider.Object,
				logger: NullLogger<AuthenticatingAffinityFailurePolicy>.Instance,
				cargoWiseAuthenticator: new CargoWiseAuthenticator(
					databaseAccessor,
					new AuthenticationDatabaseAccessor(databaseAccessor, NullLogger.Instance),
					new AuthenticationConfigAccessor(new RegistryAccessor(databaseAccessor)),
					NullLogger.Instance,
					Mock.Of<ISiteOfflineChecker>()),
				factory.Services,
				memoryCache);

			context.Request.Path = "/";
			context.Request.QueryString = new QueryString(requestQuery);

			var result = await policy.Handle(context, null, AffinityStatus.AffinityKeyExtractionFailed);
			Assert.That(result, Is.False);
			Assert.That(context.Response.Headers["Location"].ToString(), Is.EqualTo($"/_sessionbroker/auth{redirectQuery}"));
		}

		[Test]
		public async Task ShouldPromptMessageBoxWhenAuthorisationFailed()
		{
			var (response, _, _) = await MockLoginService.OIDCLoginAsync(false);

			Assert.That(response.AccessToken, Is.Not.Empty);
			Assert.That(response.IdentityToken, Is.Not.Empty);

			using var factory = new CustomWebApplicationFactory<Startup>();

			var cargoWiseOptions = factory.Services.GetRequiredService<IOptions<CargoWiseOptions>>();
			var memoryCache = factory.Services.GetRequiredService<IMemoryCache>();

			var context = new DefaultHttpContext();
			context.Response.Body = new MemoryStream();
			var config = new TestConfiguration();
			var reverseProxyConfig = new Mock<IProxyConfigProvider>();
			var testAppServerProcess = TestAppServerProcess.FromTestContext(config);
			var appInstanceService = new AppInstanceService(
				reverseProxyConfigProvider: reverseProxyConfig.Object,
				logger: NullLogger<AppInstanceService>.Instance,
				appServerProcess: testAppServerProcess,
				secureSecretGenerator: new SecureSecretGenerator(),
				sessionSecretStore: new SessionSecretStore(),
				cargoWiseOptions: cargoWiseOptions);
			var cookieSessionAffinityProviderOptions = Options.Create(new AffinityCookieOptionsProvider());

			var mockAutnenticator = new Mock<ICargoWiseAuthenticator>();
			mockAutnenticator.Setup(x => x.VerifyAccessTokenAsync(It.IsAny<CargoWiseAuthCookie>(), It.IsAny<CancellationToken>()))
							 .Returns(Task.FromResult(VerificationResult.AuthorisationFailed));

			var policy = new AuthenticatingAffinityFailurePolicy(
				cargoWiseOptions: cargoWiseOptions,
				appInstanceService: appInstanceService,
				affinityCookieProviderOptions: cookieSessionAffinityProviderOptions,
				dataProtectionProvider: dataProtectionProvider.Object,
				logger: NullLogger<AuthenticatingAffinityFailurePolicy>.Instance,
				cargoWiseAuthenticator: mockAutnenticator.Object,
				factory.Services,
				memoryCache);

			context.Request.Path = "/";
			context.Request.Headers[HeaderNames.Cookie] = ExtractCookieValue(response);

			var result = await policy.Handle(context, null, AffinityStatus.AffinityKeyExtractionFailed);
			context.Response.Body.Seek(0, SeekOrigin.Begin);

			Assert.That(result, Is.False);
			Assert.That(context.Response.StatusCode, Is.EqualTo(200));
			Assert.That(context.Response.ContentType, Is.EqualTo("text/html"));
			using var reader = new StreamReader(context.Response.Body);
			var body = reader.ReadToEnd();
			Assert.That(body, Does.Contain("Login Failed"));
			Assert.That(body, Does.Contain("The selected user does not have permission to use this application."));
			Assert.That(body, Does.Contain("Would you like to login as a different user?"));
			Assert.That(body, Does.Contain("window.open('cargowiseclient:' + window.location.origin + '/?loginPrompt=SelectAccount');"));

			memoryCache.TryGetValue("LoginFailedPage", out string cacheValue);
			Assert.That(cacheValue, Does.Contain("Login Failed"));
			Assert.That(cacheValue, Does.Contain("The selected user does not have permission to use this application."));
			Assert.That(cacheValue, Does.Contain("Would you like to login as a different user?"));
			Assert.That(cacheValue, Does.Contain("window.open('cargowiseclient:' + window.location.origin + '/?loginPrompt=SelectAccount');"));
		}

		[Test]
		public async Task ShouldShowSiteOfflineMessage()
		{
			var (response, _, _) = await MockLoginService.OIDCLoginAsync(false);

			Assert.That(response.AccessToken, Is.Not.Empty);
			Assert.That(response.IdentityToken, Is.Not.Empty);

			var mockSiteOfflineChecker = new Mock<ISiteOfflineChecker>();
			mockSiteOfflineChecker.Setup(x => x.IsSiteOffline()).Returns(true);
			mockSiteOfflineChecker.Setup(x => x.GetOfflineMessage()).Returns("Offline message example");

			await using var originalFactory = new CustomWebApplicationFactory<Startup>();
			await using var factory = originalFactory
				.WithWebHostBuilder(b => b.ConfigureServices(services =>
				{
					services.AddSingleton(mockSiteOfflineChecker.Object);
				}));

			var cargoWiseOptions = factory.Services.GetRequiredService<IOptions<CargoWiseOptions>>();
			var memoryCache = factory.Services.GetRequiredService<IMemoryCache>();

			var context = new DefaultHttpContext();
			context.Response.Body = new MemoryStream();
			var config = new TestConfiguration();
			var reverseProxyConfig = new Mock<IProxyConfigProvider>();
			var testAppServerProcess = TestAppServerProcess.FromTestContext(config);
			var appInstanceService = new AppInstanceService(
				reverseProxyConfigProvider: reverseProxyConfig.Object,
				logger: NullLogger<AppInstanceService>.Instance,
				appServerProcess: testAppServerProcess,
				secureSecretGenerator: new SecureSecretGenerator(),
				sessionSecretStore: new SessionSecretStore(),
				cargoWiseOptions: cargoWiseOptions);
			var cookieSessionAffinityProviderOptions = Options.Create(new AffinityCookieOptionsProvider());

			var mockAutnenticator = new Mock<ICargoWiseAuthenticator>();
			mockAutnenticator.Setup(x => x.VerifyAccessTokenAsync(It.IsAny<CargoWiseAuthCookie>(), It.IsAny<CancellationToken>()))
							 .Returns(Task.FromResult(VerificationResult.SiteOffline));

			var policy = new AuthenticatingAffinityFailurePolicy(
				cargoWiseOptions: cargoWiseOptions,
				appInstanceService: appInstanceService,
				affinityCookieProviderOptions: cookieSessionAffinityProviderOptions,
				dataProtectionProvider: dataProtectionProvider.Object,
				logger: NullLogger<AuthenticatingAffinityFailurePolicy>.Instance,
				cargoWiseAuthenticator: mockAutnenticator.Object,
				factory.Services,
				memoryCache);

			context.Request.Path = "/";
			context.Request.Headers[HeaderNames.Cookie] = ExtractCookieValue(response);

			var result = await policy.Handle(context, null, AffinityStatus.AffinityKeyExtractionFailed);
			context.Response.Body.Seek(0, SeekOrigin.Begin);

			Assert.That(result, Is.False);
			Assert.That(context.Response.StatusCode, Is.EqualTo(200));
			Assert.That(context.Response.ContentType, Is.EqualTo("text/html"));
			using var reader = new StreamReader(context.Response.Body);
			var body = reader.ReadToEnd();
			Assert.That(body, Does.Contain("Site Offline"));
			Assert.That(body, Does.Contain("Offline message example"));
		}

		string ExtractCookieValue(WTG.OpenIDConnect.Login.OIDCLoginResponseMessage response)
		{
			var cargoWiseAuthCookie = new CargoWiseAuthCookie()
			{
				AccessToken = response.AccessToken,
				IdentityToken = response.IdentityToken,
			};
			var cookieValues = JsonConvert.SerializeObject(cargoWiseAuthCookie);

			return new CookieHeaderValue(OidcConstants.CargoWiseAuthCookieName, HttpUtility.UrlEncode(cookieValues))
			{
				Name = OidcConstants.CargoWiseAuthCookieName,
				Value = HttpUtility.UrlEncode(cookieValues)
			}.ToString();
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task FetchAuthCookie_ShouldValidateLoggedInUserIsActiveWithValidAccessToken(bool generateExpiredToken)
		{
			var (response, authority, clientId) = await MockLoginService.OIDCLoginAsync(generateExpiredToken);

			Assert.That(response.AccessToken, Is.Not.Empty);
			Assert.That(response.IdentityToken, Is.Not.Empty);

			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var context = new DefaultHttpContext();

				var url = $"https://{MockLoginService.GetHostName()}:{identityServer.Port}";
				var registryAccessor = ConfigWithOIDCSettingsHelper.WithRegistryAccessor(
					OIDCConfigSetupHelper.SetOidcConfigTest(url, clientId));

				using var originalFactory = new CustomWebApplicationFactory<Startup>();
				using var factory = originalFactory.WithWebHostBuilder(builder =>
					{
						builder.UseSetting("CargoWiseOptions:AppServerPathOverride", Path.Combine("CargoWise.Blazor.SessionBroker.Test.MockAppServer-bin", "CargoWise.Blazor.SessionBroker.Test.MockAppServer.exe"));
						builder.UseSetting("AdditionalArguments", "--wait 100");
						builder.ConfigureServices(services =>
						{
							services.AddSingleton(registryAccessor);
						});
					});

				var cargoWiseOptions = factory.Services.GetRequiredService<IOptions<CargoWiseOptions>>();
				cargoWiseOptions.Value.SessionBrokerProcessCorrelationId = Guid.NewGuid();
				cargoWiseOptions.Value.VersionBrokerProcessCorrelationId = Guid.NewGuid();

				var policy = CreateAuthenticatingAffinityFailurePolicy(
					cargoWiseOptions,
					appInstanceService: factory.Services.GetRequiredService<AppInstanceService>(),
					registryAccessor: registryAccessor);

				context.Request.Path = "/";

				var result = await policy.Handle(context, null, AffinityStatus.OK);

				Assert.That(result, Is.False);

				Assert.That(context.Response.Headers["Location"].ToString(), Does.Contain("/_sessionbroker/auth"));
				Assert.That(context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.Found));

				context.Request.Headers[HeaderNames.Cookie] = ExtractCookieValue(response);

				result = await policy.Handle(context, null, AffinityStatus.OK);

				if (generateExpiredToken)
				{
					Assert.That(context.Response.Headers["Location"].ToString(), Does.Contain("/_sessionbroker/auth"));
				}
				else
				{
					Assert.That(context.Response.Headers["Location"].ToString(), Is.EqualTo("/"));
					Assert.That(context.Response.Headers.SetCookie.SingleOrDefault(c => c.Contains(AffinityCookieName)), Is.Not.Null);
				}
			}

			AuthenticatingAffinityFailurePolicy CreateAuthenticatingAffinityFailurePolicy(
				IOptions<CargoWiseOptions> cargoWiseOptions,
				AppInstanceService appInstanceService = null,
				IAuthenticationConfigAccessor registryAccessor = null)
			{
				var cookieSessionAffinityProviderOptions = Options.Create(new AffinityCookieOptionsProvider());
				using var memoryCache = new MemoryCache(new MemoryCacheOptions());
				return new AuthenticatingAffinityFailurePolicy(
					cargoWiseOptions: cargoWiseOptions,
					appInstanceService: appInstanceService,
					affinityCookieProviderOptions: cookieSessionAffinityProviderOptions,
					dataProtectionProvider: dataProtectionProvider.Object,
					logger: NullLogger<AuthenticatingAffinityFailurePolicy>.Instance,
					cargoWiseAuthenticator: new CargoWiseAuthenticator(
						databaseAccessor,
						new AuthenticationDatabaseAccessor(databaseAccessor, NullLogger.Instance),
						registryAccessor,
						NullLogger.Instance,
						Mock.Of<ISiteOfflineChecker>()),
					factory.Services,
					memoryCache);
			}
		}
	}
}
