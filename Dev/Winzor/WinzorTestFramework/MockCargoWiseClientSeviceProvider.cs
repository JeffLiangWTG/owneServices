using CargoWise.Blazor.Client.Integration.Files;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.Client.Integration.OIDCVerify;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace WinzorTestFramework;

public class MockCargoWiseClientSeviceProvider : ICargoWiseClientServiceProvider
{
	public Mock<ILifecycleService> MockLifecycleService { get; set; } = new Mock<ILifecycleService>();

	public Mock<IWindowService> MockWindowService { get; set; } = new Mock<IWindowService>();

	public Mock<IMenuDisplayer> MockMenuDisplayer { get; set; } = new Mock<IMenuDisplayer>();

	public Mock<IOIDCSettingsVerification> MockOIDCSettingsVerification { get; set; } = new Mock<IOIDCSettingsVerification>();

	public Mock<IRemoteFileService> MockRemoteFileService { get; set; } = new Mock<IRemoteFileService>();

	public Mock<IClientFileApi> MockClientFileApi { get; set; } = new Mock<IClientFileApi>();

	public IServiceCollection AddCargoWiseClient(IServiceCollection services)
	{
		if (MockLifecycleService != null)
		{
			services.AddSingleton(MockLifecycleService.Object);
		}
		if (MockWindowService != null)
		{
			services.AddSingleton(MockWindowService.Object);
		}
		if (MockMenuDisplayer != null)
		{
			services.AddSingleton(MockMenuDisplayer.Object);
		}
		if (MockOIDCSettingsVerification != null)
		{
			services.AddSingleton(MockOIDCSettingsVerification.Object);
		}
		if (MockRemoteFileService != null)
		{
			services.AddSingleton(MockRemoteFileService.Object);
		}
		if (MockClientFileApi != null)
		{
			services.AddSingleton(MockClientFileApi.Object);
		}
		return services;
	}
}
