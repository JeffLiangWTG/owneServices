using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Environment.CodeLists.US;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsAdjustmentLineValidationUSTest : WhsAdjustmentLineValidationTest
	{
		#region TestCheckWE_TransactionQuantity

		#region TestCheckWE_TransactionQuantity_CanOnlyAdjustInMultiplesOfPerPackageQty

		public void TestCheckWE_TransactionQuantity_CanOnlyAdjustInMultiplesOfPerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			var location = data.Whs1.FindLocation("A-1");
			location.WLV_WA_PickingArea = bondedArea.PK;

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, location, "123-1", "Group1", 5m);
			AssertNoErrors("-5 Units / 5 = -1 Packs. No errors on Units expected.", adjustmentLine.WE_TransactionQuantityInfo);

			adjustmentLine.WE_TransactionQuantity = 10m;
			AssertNoErrors("10 Units / 5 = 2 Packs. No errors on Units expected.", adjustmentLine.WE_TransactionQuantityInfo);

			var expectedErrorMessage = "Units to be adjusted must be divisible by Per Group Quantity.";
			adjustmentLine.WE_TransactionQuantity = 2m;
			AssertHasError("2 Units / 5 = 0.4 packs. Units must be divisible by Per Group Quantity.", adjustmentLine.WE_TransactionQuantityInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_TransactionQuantity_NeedToAdjustEquivalentPackageAmountInTheSamePackageGroup

		public void TestCheckWE_TransactionQuantity_NeedToAdjustEquivalentPackageAmountInTheSamePackageGroup()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			var location = data.Whs1.FindLocation("A-1");
			location.WLV_WA_PickingArea = bondedArea.PK;
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			// Adjusting In
			var adjustment1 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment1.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment1, data.Part1, -5m, location, "123-1", "ABC", 5m);
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment1, data.Part2, -10m, location, "123-1", "ABC", 10m);
			AssertNoErrors(adjustmentLine1.WE_TransactionQuantityInfo);
			AssertNoErrors(adjustmentLine2.WE_TransactionQuantityInfo);

			adjustmentLine1.WE_TransactionQuantity = 15m;
			adjustmentLine2.WE_TransactionQuantity = 30m;
			AssertNoErrors(adjustmentLine2.WE_TransactionQuantityInfo);

			// Adjusting Out
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location.PK, "123-1", "ABC", 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, location.PK, "123-1", "ABC", 10m);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var expectedErrorMessage = "Only full packages (by Package Group ID) can be adjusted out from a location.";
			adjustmentLine1.WE_TransactionQuantity = -5m;
			adjustmentLine2.WE_TransactionQuantity = -20m;
			AssertHasError(adjustmentLine1.WE_TransactionQuantityInfo, expectedErrorMessage);
			AssertHasError(adjustmentLine2.WE_TransactionQuantityInfo, expectedErrorMessage);
		}

		public void TestCheckWE_TransactionQuantity_NeedToAdjustEquivalentPackageAmountInTheSamePackageGroup_WithPartAttributes_PartAttrib1()
		{
			TestCheckWE_TransactionQuantity_NeedToAdjustEquivalentPackageAmountInTheSamePackageGroup_WithPartAttributesCore(AttributeNumber.One, (docketLine, attribValue) => docketLine.WE_PartAttrib1 = attribValue);
		}

		public void TestCheckWE_TransactionQuantity_NeedToAdjustEquivalentPackageAmountInTheSamePackageGroup_WithPartAttributes_PartAttrib2()
		{
			TestCheckWE_TransactionQuantity_NeedToAdjustEquivalentPackageAmountInTheSamePackageGroup_WithPartAttributesCore(AttributeNumber.Two, (docketLine, attribValue) => docketLine.WE_PartAttrib2 = attribValue);
		}

		public void TestCheckWE_TransactionQuantity_NeedToAdjustEquivalentPackageAmountInTheSamePackageGroup_WithPartAttributes_PartAttrib3()
		{
			TestCheckWE_TransactionQuantity_NeedToAdjustEquivalentPackageAmountInTheSamePackageGroup_WithPartAttributesCore(AttributeNumber.Three, (docketLine, attribValue) => docketLine.WE_PartAttrib3 = attribValue);
		}

		public void TestCheckWE_TransactionQuantity_NeedToAdjustEquivalentPackageAmountInTheSamePackageGroup_WithPartAttributes_SerialNumber()
		{
			TestCheckWE_TransactionQuantity_NeedToAdjustEquivalentPackageAmountInTheSamePackageGroup_WithPartAttributesCore(AttributeNumber.Serial, (docketLine, attribValue) => docketLine.WE_SerialNumber = attribValue);
		}

		void TestCheckWE_TransactionQuantity_NeedToAdjustEquivalentPackageAmountInTheSamePackageGroup_WithPartAttributesCore(AttributeNumber attributeNumber, Action<WhsDocketLine, string> attribValueSetter)
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			var location = data.Whs1.FindLocation("A-1");
			location.WLV_WA_PickingArea = bondedArea.PK;
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Helper.SetClientAttributeType(data.Org1, attributeNumber, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location.PK, "123-1", "ABC", 1m);
			attribValueSetter(inventory.InDocketLine, "VAL1");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, location, "123-1", "ABC", 1m);
			attribValueSetter(adjustmentLine, "VAL1");
			adjustmentLine.Validation.ValidateWE_TransactionQuantity();
			AssertNoErrors(adjustmentLine.WE_TransactionQuantityInfo);

			attribValueSetter(adjustmentLine, "VAL2");
			var expectedErrorMessage = "Only full packages (by Package Group ID) can be adjusted out from a location.";
			adjustmentLine.Validation.ValidateWE_TransactionQuantity();
			AssertHasError(adjustmentLine.WE_TransactionQuantityInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_TransactionQuantity_NewInventoryMustHaveSamePackageGroupIDAndPerPackageQtyAsAdjustment

		public void TestCheckWE_TransactionQuantity_NewInventoryMustHaveSamePackageGroupIDAndPerPackageQtyAsAdjustment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			var location = data.Whs1.FindLocation("A-1");
			location.WLV_WA_PickingArea = bondedArea.PK;

			var adjustment1 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ1", Notify);
			adjustment1.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment1, data.Part1, 5m, location, "123-1", "ABC", 1m);
			adjustment1.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment1);

			var newInventory = Helper.LoadInventory(data.Org1, data.Part1).Inventory.Single();
			AssertEquals("The Package Group ID of created inventory should be as same as the Package Group ID of original Adjustment.", "ABC", newInventory.PackageGroupId);
			AssertEquals("The Per Package Qty of created inventory should be as same as the Per Package Qty of original Adjustment.", 1m, newInventory.PerPackageQty);
		}

		#endregion

		#region TestCheckWE_TransactionQuantity_PackagesAreAdjustedWithSamePackageCountAndAreNotSplit

		public void TestCheckWE_TransactionQuantity_PackagesAreAdjustedWithSamePackageCountAndAreNotSplit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_WA_PickingArea = bondedArea.PK;
			locations[0].WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[0].PK, "123-1", "ABC", 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, locations[0].PK, "123-1", "ABC", 10m);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var expectedErrorMessage = "Only full packages (by Package Group ID) can be adjusted into a location.";

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ1", Notify);
			adjustment.WD_DocketSubType = ReceiveType.Codes.Customs;
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 5m, locations[0], "123-1", "ABC", 5m);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(false, adjustment.IsFinalised);
			AssertHasError(adjustmentLine1.WE_TransactionQuantityInfo, expectedErrorMessage);

			// PerPackageQty = 0
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, 10m, locations[0], "123-1", "ABC", 0m);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(false, adjustment.IsFinalised);
			AssertHasError(adjustmentLine2.WE_PerPackageQtyInfo, "Per Group Quantity must be specified if Package Group ID is specified.");

			// valid PerPackageQty but different location
			adjustmentLine2.WE_PerPackageQty = 10m;
			adjustmentLine2.WE_WL = locations[1].PK;
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(false, adjustment.IsFinalised);
			AssertHasError(adjustmentLine2.WE_TransactionQuantityInfo, expectedErrorMessage);

			adjustmentLine2.WE_WL = locations[0].PK;
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, adjustment.IsFinalised);
			AssertNoErrors(adjustmentLine1.WE_TransactionQuantityInfo);
			AssertNoErrors(adjustmentLine2.WE_TransactionQuantityInfo);
		}

		#endregion

		#endregion

		#region TestCheckWE_TransactionQuantity_PackageGroupExistsOnTwoDifferentPallets

		public void TestCheckWE_TransactionQuantity_PackageGroupExistsOnTwoDifferentPallets()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.RemoveAreasOfTypeFromWarehouse(data.Whs1, AreaTypes.Codes.FreeStore);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO("US").RL_Code;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, "1234", "XYZ", 2m);
			receiveLine1.WE_PalletID = "PLT-123";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, "1234-1", "XYZ", 2m);
			receiveLine2.WE_PalletID = "PLT-456";

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, receiveLine1.Location, "1234", "XYZ", 2m);
			adjustmentLine.WE_PalletID = "PLT-123";
			adjustment.RunPreSaveValidation();

			AssertEquals("Line should be committed.", 10m, adjustmentLine.CommittedQuantity);
			AssertEquals("Line should have no errors.", false, adjustmentLine.HasErrors);

			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustment);
			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestCheckLocationStringLocationIsNotTSAKnownError

		public void TestCheckLocationStringLocationIsNotKnownByTSAError()
		{
			string errorMessage = "This location is not known by TSA.";
			var adjustmentLine = DocketLine;
			AssertNotNull("Precondition: Docket should be set", adjustmentLine.Docket);
			AssertNotNull("Precondition: Client should be set", adjustmentLine.Docket.Client);
			AssertNotNull("Precondition: Warehouse should be set", adjustmentLine.Docket.Warehouse);

			var row = Helper.CreateRow(adjustmentLine.Docket.Warehouse, "ROW");
			var location = row.Locations.AddNew();
			adjustmentLine.WE_TransactionQuantity = 10m;
			adjustmentLine.WE_WL = location.PK;
			Helper.SetWarehouseTSAStatus(adjustmentLine.Docket.Warehouse, TSAStatus.Codes.Known);
			Helper.SetOrgAddressTSAStatus(adjustmentLine.Docket.Client.MainAddress, TSAStatus.Codes.Known);
			location.WLV_ApprovedKnownLocation = TSAStatus.Codes.Unknown;
			adjustmentLine.Validation.ValidateAll();
			AssertHasError(adjustmentLine.LocationStringInfo, errorMessage);

			location.WLV_ApprovedKnownLocation = TSAStatus.Codes.Known;
			adjustmentLine.Validation.ValidateAll();
			AssertNoError(adjustmentLine.LocationStringInfo, errorMessage);

			location.WLV_ApprovedKnownLocation = TSAStatus.Codes.Unknown;
			Helper.SetOrgAddressTSAStatus(adjustmentLine.Docket.Client.MainAddress, TSAStatus.Codes.Unknown);
			adjustmentLine.Validation.ValidateAll();
			AssertNoError(adjustmentLine.LocationStringInfo, errorMessage);

			Helper.SetOrgAddressTSAStatus(adjustmentLine.Docket.Client.MainAddress, TSAStatus.Codes.Known);
			Helper.SetWarehouseTSAStatus(adjustmentLine.Docket.Warehouse, TSAStatus.Codes.Unknown);
			adjustmentLine.Validation.ValidateAll();
			AssertNoError(adjustmentLine.LocationStringInfo, errorMessage);

			adjustmentLine.Docket.WD_WW_Whs = ZGuid.Empty;
			adjustmentLine.Validation.ValidateAll();
			AssertNoError(adjustmentLine.LocationStringInfo, errorMessage);

			adjustmentLine.Docket.WD_WW_Whs = row.Warehouse.PK;
			adjustmentLine.WE_WL = ZGuid.Empty;
			adjustmentLine.Validation.ValidateAll();
			AssertNoError(adjustmentLine.LocationStringInfo, errorMessage);

			adjustmentLine.WE_WL = location.PK;
			adjustmentLine.WE_TransactionQuantity = 0m;
			adjustmentLine.Validation.ValidateAll();
			AssertNoError(adjustmentLine.LocationStringInfo, errorMessage);

			adjustmentLine.WE_TransactionQuantity = -10m;
			adjustmentLine.Validation.ValidateAll();
			AssertNoError(adjustmentLine.LocationStringInfo, errorMessage);

			adjustmentLine.WE_TransactionQuantity = 10m;
			adjustmentLine.WE_WD = ZGuid.Empty;
			adjustmentLine.Validation.ValidateAll();
			AssertNoError(adjustmentLine.LocationStringInfo, errorMessage);
		}

		#endregion

		#region TestCheckLocationStringLocationIsNotTSAUnknownError

		public void TestCheckLocationStringLocationIsKnownByTSAError()
		{
			string errorMessage = "This location is known by TSA.";
			var adjustmentLine = DocketLine;
			AssertNotNull("Precondition: Docket should be set", adjustmentLine.Docket);
			AssertNotNull("Precondition: Client should be set", adjustmentLine.Docket.Client);
			AssertNotNull("Precondition: Warehouse should be set", adjustmentLine.Docket.Warehouse);

			var row = Helper.CreateRow(adjustmentLine.Docket.Warehouse, "ROW");
			var location = row.Locations.AddNew();
			adjustmentLine.WE_TransactionQuantity = 10m;
			adjustmentLine.WE_WL = location.PK;
			Helper.SetWarehouseTSAStatus(adjustmentLine.Docket.Warehouse, TSAStatus.Codes.Known);
			Helper.SetOrgAddressTSAStatus(adjustmentLine.Docket.Client.MainAddress, TSAStatus.Codes.Unknown);
			location.WLV_ApprovedKnownLocation = TSAStatus.Codes.Known;
			adjustmentLine.Validation.ValidateAll();
			AssertHasError(adjustmentLine.LocationStringInfo, errorMessage);

			location.WLV_ApprovedKnownLocation = TSAStatus.Codes.Unknown;
			adjustmentLine.Validation.ValidateAll();
			AssertNoError(adjustmentLine.LocationStringInfo, errorMessage);

			location.WLV_ApprovedKnownLocation = TSAStatus.Codes.Known;
			Helper.SetOrgAddressTSAStatus(adjustmentLine.Docket.Client.MainAddress, TSAStatus.Codes.Known);
			adjustmentLine.Validation.ValidateAll();
			AssertNoError(adjustmentLine.LocationStringInfo, errorMessage);

			Helper.SetOrgAddressTSAStatus(adjustmentLine.Docket.Client.MainAddress, TSAStatus.Codes.Unknown);
			Helper.SetWarehouseTSAStatus(adjustmentLine.Docket.Warehouse, TSAStatus.Codes.Unknown);
			adjustmentLine.Validation.ValidateAll();
			AssertNoError(adjustmentLine.LocationStringInfo, errorMessage);

			adjustmentLine.Docket.WD_WW_Whs = ZGuid.Empty;
			adjustmentLine.Validation.ValidateAll();
			AssertNoError(adjustmentLine.LocationStringInfo, errorMessage);

			adjustmentLine.Docket.WD_WW_Whs = row.Warehouse.PK;
			adjustmentLine.WE_WL = ZGuid.Empty;
			adjustmentLine.Validation.ValidateAll();
			AssertNoError(adjustmentLine.LocationStringInfo, errorMessage);

			adjustmentLine.WE_WL = location.PK;
			adjustmentLine.WE_TransactionQuantity = 0m;
			adjustmentLine.Validation.ValidateAll();
			AssertNoError(adjustmentLine.LocationStringInfo, errorMessage);

			adjustmentLine.WE_TransactionQuantity = -10m;
			adjustmentLine.Validation.ValidateAll();
			AssertNoError(adjustmentLine.LocationStringInfo, errorMessage);

			adjustmentLine.WE_TransactionQuantity = 10m;
			adjustmentLine.WE_WD = ZGuid.Empty;
			adjustmentLine.Validation.ValidateAll();
			AssertNoError(adjustmentLine.LocationStringInfo, errorMessage);
		}

		#endregion

		#region TestCheckHeldCodeToChangeTo_CannotChangeStatusIfInventoryIsInPackageGroup

		public void TestCheckHeldCodeToChangeTo_CannotChangeStatusIfInventoryIsInPackageGroup()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.RemoveAreasOfTypeFromWarehouse(data.Whs1, "FRE");

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			Factory.Save();

			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.DefaultLocation, "123-1", "A", 2m);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustment);
			AssertNoErrors("Precondition", adjustmentLine.HeldCodeToChangeToInfo);

			adjustmentLine.IsInventoryEditForm = true;
			adjustmentLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			AssertHasError(adjustmentLine.HeldCodeToChangeToInfo, "Cannot change the Hold Code of Inventory in a Package Group.");
		}

		#endregion

		#region TestCheckHeldCodeChangeQuantity_ChangeQuantityMustBeDivisibleByPerPackageQty

		public void TestCheckHeldCodeChangeQuantity_ChangeQuantityMustBeDivisibleByPerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.RemoveAreasOfTypeFromWarehouse(data.Whs1, "FRE");
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.DefaultLocation, "123-1", "", 2m);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustment);
			AssertNoErrors("Precondition", adjustmentLine.HeldCodeChangeQuantityInfo);

			adjustmentLine.IsInventoryEditForm = true;
			adjustmentLine.HeldCodeChangeQuantity = 3m;
			AssertHasError(adjustmentLine.HeldCodeChangeQuantityInfo, "Quantity must be divisible by Per Group Quantity.");

			adjustmentLine.HeldCodeChangeQuantity = 4m;
			AssertNoErrors(adjustmentLine.HeldCodeChangeQuantityInfo);
		}

		#endregion

		#region Implementation

		protected override Environment.Business.Testing.WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new US.Testing.WhsTestHelperFunctionsUS(Factory);
		}

		protected override WhsAdjustment GetNewDocket()
		{
			return Helper.CreateWhsAdjustment(Helper.CreateClient(), Helper.CreateWarehouse("WHSUS"));
		}

		protected new US.Testing.WhsTestHelperFunctionsUS Helper
		{
			get { return (US.Testing.WhsTestHelperFunctionsUS)base.Helper; }
		}

		#endregion
	}
}
