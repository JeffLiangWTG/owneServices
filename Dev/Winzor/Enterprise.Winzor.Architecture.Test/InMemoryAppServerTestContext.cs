using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.Testing.Common;
using CargoWise.DataProtection;
using CargoWise.Winzor.AppServer;
using CargoWise.Winzor.Telemetry;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using WinzorFramework;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;

public sealed class InMemoryAppServerTestContext : IAsyncDisposable
{
	public InMemoryAppServerTestContext(bool needCargoWiseRuntime = false, bool keepCircuitHandler = false, bool useHttps = false)
		: this(new MockCargoWiseClientSeviceProvider(), needCargoWiseRuntime: needCargoWiseRuntime, keepCircuitHandler: keepCircuitHandler, useHttps: useHttps)
	{
	}

	public InMemoryAppServerTestContext(ICargoWiseClientServiceProvider cargoWiseClientServiceProvider, string environmentType, CargoWiseAuthOptions cargoWiseAuthOptions = null)
	{
		this.environmentType = environmentType;
		Init(cargoWiseClientServiceProvider, cargoWiseAuthOptions: cargoWiseAuthOptions, useHttps: environmentType == Environments.Production);
	}

	public InMemoryAppServerTestContext(ICargoWiseClientServiceProvider cargoWiseClientServiceProvider, bool needCargoWiseRuntime = false, bool useHttps = false, TelemetryOptions telemetryOptions = null, bool keepCircuitHandler = false)
	{
		Init(cargoWiseClientServiceProvider, needCargoWiseRuntime: needCargoWiseRuntime, useHttps: useHttps, telemetryOptions: telemetryOptions, keepCircuitHandler: keepCircuitHandler);
	}

	void Init(ICargoWiseClientServiceProvider cargoWiseClientServiceProvider, bool needCargoWiseRuntime = false, CargoWiseAuthOptions cargoWiseAuthOptions = null, bool useHttps = false, TelemetryOptions telemetryOptions = null, bool keepCircuitHandler = false)
	{
		serverBaseUrl = $"{(useHttps ? "https" : "http")}://localhost:5000";
		if (useHttps)
		{
			var process = Process.Start(new ProcessStartInfo("dotnet", " dev-certs https"));
			process.WaitForExit();
		}

		var hostBuilder = Program.CreateHostBuilder(Array.Empty<string>(), WinzorDispatcher).UseEnvironment(environmentType ?? (DatIsTesting ? "DAT" : "Development"));
		if (cargoWiseAuthOptions is not null)
		{
			hostBuilder.ConfigureAppConfiguration(configBuilder =>
			{
				configBuilder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { CargoWiseAuthOptions = cargoWiseAuthOptions }))));
			});
		}
		if (telemetryOptions is not null)
		{
			hostBuilder.ConfigureAppConfiguration(configBuilder =>
			{
				configBuilder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { TelemetryOptions = telemetryOptions }))));
			});
		}
		hostBuilder.UseContentRoot(ContentRootOverride);
		hostBuilder.ConfigureServices(services =>
		{
			cargoWiseClientServiceProvider.AddCargoWiseClient(services);
			if (!keepCircuitHandler)
			{
				services.Remove(services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(CircuitHandler)));
			}
			services.AddSingleton(UrlHandlerProviderMock.GetObject());
			services.ConfigureProtectedDataFactoryServices();
			services.ConfigureProtectedDataSqlExtensions(ApplicationType.Web);
			services.AddSingleton(Options.Create(new EntryPointPoolOptions { Enabled = false }));
			services.AddSingleton(_ => WinzorDispatcher.FormInstanceRegister);
			Services = services;
		});
		// we don't want tests writing to disk or Kafka so this effectively suppresses logging
		hostBuilder.UseSerilog((_, __) => { });
		hostBuilder.ConfigureWebHost(options => options.UseUrls(serverBaseUrl));
		host = hostBuilder.Build();

		if (needCargoWiseRuntime)
		{
			Initialization.ConfigureCargoWise(
				HostServices.GetRequiredService<WinzorDispatcher>(),
				HostServices.GetRequiredService<IWinzorCargoWiseLoginHandler>(),
				HostServices.GetRequiredService<UserMonitorRegistry>());
		}

		host.Start();
	}

	public string ServerBaseUrl => serverBaseUrl;
	string serverBaseUrl;

	public async ValueTask DisposeAsync()
	{
		await WinzorDispatcher.InvokeAsync(() =>
		{
			foreach (var disposable in disposables)
			{
				disposable.Dispose();
			}
		});

		foreach (var action in asyncDisposeActions)
		{
			await action();
		}

		try
		{
			await host.StopAsync();
		}
		finally
		{
			host.Dispose();
		}
	}

	public T Using<T>(T disposable) where T : IDisposable
	{
		disposables.Add(disposable);
		return disposable;
	}

	readonly List<IDisposable> disposables = new();

	public void RegisterDisposeAction(Action action) => RegisterDisposeAction(() =>
	{
		action();
		return Task.CompletedTask;
	});

	public void RegisterDisposeAction(Func<Task> action) => asyncDisposeActions.Add(action);

	readonly List<Func<Task>> asyncDisposeActions = new();

	IHost host;

	public WinzorDispatcher WinzorDispatcher => EnterpriseTestSetup.WinzorDispatcher;

	const string ContentRootOverride = @"..\AppServer";

	static bool DatIsTesting => bool.TryParse(System.Environment.GetEnvironmentVariable("DAT_IS_TESTING"), out var datIsTesting) && datIsTesting;

	readonly string environmentType;

	internal async Task WaitForHostShutdownAsync() => await host.WaitForShutdownAsync();

	public IServiceProvider HostServices => host.Services;

	internal IServiceCollection Services { get; private set; }
}
