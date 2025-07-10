using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class USBondedHelperTest : WhsTestCaseWithFactory
	{
		#region TestIsUSWarehouseBondedEnabled

		public void TestIsUSWarehouseBondedEnabled()
		{
			var whs = Helper.CreateWarehouse("WHS");
			Helper.EnableWarehouseForBond(whs, true);
			var client = Helper.CreateClient();
			var docket = Helper.CreateWhsReceive(client, whs);

			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Core.Constants.CountryCodes.Australia).RL_Code;
			AssertEquals(false, docket.IsWarehouseBondedEnabledAndUSJurisdiction());
			AssertEquals(false, whs.IsBondedEnabledAndUSJurisdiction());

			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Core.Constants.CountryCodes.PuertoRico).RL_Code;
			AssertEquals(true, docket.IsWarehouseBondedEnabledAndUSJurisdiction());
			AssertEquals(true, whs.IsBondedEnabledAndUSJurisdiction());

			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Core.Constants.CountryCodes.UnitedStates).RL_Code;
			AssertEquals(true, docket.IsWarehouseBondedEnabledAndUSJurisdiction());
			AssertEquals(true, whs.IsBondedEnabledAndUSJurisdiction());
			Helper.EnableWarehouseForBond(whs, false);
			AssertEquals(false, docket.IsWarehouseBondedEnabledAndUSJurisdiction());
			AssertEquals(false, whs.IsBondedEnabledAndUSJurisdiction());
		}

		#endregion

		#region TestIsUSFTZWarehouseBondedEnabled

		public void TestIsUSFTZWarehouseBondedEnabled()
		{
			var whs = Helper.CreateFTZWarehouseInUS("WHS");
			Helper.EnableWarehouseForBond(whs, true);
			var client = Helper.CreateClient();
			var docket = Helper.CreateWhsReceive(client, whs);

			SetAndAssertIsUSFTZWarehouseBondedEnabled(whs, WarehouseTypes.Codes.FreeTradeZone, Core.Constants.CountryCodes.Australia, expectedIsUSFTZ: false);
			SetAndAssertIsUSFTZWarehouseBondedEnabled(whs, WarehouseTypes.Codes.FreeTradeZone, Core.Constants.CountryCodes.UnitedStates, expectedIsUSFTZ: true);
			SetAndAssertIsUSFTZWarehouseBondedEnabled(whs, WarehouseTypes.Codes.FreeTradeZone, Core.Constants.CountryCodes.PuertoRico, expectedIsUSFTZ: true);
			SetAndAssertIsUSFTZWarehouseBondedEnabled(whs, WarehouseTypes.Codes.Transit, Core.Constants.CountryCodes.Australia, expectedIsUSFTZ: false);
			SetAndAssertIsUSFTZWarehouseBondedEnabled(whs, WarehouseTypes.Codes.Transit, Core.Constants.CountryCodes.UnitedStates, expectedIsUSFTZ: false);
			SetAndAssertIsUSFTZWarehouseBondedEnabled(whs, WarehouseTypes.Codes.Transit, Core.Constants.CountryCodes.PuertoRico, expectedIsUSFTZ: false);
		}

		void SetAndAssertIsUSFTZWarehouseBondedEnabled(WhsWarehouse whs, string warehouseType, ZString countryCode, bool expectedIsUSFTZ)
		{
			whs.WW_WarehouseType = warehouseType;
			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(countryCode).RL_Code;
			AssertEquals(expectedIsUSFTZ, whs.IsFTZBondedEnabledAndUSJurisdiction());
		}

		#endregion

		#region TestIsUSFTZWarehouse

		public void TestIsUSFTZWarehouse()
		{
			var whs = Helper.CreateFTZWarehouseInUS("WHS");
			var client = Helper.CreateClient();
			var docket = Helper.CreateWhsReceive(client, whs);

			AssertIsUSFTZWarehouse(whs, WarehouseTypes.Codes.FreeTradeZone, Core.Constants.CountryCodes.Australia, expectedIsUSFTZ: false);
			AssertIsUSFTZWarehouse(whs, WarehouseTypes.Codes.FreeTradeZone, Core.Constants.CountryCodes.UnitedStates, expectedIsUSFTZ: true);
			AssertIsUSFTZWarehouse(whs, WarehouseTypes.Codes.FreeTradeZone, Core.Constants.CountryCodes.PuertoRico, expectedIsUSFTZ: true);
			AssertIsUSFTZWarehouse(whs, WarehouseTypes.Codes.Transit, Core.Constants.CountryCodes.Australia, expectedIsUSFTZ: false);
			AssertIsUSFTZWarehouse(whs, WarehouseTypes.Codes.Transit, Core.Constants.CountryCodes.UnitedStates, expectedIsUSFTZ: false);
			AssertIsUSFTZWarehouse(whs, WarehouseTypes.Codes.Transit, Core.Constants.CountryCodes.PuertoRico, expectedIsUSFTZ: false);
		}

		void AssertIsUSFTZWarehouse(WhsWarehouse whs, string warehouseType, ZString countryCode, bool expectedIsUSFTZ)
		{
			whs.WW_WarehouseType = warehouseType;
			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(countryCode).RL_Code;
			AssertEquals(expectedIsUSFTZ, whs.IsFTZAndUSJurisdiction());
		}

		#endregion

		#region TestGetDistinctPackageGroupIDInventories

		public void TestGetDistinctPackageGroupIDInventories()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var location = data.Whs1.DefaultLocation;
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, "KEY-1", "", 3m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, "KEY-2", "123", 3m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m, "KEY-2", "123", 5m);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, "KEY-3", "456", 5m);
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 21m, "KEY-3", "456", 3m);
			var inventory6 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 9m, "KEY-3", "456", 3m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var results1 = USBondedHelper.GetDistinctPackageGroupIDInventories(Factory, data.Whs1, "");
			AssertEquals("When Package Group ID is not specified, nothing should be found.", 0, results1.Count);

			var results2 = USBondedHelper.GetDistinctPackageGroupIDInventories(Factory, data.Whs1, "123");
			AssertEquals("Package 123 consists of 2 different products.", 2, results2.Count);
			AssertEquals(3m, results2[ProductWithAttributes.GetProductWithAttributes(inventory2, useBondedEntryKey: true)]);
			AssertEquals(5m, results2[ProductWithAttributes.GetProductWithAttributes(inventory3, useBondedEntryKey: true)]);
			AssertEquals(false, results2.ContainsKey(ProductWithAttributes.GetProductWithAttributes(inventory2, useBondedEntryKey: false)));
			AssertEquals(false, results2.ContainsKey(ProductWithAttributes.GetProductWithAttributes(inventory3, useBondedEntryKey: false)));

			var results3 = USBondedHelper.GetDistinctPackageGroupIDInventories(Factory, data.Whs1, "456");
			AssertEquals("Package 456 consists of 2 different products.", 2, results3.Count);
			AssertEquals(5m, results3[ProductWithAttributes.GetProductWithAttributes(inventory4, useBondedEntryKey: true)]);
			AssertEquals(3m, results3[ProductWithAttributes.GetProductWithAttributes(inventory5, useBondedEntryKey: true)]);
			AssertEquals(false, results3.ContainsKey(ProductWithAttributes.GetProductWithAttributes(inventory4, useBondedEntryKey: false)));
			AssertEquals(false, results3.ContainsKey(ProductWithAttributes.GetProductWithAttributes(inventory5, useBondedEntryKey: false)));
		}

		#endregion

		#region TestGetInventoryMatchingPackageGroupID

		#region TestGetInventoryMatchingPackageGroupID

		public void TestGetInventoryMatchingPackageGroupID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var location = data.Whs1.DefaultLocation;
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, "KEY-1", "", 3m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, "KEY-2", "123", 3m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m, "KEY-2", "123", 5m);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, "KEY-3", "456", 5m);
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 21m, "KEY-3", "456", 3m);
			var inventory6 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 9m, "KEY-3", "456", 3m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var results1 = USBondedHelper.GetInventoryMatchingPackageGroupID(Factory, data.Whs1, "");
			AssertEquals("When Package Group ID is not specified, nothing should be found.", 0, results1.Length);

			var results2 = USBondedHelper.GetInventoryMatchingPackageGroupID(Factory, data.Whs1, "123");
			AssertEquals("Package 123 consists of 2 inventories.", 2, results2.Length);
			results2.Single(i => i.PK == inventory2.PK);
			results2.Single(i => i.PK == inventory3.PK);

			var results3 = USBondedHelper.GetInventoryMatchingPackageGroupID(Factory, data.Whs1, "456");
			AssertEquals("Package 456 consists of 3 inventories.", 3, results3.Length);
			results3.Single(i => i.PK == inventory4.PK);
			results3.Single(i => i.PK == inventory5.PK);
			results3.Single(i => i.PK == inventory6.PK);
		}

		public void TestGetInventoryMatchingPackageGroupID_InTransitTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var location = data.Whs1.DefaultLocation;
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, "KEY-2", "123", 3m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m, "KEY-2", "123", 5m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, "KEY-3", "456", 5m);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 30m, "KEY-3", "456", 3m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1ForGroup1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 30m, inventory1.Location, inventory1.Location);
			transferLine1ForGroup1.WE_BondedEntryKey = "KEY-2";
			transferLine1ForGroup1.WE_PackageGroupId = "123";
			transferLine1ForGroup1.WE_PerPackageQty = 3m;
			var transferLine2ForGroup1 = Helper.CreateWhsTransferLine(transfer1, data.Part2, 50m, inventory2.Location, inventory2.Location);
			transferLine2ForGroup1.WE_BondedEntryKey = "KEY-2";
			transferLine2ForGroup1.WE_PackageGroupId = "123";
			transferLine2ForGroup1.WE_PerPackageQty = 5m;

			transferLine2ForGroup1.RunPreSaveValidation();
			AssertEquals("Precondition: Transfer Line is committed.", 50m, transferLine2ForGroup1.QtyCommittedIncludingMatchingLines);

			transferLine1ForGroup1.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer Line is picked.", true, transferLine1ForGroup1.IsPicked);

			Factory.Save();

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1ForGroup2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 50m, inventory3.Location, inventory3.Location);
			transferLine1ForGroup2.WE_BondedEntryKey = "KEY-3";
			transferLine1ForGroup2.WE_PackageGroupId = "456";
			transferLine1ForGroup2.WE_PerPackageQty = 5m;
			var transferLine2ForGroup2 = Helper.CreateWhsTransferLine(transfer2, data.Part2, 30m, inventory4.Location, inventory4.Location);
			transferLine2ForGroup2.WE_BondedEntryKey = "KEY-3";
			transferLine2ForGroup2.WE_PackageGroupId = "456";
			transferLine2ForGroup2.WE_PerPackageQty = 3m;

			transferLine2ForGroup2.RunPreSaveValidation();
			AssertEquals("Precondition: Transfer Line is committed.", 30m, transferLine2ForGroup2.QtyCommittedIncludingMatchingLines);

			transferLine1ForGroup2.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer Line is picked.", true, transferLine1ForGroup2.IsPicked);

			var results1 = USBondedHelper.GetInventoryMatchingPackageGroupID(Factory, data.Whs1, "123");
			AssertContainsExactElementsInAnyOrder("Package 123 consists of Part 1 with PPQ of 3 and Part 2 with PPQ of 5 even though one is in-transit.", new[] { transferLine1ForGroup1.PK, inventory2.PK }, results1.Select(r => r.PK));

			var results2 = USBondedHelper.GetInventoryMatchingPackageGroupID(Factory, data.Whs1, "456");
			AssertContainsExactElementsInAnyOrder("Package 456 consists of Part 1 with PPQ of 5 and Part 2 with PPQ of 3.", new[] { inventory3.PK, inventory4.PK }, results2.Select(r => r.PK));
		}

		#endregion

		#region TestGetDistinctPackageGroupIDInventories_WhenCancelingFinalisedPick

		public void TestGetDistinctPackageGroupIDInventories_WhenCancelingFinalisedPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var location = data.Whs1.DefaultLocation;
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, "KEY-1", "123", 3m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, "KEY-1", "123", 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			// items were taken from Whs by finalised Pick.
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 3m, "KEY-1", "OUT-1", "123");
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m, "KEY-1", "OUT-1", "123");
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var results1 = USBondedHelper.GetDistinctPackageGroupIDInventories(Factory, data.Whs1, "123");
			AssertEquals("When no stock exist for the Group ID, nothing should be found.", 0, results1.Count);

			// assume items were put back into Whs by cancelling of finalised pick.
			inventory1.WI_TotalUnits = 3m;
			inventory2.WI_TotalUnits = 5m;
			var results2 = USBondedHelper.GetInventoryMatchingPackageGroupID(Factory, data.Whs1, "123");
			AssertContainsExactElementsInAnyOrder("Stock exists for the Group ID only in memory due to cancellation of Finalised Pick which is not saved yet.",
				new[] { inventory1.PK, inventory2.PK }, results2.Select(i => i.PK));
		}

		#endregion

		#endregion

		#region TestGetPackageGroupIDContent

		public void TestGetPackageGroupIDContent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var location = data.Whs1.DefaultLocation;
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, "KEY-1", "123", 3m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, "KEY-2", "123", 3m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m, "KEY-2", "123", 5m);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 70m, "KEY-2", "456", 5m);
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 21m, "KEY-3", "456", 3m);
			var inventory6 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 21m, "KEY-3", "456", 3m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var results1 = USBondedHelper.GetPackageGroupIDContent(Factory, data.Whs1, "");
			AssertEquals("When Package Group ID is not specified, nothing should be found.", 0, results1.Count);

			var results2 = USBondedHelper.GetPackageGroupIDContent(Factory, data.Whs1, "123");
			AssertEquals("Package 123 consists of 2 different products.", 2, results2.Count);
			AssertEquals(3m, results2[ProductWithAttributes.GetProductWithAttributes(inventory1, useBondedEntryKey: false)]);
			AssertEquals(3m, results2[ProductWithAttributes.GetProductWithAttributes(inventory2, useBondedEntryKey: false)]);
			AssertEquals(5m, results2[ProductWithAttributes.GetProductWithAttributes(inventory3, useBondedEntryKey: false)]);
			AssertEquals(false, results2.ContainsKey(ProductWithAttributes.GetProductWithAttributes(inventory1, useBondedEntryKey: true)));
			AssertEquals(false, results2.ContainsKey(ProductWithAttributes.GetProductWithAttributes(inventory2, useBondedEntryKey: true)));
			AssertEquals(false, results2.ContainsKey(ProductWithAttributes.GetProductWithAttributes(inventory3, useBondedEntryKey: true)));

			var results3 = USBondedHelper.GetPackageGroupIDContent(Factory, data.Whs1, "456");
			AssertEquals("Package 456 consists of 2 different products.", 2, results3.Count);
			AssertEquals(5m, results3[ProductWithAttributes.GetProductWithAttributes(inventory4, useBondedEntryKey: false)]);
			AssertEquals(3m, results3[ProductWithAttributes.GetProductWithAttributes(inventory5, useBondedEntryKey: false)]);
			AssertEquals(3m, results3[ProductWithAttributes.GetProductWithAttributes(inventory6, useBondedEntryKey: false)]);
			AssertEquals(false, results3.ContainsKey(ProductWithAttributes.GetProductWithAttributes(inventory4, useBondedEntryKey: true)));
			AssertEquals(false, results3.ContainsKey(ProductWithAttributes.GetProductWithAttributes(inventory5, useBondedEntryKey: true)));
			AssertEquals(false, results3.ContainsKey(ProductWithAttributes.GetProductWithAttributes(inventory6, useBondedEntryKey: true)));
		}

		#endregion
	}
}
