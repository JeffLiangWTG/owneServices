using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.GUI.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using GUITestHelper = Enterprise.Warehouse.Transactions.GUI.Testing.GUITestHelper;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	class InventoryFilterUserControlTest : WhsGuiTestCaseWithFactory
	{
		#region Constructors

		public void TestConstructionWithDocket()
		{
			var transfer = Factory.New<WhsTransfer>();
			using (var userControl = new InventoryFilterUserControl(transfer))
			{
				var inventoryFilterControl = GUITestHelper.FindControl<InventoryFilterControl>(userControl.Controls, "InventoryFilterControl");
				AssertNotNull("Accept MenuItem should be added", inventoryFilterControl.FilteredGrid.ContextMenu.MenuItems.ContainsKey("Accept"));
			}
		}

		#endregion

		#region TestOnAcceptMenuItemClick

		public void TestOnAcceptMenuItemClick()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 10m, 20m, 30m });

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "1", Notify);

			using (var form = new TestForm(transfer))
			{
				form.Show();

				((ModuleGuidFilter)form.InventoryFilterControl.FilterBusinessObject.ModuleFilters[InventoryFilterBusinessObject.Schema.Client]).Property = ZGuid.Empty;
				((ModuleGuidFilter)form.InventoryFilterControl.FilterBusinessObject.ModuleFilters[InventoryFilterBusinessObject.Schema.Warehouse]).Property = ZGuid.Empty;

				AssertEquals("Precondition, transfer should be empty from any lines", 0, transfer.Lines.Count);

				form.UserControl.OnAcceptMenuItemClick(null, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Please select Inventory lines you would like to Accept first"));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);

				form.InventoryFilterControl.FirePerformSearch();

				form.InventoryFilterControl.FilteredGrid.Select(0);
				form.InventoryFilterControl.FilteredGrid.Select(2);

				form.UserControl.OnAcceptMenuItemClick(null, EventArgs.Empty);
				AssertEquals("Transfer should have accepted 2 lines", 2, transfer.Lines.Count);
			}
		}

		#endregion

		#region TestOnAcceptMenuItemClick_CopyCustomsData

		public void TestOnAcceptMenuItemClick_CopyCustomsData()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_DocketSubType = "CUS";
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Today.AddDays(+2), ZDate.Today.AddDays(-5), "PA11", "PA21", "PA31", "BEK-11");
			inventory.CustomsData.WB_AddInfo = "Hello";

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			order.WD_DocketSubType = OrderType.Codes.Customs;
			using (var form = new TestForm(order))
			{
				form.Show();

				((ModuleGuidFilter)form.InventoryFilterControl.FilterBusinessObject.ModuleFilters[InventoryFilterBusinessObject.Schema.Client]).Property = ZGuid.Empty;
				((ModuleGuidFilter)form.InventoryFilterControl.FilterBusinessObject.ModuleFilters[InventoryFilterBusinessObject.Schema.Warehouse]).Property = ZGuid.Empty;

				AssertEquals("Precondition, transfer should be empty from any lines", 0, order.Lines.Count);

				form.InventoryFilterControl.FirePerformSearch();
				form.InventoryFilterControl.FilteredGrid.Select(0);
				form.UserControl.OnAcceptMenuItemClick(null, EventArgs.Empty);
				AssertEquals("Transfer should have accepted 1 lines", 1, order.Lines.Count);
				AssertEquals("CustomsData.WB_AddInfo", "Hello", order.Lines[0].CustomsData.WB_AddInfo);
			}
		}

		#endregion

		#region TestWarehouseAndClientChangeUpdatesFilter

		public void TestWarehouseAndClientChangeUpdatesFilter()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory();
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);

			using (var form = new TestForm(transfer))
			{
				form.Show();

				AssertEquals("Editing a saved Docket should update the Client filter.", data.Org1.PK, form.ClientModuleFilter.Property);
				AssertEquals("Editing a saved Docket should update the Warehouse filter.", data.Whs1.PK, form.WarehouseModuleFilter.Property);

				transfer.WD_OH_Client = data.Org2.PK;
				AssertEquals(data.Org2.PK, form.ClientModuleFilter.Property);
				AssertEquals(data.Whs1.PK, form.WarehouseModuleFilter.Property);

				transfer.WD_WW_Whs = data.Whs2.PK;
				AssertEquals(data.Org2.PK, form.ClientModuleFilter.Property);
				AssertEquals(data.Whs2.PK, form.WarehouseModuleFilter.Property);
			}

			// test the case where a filter layout exists (the load of the layout will throw away the existing module filters)
			using (var form = new TestForm(transfer))
			{
				transfer.WD_OH_Client = data.Org1.PK;
				transfer.WD_WW_Whs = data.Whs1.PK;

				form.Show();
				form.FilterBizO.LoadLayout(null);

				AssertEquals("Editing a saved Docket should update the Client filter.", data.Org1.PK, form.ClientModuleFilter.Property);
				AssertEquals("Editing a saved Docket should update the Warehouse filter.", data.Whs1.PK, form.WarehouseModuleFilter.Property);
			}
		}

		#endregion

		#region TestInventoryViewFetchForView

		public void TestInventoryFetchForView()
		{
			const int numberOfInventoriesToCreate = 10;
			var data = new TestDataSimpleEnvironment(Factory, numberOfInventoriesToCreate, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Core.Constants.CountryCodes.UnitedStates).RL_Code;
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			AssertEquals("Precondition", Constants.CountryCodes.UnitedStates, receive.CountryCode);

			for (int i = 0; i < numberOfInventoriesToCreate; i++)
			{
				var part = Helper.CreateProduct(data.Org1, "PR" + i);
				Helper.CreateClient("P" + i, "Client" + i);
				var location = data.Whs1.FindLocation("A-" + (i + 1));
				var line = (WhsReceiveLine)Helper.CreateWhsReceiveInventoryLine(receive, part, 1m, location).InDocketLine;
				line.ConsigneeDocAddress.OrganisationPK = Helper.CreateClient("C" + i, "CNE" + i).PK;
			}

			receive.AllocateLocationsWithMock();
			Factory.Save();

			List<TableColumn> tableColumns;
			using (var form = new TestForm(receive))
			{
				form.Show();
				var grid = form.InventoryFilterControl.Grid;
				grid.SetAllColumnsVisible(true);
				var inventory = (WhsInventoryViewCollection)grid.DataSource;
				tableColumns = new TableColumnCalculator().GetTableColumnsOnThisObject(inventory[0], grid.Columns).ToList();
			}

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInOtherFactory = newFactory.Load<WhsReceive>(receive.PK);

			foreach (var inventory in receiveInOtherFactory.Inventory)
			{
				inventory.FetchStrategy.FetchForView(tableColumns.ToArray());
			}

			using (RowFactory.SetCachedTables())
			{
				foreach (WhsInventoryView inventory in receiveInOtherFactory.Inventory)
				{
					PokeColumnsProperties(inventory, tableColumns);
				}
			}

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ StmNoteSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ CusClassPartPivotSchema.Constants.TableName, 2 },
				{ WhsBondedWarehouseAttributeSchema.Constants.TableName, 1 },
			};
			AssertDbHits(expectedDbHits, newFactory);
		}

		void PokeColumnsProperties(WhsInventoryView inventory, List<TableColumn> tableColumns)
		{
			var properties = inventory.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (var property in properties.Where(p => p.Name != "Item" && tableColumns.Any(c => c.ColumnName.Contains(p.Name))))
			{
				var poke = property.GetValue(inventory, null);
			}
		}

		#endregion

		#region TesInventoryFilterUserControlWithUserDefinedFilter

		public void TestInventoryFilterUserControlWithUserDefinedFilter()
		{
			var adjustment = Factory.New<WhsAdjustment>();
			using (var form = new TestForm(adjustment))
			{
				FilterStripsTestHelper.SaveFilterLayout(form.FilterBizO, "My UserDefined Filter", false, false, true);
			}

			AssertNoExceptionThrown(() =>
			{
				using (var form = new TestForm(adjustment))
				{
					form.Show();
				}
			});
		}

		#endregion

		#region TestForm class

		class TestForm : ZForm
		{
			public TestForm(WhsDocket docket)
				: base(docket)
			{
				UserControl = new InventoryFilterUserControl(docket);
				Controls.Add(this.UserControl);
				InventoryFilterControl = GUITestHelper.FindControl<InventoryFilterControl>(UserControl.Controls, "InventoryFilterControl");
			}

			public readonly InventoryFilterUserControl UserControl;
			public readonly InventoryFilterControl InventoryFilterControl;

			public InventoryFilterBusinessObject FilterBizO => (InventoryFilterBusinessObject)InventoryFilterControl.FilterBusinessObject;

			public ModuleGuidFilter ClientModuleFilter => (ModuleGuidFilter)FilterBizO.ModuleFilters[InventoryFilterBusinessObject.Schema.Client];

			public ModuleGuidFilter WarehouseModuleFilter => (ModuleGuidFilter)FilterBizO.ModuleFilters[InventoryFilterBusinessObject.Schema.Warehouse];
		}

		#endregion
	}
}
