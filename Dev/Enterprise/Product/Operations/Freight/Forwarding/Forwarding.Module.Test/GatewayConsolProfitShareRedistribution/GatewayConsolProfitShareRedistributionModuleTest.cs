using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(GatewayConsolProfitShareRedistributionModule))]
	public class GatewayConsolProfitShareRedistributionModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.GatewayConsolProfitShareRedistribution;

		public void TestActionMenu_ExportToExcel()
		{
			using (var module = new GatewayConsolProfitShareRedistributionModule())
			{
				var actionsMenuItem = module.FormActionMenu.FindByText("D&ata Transfer");
				var menuItem = actionsMenuItem.MenuItems.FindByText("Export Selected Profit Share Redistribution To Excel");
				AssertNotNull("Menu item should be available", menuItem);
			}
		}
	}
}
