using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(CompanyTariffsModule))]
	class CompanyTariffsModuleTest : ZModuleBasherTest
	{
		public void TestSupportsWorkflow()
		{
			using (var module = new CompanyTariffsModule())
			{
				Assert(module.SupportsWorkflow);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlobalRates;
		}

		[TestDate(2015, 10, 21)]
		public void TestImportFromXmlMenuItem()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (var module = new CompanyTariffsModule())
			{
				var found = module.ToolBarButtons.Where(button => button.Text.Equals("Actions")).
					Any(button => button.DropDownMenu.MenuItems.Cast<MenuItem>().
					Where(actionItem => actionItem.Text.Equals("D&ata Transfer")).
					Any(actionItem => actionItem.MenuItems.Cast<MenuItem>().
					Any(item => item.Text == "Import From &XML")));
				AssertEquals("Should find the Import From XML menu Item", true, found);
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		public void TestNewCompanyTariffMenu()
		{
			using (var module = new CompanyTariffsModuleForTest())
			{
				var menuItems = module.GetNewStandardMenuItemsForTest();
				var newMenu = menuItems.FirstOrDefault(m => m.Text == "&New").MenuItems.Cast<MenuItem>();
				var menuItemNewCompanyTariff = newMenu.FirstOrDefault(m => m.Text == "New Company Tariff (Default)");
				AssertNotNull(menuItemNewCompanyTariff);

				var menuItemNewGlobalTariff = newMenu.FirstOrDefault(m => m.Text == "New Global Tariff");
				AssertNotNull("Global Tariff should be visible", menuItemNewGlobalTariff);
			}
		}
	}

	class CompanyTariffsModuleForTest : CompanyTariffsModule
	{
		public MenuItem[] GetNewStandardMenuItemsForTest()
		{
			return GetNewStandardMenuItems();
		}
	}
}
