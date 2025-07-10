using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Bonded;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAdjustment))]
	class WhsAdjustmentTest : WhsDocketTestCase<WhsAdjustment>
	{
		#region Constructor

		public void TestConstructor_SetConcurrencyPolicy()
		{
			var adjustment = Factory.New<WhsAdjustment>();

			AssertEquals("Concurrency Policy should be strict for WD_DocketStatus.", ConcurrencyPolicy.Strict, adjustment.WD_DocketStatusInfo.ConcurrencyPolicy);
			AssertEquals("Concurrency Policy should be strict for WD_FinalisedDate.", ConcurrencyPolicy.Strict, adjustment.WD_FinalisedDateInfo.ConcurrencyPolicy);
		}

		#endregion

		#region Customs Stuff

		protected override void TestIsCustomsTransactionCore()
		{
			var adjustment = GetNewBusinessObject();
			AssertEquals(false, adjustment.IsCustomsTransaction);
			AssertEquals(false, adjustment.IsCustomsDataVisible);

			adjustment.WD_WW_Whs = Helper.CreateWarehouse("1").PK;

			Helper.EnableWarehouseForBond(adjustment.Warehouse, true);
			AssertEquals(false, adjustment.IsCustomsTransaction);
			AssertEquals(false, adjustment.IsCustomsDataVisible);

			Helper.EnableWarehouseForExcise(adjustment.Warehouse, true);
			AssertEquals(false, adjustment.IsCustomsTransaction);
			AssertEquals(false, adjustment.IsCustomsDataVisible);

			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			AssertEquals(true, adjustment.IsCustomsTransaction);
			AssertEquals(true, adjustment.IsCustomsDataVisible);
		}

		#endregion

		#region Business Object Overrides

		#region TestSetDefaultValues

		protected override void TestSetDefaultValuesCore(WhsAdjustment docket)
		{
			base.TestSetDefaultValuesCore(docket);
			AssertEquals("WD_DocketType must be 'ADJ'", CodeLists.DocketType.Codes.Adjustment, docket.WD_DocketType);
			AssertEquals("WD_ExternalReference must be 'ADJUSTMENT'", "ADJUSTMENT", docket.WD_ExternalReference);
			AssertEquals("CreateUniqueReferenceOnSaving must be true", true, docket.IsUniqueExternalReferenceCreatedOnSave);
		}

		#endregion

		#region TestIsUniqueExternalReferenceCreatedOnSaveCore

		protected override void TestIsUniqueExternalReferenceCreatedOnSaveCore()
		{
			var adjustment = GetNewBusinessObject();
			AssertEquals(true, adjustment.IsUniqueExternalReferenceCreatedOnSave);
		}

		#endregion

		#region TestOnWarehouseChanged

		public void TestOnWarehouseChanged()
		{
			var org = Helper.CreateClient();
			var whs1 = Helper.CreateWarehouse("1", "A");
			var whs2 = Helper.CreateWarehouse("2", "A");
			var whs3 = Helper.CreateWarehouse("3", "A");
			Helper.EnableWarehouseForBond(whs3, true);

			var adjustment = GetNewBusinessObject();
			adjustment.WD_OH_Client = org.PK;
			adjustment.WD_WW_Whs = whs1.PK;

			var line1 = adjustment.Lines.AddNew();
			var line2 = adjustment.Lines.AddNew();

			line1.WE_WL = whs1.DefaultLocation.PK;
			line2.WE_WL = whs1.DefaultLocation.PK;

			adjustment.WD_WW_Whs = whs2.PK;

			AssertEquals("Location on Line1 should not of changed because warehouse is not bonded", whs1.DefaultLocation.PK, line1.WE_WL);
			AssertEquals("Location on Line2 should not of changed because warehouse is not bonded", whs1.DefaultLocation.PK, line2.WE_WL);

			adjustment.WD_WW_Whs = whs3.PK;

			AssertEquals("Location on Line1 should of changed", whs3.DefaultLocation.PK, line1.WE_WL);
			AssertEquals("Location on Line2 should of changed", whs3.DefaultLocation.PK, line2.WE_WL);
		}

		[TestDate(2023, 12, 14, 2, 10, 0)]
		public void TestOnWarehouseChanged_PopulateWD_ArrivalDateDateIfRequired()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var company = Factory.Load<GlbCompany>(Env.CurrentCompanyPK);
			var branchCNNJI = company.Branches.AddNew();
			branchCNNJI.GB_Code = "NJ";
			branchCNNJI.GB_RL_NKHomePort = "CNNJI";
			whs.WW_GB_RelatedCompanyBranch = branchCNNJI.PK;

			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;

			AssertEquals("WD_ArrivalDate should be empty with new adjustment.", ZDateTimeOffset.Empty, docket.WD_ArrivalDate);

			docket.WD_WW_Whs = whs.PK;
			AssertEquals(new ZDateTimeOffset(2023, 12, 14, 10, 10, 0, TimeSpan.FromHours(8)), docket.WD_ArrivalDate);
		}

		[TestDate(2023, 12, 14, 2, 10, 0)]
		public void TestOnWarehouseChanged_PopulateWD_ArrivalDateIfRequired_OnlyIfNotInDBAndIsEmpty()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var company = Factory.Load<GlbCompany>(Env.CurrentCompanyPK);
			var branchCNNJI = company.Branches.AddNew();
			branchCNNJI.GB_Code = "NJ";
			branchCNNJI.GB_RL_NKHomePort = "CNNJI";
			whs.WW_GB_RelatedCompanyBranch = branchCNNJI.PK;

			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;
			var arrivalDate = new ZDateTimeOffset(2012, 3, 4, 5, 0, 0, TimeSpan.FromHours(2));
			docket.WD_ArrivalDate = arrivalDate;
			AssertEquals("WD_ArrivalDate is set.", arrivalDate, docket.WD_ArrivalDate);

			docket.WD_WW_Whs = whs.PK;
			AssertEquals("WD_ArrivalDate won't change.", arrivalDate, docket.WD_ArrivalDate);
		}

		#endregion

		#region TestOnWarehouseChangedForNewOwnershipAdjustedClient

		public void TestOnWarehouseChangedForNewOwnershipAdjustedClient()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var newWarehouse = Helper.CreateWarehouse("W2");
			var adjustmentParent = GetNewBusinessObject();
			var adjustmentChild = GetNewBusinessObject();
			adjustmentParent.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			adjustmentChild.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			adjustmentChild.WD_WD_ParentDocket = adjustmentParent.PK;
			AssertNull("Precondition", adjustmentParent.Warehouse);
			AssertNull("Precondition", adjustmentChild.Warehouse);

			adjustmentParent.WD_WW_Whs = data.Whs1.PK;
			AssertEquals(data.Whs1, adjustmentParent.Warehouse);
			AssertEquals(data.Whs1, adjustmentChild.Warehouse);

			adjustmentParent.WD_WW_Whs = newWarehouse.PK;
			AssertEquals(newWarehouse, adjustmentParent.Warehouse);
			AssertEquals(newWarehouse, adjustmentChild.Warehouse);

			adjustmentParent.WD_WW_Whs = ZGuid.Empty;
			AssertNull(adjustmentParent.Warehouse);
			AssertNull(adjustmentChild.Warehouse);
		}

		#endregion

		#region TestNewOwnershipAdjustedClient_ShouldCopyWarehouseFromParent

		public void TestNewOwnershipAdjustedClient_ShouldCopyWarehouseFromParent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			Helper.EnableWarehouseForBond(whs, true);
			Helper.EnableWarehouseForFreeStore(whs, false);
			var adjustmentParent = GetNewBusinessObject();
			var adjustmentChild = GetNewBusinessObject();
			adjustmentParent.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			adjustmentChild.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			adjustmentChild.WD_WD_ParentDocket = adjustmentParent.PK;

			adjustmentParent.WD_WW_Whs = whs.PK;
			AssertHasError("OwnershipAdjustment should have validation error for bonded warehouse.", adjustmentParent.WD_WW_WhsInfo, WhsDocketValidation.FreeStoreNotEnabledErrorMsg);
		}

		#endregion

		#region TestOnWarehouseChanged_BondedWarehouseSubType

		public void TestOnWarehouseChanged_BondedWarehouseSubType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = GetNewBusinessObject();
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Adjustment;
			AssertNull("Precondition", adjustment.Warehouse);

			Helper.EnableWarehouseForBond(data.Whs1, true);
			adjustment.WD_WW_Whs = data.Whs1.PK;
			AssertEquals("Should be bonded warehouse.", true, adjustment.IsWarehouseBondEnabled);
			AssertEquals("Should not change sub type.", AdjustmentType.Codes.Adjustment, adjustment.WD_DocketSubType);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var adjustment = GetNewBusinessObject();
			adjustment.WD_DocketID = "A00001001";
			AssertEquals("Warehouse Adjustment A00001001", adjustment.HumanReadableName);
		}

		#endregion

		#endregion

		#region Related Entities

		#region TestLines

		protected override Type ExpectedLineCollectionType => typeof(WhsAdjustmentLineCollection);

		public void TestLines_IsOriginalInventory()
		{
			var adjustment = GetNewBusinessObject();
			var adjustmentLine1 = adjustment.Lines.AddNew();
			var adjustmentLine2 = adjustment.Lines.AddNew();
			AssertEquals("Both lines should be in the collection", 2, adjustment.Lines.Count);

			adjustmentLine2.WE_IsOriginalInventory = false;
			AssertEquals("Only original inventory lines should be in the collection.", 1, adjustment.Lines.Count);
			AssertContainsExactElementsInAnyOrder(new[] { adjustmentLine1 }, adjustment.Lines);
		}

		#endregion

		#region TestChildAdjustment

		public void TestChildAdjustment()
		{
			var adjustment = GetNewBusinessObject();
			var childAdjustment = GetNewBusinessObject();
			adjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			childAdjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			AssertNull("Precondition", adjustment.ChildAdjustment);
			AssertNull("Precondition", childAdjustment.ChildAdjustment);

			childAdjustment.WD_WD_ParentDocket = adjustment.PK;
			AssertEquals(childAdjustment, adjustment.ChildAdjustment);
			AssertNull(childAdjustment.ChildAdjustment);
		}

		#endregion

		#region TestOwnershipAdjustedClient

		public void TestOwnershipAdjustedClient()
		{
			var newClient = Factory.New<OrgHeader>();
			var adjustment = GetNewBusinessObject();
			var childAdjustment = GetNewBusinessObject();
			adjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			childAdjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			childAdjustment.WD_WD_ParentDocket = adjustment.PK;
			adjustment.Lines.AddNew();
			adjustment.OwnershipAdjustedClientPK = newClient.PK;
			AssertEquals("OwnershipAdjustedClient should not be readonly.", false, adjustment.OwnershipAdjustedClientPKInfo.ReadOnly);
			AssertEquals("Client on the child adjustment should be readonly.", true, childAdjustment.WD_OH_ClientInfo.ReadOnly);

			adjustment.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals("OwnershipAdjustedClient should be readonly for a finalised Adjustment.", true, adjustment.OwnershipAdjustedClientPKInfo.ReadOnly);
			AssertEquals("New client should be readonly", true, childAdjustment.WD_OH_ClientInfo.ReadOnly);
		}

		#endregion

		#region TestNewOwnershipAdjustedClient

		public void TestNewOwnershipAdjustedClient()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var adjustment = GetNewBusinessObject();
			var childAdjustment = GetNewBusinessObject();
			childAdjustment.WD_WD_ParentDocket = adjustment.PK;
			adjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			childAdjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			AssertEquals("Precondition", ZGuid.Empty, adjustment.OwnershipAdjustedClientPK);
			AssertNull("Precondition", adjustment.OwnershipAdjustedClient);
			AssertEquals("Precondition", ZGuid.Empty, childAdjustment.WD_OH_Client);
			AssertNull("Precondition", childAdjustment.Client);

			// assign initial client
			adjustment.OwnershipAdjustedClientPK = client1.PK;
			AssertEquals(client1.PK, adjustment.OwnershipAdjustedClientPK);
			AssertEquals(client1, adjustment.OwnershipAdjustedClient);
			AssertEquals(client1.PK, childAdjustment.WD_OH_Client);
			AssertEquals(client1, childAdjustment.Client);

			// change client
			adjustment.OwnershipAdjustedClientPK = client2.PK;
			AssertEquals(client2.PK, adjustment.OwnershipAdjustedClientPK);
			AssertEquals(client2, adjustment.OwnershipAdjustedClient);
			AssertEquals(client2.PK, childAdjustment.WD_OH_Client);
			AssertEquals(client2, childAdjustment.Client);

			// remove client
			adjustment.OwnershipAdjustedClientPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, adjustment.OwnershipAdjustedClientPK);
			AssertNull(adjustment.OwnershipAdjustedClient);
			AssertEquals(ZGuid.Empty, childAdjustment.WD_OH_Client);
			AssertNull(childAdjustment.Client);
		}

		#endregion

		#region TestSerialNumber

		public void TestCheckSerialNumberIsUnique_HasError()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Factory.Save();

				var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
				var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, data.Whs1.DefaultLocation);
				var pivot1 = adjustmentLine.SerialNumbers.AddNew();
				pivot1.SerialNumberValue = "SN1";
				var pivot2 = adjustmentLine.SerialNumbers.AddNew();
				pivot2.SerialNumberValue = "SN1";

				AssertHasError("WSN_SerialNumberInfo has an error.", pivot2.SerialNumberValueInfo, "Serial # already used.");
			}
		}

		public void TestCheckSerialNumberIsUnique_SaveDbHits()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var n = 100;
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

				var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
				var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, n, data.Whs1.DefaultLocation);
				for (var i = 0; i < n; i++)
				{
					adjustmentLine.SerialNumbers.AddNew().SerialNumberValue = $"SN{i}";
				}
				Factory.ClearQueryCache(WhsSerialNumberSchema.Constants.TableName);
				Factory.ResetDatabaseLoadCount();

				var expectedDbHits = new Dictionary<string, int>
				{
					{ OrgCustomLabelsSchema.Constants.TableName, 1 },
					{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
					{ WhsSerialNumberSchema.Constants.TableName, 1 },
				};
				using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, Factory))
				using (RowFactory.SetCachedTables())
				{
					adjustment.RunPreSaveValidation();
					Factory.Save();
				}
			}
		}

		#endregion

		#endregion

		#region Notes

		protected override void TestNoteContextsForRelatedNotesAssertions(WhsAdjustment docket)
		{
			Assert("Should always be 'Warehouse' module", (docket.GetNoteContextsForRelatedNotes().Module & StmNoteContextModule.W) != 0);
			Assert("Should always be 'Internal' direction", (docket.GetNoteContextsForRelatedNotes().Direction & StmNoteContextDirection.I) != 0);
			Assert("Should always be 'Adjustment' freight mode", (docket.GetNoteContextsForRelatedNotes().FreightMode & StmNoteContextFreightMode.D) != 0);
			AssertEquals("Visible Notes Count", 8, docket.Notes.VisibleNotes.Count);
		}

		#endregion

		#region Validation

		#region TestRunPreSaveValidation_UnCommitsExcessInventoryAndCommitsRequiredInventory

		public void TestRunPreSaveValidation_UnCommitsExcessInventoryAndCommitsRequiredInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, data.Whs1.FindLocation("A-1"));
			adjustmentLine1.RunPreSaveValidation(); // commit inventory
			adjustmentLine1.WE_TransactionQuantity = -1m;

			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, data.Whs1.FindLocation("A-1"));
			AssertEquals("Precondition", 5m, adjustmentLine1.CommittedQuantity);
			AssertEquals("Precondition", 0m, adjustmentLine2.CommittedQuantity);

			// unregister adjustment lines as children to prevent validation being run on them
			adjustment.UnRegisterEditableChildObject(adjustment.Lines);
			adjustment.RunPreSaveValidation();
			AssertEquals("Should have uncommitted excess inventory.", 1m, adjustmentLine1.CommittedQuantity);
			AssertEquals("Should have committed required inventory.", 9m, adjustmentLine2.CommittedQuantity);
		}

		#endregion

		#region TestRunPreSaveValidationCore_SynchronisesOffset

		public override void TestRunPreSaveValidationCore_SynchronisesOffset_UpdatesDocketLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "R1");
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, data.Whs1.FindLocation("A-1"));

			var warehouse = data.Whs1;
			var warehouseTimeZone = warehouse.RelatedCompanyBranch.HomePort.TimeZoneSet;
			var calculationTimeZone = warehouseTimeZone.GetCalculationTimeZone();
			var dateTime = new ZDateTime(2024, 06, 12, 12, 30, 00);

			adjustmentLine.WE_AdjustmentArrivalDate = new ZDateTimeOffset(dateTime, TimeSpan.FromHours(0));

			adjustmentLine.SynchroniseOffsetsToWarehouseTime(calculationTimeZone);

			Assert("Docket should not be in error", !adjustment.HasErrors);

			var expectedOffset = warehouse.GetWarehouseBranchDateTimeOffset(dateTime);
			AssertEquals(
				"Offsets should have same value, including offset component",
				expectedOffset.ToString("dd-MMM-yyyy hh:mm:ss zzz"),
				adjustmentLine.WE_AdjustmentArrivalDate.ToString("dd-MMM-yyyy hh:mm:ss zzz"));
		}

		#endregion

		#region TestAdjustOutForFreeStockInBondedArea

		public void TestAdjustOutForFreeStockInBondedArea()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var freeStoreArea = Helper.CreateArea(data.Whs1, "FREE", AreaTypes.Codes.FreeStore);
			var freeStoreLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "X", 1, 1).Locations[0];
			freeStoreLocation.WLV_WA_PickingArea = freeStoreArea.PK;
			Factory.Save();

			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			var bondedLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "Y", 1, 1).Locations[0];
			bondedLocation.WLV_WA_PickingArea = bondedArea.PK;
			bondedLocation.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var bondedReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			bondedReceive.WD_TotalUnits = 1;
			bondedReceive.WD_DocketSubType = "CUS";
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(bondedReceive, data.Part1, 1, bondedLocation);
			inventoryLine.InDocketLine.CustomsData.WB_EntryKey = "EntryKey";
			bondedReceive.FinaliseDocket();

			inventoryLine.InDocketLine.CustomsData.WB_EntryKey = null;
			bondedReceive.WD_DocketSubType = "REC";
			Factory.Save();
			AssertIsFinalisedPrecondition(bondedReceive);
			AssertEquals("Precondition", "REC", bondedReceive.WD_DocketSubType);

			var adjustmentOut = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			Helper.CreateWhsAdjustmentLine(adjustmentOut, data.Part1, -1, bondedLocation);
			Helper.CreateProductClientRelationShip(data.Org1, data.Part1);
			adjustmentOut.FinaliseDocket();
			AssertEquals("Transfer Out of bonded area should be allowed for non-bonded adjustment.", true, adjustmentOut.IsFinalised);

			var adjustmentin = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A2");
			Helper.CreateWhsAdjustmentLine(adjustmentin, data.Part1, 1, bondedLocation);
			Helper.CreateProductClientRelationShip(data.Org1, data.Part1);
			adjustmentin.FinaliseDocket();
			AssertEquals("Transfer in to Bonded area should not be allowed for non-bonded adjustment.", false, adjustmentin.IsFinalised);

			var adjustmentIn_FreeStore = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A4");
			Helper.CreateWhsAdjustmentLine(adjustmentIn_FreeStore, data.Part1, 1, freeStoreLocation);
			Helper.CreateProductClientRelationShip(data.Org1, data.Part1);
			adjustmentIn_FreeStore.FinaliseDocket();
			AssertEquals("Transfer into Free Store area should not have issues.", true, adjustmentIn_FreeStore.IsFinalised);

			var adjustmentOut_FreeStore = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A3");
			Helper.CreateWhsAdjustmentLine(adjustmentOut_FreeStore, data.Part1, -1, freeStoreLocation);
			Helper.CreateProductClientRelationShip(data.Org1, data.Part1);
			adjustmentOut_FreeStore.FinaliseDocket();
			AssertEquals("Transfer Out of Free Store area should not have issues.", true, adjustmentOut_FreeStore.IsFinalised);
		}

		#endregion

		protected override Type GetExpectedValidationType()
		{
			return typeof(WhsAdjustmentValidation);
		}

		#region TestIsPalletInTransit_TransferFullPallet

		public void TestIsPalletInTransit_TransferFullPallet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var picker = Helper.CreateGlbStaff("LIM", "LIM");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locations[0], "PLT1");
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 10m, locations[0], "PLT1", locations[1], "PLT1", picker);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, locations[2].ToLocationString(), "PLT1");
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, locations[2].ToLocationString(), "plt1");
			var isPalletInTransitWhenTransferIsNotFinalised = adjustment.IsPalletInTransit("PLT1");
			var isPalletInTransitWhenTransferIsNotFinalised_LowerCase = adjustment.IsPalletInTransit("plt1");
			Assert("PLT1 is in transit because transfer is not finalised.", isPalletInTransitWhenTransferIsNotFinalised);
			AssertEquals("plt1 is in transit because PalletID is case insensitive.", isPalletInTransitWhenTransferIsNotFinalised, isPalletInTransitWhenTransferIsNotFinalised_LowerCase);

			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);
			Factory.Save();

			var isPalletInTransitAfterFinalisingTransfer = adjustment.IsPalletInTransit("PLT1");
			Assert("PLT1 is not in transit since transfer is finalised.", !isPalletInTransitAfterFinalisingTransfer);
		}

		#endregion

		#region TestIsPalletInTransit_TransferBetweenTwoPallets

		public void TestIsPalletInTransit_TransferBetweenTwoPallets()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var picker = Helper.CreateGlbStaff("LIM", "LIM");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locations[0], "PLT1");
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 5m, locations[0], "PLT1", locations[1], "PLT2", picker);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, locations[2].ToLocationString(), "PLT2");
			var isDestinationPalletInTransit = adjustment.IsPalletInTransit("PLT2");
			Assert("PLT2 is used as a destination pallet and should be in-transit.", isDestinationPalletInTransit);

			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);
			Factory.Save();

			var isDestinationPalletInTransitAfterFinalisingTransfer = adjustment.IsPalletInTransit("PLT2");
			Assert("After finalising the transfer destination pallet must not be in-transit.", !isDestinationPalletInTransitAfterFinalisingTransfer);
		}

		#endregion

		#region TestIsPalletInTransit_TransferFromPalletToNonPallet

		public void TestIsPalletInTransit_TransferFromPalletToNonPallet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var picker = Helper.CreateGlbStaff("LIM", "LIM");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locations[0], "PLT1");
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 2m, locations[0], "PLT1", locations[1], "", picker);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, locations[2].ToLocationString(), "PLT1");
			var isPalletInTransit = adjustment.IsPalletInTransit("PLT1");
			Assert("Inventory is being transferred out of that pallet hence inventory is not in-transit.", !isPalletInTransit);
		}

		#endregion

		#region TestIsPalletInTransit_PuttingAway

		public void TestIsPalletInTransit_PuttingAway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1, "PLT1", 15m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, nonDockDoorLocation, "PLT1", 15m);
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();

			AssertEquals($"Precondition: InventoryStatus of Transfer Line should be {InventoryStatus.Codes.PuttingAway}.", InventoryStatus.Codes.PuttingAway, transferLine1.WE_CurrentInventoryStatus);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, nonDockDoorLocation.ToLocationString(), "PLT1");
			var isPalletInTransit = adjustment.IsPalletInTransit("PLT1");
			Assert("Inventory is putting away hence pallet should be in-transit", isPalletInTransit);
		}

		#endregion

		#region TestCheckIfPalletIDExistsInAnotherLocationInThisWarehouse

		public void TestCheckIfPalletIDExistsInAnotherLocationInThisWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var picker = Helper.CreateGlbStaff("LIM", "LIM");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locations[0], "PLT1");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, locations[0].ToLocationString(), "PLT1");
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, locations[1].ToLocationString(), "PLT1");
			var adjustmentLine2_LowerCase = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, locations[1].ToLocationString(), "plt1");
			var adjustmentLine3 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, locations[1].ToLocationString(), "PLT2");
			AssertEquals("PLT1 doesn't exist in another location.", ZString.Empty, adjustment.CheckIfPalletIDExistsInAnotherLocationInThisWarehouse(adjustmentLine1));
			AssertEquals("PLT1 exists in A-1 and Line2 uses a different location.", "A-1", adjustment.CheckIfPalletIDExistsInAnotherLocationInThisWarehouse(adjustmentLine2));
			AssertEquals("plt1 exists in A-1 and Line2 uses a different location because PalletID is case insensitive.", "A-1", adjustment.CheckIfPalletIDExistsInAnotherLocationInThisWarehouse(adjustmentLine2_LowerCase));
			AssertEquals("PLT2 doesn't exist in another location.", ZString.Empty, adjustment.CheckIfPalletIDExistsInAnotherLocationInThisWarehouse(adjustmentLine3));
		}

		#endregion

		#endregion

		#region Lookups

		protected override Type GetExpectedLookupsType()
		{
			return typeof(WhsAdjustmentLookups);
		}

		#endregion

		#region Properties

		#region TestAdjustmentTypeDescription

		public void TestAdjustmentTypeDescription()
		{
			var adjustment = GetNewBusinessObject();
			AssertEquals(CodeLists.AdjustmentType.Descriptions.Adjustment, adjustment.AdjustmentTypeDescription);

			adjustment.WD_DocketSubType = "xXx";
			AssertEquals("", adjustment.AdjustmentTypeDescription);

			AssertEquals("Precondition", true, new AdjustmentType().Count > 0);
			foreach (CodeDescriptionPair pair in new AdjustmentType())
			{
				adjustment.WD_DocketSubType = pair.Code;
				AssertEquals(pair.Description, adjustment.AdjustmentTypeDescription);
			}
		}

		#endregion

		#region TestSubTypeDesc

		public override void TestSubTypeDesc()
		{
			AssertEquals(CodeLists.AdjustmentType.Descriptions.Adjustment, Docket.SubTypeDesc);
		}

		#endregion

		#region TestShouldUpdateWeightAndVolumeOnTheFly

		public override void TestShouldUpdateWeightAndVolumeOnTheFly()
		{
			Docket.WD_WeightVolSetFromImport = true;
			AssertEquals(false, Docket.ShouldUpdateWeightAndVolumeOnTheFly);
			Docket.WD_WeightVolSetFromImport = false;
			AssertEquals(false, Docket.ShouldUpdateWeightAndVolumeOnTheFly);
		}

		#endregion

		#region TestIsNewOwnershipAdjustmentParentAndChild

		public void TestIsNewOwnershipAdjustmentParentAndChild()
		{
			var adjustment = GetNewBusinessObject();
			var parentAdjustment = GetNewBusinessObject();
			var childAdjustment = GetNewBusinessObject();
			childAdjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			parentAdjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			childAdjustment.WD_WD_ParentDocket = parentAdjustment.PK;

			AssertEquals(false, adjustment.IsNewOwnershipAdjustmentParent);
			AssertEquals(true, parentAdjustment.IsNewOwnershipAdjustmentParent);
			AssertEquals(false, childAdjustment.IsNewOwnershipAdjustmentParent);
			AssertEquals(false, adjustment.IsNewOwnershipAdjustmentChild);
			AssertEquals(false, parentAdjustment.IsNewOwnershipAdjustmentChild);
			AssertEquals(true, childAdjustment.IsNewOwnershipAdjustmentChild);
		}

		#endregion

		#region TestIsNewOwnershipAdjustmentParent

		public void TestIsNewOwnershipAdjustmentParent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var newClient = Helper.CreateClient("A1");
			var newAdjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "D1", Notify);
			var newOwnershipParentAdjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);

			AssertNotNull("Precondition", newOwnershipParentAdjustment.ChildAdjustment);
			AssertEquals(false, newAdjustment.IsNewOwnershipAdjustmentParent);
			AssertEquals(true, newOwnershipParentAdjustment.IsNewOwnershipAdjustmentParent);
			AssertEquals(false, newOwnershipParentAdjustment.ChildAdjustment.IsNewOwnershipAdjustmentParent);
		}

		#endregion

		#region TestCanCreateInventory

		public void TestCanCreateInventory()
		{
			AssertEquals("Adjustment can modify existing and create new Inventroy.", true, Docket.CanCreateInventory);
		}

		#endregion

		#region TestManualFinaliseReadonly

		public void TestManualFinaliseReadonly()
		{
			var adjustment = GetNewBusinessObject();
			var ownershipAdjustmentParent = GetNewBusinessObject();
			var ownershipAdjustmentChild = GetNewBusinessObject();
			adjustment.WD_DocketStatus = DocketStatus.Codes.Entered;
			ownershipAdjustmentParent.WD_DocketStatus = DocketStatus.Codes.Entered;
			ownershipAdjustmentChild.WD_DocketStatus = DocketStatus.Codes.Entered;
			ownershipAdjustmentParent.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			ownershipAdjustmentChild.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			ownershipAdjustmentChild.WD_WD_ParentDocket = ownershipAdjustmentParent.PK;

			// Entered adjustments
			AssertEquals(false, adjustment.ManualFinaliseReadonly);
			AssertEquals(false, ownershipAdjustmentParent.ManualFinaliseReadonly);
			AssertEquals(true, ownershipAdjustmentChild.ManualFinaliseReadonly);

			// Finalised adjustments
			adjustment.WD_FinalisedDate = ZDateTimeOffset.Now;
			ownershipAdjustmentParent.WD_FinalisedDate = ZDateTimeOffset.Now;
			ownershipAdjustmentChild.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals(true, adjustment.ManualFinaliseReadonly);
			AssertEquals(true, ownershipAdjustmentParent.ManualFinaliseReadonly);
			AssertEquals(true, ownershipAdjustmentChild.ManualFinaliseReadonly);
		}

		#endregion

		#region Test Standard Readonly Properties

		#region TestStandardReadOnly

		protected override void TestStandardReadOnlyCore(Func<WhsAdjustment, bool> getReadOnly, string name, WhsAdjustment docket, bool readOnlyWhenDocketHasLines)
		{
			AssertStandardReadonly(getReadOnly, name, docket);
		}

		#endregion

		#region TestNonStandardReadOnly1

		protected override void TestNonStandardReadOnly1(Func<WhsAdjustment, bool> getReadOnly, Func<WhsAdjustment, string> getName)
		{
			base.TestNonStandardReadOnly1(getReadOnly, getName);
			var docket = GetNewBusinessObject();
			AssertStandardReadonly(getReadOnly, getName(docket), docket);
		}

		#endregion

		void AssertStandardReadonly(Func<WhsAdjustment, bool> getReadOnly, string name, WhsAdjustment docket)
		{
			var childAdjustment = GetNewBusinessObject();
			docket.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			childAdjustment.WD_WD_ParentDocket = docket.PK;
			AssertEquals($"Property {name} should be not readonly", false, getReadOnly(docket));

			docket.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals($"Property {name} should be readonly", true, getReadOnly(docket));

			docket.WD_FinalisedDate = ZDateTimeOffset.Empty;
			childAdjustment.WD_WD_ParentDocket = ZGuid.Empty;
			var changeOwnershipAdjustmentParent = GetNewBusinessObject();
			docket.WD_WD_ParentDocket = changeOwnershipAdjustmentParent.PK;
			AssertEquals($"Property {name} should be readonly", true, getReadOnly(docket));

			docket.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals($"Property {name} should be readonly", true, getReadOnly(docket));
		}

		#endregion

		#region TestWD_FinalisedDate

		public void TestWD_FinalisedDate_UpdateVersionIdPolicyWhenSet()
		{
			var adjustment = Factory.New<WhsAdjustment>();
			AssertEquals("Concurrency Policy should be Ignore when WVO_WD_TransferIntoServiceArea is not set.", ConcurrencyPolicy.Ignore, adjustment.WD_CriticalChangesVersionIDInfo.ConcurrencyPolicy);

			adjustment.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals("Concurrency Policy should be Strict when WVO_WD_TransferIntoServiceArea is set.", ConcurrencyPolicy.Strict, adjustment.WD_CriticalChangesVersionIDInfo.ConcurrencyPolicy);

			adjustment.WD_FinalisedDate = ZDateTimeOffset.Empty;
			AssertEquals("Concurrency Policy should be Ignore when WVO_WD_TransferIntoServiceArea is not set.", ConcurrencyPolicy.Ignore, adjustment.WD_CriticalChangesVersionIDInfo.ConcurrencyPolicy);
		}

		#endregion

		#endregion

		#region Finalisation

		protected override WhsAdjustment SetupForTestFinaliseDocket()
		{
			var adjustment = base.SetupForTestFinaliseDocket();
			Factory.Save();

			var part = Helper.CreateProduct(adjustment.Client, "P1");
			var docketLine = Helper.CreateWhsAdjustmentLine(adjustment, part, 10m, "A-1");
			docketLine.WE_LineComment = "TEST";

			return adjustment;
		}

		#region TestFinaliseDocket_DBHits

		public void TestFinaliseDocket_DBHits()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(10, 10, 10, 10, 10);
			Factory.Save(); // need to save as finalise uses a 2nd factory

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "1", Notify);
			var line1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10, "A-1-1");
			line1.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			var line2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 20, "A-2-2");
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			otherFactory.RefreshEnabled = false;
			var otherHelper = new WhsTestHelperFunctions(otherFactory);
			var adjustmentInOtherFactory = otherFactory.Load<WhsAdjustment>(adjustment.PK);

			using (RowFactory.SetCachedTables())
			{
				adjustmentInOtherFactory.RunPreSaveValidation();
				adjustmentInOtherFactory.Validation.ValidateAll();
			}

			var expectedDBHitsForValidation = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsInventoryHeldCodeSchema.Constants.TableName, 1 }, // Due to inventory hold code list validation in docketlines
				{ RefPacksSchema.Constants.TableName, 1 }
			};

			AssertDbHits(expectedDBHitsForValidation, otherFactory);

			var secondOtherFactory = new BusinessObjectFactory();
			secondOtherFactory.RefreshEnabled = false;
			var adjustmentIn2ndOtherFactory = secondOtherFactory.Load<WhsAdjustment>(adjustment.PK);

			var expectedDBHitsForFinalisation = new Dictionary<string, int>(expectedDBHitsForValidation);
			expectedDBHitsForFinalisation.Add(WhsInventoryViewSchema.Constants.TableName, 1);
			// increased by A.V from 0 to 2
			expectedDBHitsForFinalisation[WhsPickLineSchema.Constants.TableName] += 2;

			expectedDBHitsForFinalisation[JobDocAddressSchema.Constants.TableName] = 1;

			expectedDBHitsForFinalisation.Add(WhsDocketReferenceSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(WhsDocketPalletSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(ProcessTasksSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(ProcessTaskTemplateSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(JobServiceSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(WhsDocketContainerSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(GlbStaffSchema.Constants.TableName, 1);

			using (RowFactory.SetCachedTables())
			{
				adjustmentIn2ndOtherFactory.FinaliseDocketWithoutUserConfirmation();
			}

			AssertEquals(true, adjustmentIn2ndOtherFactory.IsFinalised);
			AssertDbHits(expectedDBHitsForFinalisation, secondOtherFactory);
		}

		#endregion

		#region TestFinaliseDocketValidatesLinesAndDoubleChecksLocations

		public void TestFinaliseDocketValidatesLinesAndDoubleChecksLocations()
		{
			var adjustment = SetupForTestFinaliseDocket();

			var line = adjustment.Lines[0];
			using (line.GetValidationSuspender())
			{
				line.WE_WL = ZGuid.Empty;
			}
			AssertNoErrors(line.WE_WLInfo);

			adjustment.FinaliseDocket();
			AssertHasErrors(line.WE_WLInfo);
		}

		#endregion

		#region TestFinaliseDocket_Failed

		public void TestFinaliseDocket_Failed()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(10, 10, 10, 10, 10);
			data.Part1.OP_StockKeepingUnit = "BAG";
			Factory.Save(); // need to save as finalise uses a 2nd factory
			var inventories = Helper.LoadInventory();
			AssertEquals("Precondition", 50m, inventories.UnitsAvailable);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "1", Notify);
			var line1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10, "A-1-1");
			var line2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -20, "A-1-2");  // will fail overpicked
			var line3 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10, "A-3-2");  // will fail wrong location
			var line4 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 20, "A-2-2");
			adjustment.RunPreSaveValidation();
			AssertEquals("Some Stock was committed so units available should reduce.", 40m, inventories.UnitsAvailable);

			adjustment.FinaliseDocket();
			AssertEquals("Precondition", true, adjustment.HasErrors && !adjustment.IsFinalised);
			AssertEquals("Lines should not disappear", 4, adjustment.Lines.Count);
			AssertEquals("Lines should not be deleted", true, !line1.IsDeleted && !line2.IsDeleted && !line3.IsDeleted && !line4.IsDeleted);
			AssertHasErrorContaining(line2.WE_TransactionQuantityInfo, "only 10 Bags are available for adjustment out of this location");
			AssertHasErrorContaining(line3.WE_TransactionQuantityInfo, "no Bags are available for adjustment out of this location");
		}

		#endregion

		#region TestFinaliseDocket_Failed_InventoryError

		public void TestFinaliseDocket_Failed_InventoryError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A"), "PLT-1");
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ1", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, "A", "PLT-1");

			// hack to fail finalise due error on Inventory line - which will be deleted during rollback.
			adjustmentLine.Inventory.CountChanged += OnInventoryLineCountChange;
			adjustment.FinaliseDocket();
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Adjustment should NOT be finalised.", false, adjustment.IsFinalised);
			AssertEquals("New inventory records should be removed.", 0, adjustmentLine.Inventory.Count);
			AssertHasRowError(adjustmentLine, @"Could not update Inventory:
Error - WhsInventoryView: Test Error
Try deleting this line and re-adding it.");

			adjustmentLine.ClearAllNotifications();
			adjustmentLine.Inventory.CountChanged -= OnInventoryLineCountChange;
			AssertEquals("Precondtion", false, adjustmentLine.HasErrors);

			adjustment.ClearAllNotifications();
			AssertEquals("Precondtion", false, adjustment.HasErrors);

			adjustment.FinaliseDocket();
			AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		void OnInventoryLineCountChange(object sender, CollectionCountChangedEventArgs args)
		{
			if (args.ItemAdded)
			{
				args.BizObject.AddRowError("Test Error");
			}
		}

		#endregion

		#region TestFinaliseDocket_Failed_ShouldReloadInventory

		public void TestFinaliseDocket_Failed_ShouldReloadInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A"), "PLT-1");
			var inventory = receive.Inventory[0];
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ1", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, "A", "PLT-1");
			inventory.WI_TotalUnitsInfo.ValueChanged += delegate
			{ adjustmentLine.AddRowError("Test Error"); };
			adjustment.FinaliseDocket();
			AssertEquals("Adjustment should NOT be finalised.", false, adjustment.IsFinalised);
			AssertEquals("Should have reloaded Inventory.", 10m, inventory.WI_TotalUnits);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestFinaliseDocket_UpdateLastInventoryChangeDate

		[TestDate(2018, 12, 12)]
		public void TestFinaliseDocket_UpdateLastInventoryChangeDate_In()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ1", Notify);
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 5m, "A-1-1", "PLT-1");
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 5m, "A-1-2", "PLT-2");

			AssertEquals("Precondition.", ZDateTimeOffset.Empty, adjustmentLine1.Location.WLV_LastInventoryChangeDate);
			AssertEquals("Precondition.", ZDateTimeOffset.Empty, adjustmentLine2.Location.WLV_LastInventoryChangeDate);

			adjustmentLine1.WE_ReasonCode = "";
			adjustment.FinaliseDocket();
			AssertEquals("Adjustment should NOT be finalised.", false, adjustment.IsFinalised);
			AssertEquals("Should not update if cannot finalised.", ZDateTimeOffset.Empty, adjustmentLine1.Location.WLV_LastInventoryChangeDate);
			AssertEquals("Should not update if cannot finalised.", ZDateTimeOffset.Empty, adjustmentLine2.Location.WLV_LastInventoryChangeDate);

			adjustmentLine1.WE_ReasonCode = AdjustmentReasonCodesCodeList.Codes.StocktakeAdjustment;
			adjustment.FinaliseDocket();
			AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);
			AssertEquals("Should update if finalised.", now, adjustmentLine1.Location.WLV_LastInventoryChangeDate);
			AssertEquals("Should update if finalised.", now, adjustmentLine2.Location.WLV_LastInventoryChangeDate);
		}

		[TestDate(2018, 12, 12)]
		public void TestFinaliseDocket_UpdateLastInventoryChangeDate_Out()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A"), "PLT-1");
			var inventory = receive.Inventory[0];
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ1", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, "A", "PLT-1");

			AssertEquals("Precondition.", now, adjustmentLine.Location.WLV_LastInventoryChangeDate);

			TestDateAttribute.Date = now.AddDays(1).ToDateTime();
			EventHandler addError = delegate
			{ adjustmentLine.AddRowError("Test Error"); };
			inventory.WI_TotalUnitsInfo.ValueChanged += addError;
			adjustment.FinaliseDocket();
			AssertEquals("Adjustment should NOT be finalised.", false, adjustment.IsFinalised);
			AssertEquals("Pick line update LastInventoryChangeDate but cannot save.", now.AddDays(1), adjustmentLine.Location.WLV_LastInventoryChangeDate);

			adjustmentLine.ClearAllNotifications();
			inventory.WI_TotalUnitsInfo.ValueChanged -= addError;
			adjustment.FinaliseDocket();
			AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);
			AssertEquals("Should update if finalised.", now.AddDays(1), adjustmentLine.Location.WLV_LastInventoryChangeDate);
		}

		#endregion

		#region TestFinaliseDocket_AdjustingInBeforeAdjustingOutSamePallet

		public void TestFinaliseDocket_AdjustingInBeforeAdjustingOutSamePallet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-1");

			// Adjustment in is before Adjustment out for same Pallet ID and different location. Should not fail.
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ1", Notify);
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, "A-2", "PLT-1");
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, "A-1", "PLT-1");
			adjustment.FinaliseDocket();
			AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);
			AssertEquals("New inventory records should be created for adjusting in Line.", 10m, adjustmentLine1.Inventory[0].WI_TotalUnits);
			AssertEquals("Stock should be removed from old location by adjustment out.", 0m, receive.Inventory[0].WI_TotalUnits);

			// Adjustment lines order should not change.
			AssertEquals(adjustmentLine1, adjustment.Lines[0]);
			AssertEquals(adjustmentLine2, adjustment.Lines[1]);
		}

		#endregion

		#region TestFinaliseDocket_DoesAdjustmentHasPositiveChangesThatAffectAdjustmentLineLocationCapacity

		public void TestFinaliseDocket_DoesAdjustmentHasPositiveChangesThatAffectAdjustmentLineLocationCapacity_Error()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var loc1 = data.Whs1.FindLocation("A");
			loc1.WLV_MaxQuantity = 100;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, loc1, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 40m, loc1, "");
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);

			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			using (adjustment.GetValidationSuspender())
			{
				var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10, loc1);
				var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, 30, loc1);
				adjustment.RunPreSaveValidation(); // to commit inventory and create pickline
				Factory.Save();
				AssertEquals("Precondition - DoesAdjustmentHasPositiveChangesThatAffectAdjustmentLineLocationCapacity is false before Finalisation", false, adjustmentLine1.DoesAdjustmentHasPositiveChangesThatAffectAdjustmentLineLocationCapacity);
				AssertEquals("Precondition - DoesAdjustmentHasPositiveChangesThatAffectAdjustmentLineLocationCapacity is false before Finalisation", false, adjustmentLine2.DoesAdjustmentHasPositiveChangesThatAffectAdjustmentLineLocationCapacity);
				adjustment.FinaliseDocket();
				AssertIsFinalisedPrecondition(adjustment);

				AssertEquals(true, adjustmentLine1.DoesAdjustmentHasPositiveChangesThatAffectAdjustmentLineLocationCapacity);
				AssertEquals(true, adjustmentLine2.DoesAdjustmentHasPositiveChangesThatAffectAdjustmentLineLocationCapacity);
			}
		}

		public void TestFinaliseDocket_DoesAdjustmentHasPositiveChangesThatAffectAdjustmentLineLocationCapacity_Default()
		{
			var adjustment = SetupAdjustment(false, true);
			var line = adjustment.Lines[0];
			AssertEquals("Precondition - DoesAdjustmentHasPositiveChangesThatAffectAdjustmentLineLocationCapacity is false before Finalisation", false, line.DoesAdjustmentHasPositiveChangesThatAffectAdjustmentLineLocationCapacity);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);
			AssertEquals("DoesAdjustmentHasPositiveChangesThatAffectAdjustmentLineLocationCapacity is still false after Finalisation", false, line.DoesAdjustmentHasPositiveChangesThatAffectAdjustmentLineLocationCapacity);
		}

		#endregion

		#region TestFinaliseDocket_AdjustIn

		public void TestFinaliseDocket_AdjustIn()
		{
			var adjustment = SetupAdjustment(false, true);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);

			var line = adjustment.Lines[0];
			var inventoryFromAdjustment = line.Inventory[0];
			AssertEquals("WI_OH_Client set incorrectly", adjustment.Client.PK, inventoryFromAdjustment.WI_OH_Client);
			AssertEquals("WI_OP set incorrectly", line.SupplierPart.PK, inventoryFromAdjustment.WI_OP);
			AssertEquals("WI_InventoryStatus set incorrectly", InventoryStatus.Codes.Available, inventoryFromAdjustment.WI_InventoryStatus);
			AssertEquals("WI_WL set incorrectly", line.Location.PK, inventoryFromAdjustment.WI_WL);
			AssertEquals("WI_PalletID set incorrectly", "P_ID_001", inventoryFromAdjustment.WI_PalletID);
			AssertEquals("WI_ArrivalDate set incorrectly", line.WE_AdjustmentArrivalDate, inventoryFromAdjustment.WI_ArrivalDate);
			Assert("WI_ArrivalDate is empty", !inventoryFromAdjustment.WI_ArrivalDate.IsEmpty);
			AssertEquals("WI_TotalUnits set incorrectly", 100m, inventoryFromAdjustment.WI_TotalUnits);
			AssertEquals("WI_F3_NKPackType set incorrectly", "XXX", inventoryFromAdjustment.WI_F3_NKPackType);
			AssertEquals("WI_InDocketLineUnits set incorrectly", 0m, inventoryFromAdjustment.WI_InDocketLineUnits);
			AssertEquals("WI_InDocketLineType set incorrectly", "ADJ", inventoryFromAdjustment.WI_InDocketLineType);
			AssertEquals("WI_WE_InDocketLine set incorrectly", line.PK, inventoryFromAdjustment.WI_WE_InDocketLine);
			AssertEquals("WI_WE_OriginalInDocketLineForRating set incorrectly", line.PK, inventoryFromAdjustment.WI_WE_OriginalInDocketLineForRating);
			AssertEquals("WI_WD set incorrectly", adjustment.PK, inventoryFromAdjustment.WI_WD);
			Helper.AssertInventoryAttributes(inventoryFromAdjustment, line.WE_ExpiryDate, line.WE_PackingDate, line.WE_PartAttrib1, line.WE_PartAttrib2, line.WE_PartAttrib3, line.WE_BondedEntryKey);
		}

		WhsAdjustment SetupAdjustment(bool createStock, bool usePalletID, bool isCustomsTransaction = false)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var today = ZDate.Today;
			var arrivalDate = today.AddMonths(-1).ToZDateTime().ToOffset();
			var expiryDate = today.AddMonths(1);
			var packingDate = today.AddMonths(-2);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			if (createStock)
			{
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
				receive.WD_ArrivalDate = arrivalDate;
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m); // Location: A-1
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 130m);  // Location: A-2
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m); // Location: A-1
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m); // Location: A-1

				foreach (WhsInventoryView inv in receive.Inventory)
				{
					Helper.SetInventoryAttributes(inv, expiryDate, packingDate, "PA1", "PA2", "PA3", isCustomsTransaction ? "BEK1-1" : "");
				}

				receive.AllocateLocationsWithMock();

				if (usePalletID)
				{
					receive.Inventory[0].WI_PalletID = "P_ID_001";
					receive.Inventory[1].WI_PalletID = "P_ID_002";
					receive.Inventory[2].WI_PalletID = "P_ID_001";
					receive.Inventory[3].WI_PalletID = "P_ID_001";
				}

				receive.FinaliseDocket();
				AssertEquals("Could not finalise Receive", true, receive.IsFinalised);
			}

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "1", Notify);
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 100m, location);
			line.WE_AdjustmentArrivalDate = arrivalDate;
			line.WE_F3_NKPackType = "XXX";
			Helper.SetDocketLineAttributes(line, expiryDate, packingDate, "PA1", "PA2", "PA3", isCustomsTransaction ? "BEK1-1" : "");

			if (usePalletID)
			{
				line.WE_PalletID = "P_ID_001";
			}

			return adjustment;
		}

		#endregion

		#region TestFinaliseDocket_AdjustIn_SetsArrivalDateIfEmpty

		[TestDate(2007, 10, 22)]
		public void TestFinaliseDocket_AdjustIn_SetsArrivalDateIfEmpty()
		{
			var adjustment = SetupAdjustment(false, false);
			var adjustmentLine = adjustment.Lines[0];
			adjustmentLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Empty;

			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);

			var inventoryFromAdjustment = adjustmentLine.Inventory[0];
			AssertEquals(new ZDateTimeOffset(2007, 10, 22, 0, 0, 0, TimeSpan.FromHours(10)), inventoryFromAdjustment.WI_ArrivalDate);
		}

		#endregion

		#region TestFinaliseDocket_AdjustIn_US

		public void TestFinaliseDocket_AdjustIn_US()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = bondedArea.PK;
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.DefaultLocation);
			adjustmentLine.WE_PackageGroupId = "ABC";
			adjustmentLine.WE_PerPackageQty = 2m;
			adjustmentLine.WE_BondedEntryKey = "AAA-1";
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);
			AssertEquals("ABC", adjustmentLine.Inventory[0].PackageGroupId);
			AssertEquals(2m, adjustmentLine.Inventory[0].PerPackageQty);
			AssertEquals(10m, adjustmentLine.Inventory[0].WI_TotalUnits);
		}

		#endregion

		#region TestFinaliseDocket_AdjustOut

		public void TestFinaliseDocket_AdjustOut()
		{
			var staff = Helper.CreateGlbStaff("AA", "Antman");
			Factory.Save();

			var adjustment = SetupAdjustment(true, true);
			var adjustmentLine = adjustment.Lines[0];
			adjustmentLine.WE_TransactionQuantity = -100m;
			adjustment.RunPreSaveValidation();
			AssertEquals("Precondition: Adjustment is committed.", 100m, adjustmentLine.CommittedQuantity);
			AssertEquals("All Picked Times should be empty.", true, adjustmentLine.PickLines.All(pl => pl.WZ_PickedDateTime.IsEmpty));

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				adjustment.FinaliseDocketWithoutUserConfirmation();
				AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);
				AssertEquals("Precondition: Finalised Date is set.", true, !adjustment.WD_FinalisedDate.IsEmpty);
				AssertEquals("All Picked Times should be set to finalised time.", true, adjustmentLine.PickLines.All(pl => pl.WZ_PickedDateTime == adjustment.WD_FinalisedDate && pl.WZ_GS_NKAssignedTo == "AA"));
			}

			var inventories = Helper.LoadInventory(adjustmentLine);
			AssertEquals(20m, inventories.UnitsAvailable);
		}

		#endregion

		#region TestFinaliseDocket_AdjustOut_NotEnoughStock

		public void TestFinaliseDocket_AdjustOut_NotEnoughStock()
		{
			var adjustment = SetupAdjustment(true, false);
			var adjustmentLine = adjustment.Lines[0];
			adjustmentLine.WE_TransactionQuantity = -200m;
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Adjustment should *not* be finalised.", false, adjustment.IsFinalised);
			AssertHasError(adjustmentLine.WE_TransactionQuantityInfo, @"Attempted to adjust 200 Units, but only 120 Units are available for adjustment out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the adjustment line.
Check that all the attributes exactly match the attributes on the inventory you are trying to adjust.
If you are trying to adjust stock with attributes, you must enter the attribute exactly. Blank non mandatory attributes will only match to inventory with blank attributes.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to adjust.
If you are trying to adjust stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to adjust stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.");
			AssertEquals("All Picked Times should be empty on failed finalisation.", true, adjustmentLine.PickLines.All(pl => pl.WZ_PickedDateTime.IsEmpty));
		}

		#endregion

		#region TestFinaliseDocket_AdjustOut_WithReservedStock

		public void TestFinaliseDocket_AdjustOut_WithReservedStock()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			AssertNotNull("Precondition: Stock is reserved.", orderLine.ReserveStockIfAbleTo(inventory));

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, inventory.Location);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Adjustment should *not* be finalised.", false, adjustment.IsFinalised);
			AssertHasError(adjustmentLine.WE_TransactionQuantityInfo, @"Attempted to adjust 10 Units, but only 5 Units are available for adjustment out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the adjustment line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to adjust.
If you are trying to adjust stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to adjust stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.");
		}

		#endregion

		#region TestFinaliseDocket_AdjustOut_WithMultipleLinesOfSameProduct

		public void TestFinaliseDocket_AdjustOut_WithMultipleLinesOfSameProduct()
		{
			// this test is for dbonly dirty reads
			var data = new TestDataForInventory(Factory, Notify);
			data.CreateSimpleInventoryManyLines();
			Factory.Save();

			var location = data.Whs1.FindLocation("A-1-1");

			foreach (WhsInventoryView inventory in data.Receive11.Inventory)
			{
				inventory.WI_WL = location.PK;
			}

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "1", data.Notify);
			for (int i = 0; i < 9; i++)
			{
				Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10, location);
			}

			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);

			var inventories = Helper.LoadInventory(data.Org1, data.Part1, location);
			AssertEquals("Stock level incorrect", 10m, inventories.UnitsAvailable);
		}

		#endregion

		#region TestFinaliseDocket_AdjustOut_DoesNotOverReduceStock

		public void TestFinaliseDocket_AdjustOut_DoesNotOverReduceStock()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Receive 50 Units
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.DefaultLocation, "");

			// Create Adjustment with line for 50 units
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "");
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -50m, data.Whs1.DefaultLocation);
			adjustmentLine.RunPreSaveValidation(); // Commit inventory
			AssertEquals("Precondition: Stock is committed", 50m, adjustmentLine.CommittedQuantity);
			Factory.Save();

			// HACK: Mock bad existing data that is no longer possible after "PreventOverCommitOfStockViaPickLine" was added
			// Testing that we dont create new stock if the source location could not satisfy the required quantity.
			// Previously stock in source location was not reduced, but new stock was created.
			adjustmentLine.WE_TransactionQuantity = -75m;
			adjustmentLine.PickLines[0].WZ_Units = 75m;
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Adjustment should not be finalised.", false, adjustment.IsFinalised);

			AssertHasError(adjustmentLine.WE_TransactionQuantityInfo, @"Attempted to adjust 75 Units, but only 50 Units are available for adjustment out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the adjustment line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to adjust.
If you are trying to adjust stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to adjust stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.");

			var inventoryUnits = Factory.Load<WhsInventoryView>(new ZQuery { FetchOnlyFromLocalCache = true }).Sum(i => i.WI_TotalUnits);
			AssertEquals("Should not have created stock.", 50m, inventoryUnits);
		}

		#endregion

		#region TestFinaliseDocket_AdjustOut_CanPerformWithInactiveProduct

		public void TestFinaliseDocket_AdjustOut_CanPerformWithInactiveProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location, "");

			data.Part1.OP_IsActive = false;
			Factory.Save();
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, location);

			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);
		}

		#endregion

		#region TestAdjustmentGetsWarningsAndErrorsForInactiveProducts

		public void TestAdjustmentGetsWarningsAndErrorsForInactiveProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location, "");

			data.Part1.OP_IsActive = false;
			Factory.Save();
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "1", Notify);
			var adjustingOut = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, location);

			adjustment.RunPreSaveValidation();
			AssertNoErrors("WE_OP should not have an error.", adjustingOut.WE_OPInfo);
			AssertHasWarning("WE_OP Should have inactive warning.", adjustingOut.WE_OPInfo, "This Product is Inactive");

			var adjustingIn = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, location);
			adjustment.RunPreSaveValidation();

			AssertHasError("WE_OP should have an inactive error.", adjustingIn.WE_OPInfo, "This Product Code is inactive - it may not be used.");
			AssertNoWarnings("WE_OP Should not have inactive warning.", adjustingIn.WE_OPInfo);
		}

		#endregion

		#region TestFinaliseDocket_DodgyBondedFinaliseAdjustInAndOutSameStock

		public void TestFinaliseDocket_DodgyBondedFinaliseAdjustInAndOutSameStock()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = bondedArea.PK;

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1", Notify);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.DefaultLocation);
			adjustmentLine1.WE_BondedEntryKey = "ABC-1";
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, data.Whs1.DefaultLocation);
			adjustmentLine2.WE_BondedEntryKey = "ABC-1";

			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Adjustment should *not* be finalised as this behaviour is not normally allowed.", false, adjustment.IsFinalised);

			using (adjustment.AttemptDodgyBondedFinalise())
			{
				adjustment.FinaliseDocketWithoutUserConfirmation();
			}

			AssertEquals("Adjustment should be finalised as we have temporarily allowed the old behaviour.", true, adjustment.IsFinalised);

			var inventories = Helper.LoadInventory(data.Org1, data.Part1, data.Whs1.DefaultLocation);
			AssertEquals("Stock level incorrect", 0m, inventories.UnitsAvailable);
		}

		#endregion

		#region TestFinaliseDocket_GivesPromptWithDefaultableQueryEventArgs

		public void TestFinaliseDocket_GivesPromptWithDefaultableQueryEventArgs()
		{
			var adjustment = SetupAdjustment(true, true);

			AssertEquals("Precondition: adjustment has no errors", false, adjustment.HasErrors);
			AssertEquals("Precondition: adjustment is unfinalized", false, adjustment.IsFinalised);

			Notify.PreQueryUser += (sender, e) =>
			{
				if (e is DefaultableQueryUserEventArgs args)
				{
					AssertEquals("Precondition: default response correct", false, args.Response);
					args.Response = true;
				}
			};

			adjustment.FinaliseDocket();

			var lastQueryEventArgs = (DefaultableQueryUserEventArgs)Notify.LastQueryUserEventArgs;
			AssertEquals("Postcondition: correct eventArgs type", true, lastQueryEventArgs is DefaultableQueryUserEventArgs);
			var expectedMessage = "Finalizing this Adjustment will update the inventory.\r\nOn finalization, this Adjustment will become read-only so that it cannot be modified.\r\nThis finalization process cannot be undone once saved.\r\nDo you wish to Finalize this Adjustment?";
			AssertEquals("Postcondition: correct notification message", expectedMessage, lastQueryEventArgs.Message);
			AssertEquals("Postcondition: correct response", true, lastQueryEventArgs.Response);
			AssertEquals("User query Buttons should be correct.", ZMessageBoxButtons.YesNoCancel, lastQueryEventArgs.Context.Buttons);
			AssertContainsExactElementsInAnyOrder("User query Results Not To Save should be correct.", new[] { ZDialogResult.No, ZDialogResult.Cancel }, lastQueryEventArgs.Context.DialogResultsToNotSave);
			AssertEquals("Postcondition: adjustment is finalized.", true, adjustment.IsFinalised);
			AssertEquals("Postcondition: adjustment has no errors.", false, adjustment.HasErrors);
		}

		#endregion

		#region TestCreateInventoryWithInventoryStatus

		public void TestCreateInventoryWithInventoryStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			var lineWithInventoryStatusAvailable = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 10, locations[0].PK, "", "", InventoryStatus.Codes.Available);
			var lineWithInventoryStatusHeld = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 10, locations[1].PK, "", "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
			var lineWithInventoryStatusDMG = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 10, locations[2].PK, "", "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			Factory.Save();

			AssertIsFinalisedPrecondition(receive);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "11", Notify);
			var lineToIncreaseAVLStock = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 5, locations[0]);
			var lineToReduceAVLStock = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -3m, locations[0]);

			var lineToIncreaseHeldStock = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 5, locations[1], InventoryHoldCodes.Codes.Held);
			var lineToReduceHeldStock = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -3m, locations[1], InventoryHoldCodes.Codes.Held);

			var lineToIncreaseDMGStock = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 5, locations[2], InventoryHoldCodes.Codes.Damaged);
			var lineToReduceDMGStock = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -3m, locations[2], InventoryHoldCodes.Codes.Damaged);

			var lineToAddNewAVLStock = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 5, locations[3]);
			var lineToAddNewHeldStock = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 5, locations[3], InventoryHoldCodes.Codes.Held);
			var lineToAddNewDMGStock = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 5, locations[3], InventoryHoldCodes.Codes.Damaged);

			adjustment.FinaliseDocket();

			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			var otherHelper = new WhsTestHelperFunctions(otherFactory);
			var inventoriesForLocation1 = otherHelper.LoadInventory(locations[0], InventoryStatus.Codes.Available);
			var inventoriesForLocation2 = otherHelper.LoadInventory(locations[1], InventoryStatus.Codes.Held);
			var inventoriesForLocation3DMG = otherHelper.LoadInventory(locations[2], InventoryStatus.Codes.Held).Inventory.Where(i => i.IsDamaged);
			var inventoriesForLocation4AVL = otherHelper.LoadInventory(locations[3], InventoryStatus.Codes.Available);
			var inventoriesForLocation4HEL = otherHelper.LoadInventory(locations[3], InventoryStatus.Codes.Held).Inventory.Where(i => !i.IsDamaged);
			var inventoriesForLocation4DMG = otherHelper.LoadInventory(locations[3], InventoryStatus.Codes.Held).Inventory.Where(i => i.IsDamaged);

			AssertEquals(12m, inventoriesForLocation1.UnitsAvailable);
			AssertEquals(12m, inventoriesForLocation2.UnitsAvailable);
			AssertEquals(12m, inventoriesForLocation3DMG.Sum(i => i.WI_TotalUnits));
			AssertEquals(5m, inventoriesForLocation4AVL.UnitsAvailable);
			AssertEquals(5m, inventoriesForLocation4HEL.Sum(i => i.WI_TotalUnits));
			AssertEquals(5m, inventoriesForLocation4DMG.Sum(i => i.WI_TotalUnits));
		}

		#endregion

		#region Bond Warehouse

		#region TestFinaliseDocket_CalculateCustomsData

		public void TestFinaliseDocket_CalculateCustomsData()
		{
			var data = new TestDataForBondedEntriesWithVOC(Factory, processBondedInwardDuringConstruction: false);

			var putawayEngineMock = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive();

			using (ObjectFactory.Substitute(putawayEngineMock.Object))
			{
				data.IReceiveLine1.TILV = new Money(51m, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD"));
				data.IReceiveLine2.TILV = new Money(5.1m, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD"));
				data.ProcessBondedMovement();
			}

			Factory.Save();
			var location = data.Whs.FindLocation("RR1");
			var adjustment = Helper.CreateWhsAdjustment(data.Org, data.Whs, "ADJ", Helper.Notify);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -25, location);
			line.WE_BondedEntryKey = "E11AA1-1";
			line.CustomsData.WB_EntryKey = "E11AA1";
			line.CustomsData.WB_EntryLineNo = 1;

			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);

			var inventories = Helper.LoadInventory("E11AA1-1");
			AssertEquals("Units", 25m, inventories.UnitsTotal);

			AssertEquals("WB_EntryKey", "E11AA1", line.CustomsData.WB_EntryKey);
			AssertEquals("WB_EntryLineNo", (short)1, line.CustomsData.WB_EntryLineNo);
			AssertEquals("WB_EntryDate", data.IReceiveLine1.EntryDate, line.CustomsData.WB_EntryDate);

			AssertEquals("WB_CustomsQty", 100m, line.CustomsData.WB_CustomsQty);
			AssertEquals("WB_CustomsSecondQuantity", 10m, line.CustomsData.WB_CustomsSecondQuantity);
			AssertEquals("WB_CustomsThirdQuantity", 1m, line.CustomsData.WB_CustomsThirdQuantity);
			AssertEquals("WB_CustomsUnitOfQty", "KG", line.CustomsData.WB_CustomsUnitOfQty);
			AssertEquals("WB_CustomsSecondUnitQty", "M3", line.CustomsData.WB_CustomsSecondUnitQty);
			AssertEquals("WB_CustomsThirdUnitQty", "CU", line.CustomsData.WB_CustomsThirdUnitQty);
			AssertEquals("WB_BondedWhsQty", 50m, line.CustomsData.WB_BondedWhsQty);
			AssertEquals("WB_BondedWhsUnitQty", "UNT", line.CustomsData.WB_BondedWhsUnitOfQty);
			AssertEquals("WB_TILV", 51m, line.CustomsData.WB_TILV);
			AssertEquals("WB_RX_NKTILVCurrency", "USD", line.CustomsData.WB_RX_NKTILVCurrency);
			AssertEquals("WB_ValueForDuty", 200m, line.CustomsData.WB_ValueForDuty);
			AssertEquals("WB_RN_NKCountryOfOrigin", data.IReceiveLine1.CountryOfOrigin.RN_Code, line.CustomsData.WB_RN_NKCountryOfOrigin);
			AssertEquals("WB_AddInfo", "ADD INFO1", line.CustomsData.WB_AddInfo);
		}

		#endregion

		#region TestFinaliseDocket_CasesWhereCustomsDataIsNotCalculated

		public void TestFinaliseDocket_CasesWhereCustomsDataIsNotCalculated()
		{
			var data = new TestDataForBondedEntriesWithVOC(Factory);
			Factory.Save();
			var location = data.Whs.FindLocation("RR1");
			var adjustment = Helper.CreateWhsAdjustment(data.Org, data.Whs, "ADJ", Helper.Notify);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -25, location);

			line.WE_BondedEntryKey = "E11AA1-1";
			line.CustomsData.WB_EntryKey = "E11AA1";
			line.CustomsData.WB_EntryLineNo = 1;
			line.CustomsData.WB_CustomsQty = 500m;
			line.CustomsData.WB_CustomsSecondQuantity = 1m;
			line.CustomsData.WB_CustomsThirdQuantity = 2m;
			line.CustomsData.WB_CustomsUnitOfQty = "KG";
			line.CustomsData.WB_CustomsSecondUnitQty = "M3";
			line.CustomsData.WB_CustomsThirdUnitQty = "CU";
			line.CustomsData.WB_ValueForDuty = 250m;

			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);
			AssertEquals("No Change WB_EntryKey", "E11AA1", line.CustomsData.WB_EntryKey);
			AssertEquals("No Change WB_EntryLineNo", (short)1, line.CustomsData.WB_EntryLineNo);
			AssertEquals("No Change WB_CustomsQty", 100m, line.CustomsData.WB_CustomsQty);
			AssertEquals("No Change WB_CustomsSecondQuantity", 10m, line.CustomsData.WB_CustomsSecondQuantity);
			AssertEquals("No Change WB_CustomsThirdQuantity", 1m, line.CustomsData.WB_CustomsThirdQuantity);
			AssertEquals("No Change WB_CustomsUnitOfQty", "KG", line.CustomsData.WB_CustomsUnitOfQty);
			AssertEquals("No Change WB_CustomsSecondUnitQty", "M3", line.CustomsData.WB_CustomsSecondUnitQty);
			AssertEquals("No Change WB_CustomsThirdUnitQty", "CU", line.CustomsData.WB_CustomsThirdUnitQty);
			AssertEquals("No Change WB_ValueForDuty", 200m, line.CustomsData.WB_ValueForDuty);
		}

		#endregion

		#region TestFinaliseDocket_AdjustingBondedWhsQty

		public void TestFinaliseDocket_AdjustingBondedWhsQty()
		{
			var data = new TestDataForBondedEntriesWithVOC(Factory);
			Factory.Save();
			var location = data.Whs.FindLocation("RR1");
			var adjustment = Helper.CreateWhsAdjustment(data.Org, data.Whs, "ADJ", Helper.Notify);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, location);

			line.WE_BondedEntryKey = "E11AA1-1";
			line.CustomsData.WB_BondedWhsQty = 20m;
			line.CustomsData.WB_BondedWhsUnitOfQty = "UNT";
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);

			var inventories = Helper.LoadInventory("E11AA1-1");
			AssertEquals("new record creatd", 2, inventories.Inventory.Count());
			AssertEquals("WE_TransactionQuantity", 1m, line.WE_TransactionQuantity);
			AssertEquals("WB_BondedWhsQty", 20m, line.CustomsData.WB_BondedWhsQty);
			AssertEquals("WB_BondedWhsUnitOfQty", "UNT", line.CustomsData.WB_BondedWhsUnitOfQty);
		}

		#endregion

		#region TestBondedWhsAdjustmentSimple

		public void TestBondedWhsAdjustmentSimple()
		{
			var data = new TestDataForBondedEntriesWithVOC(Factory);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org, data.Whs, "ADJ", Helper.Notify);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var sourceLocation = data.Whs.FindLocation("RR1");
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 19m, sourceLocation);

			adjustmentLine.WE_BondedEntryKey = "E11AA1-1";
			adjustmentLine.WE_LineComment = "LineComment";
			adjustmentLine.CustomsData.WB_EntryDate = data.IReceiveLine1.EntryDate;
			adjustmentLine.CustomsData.WB_CustomsQty = 100m;
			adjustmentLine.CustomsData.WB_CustomsSecondQuantity = 10m;
			adjustmentLine.CustomsData.WB_CustomsThirdQuantity = 1m;
			adjustmentLine.CustomsData.WB_CustomsUnitOfQty = "KG";
			adjustmentLine.CustomsData.WB_BondedWhsQty = 50m;
			adjustmentLine.CustomsData.WB_BondedWhsUnitOfQty = "UNT";
			adjustmentLine.CustomsData.WB_TILV = 51m;
			adjustmentLine.CustomsData.WB_RX_NKTILVCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			adjustmentLine.CustomsData.WB_ValueForDuty = 200m;
			adjustmentLine.CustomsData.WB_AddInfo = "ADD INFO1";
			adjustment.FinaliseDocket();

			Assert("Finalised", adjustment.IsFinalised);

			Factory.Save(); // need to save as finalise uses a 2nd factory

			var inventories = Helper.LoadInventory("E11AA1-1");
			AssertEquals("", 69m, inventories.UnitsAvailable);

			AssertEquals("Product", data.Part1, adjustmentLine.SupplierPart);
			AssertEquals("WE_TransactionQuantity", 19m, adjustmentLine.WE_TransactionQuantity);
			AssertEquals("WE_LineComment", "LineComment", adjustmentLine.WE_LineComment);
			AssertEquals("WE_BondedEntryKey", "E11AA1-1", adjustmentLine.WE_BondedEntryKey);

			AssertEquals("WB_EntryKey", "E11AA1", adjustmentLine.CustomsData.WB_EntryKey);
			AssertEquals("WB_EntryLineNo", (short)1, adjustmentLine.CustomsData.WB_EntryLineNo);
			AssertEquals("WB_EntryDate", data.IReceiveLine1.EntryDate, adjustmentLine.CustomsData.WB_EntryDate);
			AssertEquals("WB_CustomQty", 100m, adjustmentLine.CustomsData.WB_CustomsQty);
			AssertEquals("WB_CustomsSecondQuantity", 10m, adjustmentLine.CustomsData.WB_CustomsSecondQuantity);
			AssertEquals("WB_CustomsThirdQuantity", 1m, adjustmentLine.CustomsData.WB_CustomsThirdQuantity);
			AssertEquals("WB_CustomUnitOfQty", "KG", adjustmentLine.CustomsData.WB_CustomsUnitOfQty);
			AssertEquals("WB_BondedWhsQty", 50m, adjustmentLine.CustomsData.WB_BondedWhsQty);
			AssertEquals("WB_BondedWhsUnitOfQty", "UNT", adjustmentLine.CustomsData.WB_BondedWhsUnitOfQty);
			AssertEquals("WB_TILV", 51m, adjustmentLine.CustomsData.WB_TILV);
			AssertEquals("WB_RX_NKCurrency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, adjustmentLine.CustomsData.WB_RX_NKTILVCurrency);
			AssertEquals("WB_ValueForDuty", 200m, adjustmentLine.CustomsData.WB_ValueForDuty);
			AssertEquals("WB_AddInfo", "ADD INFO1", adjustmentLine.CustomsData.WB_AddInfo);
		}

		#endregion

		#region TestAdjustmentHasNotChangedProrationRatio

		public void TestAdjustmentHasNotChangedProrationRatio()
		{
			var data = new TestDataForBondedEntriesWithVOC(Factory);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org, data.Whs, "ADJ", Helper.Notify);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var sourceLocation = data.Whs.FindLocation("RR1");
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 34m, sourceLocation);

			var calcBeforeAdjustment = new WhsBondedCalculator(Factory, "E11AA1", 1);
			adjustmentLine.WE_BondedEntryKey = "E11AA1-1";
			adjustmentLine.WE_LineComment = "LineComment";
			adjustment.FinaliseDocket();

			Assert("Finalised", adjustment.IsFinalised);
			Factory.Save();

			var calcAfterAdjustment = new WhsBondedCalculator(Factory, "E11AA1", 1);

			AssertEquals("BondedWhsQty", calcBeforeAdjustment.GetAvailableBondedWhsQty(), calcAfterAdjustment.GetAvailableBondedWhsQty());
			AssertEquals("BondedWhsUnitOfQty", calcBeforeAdjustment.GetAvailableBondedWhsQty(), calcAfterAdjustment.GetAvailableBondedWhsQty());
		}

		#endregion

		#region TestAdjustmentWithZeroUnitsAndCartonSpecified

		public void TestAdjustmentWithZeroUnitsAndCartonSpecified()
		{
			var data = new TestDataForBondedEntriesWithVOC(Factory);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org, data.Whs, "ADJ", Helper.Notify);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var sourceLocation = data.Whs.FindLocation("RR1");
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, sourceLocation);
			var calcBeforeAdjustment = new WhsBondedCalculator(Factory, "E11AA1", 1);
			var availableBondedWhsQtyBeforeAdjustmentFinalised = calcBeforeAdjustment.GetAvailableBondedWhsQty();

			adjustmentLine.WE_BondedEntryKey = "E11AA1-1";
			adjustmentLine.WE_LineComment = "LineComment";
			adjustmentLine.CustomsData.WB_BondedWhsQty = 10m;
			adjustmentLine.CustomsData.WB_BondedWhsUnitOfQty = "CTN";
			adjustment.RunPreSaveValidation();
			Factory.Save();

			adjustment.FinaliseDocket();

			AssertEquals("WE_TransactionQuantity", 1m, adjustmentLine.WE_TransactionQuantity);
			AssertEquals("WB_CustomsQty", 0m, adjustmentLine.CustomsData.WB_CustomsQty);
			AssertEquals("WB_CustomsSecondQuantity", 0m, adjustmentLine.CustomsData.WB_CustomsSecondQuantity);
			AssertEquals("WB_CustomsThirdQuantity", 0m, adjustmentLine.CustomsData.WB_CustomsThirdQuantity);
			AssertEquals("WB_BondedWhsQty remains unchanged.", 10m, adjustmentLine.CustomsData.WB_BondedWhsQty);
			AssertEquals("WB_BondedWhsUnitOfQty remains unchanged.", "CTN", adjustmentLine.CustomsData.WB_BondedWhsUnitOfQty);
			AssertEquals("WB_TILV", 0m, adjustmentLine.CustomsData.WB_TILV);
			AssertEquals("WB_ValueForDuty", 0m, adjustmentLine.CustomsData.WB_ValueForDuty);

			Assert("Finalised", adjustment.IsFinalised);
			Assert("No Error", !adjustment.HasErrors);
			Factory.Save();

			var calcAfterAdjustment = new WhsBondedCalculator(Factory, "E11AA1", 1);
			AssertEquals("BondedWhsQty", availableBondedWhsQtyBeforeAdjustmentFinalised + 1, calcAfterAdjustment.GetAvailableBondedWhsQty());
		}

		#endregion

		#region TestFinaliseDocket_CalculateCustomsData_WithNoMatchingBondedEntryKey

		[TestDate(2017, 11, 2)]
		public void TestFinaliseDocket_CalculateCustomsData_WithNoMatchingBondedEntryKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = bondedArea.PK;

			var location = data.Whs1.DefaultLocation;
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20, location, "");
			Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 2);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ", Helper.Notify);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 25, location);
			line.WE_BondedEntryKey = "E11AA1-1"; // random BEK that does not exist on any existing Receive nor Adjustment.
			line.CustomsData.WB_EntryKey = "E11AA1";
			line.CustomsData.WB_EntryLineNo = 1;
			line.CustomsData.WB_BondedWhsQty = 5;
			line.CustomsData.WB_CustomsQty = 123m;
			line.CustomsData.WB_CustomsSecondQuantity = 1m;
			line.CustomsData.WB_CustomsThirdQuantity = 2m;
			line.CustomsData.WB_CustomsUnitOfQty = "UNT";
			line.CustomsData.WB_CustomsSecondUnitQty = "M3";
			line.CustomsData.WB_CustomsThirdUnitQty = "CU";
			line.CustomsData.WB_ValueForDuty = 123m;
			line.CustomsData.WB_RN_NKCountryOfOrigin = "US";
			line.CustomsData.WB_TILV = 123m;
			line.CustomsData.WB_RX_NKTILVCurrency = "EUR";
			line.CustomsData.WB_EntryDate = ZDate.Today;
			line.CustomsData.WB_AddInfo = "TEST";
			line.CustomsData.WB_BondedWhsUnitOfQty = "UNT";

			adjustment.FinaliseDocket();
			AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);

			AssertEquals("WB_EntryKey value should be retained.", "E11AA1", line.CustomsData.WB_EntryKey);
			AssertEquals("WB_EntryLineNo value should be retained.", (short)1, line.CustomsData.WB_EntryLineNo);
			AssertEquals("WB_CustomsQty value should be retained.", 123m, line.CustomsData.WB_CustomsQty);
			AssertEquals("WB_CustomsSecondQuantity value should be retained.", 1m, line.CustomsData.WB_CustomsSecondQuantity);
			AssertEquals("WB_CustomsThirdQuantity value should be retained.", 2m, line.CustomsData.WB_CustomsThirdQuantity);
			AssertEquals("WB_CustomsUnitOfQty value should be retained.", "UNT", line.CustomsData.WB_CustomsUnitOfQty);
			AssertEquals("WB_CustomsSecondUnitQty value should be retained.", "M3", line.CustomsData.WB_CustomsSecondUnitQty);
			AssertEquals("WB_CustomsThirdUnitQty value should be retained.", "CU", line.CustomsData.WB_CustomsThirdUnitQty);
			AssertEquals("WB_ValueForDuty value should be retained.", 123m, line.CustomsData.WB_ValueForDuty);
			AssertEquals("WB_RN_NKCountryOfOrigin value should be retained.", "US", line.CustomsData.WB_RN_NKCountryOfOrigin);
			AssertEquals("WB_TILV value should be retained.", 123m, line.CustomsData.WB_TILV);
			AssertEquals("WB_RX_NKTILVCurrency value should be retained.", "EUR", line.CustomsData.WB_RX_NKTILVCurrency);
			AssertEquals("WB_EntryDate value should be retained.", ZDate.Today, line.CustomsData.WB_EntryDate);
			AssertEquals("WB_AddInfo value should be retained.", "TEST", line.CustomsData.WB_AddInfo);
			AssertEquals("WB_BondedWhsUnitOfQty value should be retained.", "UNT", line.CustomsData.WB_BondedWhsUnitOfQty);
		}

		#endregion

		#region TestFinaliseDocket_CalculateCustomsData_WithMatchingBondedEntryKeyOnReceive

		[TestDate(2017, 11, 2)]
		public void TestFinaliseDocket_CalculateCustomsData_WithMatchingBondedEntryKeyOnReceive()
		{
			var data = new TestDataForBondedEntriesWithVOC(Factory);

			Factory.Save();
			var location = data.Whs.FindLocation("RR1");
			var adjustment = Helper.CreateWhsAdjustment(data.Org, data.Whs, "ADJ", Helper.Notify);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 50, location);
			line.WE_BondedEntryKey = "E11AA1-1";
			line.CustomsData.WB_EntryKey = "E11AA1";
			line.CustomsData.WB_EntryLineNo = 1;
			line.CustomsData.WB_BondedWhsQty = 5;
			line.CustomsData.WB_CustomsQty = 123m;
			line.CustomsData.WB_CustomsSecondQuantity = 1m;
			line.CustomsData.WB_CustomsThirdQuantity = 2m;
			line.CustomsData.WB_CustomsUnitOfQty = "UNT";
			line.CustomsData.WB_CustomsSecondUnitQty = "UNT";
			line.CustomsData.WB_CustomsThirdUnitQty = "UNT";
			line.CustomsData.WB_ValueForDuty = 123m;
			line.CustomsData.WB_RN_NKCountryOfOrigin = "US";
			line.CustomsData.WB_TILV = 123m;
			line.CustomsData.WB_RX_NKTILVCurrency = "EUR";
			line.CustomsData.WB_EntryDate = ZDate.Today.AddDays(1);
			line.CustomsData.WB_AddInfo = "TEST";
			line.CustomsData.WB_BondedWhsUnitOfQty = "UNT";

			adjustment.FinaliseDocket();
			AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);

			AssertEquals("WB_EntryKey value should be retained.", "E11AA1", line.CustomsData.WB_EntryKey);
			AssertEquals("WB_EntryLineNo value should be retained.", (short)1, line.CustomsData.WB_EntryLineNo);
			AssertEquals("WB_CustomsQty value should be from matching bond record.", 123m, line.CustomsData.WB_CustomsQty);
			AssertEquals("WB_CustomsSecondQuantity value should be from matching bond record.", 1m, line.CustomsData.WB_CustomsSecondQuantity);
			AssertEquals("WB_CustomsThirdQuantity value should be from matching bond record.", 2m, line.CustomsData.WB_CustomsThirdQuantity);
			AssertEquals("WB_CustomsUnitOfQty value should be from matching bond record.", "UNT", line.CustomsData.WB_CustomsUnitOfQty);
			AssertEquals("WB_CustomsSecondUnitQty value should be from matching bond record.", "UNT", line.CustomsData.WB_CustomsSecondUnitQty);
			AssertEquals("WB_CustomsThirdUnitQty value should be from matching bond record.", "UNT", line.CustomsData.WB_CustomsThirdUnitQty);
			AssertEquals("WB_ValueForDuty value should be from matching bond record.", 123m, line.CustomsData.WB_ValueForDuty);
			AssertEquals("WB_RN_NKCountryOfOrigin value should be from matching bond record.", "US", line.CustomsData.WB_RN_NKCountryOfOrigin);
			AssertEquals("WB_TILV value should be from matching bond record.", 123m, line.CustomsData.WB_TILV);
			AssertEquals("WB_RX_NKTILVCurrency value should be from matching bond record.", "EUR", line.CustomsData.WB_RX_NKTILVCurrency);
			AssertEquals("WB_EntryDate value should be from matching bond record.", ZDate.Today.AddDays(1), line.CustomsData.WB_EntryDate);
			AssertEquals("WB_AddInfo value should be from matching bond record.", "TEST", line.CustomsData.WB_AddInfo);
			AssertEquals("WB_BondedWhsUnitOfQty value should be retained.", "UNT", line.CustomsData.WB_BondedWhsUnitOfQty);
		}

		#endregion

		#region TestFinaliseDocket_CalculateCustomsData_WithMatchingBondedEntryKeyOnAdjustment

		[TestDate(2017, 11, 2)]
		public void TestFinaliseDocket_CalculateCustomsData_WithMatchingBondedEntryKeyOnAdjustment()
		{
			// create adjustment with bonded entry key
			var data = new TestDataSimpleEnvironment(Factory);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = bondedArea.PK;

			var location = data.Whs1.DefaultLocation;
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Factory.Save();

			var existingAdjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ", Helper.Notify);
			existingAdjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var existingAdjustmentLine = Helper.CreateWhsAdjustmentLine(existingAdjustment, data.Part1, 25, location);
			existingAdjustmentLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today.AddDays(-1);
			existingAdjustmentLine.WE_BondedEntryKey = "E11AA1-1";
			existingAdjustmentLine.CustomsData.WB_EntryKey = "E11AA1";
			existingAdjustmentLine.CustomsData.WB_EntryLineNo = 1;
			existingAdjustmentLine.CustomsData.WB_BondedWhsQty = 5;
			existingAdjustmentLine.CustomsData.WB_CustomsQty = 300m;
			existingAdjustmentLine.CustomsData.WB_CustomsSecondQuantity = 200m;
			existingAdjustmentLine.CustomsData.WB_CustomsThirdQuantity = 100m;
			existingAdjustmentLine.CustomsData.WB_CustomsUnitOfQty = "KG";
			existingAdjustmentLine.CustomsData.WB_CustomsSecondUnitQty = "M3";
			existingAdjustmentLine.CustomsData.WB_CustomsThirdUnitQty = "CU";
			existingAdjustmentLine.CustomsData.WB_ValueForDuty = 300m;
			existingAdjustmentLine.CustomsData.WB_RN_NKCountryOfOrigin = "AU";
			existingAdjustmentLine.CustomsData.WB_TILV = 51m;
			existingAdjustmentLine.CustomsData.WB_RX_NKTILVCurrency = "AUD";
			existingAdjustmentLine.CustomsData.WB_EntryDate = ZDateTime.Today;
			existingAdjustmentLine.CustomsData.WB_AddInfo = "ADD INFO1";
			existingAdjustmentLine.CustomsData.WB_BondedWhsUnitOfQty = "KG";
			existingAdjustment.FinaliseDocket();
			AssertIsFinalisedPrecondition(existingAdjustment);
			Factory.Save();

			var newAdjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ", Helper.Notify);
			newAdjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var newAdjustmentLine = Helper.CreateWhsAdjustmentLine(newAdjustment, data.Part1, 25, location);
			newAdjustmentLine.WE_BondedEntryKey = "E11AA1-1";
			newAdjustmentLine.CustomsData.WB_EntryKey = "E11AA1";
			newAdjustmentLine.CustomsData.WB_EntryLineNo = 1;
			newAdjustmentLine.CustomsData.WB_BondedWhsQty = 5;
			newAdjustmentLine.CustomsData.WB_CustomsQty = 123m;
			newAdjustmentLine.CustomsData.WB_CustomsSecondQuantity = 50m;
			newAdjustmentLine.CustomsData.WB_CustomsThirdQuantity = 40m;
			newAdjustmentLine.CustomsData.WB_CustomsUnitOfQty = "UNT";
			newAdjustmentLine.CustomsData.WB_CustomsSecondUnitQty = "UNT";
			newAdjustmentLine.CustomsData.WB_CustomsThirdUnitQty = "UNT";
			newAdjustmentLine.CustomsData.WB_ValueForDuty = 123m;
			newAdjustmentLine.CustomsData.WB_RN_NKCountryOfOrigin = "US";
			newAdjustmentLine.CustomsData.WB_TILV = 123m;
			newAdjustmentLine.CustomsData.WB_RX_NKTILVCurrency = "EUR";
			newAdjustmentLine.CustomsData.WB_EntryDate = ZDateTime.Today.AddDays(1);
			newAdjustmentLine.CustomsData.WB_AddInfo = "TEST";
			newAdjustmentLine.CustomsData.WB_BondedWhsUnitOfQty = "UNT";

			newAdjustment.FinaliseDocket();
			AssertEquals("Adjustment should be finalised.", true, newAdjustment.IsFinalised);

			AssertEquals("WB_EntryKey value should be retained.", "E11AA1", newAdjustmentLine.CustomsData.WB_EntryKey);
			AssertEquals("WB_EntryLineNo value should be retained.", (short)1, newAdjustmentLine.CustomsData.WB_EntryLineNo);
			AssertEquals("WB_CustomsQty value should be from matching bond record.", 123m, newAdjustmentLine.CustomsData.WB_CustomsQty);
			AssertEquals("WB_CustomsSecondQuantity value should be from matching bond record.", 50m, newAdjustmentLine.CustomsData.WB_CustomsSecondQuantity);
			AssertEquals("WB_CustomsThirdQuantity value should be from matching bond record.", 40m, newAdjustmentLine.CustomsData.WB_CustomsThirdQuantity);
			AssertEquals("WB_CustomsUnitOfQty value should be from matching bond record.", "UNT", newAdjustmentLine.CustomsData.WB_CustomsUnitOfQty);
			AssertEquals("WB_CustomsSecondUnitQty value should be from matching bond record.", "UNT", newAdjustmentLine.CustomsData.WB_CustomsSecondUnitQty);
			AssertEquals("WB_CustomsThirdUnitQty value should be from matching bond record.", "UNT", newAdjustmentLine.CustomsData.WB_CustomsThirdUnitQty);
			AssertEquals("WB_ValueForDuty value should be from matching bond record.", 123m, newAdjustmentLine.CustomsData.WB_ValueForDuty);
			AssertEquals("WB_RN_NKCountryOfOrigin value should be from matching bond record.", "US", newAdjustmentLine.CustomsData.WB_RN_NKCountryOfOrigin);
			AssertEquals("WB_TILV value should be from matching bond record.", 123m, newAdjustmentLine.CustomsData.WB_TILV);
			AssertEquals("WB_RX_NKTILVCurrency value should be from matching bond record.", "EUR", newAdjustmentLine.CustomsData.WB_RX_NKTILVCurrency);
			AssertEquals("WB_EntryDate value should be from matching bond record.", ZDateTime.Today.AddDays(1), newAdjustmentLine.CustomsData.WB_EntryDate);
			AssertEquals("WB_AddInfo value should be from matching bond record.", "TEST", newAdjustmentLine.CustomsData.WB_AddInfo);
			AssertEquals("WB_BondedWhsUnitOfQty value should be retained.", "UNT", newAdjustmentLine.CustomsData.WB_BondedWhsUnitOfQty);
		}

		#endregion

		#region TestFinaliseDocket_CalculateCustomsData_WithMatchingBondedEntryKeyOnReceive_NonBondedAdjustment

		[TestDate(2017, 11, 2)]
		public void TestFinaliseDocket_CalculateCustomsData_WithMatchingBondedEntryKeyOnReceive_NonBondedAdjustment()
		{
			var data = new TestDataForBondedEntriesWithVOC(Factory);
			Helper.EnableWarehouseForFreeStore(data.Whs, true);

			var freeStoreArea = Helper.CreateArea(data.Whs, "FREE", AreaTypes.Codes.FreeStore);
			var freeStoreLocation = Helper.CreateRowAndGenerateLocations(data.Whs, "X", 1, 1).Locations[0];
			freeStoreLocation.WLV_WA_PickingArea = freeStoreArea.PK;
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org, data.Whs, "ADJ", Helper.Notify);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Adjustment;
			using (adjustment.GetValidationSuspender()) // to allow bonded entry key input on non-customs transaction
			{
				var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 50m, freeStoreLocation);

				line.WE_BondedEntryKey = "E11AA1-1";
				adjustment.FinaliseDocket();
				AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);

				AssertEquals("WB_EntryKey value should be empty.", string.Empty, line.CustomsData.WB_EntryKey);
				AssertEquals("WB_EntryLineNo value should be 0.", (short)0, line.CustomsData.WB_EntryLineNo);
				AssertEquals("WB_CustomsQty value should 0.", 0m, line.CustomsData.WB_CustomsQty);
				AssertEquals("WB_CustomsUnitOfQty value should be empty.", string.Empty, line.CustomsData.WB_CustomsUnitOfQty);
				AssertEquals("WB_ValueForDuty value should be 0.", 0m, line.CustomsData.WB_ValueForDuty);
				AssertEquals("WB_RN_NKCountryOfOrigin value should be empty.", string.Empty, line.CustomsData.WB_RN_NKCountryOfOrigin);
				AssertEquals("WB_TILV value should be 0.", 0m, line.CustomsData.WB_TILV);
				AssertEquals("WB_RX_NKTILVCurrency value should be empty.", string.Empty, line.CustomsData.WB_RX_NKTILVCurrency);
				AssertEquals("WB_EntryDate value should be empty.", ZDateTime.Empty, line.CustomsData.WB_EntryDate);
				AssertEquals("WB_AddInfo value should be empty.", string.Empty, line.CustomsData.WB_AddInfo);
				AssertEquals("WB_BondedWhsUnitOfQty value should be empty.", string.Empty, line.CustomsData.WB_BondedWhsUnitOfQty);
			}
		}

		#endregion

		#endregion

		#region TestFinalise_OwnershipChangeAdjustment

		#region TestFinalise_OwnershipChangeAdjustment_NoPartAttributes

		public void TestFinalise_OwnershipChangeAdjustment_NoPartAttributes()
		{
			// setup products, two clients and one product client relationship
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var newClientWithoutProductClientRelationship = Helper.CreateClient("C2");
			var newClientWithProductClientRelationship = Helper.CreateClient("C3");
			Helper.CreateProductClientRelationShip(newClientWithProductClientRelationship, data.Part1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2, location, "");
			Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 2);
			Factory.Save();
			AssertEquals("Precondition", 2, data.Part1.RelatedOrganisations.Count);

			// Adjustment for new client without product client relationship
			var adjustmentForNewClientWithoutProductClientRelationship = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClientWithoutProductClientRelationship, Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustmentForNewClientWithoutProductClientRelationship, data.Part1, -1m, location);
			adjustmentForNewClientWithoutProductClientRelationship.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustmentForNewClientWithoutProductClientRelationship);
			AssertEquals("Units should be reduced from Adjustment.", adjustmentLine.WE_TransactionQuantity, -1m);
			AssertOwnershipAdjustmentChildIsFinalised(adjustmentLine, newClientWithoutProductClientRelationship);
			AssertNull("Since Julian batch number is not used, there shouldn't be Params by Whs and client created.",
				WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.Cast<WhsProductParamsByWhsAndClient>().SingleOrDefault(i => i.W3_OH == newClientWithoutProductClientRelationship.PK));

			// Adjustment for new client with product client relationship
			var adjustmentForNewClientWithProductClientRelationship = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClientWithProductClientRelationship, Notify);
			var adjustmentLineForNewClientWithProductClientRelationship = Helper.CreateWhsAdjustmentLine(adjustmentForNewClientWithProductClientRelationship, data.Part1, -1, location);
			adjustmentForNewClientWithProductClientRelationship.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustmentForNewClientWithProductClientRelationship);
			AssertEquals("Units should be reduced from Adjustment.", adjustmentLineForNewClientWithProductClientRelationship.WE_TransactionQuantity, -1m);
			AssertOwnershipAdjustmentChildIsFinalised(adjustmentLineForNewClientWithProductClientRelationship, newClientWithProductClientRelationship);
			AssertNull("Since Julian batch number is not used, there shouldn't be Params by Whs and client created.",
				WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.Cast<WhsProductParamsByWhsAndClient>().SingleOrDefault(i => i.W3_OH == newClientWithProductClientRelationship.PK));

			// test newly created product client relationship
			AssertEquals("There should be three product client relationships.", 3, data.Part1.RelatedOrganisations.Count);
			var clientWithoutProductClientRelationship = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single(l => l.OU_OH == newClientWithoutProductClientRelationship.PK);
			AssertEquals(false, clientWithoutProductClientRelationship.OU_UsePartAttrib1);
			AssertEquals(false, clientWithoutProductClientRelationship.OU_UsePartAttrib2);
			AssertEquals(false, clientWithoutProductClientRelationship.OU_UsePartAttrib3);
			AssertEquals(false, clientWithoutProductClientRelationship.OU_UseExpiryDate);
			AssertEquals(false, clientWithoutProductClientRelationship.OU_UsePackingDate);
		}

		void AssertOwnershipAdjustmentChildIsFinalised(WhsAdjustmentLine oldLine, OrgHeader newClientWithoutProductClientRelationship, OrgSupplierPart productForNewOwner = null)
		{
			var changedOwnershipAdjustmentForNewClientWithoutProductClientRelationship = Factory.Load<WhsAdjustment>(new ZQuery(WhsDocketSchema.WD_OH_Client, newClientWithoutProductClientRelationship.PK)).Single();
			AssertEquals("New ownership adjustment should be finalised.", true, changedOwnershipAdjustmentForNewClientWithoutProductClientRelationship.IsFinalised);

			var productToCheck = productForNewOwner ?? oldLine.SupplierPart;
			var changedOwnershipAdjustmentLine = changedOwnershipAdjustmentForNewClientWithoutProductClientRelationship.Lines.Single(l => l.WE_OP == productToCheck.PK);
			AssertEquals("Same number of units should be added to the new client.", Math.Abs(oldLine.WE_TransactionQuantity), changedOwnershipAdjustmentLine.WE_TransactionQuantity);
			AssertEquals("Location should not be changed.", oldLine.LocationString, changedOwnershipAdjustmentLine.LocationString);
			AssertEquals("Reason code should be client change.", oldLine.WE_ReasonCode, changedOwnershipAdjustmentLine.WE_ReasonCode);

			var inventoryLineForNewClientWithoutProductClientRelationship = Factory.Load<WhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WD, changedOwnershipAdjustmentForNewClientWithoutProductClientRelationship.PK)).Single(i => i.WI_OP == productToCheck.PK);
			AssertEquals("Ownership of the product should be changed.", newClientWithoutProductClientRelationship.PK, inventoryLineForNewClientWithoutProductClientRelationship.WI_OH_Client);
			AssertEquals("Same number of units should be added to the new client.", Math.Abs(oldLine.WE_TransactionQuantity), inventoryLineForNewClientWithoutProductClientRelationship.InDocketLine.WE_TransactionQuantity);
		}

		#endregion

		#region TestFinalise_OwnershipChangeAdjustment_NonMandatoryAttributes

		[TestDate(2012, 11, 12)]
		public void TestFinalise_OwnershipChangeAdjustment_NonMandatoryAttributes()
		{
			// setup products, two clients and one product client relationship
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var newClient = Helper.CreateClient("C2");
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetClientAllAttributeType(newClient, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true, useSerialNumber: false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 3m, location, ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1), "PA1", "PA2", "PA3", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			// Adjustment for new client without product client relationship
			var adjustmentForNewClientWithoutProductClientRelationship = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			var adjustmentLineWithoutAttributes = Helper.CreateWhsAdjustmentLine(adjustmentForNewClientWithoutProductClientRelationship, data.Part1.PK, -1, "A");
			var adjustmentLineWithAttributes = Helper.CreateWhsAdjustmentLine(adjustmentForNewClientWithoutProductClientRelationship, data.Part2.PK, -2, "A", "PA1", "PA2", "PA3", "", ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1));
			adjustmentForNewClientWithoutProductClientRelationship.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustmentForNewClientWithoutProductClientRelationship);
			AssertEquals("Units should be reduced from Adjustment.", -1m, adjustmentLineWithoutAttributes.WE_TransactionQuantity);
			AssertEquals("Units should be reduced from Adjustment.", -2m, adjustmentLineWithAttributes.WE_TransactionQuantity);

			var changedOwnershipAdjustmentForNewClientWithoutProductClientRelationship = Factory.Load<WhsAdjustment>(new ZQuery(WhsDocketSchema.WD_OH_Client, newClient.PK)).Single();
			AssertEquals("New ownership adjustment should be finalised.", true, changedOwnershipAdjustmentForNewClientWithoutProductClientRelationship.IsFinalised);
			AssertAdjustmentLine(changedOwnershipAdjustmentForNewClientWithoutProductClientRelationship.Lines, data.Part1, 1m, "", "", "", "", ZDate.Empty, ZDate.Empty);
			AssertAdjustmentLine(changedOwnershipAdjustmentForNewClientWithoutProductClientRelationship.Lines, data.Part2, 2m, "PA1", "PA2", "PA3", "", ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1));

			Factory.Save();
			var inventoryLines = Factory.Load<WhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WD, changedOwnershipAdjustmentForNewClientWithoutProductClientRelationship.PK));
			AssertInventoryLine(inventoryLines, newClient, data.Part1, 1m, "", "", "", "", ZDate.Empty, ZDate.Empty);
			AssertInventoryLine(inventoryLines, newClient, data.Part2, 2m, "PA1", "PA2", "PA3", "", ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1));

			// test newly created product client relationship - data.Part1
			AssertEquals("There should be three product client relationships.", 2, data.Part1.RelatedOrganisations.Count);
			var relationshipNoUseAllAttributes = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single(l => l.OU_OH == newClient.PK);
			AssertEquals(false, relationshipNoUseAllAttributes.OU_UsePartAttrib1);
			AssertEquals(false, relationshipNoUseAllAttributes.OU_UsePartAttrib2);
			AssertEquals(false, relationshipNoUseAllAttributes.OU_UsePartAttrib3);
			AssertEquals(false, relationshipNoUseAllAttributes.OU_UseExpiryDate);
			AssertEquals(false, relationshipNoUseAllAttributes.OU_UsePackingDate);

			AssertEquals("There should be three product client relationships.", 2, data.Part2.RelatedOrganisations.Count);
			var relationshipUseAllAttributes = data.Part2.RelatedOrganisations.Cast<OrgPartRelation>().Single(l => l.OU_OH == newClient.PK);
			AssertEquals(true, relationshipUseAllAttributes.OU_UsePartAttrib1);
			AssertEquals(true, relationshipUseAllAttributes.OU_UsePartAttrib2);
			AssertEquals(true, relationshipUseAllAttributes.OU_UsePartAttrib3);
			AssertEquals(true, relationshipUseAllAttributes.OU_UseExpiryDate);
			AssertEquals(true, relationshipUseAllAttributes.OU_UsePackingDate);
		}

		#endregion

		#region TestFinalise_OwnershipChangeAdjustment_MandatoryAttributes

		[TestDate(2012, 11, 12)]
		public void TestFinalise_OwnershipChangeAdjustment_MandatoryAttributes()
		{
			// setup products, two clients and one product client relationship
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var newClient = Helper.CreateClient("C2");
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetClientAllAttributeType(newClient, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true, useSerialNumber: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 1m, location.PK, "", ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1), "PA1", "PA2", "PA3", "SN1", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			// Adjustment for new client without product client relationship
			var adjustmentForNewClientWithoutProductClientRelationship = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			var adjustmentLineWithoutAttributes = Helper.CreateWhsAdjustmentLine(adjustmentForNewClientWithoutProductClientRelationship, data.Part1.PK, -2m, "A");
			var adjustmentLineWithAttributes = Helper.CreateWhsAdjustmentLine(adjustmentForNewClientWithoutProductClientRelationship, data.Part2.PK, -1m, "A", "PA1", "PA2", "PA3", "SN1", ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1));
			adjustmentForNewClientWithoutProductClientRelationship.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustmentForNewClientWithoutProductClientRelationship);
			AssertEquals("Units should be reduced from Adjustment.", -2m, adjustmentLineWithoutAttributes.WE_TransactionQuantity);
			AssertEquals("Units should be reduced from Adjustment.", -1m, adjustmentLineWithAttributes.WE_TransactionQuantity);

			var changedOwnershipAdjustmentForNewClientWithoutProductClientRelationship = Factory.Load<WhsAdjustment>(new ZQuery(WhsDocketSchema.WD_OH_Client, newClient.PK)).Single();
			AssertEquals("New ownership adjustment should be finalised.", true, changedOwnershipAdjustmentForNewClientWithoutProductClientRelationship.IsFinalised);
			AssertAdjustmentLine(changedOwnershipAdjustmentForNewClientWithoutProductClientRelationship.Lines, data.Part1, 2m, "", "", "", "", ZDate.Empty, ZDate.Empty);
			AssertAdjustmentLine(changedOwnershipAdjustmentForNewClientWithoutProductClientRelationship.Lines, data.Part2, 1m, "PA1", "PA2", "PA3", "SN1", ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1));

			Factory.Save();
			var inventoryLines = Factory.Load<WhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WD, changedOwnershipAdjustmentForNewClientWithoutProductClientRelationship.PK));
			AssertInventoryLine(inventoryLines, newClient, data.Part1, 2m, "", "", "", "", ZDate.Empty, ZDate.Empty);
			AssertInventoryLine(inventoryLines, newClient, data.Part2, 1m, "PA1", "PA2", "PA3", "SN1", ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1));

			// test newly created product client relationship - data.Part1
			AssertEquals("There should be two product client relationships.", 2, data.Part1.RelatedOrganisations.Count);
			var relationshipNotUseAllAttributes = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single(l => l.OU_OH == newClient.PK);
			AssertEquals(false, relationshipNotUseAllAttributes.OU_UsePartAttrib1);
			AssertEquals(false, relationshipNotUseAllAttributes.OU_UsePartAttrib2);
			AssertEquals(false, relationshipNotUseAllAttributes.OU_UsePartAttrib3);
			AssertEquals(false, relationshipNotUseAllAttributes.OU_UseSerialNumber);
			AssertEquals(false, relationshipNotUseAllAttributes.OU_UseExpiryDate);
			AssertEquals(false, relationshipNotUseAllAttributes.OU_UsePackingDate);

			AssertEquals("There should be two product client relationships.", 2, data.Part2.RelatedOrganisations.Count);
			var relationshipUseAllAttributes = data.Part2.RelatedOrganisations.Cast<OrgPartRelation>().Single(l => l.OU_OH == newClient.PK);
			AssertEquals(true, relationshipUseAllAttributes.OU_UsePartAttrib1);
			AssertEquals(true, relationshipUseAllAttributes.OU_UsePartAttrib2);
			AssertEquals(true, relationshipUseAllAttributes.OU_UsePartAttrib3);
			AssertEquals(true, relationshipUseAllAttributes.OU_UseSerialNumber);
			AssertEquals(true, relationshipUseAllAttributes.OU_UseExpiryDate);
			AssertEquals(true, relationshipUseAllAttributes.OU_UsePackingDate);
		}

		#endregion

		#region TestFinalise_OwnershipChangeAdjustment_WithUniqueProductAttributes

		[TestDate(2012, 11, 12)]
		public void TestFinalise_OwnershipChangeAdjustment_WithUniqueProductAttributes()
		{
			// setup products, two clients and one product client relationship
			var data = new TestDataSimpleEnvironment(Factory);
			var expiryDate = ZDate.Today.AddDays(1);
			var packingDate = ZDate.Today.AddDays(-1);
			var location = data.Whs1.FindLocation("A");
			var newClient = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(newClient, data.Part1);

			// Setup product client relation for data.Org1 and newClient and data.Part1
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.BatchNumber, "A1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory, "A2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.VIN, "A3");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);

			Helper.SetClientAttributeType(newClient, AttributeNumber.One, PartAttributeTypeList.Codes.VIN, "A1");
			Helper.SetClientAttributeType(newClient, AttributeNumber.Two, PartAttributeTypeList.Codes.BatchNumber, "A2");
			Helper.SetClientAttributeType(newClient, AttributeNumber.Three, PartAttributeTypeList.Codes.Mandatory, "A3");
			Helper.SetClientAttributeType(newClient, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(newClient, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(newClient, AttributeNumber.PackingDate, true);

			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, setReleaseCaptured: false, useSerialNumber: true);
			Helper.SetProductAllAttributeUse(newClient, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: true);

			// create receive and inventory
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, location.PK, "", expiryDate, packingDate, "BN1", "PA2", "S1", "SN1", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 1m, location.PK, "", expiryDate, packingDate, "BN1", "PA2", "S2", "SN2", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 1m, location.PK, "", expiryDate, packingDate, "BN1", "PA2", "S3", "SN3", "");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// create new ownership adjustment -- Adjustment which adjust out
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			var line1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, -1m, "A", "BN1", "PA2", "S1", "SN1", expiryDate, packingDate);
			var line2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2.PK, -1m, "A", "BN1", "PA2", "S2", "SN2", expiryDate, packingDate);
			adjustment.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment);
			AssertAdjustmentLine(adjustment.Lines, data.Part1, -1m, "BN1", "PA2", "S1", "SN1", expiryDate, packingDate);
			AssertAdjustmentLine(adjustment.Lines, data.Part2, -1m, "BN1", "PA2", "S2", "SN2", expiryDate, packingDate);

			// create new ownership adjustment -- Adjustment which adjust in
			var changedOwnershipAdjustment = Factory.Load<WhsAdjustment>(new ZQuery(WhsDocketSchema.WD_OH_Client, newClient.PK)).Single();
			AssertEquals("New ownership adjustment should be finalised.", true, changedOwnershipAdjustment.IsFinalised);
			var changedOwnershipAdjustmentLineForPart1 = changedOwnershipAdjustment.Lines.Single(l => l.SupplierPart.PK == data.Part1.PK);
			AssertAdjustmentLine(changedOwnershipAdjustment.Lines, data.Part1, 1m, "S1", "BN1", "PA2", "SN1", expiryDate, packingDate);
			AssertAdjustmentLine(changedOwnershipAdjustment.Lines, data.Part2, 1m, "S2", "BN1", "PA2", "SN2", expiryDate, packingDate);

			Factory.Save();

			// Assert inventory
			var inventoryLines = Factory.Load<WhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WD, changedOwnershipAdjustment.PK));
			AssertInventoryLine(inventoryLines, newClient, data.Part1, 1m, "S1", "BN1", "PA2", "SN1", ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1));
			AssertInventoryLine(inventoryLines, newClient, data.Part2, 1m, "S2", "BN1", "PA2", "SN2", ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1));

			// test newly created product client relationship - data.Part1
			AssertEquals("There should be two product client relationships.", 2, data.Part1.RelatedOrganisations.Count);
			var clientNotUseAllAttributes = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single(l => l.OU_OH == newClient.PK);
			AssertEquals(true, clientNotUseAllAttributes.OU_UsePartAttrib1);
			AssertEquals(true, clientNotUseAllAttributes.OU_UsePartAttrib2);
			AssertEquals(true, clientNotUseAllAttributes.OU_UsePartAttrib3);
			AssertEquals(true, clientNotUseAllAttributes.OU_UseSerialNumber);
			AssertEquals(true, clientNotUseAllAttributes.OU_UseExpiryDate);
			AssertEquals(true, clientNotUseAllAttributes.OU_UsePackingDate);

			AssertEquals("There should be two product client relationships.", 2, data.Part2.RelatedOrganisations.Count);
			var clientUseAllAttributes = data.Part2.RelatedOrganisations.Cast<OrgPartRelation>().Single(l => l.OU_OH == newClient.PK);
			AssertEquals(true, clientUseAllAttributes.OU_UsePartAttrib1);
			AssertEquals(true, clientUseAllAttributes.OU_UsePartAttrib2);
			AssertEquals(true, clientUseAllAttributes.OU_UsePartAttrib3);
			AssertEquals(true, clientUseAllAttributes.OU_UseSerialNumber);
			AssertEquals(true, clientUseAllAttributes.OU_UseExpiryDate);
			AssertEquals(true, clientUseAllAttributes.OU_UsePackingDate);
		}

		#endregion

		#region TestFinalise_OwnershipChangeAdjustment_WithDuplicatedProductAttributes

		[TestDate(2012, 11, 12)]
		public void TestFinalise_OwnershipChangeAdjustment_WithDuplicatedProductAttributes()
		{
			// setup products, two clients and one product client relationship
			var data = new TestDataSimpleEnvironment(Factory);
			var expiryDate = ZDate.Today.AddDays(1);
			var packingDate = ZDate.Today.AddDays(-1);
			var location = data.Whs1.FindLocation("A");
			var newClient = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(newClient, data.Part1);

			// Setup product client relation for data.Org1 and newClient and data.Part1
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.BatchNumber, "A1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.BatchNumber, "A2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.BatchNumber, "A3");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);

			Helper.SetClientAttributeType(newClient, AttributeNumber.One, PartAttributeTypeList.Codes.BatchNumber, "a3");
			Helper.SetClientAttributeType(newClient, AttributeNumber.Two, PartAttributeTypeList.Codes.BatchNumber, "a1");
			Helper.SetClientAttributeType(newClient, AttributeNumber.Three, PartAttributeTypeList.Codes.BatchNumber, "A2");
			Helper.SetClientAttributeType(newClient, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(newClient, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(newClient, AttributeNumber.PackingDate, true);

			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, setReleaseCaptured: false, useSerialNumber: true);
			Helper.SetProductAllAttributeUse(newClient, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: true);

			// create receive and inventory
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, location.PK, "", expiryDate, packingDate, "BN1", "PA1", "S1", "SN1", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 1m, location.PK, "", expiryDate, packingDate, "BN2", "PA2", "S2", "SN2", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 1m, location.PK, "", expiryDate, packingDate, "BN3", "PA3", "S3", "SN3", "");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// create new ownership adjustment -- Adjustment which adjust out
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			var line1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, -1m, "A", "BN1", "PA1", "S1", "SN1", expiryDate, packingDate);
			var line2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2.PK, -1m, "A", "BN2", "PA2", "S2", "SN2", expiryDate, packingDate);
			adjustment.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment);
			AssertAdjustmentLine(adjustment.Lines, data.Part1, -1m, "BN1", "PA1", "S1", "SN1", expiryDate, packingDate);
			AssertAdjustmentLine(adjustment.Lines, data.Part2, -1m, "BN2", "PA2", "S2", "SN2", expiryDate, packingDate);

			// create new ownership adjustment -- Adjustment which adjust in
			var changedOwnershipAdjustment = Factory.Load<WhsAdjustment>(new ZQuery(WhsDocketSchema.WD_OH_Client, newClient.PK)).Single();
			AssertEquals("New ownership adjustment should be finalised.", true, changedOwnershipAdjustment.IsFinalised);

			var changedOwnershipAdjustmentLineForPart1 = changedOwnershipAdjustment.Lines.Single(l => l.WE_OP == data.Part1.PK);
			AssertAdjustmentLine(changedOwnershipAdjustment.Lines, data.Part1, 1m, "S1", "BN1", "PA1", "SN1", expiryDate, packingDate);
			AssertAdjustmentLine(changedOwnershipAdjustment.Lines, data.Part2, 1m, "S2", "BN2", "PA2", "SN2", expiryDate, packingDate);
			Factory.Save();

			// Assert inventory
			var inventoryLines = Factory.Load<WhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WD, changedOwnershipAdjustment.PK));
			AssertInventoryLine(inventoryLines, newClient, data.Part1, 1m, "S1", "BN1", "PA1", "SN1", ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1));
			AssertInventoryLine(inventoryLines, newClient, data.Part2, 1m, "S2", "BN2", "PA2", "SN2", ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1));

			// test created product client relationship
			AssertEquals("There should be two product client relationships.", 2, data.Part1.RelatedOrganisations.Count);
			var clientNotUseAllAttributes = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single(l => l.OU_OH == newClient.PK);
			AssertEquals(true, clientNotUseAllAttributes.OU_UsePartAttrib1);
			AssertEquals(true, clientNotUseAllAttributes.OU_UsePartAttrib2);
			AssertEquals(true, clientNotUseAllAttributes.OU_UsePartAttrib3);
			AssertEquals(true, clientNotUseAllAttributes.OU_UseSerialNumber);
			AssertEquals(true, clientNotUseAllAttributes.OU_UseExpiryDate);
			AssertEquals(true, clientNotUseAllAttributes.OU_UsePackingDate);

			AssertEquals("There should be two product client relationships.", 2, data.Part2.RelatedOrganisations.Count);
			var clientUseAllAttributes = data.Part2.RelatedOrganisations.Cast<OrgPartRelation>().Single(l => l.OU_OH == newClient.PK);
			AssertEquals(true, clientUseAllAttributes.OU_UsePartAttrib1);
			AssertEquals(true, clientUseAllAttributes.OU_UsePartAttrib2);
			AssertEquals(true, clientUseAllAttributes.OU_UsePartAttrib3);
			AssertEquals(true, clientUseAllAttributes.OU_UseSerialNumber);
			AssertEquals(true, clientUseAllAttributes.OU_UseExpiryDate);
			AssertEquals(true, clientUseAllAttributes.OU_UsePackingDate);
		}

		#endregion

		#region TestFinalise_OwnershipChangeAdjustment_FailingChildAdjustmentFinalisation

		#region TestFinalise_OwnershipChangeAdjustment_FailingChildAdjustmentFinalisation

		public void TestFinalise_OwnershipChangeAdjustment_FailingChildAdjustmentFinalisation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var newClient = Helper.CreateClient("O1");
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");
			var receiveForPLT1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, location1, "PL1", false, true);
			var inValidReceiveForPLT1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m, location2, "PL1", false, false);
			Factory.Save();
			AssertNull("Precondition", data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(newClient.PK, OrgPartRelation.RelationshipTypes.Owner));
			AssertNotNull("Precondition", data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner));

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			AssertNotNull("Precondition", adjustment.ChildAdjustment);

			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, -1m, "A-1-1", "PL1", ZDateTimeOffset.Now);
			adjustment.FinaliseDocket();
			AssertEquals("Precondition", false, adjustment.IsFinalised);
			AssertEquals("Precondition", false, adjustment.ChildAdjustment.IsFinalised);
			AssertHasRowError("Adjustment bizO should has this error.", adjustment, WhsErrorTypes.CannotFinaliseChildAdjustment.Message);

			var inventoryLinesForReceiveForPLT1 = Factory.Load<WhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WD, receiveForPLT1.PK));
			var inventoryLinesForInValidReceiveForPLT1 = Factory.Load<WhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WD, inValidReceiveForPLT1.PK));
			AssertInventoryLine(inventoryLinesForReceiveForPLT1, data.Org1, data.Part1, 1m, "", "", "", "", ZDate.Empty, ZDate.Empty, "A-1-1", "");
			AssertInventoryLine(inventoryLinesForInValidReceiveForPLT1, data.Org1, data.Part1, 1m, "", "", "", "", ZDate.Empty, ZDate.Empty, "A-1-2", "");
			AssertNotNull("Precondition", data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner));
			AssertNull("Precondition", data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(newClient.PK, OrgPartRelation.RelationshipTypes.Owner));
		}

		#endregion

		#region TestFinalise_OwnershipChangeAdjustment_FailingChildAdjustmentFinalisation_WithJulianBatchNumber

		#region TestFinalise_OwnershipChangeAdjustment_FailingChildAdjustmentFinalisation_WithJulianBatchNumber_Attribute1

		[TestDate(2013, 01, 01)]
		public void TestFinalise_OwnershipChangeAdjustment_FailingChildAdjustmentFinalisation_WithJulianBatchNumber_Attribute1()
		{
			TestFinalise_OwnershipChangeAdjustment_FailingChildAdjustmentFinalisation_WithJulianBatchNumberCore(WhsInventoryViewSchema.WI_PartAttrib1, WhsDocketLineSchema.WE_PartAttrib1, AttributeNumber.One);
		}

		#endregion

		#region TestFinalise_OwnershipChangeAdjustment_FailingChildAdjustmentFinalisation_WithJulianBatchNumber_Attribute2

		[TestDate(2013, 01, 01)]
		public void TestFinalise_OwnershipChangeAdjustment_FailingChildAdjustmentFinalisation_WithJulianBatchNumber_Attribute2()
		{
			TestFinalise_OwnershipChangeAdjustment_FailingChildAdjustmentFinalisation_WithJulianBatchNumberCore(WhsInventoryViewSchema.WI_PartAttrib2, WhsDocketLineSchema.WE_PartAttrib2, AttributeNumber.Two);
		}

		#endregion

		#region TestFinalise_OwnershipChangeAdjustment_FailingChildAdjustmentFinalisation_WithJulianBatchNumber_Attribute3

		[TestDate(2013, 01, 01)]
		public void TestFinalise_OwnershipChangeAdjustment_FailingChildAdjustmentFinalisation_WithJulianBatchNumber_Attribute3()
		{
			TestFinalise_OwnershipChangeAdjustment_FailingChildAdjustmentFinalisation_WithJulianBatchNumberCore(WhsInventoryViewSchema.WI_PartAttrib3, WhsDocketLineSchema.WE_PartAttrib3, AttributeNumber.Three);
		}

		#endregion

		#region TestFinalise_OwnershipChangeAdjustment_FailingChildAdjustmentFinalisation_WithJulianBatchNumberCore

		void TestFinalise_OwnershipChangeAdjustment_FailingChildAdjustmentFinalisation_WithJulianBatchNumberCore(SchemaStringColumn inventoryPartAttribColumn, SchemaStringColumn docketLinePartAttribColumn, AttributeNumber number)
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var newClient = Helper.CreateClient("O1");
			Helper.SetClientAttributeType(data.Org1, number, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetClientAttributeType(newClient, number, PartAttributeTypeList.Codes.JulianBatchNumber);
			var relationForOrg1 = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single(l => l.OU_OH == data.Org1.PK);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, number, true);
			relationForOrg1.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;
			Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");
			Factory.Save();
			var receiveForPLT1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveForPLT1Line = Helper.CreateWhsReceiveInventoryLine(receiveForPLT1, data.Part1, 1m, location1, "PL1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			receiveForPLT1Line[inventoryPartAttribColumn] = "ABC3001";
			receiveForPLT1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receiveForPLT1);
			Factory.Save();

			var inValidReceiveForPLT1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inValidReceiveForPLT1Line = Helper.CreateWhsReceiveInventoryLine(inValidReceiveForPLT1, data.Part1, 1m, location2, "PL1", ZDate.Today, ZDate.Empty, "", "", "", "");
			inValidReceiveForPLT1Line[inventoryPartAttribColumn] = "ABC3001";
			Factory.Save();
			AssertEquals("Precondition", false, inValidReceiveForPLT1.IsFinalised);
			AssertNull("Precondition", data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(newClient.PK, OrgPartRelation.RelationshipTypes.Owner));
			AssertNotNull("Precondition", data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner));

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			AssertNotNull("Precondition", adjustment.ChildAdjustment);

			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, -1m, "A-1-1", "PL1", ZDateTimeOffset.Now);
			line[docketLinePartAttribColumn] = "ABC3001";
			adjustment.FinaliseDocket();
			AssertEquals("Precondition", false, adjustment.IsFinalised);
			AssertEquals("Precondition", false, adjustment.ChildAdjustment.IsFinalised);
			AssertHasRowError("Adjustment bizO should has this error.", adjustment, WhsErrorTypes.CannotFinaliseChildAdjustment.Message);

			var inventoryLinesForReceiveForPLT1 = Factory.Load<WhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WD, receiveForPLT1.PK));
			var inventoryLinesForInValidReceiveForPLT1 = Factory.Load<WhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WD, inValidReceiveForPLT1.PK));
			AssertNotNull("Precondition", data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner));
			AssertNull("Precondition", data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(newClient.PK, OrgPartRelation.RelationshipTypes.Owner));
			AssertEquals("Maximum shelf life should not be changed.",
				(ZShort)2, WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.Cast<WhsProductParamsByWhsAndClient>().Single(i => i.W3_OH == data.Org1.PK).W3_MaximumShelfLife);
			AssertNull("It should not create WhsProduct Params by Whs and Client for the new client, since child adjustment creation has failed.",
				WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.Cast<WhsProductParamsByWhsAndClient>().SingleOrDefault(i => i.W3_OH == newClient.PK));
		}

		#endregion

		#endregion

		#endregion

		#region TestFinalise_OwnershipChangeAdjustment_DateFormats

		public void TestFinalise_OwnershipChangeAdjustment_DateFormats()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.FindLocation("A");
			var newClient = Helper.CreateClient("C2");
			var relationForOrg1 = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single(l => l.OU_OH == data.Org1.PK);
			relationForOrg1.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;
			relationForOrg1.OU_PackingDateFormatString = "DDMMYY";
			relationForOrg1.OU_ExpiryDateFormatString = "DDMMMYY";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5, location, "");
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, location);
			adjustment.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment);

			var relationForNewClient = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single(l => l.OU_OH == newClient.PK);
			AssertEquals(JulianBatchNumberFormatList.Codes.BatchNumber_YDDD, relationForNewClient.OU_JulianBatchNoFormat);
			AssertEquals("DDMMYY", relationForNewClient.OU_PackingDateFormatString);
			AssertEquals("DDMMMYY", relationForNewClient.OU_ExpiryDateFormatString);
		}

		#endregion

		#region TestFinalise_OwnershipChangeAdjustment_MaximumShelfLife

		[TestDate(2013, 01, 01)]
		public void TestFinalise_OwnershipChangeAdjustment_MaximumShelfLife_Attribute1()
		{
			TestFinalise_OwnershipChangeAdjustment_MaximumShelfLifeCore(WhsInventoryViewSchema.WI_PartAttrib1, WhsDocketLineSchema.WE_PartAttrib1, AttributeNumber.One);
		}

		[TestDate(2013, 01, 01)]
		public void TestFinalise_OwnershipChangeAdjustment_MaximumShelfLife_Attribute2()
		{
			TestFinalise_OwnershipChangeAdjustment_MaximumShelfLifeCore(WhsInventoryViewSchema.WI_PartAttrib2, WhsDocketLineSchema.WE_PartAttrib2, AttributeNumber.Two);
		}

		[TestDate(2013, 01, 01)]
		public void TestFinalise_OwnershipChangeAdjustment_MaximumShelfLife_Attribute3()
		{
			TestFinalise_OwnershipChangeAdjustment_MaximumShelfLifeCore(WhsInventoryViewSchema.WI_PartAttrib3, WhsDocketLineSchema.WE_PartAttrib3, AttributeNumber.Three);
		}

		void TestFinalise_OwnershipChangeAdjustment_MaximumShelfLifeCore(SchemaStringColumn inventoryPartAttribColumn, SchemaStringColumn docketLinePartAttribColumn, AttributeNumber attributeNumber)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.FindLocation("A");
			var newClientWithoutProductClientRelationship = Helper.CreateClient("C2");
			var newClientWithProductClientRelationship = Helper.CreateClient("C3");
			var relationshipForNewClientWithProductClientRelationship = Helper.CreateProductClientRelationShip(newClientWithProductClientRelationship, data.Part1);
			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetClientAttributeType(newClientWithoutProductClientRelationship, attributeNumber, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetClientAttributeType(newClientWithProductClientRelationship, attributeNumber, PartAttributeTypeList.Codes.JulianBatchNumber);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true);
			Helper.SetProductAttributeUse(newClientWithProductClientRelationship, data.Part1, attributeNumber, true);
			var relationForOrg1 = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single(l => l.OU_OH == data.Org1.PK);
			relationForOrg1.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;
			relationshipForNewClientWithProductClientRelationship.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;
			var productParamsForOldClient = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location);
			inventory[inventoryPartAttribColumn] = "ABC3001";
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			// New client without relationship
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClientWithoutProductClientRelationship, Notify);
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, location);
			line[docketLinePartAttribColumn] = "ABC3001";
			adjustment.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment);

			var relationForNewClient = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single(l => l.OU_OH == newClientWithoutProductClientRelationship.PK);
			AssertEquals(JulianBatchNumberFormatList.Codes.BatchNumber_YDDD, relationForNewClient.OU_JulianBatchNoFormat);

			var newClientParams = WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.Cast<WhsProductParamsByWhsAndClient>().Single(i => i.W3_OH == newClientWithoutProductClientRelationship.PK);
			AssertEquals("Since WhsProduct params by whs and client doesn't exist, it should create it.", (ZShort)1, newClientParams.W3_MaximumShelfLife);

			// New Client with relationship
			var adjustmentForNewClientWithProductClientRelationship = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClientWithProductClientRelationship, Notify);
			var lineForAdjustmentForNewClientWithProductClientRelationship = Helper.CreateWhsAdjustmentLine(adjustmentForNewClientWithProductClientRelationship, data.Part1, -5m, location);
			lineForAdjustmentForNewClientWithProductClientRelationship[docketLinePartAttribColumn] = "ABC3001";
			adjustmentForNewClientWithProductClientRelationship.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustmentForNewClientWithProductClientRelationship);

			var relationForNewClientWithProductClientRelationship = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single(l => l.OU_OH == newClientWithProductClientRelationship.PK);
			AssertEquals(JulianBatchNumberFormatList.Codes.BatchNumber_YDDD, relationForNewClientWithProductClientRelationship.OU_JulianBatchNoFormat);

			var newClientWithProductClientRelationshipParams = WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.Cast<WhsProductParamsByWhsAndClient>().Single(i => i.W3_OH == newClientWithProductClientRelationship.PK);
			AssertEquals("Since WhsProduct params by whs and client doesn't exist, it should create it.", (ZShort)1, newClientWithProductClientRelationshipParams.W3_MaximumShelfLife);
		}

		#endregion

		#region TestFinalise_OwnershipChangeAdjustment_ProductWithSameProductCodeAndNewOwnerExists

		public void TestFinalise_OwnershipChangeAdjustment_ProductWithSameProductCodeAndNewOwnerExists()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var newClient = Helper.CreateClient("C2");
			var part3 = Helper.CreateProduct(data.Part1.OP_PartNum, newClient);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2, location, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 2, location, "");
			Factory.Save();

			var adjustmentForNewClient = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustmentForNewClient, data.Part1, -1m, location);
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustmentForNewClient, data.Part2, -1m, location);
			adjustmentForNewClient.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustmentForNewClient);
			AssertNoExceptionThrown(() => Factory.Save());

			// Should have found the Product with Same Code and New Owner, and set Adjustment Line to that
			AssertOwnershipAdjustmentChildIsFinalised(adjustmentLine1, newClient, productForNewOwner: part3);
			AssertOwnershipAdjustmentChildIsFinalised(adjustmentLine2, newClient);
		}

		#endregion

		#region TestFinalise_OwnershipChangeAdjustment_BarcodeWithSameProductCodeAndNewOwnerExists

		public void TestFinalise_OwnershipChangeAdjustment_BarcodeWithSameProductCodeAndNewOwnerExists()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var newClient = Helper.CreateClient("C2");
			var part3 = Helper.CreateProduct("P3", newClient);
			var part4 = Helper.CreateProduct("P4", data.Org1);
			var part5 = Helper.CreateProduct("P5", newClient);
			Helper.CreateProductBarcode(part3, part3.OP_StockKeepingUnit, data.Part1.OP_PartNum);
			Helper.CreateProductBarcode(part5, part5.OP_StockKeepingUnit, part4.OP_PartNum);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2, location, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 2, location, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part4, 2, location, "");
			Factory.Save();

			var adjustmentForNewClient = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			Helper.CreateWhsAdjustmentLine(adjustmentForNewClient, data.Part1, -1m, location);
			Helper.CreateWhsAdjustmentLine(adjustmentForNewClient, data.Part1, -1m, location);
			Helper.CreateWhsAdjustmentLine(adjustmentForNewClient, data.Part2, -1m, location);
			Helper.CreateWhsAdjustmentLine(adjustmentForNewClient, part4, -1m, location);
			adjustmentForNewClient.FinaliseDocket();
			AssertEquals("Finalisation should fail.", false, adjustmentForNewClient.IsFinalised);
			AssertHasRowError(adjustmentForNewClient, "Product Code 'P1' is used as a Barcode for Owner 'C2' on Product 'P3'. Unable to process Ownership change.");
			AssertHasRowError(adjustmentForNewClient, "Product Code 'P4' is used as a Barcode for Owner 'C2' on Product 'P5'. Unable to process Ownership change.");
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region AssertAdjustmentLine

		void AssertAdjustmentLine(WhsAdjustmentLineCollection lines, OrgSupplierPart part, decimal eUnits, string ePa1, string ePa2, string ePa3, string eSer, ZDate expiryDate, ZDate packingDate)
		{
			var adjustmentLine = lines.Single(l => l.WE_OP == part.PK);
			AssertEquals("Same number of units should be added to the new client.", eUnits, adjustmentLine.WE_TransactionQuantity);
			AssertEquals("Same product should be in for the new client.", part, adjustmentLine.SupplierPart);
			AssertEquals("Location should not be changed.", "A", adjustmentLine.LocationString);
			AssertEquals("Reason code should be client change.", "OCH", adjustmentLine.WE_ReasonCode);
			AssertEquals("Part Attribute 1 should be correct.", ePa1, adjustmentLine.WE_PartAttrib1);
			AssertEquals("Part Attribute 2 should be correct.", ePa2, adjustmentLine.WE_PartAttrib2);
			AssertEquals("Part Attribute 3 should be correct.", ePa3, adjustmentLine.WE_PartAttrib3);
			AssertEquals("Serial Number should be correct.", eSer, adjustmentLine.WE_SerialNumber);
			AssertEquals("Expiry Date should be correct.", expiryDate, adjustmentLine.WE_ExpiryDate);
			AssertEquals("Packing Date should be correct.", packingDate, adjustmentLine.WE_PackingDate);
		}

		#endregion

		#region AssertInventoryLine

		void AssertInventoryLine(WhsInventoryView[] lines, OrgHeader newClient, OrgSupplierPart part, decimal eUnits, string ePa1, string ePa2, string ePa3, string eSer, ZDate expiryDate, ZDate packingDate, string location = "A", string reasonCode = "OCH")
		{
			var inventory = lines.Single(l => l.InDocketLine.WE_OP == part.PK);
			var inventoryLine = inventory.InDocketLine;
			AssertEquals("Ownership of the product should be changed.", newClient.PK, inventory.WI_OH_Client);
			AssertEquals("Same number of units should be added to the new client.", eUnits, inventoryLine.WE_TransactionQuantity);
			AssertEquals("Same product should be in for the new client.", part, inventoryLine.SupplierPart);
			AssertEquals("Location should not be changed.", location, inventoryLine.LocationString);
			AssertEquals("Reason code should be client change.", reasonCode, inventoryLine.WE_ReasonCode);
			AssertEquals("Part Attribute 1 should be correct.", ePa1, inventoryLine.WE_PartAttrib1);
			AssertEquals("Part Attribute 2 should be correct.", ePa2, inventoryLine.WE_PartAttrib2);
			AssertEquals("Part Attribute 3 should be correct.", ePa3, inventoryLine.WE_PartAttrib3);
			AssertEquals("Serial Number should be correct.", eSer, inventoryLine.WE_SerialNumber);
			AssertEquals("Expiry Date should be correct.", expiryDate, inventoryLine.WE_ExpiryDate);
			AssertEquals("Packing Date should be correct.", packingDate, inventoryLine.WE_PackingDate);
		}

		#endregion

		#endregion

		#endregion

		#region TestRelatedJobs

		protected override List<IRelatedJob> GetValidRelatedJobs(WhsAdjustment docket)
		{
			var parentAdjustment = Factory.NewWithValidTestData<WhsAdjustment>();
			parentAdjustment.WD_DocketType = DocketType.Codes.Adjustment;
			parentAdjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			docket.WD_DocketType = DocketType.Codes.Adjustment;
			docket.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			docket.WD_WD_ParentDocket = parentAdjustment.PK;

			var childAdjustment = Factory.NewWithValidTestData<WhsAdjustment>();
			childAdjustment.WD_DocketType = DocketType.Codes.Adjustment;
			childAdjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			childAdjustment.WD_WD_ParentDocket = docket.PK;
			docket.FillWithValidTestData();
			Factory.Save();

			return new List<IRelatedJob>() { parentAdjustment, childAdjustment };
		}

		protected override List<IRelatedJob> GetInvalidRelatedJobs(WhsAdjustment docket)
		{
			var adjustment = Factory.NewWithValidTestData<WhsAdjustment>();
			adjustment.WD_DocketType = DocketType.Codes.Adjustment;
			Factory.Save();
			return new List<IRelatedJob>() { adjustment };
		}

		#endregion

		#region TestSetDocketLineFromInventory

		public void TestSetDocketLineFromInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			inventory.OriginalInventoryHeldCode = InventoryStatus.Codes.Held;

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			AssertEquals(inventory.OriginalInventoryStatus, InventoryStatus.Codes.Held);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "TR1", Notify);
			var adjustmentLine1 = adjustment.CreateDocketLineFromInventory(inventory);
			AssertEquals(adjustmentLine1.WE_WHC_NKOriginalInventoryHeldCode, InventoryStatus.Codes.Held);

			inventory.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = "AAA";
			var adjustmentLine2 = adjustment.CreateDocketLineFromInventory(inventory);
			AssertEquals(adjustmentLine2.WE_WHC_NKOriginalInventoryHeldCode, "AAA");

			var manualExpiryDate = new ZDate(ZDateTime.Now.Year + 2, 2, 1);
			inventory.InDocketLine.WE_ExpiryDate = manualExpiryDate;
			var adjustmentLine3 = adjustment.CreateDocketLineFromInventory(inventory);
			AssertEquals(adjustmentLine3.WE_ExpiryDate, manualExpiryDate);
		}

		#region TestSetDocketLineFromInventory_WithJulianBatchNumber_CheckWE_ExpiryDate

		public void TestSetDocketLineFromInventory_WithJulianBatchNumberCheckWE_ExpiryDateOne()
		{
			AssertSetDocketLineFromInventory_WithJulianBatchNumberCheckWE_ExpiryDate(AttributeNumber.One, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestSetDocketLineFromInventory_WithJulianBatchNumberCheckWE_ExpiryDateTwo()
		{
			AssertSetDocketLineFromInventory_WithJulianBatchNumberCheckWE_ExpiryDate(AttributeNumber.Two, WhsDocketLineSchema.WE_PartAttrib2);
		}
		public void TestSetDocketLineFromInventory_WithJulianBatchNumberCheckWE_ExpiryDateThree()
		{
			AssertSetDocketLineFromInventory_WithJulianBatchNumberCheckWE_ExpiryDate(AttributeNumber.Three, WhsDocketLineSchema.WE_PartAttrib3);
		}

		void AssertSetDocketLineFromInventory_WithJulianBatchNumberCheckWE_ExpiryDate(AttributeNumber attributeNumber, SchemaColumn partAttributeColumn)
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true);
			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;

			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParam.W3_MaximumShelfLife = 9001;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A"), "a");

			receiveLine[partAttributeColumn] = "7093 A467 789F";
			var manualExpiryDate = new ZDate(ZDateTime.Now.Year + 2, 2, 1);
			receiveLine.WE_ExpiryDate = manualExpiryDate;
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjLine1 = adjustment.CreateDocketLineFromInventory(receiveLine.Inventory[0]);
			Assert("Inventory line expiry date should be copied to adjustment line.", manualExpiryDate == adjLine1.WE_ExpiryDate);
		}

		#endregion

		#endregion

		#region TestConcurrencyPolicyProperties

		public void TestConcurrencyPolicyProperties()
		{
			var adjustment = GetNewBusinessObject();
			AssertEquals(ConcurrencyPolicy.Strict, adjustment.WD_DocketStatusInfo.ConcurrencyPolicy);
		}

		#endregion

		// interface members

		#region ICreditControlledDocumentDelivery Members

		protected override bool RequiresCreditCheck
		{
			get { return false; }
		}

		#endregion

		#region IDocManagerSupport Members

		public override void TestDocManagerInfo()
		{
			DocManagerInfo info = Docket.DocManagerInfo;
			AssertEquals(Docket, info.BusinessEntity);
			AssertEquals("WAD", info.DocManagerCode);
		}

		#endregion

		#region IDocumentSupportable Members

		public void TestDocumentSupporter()
		{
			AssertEquals(typeof(WhsAdjustmentDocumentSupporter), Docket.DocumentSupporter.GetType());
		}

		#endregion

		#region IWhsLogEventParent

		protected override string ExpectedEventReferenceParameterType => Constants.EventReferenceParameterTypes.Adjustment;

		#endregion

		#region ICanRecordStockLostDuringPick Members

		#region TestAdjustOutInventory_ThrowsErrorWhenQtyToReduceIsGreaterThanInventoryTotalUnits

		public void TestAdjustOutInventory_ThrowsErrorWhenQtyToReduceIsGreaterThanInventoryTotalUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var recorder = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, data.Org1, ReduceStockReason.Lost);

			var inventory = receive.Inventory[0];
			AssertExceptionThrown(typeof(ArgumentException), "Cannot record lost stock more than inventory TotalUnits.", () => recorder.AdjustOutInventory(inventory, inventory.WI_WL, 11m));
		}

		#endregion

		#region TestAdjustOutInventory_ThrowsErrorWhenInventoryIsNull

		public void TestAdjustOutInventory_ThrowsErrorWhenInventoryIsNull()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var recorder = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, data.Org1, ReduceStockReason.Lost);

			AssertExceptionThrown<ArgumentNullException>(() => recorder.AdjustOutInventory(null, ZGuid.NewZGuid(), 1));
		}

		#endregion

		#region TestAdjustOutInventory_ThrowsErrorWhenQuantityNoPositive

		public void TestAdjustOutInventory_ThrowsErrorWhenQuantityNoPositive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var recorder = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, data.Org1, ReduceStockReason.Lost);

			var inventory = receive.Inventory[0];
			AssertExceptionThrown(typeof(ArgumentException), "Quantity must be positive.", () => recorder.AdjustOutInventory(inventory, inventory.WI_WL, 0m));
			AssertExceptionThrown(typeof(ArgumentException), "Quantity must be positive.", () => recorder.AdjustOutInventory(inventory, inventory.WI_WL, -1m));
		}

		#endregion

		#region TestAdjustOutInventory_ThrowsErrorWhenInventoryClientIsDifferent

		public void TestAdjustOutInventory_ThrowsErrorWhenInventoryClientIsDifferent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var wrongClient = Helper.CreateClient("CL2");
			var recorder = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, wrongClient, ReduceStockReason.Lost);

			var inventory = receive.Inventory[0];
			AssertExceptionThrown(typeof(ArgumentException), "Client for inventory is different.", () => recorder.AdjustOutInventory(inventory, inventory.WI_WL, 1m));
		}

		#endregion

		#region TestAdjustOutInventory_ThrowsErrorWhenInventoryWarehouseIsDifferent

		public void TestAdjustOutInventory_ThrowsErrorWhenInventoryWarehouseIsDifferent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var wrongWarehouse = Helper.CreateWarehouse("WH2");
			var recorder = new PickedStockAdjustersFactory().GetNewAdjuster(wrongWarehouse, data.Org1, ReduceStockReason.Lost);

			var inventory = receive.Inventory[0];
			AssertExceptionThrown(typeof(ArgumentException), "Warehouse for inventory is different.", () => recorder.AdjustOutInventory(inventory, inventory.WI_WL, 1m));
		}

		#endregion

		#region TestAdjustOutInventory_ThrowsErrorWhenOriginalLocationIsNotValid

		public void TestAdjustOutInventory_ThrowsErrorWhenOriginalLocationIsNotValid()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var recorder = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, data.Org1, ReduceStockReason.Lost);

			AssertExceptionThrown(typeof(ArgumentException), "Original Inventory must be a valid Guid.", () => recorder.AdjustOutInventory(receive.Inventory[0], ZGuid.Empty, 1));
			AssertExceptionThrown(typeof(ArgumentException), "Original Inventory must be a valid Guid.", () => recorder.AdjustOutInventory(receive.Inventory[0], ZGuid.Invalid, 1));
		}

		#endregion

		#region TestAdjustOutInventory_CreatesAdjustmentOut

		public void TestAdjustOutInventory_CreatesAdjustmentOut()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			data.Part1.OP_StockKeepingUnit = "BOT";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var packingDate = ZDate.Today.AddDays(-3);
			var expiryDate = ZDate.Today.AddDays(-2);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-1", expiryDate, packingDate, "A1", "B2", "C3", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(receive);

			var inventory = receive.Inventory[0];
			var recorder = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, data.Org1, ReduceStockReason.Lost);
			recorder.AdjustOutInventory(inventory, inventory.WI_WE_InDocketLine, 3m);

			var adjustment = (WhsAdjustment)recorder;
			AssertEquals(1, adjustment.Lines.Count);
			var adjustmentLine = adjustment.Lines[0];
			CombineAssertions(() =>
			{
				AssertEquals(-3m, adjustmentLine.WE_TransactionQuantity);
				AssertEquals(InventoryStatus.Codes.Available, adjustmentLine.WE_OriginalInventoryStatus);
				AssertEquals(InventoryStatus.Codes.Available, adjustmentLine.WE_CurrentInventoryStatus);
				AssertEquals(AdjustmentReasonCodesCodeList.Codes.DamagedStock, adjustmentLine.WE_ReasonCode);
				AssertEquals(1, adjustmentLine.PickLines.Count);
				AssertEquals(3m, adjustmentLine.PickLines[0].WZ_Units);
				Assert(adjustmentLine.PickLines[0].WZ_WE_OriginalPickedInventoryLine.IsEmpty);
				AssertEquals(receive.Inventory[0], adjustmentLine.PickLines[0].Inventory);
				AssertEquals(receive.Inventory[0].WI_WL, adjustmentLine.WE_WL);
				AssertEquals("PLT-1", adjustmentLine.WE_PalletID);
				AssertEquals("A1", adjustmentLine.WE_PartAttrib1);
				AssertEquals("B2", adjustmentLine.WE_PartAttrib2);
				AssertEquals("C3", adjustmentLine.WE_PartAttrib3);
				AssertEquals(expiryDate, adjustmentLine.WE_ExpiryDate);
				AssertEquals(packingDate, adjustmentLine.WE_PackingDate);
				AssertEquals("BOT", adjustmentLine.WE_F3_NKPackType);
			});

			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestAdjustOutInventory_CreatesAdjustmentOut_WithSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			data.Part1.OP_StockKeepingUnit = "BOT";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT-1");
			receiveLine.WI_SerialNumber = "SER1";
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(receive);

			var inventory = receive.Inventory[0];
			var recorder = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, data.Org1, ReduceStockReason.Lost);
			recorder.AdjustOutInventory(inventory, inventory.WI_WE_InDocketLine, 1m);

			var adjustment = (WhsAdjustment)recorder;
			AssertEquals(1, adjustment.Lines.Count);
			var adjustmentLine = adjustment.Lines[0];
			CombineAssertions(() =>
			{
				AssertEquals(-1m, adjustmentLine.WE_TransactionQuantity);
				AssertEquals(InventoryStatus.Codes.Available, adjustmentLine.WE_OriginalInventoryStatus);
				AssertEquals(InventoryStatus.Codes.Available, adjustmentLine.WE_CurrentInventoryStatus);
				AssertEquals(AdjustmentReasonCodesCodeList.Codes.DamagedStock, adjustmentLine.WE_ReasonCode);
				AssertEquals(1, adjustmentLine.PickLines.Count);
				AssertEquals(1m, adjustmentLine.PickLines[0].WZ_Units);
				Assert(adjustmentLine.PickLines[0].WZ_WE_OriginalPickedInventoryLine.IsEmpty);
				AssertEquals(receive.Inventory[0], adjustmentLine.PickLines[0].Inventory);
				AssertEquals(receive.Inventory[0].WI_WL, adjustmentLine.WE_WL);
				AssertEquals("SER1", adjustmentLine.WE_SerialNumber);
			});

			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestAdjustOutInventory_CreatesAdjustmentOut_Staged

		public void TestAdjustOutInventory_CreatesAdjustmentOut_Staged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Available (original Inventory's status).", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);

			var inventory = transferLine.Inventory[0];
			var recorder = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, data.Org1, ReduceStockReason.Lost);
			recorder.AdjustOutInventory(inventory, originalInventory.PK, 20m);

			var adjustment = (WhsAdjustment)recorder;
			AssertEquals(1, adjustment.Lines.Count);
			var adjustmentLine = adjustment.Lines[0];
			CombineAssertions(() =>
			{
				AssertEquals(-20m, adjustmentLine.WE_TransactionQuantity);
				AssertEquals(InventoryStatus.Codes.Staged, adjustmentLine.WE_OriginalInventoryStatus);
				AssertEquals(InventoryStatus.Codes.Staged, adjustmentLine.WE_CurrentInventoryStatus);
				AssertEquals(1, adjustmentLine.PickLines.Count);
				AssertEquals("Inventory line", transferLine.PK, adjustmentLine.PickLines[0].WZ_WE_InventoryLine);
				AssertEquals("Original inventory line", originalInventory.PK, adjustmentLine.PickLines[0].WZ_WE_OriginalPickedInventoryLine);
			});
		}

		#endregion

		#region TestAdjustOutInventory_WithPackTypes

		public void TestAdjustOutInventory_WithPackTypes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "CTN";
			Helper.CreateProductUnit(data.Part1, "CTN", "PLT", 24);
			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 0);
			productParams.W3_F3_NKReceivedPackType = "PLT";
			productParams.W3_F3_NKReleasedPackType = "PLT";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 16m);
			Factory.Save();

			var inventory = receive.Inventory[0];
			var recorder = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, data.Org1, ReduceStockReason.Lost);
			recorder.AdjustOutInventory(inventory, inventory.WI_WE_InDocketLine, 16m);

			var adjustment = (WhsAdjustment)recorder;
			AssertEquals(1, adjustment.Lines.Count);
			var adjustmentLine = adjustment.Lines[0];
			CombineAssertions(() =>
			{
				AssertEquals("adjustmentLine.WE_TransactionQuantity", -16m, adjustmentLine.WE_TransactionQuantity);
				AssertEquals("adjustmentLine.WE_OP", data.Part1.PK, adjustmentLine.WE_OP);
				AssertEquals("adjustmentLine.WE_F3_NKPackType", "CTN", adjustmentLine.WE_F3_NKPackType);
				AssertEquals("adjustmentLine.PickLines.Count", 1, adjustmentLine.PickLines.Count);
				AssertEquals("adjustmentLine.PickLines[0].WZ_Units", 16m, adjustmentLine.PickLines[0].WZ_Units);
			});

			var result = recorder.PrepareForSaving();
			AssertEquals("Should be ready to Save.", true, result.IsSuccess);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestLinkToDocket_ThrowsErrorWhenDocketIdNull

		public void TestLinkToDocket_ThrowsErrorWhenDocketIdNull()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);

			AssertExceptionThrown<ArgumentNullException>(() => ((IPickedStockAdjuster)adjustment).LinkToDocket(null));
		}

		#endregion

		#region TestMakeLinkToOriginalOrder

		public void TestMakeLinkToOriginalOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			((IPickedStockAdjuster)adjustment).LinkToDocket(order);

			AssertEquals(adjustment, order.RelatedJobs.SingleOrDefault());
		}

		#endregion

		#region TestPrepareForSaving_FinalisesTheAdjustment

		public void TestPrepareForSaving_FinalisesTheAdjustment()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, data.Whs1.DefaultLocation);

			Assert("Precondition", !adjustment.IsFinalised);
			Assert("PrepareForSaving should not have any problems.", ((IPickedStockAdjuster)adjustment).PrepareForSaving().IsSuccess);
			Assert("Adjustment must become finalised", adjustment.IsFinalised);
		}

		#endregion

		#region TestPrepareForSaving_ReportsValidationErrors

		public void TestPrepareForSaving_ReportsValidationErrors()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, data.Whs1.DefaultLocation);

			var result = ((IPickedStockAdjuster)adjustment).PrepareForSaving();
			AssertEquals("IsSuccess should be false", false, result.IsSuccess);
			AssertEquals("ErrorMessage must be populated.", false, string.IsNullOrEmpty(result.ErrorMessage));
		}

		#endregion

		#endregion

		#region ICriticalChangesVersionID

		protected override void UpdatedDocketVersionAndSubscribeToFactoryCore(BusinessObjectFactory factory, WhsDocket docket)
			=> UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsAdjustment>.UpdateBusinessObjectVersionAndSubscribeToFactory(factory, docket.PK);

		public void TestUpdateAdjustmentVersion_CriticalFields()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var otherClient = Helper.CreateClient();
			var otherWarehouse = Helper.CreateWarehouse("W2");
			var adjustment = Helper.CreateWhsAdjustment(otherClient, otherWarehouse, "A1");
			Factory.Save();

			var changed = 0;
			adjustment.WD_CriticalChangesVersionIDInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				changed++;
			};

			adjustment.WD_WW_Whs = data.Whs1.PK;
			Factory.Save();

			AssertEquals("Should increment calls to update.", 1, changed);
			Assert("Should update to valid Guid.", adjustment.WD_CriticalChangesVersionID.IsValid);

			adjustment.WD_OH_Client = data.Org1.PK;
			Factory.Save();

			AssertEquals("Should increment calls to update.", 2, changed);
			Assert("Should update to valid Guid.", adjustment.WD_CriticalChangesVersionID.IsValid);
		}

		#endregion

		#region TestDocketUpdatedByDataRefresh

		protected override void TestDocketUpdatedByDataRefresh_FinalisedInMemoryCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var adjustmentInNewFactory = newFactory.Load<WhsAdjustment>(adjustment.PK);

			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustment);

			adjustmentInNewFactory.WD_ExternalReference = "NEWA1";
			AssertEquals(false, adjustmentInNewFactory.IsFinalised);
			newFactory.Save();

			AssertEquals("Adjustment is still finalised after data refresh.", true, adjustment.IsFinalised);
			AssertEquals("External reference is updated after data refresh.", "NEWA1", adjustment.WD_ExternalReference);
			Helper.AssertZCannotSaveExceptionThrown("The Adjustment has been updated by another job. Please reload the Adjustment.", Factory.Save);
		}

		protected override void TestDocketUpdatedByDataRefresh_CriticalChangesVersionIDUpdateCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1", Notify);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var adjustmentInNewFactory = newFactory.Load<WhsAdjustment>(adjustment.PK);

			var newCriticalChangesVersionID = ZGuid.NewZGuid();
			AssertNotEquals(adjustment.WD_CriticalChangesVersionID, newCriticalChangesVersionID);

			adjustmentInNewFactory.WD_CriticalChangesVersionID = newCriticalChangesVersionID;
			newFactory.Save();

			AssertEquals("WD_CriticalChangesVersionID is updated after data refresh.", newCriticalChangesVersionID, adjustment.WD_CriticalChangesVersionID);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Adjustment should have the 'need to reload' notification", true, Notify.ContainsNotificationType(WhsErrorTypes.CannotFinaliseWithoutReload));
		}

		#endregion

		#region Implementation

		protected override string ExpectedTemplateCategory
		{
			get { return MailTemplateCategoryList.Codes.WarehouseAdjustment; }
		}

		protected override void AssertDocketLineEqualsInventory(WhsInventoryView inventory, WhsDocketLine line)
		{
			base.AssertDocketLineEqualsInventory(inventory, line);

			AssertEquals("WE_AdjustmentArrivalDate", inventory.WI_ArrivalDate, line.WE_AdjustmentArrivalDate);
			AssertEquals("WE_TransactionQuantity", -inventory.WI_AvailableToPickQuantity, line.WE_TransactionQuantity);
			AssertEquals("WE_OriginalInventoryStatus", inventory.WI_InventoryStatus, line.WE_OriginalInventoryStatus);
			AssertEquals("WE_WHC_NKOriginalInventoryHeldCode", inventory.InDocketLine.WE_WHC_NKOriginalInventoryHeldCode, line.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals("WE_CurrentInventoryStatus", inventory.WI_InventoryStatus, line.WE_CurrentInventoryStatus);
			AssertEquals("WE_WHC_NKCurrentInventoryHeldCode", inventory.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode, line.WE_WHC_NKCurrentInventoryHeldCode);
		}

		protected override ZString ExpectedDescription
		{
			get { return "Adjustment"; }
		}

		protected override ControllerID ExpectedControllerId
		{
			get { return ControllerIDs.WhsAdjustment; }
		}

		protected override DataContextType? ExpectedDataContextType => DataContextType.WarehouseAdjustment;

		protected override string WorkflowDescriptorCode
		{
			get { return WorkflowDescriptors.WhsAdjustmentWorkflowDescriptorCode; }
		}

		protected override WhsAdjustment GetDocketForRating()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			return Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
		}

		#endregion
	}
}
