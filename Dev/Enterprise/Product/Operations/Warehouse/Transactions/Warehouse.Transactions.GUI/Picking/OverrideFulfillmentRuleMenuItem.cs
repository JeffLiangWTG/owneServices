using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class OverrideFulfillmentRuleMenuItem : MenuItem
	{
		public OverrideFulfillmentRuleMenuItem(EventHandler onClick)
			: base(Res.GetString("882f875c-4d25-4d0f-a4da-07c9106d7fbb", "Override Fulfillment Rule"), onClick)
		{
		}

		public void UpdateEnabledStateForSelectedOrders(WhsOrder[] selectedOrders)
		{
			Enabled = IsFulfillmentRuleSetInSelectedOrders(selectedOrders);
		}

		bool IsFulfillmentRuleSetInSelectedOrders(WhsOrder[] selectedOrders)
		{
			foreach (WhsOrder order in selectedOrders)
			{
				if (order.WD_WhsOrderFulfillmentRule == WhsOrderFulfillmentRuleList.Codes.All)
				{
					return true;
				}
			}
			return false;
		}
	}
}
