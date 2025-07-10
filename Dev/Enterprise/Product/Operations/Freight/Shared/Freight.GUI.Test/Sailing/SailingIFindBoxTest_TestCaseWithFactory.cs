using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.GUI
{
	sealed class SailingIFindBoxTest_TestCaseWithFactory : TestCaseWithFactory
	{
		#region TestGetModuleFromTransportMode

		public void TestGetModuleFromTransportMode()
		{
			CommonShipment exportBooking = Factory.New<CommonShipment>();
			Testing.SailingIFindBoxForTest sailingFindBox = new Testing.SailingIFindBoxForTest(exportBooking);
			AssertEquals(ModuleIDs.JobAirSailing, sailingFindBox.GetModuleFromTransportMode(Constants.TransportModes.Air));
			AssertEquals(ModuleIDs.JobSeaSailing, sailingFindBox.GetModuleFromTransportMode(Constants.TransportModes.Sea));
			AssertEquals(ModuleIDs.JobRailSailing, sailingFindBox.GetModuleFromTransportMode(Constants.TransportModes.Rail));
			AssertEquals(ModuleIDs.JobRoadSailing, sailingFindBox.GetModuleFromTransportMode(Constants.TransportModes.Road));
		}

		#endregion
	}
}
