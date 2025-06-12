using System.Threading;
using CargoWise.eHub.Products.JPCustoms.PullService.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	[TestClass]
	public class JPCustomsPullRunnerTest : TestBase
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsPullRunnerTests_OnStartOnStop()
		{
			var manager = new Mock<IJPCustomsPullManager>();
			manager.Setup(m => m.PullCustomsForNewMessageAndPushToEHub());

			using (var service = new JPCustomsPullRunner(Logger, PullRunnerConfiguration, manager.Object))
			{
				service.Start();
				Thread.Sleep(1900);
				service.Stop();
			}

			manager.Verify(m => m.PullCustomsForNewMessageAndPushToEHub(), Times.Exactly(2));
		}
	}
}
