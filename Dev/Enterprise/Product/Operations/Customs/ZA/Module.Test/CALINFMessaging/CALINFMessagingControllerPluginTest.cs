using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.ZA.Module.Testing
{
	sealed class CALINFMessagingControllerPluginTest : TestCaseWithFactory
	{
		public void TestPlugin()
		{
			var voyage = Factory.New<JobVoyage>();
			var other = Factory.New<JobMawb>();
			var controllerTest = new CALINFMessagingControllerTester();
			AssertEquals("CALINF Messages", controllerTest.PluginTabPageCaption.Caption);
			using (var plugin = controllerTest.ExposedGetPlugIn(voyage))
			{
				AssertNotNull("Plugin should exist", plugin);
				AssertType("Plugin should be CALINFMessagingPlugin", typeof(CALINFMessagingPlugin), plugin);
			}

			using (var plugin = controllerTest.ExposedGetPlugIn(other))
			{
				AssertNull("Plugin should be null", plugin);
			}
		}

		sealed class CALINFMessagingControllerTester : CALINFMessagingController
		{
			public ZPlugIn ExposedGetPlugIn(IBusiness businessEntity) => GetPlugIn(businessEntity);
		}
	}
}
