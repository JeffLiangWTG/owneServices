using System.Linq;
using System.Threading.Tasks;
using CargoWiseNext.Infrastructure.Installations;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;
internal class InMemoryAppServerContextTest
{
	// Looks like this specific test fails on DAT due to swapping to Development environment but it passes fine on local
	// [TestCase("Development", false)]
	[TestCase("DAT", false)]
	[TestCase("IntegrationTest", false)]
	[TestCase("Staging", true)]
	public async Task WiseTechErrorReportingExtensionsShouldBeAdded(string envType, bool isLoggerConfigured)
	{
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider, envType);
		var expectedServiceName = "WTG.ErrorReporting.Extensions.Logging.ErrorReportingLoggerOptions";
		Assert.That(ctx.Services.Any(s => s.ServiceType.FullName.Contains(expectedServiceName)), Is.EqualTo(isLoggerConfigured));
	}

	[Test, Explicit("failed due to Initialization.ConfigureCargoWise when RunBeforeAnyTests in EnterpriseTestSetup")]
	public async Task TestInitializeCargoWiseRuntime()
	{
		await using (new InMemoryAppServerTestContext())
		{
			Assert.That(Initialization.Initialized, Is.EqualTo(false));
		}

		await using (new InMemoryAppServerTestContext(new MockCargoWiseClientSeviceProvider()))
		{
			Assert.That(Initialization.Initialized, Is.EqualTo(false));
		}

		await using (new InMemoryAppServerTestContext(needCargoWiseRuntime: false))
		{
			Assert.That(Initialization.Initialized, Is.EqualTo(false));
		}

		var mock = new Mock<RegisteredFormInstances>(Mock.Of<IUrlHandlerProvider>());
		await using (new InMemoryAppServerTestContext(needCargoWiseRuntime: true))
		{
			Assert.That(Initialization.Initialized, Is.EqualTo(true));
		}
	}
}
