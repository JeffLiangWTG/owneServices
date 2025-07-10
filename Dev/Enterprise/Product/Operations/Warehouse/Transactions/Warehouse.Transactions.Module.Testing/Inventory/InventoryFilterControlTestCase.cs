using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.Module.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	class InventoryFilterControlTestCase : WhsTestCaseWithFactory
	{
		public void TestSerialNumberColumnInitialised()
		{
			var org = Helper.CreateClient("TESTCO", "TESTCO");
			var whs = Helper.CreateWarehouse("1", "A", 2, 2);
			var transfer = Helper.CreateWhsTransfer(org, whs, "T11", Notify);

			using (var form = new ZForm(transfer))
			using (var inventoryFilterControl = new InventoryFilterControl(transfer))
			{
				form.Controls.Add(inventoryFilterControl);
				form.Show();

				AssertEquals(false,
					inventoryFilterControl.Grid.ColumnStyles.Cast<ZGridColumnInfo>().Single(c => c.ColumnName == WhsInventoryViewSchema.Constants.WI_SerialNumber).IsUnavailable);
			}
		}

		public void TestClientChanged()
		{
			var org = Helper.CreateClient("TESTCO", "TESTCO");
			org.MiscServ.OM_IMPartAttrib1Name = "Batch #";
			org.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.BatchNumber;
			org.MiscServ.OM_IMPartAttrib2Name = "Vehicle #";
			org.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.VIN;
			org.MiscServ.OM_IMPartAttrib3Name = "Colour";
			org.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;
			org.MiscServ.OM_IMUseExpiryDate = true;
			org.MiscServ.OM_IMUsePackingDate = true;

			var whs = Helper.CreateWarehouse("1", "A", 2, 2);
			var transfer = Helper.CreateWhsTransfer(org, whs, "T11", Notify);
			Factory.Save();

			using (var form = new ZForm(transfer))
			{
				using (var inventoryFilterControl = new InventoryFilterControl(transfer))
				{
					form.Controls.Add(inventoryFilterControl);
					form.Show();

					AssertAttributeVisibility(inventoryFilterControl.Grid, expiry: false, packing: false, part1: false, part2: false, part3: false);

					var filter = (InventoryFilterBusinessObject)inventoryFilterControl.FilterBusinessObject;
					var clientFilter = (ModuleGuidFilter)filter.ModuleFilters[InventoryFilterBusinessObject.Schema.Client];
					clientFilter.Property = org.PK;

					AssertAttributeVisibility(inventoryFilterControl.Grid, expiry: true, packing: true, part1: true, part2: true, part3: true);
					AssertAttributeTitles(inventoryFilterControl.Grid, "Batch #", "Vehicle #", "Colour");

					AssertWBColumn(inventoryFilterControl.Grid, WhsBondedWarehouseAttributeSchema.WB_CustomsDeadline.Name, null, false);
					AssertWBColumn(inventoryFilterControl.Grid, WhsBondedWarehouseAttributeSchema.WB_InwardStyle.Name, null, false);
					AssertWBColumn(inventoryFilterControl.Grid, WhsBondedWarehouseAttributeSchema.WB_InwardProcedure.Name, null, false);
				}
			}
		}

		public void TestClientChanged_SerialNumber()
		{
			var org = Helper.CreateClient("TESTCO", "TESTCO");
			org.MiscServ.OM_IMUseSerialNumber = true;

			var whs = Helper.CreateWarehouse("1", "A", 2, 2);
			var transfer = Helper.CreateWhsTransfer(org, whs, "T11", Notify);
			Factory.Save();

			using (var form = new ZForm(transfer))
			{
				using (var inventoryFilterControl = new InventoryFilterControl(transfer))
				{
					form.Controls.Add(inventoryFilterControl);
					form.Show();

					AssertAttributeVisibility(inventoryFilterControl.Grid, expiry: false, packing: false, part1: false, part2: false, part3: false);
					AssertEquals("Serial visibility incorrect", false, inventoryFilterControl.Grid.Columns[WhsInventoryViewSchema.WI_SerialNumber.Name].IsVisible);

					var filter = (InventoryFilterBusinessObject)inventoryFilterControl.FilterBusinessObject;
					var clientFilter = (ModuleGuidFilter)filter.ModuleFilters[InventoryFilterBusinessObject.Schema.Client];
					clientFilter.Property = org.PK;

					AssertAttributeVisibility(inventoryFilterControl.Grid, expiry: false, packing: false, part1: false, part2: false, part3: false);
					AssertEquals("Serial visibility incorrect", true, inventoryFilterControl.Grid.Columns[WhsInventoryViewSchema.WI_SerialNumber.Name].IsVisible);
				}
			}

			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var form = new ZForm(transfer))
				{
					using (var inventoryFilterControl = new InventoryFilterControl(transfer))
					{
						form.Controls.Add(inventoryFilterControl);
						form.Show();

						AssertEquals("WI_SerialNumber is unavailable when EnableSchemaRedesignChanges is true.",
							false, inventoryFilterControl.Grid.Columns.Contains(WhsInventoryViewSchema.WI_SerialNumber.Name));
					}
				}
			}
		}

		void AssertAttributeVisibility(ZGrid grid, bool expiry, bool packing, bool part1, bool part2, bool part3)
		{
			AssertEquals("Expiry visibility incorrect", expiry, grid.Columns[WhsInventoryViewSchema.WI_ExpiryDate.Name].IsVisible);
			AssertEquals("Packing visibility incorrect", packing, grid.Columns[WhsInventoryViewSchema.WI_PackingDate.Name].IsVisible);
			AssertEquals("PartAttrib1 visibility incorrect", part1, grid.Columns[WhsInventoryViewSchema.WI_PartAttrib1.Name].IsVisible);
			AssertEquals("PartAttrib2 visibility incorrect", part2, grid.Columns[WhsInventoryViewSchema.WI_PartAttrib2.Name].IsVisible);
			AssertEquals("PartAttrib3 visibility incorrect", part3, grid.Columns[WhsInventoryViewSchema.WI_PartAttrib3.Name].IsVisible);
		}

		void AssertAttributeTitles(ZGrid grid, string part1, string part2, string part3)
		{
			AssertEquals("PartAttrib1 title incorrect", part1, grid.Columns[WhsInventoryViewSchema.WI_PartAttrib1.Name].ColumnStyle.HeaderText);
			AssertEquals("PartAttrib2 title incorrect", part2, grid.Columns[WhsInventoryViewSchema.WI_PartAttrib2.Name].ColumnStyle.HeaderText);
			AssertEquals("PartAttrib3 title incorrect", part3, grid.Columns[WhsInventoryViewSchema.WI_PartAttrib3.Name].ColumnStyle.HeaderText);
		}

		void AssertWBColumn(ZGrid grid, string columnName, string groupName, bool shouldVisible)
		{
			var column = grid.Columns[WhsInventoryView.Schema.CustomsData + "+" + columnName];
			AssertNotNull("Column \"" + columnName + "\" should exist.", column);
			AssertEquals(groupName, column.GroupName.Caption);
			AssertEquals(shouldVisible, column.IsVisible);
		}

		public void TestAllocationKey()
		{
			var org = Helper.CreateClient("TESTCO", "TESTCO");
			var whs = Helper.CreateWarehouse("1", "A", 2, 2);
			var product = Helper.CreateProduct("ABC", org);
			var receive = Helper.CreateWhsReceive(org, whs, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, product, 10m);

			using (var form = new ZForm(receive))
			using (var inventoryFilterControl = new InventoryFilterControl(receive))
			{
				form.Controls.Add(inventoryFilterControl);
				form.Show();

				Assert(!inventoryFilterControl.Grid.ColumnStyles.Cast<ZGridColumnInfo>().Single(c => c.ColumnName == WhsInventoryViewSchema.Constants.WI_AllocationKey).IsUnavailable);
			}
		}
	}

	class InventoryFilterControlDBHitsTest : WhsEnvFilterControlDBHitsTestCase<WhsModuleInventoryCollection, InventoryFilterBusinessObject>
	{
		#region TestInventoryGridWhenAddingAndDeletingRecords

		public void TestInventoryGridWhenAddingAndDeletingRecords()
		{
			Factory.RefreshEnabled = true;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.WhsInventory))
			using (var form = (ZForm)module.ShowPopup())
			{
				module.GridCollection.Factory.RefreshEnabled = true;
				var grid = module.DisplayGrid;
				grid.SetAllColumnsVisible(true);
				Application.DoEvents();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				filterControl.FirePerformSearch();
				Factory.Save();
				Application.DoEvents();
				module.DisplayGrid.Focus();
				AssertEquals("Precondition: One inventory must be added to collection via data refresh bus.",
					1, module.GridCollection.Count);

				line.Delete();
				Factory.Save();
				Application.DoEvents();
				module.DisplayGrid.Focus();
				AssertEquals("Inventory line must be deleted.", 0, module.GridCollection.Count);

				Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
				Factory.Save();
				Application.DoEvents();
				module.DisplayGrid.Focus();
				AssertEquals("New Inventory line must be shown to user via data refresh bus.",
					1, module.GridCollection.Count);
			}
		}

		#endregion

		#region SetupData

		protected override void SetupData()
		{
			for (int i = 0; i < 10; i++)
			{
				var client = Helper.CreateClient("C" + i);
				var whs = Helper.CreateWarehouse("W" + i, "A", 1, 1);
				var part = Helper.CreateProduct("P" + i, client);
				var whsProduct = WhsProduct.GetWhsProduct(part);

				var style = Helper.CreateProductStyle("PS" + i, "Product Style " + i, client.PK);
				var styleColour = Helper.CreateProductStyleColour(style, "CO" + i, "Colour " + i);
				var styleClassification = Helper.CreateProductStyleClassification(style, "GE" + i, "Classification " + i);
				var styleSize = Helper.CreateProductStyleSize(style, (ZByte)(i + 1), "SZ" + i);

				whsProduct.ProductStylePK = style.PK;
				whsProduct.ProductStyleColourPK = styleColour.PK;
				whsProduct.ProductStyleClassificationPK = styleClassification.PK;
				whsProduct.ProductStyleSizePK = styleSize.PK;

				var receive = Helper.CreateWhsReceive(client, whs, string.Format("R{0}", i.ToString()));

				for (int j = 0; j < 10; j++)
				{
					Helper.CreateWhsReceiveLine(receive, part, 10m, whs.DefaultLocation);
				}
				receive.FinaliseDocketWithoutUserConfirmation();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			}
		}

		#endregion

		protected override bool ShouldCheckForUnusedFetchHints(string columnName) => false;

		#region GetBaseHits

		protected override Dictionary<string, int> GetBaseHits()
		{
			var baseHits = new Dictionary<string, int>();
			// 1x hit for Collection.Load
			baseHits.Add(WhsInventoryViewSchema.Constants.TableName, 1);
			return baseHits;
		}

		#endregion

		#region GetExpectedHitsDictionary

		protected override Dictionary<string, Dictionary<string, int>> GetExpectedHitsDictionary()
		{
			var baseHits = GetBaseHits();
			var hitsDictionary = new Dictionary<string, Dictionary<string, int>>();

			var docketHits = new Dictionary<string, int>(baseHits);
			docketHits.Add(WhsDocketSchema.Constants.TableName, 1);
			docketHits.Add(WhsDocketLineSchema.Constants.TableName, 2);
			hitsDictionary.Add("ReceiptReference", docketHits);

			var stmNoteHits = new Dictionary<string, int>(baseHits);
			stmNoteHits.Add(StmNoteSchema.Constants.TableName, 1);
			stmNoteHits.Add(WhsDocketLineSchema.Constants.TableName, 2);
			hitsDictionary.Add("HasEDocsOrNotesAttached", stmNoteHits);

			var locationHits = new Dictionary<string, int>(baseHits);
			locationHits.Add(WhsLocationViewSchema.Constants.TableName, 1);
			locationHits.Add(WhsDocketLineSchema.Constants.TableName, 2);
			hitsDictionary.Add("CurrentLocationString", locationHits);
			hitsDictionary.Add("CurrentLocationStatus", locationHits);
			hitsDictionary.Add("CurrentLocationStatusCode", locationHits);
			hitsDictionary.Add("CurrentPickMethod", locationHits);
			hitsDictionary.Add("TouchesUntilStocktake", locationHits);
			hitsDictionary.Add("CurrentLocation+WLV_MaxCubic", locationHits);
			hitsDictionary.Add("CurrentLocation+WLV_MaxCubicUnit", locationHits);
			hitsDictionary.Add("CurrentLocation+WLV_MaxWeight", locationHits);
			hitsDictionary.Add("CurrentLocation+WLV_MaxWeightUnit", locationHits);
			hitsDictionary.Add("CurrentLocation+WLV_MaxQuantity", locationHits);
			hitsDictionary.Add("CurrentLocation+WLV_LastInventoryChangeDateForBinding", locationHits);

			var locationTypeHits = new Dictionary<string, int>(baseHits);
			locationTypeHits.Add(WhsLocationViewSchema.Constants.TableName, 1);
			locationTypeHits.Add(WhsLocationTypeSchema.Constants.TableName, 1);
			locationTypeHits.Add(WhsDocketLineSchema.Constants.TableName, 2);
			hitsDictionary.Add("CurrentLocationType", locationTypeHits);
			hitsDictionary.Add("CurrentLocationClass", locationTypeHits);

			var warehouseHits = new Dictionary<string, int>(baseHits);
			warehouseHits[WhsInventoryViewSchema.Constants.TableName] = 1;
			warehouseHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			warehouseHits.Add(WhsLocationViewSchema.Constants.TableName, 1);
			warehouseHits.Remove(StmModuleFilterSchema.Constants.TableName);
			hitsDictionary.Add("Warehouse+WW_WarehouseNameMultilingual", warehouseHits);

			var areaHits = new Dictionary<string, int>(baseHits);
			areaHits.Add(WhsAreaSchema.Constants.TableName, 1);
			areaHits.Add(WhsLocationViewSchema.Constants.TableName, 1);
			areaHits.Add(WhsDocketLineSchema.Constants.TableName, 2);
			hitsDictionary.Add("CurrentLocationPickAreaName", areaHits);
			hitsDictionary.Add("CurrentLocationPickAreaType", areaHits);

			var clientHits = new Dictionary<string, int>(baseHits);
			clientHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_OH_Client, clientHits);

			var clientNameHits = new Dictionary<string, int>(baseHits);
			clientNameHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			hitsDictionary.Add("Client+OH_FullName", clientNameHits);

			var productHits = new Dictionary<string, int>(baseHits);
			productHits.Add(OrgSupplierPartSchema.Constants.TableName, 1);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_OP_PartNum, productHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_OP_Desc, productHits);
			hitsDictionary.Add("CommodityCode", productHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_TotalUnits, productHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_LastCost, productHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_TotalValue, productHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_Currency, productHits);

			var pickLineHits = new Dictionary<string, int>(baseHits);
			pickLineHits.Add(WhsPickLineSchema.Constants.TableName, 1);
			pickLineHits.Add(WhsDocketLineSchema.Constants.TableName, 2);
			pickLineHits.Add(OrgSupplierPartSchema.Constants.TableName, 1);
			pickLineHits.Add(WhsLocationViewSchema.Constants.TableName, 1);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_AvailableToPickQuantity, pickLineHits);

			var crossDockHits = new Dictionary<string, int>(baseHits);
			crossDockHits.Add(WhsPickLineSchema.Constants.TableName, 1);
			crossDockHits.Add(WhsDocketLineSchema.Constants.TableName, 2);
			crossDockHits.Add(OrgSupplierPartSchema.Constants.TableName, 1);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CrossDockQuantity, crossDockHits);

			var uNDGHits = new Dictionary<string, int>(baseHits);
			uNDGHits.Add(UNDGDataItemSchema.Constants.TableName, 1);
			uNDGHits.Add(OrgSupplierPartSchema.Constants.TableName, 1);
			hitsDictionary.Add("Product+FirstUNDG+DI_DG", uNDGHits);
			hitsDictionary.Add("Product+FirstUNDG+Subs+DG_SubLabel1", uNDGHits);
			hitsDictionary.Add("Product+FirstUNDG+Subs+DG_SubLabel2", uNDGHits);
			hitsDictionary.Add("Product+FirstUNDG+Subs+DG_PSN", uNDGHits);

			var productStyleHits = new Dictionary<string, int>(baseHits);
			productStyleHits.Add(OrgSupplierPartSchema.Constants.TableName, 1);
			productStyleHits.Add(WhsProductStyleSchema.Constants.TableName, 1);
			productStyleHits.Add(WhsProductStyleColourSchema.Constants.TableName, 1);
			hitsDictionary.Add("Product+ProductStyle+WST_Code", productStyleHits);
			hitsDictionary.Add("Product+ProductStyle+WST_Description", productStyleHits);

			var productStyleColourHits = new Dictionary<string, int>(baseHits);
			productStyleColourHits.Add(WhsProductStyleColourSchema.Constants.TableName, 1);
			productStyleColourHits.Add(OrgSupplierPartSchema.Constants.TableName, 1);
			hitsDictionary.Add("Product+ProductStyleColour+WSC_Code", productStyleColourHits);
			hitsDictionary.Add("Product+ProductStyleColour+WSC_Description", productStyleColourHits);

			var productStyleClassificationHits = new Dictionary<string, int>(baseHits);
			productStyleClassificationHits.Add(WhsProductStyleClassificationSchema.Constants.TableName, 1);
			productStyleClassificationHits.Add(OrgSupplierPartSchema.Constants.TableName, 1);
			hitsDictionary.Add("Product+ProductStyleClassification+WSS_Code", productStyleClassificationHits);
			hitsDictionary.Add("Product+ProductStyleClassification+WSS_Description", productStyleClassificationHits);

			var productStyleSizeHits = new Dictionary<string, int>(baseHits);
			productStyleSizeHits.Add(OrgSupplierPartSchema.Constants.TableName, 1);
			productStyleSizeHits.Add(WhsProductStyleSizeSchema.Constants.TableName, 1);
			hitsDictionary.Add("Product+ProductStyleSize+WSZ_Size", productStyleSizeHits);

			var customsDataHits = new Dictionary<string, int>(baseHits);
			customsDataHits.Add(WhsBondedWarehouseAttributeSchema.Constants.TableName, 1);
			customsDataHits.Add(WhsDocketSchema.Constants.TableName, 1);
			customsDataHits.Add(WhsDocketLineSchema.Constants.TableName, 2);
			hitsDictionary.Add("CustomsData+WB_EntryDate", customsDataHits);
			hitsDictionary.Add("CustomsData+WB_EntryKey", customsDataHits);
			hitsDictionary.Add("CustomsData+WB_EntryLineNo", customsDataHits);
			hitsDictionary.Add("CustomsData+WB_DeclarationReference", customsDataHits);
			hitsDictionary.Add("CustomsData+WB_RN_NKCountryOfOrigin", customsDataHits);
			hitsDictionary.Add("CustomsData+WB_CustomsQty", customsDataHits);
			hitsDictionary.Add("CustomsData+WB_CustomsUnitOfQty", customsDataHits);
			hitsDictionary.Add("CustomsData+WB_ValueForDuty", customsDataHits);
			hitsDictionary.Add("CustomsData+WB_TILV", customsDataHits);
			hitsDictionary.Add("CustomsData+WB_AddInfo", customsDataHits);
			hitsDictionary.Add("CustomsData+ManufacturerCode", customsDataHits);
			hitsDictionary.Add("CustomsData+WB_CustomsDeadline", customsDataHits);
			hitsDictionary.Add("CustomsData+WB_InwardStyle", customsDataHits);
			hitsDictionary.Add("CustomsData+WB_InwardProcedure", customsDataHits);

			var customsTariffHits = new Dictionary<string, int>(baseHits);
			customsTariffHits.Add(CusClassPartPivotSchema.Constants.TableName, 1);
			hitsDictionary.Add("CustomsTariffItem", customsTariffHits);

			var internalsProxyHits = new Dictionary<string, int>(baseHits);
			internalsProxyHits.Add(WhsPickLineSchema.Constants.TableName, 1);
			internalsProxyHits.Add(OrgSupplierPartSchema.Constants.TableName, 1);
			hitsDictionary.Add("InternalsProxy+CommittedToTransactionQuantity", internalsProxyHits);

			var docketLineHits = new Dictionary<string, int>(baseHits);
			docketLineHits.Add(WhsDocketLineSchema.Constants.TableName, 2);
			hitsDictionary.Add(nameof(WhsInventoryView.InDocketLine) + "+" + WhsDocketLineSchema.Constants.WE_CurrentHoldReason, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomAttrib1, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomAttrib2, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomAttrib3, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomAttrib4, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomAttrib5, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomAttrib6, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomDate1, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomDate2, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomDate3, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomDate4, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomDate5, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomFlag1, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomFlag2, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomFlag3, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomFlag4, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomFlag5, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomDecimal1, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomDecimal2, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomDecimal3, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomDecimal4, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomDecimal5, docketLineHits);
			hitsDictionary.Add(WhsInventoryView.Schema.WI_CustomTextBlob1, docketLineHits);

			return hitsDictionary;
		}

		#endregion

		#region GetNewCollection

		protected override WhsModuleInventoryCollection GetNewCollection(BusinessObjectFactory factory)
		{
			return new WhsModuleInventoryCollection(factory);
		}

		#endregion

		#region GetNewFilterBusinessObject

		protected override InventoryFilterBusinessObject GetNewFilterBusinessObject() => new InventoryFilterBusinessObject();

		#endregion

		#region GetNewFilterControl

		protected override ZFilterStripControl GetNewFilterControl(WhsModuleInventoryCollection collection, InventoryFilterBusinessObject filterBizO)
		{
			return new InventoryFilterControl(collection, filterBizO);
		}

		#endregion

		#region Implementation

		protected override WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new WhsTestHelperFunctions(Factory);
		}

		new WhsTestHelperFunctions Helper => (WhsTestHelperFunctions)base.Helper;

		#endregion
	}
}
