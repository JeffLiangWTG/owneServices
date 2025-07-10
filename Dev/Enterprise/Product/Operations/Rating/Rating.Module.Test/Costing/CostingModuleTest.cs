using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(CostingModule))]
	public class CostingModuleTest : ZModuleBasherTest
	{
		public void TestBusinessContexts()
		{
			using (CostingModule module = new CostingModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("Rating business context should be returned", BusinessContext.Rating, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		[TestDate(2015, 10, 21)]
		public void TestImportFromXmlMenuItem()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (var module = new CostingModule())
			{
				var found = module.ToolBarButtons.Where(button => button.Text.Equals("Actions")).
					Any(button => button.DropDownMenu.MenuItems.Cast<MenuItem>().
					Any(actionItem => actionItem.Text.Equals("D&ata Transfer") && actionItem.MenuItems.Cast<MenuItem>().
					Any(item => item.Text == "Import From &XML")));

				AssertEquals("Should find the Import From XML menu Item", true, found);
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		public void TestNewGlobalCostingMenu()
		{
			using (var module = new CostingModuleForTest())
			{
				var menuItems = module.GetNewStandardMenuItemsForTest();
				var newMenu = menuItems.FirstOrDefault(m => m.Text == "&New").MenuItems.Cast<MenuItem>();
				var menuItemNewCosting = newMenu.FirstOrDefault(m => m.Text == "New Costing (Default)");
				AssertNotNull(menuItemNewCosting);

				var menuItemNewGlobalCosting = newMenu.FirstOrDefault(m => m.Text == "New Global Costing");
				AssertNotNull("Registry is enabled - should allow Global Costing", menuItemNewGlobalCosting);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Costing;
		}
	}

	class CostingModuleForTest : CostingModule
	{
		public MenuItem[] GetNewStandardMenuItemsForTest()
		{
			return GetNewStandardMenuItems();
		}
	}
}
