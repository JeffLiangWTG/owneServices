using System.Threading;
using CargoWise.eHub.Products.NZCustoms.PullService;
using CargoWise.eHub.Products.NZCustoms.Tests;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.NZCustoms.Test
{
	[TestClass]
	public class NZCustomsPullRunnerTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsPullRunnerTests_OnStartOnStop()
		{
			var configuration = new TestConfigurationProvider();
			var logger = new TestLogger();
			var manager = new TestManager(logger);

			using (var service = new NZCustomsPullRunner(configuration, manager, logger))
			{
				service.Start();
				Thread.Sleep(500);
				service.Stop();
			}

			var expectedLog = @"Info - Service started
Info - Message recieved
Debug - Requested Stop
Info - Service stopped
";
			Assert.AreEqual(expectedLog, logger.Log);
		}

		class TestManager : INZCustomsPullManager
		{
			ILog logger;

			public TestManager(ILog logger)
			{
				this.logger = logger;
			}

			static int runCount = -1;
			int[] returnValues = new int[] { 1, 1, 1, 0, 0, 0, 1, 1, 1, 1 };

			public int RunCount
			{
				get
				{
					return runCount;
				}
			}

			public bool PullNZCustomsServiceForNewMessageAndPushItToBiztalk()
			{
				Thread.Sleep(500);
				logger.Info("Message recieved");
				Thread.Sleep(500);
				runCount++;
				return returnValues[runCount % returnValues.Length] == 1;
			}
		}
	}
}
