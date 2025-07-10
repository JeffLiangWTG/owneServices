using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;
using WinzorTestFramework.TestServer;

namespace WinzorTestFramework;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
sealed class InMemoryTestServerContext : IAsyncDisposable
{
	internal InMemoryTestServerContext() : this(new MockCargoWiseClientSeviceProvider())
	{
	}

	internal InMemoryTestServerContext(MockCargoWiseClientSeviceProvider clientServiceProvider)
	{
		Init(clientServiceProvider);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "<Pending>")]
	void Init(MockCargoWiseClientSeviceProvider cargoWiseClientServiceProvider)
	{
		const int MaxHubMessageBufferSize = 1024 * 96; // max signalR message or buffer size
		serverBaseUrl = "http://localhost:5000";
		var options = new WebApplicationOptions() { ContentRootPath = @"WinzorTestFramework", EnvironmentName = "Development" };
		var builder = WebApplication.CreateBuilder(options);
		builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider serviceProvider, LoggerConfiguration configuration) =>
		{
			configuration
			.MinimumLevel.Information()
			.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {MachineName} {Version} {AppName} {VersionBrokerProcessCorrelationId} {SessionBrokerProcessCorrelationId} {AppServerProcessCorrelationId} {Message:lj} {NewLine}{Exception}");
		}, writeToProviders: true);

		builder.Services
			.AddSingleton<WinzorDispatcher>()
			.AddSingleton<IFormInstanceRegister, RegisteredFormInstances>()
			.AddSingleton<IFormOpener, FormOpener>();
		builder.Services.AddRazorComponents()
			.AddInteractiveServerComponents();
		builder.Services.AddSignalR(e => e.MaximumReceiveMessageSize = MaxHubMessageBufferSize);

		builder.Services.AddServerSideBlazor(options =>
		{
			options.DetailedErrors = true;
		});
		builder.Services.AddScoped<IJSRuntimeWithMonitor, JSRuntimeWithMonitor>();
		cargoWiseClientServiceProvider.AddCargoWiseClient(builder.Services);
		builder.Services.AddSingleton<IFileVersionHash, FileVersionHash>();
		builder.Services.Decorate<IWindowService, WinzorWindowService>();
		builder.Services.AddSingleton<IDownloadObjectManager, DownloadObjectManager>();
		builder.Services.AddTransient<IFileService, FileService>();
		builder.Services.AddTransient<IClientEventService, ClientEventService>();
		builder.Services.AddSingleton<IImageCacher, ImageCacher>();
		builder.Services.AddJSInteropServices();
		builder.Services.Configure<KestrelServerOptions>(options =>
		{
			options.AllowSynchronousIO = true;
		});
		Services = builder.Services;
		builder.WebHost.UseUrls(serverBaseUrl);
		host = builder.Build();
		host.UseStaticFiles();
		host.UseAntiforgery();
		host.UseTestEndpoints();

		host.MapRazorComponents<Home>()
			.AddInteractiveServerRenderMode();
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
			await host.DisposeAsync();
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

	WebApplication host;

	public WinzorDispatcher WinzorDispatcher
	{
		get => host.Services.GetRequiredService<WinzorDispatcher>();
	}

	internal async Task WaitForHostShutdownAsync() => await host.WaitForShutdownAsync();

	public IServiceProvider HostServices => host.Services;

	internal IServiceCollection Services { get; private set; }
}
