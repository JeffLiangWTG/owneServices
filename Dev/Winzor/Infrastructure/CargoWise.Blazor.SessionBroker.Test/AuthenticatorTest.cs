using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.SessionBroker.Authentication;
using CargoWise.Blazor.SessionBroker.Helpers;
using CargoWise.Blazor.SessionBroker.Pages;
using CargoWise.Blazor.Testing.Common;
using CargoWise.Types;
using CargoWiseNext.Infrastructure.Authentication;
using CargoWiseNext.Infrastructure.Authentication.Test;
using Enterprise.Integration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WTG.OAuth2.Token.TestFramework;
using Yarp.ReverseProxy.SessionAffinity;
using IAuthenticationService = CargoWise.Blazor.Client.Integration.Messaging.IAuthenticationService;
using QueryString = Microsoft.AspNetCore.Http.QueryString;

namespace CargoWise.Blazor.SessionBroker.Test
{
	public class ListLogger<T> : ILogger<T>
	{
		public List<string> Logs { get; } = new List<string>();

		public IDisposable BeginScope<TState>(TState state) => null;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			var message = formatter(state, exception);
			Logs.Add(message);
		}
	}

	public class AuthenticatorTest : TestWithDatabase
	{
		Mock<IDataProtectionProvider> dataProtectionProvider;

		[Test]
		public void SystemSecurityPermissionsShouldBeReferenced()
		{
			// Arrange
			var expectedAssemblyName = "System.Security.Permissions.dll";
			var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, expectedAssemblyName);

			// Assert
			Assert.That(File.Exists(assemblyPath), $"The assembly '{expectedAssemblyName}' should be present in the file system.");
			Assert.DoesNotThrow(() => new System.Security.Policy.ApplicationTrust());
		}

		[Test]
		public async Task LoginMessageIsNullWhenAuthorityUrlAndClientIdAreBothNull()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockLoginService.GetHostName()}:{identityServer.Port}";

				var expectedErrorMessage = "OIDC login configuration is invalid";

				await ArrangeAffinityFailurePolicyAsync(null, string.Empty);

				var authenticationService = CreateAuthenticationService(null, null, errorMessage: expectedErrorMessage);

				var registryAccessor = ConfigWithOIDCSettingsHelper.WithRegistryAccessor(OIDCConfigSetupHelper.SetOidcConfigTest(string.Empty, string.Empty));

				var createAuthenticatorModel = CreateAuthenticatorModel(registryAccessor, authenticationService);

				var messageConstruction = createAuthenticatorModel.authenticatorModel.ConstructLoginMessage();

				Assert.That(messageConstruction, Contains.Substring($"errorMessage: '{expectedErrorMessage}'"));
				Assert.That(createAuthenticatorModel.logger.Logs, Has.Some.Contains("OIDC is enabled, but no valid authority or client identifier is defined. Please check your OIDC config settings and try again. Login will not be completed"));
			}
		}

		[Test]
		public async Task LoginMessageIsNullWhenAuthorityUrlIsNull()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockLoginService.GetHostName()}:{identityServer.Port}";

				var expectedErrorMessage = "OIDC login configuration is invalid";

				var clientId = Guid.NewGuid().ToString();

				await ArrangeAffinityFailurePolicyAsync(null, clientId);

				var authenticationService = CreateAuthenticationService(clientId, null, errorMessage: expectedErrorMessage);

				var registryAccessor = ConfigWithOIDCSettingsHelper.WithRegistryAccessor(OIDCConfigSetupHelper.SetOidcConfigTest(string.Empty, clientId));

				var createAuthenticatorModel = CreateAuthenticatorModel(registryAccessor, authenticationService);

				var messageConstruction = createAuthenticatorModel.authenticatorModel.ConstructLoginMessage();

				Assert.That(messageConstruction, Contains.Substring($"errorMessage: '{expectedErrorMessage}'"));
				Assert.That(createAuthenticatorModel.logger.Logs, Has.Some.Contains("OIDC is enabled, but no valid authority URL is defined. Login will not be completed"));
			}
		}

		[Test]
		public async Task LoginMessageIsNullWhenClientIdIsNull()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockLoginService.GetHostName()}:{identityServer.Port}";

				var expectedErrorMessage = "OIDC login configuration is invalid";

				await ArrangeAffinityFailurePolicyAsync(authorityUrl, string.Empty);

				var authenticationService = CreateAuthenticationService(null, new Uri(authorityUrl), errorMessage: expectedErrorMessage);

				var registryAccessor = ConfigWithOIDCSettingsHelper.WithRegistryAccessor(OIDCConfigSetupHelper.SetOidcConfigTest(authorityUrl, string.Empty));

				var createAuthenticatorModel = CreateAuthenticatorModel(registryAccessor, authenticationService);

				var messageConstruction = createAuthenticatorModel.authenticatorModel.ConstructLoginMessage();

				Assert.That(messageConstruction, Contains.Substring($"errorMessage: '{expectedErrorMessage}'"));
				Assert.That(createAuthenticatorModel.logger.Logs, Has.Some.Contains("OIDC is enabled, but no valid client identifier is defined. Login will not be completed"));
			}
		}

		[Test]
		public async Task LoginMessageIsNullWhenOIDCIsNotEnabled()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockLoginService.GetHostName()}:{identityServer.Port}";

				var expectedErrorMessage = "OIDC login configuration is invalid";

				var clientId = Guid.NewGuid().ToString();

				await ArrangeAffinityFailurePolicyAsync(authorityUrl, clientId);

				var authenticationService = CreateAuthenticationService(clientId, new Uri(authorityUrl), errorMessage: expectedErrorMessage);

				var registryAccessor = ConfigWithOIDCSettingsHelper.WithRegistryAccessor(OIDCConfigSetupHelper.SetOidcConfigTest(authorityUrl, clientId, false));

				var createAuthenticatorModel = CreateAuthenticatorModel(registryAccessor, authenticationService);

				var messageConstruction = createAuthenticatorModel.authenticatorModel.ConstructLoginMessage();

				Assert.That(messageConstruction, Contains.Substring($"errorMessage: '{expectedErrorMessage}'"));
				Assert.That(createAuthenticatorModel.logger.Logs, Has.Some.Contains("OIDC is not enabled. Login will not be completed"));
			}
		}

		[Test]
		public async Task FailedLoginMessageIsReturnWhenNoOidcConfigFoundAsync()
		{
			var expectedErrorMessage = "OIDC login configuration not found";

			await ArrangeAffinityFailurePolicyAsync(null, string.Empty);

			var authenticationService = CreateAuthenticationService(null, null, errorMessage: expectedErrorMessage);

			var registryAccessor = ConfigWithOIDCSettingsHelper.WithRegistryAccessor(null);

			var createAuthenticatorModel = CreateAuthenticatorModel(registryAccessor, authenticationService);

			var messageConstruction = createAuthenticatorModel.authenticatorModel.ConstructLoginMessage();

			Assert.That(messageConstruction, Contains.Substring($"errorMessage: '{expectedErrorMessage}'"));
			Assert.That(createAuthenticatorModel.logger.Logs, Has.Some.Contains(expectedErrorMessage));
		}

		[Test]
		public async Task FailedLoginMessageIsReturnWhenExceptionHappenedAsync()
		{
			var expectedErrorMessage = "Failed to build the login message";

			await ArrangeAffinityFailurePolicyAsync(null, string.Empty);

			var authenticationService = CreateAuthenticationService(null, null, errorMessage: expectedErrorMessage);

			var registryAccessorMock = new Mock<IAuthenticationConfigAccessor>();
			registryAccessorMock.Setup(m => m.GetOIDCConfig())
								.Throws(new Exception("test exception"));

			var createAuthenticatorModel = CreateAuthenticatorModel(registryAccessorMock.Object, authenticationService);

			var messageConstruction = createAuthenticatorModel.authenticatorModel.ConstructLoginMessage();

			Assert.That(messageConstruction, Contains.Substring($"errorMessage: '{expectedErrorMessage}'"));
			Assert.That(createAuthenticatorModel.logger.Logs, Has.Some.Contains(expectedErrorMessage));
		}

		[Test]
		public async Task FailedLoginMessageIsReturnWhenDatabaseAccessExceptionHappenedAsync()
		{
			const string expectedErrorMessage = "Database Access Failed";
			const string expectedLogMessage = "Failed to query OIDC settings from database.";

			await ArrangeAffinityFailurePolicyAsync(null, string.Empty);

			var authenticationService = CreateAuthenticationService(null, null, errorMessage: expectedErrorMessage);

			var registryAccessorMock = new Mock<IAuthenticationConfigAccessor>();
			registryAccessorMock.Setup(m => m.GetOIDCConfig())
								.Throws(new DatabaseAccessException(expectedErrorMessage));

			var createAuthenticatorModel = CreateAuthenticatorModel(registryAccessorMock.Object, authenticationService);

			var messageConstruction = createAuthenticatorModel.authenticatorModel.ConstructLoginMessage();

			Assert.That(messageConstruction, Contains.Substring($"errorMessage: '{expectedErrorMessage}'"));
			Assert.That(createAuthenticatorModel.logger.Logs, Has.Some.Contains(expectedLogMessage));
		}

		[TestCase(null, "Default")]
		[TestCase("Default", "Default")]
		[TestCase("Login", "Login")]
		[TestCase("SelectAccount", "SelectAccount")]
		[TestCase("AnyPrompt", "AnyPrompt")]
		public async Task LoginMessageIsNotNullWhenOIDCISettingsAreCorrectlySupplied(string loginPrompt, string expectedPrompt)
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockLoginService.GetHostName()}:{identityServer.Port}";

				var clientId = Guid.NewGuid().ToString();

				await ArrangeAffinityFailurePolicyAsync(authorityUrl, clientId);

				var authenticationService = CreateAuthenticationService(clientId, new Uri(authorityUrl), loginPrompt: expectedPrompt);

				var registryAccessor = ConfigWithOIDCSettingsHelper.WithRegistryAccessor(OIDCConfigSetupHelper.SetOidcConfigTest(authorityUrl, clientId));

				var createAuthenticatorModel = CreateAuthenticatorModel(registryAccessor, authenticationService, loginPrompt);

				var messageConstruction = createAuthenticatorModel.authenticatorModel.ConstructLoginMessage();

				Assert.That(messageConstruction, Is.Not.Null);
			}
		}

		[Test]
		public void FlatOIDCScopesShouldReturnEmptyArrayWhenScopesIsNullOrEmpty()
		{
			Assert.That(AuthenticatorModel.FlatOIDCScopes(null), Is.EqualTo(Array.Empty<string>()));
			Assert.That(AuthenticatorModel.FlatOIDCScopes(Enumerable.Empty<IOIDCScope>()), Is.EqualTo(Array.Empty<string>()));
		}

		[Test]
		public void FlatOIDCScopesValueShouldSameAsScopeName()
		{
			var oidcScopes = new IOIDCScope[]
			{
				new DeserializedOIDCScope { ScopeName = "openid" },
				new DeserializedOIDCScope { ScopeName = "profile" },
				new DeserializedOIDCScope { ScopeName = "offline_access" },
				new DeserializedOIDCScope { ScopeName = "test_scope" },
				new DeserializedOIDCScope { ScopeName = ZString.Empty },
				new DeserializedOIDCScope { ScopeName = null },
				null,
			};

			var flatScopes = AuthenticatorModel.FlatOIDCScopes(oidcScopes);
			Assert.That(flatScopes, Is.TypeOf(typeof(string[])));

			var flatScopesArray = flatScopes as string[];
			Assert.That(flatScopesArray.Length, Is.EqualTo(4));
			Assert.That(flatScopesArray[0], Is.EqualTo(oidcScopes[0].ScopeName.ToString()));
			Assert.That(flatScopesArray[1], Is.EqualTo(oidcScopes[1].ScopeName.ToString()));
			Assert.That(flatScopesArray[2], Is.EqualTo(oidcScopes[2].ScopeName.ToString()));
			Assert.That(flatScopesArray[3], Is.EqualTo(oidcScopes[3].ScopeName.ToString()));
		}

		[TestCase(true, ExpectedResult = "https://localhost:5001/?netcore")]
		[TestCase(false, ExpectedResult = "https://localhost:5001/")]
		public string BuildRedirectUriShouldAppendNetCoreQueryString(bool isNetCore)
		{
			var authenticationService = new Mock<IAuthenticationService>().Object;
			var registryAccessor = new Mock<IAuthenticationConfigAccessor>().Object;
			var createAuthenticatorModel = CreateAuthenticatorModel(registryAccessor, authenticationService, isNetCore: isNetCore);
			return createAuthenticatorModel.authenticatorModel.BuildRedirectUri().ToString();
		}

		[TestCase("TestPersistForm", ExpectedResult = "https://localhost:5001/?persist=TestPersistForm")]
		[TestCase(null, ExpectedResult = "https://localhost:5001/")]
		public string BuildRedirectUriShouldAppendPersistQueryString(string persist)
		{
			var authenticationService = new Mock<IAuthenticationService>().Object;
			var registryAccessor = new Mock<IAuthenticationConfigAccessor>().Object;
			var createAuthenticatorModel = CreateAuthenticatorModel(registryAccessor, authenticationService, persist: persist);
			return createAuthenticatorModel.authenticatorModel.BuildRedirectUri().ToString();
		}

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			dataProtectionProvider = new Mock<IDataProtectionProvider>();
		}

		async Task ArrangeAffinityFailurePolicyAsync(string authorityUrl, string clientId)
		{
			var context = new DefaultHttpContext();

			var registryAccessor = ConfigWithOIDCSettingsHelper.WithRegistryAccessor(OIDCConfigSetupHelper.SetOidcConfigTest(authorityUrl, clientId));

			using var factory = new CustomWebApplicationFactory<Startup>();
			factory.WithWebHostBuilder(builder =>
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
				cargoWiseOptions: cargoWiseOptions,
				appInstanceService: factory.Services.GetRequiredService<AppInstanceService>(),
				registryAccessor: registryAccessor);

			context.Request.Path = "/";
			context.Request.QueryString = new QueryString("?redirect=" + authorityUrl);

			var result = await policy.Handle(context, null, AffinityStatus.OK);

			Assert.That(result, Is.False);

			Assert.That(context.Response.Headers["Location"].ToString(), Does.Contain("/_sessionbroker/auth"));
		}

		IAuthenticationService CreateAuthenticationService(string clientId, Uri authorityUrl, string errorMessage = null, string loginPrompt = "Default")
		{
			const string azure = "Azure";
			const string redirectURL = "https://localhost:5001/";

			var mock = new Mock<IAuthenticationService>();
			mock.Setup(x => x.BuildJsonLoginMessage(clientId, azure, azure, loginPrompt, authorityUrl, new Uri(redirectURL), It.IsNotNull<string[]>()))
				.Returns(@$"{{
								""$type"": ""LoginMessage, CargoWise.Blazor.Client.Integration"",
								""OpenIdConnectSettings"":
								{{
									""$type"": ""OpenIdConnectSettings, CargoWise.Blazor.Client.Integration"",
									""ClientId"": ""{clientId}"",
									""Authority"": ""{authorityUrl}"",
									""IdentityProvider"": ""{azure}"",
									""DomainHint"": ""{azure}"",
									""LoginPrompt"": ""{loginPrompt}"",
									""AdditionalScopes"":
									{{
										""$type"": ""String[], System.Private.CoreLib"",
										""$values"": [
											""{clientId}""
										]
									}}
								}},
								""RedirectUrl"": ""{redirectURL}"",
								""ErrorMessage"": null
							}}");

			if (!string.IsNullOrEmpty(errorMessage))
			{
				mock.Setup(x => x.BuildJsonFailedLoginMessage(errorMessage))
					.Returns(@$"{{
									$type: 'LoginMessage, CargoWise.Blazor.Client.Integration',""OpenIdConnectSettings"": null,
									errorMessage: '{errorMessage}',
								}}");
			}

			return mock.Object;
		}

		AuthenticatingAffinityFailurePolicy CreateAuthenticatingAffinityFailurePolicy(
				IOptions<CargoWiseOptions> cargoWiseOptions = null,
				AppInstanceService appInstanceService = null,
				IAuthenticationConfigAccessor registryAccessor = null)
		{
			using var memoryCache = new MemoryCache(new MemoryCacheOptions());
			return new AuthenticatingAffinityFailurePolicy(
				cargoWiseOptions: cargoWiseOptions,
				appInstanceService: appInstanceService,
				affinityCookieProviderOptions: null,
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

		(AuthenticatorModel authenticatorModel, ListLogger<AuthenticatorModel> logger) CreateAuthenticatorModel(IAuthenticationConfigAccessor registryAccessor, IAuthenticationService authenticationService, string loginPrompt = null, string persist = null, bool isNetCore = false)
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			factory.WithWebHostBuilder(builder =>
				{
					builder.UseSetting("CargoWiseOptions:AppServerPathOverride", Path.Combine("CargoWise.Blazor.SessionBroker.Test.MockAppServer-bin", "CargoWise.Blazor.SessionBroker.Test.MockAppServer.exe"));
					builder.UseSetting("AdditionalArguments", "--wait 100");
					builder.ConfigureServices(services =>
					{
						services.AddSingleton(registryAccessor);
					});
				});

			var pageContext = new PageContext();
			var context = pageContext.HttpContext = new DefaultHttpContext();
			var qs = isNetCore ? new QueryString($"?{QueryParameters.NetCore}") : new QueryString();
			if (!string.IsNullOrEmpty(loginPrompt))
			{
				qs = qs.Add(QueryParameters.LoginPrompt, loginPrompt);
			}
			if (!string.IsNullOrEmpty(persist))
			{
				qs = qs.Add(QueryParameters.Persist, persist);
			}
			context.Request.QueryString = qs;

			context.Request.Scheme = "https";
			context.Request.Host = new HostString("localhost:5001");

			var logger = new ListLogger<AuthenticatorModel>();

			var authenticatorModel = new AuthenticatorModel(logger, authenticationService, registryAccessor)
			{
				PageContext = new PageContext
				{
					HttpContext = context
				}
			};

			return (authenticatorModel, logger);
		}
	}
}
