using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ExportCustomsManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJE_VesselName_HasDuplicates()
		{
			const string expectedMessageError = "Duplicate Vessels exist for this Vessel Name.\r\nUse the <F4> key to show all vessels with this name for appropriate selection of the required vessel.";
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "DUPLICATE VESSEL";

			var manifestHeader = Factory.New<ExportCustomsManifestHeader>();
			manifestHeader.ED_TransportMode = Core.Constants.TransportModes.Sea;
			manifestHeader.ED_VesselName = vessel1.RV_Name;

			CombineAssertions(() =>
			{
				AssertNoMessageError("No duplicate", manifestHeader.ED_VesselNameInfo, expectedMessageError);
				var vessel2 = Factory.NewWithValidTestData<RefVessel>();
				vessel2.RV_Name = "DUPLICATE VESSEL";
				manifestHeader.Validation.ValidateED_VesselName();
				AssertHasMessageError("Has Duplicate Vessel Name", manifestHeader.ED_VesselNameInfo, expectedMessageError);
			});
		}
	}
}
