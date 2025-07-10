using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Module.Testing
{
	[TestedType(typeof(LoadListConsolModule))]
	sealed class LoadListConsolModuleBasherTest : ZModuleBasherTest
	{
		public void TestBusinessContexts()
		{
			using (LoadListConsolModule module = new LoadListConsolModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("CFSLoadList business context should be returned", BusinessContext.CFSLoadList, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.LoadListConsol;
		}

		public void TestExportToXMLMenuItemExists()
		{
			AssertImportExportMenuExists("Export To &XML");
		}

		public void TestImportFromXMLMenuItemExists()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			AssertImportExportMenuExists("Import From &XML");
		}

		[TestDate(2015, 10, 21)]
		void AssertImportExportMenuExists(string menuText)
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (var module = (LoadListConsolModule)ZModuleFactory.Instance.Create(ModuleIDs.LoadListConsol))
			{
				var menuItemFound = false;
				foreach (MenuItem actionMenu in module.FormActionMenu.FindByText("&Actions").MenuItems)
				{
					if (actionMenu.Text == "D&ata Transfer")
					{
						if (actionMenu.MenuItems.Cast<MenuItem>().Any(item => item.Text == menuText))
						{
							menuItemFound = true;
						}
					}
				}

				AssertEquals("Menu item " + menuText + " should be present", true, menuItemFound);
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		public void TestSetGuiProviders()
		{
			using (LoadListConsolModuleForTest module = new LoadListConsolModuleForTest())
			{
				module.GetNewGridCollection();

				var consolDocumentSupporterQueryProvider = module.Factory.GetValue<ICommonConsolDocumentSupporterQueryProvider>();
				AssertNotNull(consolDocumentSupporterQueryProvider);
				Assert(consolDocumentSupporterQueryProvider is ConsolDocumentSupporterGuiQueryProvider);

				var shipmentDocumentSupporterQueryProvider = module.Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>();
				AssertNotNull(shipmentDocumentSupporterQueryProvider);
				Assert(shipmentDocumentSupporterQueryProvider is ShipmentDocumentSupporterGuiQueryProvider);

				var servicesSelectionProvider = module.Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull(servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);

				module.PerformSearch();

				consolDocumentSupporterQueryProvider = module.Factory.GetValue<ICommonConsolDocumentSupporterQueryProvider>();
				AssertNotNull(consolDocumentSupporterQueryProvider);
				Assert(consolDocumentSupporterQueryProvider is ConsolDocumentSupporterGuiQueryProvider);

				shipmentDocumentSupporterQueryProvider = module.Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>();
				AssertNotNull(shipmentDocumentSupporterQueryProvider);
				Assert(shipmentDocumentSupporterQueryProvider is ShipmentDocumentSupporterGuiQueryProvider);

				servicesSelectionProvider = module.Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull(servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);
			}
		}

		#region Implementation

		class LoadListConsolModuleForTest : LoadListConsolModule
		{
			public new BusinessObjectFactory Factory
			{
				get { return base.Factory; }
			}

			public new IBusinessObjectCollection GetNewGridCollection()
			{
				return base.GetNewGridCollection();
			}

			public void PerformSearch()
			{
				base.PerformSearch();
			}
		}

		#endregion
	}
}
