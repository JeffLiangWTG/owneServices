using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.DependencyInjection;
using CargoWise.Blazor.Client.Integration.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace WinzorFramework.Samples;

public class ApplicationServer
{
	const int MaxHubMessageBufferSize = 1024 * 96; // max signalR message or buffer size

	public void Run<TComponent>(Func<Form> constructor)
		where TComponent : ComponentBase
	{
		var builder = WebApplication.CreateBuilder();
		var environment = builder.Environment;
		builder.Services.Configure<ForwardedHeadersOptions>(options =>
		{
			// The ASP.NET Core defaults only allow 1 proxy server on the localhost.
			// Our solution requires 3 proxy servers including the HAProxy on a different host.
			// If publicly hosted (e.g. WiseCloud), this service must be hosted behind a
			// trusted frontend proxy which strips spoofed forwarded headers.
			// https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer
			// These headers are important so that NavigationManager can calculate the app's absolute URL.
			options.ForwardLimit = null;
			options.KnownNetworks.Add(new IPNetwork(IPAddress.Any, 0));
			options.ForwardedHeaders = ForwardedHeaders.All;
		});
		builder.Services
			.AddSingleton<WinzorDispatcher>()
			.AddSingleton<IFormInstanceRegister, RegisteredFormInstances>()
			.AddSingleton<IFormOpener, FormOpener>();
		builder.Services.AddCargoWiseClient();
		builder.Services.AddControllers();
		builder.Services.AddRazorComponents()
			.AddInteractiveServerComponents();

		builder.Services.AddSignalR(e => e.MaximumReceiveMessageSize = MaxHubMessageBufferSize);
		builder.Services.AddServerSideBlazor().AddCircuitOptions(o =>
		{
			if (environment.IsDevelopment())
			{
				o.DetailedErrors = true;
			}
		});

		builder.Services.AddScoped<IJSRuntimeWithMonitor, JSRuntimeWithMonitor>();
		builder.Services.AddScoped<HttpClient>();
		builder.Services.AddSingleton<IFileVersionHash, FileVersionHash>();
		builder.Services.AddHealthChecks();
		builder.Services.Decorate<IWindowService, WinzorWindowService>();
		builder.Services.AddSingleton<IDownloadObjectManager, DownloadObjectManager>();
		builder.Services.AddTransient<IFileService, FileService>();
		builder.Services.AddTransient<IClientEventService, ClientEventService>();
		builder.Services.AddJSInteropServices();

		builder.Services.Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; });
		host = builder.Build();

		host.UseForwardedHeaders();

		if (environment.IsDevelopment())
		{
			host.UseDeveloperExceptionPage();
		}
		else
		{
			host.UseExceptionHandler("/Error");
		}

		host.UseStaticFiles();
		host.UseRouting();
		host.UseAntiforgery();
		host.MapControllers();
		host.MapRazorComponents<TComponent>().AddInteractiveServerRenderMode();
		var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
		var winzorDispatcher = host.Services.GetRequiredService<WinzorDispatcher>();
		mainFormInstanceInitializedTaskCompletionSource = new TaskCompletionSource();
		lifetime.ApplicationStarted.Register(() =>
		{
			_ = winzorDispatcher.InvokeAsync(() =>
			{
				MainFormInstance = constructor();
				mainFormInstanceInitializedTaskCompletionSource.SetResult();
			});
		});

		host.Run();
	}

	public static Form MainFormInstance { get; set; }
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "<Pending>")]
	static TaskCompletionSource mainFormInstanceInitializedTaskCompletionSource = new TaskCompletionSource();
	public static Task MainFormInstanceInitializedTask => mainFormInstanceInitializedTaskCompletionSource.Task;

	WebApplication host;

	public IServiceProvider HostServices => host?.Services;
}
