using CargoWise.DataProtection;
using Enterprise.Winzor.Architecture;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace CargoWise.Blazor.HostedAppRuntime.Test;

sealed class HostedAppRuntimeConfigurationExtensionsTest
{
	[Test]
	public void TestAddCargoWiseRuntime()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		HostedAppRuntimeConfigurationExtensions.AddCargoWiseRuntime(services);

		// Assert
		var serviceProvider = services.BuildServiceProvider();
		Assert.That(serviceProvider.GetRequiredService<IProtectedDataServiceFactory>(), Is.Not.Null);
		Assert.That(serviceProvider.GetRequiredService<ISqlConnectionProvider>(), Is.Not.Null);
		Assert.That(serviceProvider.GetRequiredService<IKnownGoodProtectedDataCache>(), Is.Not.Null);
		Assert.That(serviceProvider.GetRequiredService<IKnownGoodProtectedDataCache>().GetType().Name, Is.EqualTo("WebKnownGoodProtectedDataCache"));
	}
}
