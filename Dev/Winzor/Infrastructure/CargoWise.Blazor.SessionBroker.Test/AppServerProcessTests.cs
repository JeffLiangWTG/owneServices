using System;
using System.IO;
using CargoWise.Blazor.Common;
using CargoWiseNext.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test;

[KillBlazorAppProcesses]
[KillMockAppProcesses]
public class AppServerProcessTests
{
	AppServerProcess appServerProcess;
	IConfiguration configuration;

	[SetUp]
	public void Setup()
	{
		using var applicationFactory = new CustomWebApplicationFactory<Startup>();
		configuration = applicationFactory.Services.GetRequiredService<IConfiguration>();
		appServerProcess = applicationFactory.Services.GetRequiredService<AppServerProcess>();
	}

	[Test]
	public void GetCommandLineArgsSetsUrlsToLocalhostOnly()
	{
		var commandLineArguments = appServerProcess.GetCommandLineArguments(string.Empty, string.Empty, string.Empty);

		var urls = commandLineArguments["--urls"];

		Assert.That(urls, Is.EqualTo("http://127.0.0.1:0"));
	}

	[Test]
	public void GetCommandLineArgsSetsReadStdInArgs()
	{
		var commandLineArguments = appServerProcess.GetCommandLineArguments(string.Empty, string.Empty, string.Empty);

		var stdIn = commandLineArguments["--CargoWiseOptions:ReadConfigFromStdIn"];

		Assert.That(stdIn, Is.EqualTo("True"));
	}

	[Test]
	public void GetCommandLineArgsNotSetsPersistInArgs()
	{
		var commandLineArguments = appServerProcess.GetCommandLineArguments(string.Empty, string.Empty, string.Empty);

		Assert.That(commandLineArguments.ContainsKey("--CargoWiseOptions:Persist"), Is.False);
	}

	[Test]
	public void GetCommandLineArgsSetsPersistInArgs()
	{
		const string persist = "TestPersistForm";
		var commandLineArguments = appServerProcess.GetCommandLineArguments(string.Empty, string.Empty, string.Empty, persist);

		Assert.That(commandLineArguments["--CargoWiseOptions:Persist"], Is.EqualTo(persist));
	}

	[Test]
	public void GetCommandLineArgsNotSetsHostnameInArgs()
	{
		var commandLineArguments = appServerProcess.GetCommandLineArguments(string.Empty, string.Empty, string.Empty);

		Assert.That(commandLineArguments.ContainsKey("--CargoWiseOptions:Hostname"), Is.False);
	}

	[Test]
	public void GetCommandLineArgsSetsHostnameInArgs()
	{
		const string hostname = "testhostname";
		var commandLineArguments = appServerProcess.GetCommandLineArguments(string.Empty, string.Empty, string.Empty, hostname: hostname);

		Assert.That(commandLineArguments["--CargoWiseOptions:Hostname"], Is.EqualTo(hostname));
	}

	[Test]
	public void CreateProcessAddsLocalhostOnlyUrlArgument()
	{
		AppServerProcess process = null;

		try
		{
			process = appServerProcess.CreateWithStmAccessToken(string.Empty, string.Empty, "sessionToken", BuildFileSystem.AppServerBin.PublishExePath, Guid.Empty, Guid.Empty, null);

			Assert.That(process.Process.StartInfo.Arguments, Does.Not.Contain("--urls http://*:0"));
			Assert.That(process.Process.StartInfo.Arguments, Does.Contain("--urls http://127.0.0.1:0"));
		}
		finally
		{
			process?.Process.Kill();
			process?.Process.Dispose();
		}
	}

	[Test]
	public void CreateProcessThrowsIfSessionTokenNotProvided()
	{
		Assert.Throws<ArgumentNullException>(() => appServerProcess.CreateWithStmAccessToken(string.Empty, string.Empty, null, BuildFileSystem.AppServerBin.PublishExePath, Guid.Empty, Guid.Empty, null));
	}

	[Test]
	public void SessionBrokerDoesNotPassSecureSecretToCommandLine()
	{
		var process = appServerProcess.CreateWithStmAccessToken(string.Empty, string.Empty, "sessionToken", BuildFileSystem.AppServerBin.PublishExePath, Guid.Empty, Guid.Empty, null);
		Assert.That(process.Process.StartInfo.Arguments.Contains("sessionsecret", StringComparison.OrdinalIgnoreCase), Is.False);
	}

	[Test]
	public void SessionBrokerDoesNotPassClientTokenOnCommandLine()
	{
		var process = appServerProcess.CreateWithStmAccessToken(string.Empty, "clienttoken", "sessionToken", BuildFileSystem.AppServerBin.PublishExePath, Guid.Empty, Guid.Empty, null);
		Assert.That(process.Process.StartInfo.Arguments.Contains("clienttoken", StringComparison.OrdinalIgnoreCase), Is.False);
	}

	[Test]
	[CancelAfter(5000)]
	public void ClientTokenIsWrittenToStandardIn()
	{
		AppServerProcess process = null;
		try
		{
			configuration["AdditionalArguments"] = "--parrot";
			var clientToken = Guid.NewGuid().ToString();
			process = appServerProcess.CreateWithStmAccessToken(string.Empty, clientToken, "sessionToken", Path.Combine("CargoWise.Blazor.SessionBroker.Test.MockAppServer-bin", "CargoWise.Blazor.SessionBroker.Test.MockAppServer.exe"), Guid.Empty, Guid.Empty, null);
			AssertConfigurationValue(process, $"{nameof(CargoWiseAuthOptions)}:{nameof(CargoWiseAuthOptions.ClientToken)}", clientToken);
		}
		finally
		{
			process?.Process.Dispose();
		}
	}

	[Test]
	[CancelAfter(5000)]
	public void SessionTokenIsWrittenToStandardIn()
	{
		AppServerProcess process = null;
		try
		{
			configuration["AdditionalArguments"] = "--parrot";
			var sessionToken = new SecureSecretGenerator().Generate();
			process = appServerProcess.CreateWithStmAccessToken(string.Empty, string.Empty, sessionToken, Path.Combine("CargoWise.Blazor.SessionBroker.Test.MockAppServer-bin", "CargoWise.Blazor.SessionBroker.Test.MockAppServer.exe"), Guid.Empty, Guid.Empty, null);
			AssertConfigurationValue(process, $"{nameof(CargoWiseAuthOptions)}:{nameof(CargoWiseAuthOptions.SessionToken)}", sessionToken);
		}
		finally
		{
			process?.Process.Dispose();
		}
	}

	[Test]
	public void EnvironmentVariablesWinzorUseNetCoreLibsShouldBeTrueWhenIsNetCore()
	{
		AppServerProcess process = null;
		try
		{
			var authCookie = new CargoWiseAuthCookie
			{
				AccessToken = "DummyToken",
			};
			var sessionToken = new SecureSecretGenerator().Generate();
			process = appServerProcess.CreateWithOIDCAuthCookie(string.Empty, sessionToken, Path.Combine("CargoWise.Blazor.SessionBroker.Test.MockAppServer-bin", "CargoWise.Blazor.SessionBroker.Test.MockAppServer.exe"), authCookie, Guid.Empty, Guid.Empty, null, isNetCore: true);
			Assert.That(process.Process.StartInfo.EnvironmentVariables["WINZOR_USE_NETCORE_LIBS"], Is.EqualTo("True"));
		}
		finally
		{
			process?.Process.Dispose();
		}
	}

	[Test]
	public void EnvironmentVariablesShouldNotContainWinzorUseNetCoreLibsWhenIsNotNetCore()
	{
		AppServerProcess process = null;
		try
		{
			var authCookie = new CargoWiseAuthCookie
			{
				AccessToken = "DummyToken",
			};
			var sessionToken = new SecureSecretGenerator().Generate();
			process = appServerProcess.CreateWithOIDCAuthCookie(string.Empty, sessionToken, Path.Combine("CargoWise.Blazor.SessionBroker.Test.MockAppServer-bin", "CargoWise.Blazor.SessionBroker.Test.MockAppServer.exe"), authCookie, Guid.Empty, Guid.Empty, null, isNetCore: false);
			Assert.That(!process.Process.StartInfo.EnvironmentVariables.ContainsKey("WINZOR_USE_NETCORE_LIBS"));
		}
		finally
		{
			process?.Process.Dispose();
		}
	}

	static void AssertConfigurationValue(AppServerProcess process, string configurationKey, string configurationValue)
	{
		var path = Path.Combine(SetUpTests.BlazorAppProcInfoDirectory, process.ProcessId + ".txt");
		Assert.That(
			() => process.Process.HasExited,
			Is.True.After(4000).PollEvery(10),
			"The process hasn't finished within the timeout, so we don't continue because something is wrong if it's this slow");
		Assert.That(File.Exists(path));
		using var file = File.Open(path, FileMode.Open);
		var provider = new JsonStreamConfigurationProvider(new JsonStreamConfigurationSource());
		provider.Load(file);
		provider.TryGet(configurationKey, out var value);
		Assert.That(value, Is.EqualTo(configurationValue));
	}
}
