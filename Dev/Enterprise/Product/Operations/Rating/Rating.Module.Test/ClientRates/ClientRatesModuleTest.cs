using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(ClientRatesModule))]
	public class ClientRatesModuleTest : ZModuleBasherTest
	{
		public void TestSupportsWorkflow()
		{
			using (var module = new ClientRatesModule())
			{
				Assert(module.SupportsWorkflow);
			}
		}

		public void TestBusinessContexts()
		{
			using (ClientRatesModule module = new ClientRatesModule())
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

			using (var module = new ClientRatesModule())
			{
				var found = module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems.FindByText("Data Transfer").MenuItems.Cast<MenuItem>().Any(item => item.Text == "Import From &XML");
				AssertEquals("Should find the Import From XML menu Item", true, found);
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		public void TestNewGlobalClientRateMenu()
		{
			using (var module = new ClientRatesModuleForTest())
			{
				var menuItems = module.GetNewStandardMenuItemsForTest();
				var newMenu = menuItems.FirstOrDefault(m => m.Text == "&New").MenuItems.Cast<MenuItem>();
				var menuItemNewClientRate = newMenu.FirstOrDefault(m => m.Text == "New Client Rate (Default)");
				AssertNotNull(menuItemNewClientRate);

				menuItems = module.GetNewStandardMenuItemsForTest();
				newMenu = menuItems.FirstOrDefault(m => m.Text == "&New").MenuItems.Cast<MenuItem>();
				var menuItemNewGlobalClientRate = newMenu.FirstOrDefault(m => m.Text == "New Global Client Rate");
				AssertNotNull("Registry is enabled - should allow Global Client Rate", menuItemNewGlobalClientRate);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ClientRates;
		}
	}

	class ClientRatesModuleForTest : ClientRatesModule
	{
		public MenuItem[] GetNewStandardMenuItemsForTest()
		{
			return GetNewStandardMenuItems();
		}
	}
}
