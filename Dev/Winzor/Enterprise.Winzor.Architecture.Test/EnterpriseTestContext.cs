using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Async;
using CargoWise.Blazor.Client.Integration.Files;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.Client.Integration.OIDCVerify;
using CargoWise.Blazor.Common;
using CargoWise.Winzor.Telemetry;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;

public class EnterpriseTestContext : WinzorTestContext
{
	public EnterpriseTestContext() : base(EnterpriseTestSetup.WinzorDispatcher)
	{
		Services.AddSingleton(Mock.Of<IMenuDisplayer>());
		Services.AddSingleton(Mock.Of<IWinzorTelemetry>());
	}

	public IRenderedComponent<EntryPointComponent> RenderEntryPointComponent(string url)
	{
		return RenderEntryPointComponent(url, Mock.Of<ILifecycleService>(), DefaultClientServices.WindowService);
	}

	public IRenderedComponent<EntryPointComponent> RenderEntryPointComponent(string url, ILifecycleService lifecycleService)
	{
		return RenderEntryPointComponent(url, lifecycleService, DefaultClientServices.WindowService);
	}

	public IRenderedComponent<EntryPointComponent> RenderEntryPointComponent(string url, IWindowService windowService, int timeOut = 30)
	{
		timeSpan = TimeSpan.FromSeconds(timeOut);
		return RenderEntryPointComponent(url, Mock.Of<ILifecycleService>(), windowService);
	}

	public IRenderedComponent<EntryPointComponent> RenderEntryPointComponent(string url, ILifecycleService lifecycleService, IWindowService windowService)
	{
		var component = RenderEntryPointComponent<EntryPointComponent>(url, lifecycleService, windowService);
		return component;
	}

	public IRenderedComponent<EntryPointComponent> RenderEntryPointComponent(string url, IWindowService windowService, ILogger<EntryPointComponent> logger)
	{
		Services.AddSingleton(logger);
		return RenderEntryPointComponent(url, windowService);
	}

	public IRenderedComponent<T> RenderEntryPointComponent<T>(string url, ILifecycleService lifecycleService, IWindowService windowService) where T : EntryPointComponent
	{
		Services.AddSingleton<NavigationManager>(new TestNavigationManager(url));
		Services.AddSingleton(WinzorDispatcher);
		Services.AddSingleton(lifecycleService);
		Services.AddSingleton(windowService);
		Services.Decorate<IWindowService, WinzorWindowService>();
		var channel = new OpeningFormQueue();
		Services.TryAddSingleton<IOpeningFormQueue>(channel);
		Services.AddSingleton(Mock.Of<IFileService>());
		Services.AddSingleton(Mock.Of<IClientEventService>());
		Services.AddSingleton(Mock.Of<IPingService>());
		Services.AddSingleton(Mock.Of<IClientFileApi>());
		Services.AddSingleton<EntryPointPool>();
		Services.TryAddSingleton(Options.Create(new EntryPointPoolOptions { Enabled = false }));
		Services.TryAddSingleton(Mock.Of<IOIDCSettingsVerification>());

		if (!Services.Any(x => x.ServiceType == typeof(IRemoteFileService)))
		{
			Services.AddSingleton(DefaultClientServices.RemoteFileService);
		}

		var form = WinzorDispatcher.FormInstanceRegister.Lookup(new Uri(url));
		channel.Write(form);
		var component = RenderComponent<T>();
		component.WaitForState(() => component.Instance.ComponentResolved, timeSpan);
		RenderedForms.Add(form);
		return component;
	}

	public async Task<IRenderedComponent<EntryPointComponent>> RenderEntryPointComponent(Func<Form> formConstructor, IWindowService windowService, bool usePool = false)
	{
		var uri = default(Uri);
		await WinzorDispatcher.InvokeAsync(() =>
		{
			var form = formConstructor();
			var manageUri = usePool ? TestNavigationManager.PoolServerUri : TestNavigationManager.BaseServerUri;
			uri = WinzorDispatcher.FormInstanceRegister.Add(manageUri, form);
		});
		return RenderEntryPointComponent(uri.ToString(), windowService);
	}

	public async Task<IRenderedComponent<EntryPointComponent>> RenderEntryPointComponent(Func<Form> formConstructor, IOIDCSettingsVerification oidcSettingsVerification)
	{
		var uri = default(Uri);
		await WinzorDispatcher.InvokeAsync(() =>
		{
			var form = formConstructor();
			uri = WinzorDispatcher.FormInstanceRegister.Add(TestNavigationManager.BaseServerUri, form);
		});
		Services.AddSingleton(oidcSettingsVerification);
		return RenderEntryPointComponent(uri.ToString());
	}

	TimeSpan timeSpan = TimeSpan.FromSeconds(30);

	protected override void HookExceptionEvents()
	{
		// exception monitoring is implemented by CW1 test listeners
	}

	protected override void UnHookExceptionEvents()
	{
	}

	protected override void RegisterJSInteropServices()
	{
		CargoWise.Winzor.AppServer.Startup.RegisterJSInteropServices(Services);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		AsyncHelper.WaitAllActiveTasksForTest();
	}
}
