using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using Enterprise.Winzor.Architecture;
using Enterprise.Winzor.Architecture.Test;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using WinzorFramework;

[SetUpFixture]
public class EnterpriseTestSetup
{
	[OneTimeSetUp]
	public void RunBeforeAnyTests()
	{
		AssemblyResolver.Setup();
		InitializeTestingState();
		Initialization.ConfigureCargoWise(
			WinzorDispatcher,
			ServiceProvider.GetRequiredService<IWinzorCargoWiseLoginHandler>(),
			ServiceProvider.GetRequiredService<UserMonitorRegistry>());
	}

	[OneTimeTearDown]
	public async Task AfterRunningAllTests()
	{
		if (ServiceProvider != null)
		{
			await ServiceProvider.DisposeAsync();
		}

		WinzorDispatcher.Dispose();
	}

	void InitializeTestingState()
	{
		Globals.IsUserInteractive = true;
		TestingState.IsRunningTests = true;
		TestingState.IsRunningOnDAT = Initialization.DatIsTesting;
	}

	static readonly ServiceProvider ServiceProvider = InitializationConfigurationHelper.ConfigTestServices(new CargoWiseAuthStateProvider(NullLogger<CargoWiseAuthStateProvider>.Instance, It.IsAny<ITokenValidatorWrapper>()));

	public static readonly WinzorDispatcher WinzorDispatcher = ServiceProvider.GetRequiredService<WinzorDispatcher>();
}
