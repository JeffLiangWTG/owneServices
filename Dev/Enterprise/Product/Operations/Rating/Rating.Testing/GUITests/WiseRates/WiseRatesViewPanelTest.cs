using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Test
{
	public class WiseRatesViewPanelTest : TestCase
	{
		public void TestTag()
		{
			using (var panel = new WiseRatesViewPanel())
			{
				AssertEquals("The category should be RatesServiceCategory.", RatingConstants.RateCategory.RatesServiceCategory, panel.Category);
			}
		}

		public void TestContextMenuItemsInWiseEntryGrid()
		{
			using (var panel = new WiseRatesViewPanel())
			{
				var menuItems = panel.RateEntryGrid.ContextMenu.MenuItems;

				var showCommodityGroupsMenuItem = menuItems.FindByText("Show Commodity Groups");
				AssertNotNull("there should be a menu item for quick commodity group mapping", showCommodityGroupsMenuItem);

				var assignGlobalChargeCodeMenuItem = menuItems.FindByText("Assign Universal Charge Code to Charge Code");
				AssertNotNull("there should be a menu item for quick charge code mapping", assignGlobalChargeCodeMenuItem);

				var assignLocalChargeCodeMenuItem = menuItems.FindByText("Assign Universal Charge Code to Global Charge Code");
				AssertNotNull("there should be a menu item for quick global charge code mapping", assignLocalChargeCodeMenuItem);

				var assignCarrierCodeMenuItem = menuItems.FindByText("Assign SCAC/C1C to Carrier");
				AssertNotNull("there should be a menu item for a general quick SCAC/C1C code mapping", assignCarrierCodeMenuItem);

				var wiseRatesAssignCarrierServiceLevelsMenuItem = menuItems.FindByText("Assign Carrier Service Level(s) to Carrier");
				AssertNotNull("wiseRatesAssignCarrierServiceLevelsMenuItem", wiseRatesAssignCarrierServiceLevelsMenuItem);
			}
		}
	}
}
