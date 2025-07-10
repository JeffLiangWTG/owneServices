using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class OceanBookingNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestParameters()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			Set(AgencyRegistry.Instance.BookingNumberCustomisation, "BNC");
			OceanBookingNumberGeneratorTarget target = new OceanBookingNumberGeneratorTarget();
			target.Context = new NumberGeneratorContext();
			AssertCustomisation("Should find the BookingNumberCustomisation", "BNC", target.NumberCustomisation);
			AssertLocation(AgencyRegistry.Instance.BookingNumberCustomisation, target.NumberCustomisationLocation);
			AssertEquals(JobShipmentSchema.JS_CFSReference.MaxLength, target.MaxLength);
			AssertEquals("booking number", target.Name);
			Factory.Save();
			AssertEquals("Should have set JS_UniqueConsignRef", "V00001000", shipment.JS_UniqueConsignRef);
			AssertEquals("Should have set JS_HouseBill", "V00001000", shipment.JS_HouseBill);
			AssertEquals("Should have set JS_CFSReference", "VBNC00001000", shipment.JS_CFSReference);
		}
	}
}
