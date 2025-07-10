using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsAdjustmentLineValidationTest : WhsDocketLineValidationTestCase<WhsAdjustmentLine, WhsAdjustment>
	{
		#region PerformanceTest

		public void TestDBHits_FinalizeAdjustmentInLinesWithPalletIDs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);

			for (int i = 0; i < 100; i++)
			{
				Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, data.Whs1.DefaultLocation.ToLocationString(), palletID: "PLT-1");
			}
			adjustment.RunPreSaveValidation();
			Factory.Save();

			var newfactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var adjustmentInOtherFactory = newfactory.Load<WhsAdjustment>(adjustment.PK);
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ JobDocAddressSchema.Constants.TableName, 2 },
				{ JobServiceSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketContainerSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsDocketReferenceSchema.Constants.TableName, 1 },
				{ WhsDocketPalletSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 3 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 102 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
		};

			using (RowFactory.SetCachedTables())
			{
				adjustmentInOtherFactory.FinaliseDocketWithoutUserConfirmation();
			}

			AssertDbHits(expectedDbHits, newfactory);
			Assert("Adjustment should be finalized.", adjustmentInOtherFactory.IsFinalised);
		}

		#region TestDBHits_RunPreSaveValidationAdjustmentInLinesWithDifferentPalletIDs

		public void TestDBHits_RunPreSaveValidationAdjustmentInLinesWithDifferentPalletIDs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 100);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);

			for (int i = 0; i < 100; i++)
			{
				Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, $"A-1-{i + 1}", palletID: $"PLT-{i}");
			}
			adjustment.RunPreSaveValidation();
			Factory.Save();

			var newfactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var adjustmentInOtherFactory = newfactory.Load<WhsAdjustment>(adjustment.PK);
			var expectedDbHits = new Dictionary<string, int>()
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
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsInventoryViewSchema.Constants.TableName, 3 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 2 },
				{ WhsPickLineSchema.Constants.TableName, 2 },
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 }
			};

			using (RowFactory.SetCachedTables())
			{
				adjustmentInOtherFactory.RunPreSaveValidation();
			}

			AssertDbHits(expectedDbHits, newfactory);
		}

		#endregion

		#endregion

		#region TestCheckWE_AdjustmentArrivalDateIsValidZDateTimeRange

		[TestDate(2019, 10, 10)]
		public override void TestCheckWE_AdjustmentArrivalDateIsValidZDateTimeRange()
		{
			var line = Factory.New<WhsAdjustmentLine>();

			// testing for past date
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now.AddYears(-10).AddDays(-1);
			AssertNoErrors("Should *not* show errors for old dates.", line.WE_AdjustmentArrivalDateInfo);

			line.WE_TransactionQuantity = 1;
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now.AddYears(-10).AddDays(-1); // To trigger validation
			AssertNoErrors("Should *not* show errors for old dates.", line.WE_AdjustmentArrivalDateInfo);

			line.WE_TransactionQuantity = -1;
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now.AddYears(-10).AddDays(-1); // To trigger validation
			AssertNoErrors("Should *not* show errors for old dates.", line.WE_AdjustmentArrivalDateInfo);

			line.WE_TransactionQuantity = 1;
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now.AddYears(-10).AddDays(-1); // To trigger validation
			AssertNoErrors("Should *not* show errors for old dates.", line.WE_AdjustmentArrivalDateInfo);

			// testing for future date
			// Doesnt make sense to adjust in a future arrival date so we will retain this validation
			line.WE_TransactionQuantity = 0;
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now.AddYears(5).AddDays(1); // To trigger validation
			AssertHasErrors("Should show errors for future dates.", line.WE_AdjustmentArrivalDateInfo);

			line.WE_TransactionQuantity = -1;
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now.AddYears(5).AddDays(1); // To trigger validation
			AssertNoErrors("Should *not* show errors for future dates.", line.WE_AdjustmentArrivalDateInfo);

			line.WE_TransactionQuantity = 1;
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now.AddYears(5).AddDays(1); // To trigger validation
			AssertHasErrors("Should show errors for future dates.", line.WE_AdjustmentArrivalDateInfo);
		}

		#endregion

		#region TestCheckWE_ExpiryDateIsValidZDateTimeRange

		[TestDate(2019, 10, 10)]
		public override void TestCheckWE_ExpiryDateIsValidZDateTimeRange()
		{
			var line = Factory.New<WhsAdjustmentLine>();
			line.WE_TransactionQuantity = -5;
			line.WE_ExpiryDate = ZDate.Today;
			AssertNoErrors("Precondition - Should be no errors.", line.WE_ExpiryDateInfo);

			line.WE_ExpiryDate = new ZDate(1941, 06, 22);
			AssertNoErrors("Should *not* show errors for old dates.", line.WE_ExpiryDateInfo);
			AssertHasWarning(line.WE_ExpiryDateInfo, "The date '22-Jun-1941' is more than 1 year old.");

			line.WE_TransactionQuantity = 5;
			line.Validation.ValidateWE_ExpiryDate();
			AssertNoErrors("Should *not* show errors for old dates.", line.WE_ExpiryDateInfo);
			AssertHasWarning(line.WE_ExpiryDateInfo, "The date '22-Jun-1941' is more than 1 year old.");

			// Expiry dates are validly allowed to be old
			line.WE_ExpiryDate = ZDateTime.Now.AddYears(25).AddDays(1).Date;
			AssertNoErrors("Should *not* show errors for future dates.", line.WE_ExpiryDateInfo);

			line.WE_TransactionQuantity = 5;
			line.Validation.ValidateWE_ExpiryDate();
			AssertNoErrors("Should *not* show errors for future dates.", line.WE_ExpiryDateInfo);
		}

		#endregion

		#region TestCheckWE_PackingDateIsValidZDateTimeRange

		[TestDate(2019, 10, 10)]
		public override void TestCheckWE_PackingDateIsValidZDateTimeRange()
		{
			var line = Factory.New<WhsAdjustmentLine>();
			line.WE_TransactionQuantity = -5;
			line.WE_PackingDate = ZDate.Today;
			AssertNoErrors("Precondition - Should be no errors.", line.WE_PackingDateInfo);

			line.WE_PackingDate = new ZDate(1941, 06, 22);
			AssertNoErrors("Should *not* show errors for old dates.", line.WE_PackingDateInfo);
			AssertHasWarning(line.WE_PackingDateInfo, "The date '22-Jun-1941' is more than 1 year old.");

			line.WE_TransactionQuantity = 5;
			line.Validation.ValidateWE_PackingDate();
			AssertNoErrors(line.WE_PackingDateInfo);
			AssertHasWarning(line.WE_PackingDateInfo, "The date '22-Jun-1941' is more than 1 year old.");

			// Doesnt make sense to adjust in a future date so we will retain this validation
			line.WE_PackingDate = ZDateTime.Now.AddYears(5).AddDays(1).Date;
			AssertHasErrors("Should show errors for future dates.", line.WE_PackingDateInfo);

			line.WE_TransactionQuantity = 5;
			line.Validation.ValidateWE_PackingDate();
			AssertHasErrors("Should show errors for future dates.", line.WE_PackingDateInfo);
		}

		#endregion

		#region TestCheckWE_RequiredByDate_IsValidZDateTimeRange

		[TestDate(2019, 10, 10)]
		public override void TestCheckWE_RequiredByDate_IsValidZDateTimeRange()
		{
			var line = Factory.New<WhsAdjustmentLine>();
			line.WE_TransactionQuantity = -5;
			line.WE_RequiredByDate = ZDateTimeOffset.Now;
			AssertNoErrors("Precondition - Should be no errors.", line.WE_RequiredByDateInfo);

			line.WE_RequiredByDate = new ZDateTimeOffset(1941, 06, 22);
			AssertNoErrors("Should *not* show errors for old dates.", line.WE_RequiredByDateInfo);
			AssertHasWarning(line.WE_RequiredByDateInfo, "The date '22-Jun-1941' is more than 1 year old.");

			line.WE_TransactionQuantity = 5;
			line.Validation.ValidateWE_RequiredByDate();
			AssertNoErrors("Should *not* show errors for old dates.", line.WE_RequiredByDateInfo);
			AssertHasWarning(line.WE_RequiredByDateInfo, "The date '22-Jun-1941' is more than 1 year old.");

			// Doesnt make sense to adjust in a future date so we will retain this validation
			line.WE_RequiredByDate = ZDateTimeOffset.Now.AddYears(5).AddDays(1);
			AssertHasErrors("Should show errors for future dates.", line.WE_RequiredByDateInfo);

			line.WE_TransactionQuantity = 5;
			line.Validation.ValidateWE_RequiredByDate();
			AssertHasErrors("Should show errors for future dates.", line.WE_RequiredByDateInfo);
		}

		#endregion

		#region TestCheckWE_ReasonCode

		public void TestCheckWE_ReasonCode()
		{
			var line = DocketLine;
			line.WE_ReasonCode = "";
			AssertMandatoryValidationError(line.WE_ReasonCodeInfo, true);

			line.WE_ReasonCode = "STA";
			AssertMandatoryValidationError(line.WE_ReasonCodeInfo, false);
			AssertListValidationInvalidCodeError(line.WE_ReasonCodeInfo, false);

			line.WE_ReasonCode = "XXX";
			AssertListValidationInvalidCodeError(line.WE_ReasonCodeInfo, true);
		}

		#endregion

		#region TestCheckWE_CurrentHoldReason

		public void TestCheckWE_CurrentHoldReasonPositiveQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, "MEL");
			adjustmentLine.WE_CurrentHoldReason = "Because I said so!";
			AssertHasError(adjustmentLine.WE_CurrentHoldReasonInfo, WhsDocketLineValidation.ErrorForHoldReasonWithoutHoldCode);

			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, "MEL");
			adjustmentLine2.WE_WHC_NKOriginalInventoryHeldCode = "HEL";
			adjustmentLine2.WE_CurrentHoldReason = "Because I said so!";
			AssertNoErrors(adjustmentLine2.WE_CurrentHoldReasonInfo);
		}

		public void TestCheckWE_CurrentHoldReasonNegativeQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, "MEL");
			adjustmentLine.WE_CurrentHoldReason = "Because I said so!";
			AssertNoErrors(adjustmentLine.WE_CurrentHoldReasonInfo);
		}

		#endregion

		#region TestCheckWE_TransactionQuantity

		protected override void TestCheckWE_TransactionQuantityCore()
		{
			base.TestCheckWE_TransactionQuantityCore();

			var adjustmentLine = Factory.New<WhsAdjustmentLine>();
			var data = new TestDataSimpleDocketAdjustmentDocket(Factory);
			data.RemoveLineFromParent();
			data.Line.WE_TransactionQuantity = 0m;
			AssertHasErrorContaining(data.Line.WE_TransactionQuantityInfo, WhsAdjustmentLineValidation.ErrorMsgCannotBeZero);

			data.AttachLineToParent();
			data.Line.WE_TransactionQuantity = 0m;
			AssertHasErrorContaining(data.Line.WE_TransactionQuantityInfo, WhsAdjustmentLineValidation.ErrorMsgCannotBeZero);

			Helper.EnableWarehouseForBond(data.Whs, true);

			data.Line.WE_TransactionQuantity = 0m;
			data.Line.Validation.ValidateWE_TransactionQuantity();
			// Can not be zero for bonded whs
			AssertHasErrorContaining(data.Line.WE_TransactionQuantityInfo, WhsAdjustmentLineValidation.ErrorMsgCannotBeZero);
		}

		#endregion

		#region TestCheckWE_TransactionQuantityCannotZero

		public void TestCheckWE_TransactionQuantityCannotZero()
		{
			var adjustment = Factory.New<WhsAdjustment>();
			var adjustmentLine = adjustment.Lines.AddNew();
			adjustment.WD_WW_Whs = Helper.CreateWarehouse("1").PK;

			TestCheckWE_TransactionQuantityCannotZeroCore(adjustmentLine);
		}

		public void TestCheckWE_TransactionQuantityCannotZero_BondedWarehouse()
		{
			var adjustment = Factory.New<WhsAdjustment>();
			var adjustmentLine = adjustment.Lines.AddNew();
			var warehouse = Helper.CreateWarehouse("1");
			adjustment.WD_WW_Whs = warehouse.PK;
			Helper.EnableWarehouseForBond(warehouse, true);

			TestCheckWE_TransactionQuantityCannotZeroCore(adjustmentLine);
		}

		public void TestCheckWE_TransactionQuantityCannotZero_WarehouseForExcise()
		{
			var adjustment = Factory.New<WhsAdjustment>();
			var adjustmentLine = adjustment.Lines.AddNew();
			var warehouse = Helper.CreateWarehouse("1");
			adjustment.WD_WW_Whs = warehouse.PK;
			Helper.EnableWarehouseForExcise(warehouse, true);

			TestCheckWE_TransactionQuantityCannotZeroCore(adjustmentLine);
		}

		public void TestCheckWE_TransactionQuantityCannotZero_Customs()
		{
			var adjustment = Factory.New<WhsAdjustment>();
			var adjustmentLine = adjustment.Lines.AddNew();
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;

			TestCheckWE_TransactionQuantityCannotZeroCore(adjustmentLine);
		}

		void TestCheckWE_TransactionQuantityCannotZeroCore(WhsAdjustmentLine adjustmentLine)
		{
			adjustmentLine.WE_TransactionQuantity = 0m;
			AssertHasErrorContaining(adjustmentLine.WE_TransactionQuantityInfo, WhsAdjustmentLineValidation.ErrorMsgCannotBeZero);

			adjustmentLine.WE_TransactionQuantity = -1m;
			AssertNoErrorContaining(adjustmentLine.WE_TransactionQuantityInfo, WhsAdjustmentLineValidation.ErrorMsgCannotBeZero);

			adjustmentLine.WE_TransactionQuantity = 1m;
			AssertNoErrorContaining(adjustmentLine.WE_TransactionQuantityInfo, WhsAdjustmentLineValidation.ErrorMsgCannotBeZero);
		}

		#endregion

		#region TestCheckWE_TransactionQuantity_CheckEnoughInventoryExistsToCommit

		public void TestCheckWE_TransactionQuantity_CheckEnoughInventoryExistsToCommit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var errorMsg = @"Attempted to adjust 10 Units, but only 5 Units are available for adjustment out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the adjustment line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to adjust.
If you are trying to adjust stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to adjust stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.";
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, data.Whs1.DefaultLocation);
			adjustmentLine.RunPreSaveValidation();
			AssertHasError(adjustmentLine.WE_TransactionQuantityInfo, errorMsg);

			using (adjustment.AttemptDodgyBondedFinalise())
			{
				adjustmentLine.RunPreSaveValidation();
				AssertHasError("Error should still be present as the Adjustment Line is being validated directly.", adjustmentLine.WE_TransactionQuantityInfo, errorMsg);

				adjustment.RunPreSaveValidation();
				AssertNoErrors("Error should be gone as the Adjustment is validating the lines during a Bonded Finalise.", adjustmentLine.WE_TransactionQuantityInfo);
			}

			adjustment.RunPreSaveValidation();
			AssertHasError("Error should still be present as the Adjustment is no longer in Bonded Finalise.", adjustmentLine.WE_TransactionQuantityInfo, errorMsg);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			adjustmentLine.RunPreSaveValidation();
			AssertNoErrors("Error should be gone as the Adjustment is fully committed.", adjustmentLine.WE_TransactionQuantityInfo);
		}

		#endregion

		#region TestCheckWE_TransactionQuantityForNewOwenrshipAdjustmentParent

		public void TestCheckWE_TransactionQuantityForNewOwenrshipAdjustmentParent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var newClient = Helper.CreateClient("C1");
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var lineForAadjustment = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1, "A");
			var changeOwnershipAdjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			var lineForChangeOwnershipAdjustment = Helper.CreateWhsAdjustmentLine(changeOwnershipAdjustment, data.Part1, 1, "A");

			AssertHasError(lineForChangeOwnershipAdjustment.WE_TransactionQuantityInfo, "Units should be negative.");
			AssertNoErrors(lineForAadjustment.WE_TransactionQuantityInfo);
			lineForChangeOwnershipAdjustment.WE_TransactionQuantity = -1;
			AssertNoErrors(lineForChangeOwnershipAdjustment.WE_TransactionQuantityInfo);
		}

		#endregion

		#region TestCheckWE_LineComment

		public void TestCheckWE_LineComment()
		{
			DocketLine.Validation.ValidateWE_LineComment();
			AssertMandatoryValidationError(DocketLine.WE_LineCommentInfo, false);
		}

		#endregion

		#region TestCheckWE_BondedEntryKey

		public void TestCheckWE_BondedEntryKey()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Adjustment;
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10, data.Whs1.DefaultLocation);
			adjustmentLine.WE_BondedEntryKey = "ABC-1";

			var expectedMessage = "Bonded entry key should not be entered for Non-Bonded adjustments.";
			AssertHasError(adjustmentLine.WE_BondedEntryKeyInfo, expectedMessage);

			adjustmentLine.WE_BondedEntryKey = "";
			AssertNoError(adjustmentLine.WE_BondedEntryKeyInfo, expectedMessage);

			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			adjustmentLine.WE_BondedEntryKey = "ABC-1";
			AssertNoError(adjustmentLine.WE_BondedEntryKeyInfo, expectedMessage);
		}

		#endregion

		#region Attributes

		#region TestIsAttributeValidationRequired

		public override void TestIsAttributeValidationRequired()
		{
			AssertEquals(false, DocketLine.Validation.IsAttributeValidationRequired);
			DocketLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			Docket.WD_OH_Client = Factory.New<OrgHeader>().PK;
			AssertEquals(true, DocketLine.Validation.IsAttributeValidationRequired);

			DocketLine.WE_TransactionQuantity = -1;
			AssertEquals(true, DocketLine.Validation.IsAttributeValidationRequired);

			DocketLine.WE_TransactionQuantity = 1;
			AssertEquals(true, DocketLine.Validation.IsAttributeValidationRequired);
		}

		#endregion

		#region TestPartAttributeValidationBasedOnUnits

		public void TestPartAttributeValidationBasedOnUnits_Attribute1()
		{
			AssertPartAttributeValidationBasedOnUnits(AttributeNumber.One, WhsDocketLineSchema.WE_PartAttrib1, l => l.WE_PartAttrib1Info, "Please enter a Part Attrib. 1.");
		}

		public void TestPartAttributeValidationBasedOnUnits_Attribute2()
		{
			AssertPartAttributeValidationBasedOnUnits(AttributeNumber.Two, WhsDocketLineSchema.WE_PartAttrib2, l => l.WE_PartAttrib2Info, "Please enter a Part Attrib. 2.");
		}

		public void TestPartAttributeValidationBasedOnUnits_Attribute3()
		{
			AssertPartAttributeValidationBasedOnUnits(AttributeNumber.Three, WhsDocketLineSchema.WE_PartAttrib3, l => l.WE_PartAttrib3Info, "Please enter a Part Attrib. 3.");
		}

		void AssertPartAttributeValidationBasedOnUnits(AttributeNumber attributeNo, SchemaStringColumn attributeColumn, Func<WhsDocketLine, ZPropertyInfo> getPropertyInfo, string expectedErrorMessage)
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(10, 10, 10, 0, 0);
			data.Part1.OP_StockKeepingUnit = "BAG";
			Factory.Save();
			var inventories = Helper.LoadInventory();
			AssertEquals("Precondition", 30m, inventories.UnitsAvailable);

			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			Helper.SetClientAttributeType(data.Org1, attributeNo, PartAttributeTypeList.Codes.Mandatory);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "1", Notify);
			var line1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10, "A-1-1");
			var line2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10, "A-1-2");
			var line3 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 100, "A-3-2");  // will fail
			line1[attributeColumn] = "No1";
			adjustment.FinaliseDocket();

			AssertEquals("Precondition", true, adjustment.HasErrors && !adjustment.IsFinalised);
			AssertEquals("Lines should not disappear", 3, adjustment.Lines.Count);
			AssertNoErrors(getPropertyInfo(line1));
			AssertNoErrors(getPropertyInfo(line2));
			AssertHasErrorContaining(getPropertyInfo(line3), expectedErrorMessage);
		}

		#endregion

		#region TestAdjustOutGivesWarningNotErrorOnMandatoryFields

		public void TestPartAttributeValidation_WarningWhenMandatoryAttributeIsEmpty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.BatchNumber);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, PartAttributeTypeList.Codes.Mandatory);

			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "1", Notify);
			var adjustIn = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10, "A-1");
			adjustIn.RunPreSaveValidation();
			AssertHasError("Adjust in Attribute 1 should have error", adjustIn.WE_PartAttrib1Info, "Please enter a Part Attrib. 1.");
			AssertHasError("Adjust in Attribute 2 should have error", adjustIn.WE_PartAttrib2Info, "Please enter a Part Attrib. 2.");
			AssertHasError("Adjust in Attribute 3 should have error", adjustIn.WE_PartAttrib3Info, "Please enter a Part Attrib. 3.");
			AssertHasError("Adjust in WE_ExpiryDateInfo should have error", adjustIn.WE_ExpiryDateInfo, "Please enter an Expiry date.");
			AssertHasError("Adjust in WE_PackingDateInfo should have error", adjustIn.WE_PackingDateInfo, "Please enter a Packing date.");
			AssertNoWarnings("Adjust in should have no warnings", adjustIn.WE_PartAttrib1Info);
			AssertNoWarnings("Adjust in should have no warnings", adjustIn.WE_PartAttrib2Info);
			AssertNoWarnings("Adjust in should have no warnings", adjustIn.WE_PartAttrib3Info);
			AssertNoWarnings("Adjust in should have no warnings", adjustIn.WE_ExpiryDateInfo);
			AssertNoWarnings("Adjust in should have no warnings", adjustIn.WE_PackingDateInfo);

			var adjustOut = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, "A-1");
			adjustOut.RunPreSaveValidation();
			AssertHasWarning("Adjust out Attribute 1 should have warning", adjustOut.WE_PartAttrib1Info, "Please enter a Part Attrib. 1.");
			AssertHasWarning("Adjust out Attribute 2 should have warning", adjustOut.WE_PartAttrib2Info, "Please enter a Part Attrib. 2.");
			AssertHasWarning("Adjust out Attribute 3 should have warning", adjustOut.WE_PartAttrib3Info, "Please enter a Part Attrib. 3.");
			AssertHasWarning("Adjust out WE_ExpiryDateInfo should have warning", adjustOut.WE_ExpiryDateInfo, "Please enter an Expiry date.");
			AssertHasWarning("Adjust out WE_PackingDateInfo should have warning", adjustOut.WE_PackingDateInfo, "Please enter a Packing date.");
			AssertNoErrors("Adjust out should have no errors", adjustOut.WE_PartAttrib1Info);
			AssertNoErrors("Adjust out should have no errors", adjustOut.WE_PartAttrib2Info);
			AssertNoErrors("Adjust out should have no errors", adjustOut.WE_PartAttrib3Info);
			AssertNoErrors("Adjust out should have no errors", adjustOut.WE_ExpiryDateInfo);
			AssertNoErrors("Adjust out should have no errors", adjustOut.WE_PackingDateInfo);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.VIN);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory);

			adjustIn.RunPreSaveValidation();
			AssertHasError("Adjust out Attribute 1 should have warning", adjustIn.WE_PartAttrib1Info, "Please enter a Part Attrib. 1.");
			AssertHasError("Adjust out Attribute 2 should have warning", adjustIn.WE_PartAttrib2Info, "Please enter a Part Attrib. 2.");
			AssertNoWarnings("Adjust out should have no errors", adjustIn.WE_PartAttrib1Info);
			AssertNoWarnings("Adjust out should have no errors", adjustIn.WE_PartAttrib2Info);

			adjustOut.RunPreSaveValidation();
			AssertHasWarning("Adjust out Attribute 1 should have warning", adjustOut.WE_PartAttrib1Info, "Please enter a Part Attrib. 1.");
			AssertHasWarning("Adjust out Attribute 2 should have warning", adjustOut.WE_PartAttrib2Info, "Please enter a Part Attrib. 2.");
			AssertNoErrors("Adjust out should have no errors", adjustOut.WE_PartAttrib1Info);
			AssertNoErrors("Adjust out should have no errors", adjustOut.WE_PartAttrib2Info);
		}

		public void TestPartAttributeValidation_WarningWhenUnexpectedAttributeIsPresent()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "1", Notify);
			var adjustIn = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1, "A-1");
			adjustIn.WE_PartAttrib1 = "A";
			adjustIn.WE_PartAttrib2 = "A";
			adjustIn.WE_PartAttrib3 = "A";
			adjustIn.WE_ExpiryDate = ZDate.Today;
			adjustIn.WE_PackingDate = ZDate.Today;
			adjustIn.WE_SerialNumber = "A";
			adjustIn.RunPreSaveValidation();
			AssertHasError("Adjust in Attribute 1 should have error", adjustIn.WE_PartAttrib1Info, "Part Attrib. 1 is not specified on the Product Master. Please do not enter a value.");
			AssertHasError("Adjust in Attribute 2 should have error", adjustIn.WE_PartAttrib2Info, "Part Attrib. 2 is not specified on the Product Master. Please do not enter a value.");
			AssertHasError("Adjust in Attribute 3 should have error", adjustIn.WE_PartAttrib3Info, "Part Attrib. 3 is not specified on the Product Master. Please do not enter a value.");
			AssertHasError("Adjust in WE_ExpiryDateInfo should have error", adjustIn.WE_ExpiryDateInfo, "Expiry Date is not specified on the Product Master. Please do not enter a value.");
			AssertHasError("Adjust in WE_PackingDateInfo should have error", adjustIn.WE_PackingDateInfo, "Packing Date is not specified on the Product Master. Please do not enter a value.");
			AssertHasError("Adjust in WE_SerialNumberInfo should have error", adjustIn.WE_SerialNumberInfo, "Serial Number is not specified on the Product Master. Please do not enter a value.");
			AssertNoWarnings("Adjust in should have no warnings", adjustIn.WE_PartAttrib1Info);
			AssertNoWarnings("Adjust in should have no warnings", adjustIn.WE_PartAttrib2Info);
			AssertNoWarnings("Adjust in should have no warnings", adjustIn.WE_PartAttrib3Info);
			AssertNoWarnings("Adjust in should have no warnings", adjustIn.WE_ExpiryDateInfo);
			AssertNoWarnings("Adjust in should have no warnings", adjustIn.WE_PackingDateInfo);
			AssertNoWarnings("Adjust in should have no warnings", adjustIn.WE_SerialNumberInfo);

			var adjustOut = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, "A-1");
			adjustOut.WE_PartAttrib1 = "A";
			adjustOut.WE_PartAttrib2 = "A";
			adjustOut.WE_PartAttrib3 = "A";
			adjustOut.WE_ExpiryDate = ZDate.Today;
			adjustOut.WE_PackingDate = ZDate.Today;
			adjustOut.WE_SerialNumber = "A";
			adjustOut.RunPreSaveValidation();
			AssertHasWarning("Adjust out Attribute 1 should have warning", adjustOut.WE_PartAttrib1Info, "Part Attrib. 1 is not specified on the Product Master. Please do not enter a value.");
			AssertHasWarning("Adjust out Attribute 2 should have warning", adjustOut.WE_PartAttrib2Info, "Part Attrib. 2 is not specified on the Product Master. Please do not enter a value.");
			AssertHasWarning("Adjust out Attribute 3 should have warning", adjustOut.WE_PartAttrib3Info, "Part Attrib. 3 is not specified on the Product Master. Please do not enter a value.");
			AssertHasWarning("Adjust out WE_ExpiryDateInfo should have warning", adjustOut.WE_ExpiryDateInfo, "Expiry Date is not specified on the Product Master. Please do not enter a value.");
			AssertHasWarning("Adjust out WE_PackingDateInfo should have warning", adjustOut.WE_PackingDateInfo, "Packing Date is not specified on the Product Master. Please do not enter a value.");
			AssertHasWarning("Adjust in WE_SerialNumberInfo should have error", adjustOut.WE_SerialNumberInfo, "Serial Number is not specified on the Product Master. Please do not enter a value.");
			AssertNoErrors("Adjust out should have no errors", adjustOut.WE_PartAttrib1Info);
			AssertNoErrors("Adjust out should have no errors", adjustOut.WE_PartAttrib2Info);
			AssertNoErrors("Adjust out should have no errors", adjustOut.WE_PartAttrib3Info);
			AssertNoErrors("Adjust out should have no errors", adjustOut.WE_ExpiryDateInfo);
			AssertNoErrors("Adjust out should have no errors", adjustOut.WE_PackingDateInfo);
			AssertNoErrors("Adjust out should have no warnings", adjustOut.WE_SerialNumberInfo);
		}

		public void TestPartAttributeValidation_ReleaseCaptureValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: true);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "Reference1");
			var adjustInLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, data.Whs1.DefaultLocation);
			var adjustOutLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, data.Whs1.DefaultLocation);
			using (new SemaphoreManager(adjustment.FinaliseDocketSemaphore))
			{
				adjustInLine.WE_PartAttrib1 = "1";
				adjustInLine.WE_PartAttrib2 = "2";
				adjustInLine.WE_PartAttrib3 = "3";
				adjustOutLine.WE_PartAttrib1 = "1";
				adjustOutLine.WE_PartAttrib2 = "2";
				adjustOutLine.WE_PartAttrib3 = "3";
				AssertHasError(adjustInLine.WE_PartAttrib1Info, "This attribute is specified as Release Captured for this Product, no value should be entered.");
				AssertHasError(adjustInLine.WE_PartAttrib2Info, "This attribute is specified as Release Captured for this Product, no value should be entered.");
				AssertHasError(adjustInLine.WE_PartAttrib3Info, "This attribute is specified as Release Captured for this Product, no value should be entered.");
				AssertHasWarning(adjustOutLine.WE_PartAttrib1Info, "This attribute is specified as Release Captured for this Product, no value should be entered.");
				AssertHasWarning(adjustOutLine.WE_PartAttrib2Info, "This attribute is specified as Release Captured for this Product, no value should be entered.");
				AssertHasWarning(adjustOutLine.WE_PartAttrib3Info, "This attribute is specified as Release Captured for this Product, no value should be entered.");

				adjustOutLine.WE_PartAttrib1 = string.Empty;
				adjustOutLine.WE_PartAttrib2 = string.Empty;
				adjustOutLine.WE_PartAttrib3 = string.Empty;
				AssertNoErrors(adjustOutLine.WE_PartAttrib1Info);
				AssertNoErrors(adjustOutLine.WE_PartAttrib2Info);
				AssertNoErrors(adjustOutLine.WE_PartAttrib3Info);
			}
		}

		public void TestPartAttributeValidation_JulianBatchNumber_MultiAttribute_AdjustmentOut()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YYDDD;

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "Reference1");
			var adjustOutLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, data.Whs1.DefaultLocation);

			adjustOutLine.WE_PartAttrib1 = "ABCD";
			adjustOutLine.WE_PartAttrib2 = "ABCD";
			adjustOutLine.WE_PartAttrib3 = "ABCD";

			adjustOutLine.Validation.ValidateAll();
			AssertJulianBatchNumberAttribute(AssertHasWarning, adjustOutLine.WE_PartAttrib1Info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);
			AssertJulianBatchNumberAttribute(AssertNoWarning, adjustOutLine.WE_PartAttrib2Info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);
			AssertJulianBatchNumberAttribute(AssertNoWarning, adjustOutLine.WE_PartAttrib3Info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);
		}

		#endregion

		#region TestCheckWE_SerialNumber

		public void TestCheckWE_SerialNumber_QtyGreaterThanOne()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AA1");
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, data.Whs1.DefaultLocation);
			adjustmentLine.WE_SerialNumber = "Ser";
			AssertNoErrors("Precondition: No errors for serial number and 1 Qty", adjustmentLine.WE_TransactionQuantityInfo);

			adjustmentLine.WE_TransactionQuantity = 10m;
			AssertHasError("Errors for serial number and 10 Qty", adjustmentLine.WE_TransactionQuantityInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			adjustmentLine.WE_SerialNumber = "";
			AssertNoErrors("No errors for No serial number and 10 Qty", adjustmentLine.WE_TransactionQuantityInfo);

			adjustmentLine.WE_SerialNumber = "REM";
			AssertHasError("Errors for serial number and 10 Qty", adjustmentLine.WE_TransactionQuantityInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			adjustmentLine.WE_TransactionQuantity = -10m;
			AssertNoErrors("No errors for No serial number and -10 Qty", adjustmentLine.WE_TransactionQuantityInfo);

			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				adjustmentLine.WE_TransactionQuantity = 10m;
				AssertNoErrors("No errors for No serial number and 10 Qty", adjustmentLine.WE_TransactionQuantityInfo);
			}
		}

		public void TestCheckWE_SerialNumber_ReleaseCaptureValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "Reference1");
			var adjustInLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, data.Whs1.DefaultLocation);
			var adjustOutLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, data.Whs1.DefaultLocation);
			using (new SemaphoreManager(adjustment.FinaliseDocketSemaphore))
			{
				adjustInLine.WE_SerialNumber = "SER";
				adjustOutLine.WE_SerialNumber = "SER";
				AssertHasError(adjustInLine.WE_SerialNumberInfo, "Serial Number is specified as Release Captured for this Product, no value should be entered.");
				AssertHasWarning(adjustOutLine.WE_SerialNumberInfo, "Serial Number is specified as Release Captured for this Product, no value should be entered.");

				adjustOutLine.WE_SerialNumber = "";
				AssertNoErrors(adjustOutLine.WE_SerialNumberInfo);
			}
		}

		public void TestCheckWE_SerialNumber_ValidatesLineUnitsWithinTheCorrectPropertysValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var dodgyAdjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "DODGY");
			var dodgyLine = Helper.CreateWhsAdjustmentLine(dodgyAdjustment, data.Part1, 2m, data.Whs1.DefaultLocation);
			dodgyAdjustment.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertIsFinalisedPrecondition(dodgyAdjustment);

			dodgyLine.WE_SerialNumber = "SER";
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "Reference1");
			var adjustInLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, data.Whs1.DefaultLocation);
			var adjustOutLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -2m, data.Whs1.DefaultLocation);
			adjustInLine.WE_SerialNumber = "SER";
			adjustOutLine.WE_SerialNumber = "SER";

			using (new SemaphoreManager(adjustment.FinaliseDocketSemaphore))
			{
				adjustment.RunPreSaveValidation();
				AssertHasError(adjustInLine.WE_TransactionQuantityInfo, "Must always be 1 or less for serial number controlled products");
				AssertNoNotifications(adjustOutLine.WE_TransactionQuantityInfo);
			}
		}

		#endregion

		#endregion

		#region TestCheckWE_WL_IsDoubleCheckedDuringFinalise

		public virtual void TestCheckWE_WL_IsDoubleCheckedDuringFinalise()
		{
			AssertNoErrors(DocketLine.WE_WLInfo);

			using (new SemaphoreManager(Docket.FinaliseDocketSemaphore))
			{
				DocketLine.WE_WL = ZGuid.Empty;
				AssertMandatoryValidationError(DocketLine.WE_WLInfo, true);
			}
		}

		#endregion

		#region TestCheckWE_WL_TotalPallets
		
		public void TestCheckWE_WL_TotalPallets()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");

			location1.WLV_PalletFloorSpaces = 1;
			location1.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1, "Pallet-1");
			receive.FinaliseDocket();
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, location1.ToLocationString(), "Pallet-1");
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 4m, location1.ToLocationString(), "Pallet-2");

			adjustment.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			adjustmentLine1.RunPreSaveValidation();
			adjustmentLine2.RunPreSaveValidation();
			adjustment.FinaliseDocket();

			var expectedErrorMessage = "Total required Pallets (2) exceeds the maximum available Pallets (1) for this location.";

			AssertHasError(adjustmentLine1.WE_WLInfo, expectedErrorMessage);
			AssertHasError(adjustmentLine2.WE_WLInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckLocationString

		#region TestCheckLocationString_CheckLocationIsNotVoid

		public void TestCheckLocationString_CheckLocationIsNotVoid()
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 3, 1);
			Factory.Save();
			var locations = whs.Rows.Single(r => r.WR_Name == "A").Locations;

			Docket.WD_WW_Whs = whs.PK;
			locations[1].WLV_LocationStatus = LocationStatus.Codes.Void;
			locations[2].WLV_LocationStatus = LocationStatus.Codes.Void;

			var adjustmentLine = DocketLine;
			adjustmentLine.LocationString = "A-2";
			AssertNoError(adjustmentLine.LocationStringInfo, WhsAdjustmentLineValidation.ErrorAdjustmentStockIntoVoidLocation);

			adjustmentLine.WE_TransactionQuantity = -10m;
			adjustmentLine.LocationString = "A-3";
			AssertNoError(adjustmentLine.LocationStringInfo, WhsAdjustmentLineValidation.ErrorAdjustmentStockIntoVoidLocation);

			adjustmentLine.WE_TransactionQuantity = 10m;
			adjustmentLine.LocationString = "A-2";
			AssertHasError(adjustmentLine.LocationStringInfo, WhsAdjustmentLineValidation.ErrorAdjustmentStockIntoVoidLocation);
		}

		#endregion

		#region TestCheckLocationString_CheckLocationForMaxWeightVolumeQuantity

		#region TestCheckLocationString_CheckLocationForMaxWeight

		public void TestCheckLocationString_CheckLocationForMaxWeight()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			Helper.SetLocationMaxWeightAndVolume(locations[0], 0m, "", 0m, "");
			Helper.SetLocationMaxWeightAndVolume(locations[1], 100m, "KG", 0m, "");
			Helper.SetLocationMaxWeightAndVolume(locations[2], 100m, "KG", 0m, "");

			Helper.SetProductWeightAndVolume(data.Part1, 1m, "KG", 0m, "");
			Helper.SetProductWeightAndVolume(data.Part2, 1000m, "G", 0m, "");

			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 75m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 75m);
			inventory1.WI_WL = locations[0].PK;
			inventory2.WI_WL = locations[1].PK;

			Factory.Save();

			AssertCheckLocationString_CheckLocationForMaxWeightVolume(data, new string[] { "Total required Weight (", ") exceeds the maximum available Weight (", ") for this location." });
		}

		#endregion

		#region TestCheckLocationString_CheckLocationForMaxVolume

		public void TestCheckLocationString_CheckLocationForMaxVolume()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			Helper.SetLocationMaxWeightAndVolume(locations[0], 0m, "", 0m, "");
			Helper.SetLocationMaxWeightAndVolume(locations[1], 0m, "", 2m, "M3");
			Helper.SetLocationMaxWeightAndVolume(locations[2], 0m, "", 2m, "M3");

			Factory.Save();

			Helper.SetProductWeightAndVolume(data.Part1, 0m, "", 20m, "D3");
			Helper.SetProductWeightAndVolume(data.Part2, 0m, "", 0.02m, "M3");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 75m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 75m);
			inventory1.WI_WL = locations[0].PK;
			inventory2.WI_WL = locations[1].PK;

			Factory.Save();

			AssertCheckLocationString_CheckLocationForMaxWeightVolume(data, new string[] { "Total required Volume (", ") exceeds the maximum available Volume (", ") for this location." });
		}

		#endregion

		#region TestCheckLocationString_CheckLocationForMaxWeightVolume_DoesNotCauseNullReferencException

		[ExpectNoExceptions]
		public void TestCheckLocationString_CheckLocationForMaxWeightVolume_DoesNotCauseNullReferencException()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			Helper.SetLocationMaxWeightAndVolume(locations[1], 100m, "KG", 2m, "M3");
			Helper.SetProductWeightAndVolume(data.Part1, 1000m, "G", 20m, "D3");

			WhsAdjustment adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, ZGuid.Empty, 10m, "A-2"); // no product set
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, ""); // no location set
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, "A-2");
		}

		#endregion

		#region AssertCheckLocationString_CheckLocationForMaxWeightVolume

		void AssertCheckLocationString_CheckLocationForMaxWeightVolume(TestDataSimpleEnvironment data, string[] expectedWarningMessageParts)
		{
			WhsAdjustment adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			WhsAdjustmentLine adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 75m, "A-1");
			WhsAdjustmentLine adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 75m, "A-2");
			WhsAdjustmentLine adjustmentLine3 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 75m, "A-3");
			WhsAdjustmentLine adjustmentLine4 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, 75m, "A-3");
			AssertEquals(false, HasMessageThatContainParts(adjustmentLine1.LocationStringInfo, expectedWarningMessageParts));
			AssertEquals(true, HasMessageThatContainParts(adjustmentLine2.LocationStringInfo, expectedWarningMessageParts));
			AssertEquals(false, HasMessageThatContainParts(adjustmentLine3.LocationStringInfo, expectedWarningMessageParts));
			AssertEquals(true, HasMessageThatContainParts(adjustmentLine4.LocationStringInfo, expectedWarningMessageParts));

			WhsAdjustmentLine adjustmentLine5 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -40m, "A-3"); // 150 - 40 = 110
			WhsAdjustmentLine adjustmentLine6 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, -40m, "A-3"); // 110 - 40 = 70
			WhsAdjustmentLine adjustmentLine7 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 20m, "A-3"); // 70 + 20 = 90
			AssertEquals(false, HasMessageThatContainParts(adjustmentLine5.LocationStringInfo, expectedWarningMessageParts)); // shouldn't validate lines with WE_TransactionQuantity < 0
			AssertEquals(false, HasMessageThatContainParts(adjustmentLine6.LocationStringInfo, expectedWarningMessageParts));
			AssertEquals(false, HasMessageThatContainParts(adjustmentLine7.LocationStringInfo, expectedWarningMessageParts)); // should take into account lines with negative Units
		}

		#endregion

		#region TestCheckLocationString_CheckLocationForMaxQuantity

		public void TestCheckLocationString_CheckLocationForMaxQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_MaxQuantity = 0m;
			locations[1].WLV_MaxQuantity = 20m;
			locations[2].WLV_MaxQuantity = 20m;
			locations[3].WLV_MaxQuantity = 20m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[0]);
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[1]);
			var line3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[2]);
			var line4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[3]);

			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 20m, locations[0]);
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 20m, locations[1]);
			var adjustmentLine3 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, 20m, locations[2]);
			var adjustmentLine4_1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 6m, locations[3]);
			var adjustmentLine4_2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 6m, locations[3]);
			var expectedMessages = new string[] { "Total required Quantity (", ") exceeds the maximum available Quantity (", ") for this location." };

			AssertEquals(false, HasMessageThatContainParts(adjustmentLine1.LocationStringInfo, expectedMessages, isErrorExpected: true));
			AssertEquals(true, HasMessageThatContainParts(adjustmentLine2.LocationStringInfo, expectedMessages, isErrorExpected: true));
			AssertEquals(true, HasMessageThatContainParts(adjustmentLine3.LocationStringInfo, expectedMessages, isErrorExpected: true));
			AssertEquals("When validation was run there were no problem", false, HasMessageThatContainParts(adjustmentLine4_1.LocationStringInfo, expectedMessages, isErrorExpected: true));
			adjustmentLine4_1.Validation.ValidateLocationString();
			AssertEquals(true, HasMessageThatContainParts(adjustmentLine4_1.LocationStringInfo, expectedMessages, isErrorExpected: true));
			AssertEquals(true, HasMessageThatContainParts(adjustmentLine4_2.LocationStringInfo, expectedMessages, isErrorExpected: true));
		}

		#endregion

		#region TestCheckLocationString_CheckFixLocationMaxProductType

		public void TestCheckLocationString_CheckFixLocationMaxProductType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var maximumNumberOfProducts = 1;
			var fixLocationType = Helper.CreateLocationType("TE1", "Test 1", false, maximumNumberOfProducts, LocationClasses.Codes.FIX);
			var normalLocationType = Helper.CreateLocationType("TE2", "Test 2", false, 0, LocationClasses.Codes.NOR);

			WhsPickFace pickFace1 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, data.Whs1, "A-1");

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var fixedLcoation = locations[0];
			var normalLocation = locations[1];
			fixedLcoation.WLV_WLT_LocationType = fixLocationType.PK;
			normalLocation.WLV_WLT_LocationType = normalLocationType.PK;

			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, fixedLcoation);
			var expectedErrMessages = "This location is a fixed pick face location and '{0}' is not assigned to this location.";
			AssertNoErrors(adjustmentLine1.LocationStringInfo);

			var adjustment2 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment2, data.Part1, 2m, fixedLcoation);
			AssertNoErrors("Adjustment same product does not have error.", adjustmentLine2.LocationStringInfo);

			var adjustment3 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine3 = Helper.CreateWhsAdjustmentLine(adjustment3, data.Part2, 2m, fixedLcoation);
			AssertHasError("When try to put other product to that location it should have an error.", adjustmentLine3.LocationStringInfo, string.Format(expectedErrMessages, data.Part2.OP_PartNum));

			var adjustment4 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine3_1 = Helper.CreateWhsAdjustmentLine(adjustment4, data.Part1, 2m, normalLocation);
			var adjustmentLine3_2 = Helper.CreateWhsAdjustmentLine(adjustment4, data.Part2, 2m, normalLocation);
			AssertNoErrors("No Error for normal location", adjustmentLine3_1.LocationStringInfo);
			AssertNoErrors("No Error for normal location", adjustmentLine3_2.LocationStringInfo);
		}

		#endregion

		#endregion

		#region TestCheckLocationString_CheckLocationIsNotVoid

		public void TestCheckLocationString_DockDoorLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustingOutLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, "");
			var adjustingInLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, "");
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);

			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location
			adjustingOutLine.LocationString = "A-2";
			adjustingInLine.LocationString = "A-2";
			AssertNoErrors(adjustingOutLine.LocationStringInfo);
			AssertHasErrors("You can't adjust into Dock Door Locations.", adjustingInLine.LocationStringInfo);

			adjustingOutLine.LocationString = "A-1";
			adjustingInLine.LocationString = "A-1";
			AssertNoErrors(adjustingOutLine.LocationStringInfo);
			AssertNoErrors(adjustingInLine.LocationStringInfo);
		}

		#endregion

		#region TestCheckLocationString_DynamicLocations

		public void TestCheckLocationString_IsNotDynamicLocationForNonDynamicProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var normalLocationType = Helper.CreateLocationType("NLC", LocationClasses.Codes.NOR);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var normalLocation = locations[0];
			normalLocation.WLV_WLT_LocationType = normalLocationType.PK;

			var dynamicLocation = locations[1];
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			Factory.Save();

			var expectedError = $"Cannot put product {data.Part1.OP_PartNum} in dynamic location {dynamicLocation.ToLocationString()}, as it is not a dynamic product.";

			var adjustment1 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustLine1 = Helper.CreateWhsAdjustmentLine(adjustment1, data.Part1, 1m, dynamicLocation.ToLocationString());
			AssertHasError(adjustLine1.LocationStringInfo, expectedError);

			var adjustment2 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustLine2 = Helper.CreateWhsAdjustmentLine(adjustment2, data.Part1, 1m, normalLocation.ToLocationString());
			AssertNoError("Can adjust in non dynamic area", adjustLine2.LocationStringInfo, expectedError);

			var productParams = WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var adjustment3 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustLine3 = Helper.CreateWhsAdjustmentLine(adjustment3, data.Part1, 1m, dynamicLocation.ToLocationString());
			AssertNoError("Can now adjust dynamic product in dynamic area", adjustLine3.LocationStringInfo, expectedError);
		}

		public void TestCheckLocationString_IsLocatedInCorrectDynamicAreaForDynamicProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var dynamicArea1 = Helper.CreateArea(data.Whs1, "DYNAMIC1", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicArea2 = Helper.CreateArea(data.Whs1, "DYNAMIC2", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var dynamicLocation1 = locations[0];
			dynamicLocation1.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation1.WLV_WA_PickingArea = dynamicArea1.PK;

			var dynamicLocation2 = locations[1];
			dynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicArea2.PK;

			var productParams = WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea1.PK;
			Factory.Save();

			var adjustment1 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustLine1 = Helper.CreateWhsAdjustmentLine(adjustment1, data.Part1, 1m, dynamicLocation2.ToLocationString());
			AssertHasError(adjustLine1.LocationStringInfo, $"Cannot put dynamic product {data.Part1.OP_PartNum} in dynamic location {dynamicLocation2.ToLocationString()}, as the location is not within the product's designated dynamic area ({dynamicArea1.WA_Name}).");

			productParams.W3_WA_DynamicPickFaceArea = dynamicArea2.PK;
			var adjustment2 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustLine2 = Helper.CreateWhsAdjustmentLine(adjustment2, data.Part1, 1m, dynamicLocation2.ToLocationString());
			AssertNoErrors("Can putaway as product is assigned to a dynamic location within the correct area", adjustLine2.LocationStringInfo);
		}

		public void TestCheckLocationString_CanBeNonPickfaceLocationForDynamicProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var normalLocationType = Helper.CreateLocationType("NLC", LocationClasses.Codes.NOR);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var normalLocation = locations[0];
			normalLocation.WLV_WLT_LocationType = normalLocationType.PK;

			var productParams = WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			Factory.Save();

			var adjustment1 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustLine1 = Helper.CreateWhsAdjustmentLine(adjustment1, data.Part1, 1m, normalLocation.ToLocationString());
			AssertNoErrors("Can putaway dynamic product as location is not a pick face", adjustLine1.LocationStringInfo);
		}

		#endregion

		#region TestCheckLocationString_CheckLocationAreaType

		#region TestCheckLocationString_CheckLocationAreaType_BondedAdjustment

		public void TestCheckLocationString_CheckLocationAreaType_BondedAdjustment_AdjustmentIn()
		{
			TestCheckLocationString_CheckLocationAreaType_BondedAdjustmentCore(isAdjustmentOut: false, isErrorExpected: true);
		}

		public void TestCheckLocationString_CheckLocationAreaType_BondedAdjustment_AdjustmentOut()
		{
			TestCheckLocationString_CheckLocationAreaType_BondedAdjustmentCore(isAdjustmentOut: true, isErrorExpected: false);
		}

		void TestCheckLocationString_CheckLocationAreaType_BondedAdjustmentCore(bool isAdjustmentOut, bool isErrorExpected)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var freeStoreArea = Helper.CreateArea(data.Whs1, "FREE", AreaTypes.Codes.FreeStore);
			var freeStoreLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "freestore", 1, 1).Locations[0];
			freeStoreLocation.WLV_WA_PickingArea = freeStoreArea.PK;

			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			var bondedLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "bonded", 1, 1).Locations[0];
			bondedLocation.WLV_WA_PickingArea = bondedArea.PK;

			var exciseArea = Helper.CreateArea(data.Whs1, "EXCArea", AreaTypes.Codes.Excise);
			var exciseLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "AA", 1, 1).Locations[0];
			exciseLocation.WLV_WA_PickingArea = exciseArea.PK;

			var dynamicPickFaceArea = Helper.CreateArea(data.Whs1, "DPFArea", AreaTypes.Codes.DynamicPickFace, isPutawayArea: false);
			var dynamicPickFaceLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "BB", 1, 1).Locations[0];
			dynamicPickFaceLocation.WLV_WA_PickingArea = dynamicPickFaceArea.PK;

			var packingStationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingArea = Helper.CreateArea(data.Whs1, "PACK", AreaTypes.Codes.FreeStore, isPutawayArea: false);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "CC", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			packingLocation.WLV_WA_PickingArea = packingArea.PK;

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Consolidation", false, 0, LocationClasses.Codes.CON);
			var packingConsolidationArea = Helper.CreateArea(data.Whs1, "CON", AreaTypes.Codes.FreeStore, isPutawayArea: false);
			var packingConsolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "DD", 1, 1).Locations[0];
			packingConsolidationLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;
			packingConsolidationLocation.WLV_WA_PickingArea = packingConsolidationArea.PK;
			Factory.Save();

			var expectedMessage = "A Bonded Adjustment can only adjust into locations that has a Bonded Pick Area.";

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, isAdjustmentOut ? -10 : 10, freeStoreLocation);

			AssertEquals("Precondition", true, adjustment.IsCustomsTransaction);
			AssertErrorOnLocationString(adjustmentLine, freeStoreLocation, expectedMessage, isErrorExpected);
			AssertErrorOnLocationString(adjustmentLine, bondedLocation, expectedMessage, false);
			AssertErrorOnLocationString(adjustmentLine, exciseLocation, expectedMessage, isErrorExpected);
			AssertErrorOnLocationString(adjustmentLine, data.Whs1.DefaultOutboundDockDoorLocation, expectedMessage: "You cannot adjust into Dock Door Locations.", isErrorExpected);
			AssertErrorOnLocationString(adjustmentLine, dynamicPickFaceLocation, expectedMessage, isErrorExpected);
			AssertErrorOnLocationString(adjustmentLine, packingConsolidationLocation, expectedMessage: "You cannot adjust into Packing Consolidation Locations.", isErrorExpected);
			AssertErrorOnLocationString(adjustmentLine, packingLocation, expectedMessage: "You cannot adjust into Packing Station Locations.", isErrorExpected);
		}

		#endregion

		#region TestCheckLocationString_CheckLocationAreaType_NonBondedAdjustment

		public void TestCheckLocationString_CheckLocationAreaType_NonBondedAdjustment_AdjustmentIn()
		{
			TestCheckLocationString_CheckLocationAreaType_NonBondedAdjustmentCore(isAdjustmentOut: false, isErrorExpected: true);
		}

		public void TestCheckLocationString_CheckLocationAreaType_NonBondedAdjustment_AdjustmentOut()
		{
			TestCheckLocationString_CheckLocationAreaType_NonBondedAdjustmentCore(isAdjustmentOut: true, isErrorExpected: false);
		}

		public void TestCheckLocationString_CheckLocationAreaType_NonBondedAdjustmentCore(bool isAdjustmentOut, bool isErrorExpected)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var freeStoreArea = Helper.CreateArea(data.Whs1, "FREE", AreaTypes.Codes.FreeStore);
			var freeStoreLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "freestore", 1, 1).Locations[0];
			freeStoreLocation.WLV_WA_PickingArea = freeStoreArea.PK;

			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			var bondedLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "bonded", 1, 1).Locations[0];
			bondedLocation.WLV_WA_PickingArea = bondedArea.PK;

			var exciseArea = Helper.CreateArea(data.Whs1, "EXCArea", AreaTypes.Codes.Excise);
			var exciseLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "AA", 1, 1).Locations[0];
			exciseLocation.WLV_WA_PickingArea = exciseArea.PK;

			var dynamicPickFaceArea = Helper.CreateArea(data.Whs1, "DPFArea", AreaTypes.Codes.DynamicPickFace, isPutawayArea: false);
			var dynamicPickFaceLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "BB", 1, 1).Locations[0];
			dynamicPickFaceLocation.WLV_WA_PickingArea = dynamicPickFaceArea.PK;

			var packingStationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingArea = Helper.CreateArea(data.Whs1, "PACK", AreaTypes.Codes.FreeStore, isPutawayArea: false);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "CC", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			packingLocation.WLV_WA_PickingArea = packingArea.PK;

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Consolidation", false, 0, LocationClasses.Codes.CON);
			var packingConsolidationArea = Helper.CreateArea(data.Whs1, "CON", AreaTypes.Codes.FreeStore, isPutawayArea: false);
			var packingConsolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "DD", 1, 1).Locations[0];
			packingConsolidationLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;
			packingConsolidationLocation.WLV_WA_PickingArea = packingConsolidationArea.PK;
			Factory.Save();

			var expectedMessage = "A Non-Bonded Adjustment can only adjust into locations that does not have a Bonded Pick Area.";

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Adjustment;
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, isAdjustmentOut ? -10 : 10, bondedLocation);

			AssertEquals("Precondition", false, adjustment.IsCustomsTransaction);
			AssertErrorOnLocationString(adjustmentLine, freeStoreLocation, expectedMessage, false);
			AssertErrorOnLocationString(adjustmentLine, bondedLocation, expectedMessage, isErrorExpected);
			AssertErrorOnLocationString(adjustmentLine, exciseLocation, expectedMessage, false);
			AssertErrorOnLocationString(adjustmentLine, data.Whs1.DefaultOutboundDockDoorLocation, expectedMessage: "You cannot adjust into Dock Door Locations.", isErrorExpected);
			AssertErrorOnLocationString(adjustmentLine, dynamicPickFaceLocation, expectedMessage, false);
			AssertErrorOnLocationString(adjustmentLine, packingConsolidationLocation, expectedMessage: "You cannot adjust into Packing Consolidation Locations.", isErrorExpected);
			AssertErrorOnLocationString(adjustmentLine, packingLocation, expectedMessage: "You cannot adjust into Packing Station Locations.", isErrorExpected);
		}

		#endregion

		#region TestCheckLocationString_CheckLocationAreaType_InwardProcessingArea

		public void TestCheckLocationString_CheckLocationAreaType_InwardProcessingArea_AdjustmentIn()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var inwardProcessingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR", 1, 1).Locations[0];
			inwardProcessingLocation.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			inwardProcessingLocation.WLV_WA_PickingArea = inwardProcessingArea.PK;

			var freeStoreArea = Helper.CreateArea(data.Whs1, "FREE", AreaTypes.Codes.FreeStore);
			var freeStoreLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "freestore", 1, 1).Locations[0];
			freeStoreLocation.WLV_WA_PutawayArea = freeStoreArea.PK;
			freeStoreLocation.WLV_WA_PickingArea = freeStoreArea.PK;
			Factory.Save();

			const string expectedMessage = "Please enter a valid location, the Location you have entered is in an Inward Processing area.";

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10, inwardProcessingLocation);
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10, freeStoreLocation);

			AssertHasError(adjustmentLine1.LocationStringInfo, expectedMessage);
			AssertNoErrors(adjustmentLine2.LocationStringInfo);
		}

		public void TestCheckLocationString_CheckLocationAreaType_InwardProcessingArea_AdjustmentOut()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var inwardProcessingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR", 1, 1).Locations[0];
			inwardProcessingLocation.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			inwardProcessingLocation.WLV_WA_PickingArea = inwardProcessingArea.PK;

			var freeStoreArea = Helper.CreateArea(data.Whs1, "FREE", AreaTypes.Codes.FreeStore);
			var freeStoreLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "freestore", 1, 1).Locations[0];
			freeStoreLocation.WLV_WA_PutawayArea = freeStoreArea.PK;
			freeStoreLocation.WLV_WA_PickingArea = freeStoreArea.PK;
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10, inwardProcessingLocation);
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10, freeStoreLocation);

			AssertNoErrors(adjustmentLine1.LocationStringInfo);
			AssertNoErrors(adjustmentLine2.LocationStringInfo);
		}

		#endregion

		public void TestCheckLocationString_CheckLocationAreaType_LocationWithDifferentPickingAndPutawayAreaTypes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var freeStoreArea = Helper.CreateArea(data.Whs1, "FREE", AreaTypes.Codes.FreeStore);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			var locationWithDifferentAreaTypes = Helper.CreateRowAndGenerateLocations(data.Whs1, "X", 1, 1).Locations[0];
			locationWithDifferentAreaTypes.WLV_WA_PickingArea = freeStoreArea.PK;
			locationWithDifferentAreaTypes.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10, locationWithDifferentAreaTypes);

			AssertEquals("Location area type is based on location's picking area.", AreaTypes.Codes.FreeStore, adjustmentLine.LocationAreaType);
			AssertErrorOnLocationString(adjustmentLine, locationWithDifferentAreaTypes, "A Bonded Adjustment can only adjust into locations that has a Bonded Pick Area.", isErrorExpected: true);
		}

		void AssertErrorOnLocationString(WhsAdjustmentLine adjustmentLine, WhsLocation location, string expectedMessage, bool isErrorExpected)
		{
			adjustmentLine.LocationString = location.ToLocationString();
			if (isErrorExpected)
			{
				AssertHasError(adjustmentLine.LocationStringInfo, expectedMessage);
			}
			else
			{
				AssertNoErrors(adjustmentLine.LocationStringInfo);
			}
		}

		#endregion

		#endregion

		#region TestCheckWE_PalletID

		#region TestCheckWE_PalletID

		protected override void TestCheckWE_WE_PalletIDCore(TestDataSimpleEnvironment data)
		{
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			inventory.WI_PalletID = "PLT1";
			inventory.WI_WL = locations[0].PK;
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, locations[0]);
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, locations[0]);
			var adjustmentLine3 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, locations[1]);
			adjustmentLine1.WE_PalletID = "PLT1";
			adjustmentLine2.WE_PalletID = "PLT1";
			adjustmentLine3.WE_PalletID = "PLT1";
			adjustment.FinaliseDocket();

			AssertNoErrors("It's allowed to adjust units out of the inventory. No errors on Pallet ID expected.", adjustmentLine1.WE_PalletIDInfo);
			AssertHasError(adjustmentLine2.WE_PalletIDInfo, "Another location was already used for the same Pallet ID on this Adjustment.");
			AssertHasError(adjustmentLine3.WE_PalletIDInfo, string.Format("Another location ({0}) was already used for the same Pallet ID. Please select another location or Pallet ID.", locations[0].ToLocationString()));
		}

		public void TestCheckWE_PalletID_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			inventory.WI_PalletID = "PLT1";
			inventory.WI_WL = locations[0].PK;
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			AssertEquals("Precondition: Stock On Hand.", 10m, inventory.WI_TotalUnits);

			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_PalletID = "PLT1";
			AssertEquals("Precondition: No Stock On Hand.", 0m, inventory.WI_TotalUnits);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, locations[1]);
			adjustmentLine.WE_PalletID = "PLT1";
			adjustment.FinaliseDocket();
			AssertHasError(adjustmentLine.WE_PalletIDInfo, "This pallet is currently In-Transit and cannot be adjusted. Please select another Pallet ID.");

			transferLine.FinaliseDocketLine();
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			adjustmentLine.Validation.ValidateWE_PalletID();
			AssertNoErrors("No stock on hand or in transit in another location for this Pallet ID.", adjustmentLine.WE_PalletIDInfo);
		}

		public void TestCheckWE_PalletID_PuttingAway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1, "PLT1", 15m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, nonDockDoorLocation, "PLT1", 15m);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();

			AssertEquals($"Precondition: InventoryStatus of Transfer Line should be {InventoryStatus.Codes.PuttingAway}.", InventoryStatus.Codes.PuttingAway, transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, nonDockDoorLocation.ToLocationString(), "PLT1");
			AssertHasError(adjustmentLine.WE_PalletIDInfo, "This pallet is currently In-Transit and cannot be adjusted. Please select another Pallet ID.");

			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);
			Factory.Save();

			adjustmentLine.Validation.ValidateWE_PalletID();
			AssertNoErrors("No stock on hand or in transit in another location for this Pallet ID.", adjustmentLine.WE_PalletIDInfo);
		}

		public void TestCheckWE_PalletID_SamePalletIDDifferentWarehouses()
		{
			// locations across 2 warehouses
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("W2", "B", 2, 1);
			var org2 = Helper.CreateClient("Org2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);

			string palletId1 = "PL1";

			// receive into warehouse 1
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation, palletId1, false, true);

			// second receive into warehouse 1 but for different client
			var receive2 = Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", data.Part1, 10m, data.Whs1.DefaultLocation, "", false, true);

			Factory.Save();

			var expectedErrorMessage = "Another client ({0}) already uses the same Pallet ID. Please select Pallet ID.";

			// Same client in the same warehouse
			var adjustment1 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment1, data.Part1, 1m, data.Whs1.DefaultLocation);
			adjustmentLine1.WE_PalletID = palletId1;
			AssertNoErrors(string.Format(expectedErrorMessage, data.Org1.OH_Code), adjustmentLine1.WE_PalletIDInfo);

			// Different Client in same warehouse
			var adjustment2 = Helper.CreateWhsAdjustment(org2, data.Whs1);
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment2, data.Part1, 1m, data.Whs1.DefaultLocation);
			adjustmentLine2.WE_PalletID = palletId1;
			AssertNoErrors(string.Format(expectedErrorMessage, org2.OH_Code), adjustmentLine2.WE_PalletIDInfo);

			// Same client in different warehouse
			var adjustment3 = Helper.CreateWhsAdjustment(data.Org1, whs2);
			var adjustmentLine3 = Helper.CreateWhsAdjustmentLine(adjustment3, data.Part1, 1m, whs2.DefaultLocation);
			adjustmentLine3.WE_PalletID = palletId1;
			AssertNoErrors(string.Format(expectedErrorMessage, data.Org1.OH_Code), adjustmentLine3.WE_PalletIDInfo);

			// Different Client in different warehouse
			var adjustment4 = Helper.CreateWhsAdjustment(org2, whs2);
			var adjustmentLine4 = Helper.CreateWhsAdjustmentLine(adjustment4, data.Part1, 1m, whs2.DefaultLocation);
			adjustmentLine4.WE_PalletID = palletId1;
			AssertNoErrors(string.Format(expectedErrorMessage, org2.OH_Code), adjustmentLine4.WE_PalletIDInfo);
		}

		#endregion

		#region TestCheckWE_PalletID_NoErrorsWhenInventoryTransferred

		public void TestCheckWE_PalletID_NoErrorsWhenInventoryTransferred()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ1", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, "A-1");
			adjustmentLine.WE_PalletID = "PLT-1";
			adjustment.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment);

			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine.WE_TransferFromPalletId = "PLT-1";
			transferLine.WE_PalletID = "PLT-1";
			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var adjustmentInOtherFactory = otherFactory.Load<WhsAdjustment>(adjustment.PK);
			adjustmentInOtherFactory.Lines[0].Validation.ValidateWE_PalletID();
			AssertEquals("Should have no errors on saving previously finalised and saved Adjustment.", false, adjustmentInOtherFactory.Lines[0].WE_PalletIDInfo.HasNotifications());
		}

		#endregion

		#region TestCheckWE_PalletID_WithSiblingPartiallyAdjustingOutExistingPalletID

		public void TestCheckWE_PalletID_WithSiblingPartiallyAdjustingOutExistingPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive", data.Part1, 2m, locationA, "PLT123");
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustLineOut = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, locationA.WLV_LocationString, "PLT123");
			var adjustLineIn = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, locationB.WLV_LocationString, "PLT123");
			adjustment.FinaliseDocket();

			CombineAssertions(() =>
			{
				AssertEquals("Adjustment must *not* be finalized", false, adjustment.IsFinalised);
				AssertNoErrors("There should be no errors on the Adjustment Out.", adjustLineOut.WE_PalletIDInfo);
				AssertHasError(adjustLineIn.WE_PalletIDInfo, string.Format("Another location ({0}) was already used for the same Pallet ID. Please select another location or Pallet ID.", locationA.WLV_LocationString));
			});
		}

		#endregion

		#region TestCheckWE_PalletID_WithSiblingPartiallyAdjustingOutExistingPalletID_AdjustOutAll_Using1Line

		public void TestCheckWE_PalletID_WithSiblingPartiallyAdjustingOutExistingPalletID_AdjustOutAll_Using1Line()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 2m, locationA, "PLT123");
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustLineOut = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -2m, locationA.WLV_LocationString, "PLT123");
			var adjustLineIn = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, locationB.WLV_LocationString, "PLT123");
			adjustment.FinaliseDocket();

			CombineAssertions(() =>
			{
				AssertEquals("Adjustment must *not* be finalized", true, adjustment.IsFinalised);
				AssertNoErrors("There should be no errors on the Adjustment Out.", adjustLineOut.WE_PalletIDInfo);
				AssertNoErrors(string.Format("Another location ({0}) was already used for the same Pallet ID. Please select another location or Pallet ID.", locationA.WLV_LocationString), adjustLineIn.WE_PalletIDInfo);
			});
		}

		#endregion

		#region TestCheckWE_PalletID_WithSiblingPartiallyAdjustingOutExistingPalletID_AdjustOutAll_Using2Lines

		public void TestCheckWE_PalletID_WithSiblingPartiallyAdjustingOutExistingPalletID_AdjustOutAll_Using2Lines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 2m, locationA, "PLT123");
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustLineOut1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, locationA.WLV_LocationString, "PLT123");
			var adjustLineOut2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, locationA.WLV_LocationString, "PLT123");
			var adjustLineIn = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, locationB.WLV_LocationString, "PLT123");
			adjustment.FinaliseDocket();

			CombineAssertions(() =>
			{
				AssertEquals("Adjustment must *not* be finalized", true, adjustment.IsFinalised);
				AssertNoErrors("There should be no errors on the Adjustment Out A.", adjustLineOut1.WE_PalletIDInfo);
				AssertNoErrors("There should be no errors on the Adjustment Out B.", adjustLineOut2.WE_PalletIDInfo);
				AssertNoErrors(string.Format("Another location ({0}) was already used for the same Pallet ID. Please select another location or Pallet ID.", locationA.WLV_LocationString), adjustLineIn.WE_PalletIDInfo);
			});
		}

		#endregion

		#region TestCheckWE_PalletID_WithSiblingPartiallyAdjustingOutExistingPalletID_AdjustOutAll_Using2Adjustments

		public void TestCheckWE_PalletID_WithSiblingPartiallyAdjustingOutExistingPalletID_AdjustOutAll_Using2Adjustments()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 2m, locationA, "PLT123");

			var adjustment1 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustLineOut = Helper.CreateWhsAdjustmentLine(adjustment1, data.Part1, -2m, locationA.WLV_LocationString, "PLT123");
			adjustment1.FinaliseDocket();
			Factory.Save();

			var adjustment2 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustLineIn = Helper.CreateWhsAdjustmentLine(adjustment2, data.Part1, 1m, locationB.WLV_LocationString, "PLT123");
			adjustment2.FinaliseDocket();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Adjustment1 must *not* be finalized", true, adjustment1.IsFinalised);
				AssertEquals("Adjustment2 must *not* be finalized", true, adjustment2.IsFinalised);
				AssertNoErrors("There should be no errors on the Adjustment Out.", adjustLineOut.WE_PalletIDInfo);
				AssertNoErrors(string.Format("Another location ({0}) was already used for the same Pallet ID. Please select another location or Pallet ID.", locationA.WLV_LocationString), adjustLineIn.WE_PalletIDInfo);
			});
		}

		#endregion

		#region TestCheckWE_PalletID_WithSiblingPartiallyAdjustingOutExistingPalletID_LargeNumbers

		public void TestCheckWE_PalletID_WithSiblingPartiallyAdjustingOutExistingPalletID_LargeNumbers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive", data.Part1, 100m, locationA, "PLT123");
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustLineOut1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, locationA.WLV_LocationString, "PLT123");
			var adjustLineOut2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -20m, locationA.WLV_LocationString, "PLT123");
			var adjustLineOut3 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -30m, locationA.WLV_LocationString, "PLT123");
			var adjustLineIn1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, locationA.WLV_LocationString, "PLT123");
			var adjustLineIn2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 20m, locationB.WLV_LocationString, "PLT123");
			adjustment.FinaliseDocket();

			CombineAssertions(() =>
			{
				AssertEquals("Adjustment must *not* be finalized", false, adjustment.IsFinalised);
				AssertNoErrors("There should be no errors on the Adjustment Out 1.", adjustLineOut1.WE_PalletIDInfo);
				AssertNoErrors("There should be no errors on the Adjustment Out 2.", adjustLineOut2.WE_PalletIDInfo);
				AssertNoErrors("There should be no errors on the Adjustment Out 3.", adjustLineOut3.WE_PalletIDInfo);
				AssertHasError(adjustLineIn1.WE_PalletIDInfo, "Another location was already used for the same Pallet ID on this Adjustment.");
				AssertHasError(adjustLineIn2.WE_PalletIDInfo, string.Format("Another location ({0}) was already used for the same Pallet ID. Please select another location or Pallet ID.", locationA.WLV_LocationString));
			});
		}

		#endregion

		#region TestCheckWE_PalletID_WithSiblingPartiallyAdjustingOutExistingPalletID_LargeNumbers_AdjustOutAll

		public void TestCheckWE_PalletID_WithSiblingPartiallyAdjustingOutExistingPalletID_LargeNumbers_AdjustOutAll()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive", data.Part1, 100m, locationA, "PLT123");
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustLineOut1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, locationA.WLV_LocationString, "PLT123");
			var adjustLineOut2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -20m, locationA.WLV_LocationString, "PLT123");
			var adjustLineOut3 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -70m, locationA.WLV_LocationString, "PLT123");
			var adjustLineIn1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, locationB.WLV_LocationString, "PLT123");
			var adjustLineIn2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 20m, locationB.WLV_LocationString, "PLT123");
			adjustment.FinaliseDocket();

			CombineAssertions(() =>
			{
				AssertEquals("Adjustment must *not* be finalized", true, adjustment.IsFinalised);
				AssertNoErrors("There should be no errors on the Adjustment Out 1.", adjustLineOut1.WE_PalletIDInfo);
				AssertNoErrors("There should be no errors on the Adjustment Out 2.", adjustLineOut2.WE_PalletIDInfo);
				AssertNoErrors("There should be no errors on the Adjustment Out 3.", adjustLineOut3.WE_PalletIDInfo);
				AssertNoErrors("There should be no errors on the Adjustment In 1.", adjustLineIn1.WE_PalletIDInfo);
				AssertNoErrors("There should be no errors on the Adjustment In 2.", adjustLineIn2.WE_PalletIDInfo);
			});
		}

		#endregion

		#region TestCheckWE_PalletID_WithSiblingPartiallyAdjustingOutExistingPalletID_LargeNumbers_AdjustOutAll_IntoDifferentLocations_DifferentPalletID

		public void TestCheckWE_PalletID_WithSiblingPartiallyAdjustingOutExistingPalletID_LargeNumbers_AdjustOutAll_IntoDifferentLocations_DifferentPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 3);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var locationC = data.Whs1.FindLocation("A-3");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive", data.Part1, 100m, locationA, "PLT123");
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustLineOut1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, locationA.WLV_LocationString, "PLT123");
			var adjustLineOut2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -20m, locationA.WLV_LocationString, "PLT123");
			var adjustLineOut3 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -70m, locationA.WLV_LocationString, "PLT123");
			var adjustLineIn1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, locationB.WLV_LocationString, "PLT124");
			var adjustLineIn2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 20m, locationC.WLV_LocationString, "PLT125");
			adjustment.FinaliseDocket();

			CombineAssertions(() =>
			{
				AssertEquals("Adjustment must *not* be finalized", true, adjustment.IsFinalised);
				AssertNoErrors("There should be no errors on the Adjustment Out 1.", adjustLineOut1.WE_PalletIDInfo);
				AssertNoErrors("There should be no errors on the Adjustment Out 2.", adjustLineOut2.WE_PalletIDInfo);
				AssertNoErrors("There should be no errors on the Adjustment Out 3.", adjustLineOut3.WE_PalletIDInfo);
				AssertNoErrors("There should be no errors on the Adjustment In 1.", adjustLineIn1.WE_PalletIDInfo);
				AssertNoErrors("There should be no errors on the Adjustment In 2.", adjustLineIn2.WE_PalletIDInfo);
			});
		}

		#endregion

		#region TestCheckWE_PalletID_WithSiblingPartiallyAdjustingOutExistingPalletID_LargeNumbers_AdjustOutAll_IntoOriginalLocation

		public void TestCheckWE_PalletID_WithSiblingPartiallyAdjustingOutExistingPalletID_LargeNumbers_AdjustOutAll_IntoOriginalLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive", data.Part1, 100m, locationA, "PLT123");
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustLineOut1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, locationA.WLV_LocationString, "PLT123");
			var adjustLineOut2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -20m, locationA.WLV_LocationString, "PLT123");
			var adjustLineOut3 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -70m, locationA.WLV_LocationString, "PLT123");
			var adjustLineIn1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, locationA.WLV_LocationString, "PLT123");
			var adjustLineIn2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 20m, locationA.WLV_LocationString, "PLT123");
			adjustment.FinaliseDocket();

			CombineAssertions(() =>
			{
				AssertEquals("Adjustment must *not* be finalized", true, adjustment.IsFinalised);
				AssertNoErrors("There should be no errors on the Adjustment Out 1.", adjustLineOut1.WE_PalletIDInfo);
				AssertNoErrors("There should be no errors on the Adjustment Out 2.", adjustLineOut2.WE_PalletIDInfo);
				AssertNoErrors("There should be no errors on the Adjustment Out 3.", adjustLineOut3.WE_PalletIDInfo);
				AssertNoErrors("There should be no errors on the Adjustment In 1.", adjustLineIn1.WE_PalletIDInfo);
				AssertNoErrors("There should be no errors on the Adjustment In 2.", adjustLineIn2.WE_PalletIDInfo);
			});
		}

		#endregion

		#region TestCheckWE_PalletID_WithSiblingPartiallyAdjustingOutExistingPalletID_DifferentLocations

		public void TestCheckWE_PalletID_WithSiblingPartiallyAdjustingOutExistingPalletID_DifferentLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 3);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var locationC = data.Whs1.FindLocation("A-3");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive", data.Part1, 20m, locationA, "PLT123");
			Factory.Save();

			AssertIsFinalisedPrecondition(receive);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLineOut1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -20m, locationA.WLV_LocationString, "PLT123");
			var adjustmentLineIn1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, locationB.WLV_LocationString, "PLT123");
			var adjustmentLineIn2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, locationC.WLV_LocationString, "PLT123");
			adjustment.FinaliseDocket();

			CombineAssertions(() =>
			{
				AssertEquals("Adjustment must *not* be finalized", false, adjustment.IsFinalised);
				AssertNoErrors("There should be no errors on the Adjustment Out 1.", adjustmentLineOut1.WE_PalletIDInfo);
				AssertHasError(adjustmentLineIn1.WE_PalletIDInfo, "Another location was already used for the same Pallet ID on this Adjustment.");
				AssertHasError(adjustmentLineIn2.WE_PalletIDInfo, "Another location was already used for the same Pallet ID on this Adjustment.");
			});
		}

		#endregion

		#region TestCheckWE_PalletID_NoErrorsWhenReUsePalletID

		public void TestCheckWE_PalletID_NoErrorsWhenReUsePalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			WhsAdjustment adjustment1 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ1", Notify);
			WhsAdjustmentLine adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment1, data.Part1, 10m, "A-1");
			adjustmentLine1.WE_PalletID = "PLT-1";
			adjustment1.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment1);

			Factory.Save();

			WhsTransfer transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			WhsTransferLine transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine.WE_TransferFromPalletId = "PLT-1";
			transferLine.WE_PalletID = "PLT-1";
			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);

			Factory.Save();

			WhsAdjustment adjustment2 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ1", Notify);
			WhsAdjustmentLine adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment2, data.Part1, 10m, "A-1");
			adjustmentLine2.WE_PalletID = "PLT-1";
			AssertEquals(true, adjustmentLine2.WE_PalletIDInfo.HasNotifications());
		}

		#endregion

		#region TestCheckWE_PalletID_NewSamePalletIDDifferentLocation

		public void TestCheckWE_PalletID_NewSamePalletIDDifferentLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ", Notify);
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, locationA.WLV_LocationString, "PLT123");
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, ZString.Empty, "PLT123");
			adjustment.RunPreSaveValidation();

			AssertNoError(adjustmentLine1.WE_PalletIDInfo, "Another location was already used for the same Pallet ID on this Adjustment.");
			AssertNoError(adjustmentLine2.WE_PalletIDInfo, "Another location was already used for the same Pallet ID on this Adjustment.");

			adjustmentLine2.WE_WL = locationB.PK;
			adjustmentLine2.WE_PalletID = "PLT123";
			Factory.Save();

			AssertNoError(adjustmentLine1.WE_PalletIDInfo, "Another location was already used for the same Pallet ID on this Adjustment.");
			AssertHasError(adjustmentLine2.WE_PalletIDInfo, "Another location was already used for the same Pallet ID on this Adjustment.");
		}

		#endregion

		#region TestCheckWE_PalletID_NewSamePalletIDDifferentLocation_Staged

		public void TestCheckWE_PalletID_NewSamePalletIDDifferentLocation_Staged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, data.Whs1.FindLocation("A"), "PLT-123");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order1);

			var pickLine = order1.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_PalletID = "PLT-123";
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, "A", "PLT-123");
			adjustment.RunPreSaveValidation();
			AssertHasError("Should have Pallet ID error as the ID exists in the dock door.", adjustmentLine.WE_PalletIDInfo,
				"Another location (DOCKDOOR) was already used for the same Pallet ID. Please select another location or Pallet ID.");

			adjustmentLine.WE_WL = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			adjustment.RunPreSaveValidation();
			AssertNoError("Should have no PalletID error.", adjustmentLine.WE_PalletIDInfo,
				"Another location (DOCKDOOR) was already used for the same Pallet ID. Please select another location or Pallet ID.");
		}

		#endregion

		#region TestCheckWE_PalletID_WithPalletUsedInAnotherLocationAndThatInventoryBeingAdjustedOut

		public void TestCheckWE_PalletID_WithPalletUsedInAnotherLocationAndThatInventoryBeingAdjustedOut()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			inventory.WI_PalletID = "PLT-1";
			inventory.WI_WL = locations[0].PK;
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ1", Notify);
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, locations[1].PK);
			adjustmentLine1.WE_PalletID = "PLT-1";
			adjustment.RunPreSaveValidation();
			AssertHasError("PLT-1 exists on inventory in a different location", adjustmentLine1.WE_PalletIDInfo,
				string.Format("Another location ({0}) was already used for the same Pallet ID. Please select another location or Pallet ID.", locations[0].ToLocationString()));

			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, locations[0].PK);
			adjustmentLine2.WE_PalletID = "PLT-1";

			adjustmentLine1.WE_PalletID = "PLT-1";
			adjustment.RunPreSaveValidation();
			AssertEquals("Existing inventory on PLT-1 was adjusted out of Location 0, New inventory in Location 1 should now be ok.",
				false, adjustmentLine1.WE_PalletIDInfo.HasNotifications());
		}

		#endregion

		#region TestCheckWE_PalletID_WithInventoryInTransit

		public void TestCheckWE_PalletID_PreventAdjustmentToInTransitPalletDestinationLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");
			var expectedErrorMsg = "This pallet is currently In-Transit and cannot be adjusted. Please select another Pallet ID.";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT123");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
			var transferLine1 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 100m, locationA, "PLT123", locationB, "PLT123", picker);
			Factory.Save();

			AssertNoErrors("Precondition: No errors expected in transfer.", transferLine1.WE_PalletIDInfo);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, locationB.WLV_LocationString, "PLT123");
			adjustment.FinaliseDocket();

			AssertEquals("Adjustment must NOT be finalized", false, adjustment.IsFinalised);
			AssertHasError("Error is expected since pallet ID is already being used.", adjustLine.WE_PalletIDInfo, string.Format(expectedErrorMsg, locationA.WLV_LocationString));

			adjustLine.WE_WL = locationA.PK;
			adjustment.FinaliseDocket();

			AssertEquals("Adjustment must NOT be finalized", false, adjustment.IsFinalised);
			AssertHasError("Error is expected since pallet ID is already being used.", adjustLine.WE_PalletIDInfo, string.Format(expectedErrorMsg, locationB.WLV_LocationString));
		}

		public void TestCheckWE_PalletID_OnlyPreventsAdjustingInTransitPalletIDsWithinTheSameWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var whs2 = Helper.CreateWarehouse("W2", "W2", "A");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT123");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
			var transferLine1 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 100m, locationA, "PLT123", locationB, "PLT123", picker);
			Factory.Save();
			AssertNoErrors("Precondition: No errors expected in transfer.", transferLine1.WE_PalletIDInfo);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, whs2);
			var adjustLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, whs2.DefaultLocation.WLV_LocationString, "PLT123");
			adjustment.FinaliseDocket();

			CombineAssertions(() =>
			{
				AssertEquals("Adjustment must be finalized", true, adjustment.IsFinalised);
				AssertNoErrors("Should not show an error as the pallet ID is in another warehouse.", adjustLine.WE_PalletIDInfo);
			});
		}

		public void TestCheckWE_PalletID_OnlyPreventsAdjustingInTransitPalletIDsWithinTheSameWarehouse_InterWhs_Source()
		{
			TestCheckWE_PalletID_OnlyPreventsAdjustingInTransitPalletIDsWithinTheSameWarehouse_InterWhs(isSource: true);
		}

		public void TestCheckWE_PalletID_OnlyPreventsAdjustingInTransitPalletIDsWithinTheSameWarehouse_InterWhs_Dest()
		{
			TestCheckWE_PalletID_OnlyPreventsAdjustingInTransitPalletIDsWithinTheSameWarehouse_InterWhs(isSource: false);
		}

		void TestCheckWE_PalletID_OnlyPreventsAdjustingInTransitPalletIDsWithinTheSameWarehouse_InterWhs(bool isSource)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var whs2 = Helper.CreateWarehouse("W2", "A", 2, 2);
			var picker = Helper.CreateGlbStaff("RSL", "Russell");
			var expectedErrorMsg = "This pallet is currently In-Transit and cannot be adjusted. Please select another Pallet ID.";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT123");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, isSource ? data.Whs1 : whs2, "TR1", Notify);
			transfer.WD_DocketSubType = isSource ? TransferType.Codes.InterWhsSource : TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 100m, locationA.WLV_LocationString, "PLT123", isSource ? whs2.PK : data.Whs1.PK, whs2.DefaultLocation.WLV_LocationString, "PLT123", receive.Lines[0].WE_AdjustmentArrivalDate);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertNoErrors("Precondition: No errors expected in transfer.", transferLine.WE_PalletIDInfo);
			AssertEquals("Precondition: Created In-Transit inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			var adjustmentWhs1 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustWhs1Line = Helper.CreateWhsAdjustmentLine(adjustmentWhs1, data.Part1, 2m, "A-2", palletID: "PLT123");
			adjustmentWhs1.FinaliseDocket();
			CombineAssertions(() =>
			{
				AssertEquals("Adjustment must *not* be finalized", false, adjustmentWhs1.IsFinalised);
				AssertHasError("Should show an error.", adjustWhs1Line.WE_PalletIDInfo, expectedErrorMsg);
			});

			var adjustmentWhs2 = Helper.CreateWhsAdjustment(data.Org1, whs2, "A2");
			var adjustWhs2Line = Helper.CreateWhsAdjustmentLine(adjustmentWhs2, data.Part1, 2m, "A-2", palletID: "PLT123");
			adjustmentWhs2.FinaliseDocket();

			CombineAssertions(() =>
			{
				AssertEquals("Adjustment must *not* be finalized", false, adjustmentWhs2.IsFinalised);
				AssertHasError("Should show an error.", adjustWhs2Line.WE_PalletIDInfo, expectedErrorMsg);
			});
		}

		public void TestCheckWE_PalletID_AdjustInventoryAndMakeInTransit()
		{
			// Verify there's no errors after adjusting in and adding a transfer with inventory in transit from adjusted location and pallet ID.
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT123");
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 80m, locationA.WLV_LocationString, "PLT123");
			adjustment.FinaliseDocket();
			AssertEquals("Adjustment must be finalized", true, adjustment.IsFinalised);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
			var transferLine = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 100m, locationA, "PLT123", locationB, "PLT123", picker);
			Factory.Save();

			adjustment.RunPreSaveValidation();
			AssertNoErrors("Adjustment must NOT have any errors.", adjustLine.WE_PalletIDInfo);
		}

		#endregion

		#region TestCheckWE_PalletID_ValidateAdjustIntoPickFaceLocation

		public void TestCheckWE_PalletID_ValidateAdjustIntoPickFaceLocation_WithPalletIdErrorIsShown()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var product = WhsProduct.GetWhsProduct(data.Part1);
			var pickFace = product.PickFaces.AddNew();
			var pickFaceLocation = locations[0];
			pickFace.WF_WL = pickFaceLocation.PK;
			var normalLocation = locations[1];

			pickFaceLocation.LocationType.WLT_RetainPalletIDsInFixedPickFaces = false;

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 50m, normalLocation);
			adjustmentLine.WE_WL = pickFaceLocation.PK;
			adjustmentLine.WE_PalletID = "ABC";
			AssertHasError(adjustmentLine.WE_PalletIDInfo, "A Pallet ID cannot be entered as the Location is a Pick Face that does not Retain Pallet IDs.");

			adjustmentLine.WE_WL = normalLocation.PK;
			adjustmentLine.WE_PalletID = "ABC";
			AssertNoErrors("When location is not pick face - we don't mind to have a pallet id", adjustmentLine.WE_PalletIDInfo);

			adjustmentLine.WE_WL = pickFaceLocation.PK;
			adjustmentLine.WE_PalletID = "";
			AssertNoErrors("It is OK to not to have a palletId for pick face location", adjustmentLine.WE_PalletIDInfo);

			adjustmentLine.WE_WL = pickFaceLocation.PK;
			adjustmentLine.WE_PalletID = "ABC";
			AssertHasError(adjustmentLine.WE_PalletIDInfo, "A Pallet ID cannot be entered as the Location is a Pick Face that does not Retain Pallet IDs.");

			adjustmentLine.WE_WL = pickFaceLocation.PK;
			adjustmentLine.WE_TransactionQuantity = -75m;
			adjustmentLine.WE_PalletID = "ABC";
			AssertNoErrors("It is OK to to have a palletId for pick face location if it is an adjust out.", adjustmentLine.WE_PalletIDInfo);

			pickFaceLocation.LocationType.WLT_RetainPalletIDsInFixedPickFaces = true;

			adjustmentLine.WE_TransactionQuantity = 75m;
			adjustmentLine.WE_WL = pickFaceLocation.PK;
			adjustmentLine.WE_PalletID = "ABC";
			AssertNoErrors("When WLT_RetainPalletIDsInFixedPickFaces is set to true, we can keep Pallet ID.", adjustmentLine.WE_PalletIDInfo);
		}

		public void TestCheckWE_PalletID_ValidateAdjustIntoPickFaceLocation_WithPalletIdDifferentClient()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var differentClient = Helper.CreateClient("C2", "C2");
			Helper.CreateProductClientRelationShip(differentClient, data.Part1);
			var pickFaceLocationForDifferentClient = locations[0];
			Helper.CreateProductPickFace(data.Part1, differentClient, pickFaceLocationForDifferentClient);
			var normalLocation = locations[1];

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 50m, normalLocation);
			adjustmentLine.WE_WL = pickFaceLocationForDifferentClient.PK;
			adjustmentLine.WE_PalletID = "ABC";
			AssertNoErrors("Location is a pickface for a different client. Therefore, it should not have any errors.", adjustmentLine.WE_PalletIDInfo);

			adjustmentLine.WE_WL = pickFaceLocationForDifferentClient.PK;
			adjustmentLine.WE_PalletID = "";
			AssertNoErrors(adjustmentLine.WE_PalletIDInfo);
		}

		public void TestCheckWE_PalletID_ValidateAdjustIntoPickFaceLocation_WithPalletIdErrorIsNotShownIfOtherErrorExists()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var product = WhsProduct.GetWhsProduct(data.Part1);
			var pickFace = product.PickFaces.AddNew();
			var pickFaceLocation = locations[0];
			pickFace.WF_WL = pickFaceLocation.PK;
			var normalLocation = locations[1];

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 50m, normalLocation);
			adjustmentLine.WE_WL = pickFaceLocation.PK;
			adjustmentLine.WE_PalletID = "测试区";
			AssertNoError(adjustmentLine.WE_PalletIDInfo, "A Pallet ID cannot be entered as the Location is a Pick Face that does not Retain Pallet IDs.");

			adjustmentLine.WE_WL = pickFaceLocation.PK;
			adjustmentLine.WE_PalletID = "GRT";
			AssertHasError(adjustmentLine.WE_PalletIDInfo, "A Pallet ID cannot be entered as the Location is a Pick Face that does not Retain Pallet IDs.");
		}

		#endregion

		#endregion

		#region TestCheckWE_OP

		#region TestCheckWE_OP_WithoutNewClient

		public void TestCheckWE_OP_WithoutNewClient()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var newClient = Helper.CreateClient("O2");
			var adjustmentWithoutWhsOrgAndNewClient = Helper.CreateWhsAdjustment(ZGuid.Empty, ZGuid.Empty, newClient.PK, Notify);
			adjustmentWithoutWhsOrgAndNewClient.OwnershipAdjustedClientPK = ZGuid.Empty;
			var line = adjustmentWithoutWhsOrgAndNewClient.Lines.AddNew();
			AssertNoExceptionThrown(() => line.WE_OP = data.Part1.PK);
		}

		#endregion

		#region TestCheckWE_OP_OwnershipChangeAdjustment

		#region TestCheckWE_OP_CheckClientProductRelationshipCanBeCreatedOnNewClient_ProductLevel

		public void TestCheckWE_OP_CheckClientProductRelationshipCanBeCreatedOnNewClient_ProductLevel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var newClient = Helper.CreateClient("C2");

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			var newLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, location);
			newLine.WE_OP = ZGuid.Empty; // To trigger validation for product
			newLine.WE_OP = data.Part1.PK;
			AssertNoErrors("Since new client doesn't have product client relationship and no attributes specified, there should be no errors.", newLine.WE_OPInfo);

			Helper.SetClientAllAttributeType(data.Org1, true);
			newLine.WE_OP = ZGuid.Empty; // To trigger validation for product
			newLine.WE_OP = data.Part1.PK;
			AssertNoErrors("Since new client doesn't have product client relationship, there should be no errors.", newLine.WE_OPInfo);

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetClientAllAttributeType(newClient, false);
			Helper.CreateProductClientRelationShip(newClient, data.Part1);
			AssertClientProductRelationshipValidation(data.Org1, newClient, data.Part1, newLine, AttributeNumber.One);
			AssertClientProductRelationshipValidation(data.Org1, newClient, data.Part1, newLine, AttributeNumber.Two);
			AssertClientProductRelationshipValidation(data.Org1, newClient, data.Part1, newLine, AttributeNumber.Three);
			AssertClientProductRelationshipValidation(data.Org1, newClient, data.Part1, newLine, AttributeNumber.Serial);
			AssertClientProductRelationshipValidation(data.Org1, newClient, data.Part1, newLine, AttributeNumber.ExpiryDate);
			AssertClientProductRelationshipValidation(data.Org1, newClient, data.Part1, newLine, AttributeNumber.PackingDate);
		}

		void AssertClientProductRelationshipValidation(OrgHeader oldClient, OrgHeader newClient, OrgSupplierPart part, WhsAdjustmentLine newLine, AttributeNumber number)
		{
			Helper.SetClientAttributeType(oldClient, number, true);
			Helper.SetClientAttributeType(newClient, number, true);
			Helper.SetProductAttributeUse(oldClient, part, number, true);
			Helper.SetProductAttributeUse(newClient, part, number, false);
			newLine.WE_OP = ZGuid.Empty; // to trigger validation
			newLine.WE_OP = part.PK;
			AssertHasError(newLine.WE_OPInfo, string.Format("Attributes specified for old client '{0}' doesn't match with attributes specified for new client '{1}' in product master file.", oldClient.OH_Code, newClient.OH_Code));

			Helper.SetProductAttributeUse(newClient, part, number, true);
			newLine.WE_OP = ZGuid.Empty; // to trigger validation
			newLine.WE_OP = part.PK;
			AssertNoErrors("Since attributes matches, there should be no errors.", newLine.WE_OPInfo);
			Helper.SetProductAttributeUse(oldClient, part, number, false); // cleanup
			Helper.SetProductAttributeUse(newClient, part, number, false); // cleanup
		}

		#endregion

		#region TestCheckWE_OP_CheckClientProductRelationshipCanBeCreatedOnNewClient_ClientLevel

		public void TestCheckWE_OP_CheckClientProductRelationshipCanBeCreatedOnNewClient_ClientLevel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			AssertMatchingClientProductRelationshipDefinition(data, PartAttributeTypeList.Codes.BatchNumber);
			AssertMatchingClientProductRelationshipDefinition(data, PartAttributeTypeList.Codes.Mandatory);
			AssertMatchingClientProductRelationshipDefinition(data, PartAttributeTypeList.Codes.BatchNumber, PartAttributeTypeList.Codes.Mandatory);
			AssertMatchingClientProductRelationshipDefinition(data, PartAttributeTypeList.Codes.Mandatory, PartAttributeTypeList.Codes.BatchNumber);
			AssertMatchingClientProductRelationshipDefinition(data, PartAttributeTypeList.Codes.BatchNumber, PartAttributeTypeList.Codes.Mandatory, PartAttributeTypeList.Codes.VIN);
		}

		void AssertMatchingClientProductRelationshipDefinition(TestDataSimpleEnvironment data, string partAttributeType)
		{
			var location = data.Whs1.DefaultLocation;
			var newClientWithPartAttributeTwo = Helper.CreateClient("A2");
			var newClientWithPartAttributeThree = Helper.CreateClient("A3");
			var newClientWithoutAttributes = Helper.CreateClient("A4");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, partAttributeType);
			Helper.SetClientAttributeType(newClientWithPartAttributeTwo, AttributeNumber.Two, partAttributeType);
			Helper.SetClientAttributeType(newClientWithPartAttributeThree, AttributeNumber.Three, partAttributeType);

			Helper.CreateProductClientRelationShip(newClientWithPartAttributeTwo, data.Part1);
			Helper.CreateProductClientRelationShip(newClientWithPartAttributeThree, data.Part1);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(newClientWithPartAttributeTwo, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(newClientWithPartAttributeThree, data.Part1, AttributeNumber.Three, true);

			// Old client uses part attribute type 1 and new client has Part attribute type 2
			var adjustmentForOrg1ToNewClient1 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClientWithPartAttributeTwo, Notify);
			var newLineForOrg1ToNewClientWithPartAttributeTwo = Helper.CreateWhsAdjustmentLine(adjustmentForOrg1ToNewClient1, data.Part1, -5m, location);
			AssertNoErrors(newLineForOrg1ToNewClientWithPartAttributeTwo.WE_OPInfo);

			Helper.SetProductAttributeUse(newClientWithPartAttributeTwo, data.Part1, AttributeNumber.Two, false);
			newLineForOrg1ToNewClientWithPartAttributeTwo.WE_OP = ZGuid.Empty; // To trigger validation for Product
			newLineForOrg1ToNewClientWithPartAttributeTwo.WE_OP = data.Part1.PK;
			AssertHasError(newLineForOrg1ToNewClientWithPartAttributeTwo.WE_OPInfo, string.Format("Attributes specified for old client '{0}' doesn't match with attributes specified for new client '{1}' in product master file.", data.Org1.OH_Code, newClientWithPartAttributeTwo.OH_Code));
			Helper.SetProductAttributeUse(newClientWithPartAttributeTwo, data.Part1, AttributeNumber.Two, true); // clean up

			// Old client uses part attribute type 1 and new client has Part attribute type 3
			var adjustmentForOrg1ToNewClient2 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClientWithPartAttributeThree, Notify);
			var newLineForOrg1ToNewClientWithPartAttributeThree = Helper.CreateWhsAdjustmentLine(adjustmentForOrg1ToNewClient2, data.Part1, -5m, location);
			AssertNoErrors(newLineForOrg1ToNewClientWithPartAttributeThree.WE_OPInfo);

			Helper.SetProductAttributeUse(newClientWithPartAttributeThree, data.Part1, AttributeNumber.Three, false);
			newLineForOrg1ToNewClientWithPartAttributeThree.WE_OP = ZGuid.Empty; // To trigger validation for Product
			newLineForOrg1ToNewClientWithPartAttributeThree.WE_OP = data.Part1.PK;
			AssertHasError(newLineForOrg1ToNewClientWithPartAttributeThree.WE_OPInfo, string.Format("Attributes specified for old client '{0}' doesn't match with attributes specified for new client '{1}' in product master file.", data.Org1.OH_Code, newClientWithPartAttributeThree.OH_Code));
			Helper.SetProductAttributeUse(newClientWithPartAttributeThree, data.Part1, AttributeNumber.Three, true); // clean up

			// Old client uses part attribute type 2 and new client has Part attribute type 1
			var adjustmentForNewClientWithPartAttributeTwo = Helper.CreateWhsAdjustment(newClientWithPartAttributeTwo, data.Whs1, data.Org1, Notify);
			var newLineForNewClientWithPartAttributeTwo = Helper.CreateWhsAdjustmentLine(adjustmentForNewClientWithPartAttributeTwo, data.Part1, -5m, location);
			AssertNoErrors(newLineForNewClientWithPartAttributeTwo.WE_OPInfo);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, false);
			newLineForNewClientWithPartAttributeTwo.WE_OP = ZGuid.Empty; //To trigger validation for Product
			newLineForNewClientWithPartAttributeTwo.WE_OP = data.Part1.PK;
			AssertHasError(newLineForNewClientWithPartAttributeTwo.WE_OPInfo, string.Format("Attributes specified for old client '{0}' doesn't match with attributes specified for new client '{1}' in product master file.", newClientWithPartAttributeTwo.OH_Code, data.Org1.OH_Code));
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true); // clean up

			// Old client uses part attribute type 2 and new client has Part attribute type 3
			var adjustmentForNewClientWithPartAttributeTwoToNewClientWithPartAttributeThree = Helper.CreateWhsAdjustment(newClientWithPartAttributeTwo, data.Whs1, newClientWithPartAttributeThree, Notify);
			var newLineForNewClientWithPartAttributeTwoToNewClientWithPartAttributeThree = Helper.CreateWhsAdjustmentLine(adjustmentForNewClientWithPartAttributeTwoToNewClientWithPartAttributeThree, data.Part1, -5m, location);
			AssertNoErrors(newLineForNewClientWithPartAttributeTwoToNewClientWithPartAttributeThree.WE_OPInfo);

			Helper.SetProductAttributeUse(newClientWithPartAttributeThree, data.Part1, AttributeNumber.Three, false);
			newLineForNewClientWithPartAttributeTwoToNewClientWithPartAttributeThree.WE_OP = ZGuid.Empty; // To trigger validation for Product
			newLineForNewClientWithPartAttributeTwoToNewClientWithPartAttributeThree.WE_OP = data.Part1.PK;
			AssertHasError(newLineForNewClientWithPartAttributeTwoToNewClientWithPartAttributeThree.WE_OPInfo, string.Format("Attributes specified for old client '{0}' doesn't match with attributes specified for new client '{1}' in product master file.", newClientWithPartAttributeTwo.OH_Code, newClientWithPartAttributeThree.OH_Code));
			Helper.SetProductAttributeUse(newClientWithPartAttributeThree, data.Part1, AttributeNumber.Three, true); // clean up

			// Old client uses part attribute type 3 and new client has Part attribute type 2
			var adjustmentForNewClientWithPartAttributeThreeToNewClientWithPartAttributeTwo = Helper.CreateWhsAdjustment(newClientWithPartAttributeThree, data.Whs1, newClientWithPartAttributeTwo, Notify);
			var newLineForNewClient2ToNewClient1 = Helper.CreateWhsAdjustmentLine(adjustmentForNewClientWithPartAttributeThreeToNewClientWithPartAttributeTwo, data.Part1, -5m, location);
			AssertNoErrors(newLineForNewClient2ToNewClient1.WE_OPInfo);

			Helper.SetProductAttributeUse(newClientWithPartAttributeTwo, data.Part1, AttributeNumber.Two, false);
			newLineForNewClient2ToNewClient1.WE_OP = ZGuid.Empty; // To trigger validation for Product
			newLineForNewClient2ToNewClient1.WE_OP = data.Part1.PK;
			AssertHasError(newLineForNewClient2ToNewClient1.WE_OPInfo, string.Format("Attributes specified for old client '{0}' doesn't match with attributes specified for new client '{1}' in product master file.", newClientWithPartAttributeThree.OH_Code, newClientWithPartAttributeTwo.OH_Code));
		}

		void AssertMatchingClientProductRelationshipDefinition(TestDataSimpleEnvironment data, string partAttributeType1, string partAttributeType2)
		{
			var location = data.Whs1.DefaultLocation;
			var newClientWithPartAttributeOneAndTwo = Helper.CreateClient("A12");
			var newClientWithPartAttributeOneAndThree = Helper.CreateClient("A13");
			var newClientWithPartAttributeTwoAndThree = Helper.CreateClient("A23");

			Helper.CreateProductClientRelationShip(newClientWithPartAttributeOneAndTwo, data.Part1);
			Helper.CreateProductClientRelationShip(newClientWithPartAttributeOneAndThree, data.Part1);
			Helper.CreateProductClientRelationShip(newClientWithPartAttributeTwoAndThree, data.Part1);

			Helper.SetClientAttributeType(newClientWithPartAttributeOneAndTwo, AttributeNumber.One, partAttributeType1);
			Helper.SetClientAttributeType(newClientWithPartAttributeOneAndTwo, AttributeNumber.Two, partAttributeType2);
			Helper.SetClientAttributeType(newClientWithPartAttributeOneAndThree, AttributeNumber.One, partAttributeType1);
			Helper.SetClientAttributeType(newClientWithPartAttributeOneAndThree, AttributeNumber.Three, partAttributeType2);
			Helper.SetClientAttributeType(newClientWithPartAttributeTwoAndThree, AttributeNumber.Two, partAttributeType1);
			Helper.SetClientAttributeType(newClientWithPartAttributeTwoAndThree, AttributeNumber.Three, partAttributeType2);

			Helper.SetProductAttributeUse(newClientWithPartAttributeOneAndTwo, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(newClientWithPartAttributeOneAndTwo, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(newClientWithPartAttributeOneAndThree, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(newClientWithPartAttributeOneAndThree, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(newClientWithPartAttributeTwoAndThree, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(newClientWithPartAttributeTwoAndThree, data.Part1, AttributeNumber.Three, true);

			// attributes one-two to one-three
			var adjForClientWithPartAttributeOneTwoToOneThree = Helper.CreateWhsAdjustment(newClientWithPartAttributeOneAndTwo, data.Whs1, newClientWithPartAttributeOneAndThree, Notify);
			var lineForClientWithPartAttributeOneTwoToOneThree = Helper.CreateWhsAdjustmentLine(adjForClientWithPartAttributeOneTwoToOneThree, data.Part1, -5m, location);
			AssertNoErrors(lineForClientWithPartAttributeOneTwoToOneThree.WE_OPInfo);

			Helper.SetProductAttributeUse(newClientWithPartAttributeOneAndThree, data.Part1, AttributeNumber.One, false);
			lineForClientWithPartAttributeOneTwoToOneThree.WE_OP = ZGuid.Empty; //  To trigger validation for Product
			lineForClientWithPartAttributeOneTwoToOneThree.WE_OP = data.Part1.PK;
			AssertHasError(lineForClientWithPartAttributeOneTwoToOneThree.WE_OPInfo, string.Format("Attributes specified for old client '{0}' doesn't match with attributes specified for new client '{1}' in product master file.", newClientWithPartAttributeOneAndTwo.OH_Code, newClientWithPartAttributeOneAndThree.OH_Code));
			Helper.SetProductAttributeUse(newClientWithPartAttributeOneAndThree, data.Part1, AttributeNumber.One, true); // clean up

			// attributes one-three to one-two
			var adjForClientWithPartAttributeOneThreeToOneTwo = Helper.CreateWhsAdjustment(newClientWithPartAttributeOneAndThree, data.Whs1, newClientWithPartAttributeOneAndTwo, Notify);
			var lineForClientWithPartAttributeOneThreeToOneTwo = Helper.CreateWhsAdjustmentLine(adjForClientWithPartAttributeOneThreeToOneTwo, data.Part1, -5m, location);
			AssertNoErrors(lineForClientWithPartAttributeOneThreeToOneTwo.WE_OPInfo);

			Helper.SetProductAttributeUse(newClientWithPartAttributeOneAndTwo, data.Part1, AttributeNumber.One, false);
			lineForClientWithPartAttributeOneThreeToOneTwo.WE_OP = ZGuid.Empty; //  To trigger validation for Product
			lineForClientWithPartAttributeOneThreeToOneTwo.WE_OP = data.Part1.PK;
			AssertHasError(lineForClientWithPartAttributeOneThreeToOneTwo.WE_OPInfo, string.Format("Attributes specified for old client '{0}' doesn't match with attributes specified for new client '{1}' in product master file.", newClientWithPartAttributeOneAndThree.OH_Code, newClientWithPartAttributeOneAndTwo.OH_Code));
			Helper.SetProductAttributeUse(newClientWithPartAttributeOneAndTwo, data.Part1, AttributeNumber.One, true); // clean up

			// attributes one-two to two-three
			var adjForClientWithPartAttributeOneTwoToTwoThree = Helper.CreateWhsAdjustment(newClientWithPartAttributeOneAndTwo, data.Whs1, newClientWithPartAttributeTwoAndThree, Notify);
			var lineForClientWithPartAttributeOneTwoToTwoThree = Helper.CreateWhsAdjustmentLine(adjForClientWithPartAttributeOneTwoToTwoThree, data.Part1, -5m, location);
			AssertNoErrors(lineForClientWithPartAttributeOneTwoToTwoThree.WE_OPInfo);

			Helper.SetProductAttributeUse(newClientWithPartAttributeTwoAndThree, data.Part1, AttributeNumber.Two, false);
			lineForClientWithPartAttributeOneTwoToTwoThree.WE_OP = ZGuid.Empty; //  To trigger validation for Product
			lineForClientWithPartAttributeOneTwoToTwoThree.WE_OP = data.Part1.PK;
			AssertHasError(lineForClientWithPartAttributeOneTwoToTwoThree.WE_OPInfo, string.Format("Attributes specified for old client '{0}' doesn't match with attributes specified for new client '{1}' in product master file.", newClientWithPartAttributeOneAndTwo.OH_Code, newClientWithPartAttributeTwoAndThree.OH_Code));
			Helper.SetProductAttributeUse(newClientWithPartAttributeTwoAndThree, data.Part1, AttributeNumber.Two, true); // clean up
		}

		void AssertMatchingClientProductRelationshipDefinition(TestDataSimpleEnvironment data, string partAttributeType1, string partAttributeType2, string partAttributeType3)
		{
			var location = data.Whs1.Rows[0].Locations[0];
			var newClient1 = Helper.CreateClient("123");
			var newClient2 = Helper.CreateClient("312");

			Helper.CreateProductClientRelationShip(newClient1, data.Part1);
			Helper.CreateProductClientRelationShip(newClient2, data.Part1);

			Helper.SetClientAttributeType(newClient1, AttributeNumber.One, partAttributeType1);
			Helper.SetClientAttributeType(newClient1, AttributeNumber.Two, partAttributeType2);
			Helper.SetClientAttributeType(newClient1, AttributeNumber.Three, partAttributeType3);

			Helper.SetClientAttributeType(newClient2, AttributeNumber.One, partAttributeType3);
			Helper.SetClientAttributeType(newClient2, AttributeNumber.Two, partAttributeType1);
			Helper.SetClientAttributeType(newClient2, AttributeNumber.Three, partAttributeType2);

			Helper.SetProductAttributeUse(newClient1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(newClient1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(newClient1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(newClient2, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(newClient2, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(newClient2, data.Part1, AttributeNumber.Three, true);

			// 123 to 312
			var adjForClient1 = Helper.CreateWhsAdjustment(newClient1, data.Whs1, newClient2, Notify);
			var lineForClient1 = Helper.CreateWhsAdjustmentLine(adjForClient1, data.Part1, -5m, location);
			AssertNoErrors(lineForClient1.WE_OPInfo);

			Helper.SetProductAttributeUse(newClient2, data.Part1, AttributeNumber.Two, false);
			lineForClient1.WE_OP = ZGuid.Empty; //  To trigger validation for Product
			lineForClient1.WE_OP = data.Part1.PK;
			AssertHasError(lineForClient1.WE_OPInfo, string.Format("Attributes specified for old client '{0}' doesn't match with attributes specified for new client '{1}' in product master file.", newClient1.OH_Code, newClient2.OH_Code));
			Helper.SetProductAttributeUse(newClient2, data.Part1, AttributeNumber.Two, true); // clean up

			// 312 to 123
			var adjForClient2 = Helper.CreateWhsAdjustment(newClient2, data.Whs1, newClient1, Notify);
			var lineForClient2 = Helper.CreateWhsAdjustmentLine(adjForClient2, data.Part1, -5m, location);
			AssertNoErrors(lineForClient2.WE_OPInfo);

			Helper.SetProductAttributeUse(newClient1, data.Part1, AttributeNumber.Two, false);
			lineForClient2.WE_OP = ZGuid.Empty; //  To trigger validation for Product
			lineForClient2.WE_OP = data.Part1.PK;
			AssertHasError(lineForClient2.WE_OPInfo, string.Format("Attributes specified for old client '{0}' doesn't match with attributes specified for new client '{1}' in product master file.", newClient2.OH_Code, newClient1.OH_Code));
		}

		#endregion

		#region TestCheckWE_OP_CheckNewClientProductOwnership_WithProductNotUseAttributes

		public void TestCheckWE_OP_CheckNewClientProductOwnership_WithProductNotUseAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var newClient = Helper.CreateClient("A1");
			Helper.CreateProductClientRelationShip(newClient, data.Part1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetClientAllAttributeType(newClient, true);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, location);
			AssertNoErrors(line.WE_OPInfo);

			AssertAttributeUseage(line, data.Org1, newClient, data.Part1, AttributeNumber.One);
			AssertAttributeUseage(line, data.Org1, newClient, data.Part1, AttributeNumber.Two);
			AssertAttributeUseage(line, data.Org1, newClient, data.Part1, AttributeNumber.Three);

			//new client has non-matching attributes
			Helper.SetClientAttributeType(newClient, AttributeNumber.One, false);
			Helper.SetClientAttributeType(newClient, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(newClient, AttributeNumber.Three, false);
			line.WE_OP = ZGuid.Empty; // To trigger validation for Product
			line.WE_OP = data.Part1.PK;
			AssertNoErrors(line.WE_OPInfo);

			AssertAttributeUseMatching(line, data.Org1, newClient, data.Part1, AttributeNumber.One);
			AssertAttributeUseMatching(line, data.Org1, newClient, data.Part1, AttributeNumber.Two);
			AssertAttributeUseMatching(line, data.Org1, newClient, data.Part1, AttributeNumber.Three);
		}

		void AssertAttributeUseage(WhsAdjustmentLine line, OrgHeader oldClient, OrgHeader newClient, OrgSupplierPart part, AttributeNumber attributeNumber)
		{
			Helper.SetProductAttributeUse(newClient, part, attributeNumber, true);
			line.WE_OP = ZGuid.Empty; // To trigger validation for Product
			line.WE_OP = part.PK;
			AssertHasError(line.WE_OPInfo, string.Format("Attributes specified for old client '{0}' doesn't match with attributes specified for new client '{1}' in product master file.", oldClient.OH_Code, newClient.OH_Code));

			Helper.SetProductAttributeUse(newClient, part, attributeNumber, false); // cleanup
			line.WE_OP = ZGuid.Empty; // To trigger validation for Product
			line.WE_OP = part.PK; // cleanup
			AssertNoErrors(line.WE_OPInfo); // cleanup
		}

		void AssertAttributeUseMatching(WhsAdjustmentLine line, OrgHeader oldClient, OrgHeader newClient, OrgSupplierPart part, AttributeNumber attributeNumber)
		{
			Helper.SetProductAttributeUse(oldClient, part, attributeNumber, true);
			Helper.SetProductAttributeUse(newClient, part, attributeNumber, true);
			line.WE_OP = ZGuid.Empty; // To trigger validation for Product
			line.WE_OP = part.PK;
			AssertHasError(line.WE_OPInfo, string.Format("Attributes specified for old client '{0}' doesn't match with attributes specified for new client '{1}' in product master file.", oldClient.OH_Code, newClient.OH_Code));

			Helper.SetProductAttributeUse(oldClient, part, attributeNumber, false); // clean up
			Helper.SetProductAttributeUse(newClient, part, attributeNumber, false); // clean up
			line.WE_OP = ZGuid.Empty; // To trigger validation for Product
			line.WE_OP = part.PK;
			AssertNoError(line.WE_OPInfo, string.Format("Attributes specified for old client '{0}' doesn't match with attributes specified for new client '{1}' in product master file.", oldClient.OH_Code, newClient.OH_Code));
		}

		#endregion

		#region TestCheckWE_OP_CheckNewClientProductOwnership_DuplicateAttributeTypes_And_NameMatching

		public void TestCheckWE_OP_CheckNewClientProductOwnership_DuplicateAttributeTypes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var newClient = Helper.CreateClient("A1");
			Helper.CreateProductClientRelationShip(newClient, data.Part1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, "OldName1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true, "OldName2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true, "OldName3");
			Helper.SetClientAttributeType(newClient, AttributeNumber.One, true, "NewName1");
			Helper.SetClientAttributeType(newClient, AttributeNumber.Two, true, "NewName2");
			Helper.SetClientAttributeType(newClient, AttributeNumber.Three, true, "NewName3");

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, location);
			AssertNoErrors("Precondition", line.WE_OPInfo);

			// old client uses Mandatory attributes for Attribute one and two
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, false);
			AssertDuplicateAttributeValidation(data, newClient, line);

			// Old client uses Mandatory attributes for Attribute one and three
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			AssertDuplicateAttributeValidation(data, newClient, line);

			// Old client uses Mandatory attributes for Attribute two and three
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			AssertDuplicateAttributeValidation(data, newClient, line);
		}

		void AssertDuplicateAttributeValidation(TestDataSimpleEnvironment data, OrgHeader newClient, WhsAdjustmentLine line)
		{
			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.Three, false);
			line.WE_OP = ZGuid.Empty; // To trigger validation for Product
			line.WE_OP = data.Part1.PK;
			AssertHasError(line.WE_OPInfo, string.Format("Duplicated attribute types found on old client '{0}' and new client '{1}' without matching names in product master file.", data.Org1.OH_Code, newClient.OH_Code));

			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.Two, false);
			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.Three, true);
			line.WE_OP = ZGuid.Empty; // To trigger validation for Product
			line.WE_OP = data.Part1.PK;
			AssertHasError(line.WE_OPInfo, string.Format("Duplicated attribute types found on old client '{0}' and new client '{1}' without matching names in product master file.", data.Org1.OH_Code, newClient.OH_Code));

			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.Three, true);
			line.WE_OP = ZGuid.Empty; // To trigger validation for Product
			line.WE_OP = data.Part1.PK;
			AssertHasError(line.WE_OPInfo, string.Format("Duplicated attribute types found on old client '{0}' and new client '{1}' without matching names in product master file.", data.Org1.OH_Code, newClient.OH_Code));
		}

		public void TestCheckWE_OP_CheckNewClientProductOwnership_DuplicateAttributeTypesWithMatchingNames()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var newClient = Helper.CreateClient("A1");
			Helper.CreateProductClientRelationShip(newClient, data.Part1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, "Name1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true, "Name2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true, "Name3");
			Helper.SetClientAttributeType(newClient, AttributeNumber.One, true, "Name2");
			Helper.SetClientAttributeType(newClient, AttributeNumber.Two, true, "Name1");
			Helper.SetClientAttributeType(newClient, AttributeNumber.Three, true, "Name3");

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, location);
			AssertNoErrors("Precondition", line.WE_OPInfo);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.Three, true);

			line.WE_OP = ZGuid.Empty; // To trigger validation for Product
			line.WE_OP = data.Part1.PK;

			AssertNoErrors("Precondition", line.WE_OPInfo);
		}

		public void TestCheckWE_OP_CheckNewClientProductOwnership_DuplicateAttributeTypesWithIncorectlyMatchingNames()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var newClient = Helper.CreateClient("A1");
			Helper.CreateProductClientRelationShip(newClient, data.Part1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, "Name1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true, "Name2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false, "Name3");
			Helper.SetClientAttributeType(newClient, AttributeNumber.One, true, "Name2");
			Helper.SetClientAttributeType(newClient, AttributeNumber.Two, true, "Name3");
			Helper.SetClientAttributeType(newClient, AttributeNumber.Three, false, "Name1");

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, location);
			AssertNoErrors("Precondition", line.WE_OPInfo);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.Three, true);

			line.WE_OP = ZGuid.Empty; // To trigger validation for Product
			line.WE_OP = data.Part1.PK;

			AssertHasError(line.WE_OPInfo, string.Format("Duplicated attribute types found on old client '{0}' and new client '{1}' without matching names in product master file.", data.Org1.OH_Code, newClient.OH_Code));
		}

		public void TestCheckWE_OP_CheckNewClientProductOwnership_DuplicateAttributeTypesWithIncorectlyMatchingNamesAfterSwitchingOffSomeAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var newClient = Helper.CreateClient("A1");
			Helper.CreateProductClientRelationShip(newClient, data.Part1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, "Name1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true, "Name2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true, "Name3");
			Helper.SetClientAttributeType(newClient, AttributeNumber.One, true, "Name2");
			Helper.SetClientAttributeType(newClient, AttributeNumber.Two, true, "Name3");
			Helper.SetClientAttributeType(newClient, AttributeNumber.Three, true, "Name1");

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, location);
			AssertNoErrors("Precondition", line.WE_OPInfo);

			//Two Attributes set as 'Use' for both clients:
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, false);

			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.Three, true);

			line.WE_OP = ZGuid.Empty; // To trigger validation for Product
			line.WE_OP = data.Part1.PK;
			AssertHasError(line.WE_OPInfo, string.Format("Duplicated attribute types found on old client '{0}' and new client '{1}' without matching names in product master file.", data.Org1.OH_Code, newClient.OH_Code));

			//Different group of Attributes set as 'Use' for each of the both clients:
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, false);

			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.Two, false);
			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.Three, true);

			line.WE_OP = ZGuid.Empty; // To trigger validation for Product
			line.WE_OP = data.Part1.PK;
			AssertHasError(line.WE_OPInfo, string.Format("Attributes specified for old client '{0}' doesn't match with attributes specified for new client '{1}' in product master file.", data.Org1.OH_Code, newClient.OH_Code));

			//All Attributes set as 'Use' for one client and All Attributes set as not 'Use' for other one
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, false);

			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.Three, true);

			line.WE_OP = ZGuid.Empty; // To trigger validation for Product
			line.WE_OP = data.Part1.PK;
			AssertHasError(line.WE_OPInfo, string.Format("Duplicated attribute types found on old client '{0}' and new client '{1}' without matching names in product master file.", data.Org1.OH_Code, newClient.OH_Code));

			//All Attributes set as 'Use' for one client and only one is set as not 'Use' for other one
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, false);

			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(newClient, data.Part1, AttributeNumber.Three, true);

			line.WE_OP = ZGuid.Empty; // To trigger validation for Product
			line.WE_OP = data.Part1.PK;
			AssertHasError(line.WE_OPInfo, string.Format("Duplicated attribute types found on old client '{0}' and new client '{1}' without matching names in product master file.", data.Org1.OH_Code, newClient.OH_Code));
		}

		#endregion

		#region TestCheckWE_OP_CheckNewClientProductOwnership_NotUsedPartAttributes

		public void TestCheckWE_OP_CheckNewClientProductOwnership_NotUsedPartAttributes1()
		{
			AssertNewClientProductRelationshipWithNotUsedPartAttributes(AttributeNumber.One, AttributeNumber.One, AttributeNumber.Two);
		}

		public void TestCheckWE_OP_CheckNewClientProductOwnership_NotUsedPartAttributes2()
		{
			AssertNewClientProductRelationshipWithNotUsedPartAttributes(AttributeNumber.Two, AttributeNumber.One, AttributeNumber.Two);
		}

		public void TestCheckWE_OP_CheckNewClientProductOwnership_NotUsedPartAttributes3()
		{
			AssertNewClientProductRelationshipWithNotUsedPartAttributes(AttributeNumber.Three, AttributeNumber.One, AttributeNumber.Two);
		}

		public void TestCheckWE_OP_CheckNewClientProductOwnership_NotUsedPartAttributes4()
		{
			AssertNewClientProductRelationshipWithNotUsedPartAttributes(AttributeNumber.One, AttributeNumber.Two, AttributeNumber.Three);
		}

		public void TestCheckWE_OP_CheckNewClientProductOwnership_NotUsedPartAttributes5()
		{
			AssertNewClientProductRelationshipWithNotUsedPartAttributes(AttributeNumber.Two, AttributeNumber.Two, AttributeNumber.Three);
		}

		public void TestCheckWE_OP_CheckNewClientProductOwnership_NotUsedPartAttributes6()
		{
			AssertNewClientProductRelationshipWithNotUsedPartAttributes(AttributeNumber.Three, AttributeNumber.Two, AttributeNumber.Three);
		}

		public void TestCheckWE_OP_CheckNewClientProductOwnership_NotUsedPartAttributes7()
		{
			AssertNewClientProductRelationshipWithNotUsedPartAttributes(AttributeNumber.One, AttributeNumber.One, AttributeNumber.Three);
		}

		public void TestCheckWE_OP_CheckNewClientProductOwnership_NotUsedPartAttributes8()
		{
			AssertNewClientProductRelationshipWithNotUsedPartAttributes(AttributeNumber.Two, AttributeNumber.One, AttributeNumber.Three);
		}

		public void TestCheckWE_OP_CheckNewClientProductOwnership_NotUsedPartAttributes9()
		{
			AssertNewClientProductRelationshipWithNotUsedPartAttributes(AttributeNumber.Three, AttributeNumber.One, AttributeNumber.Three);
		}

		void AssertNewClientProductRelationshipWithNotUsedPartAttributes(AttributeNumber oldClientNumber, AttributeNumber number1, AttributeNumber number2)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var newClient = Helper.CreateClient("C1");
			var location = data.Whs1.DefaultLocation;

			Helper.SetClientAttributeType(data.Org1, oldClientNumber, PartAttributeTypeList.Codes.BatchNumber);
			Helper.SetClientAttributeType(newClient, number1, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(newClient, number2, PartAttributeTypeList.Codes.BatchNumber);

			Helper.CreateProductClientRelationShip(newClient, data.Part1);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, oldClientNumber, true);
			Helper.SetProductAttributeUse(newClient, data.Part1, number1, true);
			Helper.SetProductAttributeUse(newClient, data.Part1, number2, true);

			var adjustmentForOrg1ToNewClient1 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			var lineForAdjustmentForOrg1ToNewClient1 = Helper.CreateWhsAdjustmentLine(adjustmentForOrg1ToNewClient1, data.Part1, -5m, location);
			AssertHasError(lineForAdjustmentForOrg1ToNewClient1.WE_OPInfo, string.Format("Attributes specified for old client '{0}' doesn't match with attributes specified for new client '{1}' in product master file.",
						data.Org1.OH_Code, newClient.OH_Code));

			Helper.SetProductAttributeUse(newClient, data.Part1, number1, false);
			lineForAdjustmentForOrg1ToNewClient1.WE_OP = ZGuid.Empty; // To trigger validation for Product
			lineForAdjustmentForOrg1ToNewClient1.WE_OP = data.Part1.PK;
			AssertNoErrors(lineForAdjustmentForOrg1ToNewClient1.WE_OPInfo);

			Helper.SetProductAttributeUse(newClient, data.Part1, number1, true);
			var adjustmentForNewClient1ToOrg1 = Helper.CreateWhsAdjustment(newClient, data.Whs1, data.Org1, Notify);
			var lineForAdjustmentForNewClient1ToOrg1 = Helper.CreateWhsAdjustmentLine(adjustmentForNewClient1ToOrg1, data.Part1, -5m, location);
			AssertHasError(lineForAdjustmentForNewClient1ToOrg1.WE_OPInfo, string.Format("Attributes specified for old client '{0}' doesn't match with attributes specified for new client '{1}' in product master file.",
						newClient.OH_Code, data.Org1.OH_Code));

			Helper.SetProductAttributeUse(newClient, data.Part1, number1, false);
			lineForAdjustmentForOrg1ToNewClient1.WE_OP = ZGuid.Empty; // To trigger validation for Product
			lineForAdjustmentForOrg1ToNewClient1.WE_OP = data.Part1.PK;
			AssertNoErrors(lineForAdjustmentForOrg1ToNewClient1.WE_OPInfo);
		}

		#endregion

		#region TestCheckWE_OP_CheckClientProductRelationshipCanBeCreatedOnNewClient

		public void TestCheckWE_OP_CheckClientProductRelationshipCanBeCreatedOnNewClient()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var newClientWithNoAttribues = Helper.CreateClient("O1");
			var location = data.Whs1.DefaultLocation;
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClientWithNoAttribues, Notify);
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, location);
			AssertNoErrors(line.WE_OPInfo);

			AssertProductClientDefinitionValidation(data, line, AttributeNumber.One, newClientWithNoAttribues);
			AssertProductClientDefinitionValidation(data, line, AttributeNumber.Two, newClientWithNoAttribues);
			AssertProductClientDefinitionValidation(data, line, AttributeNumber.Three, newClientWithNoAttribues);
			AssertProductClientDefinitionValidation(data, line, AttributeNumber.Serial, newClientWithNoAttribues);
			AssertProductClientDefinitionValidation(data, line, AttributeNumber.ExpiryDate, newClientWithNoAttribues);
			AssertProductClientDefinitionValidation(data, line, AttributeNumber.PackingDate, newClientWithNoAttribues);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.BatchNumber);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.VIN);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: true);

			line.WE_OP = ZGuid.Empty; // To trigger validation for Product
			line.WE_OP = data.Part1.PK;
			AssertHasError(line.WE_OPInfo, string.Format("Attributes specified on client level for old client '{0}' cannot be specified for new client '{1}'.", data.Org1.OH_Code, newClientWithNoAttribues.OH_Code));
		}

		void AssertProductClientDefinitionValidation(TestDataSimpleEnvironment data, WhsAdjustmentLine line, AttributeNumber number, OrgHeader newClientWithNoAttribues)
		{
			Helper.SetClientAttributeType(data.Org1, number, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, number, true);
			line.WE_OP = ZGuid.Empty; // To trigger validation for Product
			line.WE_OP = data.Part1.PK;
			AssertHasError(line.WE_OPInfo, string.Format("Attributes specified on client level for old client '{0}' cannot be specified for new client '{1}'.", data.Org1.OH_Code, newClientWithNoAttribues.OH_Code));

			Helper.SetProductAttributeUse(data.Org1, data.Part1, number, false);
			line.WE_OP = ZGuid.Empty; // To trigger validation for Product
			line.WE_OP = data.Part1.PK;
			AssertNoError(line.WE_OPInfo, string.Format("Attributes specified on client level for old client '{0}' cannot be specified for new client '{1}'.", data.Org1.OH_Code, newClientWithNoAttribues.OH_Code));
		}

		#endregion

		#region TestCheckWE_OP_CheckClientProductRelationshipType

		public void TestCheckWE_OP_CheckClientProductRelationshipType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var newClient = Helper.CreateClient("C1");

			var ownershipChangedAdjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			var line = Helper.CreateWhsAdjustmentLine(ownershipChangedAdjustment, data.Part1, -1, "A");
			AssertNoErrors("No erros should be there since there is no product client relationship.", line.WE_OPInfo);

			Helper.CreateProductClientRelationShip(newClient, data.Part1, OrgPartRelation.RelationshipTypes.Supplier);
			line.WE_OP = data.Part2.PK; // To trigger validation
			line.WE_OP = data.Part1.PK;
			AssertHasError(line.WE_OPInfo, string.Format("Ownership adjusted client '{0}' has a supplier product relationship for the product '{1}'. Please change it to 'OWN' or 'BOTH'", newClient.OH_Code, data.Part1.OP_PartNum));

			Helper.CreateProductClientRelationShip(newClient, data.Part2, OrgPartRelation.RelationshipTypes.Both);
			line.WE_OP = data.Part2.PK;
			AssertNoErrors("No erros should be there since product client relationship is both.", line.WE_OPInfo);
		}

		#endregion

		#region TestCheckWE_OP_CheckJulianBatchNumberFormat

		public void TestCheckWE_OP_CheckJulianBatchNumberFormat_PartAttributeOne()
		{
			AssertCheckBatchNumberFormats(AttributeNumber.One, OrgPartRelationSchema.OU_UsePartAttrib1);
		}

		public void TestCheckWE_OP_CheckJulianBatchNumberFormat_PartAttributeTwo()
		{
			AssertCheckBatchNumberFormats(AttributeNumber.Two, OrgPartRelationSchema.OU_UsePartAttrib2);
		}

		public void TestCheckWE_OP_CheckJulianBatchNumberFormat_PartAttributeThree()
		{
			AssertCheckBatchNumberFormats(AttributeNumber.Three, OrgPartRelationSchema.OU_UsePartAttrib3);
		}

		void AssertCheckBatchNumberFormats(AttributeNumber number, SchemaBoolColumn relationSchemaColumn)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var newClient = Helper.CreateClient("C2");
			var relationForNewClient = Helper.CreateProductClientRelationShip(newClient, data.Part1);
			Helper.SetClientAttributeType(data.Org1, number, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetClientAttributeType(newClient, number, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 1);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, location);
			AssertNoErrors("Since there are no Julian batch number formats specified there shouldn't be any errors.", line.WE_OPInfo);

			relationForNewClient.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;
			line.WE_OP = ZGuid.Empty; // To trigger validation for product
			line.WE_OP = data.Part1.PK;
			AssertNoErrors("Since batch number is not used there shouldn't be any errors.", line.WE_OPInfo);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, number, true);
			Helper.SetProductAttributeUse(newClient, data.Part1, number, true);
			var relationForOrg1 = data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			line.WE_OP = ZGuid.Empty; // To trigger validation for product
			line.WE_OP = data.Part1.PK;
			AssertEquals("Precondtion", JulianBatchNumberFormatList.Codes.YDDD_BatchNumber, relationForNewClient.OU_JulianBatchNoFormat);
			AssertEquals("Precondtion", JulianBatchNumberFormatList.Codes.YDDD_BatchNumber, relationForOrg1.OU_JulianBatchNoFormat);
			AssertNoErrors("Since batch number formats are matching there shouldn't be any errors.", line.WE_OPInfo);

			relationForOrg1.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;
			line.WE_OP = ZGuid.Empty; // To trigger validation for product
			line.WE_OP = data.Part1.PK;
			AssertHasError(line.WE_OPInfo, string.Format("Julian Batch number format in product master file for old client '{0}' doesn't match with Julian Batch number format specified for new client '{1}' in product master file.", data.Org1.OH_Code, newClient.OH_Code));
		}

		#endregion

		#region TestCheckWE_OP_CheckPackingDateFormat

		public void TestCheckWE_OP_CheckPackingDateFormat()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.Rows[0].Locations[0];
			var newClient = Helper.CreateClient("C2");

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, location);
			var relationForOrg1 = data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			var relationForNewClient = Helper.CreateProductClientRelationShip(newClient, data.Part1);
			relationForOrg1.OU_UsePackingDate = true;
			relationForNewClient.OU_UsePackingDate = true;
			line.WE_OP = ZGuid.Empty; // To trigger validation for product
			line.WE_OP = data.Part1.PK;
			AssertNoErrors("Since packing date formats are not specified there shouldn't be any errors.", line.WE_OPInfo);

			relationForOrg1.OU_PackingDateFormatString = "DDMMYY";
			line.WE_OP = ZGuid.Empty; // To trigger validation for product
			line.WE_OP = data.Part1.PK;
			AssertHasErrors(string.Format("RF Packing Date format in product master file for old client '{0}' doesn't match with RF Packing Date format specified for new client '{1}' in product master file.", data.Org1.OH_Code, newClient.OH_Code), line.WE_OPInfo);

			relationForNewClient.OU_PackingDateFormatString = "DDMMYY";
			line.WE_OP = ZGuid.Empty; // To trigger validation for product
			line.WE_OP = data.Part1.PK;
			AssertNoErrors("Since RF Packing Date formats are matching there shouldn't be any errors.", line.WE_OPInfo);

			relationForOrg1.OU_PackingDateFormatString = "DDMMYYYY";
			line.WE_OP = ZGuid.Empty; // To trigger validation for product
			line.WE_OP = data.Part1.PK;
			AssertHasErrors(string.Format("RF Packing Date format in product master file for old client '{0}' doesn't match with RF Packing Date format specified for new client '{1}' in product master file.", data.Org1.OH_Code, newClient.OH_Code), line.WE_OPInfo);
		}

		#endregion

		#region TestCheckWE_OP_CheckExpiryDateFormat

		public void TestCheckWE_OP_CheckExpiryDateFormat()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.Rows[0].Locations[0];
			var newClient = Helper.CreateClient("C2");

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, location);
			var relationForOrg1 = data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			var relationForNewClient = Helper.CreateProductClientRelationShip(newClient, data.Part1);
			relationForOrg1.OU_UseExpiryDate = true;
			relationForNewClient.OU_UseExpiryDate = true;
			line.WE_OP = ZGuid.Empty; // To trigger validation for product
			line.WE_OP = data.Part1.PK;
			AssertNoErrors("Since expiry date formats are not specified there shouldn't be any errors.", line.WE_OPInfo);

			relationForOrg1.OU_ExpiryDateFormatString = "DDMMYY";
			line.WE_OP = ZGuid.Empty; // To trigger validation for product
			line.WE_OP = data.Part1.PK;
			AssertHasErrors(string.Format("RF Expiry Date format in product master file for old client '{0}' doesn't match with RF Expiry Date format specified for new client '{1}' in product master file.", data.Org1.OH_Code, newClient.OH_Code), line.WE_OPInfo);

			relationForNewClient.OU_ExpiryDateFormatString = "DDMMYY";
			line.WE_OP = ZGuid.Empty; // To trigger validation for product
			line.WE_OP = data.Part1.PK;
			AssertNoErrors("Since RF Expiry Date formats are matching there shouldn't be any errors.", line.WE_OPInfo);

			relationForOrg1.OU_ExpiryDateFormatString = "DDMMYYYY";
			line.WE_OP = ZGuid.Empty; // To trigger validation for product
			line.WE_OP = data.Part1.PK;
			AssertHasErrors(string.Format("RF Expiry Date format in product master file for old client '{0}' doesn't match with RF Expiry Date format specified for new client '{1}' in product master file.", data.Org1.OH_Code, newClient.OH_Code), line.WE_OPInfo);
		}

		#endregion

		#region TestCheckWE_OP_CheckMaximumShelfLife

		public void TestCheckWE_OP_CheckMaximumShelfLife1()
		{
			AssertValidationForMaximumShelfLife(AttributeNumber.One);
		}

		public void TestCheckWE_OP_CheckMaximumShelfLife2()
		{
			AssertValidationForMaximumShelfLife(AttributeNumber.Two);
		}

		public void TestCheckWE_OP_CheckMaximumShelfLife3()
		{
			AssertValidationForMaximumShelfLife(AttributeNumber.Three);
		}

		void AssertValidationForMaximumShelfLife(AttributeNumber number)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var newClient = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(newClient, data.Part1);
			Helper.SetClientAttributeType(data.Org1, number, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetClientAttributeType(newClient, number, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, number, true);
			Helper.SetProductAttributeUse(newClient, data.Part1, number, true);
			var productParamsForOldClient = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 2);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, newClient, Notify);
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, location);
			AssertNoErrors("Since Maximum shelf life doesn't exist. it would get created during finalisation.", line.WE_OPInfo);

			var productParamsForNewClient = Helper.CreateProductParamsByWhsAndClient(data.Part1, newClient, data.Whs1, 2);
			line.WE_OP = ZGuid.Empty; // To trigger validation for product
			line.WE_OP = data.Part1.PK;
			AssertNoErrors("Since there is maximum shelf life there shouldn't be any errors.", line.WE_OPInfo);

			productParamsForOldClient.W3_MaximumShelfLife = 1;
			line.WE_OP = ZGuid.Empty; // To trigger validation for product
			line.WE_OP = data.Part1.PK;
			AssertHasError("Since maximum shelf life is not matching, there should be errors.", line.WE_OPInfo,
				string.Format("Maximum Shelf Life defined for Product: {0}, Client: {1}, Warehouse: {2} does not match with Maximum Shelf Life defined for Product: {0}, Client: {3}, Warehouse: {2}.",
				data.Part1.OP_Desc, data.Org1.OH_Code, data.Whs1.WW_WarehouseCode, newClient.OH_Code));

			productParamsForNewClient.W3_MaximumShelfLife = 1;
			line.WE_OP = ZGuid.Empty; // To trigger validation for product
			line.WE_OP = data.Part1.PK;
			AssertNoErrors("Since maximum shelf life is matching, there shouldn't be any errors.", line.WE_OPInfo);
		}

		#endregion

		#endregion

		#region TestCheckWE_OP_InactiveProduct_AdjustmentLine

		public void TestCheckWE_OP_InactiveProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "1", Notify);
			adjustment.OwnershipAdjustedClientPK = ZGuid.Empty;
			var line = adjustment.Lines.AddNew();
			line.WE_TransactionQuantity = 5;

			line.WE_OP = data.Part1.PK;
			AssertNoError(line.WE_OPInfo, OrgSupplierPartCollection.ProductIsNotActiveErrorMessage);
			AssertNoWarning(line.WE_OPInfo, "This Product is Inactive");

			data.Part1.OP_IsActive = false;
			line.WE_OP = ZGuid.Empty;
			line.WE_OP = data.Part1.PK;
			AssertHasError(line.WE_OPInfo, OrgSupplierPartCollection.ProductIsNotActiveErrorMessage);
			AssertNoWarning(line.WE_OPInfo, "This Product is Inactive");

			line.WE_TransactionQuantity = -5;
			line.WE_OP = ZGuid.Empty;
			line.WE_OP = data.Part1.PK;
			AssertNoError(line.WE_OPInfo, OrgSupplierPartCollection.ProductIsNotActiveErrorMessage);
			AssertHasWarning(line.WE_OPInfo, "This Product is Inactive");
		}

		#endregion

		#region TestCheckWE_WL_MixedProducts

		public void TestCheckWE_WL_MixedProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");

			location1.WLV_PalletFloorSpaces = 2;
			location1.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 500m, location1);
			receive.FinaliseDocket();
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, location1.ToLocationString(), "Pallet-1");
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, 4m, location1.ToLocationString(), "Pallet-1");

			adjustment.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			adjustmentLine1.RunPreSaveValidation();
			adjustmentLine2.RunPreSaveValidation();
			adjustment.FinaliseDocket();

			var expectedErrorMessage =
				"Only a single product can be used in locations using Pallet Space capacities.";

			AssertHasError(adjustmentLine1.WE_WLInfo, expectedErrorMessage);
			AssertHasError(adjustmentLine2.WE_WLInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_OP_PalletConversion

		public void TestCheckWE_OP_WithoutPalletConversion()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");

			location1.WLV_PalletFloorSpaces = 1;
			location1.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1, "Pallet-1");
			receive.FinaliseDocket();
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, location1.ToLocationString(), "Pallet-2");
			adjustmentLine1.RunPreSaveValidation();

			AssertHasWarning(adjustmentLine1.WE_OPInfo, "Products without pallet conversions cannot be put in locations using pallet spaces.");
		}

		public void TestCheckWE_OP_WithPalletConversion()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");

			location1.WLV_PalletFloorSpaces = 1;
			location1.WLV_PalletStackHeight = 1;
			Helper.CreateProductUnit(data.Part1, "PLT", 500m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1, "Pallet-1");
			receive.FinaliseDocket();
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, location1.ToLocationString(), "Pallet-2");
			adjustmentLine1.RunPreSaveValidation();

			AssertEquals(500m, data.Part1.OP_StockKeepingUnitPerPallet);
			AssertNoWarnings(adjustmentLine1.WE_OPInfo);
		}

		#endregion

		#region TestCheckWE_OP_CanCalculateExpiryDateIfJulianBatchNumberIsUsed_AdjustmentOut

		public void TestCheckWE_OP_CanCalculateExpiryDateIfJulianBatchNumberIsUsed_AdjustmentOut()
		{
			var client = Helper.CreateClient("CLIENT");
			var whs = Helper.CreateWarehouse("WHS", "A", 1, 1);
			Helper.SetClientAttributeType(client, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(client, AttributeNumber.Two, PartAttributeTypeList.Codes.JulianBatchNumber); // should set use Expiry Date.

			var partWithNormalAttribute = Helper.CreateProduct(client, "P1");
			Helper.SetProductAttributeUse(client, partWithNormalAttribute, AttributeNumber.One, true);

			var docket = Helper.CreateWhsAdjustment(client, whs, "A1");
			var docketLine = Helper.CreateWhsAdjustmentLine(docket, partWithNormalAttribute, -1m, whs.DefaultLocation.WLV_LocationString, "");

			// Product with Normal Attribute - NO ERROR
			AssertNoWarning(docketLine.WE_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

			// Product with Julian Batch Number Attribute but without Product-Client-Warehouse parameter - ERROR
			var partWithJulianAttributeButWithoutClientWhsParam = Helper.CreateProduct(client, "P2");
			Helper.SetProductAttributeUse(client, partWithJulianAttributeButWithoutClientWhsParam, AttributeNumber.Two, true); // should set use Expiry Date.
			docketLine.WE_OP = partWithJulianAttributeButWithoutClientWhsParam.PK;
			AssertHasWarning(docketLine.WE_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);
			AssertNoError(docketLine.WE_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

			// Product with Julian Batch Number Attribute but with wrong client Product-Client-Warehouse parameter - ERROR
			var partWithJulianAttributeAndClientWhsParam_InvalidClient = Helper.CreateProduct(client, "P3");
			var incorrectClient = Helper.CreateClient("CLIENT2");
			Helper.SetProductAttributeUse(client, partWithJulianAttributeAndClientWhsParam_InvalidClient, AttributeNumber.Two, true); // should set use Expiry Date.
			Helper.CreateProductParamsByWhsAndClient(partWithJulianAttributeAndClientWhsParam_InvalidClient, incorrectClient, whs);
			docketLine.WE_OP = partWithJulianAttributeAndClientWhsParam_InvalidClient.PK;
			AssertHasWarning(docketLine.WE_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

			// Product with Julian Batch Number Attribute but with wrong warehouse Product-Client-Warehouse parameter - ERROR
			var partWithJulianAttributeAndClientWhsParam_InvalidWhs = Helper.CreateProduct(client, "P4");
			var incorrectWhs = Helper.CreateWarehouse("WH2");
			Helper.SetProductAttributeUse(client, partWithJulianAttributeAndClientWhsParam_InvalidWhs, AttributeNumber.Two, true); // should set use Expiry Date.
			Helper.CreateProductParamsByWhsAndClient(partWithJulianAttributeAndClientWhsParam_InvalidWhs, client, incorrectWhs);
			docketLine.WE_OP = partWithJulianAttributeAndClientWhsParam_InvalidWhs.PK;
			AssertHasWarning(docketLine.WE_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

			// Product with Julian Batch Number Attribute and with Product-Client-Warehouse parameter that has Maximum Shelf Life = 0 - ERROR
			var partWithJulianAttributeAndClientWhsParam_ZeroMaxShelfLife = Helper.CreateProduct(client, "P5");
			Helper.SetProductAttributeUse(client, partWithJulianAttributeAndClientWhsParam_ZeroMaxShelfLife, AttributeNumber.Two, true); // should set use Expiry Date.
			Helper.CreateProductParamsByWhsAndClient(partWithJulianAttributeAndClientWhsParam_ZeroMaxShelfLife, client, whs);
			docketLine.WE_OP = partWithJulianAttributeAndClientWhsParam_ZeroMaxShelfLife.PK;
			AssertHasWarning(docketLine.WE_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

			// Product with Julian Batch Number Attribute and with Product-Client-Warehouse parameter that has Maximum Shelf Life > 0 - NO ERROR
			var partWithJulianAttributeAndClientWhsParam_Correct = Helper.CreateProduct(client, "P6");
			Helper.SetProductAttributeUse(client, partWithJulianAttributeAndClientWhsParam_Correct, AttributeNumber.Two, true); // should set use Expiry Date.
			Helper.CreateProductParamsByWhsAndClient(partWithJulianAttributeAndClientWhsParam_Correct, client, whs).W3_MaximumShelfLife = 5;
			docketLine.WE_OP = partWithJulianAttributeAndClientWhsParam_Correct.PK;
			AssertNoWarning(docketLine.WE_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);
		}

		#endregion

		#endregion

		#region TestCheckWE_WL_EmptyPalletID

		public void TestCheckWE_WL_EmptyPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");

			location1.WLV_PalletFloorSpaces = 1;
			location1.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1, "Pallet-1");
			receive.FinaliseDocket();
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, location1.ToLocationString(), "");
			adjustmentLine1.RunPreSaveValidation();

			AssertHasError(adjustmentLine1.WE_WLInfo, "Inventory without Pallet ID cannot be put in locations using pallet spaces.");
		}

		#endregion

		#region TestCheckWE_PalletID_EmptyPalletID

		public void TestCheckWE_PalletID_EmptyPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");

			location1.WLV_PalletFloorSpaces = 1;
			location1.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1, "Pallet-1");
			receive.FinaliseDocket();
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, location1.ToLocationString(), "");
			adjustmentLine1.RunPreSaveValidation();

			AssertHasError(adjustmentLine1.WE_PalletIDInfo, "Inventory without Pallet ID cannot be put in locations using pallet spaces.");
		}

		#endregion

		#region Implementation

		protected override FinalisableDocketHelper<WhsAdjustment> GetNewDocketHelper()
		{
			return new FinalisableAdjustmentHelper(Factory);
		}

		#endregion
	}
}
