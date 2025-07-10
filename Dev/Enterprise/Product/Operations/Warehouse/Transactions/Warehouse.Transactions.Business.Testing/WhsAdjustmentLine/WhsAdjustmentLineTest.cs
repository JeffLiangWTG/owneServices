using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAdjustmentLine))]
	class WhsAdjustmentLineTest : WhsDocketLineTestCase<WhsAdjustmentLine, WhsAdjustment>
	{
		#region Business Object Overrides

		#region TestIsInventoryLine

		protected override void TestIsInventoryLineCore()
		{
			var docketLine = GetNewBusinessObject(GetNewWhsDocket());
			docketLine.WE_TransactionQuantity = 1;
			AssertEquals(true, docketLine.IsInventoryLine);

			docketLine.WE_TransactionQuantity = 0;
			AssertEquals(false, docketLine.IsInventoryLine);

			docketLine.WE_TransactionQuantity = -1;
			AssertEquals(false, docketLine.IsInventoryLine);
		}

		#endregion

		#endregion

		#region Validation

		protected override Type GetExpectedValidationType()
		{
			return typeof(WhsAdjustmentLineValidation);
		}

		#region TestRunPreSaveValidation_CommitsStock

		public void TestRunPreSaveValidation_CommitsStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location, "");
			var inventory = receive.Inventory[0];

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, location.ToLocationString());

			AssertEquals("Precondition - ensure no stock is not committed.", 0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure no stock is not committed.", 0m, adjustmentLine.GetQtyCommittedToThisLine());

			var expectedErrorMessage =
@"Attempted to adjust 100 Units, but only 50 Units are available for adjustment out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the adjustment line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to adjust.
If you are trying to adjust stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to adjust stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.";

			adjustmentLine.RunPreSaveValidation();
			AssertEquals("When adjustment is saved it should commit stock.", 10m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("When adjustment is saved it should commit stock.", 10m, adjustmentLine.CommittedQuantity);
			AssertNoError("Adjustment line should have no error if there is enough stock to adjust out.", adjustmentLine.WE_TransactionQuantityInfo, expectedErrorMessage);

			adjustmentLine.WE_TransactionQuantity = -100m;
			adjustmentLine.RunPreSaveValidation();
			AssertEquals("When adjustment is saved it should commit stock.", 50m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("When adjustment is saved it should commit stock.", 50m, adjustmentLine.CommittedQuantity);
			AssertHasError("Adjustment line should have error if there are not enought stock to adjust out.", adjustmentLine.WE_TransactionQuantityInfo, expectedErrorMessage);
		}

		#endregion

		#endregion

		#region Lookups

		protected override Type GetExpectedLookupsType()
		{
			return typeof(WhsAdjustmentLineLookups);
		}

		public void TestGetNewLookupsUS()
		{
			US.Testing.WhsTestHelperFunctionsUS helper = new US.Testing.WhsTestHelperFunctionsUS(Factory);
			OrgHeader org = helper.CreateClient();
			WhsWarehouse whs = helper.CreateWarehouse("WHS");
			WhsAdjustment docket = helper.CreateWhsAdjustment(org, whs);
			DocketLine.WE_WD = docket.PK;
			AssertEquals(typeof(WhsAdjustmentLineValidationUS), DocketLine.Validation.GetType());
		}

		#endregion

		#region Properties

		#region TestWE_ReasonCode

		public void TestWE_ReasonCode()
		{
			var adjustment = Factory.New<WhsAdjustment>();
			var line = Factory.New<WhsAdjustmentLine>();
			line.WE_WD = adjustment.PK;
			line.WE_ReasonCode = "XXX";
			AssertEquals("XXX", line.WE_ReasonCode);
			AssertEquals(false, line.WE_ReasonCodeInfo.ReadOnly);

			adjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			AssertEquals(false, line.WE_ReasonCodeInfo.ReadOnly);

			adjustment.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals(true, line.WE_ReasonCodeInfo.ReadOnly);
			adjustment.WD_FinalisedDate = ZDateTimeOffset.Empty;

			adjustment.WD_WD_ParentDocket = Factory.New<WhsAdjustment>().PK;
			AssertEquals(true, line.WE_ReasonCodeInfo.ReadOnly);

			adjustment.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals(true, line.WE_ReasonCodeInfo.ReadOnly);
		}

		#endregion

		#region TestWE_ReasonDescription

		public void TestWE_ReasonDescription()
		{
			var line = Factory.New<WhsAdjustmentLine>();
			line.WE_ReasonCode = "CLI";
			AssertEquals("Client Instructed", line.WE_ReasonDescription);
		}

		#endregion

		#region TestWE_BondedEntryKey

		public void TestWE_BondedEntryKey()
		{
			TestDataForBondedEntries data = new TestDataForBondedEntries(Factory);
			Factory.Save();

			// create adjustment - line is created by base
			DocketLine.Docket.Lines.RemoveFromRelationship(DocketLine);
			Docket = GetNewWhsDocket(data.Org, data.Whs, DocketLine);
			Docket.WD_DocketSubType = AdjustmentType.Codes.Customs;

			AssertEquals("Precondition", ZString.Empty, DocketLine.CustomsData.WB_BondedWhsUnitOfQty);
			AssertEquals("Precondition: docket is a customs transaction.", true, Docket.IsCustomsTransaction);
			DocketLine.WE_BondedEntryKey = WhsBondedWarehouseAttribute.BuildKey(data.IReceiveLine1.EntryKey, data.IReceiveLine1.EntryLineNumber);
			AssertEquals("", DocketLine.CustomsData.WB_BondedWhsUnitOfQty);
		}

		#endregion

		#region TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume

		protected override bool SettingWE_TransactionQuantityUpdatesTotalWeightAndVolume => false;

		#endregion

		#region TestWE_PackageGroupId

		#region TestWE_PackageGroupId_ValidatesWE_TransactionQuantity

		public void TestWE_PackageGroupId_ValidatesWE_TransactionQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			var location = data.Whs1.FindLocation("A");
			location.WLV_WA_PickingArea = bondedArea.PK;
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var expectedErrorMessage = "This adjustment line does not match any inventory packed into Package Group ID '456'.";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = AdjustmentType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "KEY-1", "123", 5m).WI_WL = location.PK;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, "KEY-1", "456", 3m).WI_WL = location.PK;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, location, "KEY-1", "123", 5m);
			AssertNoError(adjustmentLine.WE_TransactionQuantityInfo, expectedErrorMessage);

			adjustmentLine.WE_PackageGroupId = "456";
			AssertHasError(adjustmentLine.WE_TransactionQuantityInfo, expectedErrorMessage);
		}

		#endregion

		#region TestWE_PackageGroupId_ValidatesWE_PerPackageQty

		public void TestWE_PackageGroupId_ValidatesWE_PerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var location = data.Whs1.FindLocation("A");

			var expectedErrorMessage = "Per Group Quantity must be specified if Package Group ID is specified.";

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, location, "KEY-1", "", 0m);
			AssertNoError(adjustmentLine.WE_PerPackageQtyInfo, expectedErrorMessage);

			adjustmentLine.WE_PackageGroupId = "456";
			AssertHasError(adjustmentLine.WE_PerPackageQtyInfo, expectedErrorMessage);
		}

		#endregion

		#endregion

		#region TestWE_PerPackageQty

		public void TestWE_PerPackageQty_ValidatesWE_TransactionQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			var location = data.Whs1.FindLocation("A-1");
			location.WLV_WA_PickingArea = bondedArea.PK;

			var expectedErrorMessage = "Only full packages (by Package Group ID) can be adjusted into a location.";

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, location, "KEY-1", "123", 5m);
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, 10m, location, "KEY-1", "123", 5m);
			AssertNoError(adjustmentLine1.WE_TransactionQuantityInfo, expectedErrorMessage);
			AssertNoError(adjustmentLine2.WE_TransactionQuantityInfo, expectedErrorMessage);

			adjustmentLine2.WE_PerPackageQty = 2m;
			AssertHasError(adjustmentLine2.WE_TransactionQuantityInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCommittedQuantity

		public void TestCommittedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -3m, data.Whs1.FindLocation("A"));
			var line2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 4m, data.Whs1.FindLocation("A"));
			AssertEquals("Precondition: No stock is committed.", 0m, line1.CommittedQuantity);
			AssertEquals("Precondition: No stock is committed.", 0m, line2.CommittedQuantity);

			adjustment.RunPreSaveValidation();
			AssertEquals("Committed Qty should be the sum of picklines from the adjustment line.", 3m, line1.CommittedQuantity);
			AssertEquals("Committed Qty should be the Zero for positive adjustment lines.", 0m, line2.CommittedQuantity);
		}

		#endregion

		#region TestIsAdjustmentOut

		public void TestIsAdjustmentOut()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 4m, data.Whs1.FindLocation("A"));

			AssertEquals("isAdjustOut should be false", false, adjustmentLine.IsAdjustmentOut);
			adjustmentLine.WE_TransactionQuantity = -4m;
			AssertEquals("isAdjustOut should be true", true, adjustmentLine.IsAdjustmentOut);
		}

		#endregion

		#region TestIsAdjustmentIn

		public void TestIsAdjustmentIn()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 4m, data.Whs1.FindLocation("A"));

			AssertEquals("Precondition: Transaction quantity is greater than 0.", true, adjustmentLine.WE_TransactionQuantity > 0);
			AssertEquals("IsAdjustmentIn should be true", true, adjustmentLine.IsAdjustmentIn);
			adjustmentLine.WE_TransactionQuantity = -4m;
			AssertEquals("IsAdjustmentIn should be false", false, adjustmentLine.IsAdjustmentIn);
			adjustmentLine.WE_TransactionQuantity = 0m;
			AssertEquals("IsAdjustmentIn should be false", false, adjustmentLine.IsAdjustmentIn);
		}

		#endregion

		#region ReadOnly

		#region TestWE_PartAttrib1InfoCore

		protected override void TestWE_PartAttrib1InfoCore(TestDataSimpleEnvironment data)
		{
			base.TestWE_PartAttrib1InfoCore(data);
			AssertPartAttributesReadOnly(WhsDocketLineSchema.WE_PartAttrib1.Name);
		}

		#endregion

		#region TestWE_PartAttrib2InfoCore

		protected override void TestWE_PartAttrib2InfoCore(TestDataSimpleEnvironment data)
		{
			base.TestWE_PartAttrib2InfoCore(data);
			AssertPartAttributesReadOnly(WhsDocketLineSchema.WE_PartAttrib2.Name);
		}

		#endregion

		#region TestWE_PartAttrib3InfoCore

		protected override void TestWE_PartAttrib3InfoCore(TestDataSimpleEnvironment data)
		{
			base.TestWE_PartAttrib3InfoCore(data);
			AssertPartAttributesReadOnly(WhsDocketLineSchema.WE_PartAttrib3.Name);
		}

		#endregion

		#region TestWE_SerialNumberInfoCore

		protected override void TestWE_SerialNumberInfoCore(TestDataSimpleEnvironment data)
		{
			base.TestWE_SerialNumberInfoCore(data);
			AssertPartAttributesReadOnly(WhsDocketLineSchema.WE_SerialNumber.Name);
		}

		#endregion

		#region TestWE_PackingDateInfoCore

		protected override void TestWE_PackingDateInfoCore(TestDataSimpleEnvironment data)
		{
			base.TestWE_PackingDateInfoCore(data);
			AssertPartAttributesReadOnly(WhsDocketLineSchema.WE_PackingDate.Name);
		}

		#endregion

		#region TestPackingDateCalculatesExpiryDate

		public void TestPackingDateCalculatesExpiryDate_AdjustmentOut()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParam.W3_MaximumShelfLife = 10;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine.WE_PackingDate = new ZDate(2020, 04, 15);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();

			Assert("Receive is finalied", receive.IsFinalised);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -4m, data.Whs1.FindLocation("A"));

			AssertEquals("Precondition: adjustment line is adjustment out.", true, adjustmentLine.IsAdjustmentOut);
			AssertEquals("Precondition: Packing date is empty.", ZDate.Empty, adjustmentLine.WE_PackingDate);
			AssertEquals("Precondition: Packing date is not readonly.", false, adjustmentLine.WE_PackingDateInfo.ReadOnly);
			AssertEquals("Precondition: Expiry date is empty.", ZDate.Empty, adjustmentLine.WE_ExpiryDate);
			AssertEquals("Precondition: Expiry date is not readonly.", false, adjustmentLine.WE_ExpiryDateInfo.ReadOnly);

			adjustmentLine.WE_PackingDate = new ZDate(2020, 04, 15);
			AssertEquals("Expiry date is not calculated from packing date and shelf life.", ZDate.Empty, adjustmentLine.WE_ExpiryDate);
		}

		protected override bool TestDocketLineCanCalculateExpiryDateFromPackingDate => true;

		#endregion

		#region TestWE_ExpiryDateInfoCore

		protected override void TestWE_ExpiryDateInfoCore(TestDataSimpleEnvironment data)
		{
			base.TestWE_ExpiryDateInfoCore(data);
			AssertPartAttributesReadOnly(WhsDocketLineSchema.WE_ExpiryDate.Name);
		}

		#endregion

		#region TestWE_BondedEntryKeyInfoCore

		protected override void TestWE_BondedEntryKeyInfoCore(ZPropertyInfo wE_BondedEntryKeyInfo)
		{
			var adjustment = ((WhsDocketLine)wE_BondedEntryKeyInfo.BizObj).Docket;
			adjustment.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals("Precondition: Adjusmtent is a customs transaction.", true, adjustment.IsCustomsTransaction);
			AssertEquals("WE_BondedEntryKeyInfo is not readonly if adjustment is a customs transaction.", false, wE_BondedEntryKeyInfo.ReadOnly);

			adjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			AssertEquals("Precondition: Adjusmtent is not a customs transaction.", false, adjustment.IsCustomsTransaction);
			AssertEquals("WE_BondedEntryKeyInfo is readonly if adjustment is not a customs transaction.", true, wE_BondedEntryKeyInfo.ReadOnly);
		}

		#endregion

		#region TestStandardReadOnly

		protected override void TestStandardReadOnly(ZPropertyInfo info)
		{
			base.TestStandardReadOnly(info);
			AssertPropertyInfoReadonly(info);
		}

		void AssertPropertyInfoReadonly(ZPropertyInfo info)
		{
			var childAdjustment = Factory.New<WhsAdjustment>();
			var adjustment = ((WhsDocketLine)info.BizObj).Docket;
			adjustment.WD_DocketStatus = DocketStatus.Codes.Entered;
			adjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			childAdjustment.WD_WD_ParentDocket = adjustment.PK;
			AssertEquals("Property " + info.Name + " should be not readonly", false, info.ReadOnly);

			adjustment.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals("Property " + info.Name + " should be readonly", true, info.ReadOnly);

			adjustment.WD_FinalisedDate = ZDateTimeOffset.Empty;
			adjustment.WD_DocketStatus = DocketStatus.Codes.Entered;
			childAdjustment.WD_WD_ParentDocket = ZGuid.Empty;
			var changeOwnershipAdjustmentParent = Factory.New<WhsAdjustment>();
			adjustment.WD_WD_ParentDocket = changeOwnershipAdjustmentParent.PK;
			AssertEquals("Property " + info.Name + " should be readonly", true, info.ReadOnly);

			adjustment.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals("Property " + info.Name + " should be readonly", true, info.ReadOnly);
		}

		#endregion

		#region TestHumanReadableShortcutName

		protected override void TestHumanReadableShortcutNameCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var org = Helper.CreateClient("ABC");
			var product = Helper.CreateProduct("TESTPROD", org);
			var adjustment = Helper.CreateWhsAdjustment(org, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, product, 10m, data.Whs1.DefaultLocation);
			adjustmentLine.WE_AdjustmentArrivalDate = new ZDateTimeOffset(2022, 12, 12);
			adjustment.FinaliseDocket();

			AssertEquals("ABC - TESTPROD - 12-Dec-22", adjustmentLine.HumanReadableShortcutName);
		}

		public void TestHumanReadableShortcutName_NotInventoryLine()
		{
			var adjustmentLine = GetNewBusinessObject();
			AssertEquals("Precondition", false, adjustmentLine.IsInventoryLine);
			AssertEquals("Docket Line", adjustmentLine.HumanReadableShortcutName);
		}

		#endregion

		#region AssertPartAttributesReadOnly

		void AssertPartAttributesReadOnly(ZString propertyName)
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var newClient = Helper.CreateClient("C2");
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: true);

			// Normal Adjustment
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, data.Whs1.DefaultLocation);
			AssertEnteredAndFinalised(adjustment, adjustmentLine, propertyName, false, true);

			// Ownership Adjustment Parent
			var newOwnershipAdjustmentParent = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1", Notify, newClient);
			var adjustmentLineInParent = Helper.CreateWhsAdjustmentLine(newOwnershipAdjustmentParent, data.Part1, -1m, data.Whs1.DefaultLocation);
			AssertEnteredAndFinalised(newOwnershipAdjustmentParent, adjustmentLineInParent, propertyName, false, true);

			// Ownership Adjustment Child
			var newOwnershipAdjustmentChild = newOwnershipAdjustmentParent.ChildAdjustment;
			var adjustmentLineInChild = Helper.CreateWhsAdjustmentLine(newOwnershipAdjustmentChild, data.Part1, 1m, data.Whs1.DefaultLocation);
			AssertEnteredAndFinalised(newOwnershipAdjustmentChild, adjustmentLineInChild, propertyName, true, true);
		}

		void AssertEnteredAndFinalised(WhsAdjustment adjustment, WhsAdjustmentLine adjustmentLine, ZString propertyName, bool isEntered, bool isFinalised)
		{
			var propertyInfoToTest = adjustmentLine.FindPropertyInfo(propertyName);
			adjustment.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals(string.Format("When adjustment line is entered, {0} should {1}be readonly.", propertyName, isEntered ? "" : "not "), isEntered, propertyInfoToTest.ReadOnly);

			adjustment.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals(string.Format("When adjustment line is finalised, {0} should not be readonly.", propertyName), isFinalised, propertyInfoToTest.ReadOnly);
		}

		#endregion

		#endregion

		#region TestBOMComponentLinks

		public override bool TestBOMComponentLinks_ExpectedLinkResult => false;

		#endregion

		#endregion

		#region TestConstraints

		#region TestConstraint_WE_CurrentInventoryStatus

		[ExpectNoExceptions]
		public void TestConstraint_WE_CurrentInventoryStatus_Finalized()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var expectedExceptionMsg = "The UPDATE statement conflicted with the CHECK constraint \"Constraint_WE_CurrentInventoryStatus";

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.DefaultLocation);
			adjustment.FinaliseDocket();

			// AVL
			Factory.Save();
			AssertEquals("Expected 'AVL' current inventory status.", InventoryStatus.Codes.Available, adjustmentLine.WE_CurrentInventoryStatus);
			AssertEquals("Docket line must be finalized.", true, adjustmentLine.IsFinalised);

			// HEL
			adjustmentLine.WE_WHC_NKCurrentInventoryHeldCode = "HOLD";
			adjustmentLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Held;
			Factory.Save();
			AssertEquals("Expected 'HEL' current inventory status.", InventoryStatus.Codes.Held, adjustmentLine.WE_CurrentInventoryStatus);

			// PUT: Not Allowed
			adjustmentLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Putaway;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), expectedExceptionMsg, true), "Exception expected from Constraint_WE_CurrentInventoryStatus");
		}

		public void TestConstraint_WE_CurrentInventoryStatus_StagedAdjustmentOutIsValid()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.DefaultLocation, "");
			var inventoryLine = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();

			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Available (original Inventory's status).", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);

			var inventory = transferLine.Inventory[0];
			Factory.Save();

			ReleaseLineReductionManager.ReduceStock(orderLine.ReleaseLines[0], pickLine.WZ_Units, new PickedStockAdjustersFactory(), ReduceStockReason.Lost);

			var adjustment = Factory.Load<WhsAdjustment>(new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, order.PK))[0];
			AssertEquals("Expected adjustment to have 1 line", 1, adjustment.Lines.Count);
			AssertEquals("Expected adjustment to be finalized", true, adjustment.IsFinalised);

			var adjustmentLine = adjustment.Lines[0];
			CombineAssertions(() =>
			{
				AssertEquals("Staged status should be allowed for adjustment out", InventoryStatus.Codes.Staged, adjustmentLine.WE_OriginalInventoryStatus);
				AssertEquals("Staged status should be allowed for adjustment out", InventoryStatus.Codes.Staged, adjustmentLine.WE_CurrentInventoryStatus);
			});
		}

		protected override WhsAdjustmentLine SetupDocketLineForCurrentInventoryStatusConstraintTest()
		{
			var docketLine = base.SetupDocketLineForCurrentInventoryStatusConstraintTest();
			docketLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;
			docketLine.WE_WL = docketLine.Docket.Warehouse.DefaultLocation.PK;

			return docketLine;
		}

		#endregion

		#endregion

		#region Methods

		#region TestUncommitOverPickedOrNonMatchingInventory

		#region TestUncommitOverPickedOrNonMatchingInventory

		public void TestUncommitOverPickedOrNonMatchingInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locations[0], "");
			var inventory = receive.Inventory[0];

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -20m, locations[0]);
			adjustmentLine.RunPreSaveValidation();
			AssertEquals("Precondition - ensure stock is committed.", 20m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure stock is committed.", 20m, adjustmentLine.CommittedQuantity);

			adjustmentLine.WE_TransactionQuantity = -30m;
			adjustmentLine.UncommitOverPickedOrNotMatchingInventory();
			AssertEquals("Uncommit inventory should not commit additional stock.", 20m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Uncommit inventory should not commit additional stock.", 20m, adjustmentLine.CommittedQuantity);

			adjustmentLine.WE_TransactionQuantity = -10m;
			adjustmentLine.UncommitOverPickedOrNotMatchingInventory();
			AssertEquals("Uncommit inventory should uncommit stock if committed more that required.", 10m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Uncommit inventory should uncommit stock if committed more that required.", 10m, adjustmentLine.CommittedQuantity);

			adjustmentLine.WE_OP = data.Part2.PK;
			adjustmentLine.UncommitOverPickedOrNotMatchingInventory();
			AssertEquals("Uncommit inventory should uncommit all stock if one of parameters doesn't much anymore.", 0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Uncommit inventory should uncommit all stock if one of parameters doesn't much anymore.", 0m, adjustmentLine.CommittedQuantity);
		}

		#endregion

		#region TestUncommitOverPickedOrNotMatchingInventory_ForFinalisedAdjustmentLine

		public void TestUncommitOverPickedOrNotMatchingInventory_ForFinalisedAdjustmentLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var inventory = receive.Inventory[0];

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -20m, "A-1");
			adjustmentLine.RunPreSaveValidation();
			AssertEquals("Precondition - ensure stock is committed.", 20m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure stock is committed.", 20m, adjustmentLine.CommittedQuantity);

			adjustmentLine.WE_TransactionQuantity = -10m;
			adjustment.WD_FinalisedDate = ZDateTimeOffset.Now;
			adjustmentLine.UncommitOverPickedOrNotMatchingInventory();
			AssertEquals("Uncommit inventory for finalised line should not uncommit stock.", 20m, adjustmentLine.CommittedQuantity);
		}

		#endregion

		#endregion

		#region TestCommitInventory

		#region TestCommitInventory

		public void TestCommitInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 25m, location);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 25m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, location);
			AssertEquals("Precondition - ensure no stock is committed.", 0m, inventory1.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure no stock is committed.", 0m, adjustmentLine.CommittedQuantity);

			adjustmentLine.RunPreSaveValidation();
			AssertEquals("CommitInventory should commit stock.", 10m, inventory1.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 10m, adjustmentLine.CommittedQuantity);

			adjustmentLine.WE_TransactionQuantity = -100m;
			adjustmentLine.RunPreSaveValidation();
			AssertEquals("CommitInventory should commit stock.", 25m, inventory1.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 25m, inventory2.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 50m, adjustmentLine.CommittedQuantity);

			adjustmentLine.WE_TransactionQuantity = 100m;
			AssertEquals("Precondition: Has Pick Lines.", 2, adjustmentLine.PickLines.Count);
			adjustmentLine.RunPreSaveValidation();
			AssertEquals("Calling Commit on a Positive Adjustment Line should clear out any existing Pick Lines.", 0, adjustmentLine.PickLines.Count);
		}

		#endregion

		#region TestCommitInventory_ForFinalisedAdjustmentLine

		public void TestCommitInventory_ForFinalisedAdjustmentLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var inventory = receive.Inventory[0];

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, "A-1");
			AssertEquals("Precondition - ensure no stock is committed.", 0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure no stock is committed.", 0m, adjustmentLine.CommittedQuantity);

			adjustment.WD_FinalisedDate = ZDateTimeOffset.Now;
			adjustmentLine.RunPreSaveValidation();
			AssertEquals("CommitInventory for finalised line should not commit stock.", 0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory for finalised line should not commit stock.", 0m, adjustmentLine.CommittedQuantity);
			adjustment.WD_FinalisedDate = ZDateTimeOffset.Empty; // clean up

			adjustmentLine.CommitInventory();
			AssertEquals("CommitInventory for not finalised job / line should commit inventory.", 10m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory for not finalised job / line should commit inventory.", 10m, adjustmentLine.GetQtyCommittedToThisLine());
		}

		#endregion

		#region TestCommitInventory_US

		public void TestCommitInventory_US()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			locations[0].WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, locations[0].PK, "123-1", "ABC", 5m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m, locations[0].PK, "123-1", "XYZ", 10m);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, locations[0].PK);
			adjustmentLine.WE_BondedEntryKey = "123-1";
			adjustmentLine.WE_PackageGroupId = "ABC";
			adjustmentLine.RunPreSaveValidation();
			AssertContainsExactElementsInAnyOrder(new[] { inventory1 }, adjustmentLine.PickLines.Select(p => p.Inventory));
		}

		#endregion

		#endregion

		public void TestSetDefaultOriginBondLocation()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region Find Attributes

		protected override void SetInventoryDataForUseChosenInventoryRowMethod(WhsInventoryView inventory)
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 2, 1);
			base.SetInventoryDataForUseChosenInventoryRowMethod(inventory);
			inventory.WI_WL = whs.Rows.Single(r => r.WR_Name == "A").Locations[1].PK;
			inventory.WI_ArrivalDate = ZDateTimeOffset.Today;
			inventory.InDocketLine.WE_ExpiryDate = inventory.WI_ExpiryDate;
		}

		protected override void AssertUseChosenInventoryRowHasSetProperties(WhsInventoryView expected, WhsDocketLine actual)
		{
			base.AssertUseChosenInventoryRowHasSetProperties(expected, actual);
			AssertEquals("Location: ", expected.WI_WL, actual.WE_WL);
			AssertEquals("Pallet", expected.WI_PalletID, actual.WE_PalletID);
		}

		#endregion

		#region TestPackTypeDefaultedFromProduct

		public void TestPackTypeDefaultedFromProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Docket.WD_OH_Client = data.Org1.PK;
			Docket.WD_WW_Whs = data.Whs1.PK;
			object createLine = DocketLine;

			WhsProduct product = WhsProduct.GetWhsProduct(data.Part1);
			WhsProductParamsByWhsAndClient productParams = product.ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_F3_NKReleasedPackType = "BAG";
			productParams.W3_F3_NKReceivedPackType = "BOX";

			DocketLine.WE_OP = data.Part2.PK;
			AssertEquals("UNT", DocketLine.WE_F3_NKPackType);

			DocketLine.WE_OP = data.Part1.PK;
			AssertEquals("Pack Type should be defaulted from W3_F3_NKReleasedPackType.", "BAG", DocketLine.WE_F3_NKPackType);

			DocketLine.WE_OP = ZGuid.Empty;

			productParams.W3_F3_NKReleasedPackType = "";
			DocketLine.WE_OP = data.Part1.PK;
			AssertEquals("W3_F3_NKReceivedPackType was empty thus Pack Type should be defaulted from W3_F3_NKReceivedPackType.", "BOX", DocketLine.WE_F3_NKPackType);
		}

		#endregion

		#region TestClone

		// TestClone test is in WhsDocketLineClass

		public void TestCloneWE_WE_OriginalDocketLineForRating()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.DefaultLocation);
			Factory.Save();

			AssertEquals("Precondition:", ZGuid.Empty, adjustmentLine.WE_WE_OriginalDocketLineForRating);
			var clonedLine = (WhsAdjustmentLine)adjustmentLine.Clone();
			adjustment.Lines.Add(clonedLine);
			AssertEquals("Precondition:", ZGuid.Empty, clonedLine.WE_WE_OriginalDocketLineForRating);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustment);
			Factory.Save(); // during save stock gets created and WE_WE_OriginalDocketLineForRating populated

			AssertEquals("WE_WE_OriginalDocketLineForRating for original line should point to PK", adjustmentLine.PK, adjustmentLine.WE_WE_OriginalDocketLineForRating);
			AssertEquals("WE_WE_OriginalDocketLineForRating for cloned line should point to PK", clonedLine.PK, clonedLine.WE_WE_OriginalDocketLineForRating);
		}

		#endregion

		#region TestAdjustmentIn

		public void TestAdjustmentIn_WithJulianBatchNumber_RecalculateExpiryDateAndPackingDate1()
		{
			AssertAdjustmentIn_WithJulianBatchNumber_RecalculateExpiryDateAndPackingDate(AttributeNumber.One, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestAdjustmentIn_WithJulianBatchNumber_RecalculateExpiryDateAndPackingDate2()
		{
			AssertAdjustmentIn_WithJulianBatchNumber_RecalculateExpiryDateAndPackingDate(AttributeNumber.Two, WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestAdjustmentIn_WithJulianBatchNumber_RecalculateExpiryDateAndPackingDate3()
		{
			AssertAdjustmentIn_WithJulianBatchNumber_RecalculateExpiryDateAndPackingDate(AttributeNumber.Three, WhsDocketLineSchema.WE_PartAttrib3);
		}

		void AssertAdjustmentIn_WithJulianBatchNumber_RecalculateExpiryDateAndPackingDate(AttributeNumber attributeNumber, SchemaColumn partAttributeColumn)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true);
			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;

			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParam.W3_MaximumShelfLife = 9001;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A"), "a");

			receiveLine[partAttributeColumn] = "7093 A467 789F";
			receiveLine.WE_ExpiryDate = new ZDate(ZDateTime.Now.Year + 2, 2, 1);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjLine1 = adjustment.CreateDocketLineFromInventory(receiveLine.Inventory[0]);
			AssertEquals("Precondition: Inventory line expiry date should be copied to adjustment line.", receiveLine.Inventory[0].WI_ExpiryDate, adjLine1.WE_ExpiryDate);
			AssertEquals("Precondition: Inventory line expiry date should be copied to adjustment line.", receiveLine.Inventory[0].WI_PackingDate, adjLine1.WE_PackingDate);

			adjLine1.WE_TransactionQuantity = 200m;
			AssertEquals($"Expiry date should be recalculated when adjustment line quantity is changed to positive and using a Julian Batch Number in part attribute {attributeNumber}.", new ZDateTime(2041, 11, 24, 0, 0, 0), adjLine1.WE_ExpiryDate);
			AssertEquals($"Packing date should be recalculated when adjustment line quantity is changed to positive and using a Julian Batch Number in part attribute {attributeNumber}.", new ZDateTime(2017, 04, 03, 0, 0, 0), adjLine1.WE_PackingDate);

			adjLine1[partAttributeColumn] = "7083 A467 789F";
			AssertEquals($"Expiry date should be recalculated when adjustment line quantity is positive and Julian Batch Number is changed in part attribute {attributeNumber}.", new ZDateTime(2041, 11, 14, 0, 0, 0), adjLine1.WE_ExpiryDate);
			AssertEquals($"Packing date should be recalculated when adjustment line quantity is positive and Julian Batch Number is changed in part attribute {attributeNumber}.", new ZDateTime(2017, 03, 24, 0, 0, 0), adjLine1.WE_PackingDate);
		}

		#endregion

		#region TestVasOrderTransferInInventoryCanOnlyBePickedByVasOrderTransferOut

		public void TestVasOrderTransferInInventoryCanOnlyBePickedByVasOrderTransferOut()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Transfer In is created.", intoServiceAreaTransfer);

			intoServiceAreaTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(intoServiceAreaTransfer);

			var transferInInventory = intoServiceAreaTransfer.Lines.Single();
			AssertEquals("Precondition: inventory status is staged.", InventoryStatus.Codes.Staged, transferInInventory.WE_CurrentInventoryStatus);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine = adjustment.CreateDocketLineFromInventory(transferInInventory.Inventory[0]);
			adjustmentLine.WE_TransactionQuantity = -4;
			adjustmentLine.WE_ReasonCode = "CLI";
			adjustmentLine.RunPreSaveValidation();

			AssertHasErrorContaining(adjustmentLine.WE_TransactionQuantityInfo, "Attempted to adjust 4 Units, but no Units are available for adjustment out of this location.");
		}

		public void TestVasOrderTransferInInventoryCanOnlyBePickedByVasOrderTransferOut_FinalisedVasOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Transfer In is created.", intoServiceAreaTransfer);

			var transferInLine = (WhsTransferLine)intoServiceAreaTransfer.Lines.Single();
			transferInLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			intoServiceAreaTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(intoServiceAreaTransfer);
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("VAS order is completed", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			vasOrder.FinaliseVASOrder(Notify, false);
			AssertEquals("VAS order is finalised.", true, vasOrder.IsFinalised);
			Factory.Save();

			var transferInInventory = intoServiceAreaTransfer.Lines.Single();
			AssertEquals("Precondition: inventory status is available.", InventoryStatus.Codes.Available, transferInInventory.WE_CurrentInventoryStatus);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine = adjustment.CreateDocketLineFromInventory(transferInInventory.Inventory[0]);
			adjustmentLine.WE_TransactionQuantity = -4;
			adjustmentLine.WE_ReasonCode = "CLI";
			adjustmentLine.RunPreSaveValidation();
			adjustment.FinaliseDocketWithoutUserConfirmation();

			AssertEquals("Adjustment is finalised.", true, adjustment.IsFinalised);
		}

		#endregion

		#region TestCheckAdjustmentLineUniqueSerialNumber

		public void TestCheckAdjustmentLineUniqueSerialNumber()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, true, false);
				var receiveLine = receive.Lines[0];
				receiveLine.SerialNumbers.AddNew().SerialNumberValue = "SN01";
				receive.FinaliseDocketWithoutUserConfirmation();
				Factory.Save();
				AssertIsFinalisedPrecondition(receive);

				var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
				var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, data.Whs1.DefaultLocation);
				adjustmentLine1.SerialNumbers.AddNew().SerialNumberValue = "SN01";
				var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, data.Whs1.DefaultLocation);
				adjustmentLine2.SerialNumbers.AddNew().SerialNumberValue = "SN02";

				adjustment.RunPreSaveValidation();

				AssertEquals(@"Error - WSV_WSN_SerialNumber: Serial # already used.",
				adjustmentLine1.NotificationsIncludingChildren.GetErrors().ToUniqueMessageListString());

				AssertEquals(false, adjustmentLine2.NotificationsIncludingChildren.HasErrors());
			}
		}

		#endregion

		#region TestCheckAdjustmentLineUniqueSerialNumber

		public void TestDefaultSelectSerialNumberInAdjustmentOut_NoAvailable()
		{
			TestDefaultSelectSerialNumberInAdjustmentOutCore(avalebleInventory: 0);
		}

		public void TestDefaultSelectSerialNumberInAdjustmentOut_LessAvailable()
		{
			TestDefaultSelectSerialNumberInAdjustmentOutCore(avalebleInventory: 1);
		}

		public void TestDefaultSelectSerialNumberInAdjustmentOut_Excat()
		{
			TestDefaultSelectSerialNumberInAdjustmentOutCore(avalebleInventory: 2);
		}

		public void TestDefaultSelectSerialNumberInAdjustmentOut_MoreAvailable()
		{
			TestDefaultSelectSerialNumberInAdjustmentOutCore(avalebleInventory: 3);
		}

		void TestDefaultSelectSerialNumberInAdjustmentOutCore(int avalebleInventory)
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, avalebleInventory, true, false);
				var receiveLine = receive.Lines[0];
				for (int i = 0; i < avalebleInventory; i++)
				{
					receiveLine.SerialNumbers.AddNew().SerialNumberValue = $"SN{i + 1}";
				}
				receive.FinaliseDocketWithoutUserConfirmation();
				Factory.Save();
				AssertIsFinalisedPrecondition(receive);

				var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
				var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -2m, data.Whs1.DefaultLocation);
				var availableSerialNumbers = adjustmentLine.SerialNumberSelectors;
				AssertEquals(avalebleInventory, availableSerialNumbers.Count);
				AssertContainsExactElementsInExactOrder((new[] { "SN1", "SN2" }).Take(avalebleInventory),
					availableSerialNumbers.Where(s => s.Selected).Select(s => s.SerialNumberValue));
			}
		}

		#endregion

		#region TestDefaultSelectSerialNumberInAdjustmentOut

		public void TestDefaultSelectSerialNumberInAdjustmentOut_ChangeSelection_OneInventory()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 3, true, false);
				var receiveLine = receive.Lines[0];
				for (int i = 0; i < 3; i++)
				{
					receiveLine.SerialNumbers.AddNew().SerialNumberValue = $"SN{i + 1}";
				}
				receive.FinaliseDocketWithoutUserConfirmation();
				Factory.Save();
				AssertIsFinalisedPrecondition(receive);

				var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
				var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -2m, data.Whs1.DefaultLocation);
				var availableSerialNumbers = adjustmentLine.SerialNumberSelectors;
				AssertEquals(3, availableSerialNumbers.Count);
				var selectedSerials = availableSerialNumbers.Where(s => s.Selected);
				AssertAdjustmentLineValues(-2, ["SN1", "SN2"]);

				availableSerialNumbers[0].Selected = false;
				AssertAdjustmentLineValues(-1, ["SN2"]);

				availableSerialNumbers[2].Selected = true;
				AssertAdjustmentLineValues(-2, ["SN2", "SN3"]);

				void AssertAdjustmentLineValues(decimal transactionQuantity, string[] expectedSelectedSNs)
				{
					AssertContainsExactElementsInExactOrder(expectedSelectedSNs, selectedSerials.Select(s => s.SerialNumberValue));
					AssertEquals(transactionQuantity, adjustmentLine.WE_TransactionQuantity);
					AssertEquals(-transactionQuantity, adjustmentLine.CommittedQuantity);
					// Make sure selection does not changes
					AssertContainsExactElementsInExactOrder(expectedSelectedSNs, selectedSerials.Select(s => s.SerialNumberValue));
				}
			}
		}

		public void TestDefaultSelectSerialNumberInAdjustmentOut_ChangeSelection_ManyInventory()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				for (int i = 0; i < 3; i++)
				{
					var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
					receiveLine.SerialNumbers.AddNew().SerialNumberValue = $"SN{i + 1}";
				}
				receive.FinaliseDocketWithoutUserConfirmation();
				Factory.Save();
				AssertIsFinalisedPrecondition(receive);

				var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
				var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -2m, data.Whs1.DefaultLocation);
				var availableSerialNumbers = adjustmentLine.SerialNumberSelectors;
				AssertEquals(2, availableSerialNumbers.Count); // is not load all available serial numbers
				var selectedSerials = availableSerialNumbers.Where(s => s.Selected);
				AssertAdjustmentLineValues(-2, ["SN1", "SN2"]);

				availableSerialNumbers[0].Selected = false;
				AssertAdjustmentLineValues(-1, ["SN2"]);

				void AssertAdjustmentLineValues(decimal transactionQuantity, string[] expectedSelectedSNs)
				{
					AssertContainsExactElementsInExactOrder(expectedSelectedSNs, selectedSerials.Select(s => s.SerialNumberValue));
					AssertEquals(transactionQuantity, adjustmentLine.WE_TransactionQuantity);
					AssertEquals(-transactionQuantity, adjustmentLine.CommittedQuantity);
					// Make sure selection does not changes
					AssertContainsExactElementsInExactOrder(expectedSelectedSNs, selectedSerials.Select(s => s.SerialNumberValue));
				}
			}
		}

		public void TestDefaultSelectSerialNumberInAdjustmentOut_SaveSelection()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 3, true, false);
				var receiveLine = receive.Lines[0];
				for (int i = 0; i < 3; i++)
				{
					receiveLine.SerialNumbers.AddNew().SerialNumberValue = $"SN{i + 1}";
				}
				receive.FinaliseDocketWithoutUserConfirmation();
				Factory.Save();
				AssertIsFinalisedPrecondition(receive);

				var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
				var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -2m, data.Whs1.DefaultLocation);
				var availableSerialNumbers = adjustmentLine.SerialNumberSelectors;
				AssertEquals(3, availableSerialNumbers.Count);
				AssertContainsExactElementsInExactOrder(["SN1", "SN2"], availableSerialNumbers.Where(s => s.Selected).Select(s => s.SerialNumberValue));
				AssertEquals(-2m, adjustmentLine.WE_TransactionQuantity);

				availableSerialNumbers[0].Selected = false;
				AssertContainsExactElementsInExactOrder(["SN2"], availableSerialNumbers.Where(s => s.Selected).Select(s => s.SerialNumberValue));

				availableSerialNumbers[2].Selected = true;
				AssertEquals(-2m, adjustmentLine.WE_TransactionQuantity);
				AssertContainsExactElementsInExactOrder(["SN2", "SN3"], availableSerialNumbers.Where(s => s.Selected).Select(s => s.SerialNumberValue));
				Factory.Save();

				var adjustmentInNewFactory = NewFactory().Load<WhsAdjustment>(adjustment.PK);
				var serialNumbers = adjustmentInNewFactory.Lines[0].SerialNumberSelectors;
				AssertContainsExactElementsInExactOrder(["SN2", "SN3"], serialNumbers.Where(s => s.Selected).Select(s => s.SerialNumberValue));
				Factory.Save();
			}
		}

		#endregion

		#region TestCheckWE_TransactionQuantitySerialNumberQtyCheck_PivotSerialNumber

		public void TestCheckWE_TransactionQuantitySerialNumberQtyCheck_PivotSerialNumber()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, true, false);
				var receiveLine = receive.Lines[0];
				receiveLine.SerialNumbers.AddNew().SerialNumberValue = "SN01";
				receiveLine.SerialNumbers.AddNew().SerialNumberValue = "SN02";
				receive.FinaliseDocketWithoutUserConfirmation();
				Factory.Save();
				AssertIsFinalisedPrecondition(receive);

				var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
				var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, data.Whs1.DefaultLocation);
				adjustmentLine1.SerialNumbers.AddNew().SerialNumberValue = "SN01";
				var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, data.Whs1.DefaultLocation);
				var adjustmentLine3 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, data.Whs1.DefaultLocation);
				adjustmentLine3.SerialNumbers.AddNew().SerialNumberValue = "SN02";
				var adjustmentLine4 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -2m, data.Whs1.DefaultLocation);

				adjustment.RunPreSaveValidation();

				AssertNoErrors(adjustmentLine1.WE_TransactionQuantityInfo);
				AssertHasError(adjustmentLine2.WE_TransactionQuantityInfo,
					"The number of entered serial numbers 0 does not match the required quantity 2. Please ensure the serial numbers match the specified quantity.");
				AssertHasError(adjustmentLine3.WE_TransactionQuantityInfo,
					"The number of entered serial numbers 1 does not match the required quantity 2. Please ensure the serial numbers match the specified quantity.");
				AssertNoErrors(adjustmentLine4.WE_TransactionQuantityInfo);

				adjustmentLine3.SerialNumbers.AddNew().SerialNumberValue = "SN02";
				adjustment.RunPreSaveValidation();
				AssertNoErrors(adjustmentLine3.WE_TransactionQuantityInfo);
				AssertEquals(true, adjustment.HasErrors);
			}
		}

		#endregion

		#region TestHasSerialNumber

		public void TestHasSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine1.WE_SerialNumber = "SER1";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, data.Whs1.DefaultLocation);
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, -1m, data.Whs1.DefaultLocation);

			AssertEquals("EnableSchemaRedesignChanges is not enable.", false, adjustmentLine1.HasSerialNumber);
			AssertEquals("Not use serial number.", false, adjustmentLine1.HasSerialNumber);

			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("EnableSchemaRedesignChanges is enabled.", true, adjustmentLine1.HasSerialNumber);
				AssertEquals("EnableSchemaRedesignChanges is enabled but serial number not used.", false, adjustmentLine2.HasSerialNumber);
			}
		}

		#endregion

		// interfaces

		#region ILineWithCommittedPickLines Members

		public void TestILineWithCommittedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductUnit(data.Part1, "PLT", 5m);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, data.Whs1.FindLocation("A-1"));
			adjustmentLine.WE_AdjustmentArrivalDate = new ZDateTimeOffset(2014, 1, 1);
			adjustmentLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Held;
			adjustmentLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			adjustmentLine.WE_F3_NKPackType = "PLT";
			adjustmentLine.WE_LineNo = 2;
			adjustmentLine.WE_PackageGroupId = "123";
			adjustmentLine.WE_PalletID = "PLT-123";
			adjustmentLine.WE_TransactionQuantity = -5m;

			// properties
			ILineWithCommittedPickLines iLineWithCommittedPickLines = adjustmentLine;
			AssertEquals(new ZDateTimeOffset(2014, 1, 1), iLineWithCommittedPickLines.ArrivalDate);
			AssertEquals(typeof(WhsInventoryCommitter<WhsAdjustmentLine>), iLineWithCommittedPickLines.CommittedStrategy.GetType());
			AssertEquals(adjustmentLine.Inventory, iLineWithCommittedPickLines.Inventory);
			AssertEquals(new ZShort(2), iLineWithCommittedPickLines.LineNo);
			AssertEquals(data.Whs1.FindLocation("A-1"), iLineWithCommittedPickLines.LocationToCommit);
			AssertEquals("adjustment", iLineWithCommittedPickLines.Noun);
			AssertEquals("123", iLineWithCommittedPickLines.PackageGroupID);
			AssertEquals("PLT", iLineWithCommittedPickLines.PackType);
			AssertEquals("PLT-123", iLineWithCommittedPickLines.PalletIDToCommit);
			AssertEquals(adjustment, iLineWithCommittedPickLines.ParentDocket);
			AssertEquals(adjustment.PK, iLineWithCommittedPickLines.ParentDocketPK);
			AssertEquals(adjustmentLine.PickLines, iLineWithCommittedPickLines.PickLines);
			AssertEquals(adjustmentLine.Product, iLineWithCommittedPickLines.Product);
			AssertEquals(data.Part1.PK, iLineWithCommittedPickLines.ProductPK);
			AssertEquals(false, iLineWithCommittedPickLines.IsFinalising);
			AssertEquals(false, iLineWithCommittedPickLines.CanCreateInventory);
			AssertEquals(false, iLineWithCommittedPickLines.IsInTransit);
			AssertEquals(InventoryStatus.Codes.Held, iLineWithCommittedPickLines.TransactionInventoryStatus);
			AssertEquals(InventoryHoldCodes.Codes.Damaged, iLineWithCommittedPickLines.TransactionInventoryHeldCode);
			AssertEquals(data.Whs1.FindLocation("A-1").PK, iLineWithCommittedPickLines.TransactionLocation);
			AssertEquals("PLT-123", iLineWithCommittedPickLines.TransactionPalletID);
			AssertEquals("Adjustment Line should have the opposite sign for its quantity because committing works off positive quantities and you only can only commit stock for negative adjustment lines.",
				5m, iLineWithCommittedPickLines.TransactionQty);
			AssertEquals("adjust", iLineWithCommittedPickLines.Verb);

			using (new SemaphoreManager(adjustment.FinaliseDocketSemaphore))
			{
				AssertEquals("IsFinalising should be true when adjustment is finalising.", true, iLineWithCommittedPickLines.IsFinalising);
				AssertEquals("CanCreateInventory should be true when adjustment is finalising.", true, iLineWithCommittedPickLines.CanCreateInventory);
			}

			adjustmentLine.RunPreSaveValidation();
			AssertHasError(adjustmentLine.WE_TransactionQuantityInfo, @"Attempted to adjust 5 Units, but no Units are available for adjustment out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the adjustment line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to adjust.
If you are trying to adjust stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to adjust stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.");

			// setters
			AssertEquals("Precondition: Per Package Qty not set.", 0m, adjustmentLine.WE_PerPackageQty);
			iLineWithCommittedPickLines.PerPackageQty = 2m;
			AssertEquals(2m, adjustmentLine.WE_PerPackageQty);

			iLineWithCommittedPickLines.TransactionQty = 10m;
			AssertEquals(-10m, adjustmentLine.WE_TransactionQuantity);

			adjustmentLine.WE_TransactionQuantity = -5m;
			AssertEquals(5m, iLineWithCommittedPickLines.TransactionQty);

			iLineWithCommittedPickLines.ParentDocketPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, adjustmentLine.WE_WD);

			adjustmentLine.WE_WD = adjustment.PK;
			AssertEquals(adjustment.PK, iLineWithCommittedPickLines.ParentDocketPK);
		}

		#endregion

		#region IPartAttributeValidationConsumer Members

		public override void TestIsRegisteredForUniqueSerialNumberChecking()
		{
			base.TestIsRegisteredForUniqueSerialNumberChecking();

			DocketLine.WE_TransactionQuantity = 1m;
			AssertEquals("Should be registerd if positive adjustment line", true, DocketLine.IsRegisteredForUniqueSerialNumberChecking);

			DocketLine.WE_TransactionQuantity = -1m;
			AssertEquals("Should not be registerd if negative adjustment line", false, DocketLine.IsRegisteredForUniqueSerialNumberChecking);
		}

		protected override bool IsRegisteredForSerialCheck { get { return true; } }

		#region TestIsInventoryAdjustedOutOnSiblings

		protected override void TestIsInventoryAdjustedOutOnSiblingsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetClientAllAttributeType(data.Org1, false);
			var location = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			var rcvLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			rcvLine.WI_WL = location.PK;
			rcvLine.WI_PalletID = "PLT123";
			rcvLine.WI_PartAttrib1 = "123";
			rcvLine.WI_PartAttrib2 = "456";
			AssertEquals("Receive must be finalized", false, receive.IsFinalised);
			receive.FinaliseDocket();
			Factory.Save();

			AssertEquals("Receive must be finalized", true, receive.IsFinalised);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustLineIn = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, location2.WLV_LocationString, "X123");

			var adjustLineOut = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, location.WLV_LocationString, "PLT123");
			adjustLineOut.WE_PartAttrib1 = "123";
			adjustLineOut.WE_PartAttrib2 = "999";
			adjustment.RunPreSaveValidation();

			AssertEquals("Picklines Created when None Should be Present", 0, adjustLineOut.PickLines.Count);
			AssertEquals("Inventory is not adjusted out on another docket line", false, adjustLineIn.IsInventoryAdjustedOutOnSiblings(rcvLine));

			adjustLineOut.WE_PartAttrib2 = "456";
			adjustment.RunPreSaveValidation();

			AssertEquals("Picklines Not Created when One Should be Present", 1, adjustLineOut.PickLines.Count);
			AssertEquals("Inventory is adjusted out on another docket line", true, adjustLineIn.IsInventoryAdjustedOutOnSiblings(rcvLine));
		}

		#endregion

		#region TestISerialNumberParentMembers

		public void TestISerialNumberParentMembers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			ISerialNumberParent adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, data.Whs1.DefaultLocation);

			AssertEquals(adjustmentLine.PK, adjustmentLine.PK);
			AssertEquals(data.Org1.PK, adjustmentLine.ClientPK);
			AssertEquals(data.Part1.PK, adjustmentLine.ProductPK);
			AssertEquals(WhsDocketLineSchema.Constants.Prefix, adjustmentLine.TablePrefix);
			AssertEquals(true, adjustmentLine.SerialNumberReadOnly);
			AssertEquals(false, adjustmentLine.IsInDatabase);
			AssertEquals(true, adjustmentLine.IsAllowedToCreateOriginalSerialNumberRecord);
			AssertEquals(adjustment.Factory, adjustmentLine.Factory);
			AssertEquals(typeof(WhsSerialNumberPivotCollection), adjustmentLine.SerialNumbers.GetType());

			Factory.Save();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var pivot = adjustmentLine.SerialNumbers.AddNew();
			pivot.SerialNumberValue = "SN1";
			AssertEquals(false, adjustmentLine.IsSerialNumberAlreadyInUse(pivot));
			AssertEquals(true, adjustmentLine.IsInDatabase);
			AssertEquals(false, adjustmentLine.SerialNumberReadOnly);

			adjustment.WD_OH_Client = ZGuid.Empty;
			adjustment.Lines[0].WE_OP = ZGuid.Empty;
			AssertEquals("Should return value without exception.", true, adjustmentLine.SerialNumberReadOnly);

			adjustment.Lines[0].WE_TransactionQuantity = -1m;
			AssertEquals(false, adjustmentLine.IsAllowedToCreateOriginalSerialNumberRecord);
		}

		#endregion

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			var helper = new WhsTestHelperFunctions(factory);

			var adjustment = helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			return helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.DefaultLocation);
		}

		protected override string ExpectedDefaultInventoryStatus
		{
			get { return InventoryStatus.Codes.Available; }
		}

		protected override FinalisableDocketHelper<WhsAdjustment> GetNewDocketHelper(BusinessObjectFactory factory)
		{
			return new FinalisableAdjustmentHelper(factory);
		}

		protected override bool NeedDocketLine_Location
		{
			get { return true; }
		}

		#endregion
	}
}
