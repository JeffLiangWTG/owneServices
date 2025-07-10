using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class BaseJobVoyageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGetCarrierLookup()
		{
			AssertEquals("air carrier lookup", typeof(AirShippingProviderCollection),
				BaseJobVoyageLookups.GetCarrierLookup(Factory, Core.Constants.TransportModes.Air).GetType());

			AssertEquals("air carrier lookup", typeof(SeaShippingProviderCollection),
				BaseJobVoyageLookups.GetCarrierLookup(Factory, Core.Constants.TransportModes.Sea).GetType());

			AssertEquals("air carrier lookup", typeof(TransportScheduleRailShippingProviderCollection),
				BaseJobVoyageLookups.GetCarrierLookup(Factory, Core.Constants.TransportModes.Rail).GetType());

			AssertEquals("air carrier lookup", typeof(TransportScheduleLineHaulShippingProviderCollection),
				BaseJobVoyageLookups.GetCarrierLookup(Factory, Core.Constants.TransportModes.Road).GetType());

			AssertEquals("air carrier lookup", typeof(ShippingProviderCollection),
				BaseJobVoyageLookups.GetCarrierLookup(Factory, "ZZZ").GetType());
		}
	}
}
