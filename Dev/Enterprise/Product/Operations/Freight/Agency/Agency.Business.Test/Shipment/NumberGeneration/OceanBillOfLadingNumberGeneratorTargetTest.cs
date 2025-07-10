using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class OceanBillOfLadingNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestParameters()
		{
			Set(AgencyRegistry.Instance.OceanBillNumberCustomisation, "OBL");
			OceanBillOfLadingNumberGeneratorTarget target = new OceanBillOfLadingNumberGeneratorTarget();
			target.Context = new NumberGeneratorContext();
			AssertCustomisation("Should find the OceanBillNumberCustomisation", "OBL", target.NumberCustomisation);
			AssertLocation(AgencyRegistry.Instance.OceanBillNumberCustomisation, target.NumberCustomisationLocation);
			AssertEquals(JobShipmentSchema.JS_HouseBill.MaxLength, target.MaxLength);
			AssertEquals("bill of lading", target.Name);
		}
	}
}
