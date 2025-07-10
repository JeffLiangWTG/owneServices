using System;
using System.Web.Http;
using CargoWiseOne.WebInfrastructure;
using Enterprise.Rating.Web.Configuration;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test.Configuration
{
	public class WebApplicationUpgradeConfiguratorTest : TestCase
	{
		public void TestConfigureWebUpgradeManager()
		{
			var config = new HttpConfiguration();

			var configurator = new WebApplicationUpgradeConfigurator(5);
			configurator.ConfigureWebUpgradeManager(config);

			Assert(config.Properties.TryGetValue("RatesAPIsWebUpgradeManagerInstance", out var property));
			AssertEquals(typeof(WebUpgradeManager), property.GetType());

			using var upgradeManager = property as WebUpgradeManager;
			AssertNotNull(upgradeManager);
		}

		[ExpectNoExceptions]
		public void TestConfigureWebUpgradeManager_ExceptionOccurs()
		{
			var config = new HttpConfiguration();

			var configurator = new WebApplicationUpgradeConfiguratorWithExceptionForTest(100); //retry every 100 milliseconds after waiting for 100 milliseconds.
			configurator.ConfigureWebUpgradeManager(config);
			Assert(config.Properties.ContainsKey("RatesAPIsUpgradeManagerInitializerTimerInstance"));

			System.Threading.Thread.Sleep(500);

			Assert(!config.Properties.ContainsKey("RatesAPIsUpgradeManagerInitializerTimerInstance"));
			AssertEquals(3, configurator.InitializeUpgradeManagerCalls);

			Assert(config.Properties.TryGetValue("RatesAPIsWebUpgradeManagerInstance", out var property));
			AssertEquals(typeof(WebUpgradeManager), property.GetType());

			using var upgradeManager = property as WebUpgradeManager;
			AssertNotNull(upgradeManager);
		}

		class WebApplicationUpgradeConfiguratorWithExceptionForTest : WebApplicationUpgradeConfigurator
		{
			public int InitializeUpgradeManagerCalls;

			readonly object lockObject = new object();

			public WebApplicationUpgradeConfiguratorWithExceptionForTest(int retryIntervalsInSecond) : base(retryIntervalsInSecond)
			{
			}

			protected override void CreateAndStartWebUpgradeManager(HttpConfiguration config)
			{
				lock (lockObject)
				{
					if (InitializeUpgradeManagerCalls <= 2)
					{
						throw new InvalidOperationException();
					}

					base.CreateAndStartWebUpgradeManager(config);
				}
			}

			protected override void InitializeUpgradeManager(object state)
			{
				lock (lockObject)
				{
					InitializeUpgradeManagerCalls++;
					base.InitializeUpgradeManager(state);
				}
			}
		}
	}
}
