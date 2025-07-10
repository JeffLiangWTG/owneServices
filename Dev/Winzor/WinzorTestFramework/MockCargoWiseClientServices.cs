using System;
using CargoWise.Blazor.Client.Integration.Files;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.Client.Integration.OIDCVerify;
using Microsoft.JSInterop;
using Moq;
using WinzorFramework;
using WinzorFramework.JSInterop;

namespace WinzorTestFramework;

public class MockCargoWiseClientServices
{
	public Mock<ILifecycleService> LifecycleService { get; } = new Mock<ILifecycleService>();

	public Mock<IWindowService> WindowService = new Mock<IWindowService>();

	public Mock<IMenuDisplayer> MenuDisplayer { get; } = new Mock<IMenuDisplayer>();

	public Mock<IFileService> FileService { get; } = new Mock<IFileService>();

	public Mock<IClientEventService> ClientEventService { get; } = new Mock<IClientEventService>();

	public Mock<IRemoteFileService> RemoteFileService { get; } = new Mock<IRemoteFileService>();

	public Mock<IClientFileApi> ClientFileApi { get; } = new Mock<IClientFileApi>();

	public Mock<IOIDCSettingsVerification> OIDCSettingsVerification { get; } = new Mock<IOIDCSettingsVerification>();

	public MockCargoWiseClientServices(WinzorTestContext ctx)
	{
	}

	public static CargoWiseClientServices MakeMock(
		Uri serverBaseUri = null,
		ILifecycleService lifecycleService = null,
		IWindowService windowService = null,
		IMenuDisplayer menuDisplayer = null,
		IJSRuntime jsRuntime = null,
		IFileService fileService = null,
		IClientEventService clientEventService = null,
		IOIDCSettingsVerification oidcSettingsVerification = null,
		IClientFileApi clientFileApi = null)
	{
		return new CargoWiseClientServices(
			serverBaseUri ?? TestNavigationManager.BaseServerUri,
			lifecycleService ?? Mock.Of<ILifecycleService>(),
			windowService ?? Mock.Of<IWindowService>(),
			menuDisplayer ?? Mock.Of<IMenuDisplayer>(),
			jsRuntime ?? Mock.Of<IJSRuntime>(),
			fileService ?? Mock.Of<IFileService>(),
			clientEventService ?? Mock.Of<IClientEventService>(),
			oidcSettingsVerification ?? Mock.Of<IOIDCSettingsVerification>(),
			clientFileApi ?? Mock.Of<IClientFileApi>());
	}
}
