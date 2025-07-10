using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.SessionBroker.Authentication;
using CargoWise.Blazor.SessionBroker.Helpers;
using CargoWise.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test;

sealed class StartupTest
{
	[Test]
	public void TestConfigureServicesConfiguresPds()
	{
		// Arrange
		var config = new Mock<IConfiguration>();
		config.Setup(x => x.GetSection(nameof(CargoWiseOptions))).Returns(Mock.Of<IConfigurationSection>());
		config.Setup(x => x.GetSection(nameof(ShutdownOptions))).Returns(Mock.Of<IConfigurationSection>());

		var startup = new Startup(config.Object);
		var serviceCollection = new ServiceCollection();

		// Act
		startup.ConfigureServices(serviceCollection);

		// Assert
		var serviceProvider = serviceCollection.BuildServiceProvider();
		Assert.That(serviceProvider.GetRequiredService<IProtectedDataServiceFactory>(), Is.Not.Null);
		Assert.That(serviceProvider.GetRequiredService<ISqlConnectionProvider>(), Is.Not.Null);
		Assert.That(serviceProvider.GetRequiredService<IKnownGoodProtectedDataCache>(), Is.Not.Null);
		Assert.That(serviceProvider.GetRequiredService<IKnownGoodProtectedDataCache>().GetType().Name, Is.EqualTo("WebKnownGoodProtectedDataCache"));
		Assert.That(serviceProvider.GetRequiredService<IDatabaseAccessor>(), Is.Not.Null);
	}
}
