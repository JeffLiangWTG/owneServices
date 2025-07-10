using CargoWise.Blazor.Client.Integration.Files;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.Client.Integration.OIDCVerify;
using Microsoft.JSInterop;
using WinzorFramework.JSInterop;

namespace WinzorFramework;

public class CargoWiseClientServices
{
	public CargoWiseClientServices(
		Uri serverBaseUri,
		ILifecycleService lifecycleService,
		IWindowService windowService,
		IMenuDisplayer menuDisplayer,
		IJSRuntime jsRuntime,
		IFileService fileService,
		IClientEventService clientEventService,
		IOIDCSettingsVerification oidcSettingsVerification,
		IClientFileApi clientFileApi)
	{
		ServerBaseUri = serverBaseUri;
		LifecycleService = lifecycleService;
		WindowService = windowService;
		MenuDisplayer = menuDisplayer;
		JSRuntime = jsRuntime;
		FileService = fileService;
		ClientEventService = clientEventService;
		OIDCSettingsVerification = oidcSettingsVerification;
		ClientFileApi = clientFileApi;
	}

	public Uri ServerBaseUri { get; set; }

	public ILifecycleService LifecycleService { get; set; }

	public IWindowService WindowService { get; set; }

	public IMenuDisplayer MenuDisplayer { get; set; }

	public IJSRuntime JSRuntime { get; set; }

	public IFileService FileService { get; set; }

	public IClientEventService ClientEventService { get; set; }

	public IRemoteFileService? RemoteFileService { get; set; }

	public IClientFileApi? ClientFileApi { get; set; }

	public IRenderService? RenderService { get; set; }

	public IOIDCSettingsVerification OIDCSettingsVerification { get; set; }
}
