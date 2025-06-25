using Microsoft.Extensions.Logging;
using Moq;

namespace CargoWise.Billing.CollectorService.Plugin.Tests
{
	public class AbstractPluginTest<TPlugin>
		where TPlugin : AbstractPlugin
	{
		protected void SetLogger(Mock<TPlugin> pluginMock, ILogger logger)
		{
			pluginMock.Setup(_ => _.CreateLogger()).Returns(logger);
		}
	}
}
