using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.GPS.Module.Testing
{
	[TestedType(typeof(GPSSupporterPlugInController))]
	sealed class GPSSupporterPlugInControllerTest : ZControllerBasherTest
	{
		public void TestCheckpoints()
		{
			var obj = Factory.New<RefEquipment>();
			AssertEquals(Env.Security.AllowVehicleMonitoringAndManagement, Controller.GetCheckPointForDelete(obj));
			AssertEquals(Env.Security.AllowVehicleMonitoringAndManagement, Controller.GetCheckPointForEdit(obj));
			AssertEquals(Env.Security.AllowVehicleMonitoringAndManagement, Controller.GetCheckPointForNew(obj));
			AssertEquals(Env.Security.AllowVehicleMonitoringAndManagement, Controller.GetCheckPointForView(obj));
		}

		protected override ControllerID GetControllerID() => ControllerIDs.GPSSupporter;
	}
}
