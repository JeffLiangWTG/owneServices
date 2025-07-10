using Enterprise.Core;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.Agency.Business.Shipment.Rating.Testing
{
	internal class BillOfLadingRateLineConditionsSupporterTest : ShipmentRateLineConditionsSupporterTest
	{
		public override void TestHasDangerousGoods()
		{
			var bookingWithFCL = Factory.New<BillOfLading>();
			bookingWithFCL.JS_PackingMode = Constants.ContainerModes.FCL;
			var packlineWithFCL = bookingWithFCL.OuterPackLines.AddNew();
			var supporterWithFCL = new BillOfLadingRateLineConditionsSupporter(bookingWithFCL);
			Assert("Booking with FCL has no dangerous good", !supporterWithFCL.HasDangerousGoods);
			packlineWithFCL.UNDGs.AddNew();
			Assert("Booking with FCL has dangerous goods", supporterWithFCL.HasDangerousGoods);
			AssertHasDangerousGoods(Constants.ContainerModes.RollOnRollOff);
			AssertHasDangerousGoods(Constants.ContainerModes.BreakBulk);
			AssertHasDangerousGoods(Constants.ContainerModes.Bulk);
			AssertHasDangerousGoods(Constants.ContainerModes.Liquid);
			void AssertHasDangerousGoods(string packingMode)
			{
				var booking = Factory.New<BillOfLading>();
				booking.JS_PackingMode = packingMode;
				var packline = booking.TopLevelPacks.AddNew();
				Assert("Precondition:IsTopLevelPacksMode should be true", booking.IsTopLevelPacksMode);
				var supporterWithBreakBulk = new BillOfLadingRateLineConditionsSupporter(booking);
				Assert("Booking has no dangerous good for packing mode:" + packingMode, !supporterWithBreakBulk.HasDangerousGoods);
				packline.UNDGs.AddNew();
				Assert("Booking has dangerous goods for packing mode:" + packingMode, supporterWithBreakBulk.HasDangerousGoods);
			}
		}
	}
}
