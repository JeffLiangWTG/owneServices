using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public abstract class InventoryModuleWithoutOperationalActionsTest : ZModuleBasherTest
	{
		#region TestGridCollection

		public void TestGridCollection()
		{
			using (var module = GetInventoryModuleForTest())
			{
				AssertEquals(typeof(WhsModuleInventoryCollection), module.GridCollection.GetType());
			}
		}

		#endregion

		#region TestAddExportMenuItems

		[TestDate(2015, 10, 21)]
		public void TestAddExportMenuItems()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "D&ata Transfer", "Export To &XML");
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		#endregion

		#region TestXMLExportIsInvoked

		[TestDate(2015, 10, 21)]
		public void TestXMLExportIsInvoked()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (var module = (InventoryModuleWithoutOperationalActions)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var menuItems = module.FormActionMenu.FindByText("&Actions").MenuItems.Cast<MenuItem>();
				var menu = menuItems.FindByText("D&ata Transfer").MenuItems.FindByText("Export To &XML");
				menu.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Select one or more items to create the XML file for."));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		#endregion

		#region TestExportToExcel

		#region TestExportToExcel_DBHits

		public void TestExportToExcel_DBHits()
		{
			// create inventory records in otherFactory
			var otherFactory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(otherFactory);
			const int NumberOfUniqueRecordsOfEachType = 3; // will create 3x3x3x3 = 81 inventories
			for (int whsNo = 0; whsNo < NumberOfUniqueRecordsOfEachType; whsNo++)
			{
				var whs = helper.CreateWarehouse("WH" + whsNo);

				for (int clientNo = 0; clientNo < NumberOfUniqueRecordsOfEachType; clientNo++)
				{
					var client = helper.CreateClient(string.Format("CLIENT{0}{1}", whsNo, clientNo));
					var receive = helper.CreateWhsReceive(client, whs, string.Format("R{0}{1}", whsNo, clientNo));

					for (int productNo = 0; productNo < NumberOfUniqueRecordsOfEachType; productNo++)
					{
						var product = helper.CreateProduct(client, string.Format("Product{0}{1}{2}", whsNo, clientNo, productNo));
						var locations = helper.CreateRowAndGenerateLocations(whs, string.Format("ROW{0}{1}{2}", whsNo, clientNo, productNo), NumberOfUniqueRecordsOfEachType, 1).Locations;

						for (int locationNo = 0; locationNo < NumberOfUniqueRecordsOfEachType; locationNo++)
						{
							helper.CreateWhsReceiveInventoryLine(receive, product, 10m, locations[locationNo]);
						}
					}
					receive.FinaliseDocketWithoutUserConfirmation();
					WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
				}
			}
			otherFactory.Save();
			using (var module = (InventoryModuleWithoutOperationalActions)ZModuleFactory.Instance.Create(GetModuleID()))
			using (var form = module.ShowPopup())
			{
				// search inventory module grid without any filters, should find all previously created inventories
				var control = ((Control)form).FindSingle<ZFilterStripCommonControl>();
				control.FirePerformSearch();
				AssertEquals("Precondition - ensure all inventory records were loaded.", (int)Math.Pow(NumberOfUniqueRecordsOfEachType, 4), module.GridCollection.Count);

				// export found inventories into excel
				var menuItems = module.FormActionMenu.FindByText("&Actions").MenuItems.Cast<MenuItem>();
				var menu = menuItems.FindByText("D&ata Transfer").MenuItems.FindByText("Export All Columns To Excel");
				try
				{
					using (RowFactory.SetCachedTables())
					{
						menu.PerformClick();
					}

					using (var excelInterface = ExcelInterfaceFactory.New())
					{
						excelInterface.LoadExcelFile(ExcelExporter.LastExportedFileNameStaticForTest);
						using (var workSheet = excelInterface.WorkSheets[0])
						{
							AssertEquals("Precondition - ensure all inventory records were exported", 1 + (int)Math.Pow(NumberOfUniqueRecordsOfEachType, 4), workSheet.RowCount); // header + 81 inventories
						}
					}
				}
				finally
				{
					if (!string.IsNullOrEmpty(ExcelExporter.LastExportedFileNameStaticForTest) && File.Exists(ExcelExporter.LastExportedFileNameStaticForTest))
					{
						File.Delete(ExcelExporter.LastExportedFileNameStaticForTest);
					}
				}
				AssertDbHits(ExpectedDBHits, module.GridCollection.Factory);
			}
		}

		protected virtual Dictionary<string, int> ExpectedDBHits
		{
			get
			{
				var expectedDBHits = new Dictionary<string, int>();
				expectedDBHits.Add(CusClassPartPivotSchema.Constants.TableName, 1);
				expectedDBHits.Add(OrgHeaderSchema.Constants.TableName, 1);
				expectedDBHits.Add(OrgSupplierPartSchema.Constants.TableName, 1);
				expectedDBHits.Add(StmNoteSchema.Constants.TableName, 2);
				expectedDBHits.Add(UNDGDataItemSchema.Constants.TableName, 1);
				expectedDBHits.Add(WhsAreaSchema.Constants.TableName, 2);
				expectedDBHits.Add(WhsBondedWarehouseAttributeSchema.Constants.TableName, 2);
				expectedDBHits.Add(WhsDocketSchema.Constants.TableName, 1);
				expectedDBHits.Add(WhsDocketLineSchema.Constants.TableName, 2);
				expectedDBHits.Add(WhsInventoryViewSchema.Constants.TableName, 1);
				expectedDBHits.Add(WhsLocationTypeSchema.Constants.TableName, 1);
				expectedDBHits.Add(WhsLocationViewSchema.Constants.TableName, 2);
				expectedDBHits.Add(WhsPickLineSchema.Constants.TableName, 2);
				expectedDBHits.Add(WhsWarehouseSchema.Constants.TableName, 2);
				return expectedDBHits;
			}
		}

		#endregion

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			var moduleID = GetModuleID();
			using (var module = (InventoryModuleWithoutOperationalActions)ZModuleFactory.Instance.Create(moduleID))
			{
				AssertEquals(moduleID, module.ID);
			}
		}

		#endregion

		#region TestLicenseCheckPoint

		public void TestLicenseCheckPoint()
		{
			using (var module = (InventoryModuleWithoutOperationalActions)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerCoreAnd4PL, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region TestSecurityCheckPoint

		public void TestSecurityCheckPoint()
		{
			using (var module = (InventoryModuleWithoutOperationalActions)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsInventory, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region TestControllersDefinedForAllCountriesModuleDefinedOn

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("No controller available for this module", true);
		}

		#endregion

		#region TestAllowDelete

		public void TestAllowDelete()
		{
			using (var module = GetInventoryModuleForTest())
			{
				Assert("Delete option should not be available", !module.AllowDelete);
			}
		}

		#endregion

		#region TestAllowNew

		public void TestAllowNew()
		{
			using (var module = GetInventoryModuleForTest())
			{
				Assert("New option should not be available", !module.AllowNew);
			}
		}

		#endregion

		#region TestCommittedStockMenuItem

		public void TestCommittedStockMenuItem()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			using (var module = (InventoryModuleWithoutOperationalActions)ZModuleFactory.Instance.Create(ModuleIDs.WhsInventory))
			{
				module.FormActionMenu.FindByText("Committed...").PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(InventoryModuleWithoutOperationalActions.NoCommittedItemSelectedErrorMsg));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		#endregion

		#region TestGetNewActionMenu

		public void TestGetNewActionMenu()
		{
			using (var module = (InventoryModuleWithoutOperationalActions)ZModuleFactory.Instance.Create(ModuleIDs.WhsInventory))
			{
				Menu.MenuItemCollection actionMenuItems = module.FormActionMenu.FindByText("&Actions").MenuItems;
				AssertMenuItemExists(module.FormActionMenu, "View Receipt", true);
				AssertMenuItemExists(module.FormActionMenu, "Committed...", true);
			}
		}

		void AssertMenuItemExists(IEnumerable menuItems, string name, bool exists)
		{
			foreach (MenuItem item in menuItems)
			{
				if (item.Text == name)
				{
					if (!exists)
					{
						Fail("Menuitem " + name + " should not exist");
					}
					else
					{
						Assert(true);
					}

					return;
				}
			}
			Fail("Menuitem " + name + " not found");
		}

		#endregion

		#region Implementation

		protected abstract InventoryModuleWithoutOperationalActions GetInventoryModuleForTest();

		#endregion
	}
}
