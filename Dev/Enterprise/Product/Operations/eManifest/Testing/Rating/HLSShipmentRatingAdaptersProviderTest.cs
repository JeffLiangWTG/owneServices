using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.eManifest.Business;
using Enterprise.eManifest.Business.Rating;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;

namespace Enterprise.eManifest.Testing.Rating
{
	public class HLSShipmentRatingAdaptersProviderTest : TestCaseWithFactory
	{
		public void TestGetAdapters()
		{
			var shipment = Factory.New<CommonShipment>();
			var bookingLine1 = Factory.New<SupplierBookingLine>();
			bookingLine1.DL_JS_ApprovedShipment = shipment.PK;
			var bookingLine2 = Factory.New<SupplierBookingLine>();
			bookingLine2.DL_JS_ApprovedShipment = shipment.PK;

			var adaptersProvider = new HLSShipmentRatingAdaptersProvider(shipment);

			AssertContainsExactElementsInAnyOrder(new[] { bookingLine1.PK, bookingLine2.PK }, adaptersProvider.GetAdapters(null, AutoRateOptions.AutorateRevenue)
				.OfType<HVLVLineRatingAdapter>()
				.Select(item => item.Parent.PK));
		}
	}
}
