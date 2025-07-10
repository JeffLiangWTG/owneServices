using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class TradeDetailsGridMenuItemsBuilderTest : TestCaseWithFactory
	{
		public void TestGetSelectedTradeDetailsForQuote()
		{
			var shipmentProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.New<OrgHeader>();
			var sales = EntitySalesWrapper.Get(Factory.New<OrgSales>(), org);
			var tradeDetail1 = sales.EntityTradeDetailsCollection.AddNew();
			var tradeDetail2 = sales.EntityTradeDetailsCollection.AddNew();
			var tradeDetail3 = sales.EntityTradeDetailsCollection.AddNew();

			using (var form = new ZForm(sales))
			using (var control = new TradeDetailsControlForTest(shipmentProduct))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Precondition", 3, control.TradeDetailsGrid.ListManager.Count);

				var builder = new TradeDetailsGridMenuItemsBuilderForTest(shipmentProduct, control.TradeDetailsGrid);

				control.TradeDetailsGrid.UnSelectAll();
				control.TradeDetailsGrid.ListManager.Position = control.TradeDetailsGrid.List.IndexOf(tradeDetail1);
				AssertContainsExactElementsInAnyOrder(new[] { tradeDetail1 }, builder.GetSelectedTradeDetailsForQuote_Exposed());

				control.TradeDetailsGrid.SelectAllElements(x => x == tradeDetail2 || x == tradeDetail3);
				AssertContainsExactElementsInAnyOrder(new[] { tradeDetail2, tradeDetail3 }, builder.GetSelectedTradeDetailsForQuote_Exposed());
			}
		}

		public void TestGetSelectedTradeDetailForSpotQuote()
		{
			var shipmentProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.New<OrgHeader>();
			var sales = EntitySalesWrapper.Get(Factory.New<OrgSales>(), org);
			var tradeDetail1 = sales.EntityTradeDetailsCollection.AddNew();
			var tradeDetail2 = sales.EntityTradeDetailsCollection.AddNew();
			var tradeDetail3 = sales.EntityTradeDetailsCollection.AddNew();

			using (var form = new ZForm(sales))
			using (var control = new TradeDetailsControlForTest(shipmentProduct))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Precondition", 3, control.TradeDetailsGrid.ListManager.Count);

				var builder = new TradeDetailsGridMenuItemsBuilderForTest(shipmentProduct, control.TradeDetailsGrid);

				control.TradeDetailsGrid.UnSelectAll();
				control.TradeDetailsGrid.ListManager.Position = control.TradeDetailsGrid.List.IndexOf(tradeDetail1);
				AssertEquals(tradeDetail1, builder.GetSelectedTradeDetailForSpotQuote_Exposed());

				control.TradeDetailsGrid.SelectAllElements(x => x == tradeDetail2 || x == tradeDetail3);
				AssertEquals(tradeDetail1, builder.GetSelectedTradeDetailForSpotQuote_Exposed());

				control.TradeDetailsGrid.List.Clear();
				AssertEquals(null, builder.GetSelectedTradeDetailForSpotQuote_Exposed());
				AssertEquals("Please select a trade lane detail you wish to create a spot quote for.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
