using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class SouthAfricaForwardingShipmentSupport : TestCaseWithFactory
	{
		public void TestAddCountryCodeToEntryDetailsCaptionCore()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "ZAJNB";
			Assert(shipment.AddCountryCodeToEntryDetailsCaption);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
			{
				shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "ZAJNB";
				Assert(!shipment.AddCountryCodeToEntryDetailsCaption);
			}
		}
	}
}
