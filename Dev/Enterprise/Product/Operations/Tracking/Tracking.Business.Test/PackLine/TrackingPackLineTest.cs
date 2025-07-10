using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Business.Testing
{
	public class TrackingPackLineTest : BaseFreightTest
	{
		PackLine GetPackLine()
		{
			return (PackLine)Factory.New(typeof(TrackingPackLine));
		}
		protected override void SetUp()
		{
			base.SetUp();
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != HomePort.SubstringSafe(0, 2))
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = HomePort.SubstringSafe(0, 2);
			}
		}

		public void TestCurrency()
		{
			TrackingPackLine loneLine = (TrackingPackLine)GetPackLine();
			AssertEquals("Line with no shipment specified", GlbCompany.CurrentCompany.LocalCurrency, loneLine.Currency);

			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.JS_RX_NKGoodsValueCurr = "USD";
			TrackingPackLine line = shipment.OuterPackLines.AddNew();
			AssertEquals("Line with shipment specified", "USD", line.Currency.RX_Code);
		}
	}
}
