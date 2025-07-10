using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Application;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.Testing.Common;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseNext.Infrastructure.Authentication.Test;
using Enterprise.Core.Environment;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Environment;
using Enterprise.Startup;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using OpenTelemetry;
using OpenTelemetry.Trace;
using WinzorFramework;
using WinzorTestFramework;
using WTG.OpenIDConnect.Login;
using WTG.OpenIDConnect.Token;
using static Enterprise.Winzor.Architecture.Initialization;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using NullLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger;
using ObjectFactory = CargoWise.Application.ObjectFactory;

namespace Enterprise.Winzor.Architecture.Test;

class InitializationTest
{
	[Test]
	public async Task UserNotification()
	{
		using var ctx = new EnterpriseTestContext();
		var windowService = new Mock<IWindowService>();
		Task loadRequestTask = null;
		TaskCompletionSource loadRequestTaskCreated = new TaskCompletionSource();
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestTask = Task.Run(() =>
				{
					using var ctx2 = new EnterpriseTestContext();
					var renderedNewForm = ctx2.RenderEntryPointComponent(createWindowOptions.Uri.ToString());
					renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);
					Assert.That(renderedNewForm.Instance.Form, Is.InstanceOf<ZMessageBox>());
					Assert.That(((ZMessageBox)renderedNewForm.Instance.Form).Message, Is.EqualTo("Hello World!"));
					_ = ctx2.WinzorDispatcher.InvokeAsync(renderedNewForm.Instance.Form.Dispose);
				});
				loadRequestTaskCreated.SetResult();
			});
		var clientServices = MockCargoWiseClientServices.MakeMock(windowService: windowService.Object);
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			ctx.Using(Globals.TemporaryOverrideForIsTest(false));
			ctx.Using(Globals.SetIsUnitTestingProductionFunctionality());
			var form = new Form();
			var button = new Button();
			button.Click += Button_Click;
			form.Controls.Add(button);
			return form;
		}, clientServices);
		await renderedForm.Find("button").ClickAsync(new WebMouseEventArgs());
		Assert.That(await loadRequestTaskCreated.Task.WithTimeout(TimeSpan.FromSeconds(10)), Is.True);
		Assert.That(await loadRequestTask.WithTimeout(TimeSpan.FromSeconds(10)), Is.True);

		void Button_Click(object sender, EventArgs e)
		{
			Globals.Message.Show("Hello World!");
		}
	}

	[HarmonyPatch(typeof(System.Environment), nameof(System.Environment.UserInteractive), MethodType.Getter)]
	class UserInteractivePatch
	{
		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is called via refelction in testing")]
		[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "It is called via refelction in testing")]
		static bool Prefix(ref bool __result)
		{
			__result = false;
			return false;
		}
	}

	[Test]
	public async Task UserNotificationNonInteractiveSession()
	{
		var harmony = new Harmony("com.wisetechglobal.winzor.system.environment.test.UserInteractive");
		var processor = harmony.CreateClassProcessor(typeof(UserInteractivePatch));
		try
		{
			processor.Patch();
			Assert.That(System.Environment.UserInteractive, Is.False, "The UserInteractive value is being patched for this test case to always return false");
			Assert.That(Globals.CanShowDialogs, Is.True);
			await UserNotification();
		}
		finally
		{
			harmony.UnpatchAll(harmony.Id);
		}
	}

	[Test]
	public async Task RunCargoWiseHandlesExceptionUsingErrorReporter()
	{
		TestControl control = null;
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			var form = new Form();
			control = new TestControl();
			form.Controls.Add(control);
		});
		try
		{
			await control.CallInvokeWinzorDispatcher(() => throw new InvalidOperationException("Fail"));
			Assert.That(ErrorReporter.LastExceptionReported, Is.InstanceOf<InvalidOperationException>().With.Message.EqualTo("Fail"));
			ErrorReporter.Clear();
		}
		finally
		{
			control?.Dispose();
		}
	}

	[Test]
	public async Task RunCargoWiseHandlesExceptionByShowingExceptionReporterForm()
	{
		ExceptionReporter.DisableExposed(); // reset the instance becase internal state prevents multiple exceptions being reported during a short period of time
		using var isTestOverride = Globals.TemporaryOverrideForIsTest(false);
		using var ctx = new EnterpriseTestContext();
		var windowService = new Mock<IWindowService>();
		Task loadRequestTask = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestTask = Task.Run(() =>
				{
					using var ctx2 = new EnterpriseTestContext();
					var renderedNewForm = ctx2.RenderEntryPointComponent(createWindowOptions.Uri.ToString());
					renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);
					Assert.That(renderedNewForm.Instance.Form, Is.InstanceOf<ExceptionReportingForm>());
					_ = ctx2.WinzorDispatcher.InvokeAsync(renderedNewForm.Instance.Form.Dispose);
				});
			});
		var clientServices = MockCargoWiseClientServices.MakeMock(windowService: windowService.Object);
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button();
			button.Click += (s, e) => throw new Exception("Fail!");
			form.Controls.Add(button);
			return form;
		}, clientServices);
		await rendered.Find("button").ClickAsync(new WebMouseEventArgs());
		Assert.That(await loadRequestTask.WithTimeout(TimeSpan.FromSeconds(10)), Is.True);
	}

	[Test]
	public void CargoWiseApplicationDispatcherInitialized()
	{
		var threadId = 0;
		var syncContext = default(SynchronizationContext);
		ApplicationDispatcher.Current.Send(_ =>
		{
			threadId = System.Environment.CurrentManagedThreadId;
			syncContext = SynchronizationContext.Current;
		}, null);
		Assert.That(threadId, Is.EqualTo(EnterpriseTestSetup.WinzorDispatcher.ManagedThreadId));
		Assert.That(syncContext, Is.SameAs(ApplicationDispatcher.Current));
	}

	[Test, WithSnapshotProtection]
	public async Task LogoutCalledWhenMainThreadExits()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(Env.LoginController.Logout);
		Guid[] initialHeartbeats;
		using (Db.DisposableActionForDbConnection())
		{
			initialHeartbeats = ActiveUserQuery.GetActiveUserSessions(true).Select(s => s.HeartbeatId).ToArray();
		}
		var dispatcher = new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>());
		await dispatcher.InvokeAsync(() =>
		{
			using (Db.DisposableActionForDbConnection())
			{
				Env.LoginController.LoginLocationAutomatically(Env.LoginController.LoginUserDeveloper());
			}
		});

		var newHeartbeat = ActiveUserQuery.GetActiveUserSessions(true).Select(s => s.HeartbeatId).Except(initialHeartbeats).Single();
		try
		{
			await dispatcher.InvokeAsync(() => ApplicationDispatcher.Current = SynchronizationContext.Current);
			dispatcher.Dispose();
			Assert.That(() => ActiveUserQuery.GetActiveUserSessions(true).Select(s => s.HeartbeatId), Does.Not.Contain(newHeartbeat).After(10000, 500));
		}
		finally
		{
			await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
			{
				ApplicationDispatcher.Current = SynchronizationContext.Current;
				Env.LoginController.LoginLocationAutomatically(Env.LoginController.LoginUserDeveloper());
			});
		}
	}

	[Test]
	public async Task ExitDuringStartupShouldNotOpenMoreForms()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(Initialization.UnconfigureCargoWise);
		Assert.That(Initialization.Initialized, Is.False);
		Assert.That(Initialization.MainFormInstance, Is.Null);

		using var loggerProvider = new TestLoggerProvider();
		var cargoWiseAuthStateProvider = new CargoWiseAuthStateProvider(null, It.IsAny<ITokenValidatorWrapper>());
		var mockLogin = new Mock<IWinzorCargoWiseLoginHandler>();
		var token = new TaskCompletionSource();
		mockLogin.Setup((m) => m.Login()).Callback(() =>
		{
			token.SetResult();
		});

		var serviceProvider = InitializationConfigurationHelper.ConfigTestServices(cargoWiseAuthStateProvider);

		Initialization.StartConfigureCargoWise(
				EnterpriseTestSetup.WinzorDispatcher,
				new ApplicationArguments(Array.Empty<string>()),
				mockLogin.Object,
				serviceProvider.GetRequiredService<UserMonitorRegistry>(),
				null);

		await token.Task;
		Application.Exit();

		var action = Initialization.Context.OpenForm(Initialization.MainFormInstance);

		Assert.That(action, Is.EqualTo(OpenFormAction.None));
	}

	[Test, WithSnapshotProtection]
	public async Task Unconfigure()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			Assert.That(Initialization.Initialized, Is.True);
			Initialization.UnconfigureCargoWise();
			Assert.That(Initialization.Initialized, Is.False);
			Assert.That(Initialization.MainFormInstance, Is.Null);
			Assert.That(Initialization.exceptionHandler, Is.Null);
		});

		var cargoWiseAuthStateProvider = new CargoWiseAuthStateProvider(NullLogger<CargoWiseAuthStateProvider>.Instance, It.IsAny<ITokenValidatorWrapper>());
		var serviceProvider = InitializationConfigurationHelper.ConfigTestServices(cargoWiseAuthStateProvider, null);

		Initialization.ConfigureCargoWise(
			EnterpriseTestSetup.WinzorDispatcher,
			serviceProvider.GetRequiredService<IWinzorCargoWiseLoginHandler>(),
			serviceProvider.GetRequiredService<UserMonitorRegistry>());

		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() => { Assert.That(Initialization.Initialized, Is.True); });
	}

	[Test, WithSnapshotProtection]
	public async Task ConfigureCargoWiseToLoginAsAnonymous()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(Initialization.UnconfigureCargoWise);
		Assert.That(Initialization.Initialized, Is.False);
		Assert.That(Initialization.MainFormInstance, Is.Null);

		using var loggerProvider = new TestLoggerProvider();
		var logger = CreateLogger(loggerProvider);
		var cargoWiseAuthStateProvider = new CargoWiseAuthStateProvider(logger, It.IsAny<ITokenValidatorWrapper>());

		var options = new CargoWiseAuthOptions()
		{
			AllowAnonymousDeveloperLogins = true,
			SessionToken = new SecureSecretGenerator().Generate(),
		};

		var serviceProvider = InitializationConfigurationHelper.ConfigTestServices(cargoWiseAuthStateProvider, options);

		// Act
		Initialization.StartConfigureCargoWise(
			EnterpriseTestSetup.WinzorDispatcher,
			new ApplicationArguments(Array.Empty<string>()),
			serviceProvider.GetRequiredService<IWinzorCargoWiseLoginHandler>(),
			serviceProvider.GetRequiredService<UserMonitorRegistry>(),
			null);

		// Asserts
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			Assert.That(Initialization.Initialized, Is.True);
			Assert.That(cargoWiseAuthStateProvider.AuthenticatedUser.IsOK, Is.True);
			Assert.That(cargoWiseAuthStateProvider.AuthenticatedUser.State, Is.EqualTo(LoginAuthenticationInfo.Status.OK));
			var logMessages = loggerProvider.GetLogTemplates();
			foreach (var logMessage in logMessages)
			{
				Assert.That(logMessage, Is.Not.Null.Or.Empty);
				Assert.That(logMessage, Does.Contain("{UserName}"));
			}
		});
	}

	[Test, WithSnapshotProtection]
	public async Task ConfigureCargoWiseToLoginFromStmAccessToken()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(Initialization.UnconfigureCargoWise);
		Assert.That(Initialization.Initialized, Is.False);
		Assert.That(Initialization.MainFormInstance, Is.Null);

		var optionsValue = new CargoWiseOptions
		{
			DbServerName = Db.ServerName,
			DatabaseName = Db.DatabaseName,
		};

		Db.InitializeDatabaseDetails(optionsValue.DbServerName, optionsValue.DatabaseName);
		using var loggerProvider = new TestLoggerProvider();
		var logger = CreateLogger(loggerProvider);
		var cargoWiseAuthStateProvider = new CargoWiseAuthStateProvider(logger, It.IsAny<ITokenValidatorWrapper>());

		using (Db.DisposableActionForDbConnection())
		{
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			var accessTokenInfo = new AccessTokenInfo(string.Empty, Guid.Parse("ABE1D8D8-A709-4BFA-88E3-53997AA925E2"), GlbStaffSchema.Constants.Prefix);
			var clientToken = accessControl.CreateLimitedToken("BLC", accessTokenInfo, TimeSpan.FromMinutes(5), 1);
			Assert.That(clientToken, Is.Not.Empty);
			Assert.That(accessControl.TryPeek(clientToken, "BLC", out var _));

			var options = new CargoWiseAuthOptions()
			{
				ClientToken = clientToken,
				SessionToken = new SecureSecretGenerator().Generate(),
			};

			var serviceProvider = InitializationConfigurationHelper.ConfigTestServices(cargoWiseAuthStateProvider, options);

			// Act
			Initialization.StartConfigureCargoWise(
				EnterpriseTestSetup.WinzorDispatcher,
				new ApplicationArguments(Array.Empty<string>()),
				serviceProvider.GetRequiredService<IWinzorCargoWiseLoginHandler>(),
				serviceProvider.GetRequiredService<UserMonitorRegistry>(),
				null);

			Assert.That(accessControl.TryPeek(clientToken, "BLC", out var _));

			await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
			{
				// Asserts
				Assert.That(() => Initialization.Initialized, Is.True);
				Assert.That(cargoWiseAuthStateProvider.AuthenticatedUser.IsOK, Is.True);
				Assert.That(cargoWiseAuthStateProvider.AuthenticatedUser.State, Is.EqualTo(LoginAuthenticationInfo.Status.OK));
				var logMessages = loggerProvider.GetLogTemplates();
				foreach (var logMessage in logMessages)
				{
					Assert.That(logMessage, Is.Not.Null.Or.Empty);
					Assert.That(logMessage, Does.Contain("{UserName}"));
				}
			});
		}
	}

	[Test, WithSnapshotProtection]
	public async Task ConfigureCargoWiseWillLoginFromIdentityTokenWhenProvided()
	{
		using var loggerProvider = new TestLoggerProvider();

		var (response, authority, clientId, cargoWiseAuthOptions, cargoWiseAuthStateProvider) = await InitialOidcSetup(loggerProvider);

		using var oicdConfig = ObjectFactory.Substitute(OIDCConfigSetupHelper.SetOidcConfigTest(authority, clientId));

		var serviceProvider = InitializationConfigurationHelper.ConfigTestServices(cargoWiseAuthStateProvider, cargoWiseAuthOptions);

		// Act
		Initialization.StartConfigureCargoWise(
			EnterpriseTestSetup.WinzorDispatcher,
			new ApplicationArguments(Array.Empty<string>()),
			serviceProvider.GetRequiredService<IWinzorCargoWiseLoginHandler>(),
			serviceProvider.GetRequiredService<UserMonitorRegistry>(),
			null);

		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			// Asserts
			Assert.That(() => Initialization.Initialized, Is.True);
			Assert.That(cargoWiseAuthStateProvider.AuthenticatedUser.IsOK, Is.True);
			Assert.That(cargoWiseAuthStateProvider.AuthenticatedUser.State, Is.EqualTo(LoginAuthenticationInfo.Status.OK));
			var logMessages = loggerProvider.GetLogTemplates();
			foreach (var logMessage in logMessages)
			{
				if (logMessage.Contains("logged in") || logMessage.Contains("Logged in"))
				{
					Assert.That(logMessage, Is.Not.Null.Or.Empty);
					Assert.That(logMessage, Does.Contain("{UserName}"));
				}
			}
		});
	}

	[Test, WithSnapshotProtection]
	public async Task ConfigureCargoWiseWithInactiveUserShouldFailOidcLogin()
	{
		var (response, authority, clientId, cargoWiseAuthOptions, cargoWiseAuthStateProvider) = await InitialOidcSetup();

		using (var adminConnection = Db.NewAdminConnection())
		using (Db.DisposableActionForDbConnection())
		{
			adminConnection.Command("UPDATE [dbo].[GlbStaff] SET GS_IsActive = 0, GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GETUTCDATE() WHERE GS_LoginName = 'CWSupport'").ExecuteNonQuery();
		}

		using var oicdConfig = ObjectFactory.Substitute(OIDCConfigSetupHelper.SetOidcConfigTest(authority, clientId));

		var serviceProvider = InitializationConfigurationHelper.ConfigTestServices(cargoWiseAuthStateProvider, cargoWiseAuthOptions);

		// Act
		Initialization.StartConfigureCargoWise(
			EnterpriseTestSetup.WinzorDispatcher,
			new ApplicationArguments(Array.Empty<string>()),
			serviceProvider.GetRequiredService<IWinzorCargoWiseLoginHandler>(),
			serviceProvider.GetRequiredService<UserMonitorRegistry>(),
			null);

		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			// Asserts
			Assert.That(() => Initialization.Initialized, Is.True);
			Assert.That(cargoWiseAuthStateProvider.AuthenticatedUser.IsOK, Is.False);
			Assert.That(cargoWiseAuthStateProvider.AuthenticatedUser.State, Is.EqualTo(LoginAuthenticationInfo.Status.Failure));
		});
	}

	[Test, WithSnapshotProtection]
	public async Task SessionBrokerLoginUpdatesWinzorUser()
	{
		LoginDirector.Instance.AuthenticatedUser = null;
		var (response, authority, clientId, cargoWiseAuthOptions, cargoWiseAuthStateProvider) = await InitialOidcSetup();

		using var oicdConfig = ObjectFactory.Substitute(OIDCConfigSetupHelper.SetOidcConfigTest(authority, clientId));

		var serviceProvider = InitializationConfigurationHelper.ConfigTestServices(cargoWiseAuthStateProvider, cargoWiseAuthOptions);

		// Act
		Initialization.StartConfigureCargoWise(
			EnterpriseTestSetup.WinzorDispatcher,
			new ApplicationArguments(Array.Empty<string>()),
			serviceProvider.GetRequiredService<IWinzorCargoWiseLoginHandler>(),
			serviceProvider.GetRequiredService<UserMonitorRegistry>(),
			null);

		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			Assert.That(() => Initialization.Initialized, Is.True);
			Assert.That(cargoWiseAuthStateProvider.AuthenticatedUser.IsOK, Is.True);
			Assert.That(cargoWiseAuthStateProvider.AuthenticatedUser.State, Is.EqualTo(LoginAuthenticationInfo.Status.OK));
			Assert.That(LoginDirector.Instance.AuthenticatedUser.State, Is.EqualTo(LoginAuthenticationInfo.Status.OK));
		});
	}

	async Task<(OIDCLoginResponseMessage response, string authority, string clientId, CargoWiseAuthOptions options, CargoWiseAuthStateProvider cargoWiseAuthStateProvider)> InitialOidcSetup(TestLoggerProvider loggerProvider = null)
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(Initialization.UnconfigureCargoWise);
		Assert.That(Initialization.Initialized, Is.False);
		Assert.That(Initialization.MainFormInstance, Is.Null);

		var (response, authority, clientId) = await MockLoginService.OIDCLoginAsync();

		var options = new CargoWiseAuthOptions
		{
			SessionToken = new SecureSecretGenerator().Generate(),
			IdentityToken = response.IdentityToken,
		};

		if (loggerProvider is null)
		{
#pragma warning disable CA2000 // Dispose objects before losing scope
			loggerProvider = new TestLoggerProvider();
#pragma warning restore CA2000 // Dispose objects before losing scope
		}

		var cargoWiseAuthProviderLogger = CreateLogger(loggerProvider);
		var mockTokenValidator = new Mock<ITokenValidatorWrapper>();

		mockTokenValidator.Setup(x => x.ValidateIdentityTokenAsync(It.IsAny<TokenValidatorParameters>()))
			.ReturnsAsync(TokenValidator.ReadJwtToken(response.IdentityToken, NullLogger.Instance));

		var cargoWiseAuthStateProvider = new CargoWiseAuthStateProvider(cargoWiseAuthProviderLogger, mockTokenValidator.Object);

		return (response, authority, clientId, options, cargoWiseAuthStateProvider);
	}

	[Test, WithSnapshotProtection]
	public async Task ShowSplashDialogDuringLogin()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(Initialization.UnconfigureCargoWise);
		Assert.That(Initialization.Initialized, Is.False);
		Assert.That(Initialization.MainFormInstance, Is.Null);

		var postLoginTasksProviders = new Hashtable();
		postLoginTasksProviders.Add("AU", typeof(ShowDialogPostLoginTaskProvider));
		ObjectFactory.Substitute("PostLoginTasksProviders", postLoginTasksProviders);

		var (response, authority, clientId, cargoWiseAuthOptions, cargoWiseAuthStateProvider) = await InitialOidcSetup();

		using var oicdConfig = ObjectFactory.Substitute(OIDCConfigSetupHelper.SetOidcConfigTest(authority, clientId));

		var serviceProvider = InitializationConfigurationHelper.ConfigTestServices(cargoWiseAuthStateProvider, cargoWiseAuthOptions);

		Initialization.StartConfigureCargoWise(
			EnterpriseTestSetup.WinzorDispatcher,
			new ApplicationArguments(Array.Empty<string>()),
			serviceProvider.GetRequiredService<IWinzorCargoWiseLoginHandler>(),
			serviceProvider.GetRequiredService<UserMonitorRegistry>(),
			null);

		using var ctx = new EnterpriseTestContext();
		var windowService = new Mock<IWindowService>();

		var uri = new UriBuilder(TestNavigationManager.BaseServerUri).Uri.ToString();
		var rendered = ctx.RenderEntryPointComponent(uri, windowService.Object);
		Assert.That(rendered.Instance.Form, Is.TypeOf<SplashForm>());
		Assert.That(() => ShowDialogPostLoginTask.PostLoginForm, Is.Not.Null.After(10000, 1000));
		ShowDialogPostLoginTask.CloseForm();
		StartupOpenMainFormTask.MainFormInstance?.Dispose();
	}

	[Test, WithSnapshotProtection]
	public async Task ShowDialogDuringInitializeBeforeMainFormCreated()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(Initialization.UnconfigureCargoWise);
		Assert.That(Initialization.Initialized, Is.False);
		Assert.That(Initialization.MainFormInstance, Is.Null);

		var mandatoryStateInitializers = new List<IMandatoryStateInitializer>();
		showSplashFormMandatoryStateInitializer = new ShowDialogMandatoryStateInitializer();
		mandatoryStateInitializers.Add(showSplashFormMandatoryStateInitializer);
		ObjectFactory.Substitute("MandatoryStateIntializers", mandatoryStateInitializers);

		using var ctx = new EnterpriseTestContext();
		var windowService = new Mock<IWindowService>();

		var options = new CargoWiseAuthOptions()
		{
			AllowAnonymousDeveloperLogins = true,
		};

		var cargoWiseAuthStateProvider = new CargoWiseAuthStateProvider(NullLogger<CargoWiseAuthStateProvider>.Instance, It.IsAny<ITokenValidatorWrapper>());

		var serviceProvider = InitializationConfigurationHelper.ConfigTestServices(cargoWiseAuthStateProvider, options);

		Initialization.StartConfigureCargoWise(
			EnterpriseTestSetup.WinzorDispatcher,
			new ApplicationArguments(Array.Empty<string>()),
			serviceProvider.GetRequiredService<IWinzorCargoWiseLoginHandler>(),
			serviceProvider.GetRequiredService<UserMonitorRegistry>(),
			null);

		var uri = new UriBuilder(TestNavigationManager.BaseServerUri).Uri.ToString();
		var rendered = ctx.RenderEntryPointComponent(uri, windowService.Object);

		using var ctx2 = new EnterpriseTestContext();
		var renderedNewForm = ctx2.RenderEntryPointComponent(uri, windowService.Object);
		renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);
		Assert.That(rendered.Instance.Form, Is.TypeOf<SplashForm>());
		Assert.That(() => showSplashFormMandatoryStateInitializer.mandatoryStateInitializerForm, Is.Not.Null.After(5000, 1000));
		showSplashFormMandatoryStateInitializer.CloseForm();
		rendered.WaitForElement(".splash", TimeSpan.FromSeconds(3));
		rendered.WaitForElement(".splash > .splash__top", TimeSpan.FromSeconds(3));
		rendered.WaitForElement(".splash > .splash__bottom", TimeSpan.FromSeconds(3));
		Assert.That(string.IsNullOrEmpty(rendered.Instance.Form.Text), Is.False);
		renderedNewForm.Instance.Form.Dispose();

		ExceptionReporterTestListener.Instance.Clear(); // Bunit.Extensions.WaitForHelpers.WaitForFailedException, but does not appear to be from our code
	}

	[Test, WithSnapshotProtection]
	public async Task ShowDialogDuringInitializeAfterLogin()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(Initialization.UnconfigureCargoWise);
		Assert.That(Initialization.Initialized, Is.False);
		Assert.That(Initialization.MainFormInstance, Is.Null);

		var postLoginTasksProviders = new Hashtable();
		postLoginTasksProviders.Add("AU", typeof(ShowDialogPostLoginTaskProvider));
		ObjectFactory.Substitute("PostLoginTasksProviders", postLoginTasksProviders);

		var options = new CargoWiseAuthOptions()
		{
			AllowAnonymousDeveloperLogins = true,
		};

		var cargoWiseAuthStateProvider = new CargoWiseAuthStateProvider(NullLogger<CargoWiseAuthStateProvider>.Instance, It.IsAny<ITokenValidatorWrapper>());

		var serviceProvider = InitializationConfigurationHelper.ConfigTestServices(cargoWiseAuthStateProvider, options);

		Initialization.StartConfigureCargoWise(
			EnterpriseTestSetup.WinzorDispatcher,
			new ApplicationArguments(Array.Empty<string>()),
			serviceProvider.GetRequiredService<IWinzorCargoWiseLoginHandler>(),
			serviceProvider.GetRequiredService<UserMonitorRegistry>(),
			null);

		using var ctx = new EnterpriseTestContext();
		var windowService = new Mock<IWindowService>();

		var uri = new UriBuilder(TestNavigationManager.BaseServerUri).Uri.ToString();
		var rendered = ctx.RenderEntryPointComponent(uri, windowService.Object);
		Assert.That(rendered.Instance.Form, Is.TypeOf<SplashForm>());

		Assert.That(() => ShowDialogPostLoginTask.PostLoginForm, Is.Not.Null.After(10000, 1000));
		ShowDialogPostLoginTask.CloseForm();
		StartupOpenMainFormTask.MainFormInstance?.Dispose();
	}

	[Test, WithSnapshotProtection]
	public async Task ShowDialogSplashFormCloseForm()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(Initialization.UnconfigureCargoWise);
		Assert.That(Initialization.Initialized, Is.False);
		Assert.That(Initialization.MainFormInstance, Is.Null);

		var mandatoryStateInitializers = new List<IMandatoryStateInitializer>();
		showSplashFormMandatoryStateInitializer = new ShowDialogMandatoryStateInitializer();
		mandatoryStateInitializers.Add(showSplashFormMandatoryStateInitializer);
		ObjectFactory.Substitute("MandatoryStateIntializers", mandatoryStateInitializers);

		using var ctx = new EnterpriseTestContext();
		var windowService = new Mock<IWindowService>();
		var loadRequestSent = new TaskCompletionSource();
		string loadRequestUrl = null;
		windowService.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, _, _) =>
			{
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});

		var cargoWiseAuthStateProvider = new CargoWiseAuthStateProvider(NullLogger<CargoWiseAuthStateProvider>.Instance, It.IsAny<ITokenValidatorWrapper>());

		var serviceProvider = InitializationConfigurationHelper.ConfigTestServices(cargoWiseAuthStateProvider, null);

		Initialization.StartConfigureCargoWise(
			EnterpriseTestSetup.WinzorDispatcher,
			new ApplicationArguments(Array.Empty<string>()),
			serviceProvider.GetRequiredService<IWinzorCargoWiseLoginHandler>(),
			serviceProvider.GetRequiredService<UserMonitorRegistry>(),
			null);

		var uri = new UriBuilder(TestNavigationManager.BaseServerUri).Uri.ToString();
		var rendered = ctx.RenderEntryPointComponent(uri, windowService.Object);
		await loadRequestSent.Task;

		using var ctx2 = new EnterpriseTestContext();
		var renderedNewForm = ctx2.RenderEntryPointComponent(loadRequestUrl);
		renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);
		Assert.That(rendered.Instance.Form, Is.TypeOf<SplashForm>());

		rendered.WaitForElement(".splash", TimeSpan.FromSeconds(3));
		rendered.WaitForElement(".splash > .splash__top", TimeSpan.FromSeconds(3));
		rendered.WaitForElement(".splash > .splash__bottom", TimeSpan.FromSeconds(3));

		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			var director = new DbUpgraderDirector();
			director.CloseForm(new ValidationResponse()
			{
				Successful = true
			});
			Assert.That(ZApplication.GetOpenForms().Any(f => f.GetType().Name == "SplashForm"), Is.True);

			director.CloseForm(new ValidationResponse()
			{
				Successful = false
			});
			Assert.That(ZApplication.GetOpenForms().Any(f => f.GetType().Name == "SplashForm"), Is.Not.True);
		});

		renderedNewForm.Instance.Form.Dispose();
	}

	[Test, WithSnapshotProtection]
	public async Task ShowDialogSplashFormAfterDbUpgrade([Values] bool isGoingToUpgrade, [Values] bool isUpgradeSuccessful)
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(Initialization.UnconfigureCargoWise);
		Assert.That(Initialization.Initialized, Is.False);
		Assert.That(Initialization.MainFormInstance, Is.Null);

		var mandatoryStateInitializers = new List<IMandatoryStateInitializer>();
		showSplashFormMandatoryStateInitializer = new ShowDialogMandatoryStateInitializer();
		mandatoryStateInitializers.Add(showSplashFormMandatoryStateInitializer);
		ObjectFactory.Substitute("MandatoryStateIntializers", mandatoryStateInitializers);

		using var ctx = new EnterpriseTestContext();
		var windowService = new Mock<IWindowService>();
		var loadRequestSent = new TaskCompletionSource();
		string loadRequestUrl = null;
		windowService.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, _, _) =>
			{
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});

		var cargoWiseAuthStateProvider = new CargoWiseAuthStateProvider(NullLogger<CargoWiseAuthStateProvider>.Instance, It.IsAny<ITokenValidatorWrapper>());

		var serviceProvider = InitializationConfigurationHelper.ConfigTestServices(cargoWiseAuthStateProvider, null);

		Initialization.StartConfigureCargoWise(
			EnterpriseTestSetup.WinzorDispatcher,
			new ApplicationArguments(Array.Empty<string>()),
			serviceProvider.GetRequiredService<IWinzorCargoWiseLoginHandler>(),
			serviceProvider.GetRequiredService<UserMonitorRegistry>(),
			null);

		var uri = new UriBuilder(TestNavigationManager.BaseServerUri).Uri.ToString();
		var rendered = ctx.RenderEntryPointComponent(uri, windowService.Object);
		await loadRequestSent.Task;

		using var ctx2 = new EnterpriseTestContext();
		var renderedNewForm = ctx2.RenderEntryPointComponent(loadRequestUrl);
		renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);
		Assert.That(rendered.Instance.Form, Is.TypeOf<SplashForm>());

		rendered.WaitForElement(".splash", TimeSpan.FromSeconds(3));
		rendered.WaitForElement(".splash > .splash__top", TimeSpan.FromSeconds(3));
		rendered.WaitForElement(".splash > .splash__bottom", TimeSpan.FromSeconds(3));

		var shouldKeepOpen = isGoingToUpgrade && isUpgradeSuccessful;

		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			var director = new TestDbUpgraderDirector()
			{
				IsGoingToUpgrade = isGoingToUpgrade,
				IsUpgradeSuccessful = isUpgradeSuccessful
			};
			var response = director.CheckAndUpgradeDb();

			Assert.That(response.Successful, Is.EqualTo(shouldKeepOpen));
			Assert.That(ZApplication.GetOpenForms().Any(f => f.GetType().Name == "SplashForm"), Is.EqualTo(shouldKeepOpen));
		});

		renderedNewForm.Instance.Form.Dispose();
	}

	[Test, WithSnapshotProtection]
	[SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method", Justification = "Need to continue on same thread after loadRequestSent.Task")]
	public async Task SetupDbConnectionStartupTaskIsExecuted()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(Initialization.UnconfigureCargoWise);
		Assert.That(Initialization.Initialized, Is.False);
		Assert.That(Initialization.MainFormInstance, Is.Null);

		using var ctx = new EnterpriseTestContext();
		var windowService = new Mock<IWindowService>();

		AdminConnection adminConnection = null;
		adminConnection = Db.NewAdminConnection();
		adminConnection.AcquireLockout(LockoutReason.Upgrade);
		DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);

		var pdsFactory = ProtectedDataService.GlobalServiceProvider.GetRequiredService<IProtectedDataServiceFactory>();
		var pds = pdsFactory.CreateSystemService(Db.ServerName, Db.DatabaseName);
		var pdsState = ProtectedDataService.GlobalServiceProvider.GetRequiredService<IProtectedDataStateService>();
		var contextManager = ProtectedDataService.GlobalServiceProvider.GetRequiredService<IProtectedDataAdministrationSqlExecutionContextManager>() as CargowisePDSAdministrationSqlContextManager;
		using (var scope = contextManager.BeginScope(adminConnection, adminConnection))
		{
			pdsState.ResestProtectedDataStatesToDefault(pds, Db.ServerName, Db.DatabaseName, "Reset logins");
		}

		EnterpriseTestContext ctx2;
		IRenderedComponent<EntryPointComponent> rendered;
		try
		{
			var cargoWiseAuthStateProvider = new CargoWiseAuthStateProvider(NullLogger<CargoWiseAuthStateProvider>.Instance, It.IsAny<ITokenValidatorWrapper>());

			var serviceProvider = InitializationConfigurationHelper.ConfigTestServices(cargoWiseAuthStateProvider, null);

			Initialization.StartConfigureCargoWise(
				EnterpriseTestSetup.WinzorDispatcher,
				new ApplicationArguments(Array.Empty<string>()),
				serviceProvider.GetRequiredService<IWinzorCargoWiseLoginHandler>(),
				serviceProvider.GetRequiredService<UserMonitorRegistry>(),
				null);

			var uri = new UriBuilder(TestNavigationManager.BaseServerUri).Uri.ToString();
			rendered = ctx.RenderEntryPointComponent(uri, windowService.Object);

			ctx2 = new EnterpriseTestContext();
			var renderedNewForm = ctx2.RenderEntryPointComponent(uri, windowService.Object);
			renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);
			Assert.That(rendered.Instance.Form, Is.TypeOf<SplashForm>());

			rendered.WaitForElement(".splash", TimeSpan.FromSeconds(3));
		}
		finally
		{
			adminConnection.ResetLockout();
			Form.ActiveForm?.Close();
		}
		ctx2.Dispose();
	}

	[Test]
	public async Task MonthAbbreviation()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() => { Assert.That(new ZDateTime(2023, 7, 4).ToString("dd-MMM-yyyy"), Is.EqualTo("04-Jul-2023")); });
	}

	[Test]
	public async Task TestShortDateFormatConsitentWithCW()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			Assert.That(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern, Is.EqualTo("d/MM/yyyy"));
			Assert.That(new ZDateTime(2023, 07, 04).ToString("d"), Is.EqualTo("4/07/2023"));
		});
	}

	[Test]
	public async Task TestProgressValueWhenExecuteStartupTask()
	{
		var mockApplicationTask = new Mock<IApplicationStartupTask>();
		var arguments = new ApplicationArguments(Array.Empty<string>());
		mockApplicationTask.Setup(t => t.TaskDescription).Returns("Mock task for test");
		mockApplicationTask.Setup(t => t.ShouldExecute(arguments)).Returns(true);
		mockApplicationTask.Setup(t => t.Execute(arguments)).Returns(true);

		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() => SplashFormInstance);
		var filler = rendered.Find(".splash > .splash__top > .splash__progressbar > .progressbar__filler");
		Assert.That(filler, Is.Not.Null);

		for (int i = 0; i < 10; i++)
		{
			await SplashFormInstance.InvokeWinzorDispatcherAsync(() => ExecuteStartupTask(arguments, mockApplicationTask.Object, (i + 1) * 10));
			Assert.That(filler.GetAttribute("style"), Does.Contain($"width: {i + 1}0%"));
			Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.Null);
			Assert.That(ErrorReporter.LastExceptionReported, Is.Null);
			Assert.That(MainFormInstanceInitializedTask.IsFaulted, Is.False);
		}
	}

	[Test]
	public async Task ExecuteStartupTaskDoNothingWhenTaskShouldNotExecute()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(UnconfigureCargoWise);

		Mock<IApplicationStartupTask> mockApplicationTask = new Mock<IApplicationStartupTask>();
		var arguments = new ApplicationArguments(Array.Empty<string>());
		mockApplicationTask.Setup(t => t.ShouldExecute(arguments)).Returns(false);
		mockApplicationTask.Setup(t => t.Execute(arguments)).Returns(false);

		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() => ExecuteStartupTask(arguments, mockApplicationTask.Object));

		Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.Null);
		Assert.That(ErrorReporter.LastExceptionReported, Is.Null);
		Assert.That(MainFormInstanceInitializedTask.IsFaulted, Is.False);
	}

	[Test]
	public async Task ExecuteStartupTaskShouldThrowTaskCancelExceptionWhenTaskRunFailure()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(UnconfigureCargoWise);

		Mock<IApplicationStartupTask> mockApplicationTask = new Mock<IApplicationStartupTask>();
		var arguments = new ApplicationArguments(Array.Empty<string>());
		mockApplicationTask.Setup(t => t.ShouldExecute(arguments)).Returns(true);
		mockApplicationTask.Setup(t => t.TaskDescription).Returns("Mock task for test");
		mockApplicationTask.Setup(t => t.Execute(arguments)).Returns(false);

		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			Assert.Throws<TaskCanceledException>(() => ExecuteStartupTask(arguments, mockApplicationTask.Object));
		});
		Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.EqualTo($"{Core.Constants.ProductName} failed to start."));
		Assert.That(ErrorReporter.LastExceptionReported, Is.Null);
		Assert.That(MainFormInstanceInitializedTask.IsFaulted, Is.True);
		var exception = MainFormInstanceInitializedTask.Exception;
		Assert.That(exception, Is.InstanceOf<AggregateException>());
		Assert.That(exception.InnerException, Is.InstanceOf<TaskCanceledException>().With.Message.EqualTo("Failed: Mock task for test"));

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
	}

	[Test]
	public async Task ExecuteStartupTaskShouldDirectlyThrowTaskCanceledExceptionThrownByExecuteTask()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(UnconfigureCargoWise);

		Mock<IApplicationStartupTask> mockApplicationTask = new Mock<IApplicationStartupTask>();
		var arguments = new ApplicationArguments(Array.Empty<string>());
		mockApplicationTask.Setup(t => t.ShouldExecute(arguments)).Returns(true);
		mockApplicationTask.Setup(t => t.TaskDescription).Returns("Mock task for test");
		mockApplicationTask.Setup(t => t.Execute(arguments)).Throws(new TaskCanceledException("Test Exception."));

		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			Assert.Throws<TaskCanceledException>(() => ExecuteStartupTask(arguments, mockApplicationTask.Object));
		});
		Assert.That(ErrorReporter.LastExceptionReported, Is.Null);
		Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.Null);
	}

	[Test]
	public async Task ExecuteStartupTaskShouldLogException()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(UnconfigureCargoWise);

		var logger = new Mock<ILogger>();
		var mockApplicationTask = new Mock<IApplicationStartupTask>();
		var arguments = new ApplicationArguments(Array.Empty<string>());
		mockApplicationTask.Setup(t => t.ShouldExecute(arguments)).Returns(true);
		mockApplicationTask.Setup(t => t.TaskDescription).Returns("Mock task for test");
		mockApplicationTask.Setup(t => t.Execute(arguments)).Throws(new MissingMethodException("Test Exception."));

		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			Assert.Throws<TaskCanceledException>(() => ExecuteStartupTask(arguments, mockApplicationTask.Object, 100, null, logger.Object));
		});

		logger.Verify(l => l.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString() == "Exception is thrown in ExecuteStartupTask. Start to handle it."), It.IsAny<MissingMethodException>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(1));
		Assert.That(ErrorReporter.LastExceptionReported.Message, Is.EqualTo("Test Exception."));
		Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.EqualTo("CargoWise failed to start."));

		ErrorReporter.Clear();
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
	}

	[Test]
	public async Task ExecuteStartupTaskShouldThrowTaskCancelExceptionWhenTaskThrowExceptionAndExceptionNotBeHandled()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			UnconfigureCargoWise();
			Assert.That(Initialized, Is.False);
			Assert.That(MainFormInstance, Is.Null);
			Assert.That(exceptionHandler, Is.Null);
		});

		var arguments = new ApplicationArguments(Array.Empty<string>());
		Mock<IApplicationStartupTask> mockApplicationTask = new Mock<IApplicationStartupTask>();
		mockApplicationTask.Setup(t => t.ShouldExecute(arguments)).Returns(true);
		mockApplicationTask.Setup(t => t.TaskDescription).Returns("Mock task for test");
		mockApplicationTask.Setup(t => t.Execute(arguments)).Throws(new Exception("Test Exception."));

		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			Assert.That(() => ExecuteStartupTask(arguments, mockApplicationTask.Object), Throws.TypeOf<TaskCanceledException>().With.Message.EqualTo("Failed: Mock task for test"));
		});
		Assert.That(ErrorReporter.LastExceptionReported, Is.InstanceOf<Exception>().With.Message.EqualTo("Test Exception."));
		Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.EqualTo($"{Core.Constants.ProductName} failed to start."));
		Assert.That(MainFormInstanceInitializedTask.IsFaulted, Is.True);
		var exception = MainFormInstanceInitializedTask.Exception;
		Assert.That(exception, Is.InstanceOf<AggregateException>());
		Assert.That(exception.InnerException, Is.InstanceOf<TaskCanceledException>().With.Message.EqualTo("Failed: Mock task for test"));
		Assert.That(exception.InnerException.InnerException, Is.InstanceOf<Exception>().With.Message.EqualTo("Test Exception."));

		ErrorReporter.Clear();
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			UnconfigureCargoWise();
			Mock<IApplicationStartupTaskExceptionHandler> mockExceptionHandle = new Mock<IApplicationStartupTaskExceptionHandler>();
			mockExceptionHandle.Setup(eh => eh.HandleException(It.IsAny<Exception>(), arguments)).Returns(false);
			exceptionHandler = mockExceptionHandle.Object;
			Assert.That(exceptionHandler, Is.Not.Null);
		});

		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			Assert.That(() => ExecuteStartupTask(arguments, mockApplicationTask.Object), Throws.TypeOf<TaskCanceledException>().With.Message.EqualTo("Failed: Mock task for test"));
		});
		Assert.That(ErrorReporter.LastExceptionReported, Is.InstanceOf<Exception>().With.Message.EqualTo("Test Exception."));
		Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.EqualTo($"{Core.Constants.ProductName} failed to start."));
		Assert.That(MainFormInstanceInitializedTask.IsFaulted, Is.True);
		exception = MainFormInstanceInitializedTask.Exception;
		Assert.That(exception, Is.InstanceOf<AggregateException>());
		Assert.That(exception.InnerException, Is.InstanceOf<TaskCanceledException>().With.Message.EqualTo("Failed: Mock task for test"));
		Assert.That(exception.InnerException.InnerException, Is.InstanceOf<Exception>().With.Message.EqualTo("Test Exception."));

		GC.Collect();
		GC.WaitForPendingFinalizers();
		ErrorReporter.Clear();
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
	}

	[Test]
	public async Task ExecuteStartupTaskShouldNotThrowExceptionWhenExceptionBeHandled()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(UnconfigureCargoWise);

		var arguments = new ApplicationArguments(Array.Empty<string>());
		Mock<IApplicationStartupTaskExceptionHandler> mockExceptionHandle = new Mock<IApplicationStartupTaskExceptionHandler>();
		mockExceptionHandle.Setup(eh => eh.HandleException(It.IsAny<Exception>(), arguments)).Returns(true);

		Mock<IApplicationStartupTask> mockApplicationTask = new Mock<IApplicationStartupTask>();
		mockApplicationTask.Setup(t => t.ShouldExecute(arguments)).Returns(true);
		mockApplicationTask.Setup(t => t.TaskDescription).Returns("Mock task for test");
		mockApplicationTask.Setup(t => t.Execute(arguments)).Throws(new Exception());

		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			exceptionHandler = mockExceptionHandle.Object;
			Assert.DoesNotThrow(() => ExecuteStartupTask(arguments, mockApplicationTask.Object));
		});
		Assert.That(ErrorReporter.LastExceptionReported, Is.Null);
		Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.Null);
		Assert.That(MainFormInstanceInitializedTask.IsFaulted, Is.False);
	}

	[Test]
	public void TestEnterStartupErrorHandlerShouldHandleDatabaseUpgradeException()
	{
		var actualDbEnv = DbEnv.Instance;
		try
		{
			var mockGuidPlugin = new Mock<IDbConnectionGuiPlugin>();
			var mockDbEnv = new Mock<BaseDbEnvironment>();
			mockDbEnv.Setup(m => m.ConnectionGuiPlugin).Returns(mockGuidPlugin.Object);
			mockGuidPlugin.Setup(m => m.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>()));
			DbEnv.SetDbEnvironment(mockDbEnv.Object);

			EnterStartupErrorHandler enterStartupErrorHandler = new EnterStartupErrorHandler();
			var arguments = new ApplicationArguments(Array.Empty<string>());
			var dbUpgradeException = new DatabaseUpgradedException();
			var handled = enterStartupErrorHandler.HandleException(dbUpgradeException, arguments);

			Assert.That(handled, Is.True);
			Assert.That(ErrorReporter.LastExceptionReported, Is.Null);
			Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.Null);
			mockGuidPlugin.Verify(m => m.HandleDatabaseUpgradeException(dbUpgradeException), Times.Once);
			mockDbEnv.VerifyAll();
			mockGuidPlugin.VerifyAll();
		}
		finally
		{
			DbEnv.SetDbEnvironment(actualDbEnv);
		}
	}

	[Test, WithSnapshotProtection]
	public void TestEnterStartupErrorHandlerShouldHandleSqlException()
	{
		EnterStartupErrorHandler enterStartupErrorHandler = new EnterStartupErrorHandler();
		var arguments = new ApplicationArguments(Array.Empty<string>());

		using (Db.DisposableActionForDbConnection())
		{
			var moduleBeingExecutedIsNotTrustedException = AdoTestUtils.GetSqlException(15562, "ModuleBeingExecutedIsNotTrusted", Db.Connection);
			var cannotExecuteAsDatabasePrincipalException = AdoTestUtils.GetSqlException(15517, "CannotExecuteAsDatabasePrincipalException", Db.Connection);
			var otherSqlException = AdoTestUtils.GetSqlException(0000, "Other SqlException", Db.Connection);

			var handled1 = enterStartupErrorHandler.HandleException(moduleBeingExecutedIsNotTrustedException, arguments);
			Assert.That(handled1, Is.True);
			Assert.That(ErrorReporter.LastExceptionReported, Is.Null);
			Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.Null);

			var handled2 = enterStartupErrorHandler.HandleException(cannotExecuteAsDatabasePrincipalException, arguments);
			Assert.That(handled2, Is.True);
			Assert.That(ErrorReporter.LastMessageReported, Is.EqualTo($"AlterDbAuthorisation on db: '{Db.DatabaseName}' failed with error: CannotExecuteAsDatabasePrincipalException.(switched to 'sa')."));

			ErrorReporter.Clear();

			var handled3 = enterStartupErrorHandler.HandleException(otherSqlException, arguments);
			Assert.That(handled3, Is.False);
			Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.Null);
			Assert.That(ErrorReporter.LastExceptionReported, Is.Null);
		}
	}

	[Test]
	public async Task TestInitializationTraces()
	{
		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
			.AddSource("*")
			.AddInMemoryExporter(traces)
			.Build();
		using var activitySource = new ActivitySource("Test");
		await OnInitialRenderAsync(null, activitySource);

		var arguments = new ApplicationArguments(Array.Empty<string>());
		var task = Mock.Of<IApplicationStartupTask>();
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			ExecuteStartupTask(arguments, task, 100, activitySource);
		});

		Assert.Multiple(() =>
		{
			Assert.That(traces.Any(t => t.OperationName == "Initialization.OnInitialRenderAsync"));
			Assert.That(traces.Any(t => t.OperationName == $"ExecuteStartupTask({task.GetType().Name})"));
		});
		Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.Null);
		Assert.That(ErrorReporter.LastExceptionReported, Is.Null);
		Assert.That(MainFormInstanceInitializedTask.IsFaulted, Is.False);
	}

	[Test]
	public void CommandLineArgumentsShouldBeSet()
	{
		var cargoWiseAuthStateProvider = new CargoWiseAuthStateProvider(NullLogger<CargoWiseAuthStateProvider>.Instance, It.IsAny<ITokenValidatorWrapper>());
		var serviceProvider = InitializationConfigurationHelper.ConfigTestServices(cargoWiseAuthStateProvider, null);

		Initialization.ConfigureCargoWise(
			EnterpriseTestSetup.WinzorDispatcher,
			serviceProvider.GetRequiredService<IWinzorCargoWiseLoginHandler>(),
			serviceProvider.GetRequiredService<UserMonitorRegistry>());

		Assert.That(CommandLineArguments.UsedToLaunchApplication, Is.Not.Null);
	}

	class ShowDialogPostLoginTaskProvider : PostLoginTasksProvider
	{
		public override IReadOnlyList<IPostLoginTask> GetPostLoginTasks()
		{
			return
			[
				new ShowDialogPostLoginTask()
			];
		}
	}

	class ShowDialogPostLoginTask : IPostLoginTask
	{
		public static Form PostLoginForm;

		public bool ShouldExecute() => true;

		public string TaskDescription => "Show Dialog";

		public void Execute()
		{
			PostLoginForm = new PostLoginForm();
			PostLoginForm.ShowDialog();
		}

		public static void CloseForm()
		{
			PostLoginForm?.Close();
			PostLoginForm?.Dispose();
			PostLoginForm = null;
		}
	}

	class PostLoginForm : Form
	{
	}

	class ShowDialogMandatoryStateInitializer : IMandatoryStateInitializer
	{
		public Form mandatoryStateInitializerForm;
		public void Initialize()
		{
			mandatoryStateInitializerForm = new MandatoryStateInitializerForm();
			mandatoryStateInitializerForm.ShowDialog();
		}

		public void CloseForm()
		{
			mandatoryStateInitializerForm?.Close();
			mandatoryStateInitializerForm?.Dispose();
			mandatoryStateInitializerForm = null;
		}
	}

	ShowDialogMandatoryStateInitializer showSplashFormMandatoryStateInitializer;

	[TearDown]
	protected void TearDown()
	{
		if (showSplashFormMandatoryStateInitializer != null)
		{
			showSplashFormMandatoryStateInitializer.CloseForm();
			showSplashFormMandatoryStateInitializer = null;
		}

		if (ShowDialogPostLoginTask.PostLoginForm != null)
		{
			ShowDialogPostLoginTask.CloseForm();
		}
	}

	class MandatoryStateInitializerForm : Form
	{
	}

	class TestControl : Control
	{
		public Task CallInvokeWinzorDispatcher(Action action) => InvokeWinzorDispatcherAsync(action);
	}

	class TestDbUpgraderDirector : DbUpgraderDirector
	{
		public bool IsGoingToUpgrade;
		public bool IsUpgradeSuccessful;

		public override bool IsDbUpgradeRequired()
		{
			return true;
		}

		protected override bool LoginForUpgrade()
		{
			return IsGoingToUpgrade;
		}

		protected override ValidationResponse DoUpgrade()
		{
			return new ValidationResponse()
			{
				Successful = IsUpgradeSuccessful
			};
		}
	}

	static ILogger<ICargoWiseAuthStateProvider> CreateLogger(TestLoggerProvider provider)
	{
		var serviceProvider = new ServiceCollection().AddLogging(builder => builder.AddProvider(provider)).BuildServiceProvider();
		var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
		return loggerFactory.CreateLogger<ICargoWiseAuthStateProvider>();
	}

	class TestLoggerProvider : ILoggerProvider
	{
		readonly List<string> logTemplates = new List<string>();

		public ILogger CreateLogger(string categoryName)
		{
			return new TestLogger(logTemplates);
		}

		public void Dispose()
		{
		}

		public IReadOnlyList<string> GetLogTemplates()
		{
			return logTemplates.AsReadOnly();
		}
	}

	class TestLogger : ILogger
	{
		readonly List<string> logTemplates;

		public TestLogger(List<string> logTemplates)
		{
			this.logTemplates = logTemplates;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return null;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (state is IReadOnlyList<KeyValuePair<string, object>> stateList)
			{
				var logTemplate = stateList.FirstOrDefault(kv => kv.Key == "{OriginalFormat}").Value as string;
				logTemplates.Add(logTemplate);
			}
		}
	}
}
