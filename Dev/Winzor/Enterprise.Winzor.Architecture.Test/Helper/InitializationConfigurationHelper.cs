using CargoWise.Blazor.Common;
using CargoWise.Blazor.Testing.Common;
using CargoWise.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using WinzorFramework;

namespace Enterprise.Winzor.Architecture.Test;

public static class InitializationConfigurationHelper
{
	public static ServiceProvider ConfigTestServices(ICargoWiseAuthStateProvider cargoWiseAuthStateProvider = null, CargoWiseAuthOptions options = null)
	{
		options ??= new CargoWiseAuthOptions();
		cargoWiseAuthStateProvider ??= new CargoWiseAuthStateProvider(NullLogger<CargoWiseAuthStateProvider>.Instance, It.IsAny<ITokenValidatorWrapper>());
		var serviceCollection = new ServiceCollection()
			.AddLogging()
			.AddSingleton<WinzorDispatcher>()
			.AddSingleton<IWinzorCargoWiseLoginHandler, WinzorCargoWiseLoginHandler>()
			.AddSingleton(cargoWiseAuthStateProvider)
			.AddSingleton(Options.Create(options))
			.AddSingleton(UrlHandlerProviderMock.GetObject())
			.AddSingleton<IFormInstanceRegister, EnterpriseRegisteredFormInstances>()
			.AddSingleton<IFormOpener, FormOpener>()
			.AddSingleton<UserMonitorRegistry>()
			.ConfigureProtectedDataFactoryServices()
			.ConfigureProtectedDataSqlExtensions(ApplicationType.Web);

		return serviceCollection.BuildServiceProvider();
	}
}
