using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class OverrideFulfillmentRuleMenuItemTest : TestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertEquals("Override Fulfillment Rule", MenuItem.Text);
			MenuItem.PerformClick();
			AssertEquals(true, eventFired);
		}

		#endregion

		#region TestUpdateTextAndEnabledStateForSelectedOrders

		public void TestUpdateEnabledStateAndTextForSelectedOrders()
		{
			var orderWithFulfillmentRule = Factory.New<WhsOrder>();
			orderWithFulfillmentRule.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			var orderWithoutFulfillmentRule = Factory.New<WhsOrder>();
			orderWithoutFulfillmentRule.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.None;

			MenuItem.UpdateEnabledStateForSelectedOrders(new WhsOrder[] { orderWithFulfillmentRule });
			AssertEquals(true, MenuItem.Enabled);

			MenuItem.UpdateEnabledStateForSelectedOrders(new WhsOrder[] { orderWithoutFulfillmentRule });
			AssertEquals(false, MenuItem.Enabled);

			MenuItem.UpdateEnabledStateForSelectedOrders(new WhsOrder[] { orderWithFulfillmentRule, orderWithoutFulfillmentRule });
			AssertEquals(true, MenuItem.Enabled);

			MenuItem.UpdateEnabledStateForSelectedOrders(Array.Empty<WhsOrder>());
			AssertEquals(false, MenuItem.Enabled);
		}

		#endregion

		OverrideFulfillmentRuleMenuItem MenuItem => menuItem ?? (menuItem = new OverrideFulfillmentRuleMenuItem((sender, e) => { eventFired = true; }));
		OverrideFulfillmentRuleMenuItem menuItem;

		bool eventFired;
	}
}
