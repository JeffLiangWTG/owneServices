using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	sealed class VesselNameImportHelperTest : TestCaseWithFactory
	{
		public void TestHelperDoesNotMatchByEmptyLloydsNumber()
		{
			var vesselWithEmptyLlodsNo = Factory.New<RefVessel>();
			vesselWithEmptyLlodsNo.RV_Name = "No LLoyds No";
			vesselWithEmptyLlodsNo.RV_LloydsNumber = ZString.Empty;

			var sailing = new Xsd.SailingWithVesselVoyage();
			sailing.VesselName = "BLAH";
			AssertEquals("BLAH", VesselNameImportHelper.MatchVesselAndGetVesselName(sailing, Factory));
		}

		public void TestMatchVesselAndGetVesselName()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";

			var sailing = new Xsd.SailingWithVesselVoyage();
			sailing.LloydsNo = vessel.RV_LloydsNumber;
			sailing.VesselName = ZString.Empty;
			AssertEquals(vessel.RV_Name, VesselNameImportHelper.MatchVesselAndGetVesselName(sailing, Factory));

			sailing.LloydsNo = ZString.Empty;
			sailing.VesselName = vessel.RV_Name;
			AssertEquals(vessel.RV_Name, VesselNameImportHelper.MatchVesselAndGetVesselName(sailing, Factory));

			sailing.LloydsNo = "9999";
			sailing.VesselName = "BLAH";
			AssertEquals("BLAH", VesselNameImportHelper.MatchVesselAndGetVesselName(sailing, Factory));

			sailing.LloydsNo = "9999";
			sailing.VesselName = ZString.Empty;
			AssertEquals("9999", VesselNameImportHelper.MatchVesselAndGetVesselName(sailing, Factory));
		}
	}
}
