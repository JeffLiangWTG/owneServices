using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class TradeLanesGridMenuItemsBuilderTest : TestCaseWithFactory
	{
		public void TestGetSelectedTradeDetailsForQuote()
		{
			var shipmentProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();
			var salesHeader = new SalesHeader(org, opp, shipmentProduct);

			var sales1 = salesHeader.FilterableEntitySalesCollection.AddNew();
			var tradeDetail1A = sales1.EntityTradeDetailsCollection.AddNew();
			var tradeDetail1B = sales1.EntityTradeDetailsCollection.AddNew();
			var sales2 = salesHeader.FilterableEntitySalesCollection.AddNew();
			var tradeDetail2A = sales2.EntityTradeDetailsCollection.AddNew();
			var tradeDetail2B = sales2.EntityTradeDetailsCollection.AddNew();
			var sales3 = salesHeader.FilterableEntitySalesCollection.AddNew();
			var tradeDetail3A = sales3.EntityTradeDetailsCollection.AddNew();
			var tradeDetail3B = sales3.EntityTradeDetailsCollection.AddNew();

			Factory.Save();

			using (var form = new ZForm(salesHeader))
			using (var control = new TradeLanesControlForTest(shipmentProduct))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Precondition", 3, control.TradeLanesGrid.ListManager.Count);

				var builder = new TradeLanesGridMenuItemsBuilderForTest(shipmentProduct, control.TradeLanesGrid);

				control.TradeLanesGrid.UnSelectAll();
				control.TradeLanesGrid.ListManager.Position = control.TradeLanesGrid.List.IndexOf(sales1);
				AssertContainsExactElementsInAnyOrder(new[] { tradeDetail1A, tradeDetail1B }, builder.GetSelectedTradeDetailsForQuote_Exposed());

				control.TradeLanesGrid.SelectAllElements(x => x == sales2 || x == sales3);
				AssertContainsExactElementsInAnyOrder(new[] { tradeDetail2A, tradeDetail2B, tradeDetail3A, tradeDetail3B }, builder.GetSelectedTradeDetailsForQuote_Exposed());
			}
		}

		public void TestGetSelectedTradeDetailForSpotQuote()
		{
			var shipmentProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();
			var salesHeader = new SalesHeader(org, opp, shipmentProduct);

			var sales1 = salesHeader.FilterableEntitySalesCollection.AddNew();
			var tradeDetail1A = sales1.EntityTradeDetailsCollection.AddNew();

			var sales2 = salesHeader.FilterableEntitySalesCollection.AddNew();
			var tradeDetail2A = sales2.EntityTradeDetailsCollection.AddNew();
			var tradeDetail2B = sales2.EntityTradeDetailsCollection.AddNew();

			var sales3 = salesHeader.FilterableEntitySalesCollection.AddNew();

			Factory.Save();

			using (var form = new ZForm(salesHeader))
			using (var control = new TradeLanesControlForTest(shipmentProduct))
			{
				form.Controls.Add(control);
				salesHeader.CompanyFilter = Env.CurrentCompany.PK;
				form.Show();

				AssertEquals("Precondition", 3, control.TradeLanesGrid.ListManager.Count);

				var builder = new TradeLanesGridMenuItemsBuilderForTest(shipmentProduct, control.TradeLanesGrid);

				control.TradeLanesGrid.UnSelectAll();
				control.TradeLanesGrid.ListManager.Position = control.TradeLanesGrid.List.IndexOf(sales1);
				AssertEquals(tradeDetail1A, builder.GetSelectedTradeDetailForSpotQuote_Exposed());

				control.TradeLanesGrid.ListManager.Position = control.TradeLanesGrid.List.IndexOf(sales2);
				builder.GetSelectedTradeDetailForSpotQuote_Exposed();
				AssertType(typeof(TradeDetailSelectionForm), ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(false, ((TradeDetailSelectionForm)ZFormModaliser.LastFormShownDialogForTest).ShowTradeLaneColumns);

				control.TradeLanesGrid.ListManager.Position = control.TradeLanesGrid.List.IndexOf(sales3);
				AssertEquals(null, builder.GetSelectedTradeDetailForSpotQuote_Exposed());
				AssertEquals("Please select the trade lane details you wish to create a quote for.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
