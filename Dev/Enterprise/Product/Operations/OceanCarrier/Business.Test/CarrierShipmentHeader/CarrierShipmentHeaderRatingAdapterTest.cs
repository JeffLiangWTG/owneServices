using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;

namespace Enterprise.OceanCarrier.Business.Testing
{
	sealed class CarrierShipmentHeaderRatingAdapterTest : TestCaseWithFactory
	{
		public void TestAdapterProperties()
		{
			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();

			var ratingAdapter = new CarrierShipmentRatingAdapter(carrierShipmentHeader);
			AssertEquals(nameof(CarrierShipmentRatingAdapter.AdapterType), AdapterType.BillOfLading, ratingAdapter.AdapterType);
			AssertEquals(nameof(CarrierShipmentRatingAdapter.RateTypeToUse), RateType.Shipping, ratingAdapter.RateTypeToUse);

			AssertContainsExactElementsInAnyOrder("ChargeCodeGroups come from the Rating registry", Env.Registry.Rating.FreightRatedCodes, ratingAdapter.ChargeCodeGroups);
			AssertNotNull("JobDatesProvider has been set", ratingAdapter.JobDatesProvider);
		}
	}
}
