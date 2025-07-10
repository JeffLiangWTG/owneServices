using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class OceanShipmentNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestParameters()
		{
			Set(AgencyRegistry.Instance.OceanBillShipmentNumberCustomisation, "OSN");
			NumberGeneratorTarget target = new OceanShipmentNumberGeneratorTarget();
			target.Context = new NumberGeneratorContext();
			AssertCustomisation("Should find the OceanBillShipmentNumberCustomisation", "OSN", target.NumberCustomisation);
			AssertLocation(AgencyRegistry.Instance.OceanBillShipmentNumberCustomisation, target.NumberCustomisationLocation);
			AssertEquals(20, target.MaxLength);
			AssertEquals("shipment number", target.Name);
		}
	}
}
