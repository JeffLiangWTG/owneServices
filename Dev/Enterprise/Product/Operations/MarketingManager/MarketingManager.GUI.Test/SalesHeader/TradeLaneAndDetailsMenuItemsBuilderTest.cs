using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class TradeLaneAndDetailsMenuItemsBuilderTest : TestCaseWithFactory
	{
		public void TestCreateQuoteMenuItem()
		{
			var shipmentProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.New<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();

			var salesHeaderCollection = (SalesHeaderCollection)opportunity.ActualAndProspectiveSalesHeaderCollection;
			var shipmentSalesHeader = salesHeaderCollection.AddNew(shipmentProduct);
			using (var form = new ZForm(opportunity))
			using (var control = new TradeLaneOrDetailsControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var builder = new TradeLaneAndDetailsMenuItemsBuilderForTest(shipmentProduct, control.Grid);
				builder.AddMenuItems();

				var createQuoteMenuItem = control.Grid.ContextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Create Quote");

				var generateQuoteController = new MockGenerateQuoteForSalesValueAssociatedEntityControllerForTest();
				using (ObjectFactory.Substitute<IGenerateQuoteForSalesValueAssociatedEntityController>(generateQuoteController))
				{
					builder.SelectedTradeDetailsForQuote = Enumerable.Empty<OrgTradeDetail>();
					createQuoteMenuItem.PerformClick();
					AssertEquals("Please select the trade lane details you wish to create a quote for.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("EntityCalled", generateQuoteController.EntityCalled);
					AssertNull("TradeDetailsCalled", generateQuoteController.TradeDetailsCalled);

					var tradeDetails = new[] { shipmentSalesHeader.EntitySalesCollectionProductView.AddNew().EntityTradeDetailsCollection.AddNew() };
					builder.SelectedTradeDetailsForQuote = tradeDetails;
					UnitTestUserNotification.Instance.ClearMessages();
					createQuoteMenuItem.PerformClick();
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertEquals("EntityCalled", opportunity, generateQuoteController.EntityCalled);
					AssertContainsExactElementsInAnyOrder("TradeDetailsCalled", tradeDetails, generateQuoteController.TradeDetailsCalled);
				}
			}
		}

		public void TestCreateSpotQuoteMenuItem()
		{
			var shipmentProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.New<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();

			var salesHeaderCollection = (SalesHeaderCollection)opportunity.ActualAndProspectiveSalesHeaderCollection;
			var shipmentSalesHeader = salesHeaderCollection.AddNew(shipmentProduct);
			using (var form = new ZForm(opportunity))
			using (var control = new TradeLaneOrDetailsControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var builder = new TradeLaneAndDetailsMenuItemsBuilderForTest(shipmentProduct, control.Grid);
				builder.AddMenuItems();

				var createSpotQuoteMenuItem = control.Grid.ContextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Create One Off Quote");

				var mockGenerateSpotQuoteController = new Mock<IGenerateSpotQuoteFromTradeDetailController>();
				using (ObjectFactory.Substitute(mockGenerateSpotQuoteController.Object))
				{
					var tradeDetail = shipmentSalesHeader.EntitySalesCollectionProductView.AddNew().EntityTradeDetailsCollection.AddNew();
					builder.SelectedTradeDetailForSpotQuote = tradeDetail;
					createSpotQuoteMenuItem.PerformClick();
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					mockGenerateSpotQuoteController.Verify(x => x.ShowNewForm(tradeDetail, opportunity));
				}
			}
		}
	}
}
