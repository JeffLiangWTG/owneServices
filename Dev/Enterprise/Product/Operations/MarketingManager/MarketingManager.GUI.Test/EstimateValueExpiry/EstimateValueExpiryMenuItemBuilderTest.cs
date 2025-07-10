using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class EstimateValueExpiryMenuItemBuilderTest : TestCaseWithFactory
	{
		public void TestSetupContextMenu()
		{
			var shipmentProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.New<OrgHeader>();
			var sales = EntitySalesWrapper.Get(Factory.New<OrgSales>(), org);
			var tradeDetail1 = sales.EntityTradeDetailsCollection.AddNew();
			var tradeDetail2 = sales.EntityTradeDetailsCollection.AddNew();
			var tradeDetail3 = sales.EntityTradeDetailsCollection.AddNew();

			tradeDetail1.PA_Status = OpportunityTradeStatus.Codes.Successful;
			tradeDetail2.PA_Status = OpportunityTradeStatus.Codes.Active;
			tradeDetail3.PA_Status = OpportunityTradeStatus.Codes.Successful;
			tradeDetail3.ProspectDetail.PAP_ExpiryReason = OrgTradeProspectExpiryReasonList.Codes.Lost;

			using (var form = new ZForm(sales))
			using (var control = new TradeDetailsControlForTest(shipmentProduct))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Precondition", 3, control.TradeDetailsGrid.ListManager.Count);

				EstimateValueExpiryMenuItemBuilder.SetupContextMenu(control.TradeDetailsGrid, () =>
					{
						return control.TradeDetailsGrid.SelectedElements.Cast<OrgTradeDetail>();
					});

				var setExpiryMenuItem = control.TradeDetailsGrid.ContextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Expire Estimate Commitment");
				AssertNotNull(setExpiryMenuItem);
				var undoExpiryMenuItem = control.TradeDetailsGrid.ContextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Undo Estimate Commitment Expiry");
				AssertNotNull(undoExpiryMenuItem);

				control.TradeDetailsGrid.UnSelectAll();
				control.TradeDetailsGrid.ListManager.Position = control.TradeDetailsGrid.List.IndexOf(tradeDetail2);
				UnitTestUserNotification.Instance.AddOKAnswer();
				setExpiryMenuItem.PerformClick();
				AssertEquals("Please select at least one committed value.", UnitTestUserNotification.Instance.LastMessage.Text);

				control.TradeDetailsGrid.SelectAllElements(x => x == tradeDetail1 || x == tradeDetail2);
				setExpiryMenuItem.PerformClick();
				AssertEquals(typeof(EstimateValueExpiryForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
				ZFormModaliser.LastFormShownDialogForTest = null;

				control.TradeDetailsGrid.UnSelectAll();
				control.TradeDetailsGrid.ListManager.Position = control.TradeDetailsGrid.List.IndexOf(tradeDetail1);
				UnitTestUserNotification.Instance.AddOKAnswer();
				undoExpiryMenuItem.PerformClick();
				AssertEquals("Please select at least one expired value.", UnitTestUserNotification.Instance.LastMessage.Text);

				control.TradeDetailsGrid.SelectAllElements(x => x == tradeDetail1 || x == tradeDetail3);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				undoExpiryMenuItem.PerformClick();
				AssertEquals("You are about to undo expiry on selected estimate values. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
