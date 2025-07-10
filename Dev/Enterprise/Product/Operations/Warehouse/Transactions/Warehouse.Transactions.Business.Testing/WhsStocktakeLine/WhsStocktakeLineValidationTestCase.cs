using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsStocktakeLineValidationTestCase : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckWU_GS_NKVerifiedBy

		public void TestCheckWU_GS_NKVerifiedBy()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("C1", "Login1");
			var inactiveStaff = Helper.CreateGlbStaff("CA1", "Login2", false);

			Factory.Save();

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1m, 1, StocktakeLineStatus.Codes.Open);

			// test the validation

			line.WU_GS_NKVerifiedBy = staff.GS_Code;
			AssertNoErrors("Since there is a valid staff code for verified by, there should be no errors.", line.WU_GS_NKVerifiedByInfo);

			line.WU_GS_NKVerifiedBy = "TC1";
			AssertHasError("Since there is an invalid staff code for verified by, there should be an error.", line.WU_GS_NKVerifiedByInfo, "Enter a valid Verified By.");

			line.WU_GS_NKVerifiedBy = "";
			AssertNoErrors("Since the staff code is empty for verified by, there should be no errors.", line.WU_GS_NKVerifiedByInfo);

			line.WU_GS_NKVerifiedBy = inactiveStaff.GS_Code;
			AssertHasError("Since staff code is in-active, there should be an error.", line.WU_GS_NKVerifiedByInfo, "This Verified By is inactive - it may not be used.");

			line.WU_GS_NKVerifiedBy = "";
			AssertNoErrors("Since the staff code is empty for verified by, there should be no errors.", line.WU_GS_NKVerifiedByInfo);
		}

		#endregion

		#region TestCheckWU_Count2VerifiedBy

		public void TestCheckWU_Count2VerifiedBy()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("C1", "Login1");
			var inactiveStaff = Helper.CreateGlbStaff("CA1", "Login2", false);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1m, 2, StocktakeLineStatus.Codes.Open);

			// test the validation

			line.WU_Count2VerifiedBy = staff.GS_Code;
			AssertNoErrors("Since there is a valid staff code for count 2 verified by, there should be no errors.", line.WU_Count2VerifiedByInfo);

			line.WU_Count2VerifiedBy = "TC1";
			AssertHasError("Since there is an invalid staff code for count 2 verified by, there should be an error.",
				line.WU_Count2VerifiedByInfo, "Enter a valid " + line.WU_Count2VerifiedByInfo.HumanReadableName + ".");

			line.WU_Count2VerifiedBy = "";
			AssertNoErrors("Since the staff code is empty for count 2 verified by, there should be no errors.", line.WU_Count2VerifiedByInfo);

			line.WU_Count2VerifiedBy = inactiveStaff.GS_Code;
			AssertHasError("Since staff code is in-active, there should be an error.",
				line.WU_Count2VerifiedByInfo, "This Count 2 Verified By is inactive - it may not be used.");

			line.WU_Count2VerifiedBy = "";
			AssertNoErrors("Since the staff code is empty for count 2 verified by, there should be no errors.", line.WU_Count2VerifiedByInfo);
		}

		#endregion

		#region TestCheckWU_Count3VerifiedBy

		public void TestCheckWU_Count3VerifiedBy()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("C1", "Login1");
			var inactiveStaff = Helper.CreateGlbStaff("CA1", "Login2", false);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1m, 3, StocktakeLineStatus.Codes.Open);

			// test the validation

			line.WU_Count3VerifiedBy = staff.GS_Code;
			AssertNoErrors("Since there is a valid staff code for count 3 verified by, there should be no errors.", line.WU_Count3VerifiedByInfo);

			line.WU_Count3VerifiedBy = "TC1";
			AssertHasError("Since there is an invalid staff code for count 3 verified by, there should be an error.",
				line.WU_Count3VerifiedByInfo, "Enter a valid " + line.WU_Count3VerifiedByInfo.HumanReadableName + ".");

			line.WU_Count3VerifiedBy = "";
			AssertNoErrors("Since staff code is empty for count 3 verified by, there should be no errors.", line.WU_Count3VerifiedByInfo);

			line.WU_Count3VerifiedBy = inactiveStaff.GS_Code;
			AssertHasError("Since staff code is in-active, there should be an error.",
				line.WU_Count3VerifiedByInfo, "This Count 3 Verified By is inactive - it may not be used.");

			line.WU_Count3VerifiedBy = "";
			AssertNoErrors("Since staff code is empty for count 3 verified by, there should be no errors.", line.WU_Count3VerifiedByInfo);
		}

		#endregion

		#region TestCheckWU_VerfiedByDate

		[TestDate(2012, 1, 1)]
		public void TestCheckWU_VerfiedByDate()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1);
			line.WU_TotalCounts = 1; // Uses count one column
			line.WU_Status = StocktakeLineStatus.Codes.Open;

			// test the validation

			AssertNoWarnings("Since verified date is empty, there should be no warnings.", line.WU_DateVerifiedInfo);

			line.WU_DateVerified = now.AddDays(1);
			AssertHasWarning("Since verified date is in the future, there should be a warning.", line.WU_DateVerifiedInfo, WhsStocktakeLineValidation.CannotSelectPastVerfiedDate);

			line.WU_DateVerified = now;
			AssertNoWarnings("Since verified date is now, there should be no warnings.", line.WU_DateVerifiedInfo);

			line.WU_DateVerified = now.AddDays(-1);
			AssertNoWarnings("Since verified date is in past, there should be no warnings.", line.WU_DateVerifiedInfo);

			line.WU_Status = StocktakeLineStatus.Codes.Closed;
			line.WU_DateVerified = now.AddDays(1);
			AssertNoWarnings("Even though verified date is in the future, the line is closed and therefore there should be no warnings.", line.WU_DateVerifiedInfo);

			line.WU_Status = StocktakeLineStatus.Codes.Open;
			line.WU_TotalCounts = 2;
			line.WU_DateVerified = now.AddDays(1);
			AssertNoWarnings("Even though verified date is in the future, the line uses second verified column and therefore there should be no warnings.", line.WU_DateVerifiedInfo);
		}

		#endregion

		#region TestCheckWU_Count2VerfiedByDate

		[TestDate(2012, 1, 1)]
		public void TestCheckWU_Count2VerfiedByDate()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1);
			line.WU_TotalCounts = 2; // Uses count 2 column
			line.WU_Status = StocktakeLineStatus.Codes.Open;

			// test the validation

			AssertNoWarnings("Since count 2 verified date is empty, there should be no warnings.", line.WU_Count2DateVerifiedInfo);

			line.WU_Count2DateVerified = now.AddDays(1);
			AssertHasWarning("Since count 2 verified date is in the future, there should be a warning.", line.WU_Count2DateVerifiedInfo, WhsStocktakeLineValidation.CannotSelectPastVerfiedDate);

			line.WU_Count2DateVerified = now;
			AssertNoWarnings("Since count 2 verified date is now, there should be no warnings.", line.WU_Count2DateVerifiedInfo);

			line.WU_Count2DateVerified = now.AddDays(-1);
			AssertNoWarnings("Since count 2 verified date is in past, there should be no warnings.", line.WU_Count2DateVerifiedInfo);

			line.WU_Status = StocktakeLineStatus.Codes.Closed;
			line.WU_Count2DateVerified = now.AddDays(1);
			AssertNoWarnings("Even though count 2 verified date is in the future, the line is closed and therefore there should be no warnings.", line.WU_Count2DateVerifiedInfo);

			line.WU_Status = StocktakeLineStatus.Codes.Open;
			line.WU_TotalCounts = 3;
			line.WU_Count2DateVerified = now.AddDays(1); // To revalidate
			AssertNoWarnings("Even though count 2 verified date is in the future, the line uses second verified column and therefore there should be no warnings.", line.WU_Count2DateVerifiedInfo);
		}

		#endregion

		#region TestCheckWU_Count3VerfiedByDate

		[TestDate(2012, 1, 1)]
		public void TestCheckWU_Count3VerfiedByDate()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1);
			line.WU_TotalCounts = 3; // Uses count 3 column
			line.WU_Status = StocktakeLineStatus.Codes.Open;

			// test the validation

			AssertNoWarnings("Since count 3 verified date is empty, there should be no warnings.", line.WU_Count3DateVerifiedInfo);

			line.WU_Count3DateVerified = now.AddDays(1);
			AssertHasWarning("Since count 3 verified date is in the future, there should be a warning.", line.WU_Count3DateVerifiedInfo, WhsStocktakeLineValidation.CannotSelectPastVerfiedDate);

			line.WU_Count3DateVerified = now;
			AssertNoWarnings("Since count 3 verified date is now, there should be no warnings.", line.WU_Count3DateVerifiedInfo);

			line.WU_Count3DateVerified = now.AddDays(-1);
			AssertNoWarnings("Since count 3 verified date is in past, there should be no warnings.", line.WU_Count3DateVerifiedInfo);

			line.WU_Status = StocktakeLineStatus.Codes.Closed;
			line.WU_Count3DateVerified = now.AddDays(1); // To revalidate
			AssertNoWarnings("Even though count 3 verified date is in the future, the line is closed and therefore there should be no warnings.", line.WU_Count3DateVerifiedInfo);
		}

		#endregion

		#region TestEmptyLocations

		#region TestEmptyLocations_ValidateAll

		public void TestEmptyLocations_ValidateAll()
		{
			// setup test data

			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup a stocktake and one line and one empty line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1);
			var tempLine = Helper.CreateEmptyWhsStocktakeLine(stocktake, data.Whs1.DefaultLocation);

			// test validation for temp stocktake line

			var validationForTempStocktakeLine = new EmptyWhsStocktakeLineValidation(tempLine);
			validationForTempStocktakeLine.ValidateAll();

			AssertNoErrors(tempLine.WU_BondedEntryKeyInfo);
			AssertNoErrors(tempLine.WU_DateClosedInfo);
			AssertNoErrors(tempLine.WU_DateVerifiedInfo);
			AssertNoErrors(tempLine.WU_ExpiryDateInfo);
			AssertNoErrors(tempLine.WU_F3_NKPackTypeInfo);
			AssertNoErrors(tempLine.WU_GS_NKVerifiedByInfo);
			AssertNoErrors(tempLine.WU_InventoryStatusInfo);
			AssertNoErrors(tempLine.WU_LastCountInfo);
			AssertNoErrors(tempLine.WU_LineCommentInfo);
			AssertNoErrors(tempLine.WU_LineNoInfo);
			AssertNoErrors(tempLine.WU_OH_ClientInfo);
			AssertNoErrors(tempLine.WU_OPInfo);
			AssertNoErrors(tempLine.WU_PackingDateInfo);
			AssertNoErrors(tempLine.WU_PalletIDInfo);
			AssertNoErrors(tempLine.WU_PartAttrib1Info);
			AssertNoErrors(tempLine.WU_PartAttrib2Info);
			AssertNoErrors(tempLine.WU_PartAttrib3Info);
			AssertNoErrors(tempLine.WU_StatusInfo);
			AssertNoErrors(tempLine.WU_SystemUnitsInfo);
			AssertNoErrors(tempLine.WU_WLInfo);
			AssertNoErrors(tempLine.WU_WSInfo);
		}

		#endregion

		#region TestEmptyLocations_Validations

		public void TestEmptyLocations_Validation()
		{
			// setup test data

			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup a stocktake and one line and one empty line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1);
			var tempLine = Helper.CreateEmptyWhsStocktakeLine(stocktake, data.Whs1.DefaultLocation);

			// test validation for temp stocktake line

			var validationForTempStocktakeLine = new EmptyWhsStocktakeLineValidation(tempLine);

			validationForTempStocktakeLine.ValidateWU_BondedEntryKey();
			AssertNoErrors(tempLine.WU_BondedEntryKeyInfo);

			validationForTempStocktakeLine.ValidateWU_DateClosed();
			AssertNoErrors(tempLine.WU_DateClosedInfo);

			validationForTempStocktakeLine.ValidateWU_DateVerified();
			AssertNoErrors(tempLine.WU_DateVerifiedInfo);

			validationForTempStocktakeLine.ValidateWU_ExpiryDate();
			AssertNoErrors(tempLine.WU_ExpiryDateInfo);

			validationForTempStocktakeLine.ValidateWU_F3_NKPackType();
			AssertNoErrors(tempLine.WU_F3_NKPackTypeInfo);

			validationForTempStocktakeLine.ValidateWU_GS_NKVerifiedBy();
			AssertNoErrors(tempLine.WU_GS_NKVerifiedByInfo);

			validationForTempStocktakeLine.ValidateWU_InventoryStatus();
			AssertNoErrors(tempLine.WU_InventoryStatusInfo);

			validationForTempStocktakeLine.ValidateWU_LastCount();
			AssertNoErrors(tempLine.WU_LastCountInfo);

			validationForTempStocktakeLine.ValidateWU_LineComment();
			AssertNoErrors(tempLine.WU_LineCommentInfo);

			validationForTempStocktakeLine.ValidateWU_LineNo();
			AssertNoErrors(tempLine.WU_LineNoInfo);

			validationForTempStocktakeLine.ValidateWU_OH_Client();
			AssertNoErrors(tempLine.WU_OH_ClientInfo);

			validationForTempStocktakeLine.ValidateWU_OP();
			AssertNoErrors(tempLine.WU_OPInfo);

			validationForTempStocktakeLine.ValidateWU_PackingDate();
			AssertNoErrors(tempLine.WU_PackingDateInfo);

			validationForTempStocktakeLine.ValidateWU_PalletID();
			AssertNoErrors(tempLine.WU_PalletIDInfo);

			validationForTempStocktakeLine.ValidateWU_PartAttrib1();
			AssertNoErrors(tempLine.WU_PartAttrib1Info);

			validationForTempStocktakeLine.ValidateWU_PartAttrib2();
			AssertNoErrors(tempLine.WU_PartAttrib2Info);

			validationForTempStocktakeLine.ValidateWU_PartAttrib3();
			AssertNoErrors(tempLine.WU_PartAttrib3Info);

			validationForTempStocktakeLine.ValidateWU_Status();
			AssertNoErrors(tempLine.WU_StatusInfo);

			validationForTempStocktakeLine.ValidateWU_SystemUnits();
			AssertNoErrors(tempLine.WU_SystemUnitsInfo);

			validationForTempStocktakeLine.ValidateWU_WL();
			AssertNoErrors(tempLine.WU_WLInfo);

			validationForTempStocktakeLine.ValidateWU_WS();
			AssertNoErrors(tempLine.WU_WSInfo);
		}

		#endregion

		#endregion

		#region Test CheckWU_LastCount

		#region TestCheckWU_LastCount_DoesNotErrorWhenSerialAttributeIsReleaseCaptured

		public void TestCheckWU_LastCount_DoesNotErrorWhenSerialAttributeIsReleaseCaptured()
		{
			AssertCountDoesNotErrorWhenSerialAttributeIsReleaseCaptured(1, WhsStocktakeLineSchema.WU_LastCount, line => line.WU_LastCountInfo);
		}

		#endregion

		#region TestCheckWU_LastCount

		public void TestCheckWU_LastCount()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2m, new ZByte(1), StocktakeLineStatus.Codes.Open);

			AssertNoError(line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			data.Org1.PartAttributeManager.SetProductToUseAttribute(data.Part1, 6, true);
			data.Org1.MiscServ.OM_IMUseSerialNumber = true;
			line.WU_LastCount = 2m;
			AssertHasError(line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line.WU_LastCount = 1m;
			AssertNoError(line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line.WU_LastCount = -1m;
			AssertNoError(line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line.WU_LastCount = 0.6d;
			AssertNoError(line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
		}

		#endregion

		#region TestCheckWU_LastCount_AttributeNeutralProducts

		public void TestCheckWU_LastCount_AttributeNeutralProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0m, new ZByte(1), StocktakeLineStatus.Codes.Open);

			AssertNoError("Pre-condition", line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			data.Org1.PartAttributeManager.SetProductToUseAttribute(data.Part1, 6, true);
			data.Org1.MiscServ.OM_IMUseSerialNumber = true;
			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			line.WU_LastCount = 1m;
			AssertNoError(line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line.WU_LastCount = 2m;
			AssertNoError(line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;

			line.WU_LastCount = 1m;
			AssertNoError(line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line.WU_LastCount = 2m;
			AssertHasError(line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
		}

		#endregion

		#region TestCheckWU_LastCount_NonNegative

		public void TestCheckWU_LastCount_NonNegative()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, -1m, new ZByte(1), StocktakeLineStatus.Codes.Open, false);

			AssertHasErrors("Should be a non-negative value.", line.WU_LastCountInfo);

			line.WU_LastCount = 1;
			AssertNoErrors("Should no errors.", line.WU_LastCountInfo);
		}

		#endregion

		#region TestCheckWU_LastCount_IsDivisibleByPerPackageQty

		public void TestCheckWU_LastCount_IsDivisibleByPerPackageQty()
		{
			AssertCountIsDivisibleByPerPackageQty(WhsStocktakeLineSchema.WU_LastCount, 1, line => line.WU_LastCountInfo);
		}

		#endregion

		#region TestCheckWU_LastCount_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupID

		public void TestCheckWU_LastCount_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupID()
		{
			AssertCount_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupID(WhsStocktakeLineSchema.WU_LastCount, 1, line => line.WU_LastCountInfo);
		}

		#endregion

		#region TestCheckWU_LastCount_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupIDPerLocation

		public void TestCheckWU_LastCount_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupIDPerLocation()
		{
			AssertCount_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupIDPerLocation(WhsStocktakeLineSchema.WU_LastCount, 1, line => line.WU_LastCountInfo);
		}

		#endregion

		#endregion

		#region Test CheckWU_Count2

		#region TestCheckWU_Count2_DoesNotErrorWhenSerialAttributeIsReleaseCaptured

		public void TestCheckWU_Count2_DoesNotErrorWhenSerialAttributeIsReleaseCaptured()
		{
			AssertCountDoesNotErrorWhenSerialAttributeIsReleaseCaptured(2, WhsStocktakeLineSchema.WU_Count2, line => line.WU_Count2Info);
		}

		#endregion

		#region TestCheckWU_Count2

		public void TestCheckWU_Count2()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2m, new ZByte(2), StocktakeLineStatus.Codes.Open);

			AssertNoError(line.WU_Count2Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			data.Org1.PartAttributeManager.SetProductToUseAttribute(data.Part1, 6, true);
			data.Org1.MiscServ.OM_IMUseSerialNumber = true;
			line.WU_Count2 = 2m;
			AssertHasError(line.WU_Count2Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line.WU_Count2 = 1m;
			AssertNoError(line.WU_Count2Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line.WU_Count2 = -1m;
			AssertNoError(line.WU_Count2Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line.WU_Count2 = 0.6d;
			AssertNoError(line.WU_Count2Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
		}

		#endregion

		#region TestCheckWU_Count2_AttributeNeutralProducts

		public void TestCheckWU_Count2_AttributeNeutralProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2m, new ZByte(2), StocktakeLineStatus.Codes.Open);

			AssertNoError("Pre-condition", line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			data.Org1.PartAttributeManager.SetProductToUseAttribute(data.Part1, 6, true);
			data.Org1.MiscServ.OM_IMUseSerialNumber = true;
			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			line.WU_Count2 = 1m;
			AssertNoError(line.WU_Count2Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line.WU_Count2 = 2m;
			AssertNoError(line.WU_Count2Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;

			line.WU_Count2 = 1m;
			AssertNoError(line.WU_Count2Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line.WU_Count2 = 2m;
			AssertHasError(line.WU_Count2Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
		}

		#endregion

		#region TestCheckWU_Count2_NonNegative

		public void TestCheckWU_Count2_NonNegative()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, -1m, new ZByte(2), StocktakeLineStatus.Codes.Open);

			AssertHasErrors("Should be a non-negative value.", line.WU_Count2Info);

			line.WU_Count2 = 1;
			AssertNoErrors("Should no errors.", line.WU_Count2Info);
		}

		#endregion

		#region TestCheckWU_Count2_IsDivisibleByPerPackageQty

		public void TestCheckWU_Count2_IsDivisibleByPerPackageQty()
		{
			AssertCountIsDivisibleByPerPackageQty(WhsStocktakeLineSchema.WU_Count2, 2, line => line.WU_Count2Info);
		}

		#endregion

		#region TestCheckWU_Count2_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupID

		public void TestCheckWU_Count2_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupID()
		{
			AssertCount_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupID(WhsStocktakeLineSchema.WU_Count2, 2, line => line.WU_Count2Info);
		}

		#endregion

		#region TestCheckWU_Count2_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupIDPerLocation

		public void TestCheckWU_Count2_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupIDPerLocation()
		{
			AssertCount_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupIDPerLocation(WhsStocktakeLineSchema.WU_Count2, 2, line => line.WU_Count2Info);
		}

		#endregion

		#endregion

		#region Test CheckWU_Count3

		#region TestCheckWU_Count3_DoesNotErrorWhenSerialAttributeIsReleaseCaptured

		public void TestCheckWU_Count3_DoesNotErrorWhenSerialAttributeIsReleaseCaptured()
		{
			AssertCountDoesNotErrorWhenSerialAttributeIsReleaseCaptured(3, WhsStocktakeLineSchema.WU_Count3, line => line.WU_Count3Info);
		}

		#endregion

		#region TestCheckWU_Count3

		public void TestCheckWU_Count3()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2m, new ZByte(3), StocktakeLineStatus.Codes.Open);

			AssertNoError(line.WU_Count3Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			data.Org1.PartAttributeManager.SetProductToUseAttribute(data.Part1, 6, true);
			data.Org1.MiscServ.OM_IMUseSerialNumber = true;
			line.WU_Count3 = 2m;
			AssertHasError(line.WU_Count3Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line.WU_Count3 = 1m;
			AssertNoError(line.WU_Count3Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line.WU_Count3 = -1m;
			AssertNoError(line.WU_Count3Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line.WU_Count3 = 0.6d;
			AssertNoError(line.WU_Count3Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
		}

		#endregion

		#region TestCheckWU_Count3_AttributeNeutralProducts

		public void TestCheckWU_Count3_AttributeNeutralProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2m, new ZByte(3), StocktakeLineStatus.Codes.Open);

			AssertNoError("Pre-condition", line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			data.Org1.PartAttributeManager.SetProductToUseAttribute(data.Part1, 6, true);
			data.Org1.MiscServ.OM_IMUseSerialNumber = true;
			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			line.WU_Count3 = 1m;
			AssertNoError(line.WU_Count3Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line.WU_Count3 = 2m;
			AssertNoError(line.WU_Count3Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;

			line.WU_Count3 = 1m;
			AssertNoError(line.WU_Count3Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line.WU_Count3 = 2m;
			AssertHasError(line.WU_Count3Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
		}

		#endregion

		#region TestCheckWU_Count3_NonNegative

		public void TestCheckWU_Count3_NonNegative()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, -1m, new ZByte(3), StocktakeLineStatus.Codes.Open);

			AssertHasErrors("Should be a non-negative value.", line.WU_Count3Info);

			line.WU_Count3 = 1;
			AssertNoErrors("Should no errors.", line.WU_Count3Info);
		}

		#endregion

		#region TestCheckWU_Count3_IsDivisibleByPerPackageQty

		public void TestCheckWU_Count3_IsDivisibleByPerPackageQty()
		{
			AssertCountIsDivisibleByPerPackageQty(WhsStocktakeLineSchema.WU_Count3, 3, line => line.WU_Count3Info);
		}

		#endregion

		#region TestCheckWU_Count3_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupID

		public void TestCheckWU_Count3_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupID()
		{
			AssertCount_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupID(WhsStocktakeLineSchema.WU_Count3, 3, line => line.WU_Count3Info);
		}

		#endregion

		#region TestCheckWU_Count3_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupIDPerLocation

		public void TestCheckWU_Count3_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupIDPerLocation()
		{
			AssertCount_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupIDPerLocation(WhsStocktakeLineSchema.WU_Count3, 3, line => line.WU_Count3Info);
		}

		#endregion

		#endregion

		#region TestCheckWU_EmptyLine_Count

		public void TestCheckWU_EmptyLine_Count()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, null, null, data.Whs1.DefaultLocation, 0m, new ZByte(1), StocktakeLineStatus.Codes.Open);
			line.WU_InventoryStatus = "EMP";

			AssertEmptyLineCount(line, line.WU_LastCountInfo, (value) =>
			{
				line.WU_LastCount = value;
				line.Validation.ValidateWU_LastCount();
			});

			AssertEmptyLineCount(line, line.WU_Count2Info, (value) =>
			{
				line.WU_Count2 = value;
				line.Validation.ValidateWU_Count2();
			});

			AssertEmptyLineCount(line, line.WU_Count3Info, (value) =>
			{
				line.WU_Count3 = value;
				line.Validation.ValidateWU_Count3();
			});
		}

		void AssertEmptyLineCount(WhsStocktakeLine stocktakeline, ZPropertyInfo colInfo, Action<decimal> setValueAndRunValidation)
		{
			//var column = stocktakeline[colInfo.Name];
			AssertNoError(colInfo, WhsStocktakeLineValidation.CountShouldBeZeroForEmptyLocation);

			setValueAndRunValidation(2m);
			AssertHasError(colInfo, WhsStocktakeLineValidation.CountShouldBeZeroForEmptyLocation);

			setValueAndRunValidation(1m);
			AssertHasError(colInfo, WhsStocktakeLineValidation.CountShouldBeZeroForEmptyLocation);

			setValueAndRunValidation(-1m);
			AssertHasError(colInfo, WhsStocktakeLineValidation.CountShouldBeZeroForEmptyLocation);

			setValueAndRunValidation(0.6m);
			AssertHasError(colInfo, WhsStocktakeLineValidation.CountShouldBeZeroForEmptyLocation);
		}

		#endregion

		#region TestCheckWU_CurrentCount

		#region LocationOverflow

		#region LocationCapacityOverflow

		public void TestLocationOverflow_LocationCapacityExceeded_RFEnvironment()
		{
			WhsEnvironment.IsRF = true;
			// Stocktake overflow message should not change if WhsEnvironemnt is RF
			AssertLocationOverflow_LocationPropertyExceeded(WhsLocationViewSchema.WLV_MaxQuantity, WhsLocationViewSchema.WLV_MaxQuantityUnit, Core.Constants.PkgUnit.Unit, 100, 1, 70, WhsStocktakeLineSchema.WU_LastCount, 110);
		}

		public void TestLocationOverflow_LocationCapacityExceededInOneStocktakeLine_Count1()
		{
			AssertLocationOverflow_LocationPropertyExceeded(WhsLocationViewSchema.WLV_MaxQuantity, WhsLocationViewSchema.WLV_MaxQuantityUnit, Core.Constants.PkgUnit.Unit, 100, 1, 70, WhsStocktakeLineSchema.WU_LastCount, 110);
		}

		public void TestLocationOverflow_LocationCapacityExceededInSumOfStocktakeLines_Count1()
		{
			AssertLocationOverflow_LocationPropertyExceeded(WhsLocationViewSchema.WLV_MaxQuantity, WhsLocationViewSchema.WLV_MaxQuantityUnit, Core.Constants.PkgUnit.Unit, 100, 2, 30, WhsStocktakeLineSchema.WU_LastCount, 60);
		}

		public void TestLocationOverflow_LocationCapacityExceededInOneStocktakeLine_Count2()
		{
			AssertLocationOverflow_LocationPropertyExceeded(WhsLocationViewSchema.WLV_MaxQuantity, WhsLocationViewSchema.WLV_MaxQuantityUnit, Core.Constants.PkgUnit.Unit, 100, 1, 70, WhsStocktakeLineSchema.WU_Count2, 110);
		}

		public void TestLocationOverflow_LocationCapacityExceededInSumOfStocktakeLines_Count2()
		{
			AssertLocationOverflow_LocationPropertyExceeded(WhsLocationViewSchema.WLV_MaxQuantity, WhsLocationViewSchema.WLV_MaxQuantityUnit, Core.Constants.PkgUnit.Unit, 100, 2, 30, WhsStocktakeLineSchema.WU_Count2, 60);
		}

		public void TestLocationOverflow_LocationCapacityExceededInOneStocktakeLine_Count3()
		{
			AssertLocationOverflow_LocationPropertyExceeded(WhsLocationViewSchema.WLV_MaxQuantity, WhsLocationViewSchema.WLV_MaxQuantityUnit, Core.Constants.PkgUnit.Unit, 100, 1, 70, WhsStocktakeLineSchema.WU_Count3, 110);
		}

		public void TestLocationOverflow_LocationCapacityExceededInSumOfStocktakeLines_Count3()
		{
			AssertLocationOverflow_LocationPropertyExceeded(WhsLocationViewSchema.WLV_MaxQuantity, WhsLocationViewSchema.WLV_MaxQuantityUnit, Core.Constants.PkgUnit.Unit, 100, 2, 30, WhsStocktakeLineSchema.WU_Count3, 60);
		}

		public void TestLocationOverflow_LocationCapacityFullStocktakeShouldNotOverFlow()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var locA1 = data.Whs1.FindLocation("A");
			locA1.WLV_MaxQuantity = 100m;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 60m, locA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 40m, locA1, "");
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, locA1);
			Factory.Save();

			var line1 = stocktake.Lines.AddNew();
			var line2 = stocktake.Lines.AddNew();

			line1.WU_OP = data.Part1.PK;
			line1.WU_WL = locA1.PK;
			line1.WU_SystemUnits = 60m;

			line2.WU_OP = data.Part2.PK;
			line2.WU_WL = locA1.PK;
			line2.WU_SystemUnits = 40m;

			Factory.Save();
			AssertNoErrors("Precondition", line1.WU_LastCountInfo);
			AssertNoErrors("Precondition", line2.WU_LastCountInfo);

			line1.WU_LastCount = 50m;
			line2.WU_LastCount = 50m;

			stocktake.CloseLines(stocktake.Lines.ToArray());
			AssertEquals("Precondition: stocktake has one adjustment", 1, stocktake.Adjustments.Count);
			stocktake.Adjustments[0].RunPreSaveValidation(); //creates picklines and allocates stock

			AssertNoErrors("Stocktake should not cause a location overflow error.", line1.WU_LastCountInfo);
			AssertNoErrors("Stocktake should not cause a location overflow error.", line2.WU_LastCountInfo);
			AssertNoExceptionThrown("There should be no save errors.", Factory.Save);
		}

		public void TestLocationOverflow_LocationCapacityExceededInSumOfStocktakeLinesAcrossTwoCounts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var locA1 = data.Whs1.FindLocation("A");
			locA1.WLV_MaxQuantity = 115m;
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m, locA1, "a");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 30m, locA1, "a");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 30m, locA1, "a");
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, locA1);
			Factory.Save();

			var line1 = stocktake.Lines.AddNew();
			var line2 = stocktake.Lines.AddNew();
			var line3 = stocktake.Lines.AddNew();

			line1.WU_OP = data.Part1.PK;
			line1.WU_WL = locA1.PK;
			line1.WU_SystemUnits = 30m;
			line1.WU_LastCount = 40m;

			line2.WU_OP = data.Part2.PK;
			line2.WU_WL = locA1.PK;
			line2.WU_SystemUnits = 30m;
			line2.WU_Count2 = 40m;

			line3.WU_OP = part3.PK;
			line3.WU_WL = locA1.PK;
			line3.WU_SystemUnits = 30m;
			line3.WU_Count2 = 40m;

			AssertEquals("Precondition: line has one total count.", (ZByte)1, line2.WU_TotalCounts);
			AssertEquals("Precondition: line has one total count.", (ZByte)1, line3.WU_TotalCounts);

			stocktake.CloseLines(new[] { line1 });
			Assert("Precondition: Line should be closed.", line1.IsClosed);

			line2.WU_TotalCounts = 2;
			line3.WU_TotalCounts = 2;

			Factory.Save();
			stocktake.CloseLines(new[] { line2, line3 });

			Assert("Line should not be closed.", !line2.IsClosed);
			Assert("Line should not be closed.", !line3.IsClosed);
			AssertHasError("Line should show location overflow error.", line2.WU_Count2Info, "The Stocktake quantity exceeds the maximum available quantity for location A by 5.000 unit(s).");
			AssertHasError("Line should show location overflow error.", line3.WU_Count2Info, "The Stocktake quantity exceeds the maximum available quantity for location A by 5.000 unit(s).");
			AssertNoExceptionThrown("Location overflow should be validated pre-save.", Factory.Save);
			stocktake.RunPreSaveValidation();
			Assert("Closed lines should not have overflow validated.", !line1.HasErrors);
		}

		public void TestLocationOverflow_LocationCapacityExceededInSumOfStocktakeLinesAcrossThreeCounts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var locA1 = data.Whs1.FindLocation("A");
			locA1.WLV_MaxQuantity = 115m;
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m, locA1, "a");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 30m, locA1, "a");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 30m, locA1, "a");
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, locA1);
			Factory.Save();

			var line1 = stocktake.Lines.AddNew();
			var line2 = stocktake.Lines.AddNew();
			var line3 = stocktake.Lines.AddNew();

			line1.WU_OP = data.Part1.PK;
			line1.WU_WL = locA1.PK;
			line1.WU_SystemUnits = 30m;
			line1.WU_LastCount = 40m;

			line2.WU_OP = data.Part2.PK;
			line2.WU_WL = locA1.PK;
			line2.WU_SystemUnits = 30m;
			line2.WU_Count2 = 40m;

			line3.WU_OP = part3.PK;
			line3.WU_WL = locA1.PK;
			line3.WU_SystemUnits = 30m;
			line3.WU_Count3 = 40m;

			AssertEquals("Precondition: line has one total count.", (ZByte)1, line2.WU_TotalCounts);
			AssertEquals("Precondition: line has one total count.", (ZByte)1, line3.WU_TotalCounts);

			stocktake.CloseLines(new[] { line1 });
			Assert("Precondition: Line should be closed.", line1.IsClosed);

			line2.WU_TotalCounts = 2;

			stocktake.CloseLines(new[] { line2 });
			Assert("Precondition: Line should be closed.", line2.IsClosed);

			line3.WU_TotalCounts = 3;

			Factory.Save();
			stocktake.CloseLines(new[] { line3 });

			Assert("Line should not be closed.", !line3.IsClosed);
			AssertHasError("Line should show location overflow error.", line3.WU_Count3Info, "The Stocktake quantity exceeds the maximum available quantity for location A by 5.000 unit(s).");
			AssertNoExceptionThrown("Location overflow should be validated pre-save.", Factory.Save);
			stocktake.RunPreSaveValidation();
			Assert("Closed lines should not have overflow validated.", !line1.HasErrors);
			Assert("Closed lines should not have overflow validated.", !line2.HasErrors);
		}

		public void TestLocationOverflow_LocationCapacityExceededWithConcurrentInventoryChange()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var locA1 = data.Whs1.FindLocation("A");
			locA1.WLV_MaxQuantity = 100m;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 60m, locA1, "a");

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, locA1);
			Factory.Save();

			var line1 = stocktake.Lines.AddNew();
			line1.WU_OP = data.Part1.PK;
			line1.WU_WL = locA1.PK;
			line1.WU_SystemUnits = 60m;
			line1.WU_LastCount = 90m;
			Factory.Save();
			AssertNoErrors("Precondition: Line should not cause overflow.", line1.WU_LastCountInfo);

			// New receive that causes location capacity to be exceeded by the stocktake line.
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 30m, locA1, "a");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var stocktakeInNewFactory = newFactory.Load<WhsStocktake>(stocktake.PK);
			var line1InNewFactory = stocktakeInNewFactory.Lines.Single();

			stocktakeInNewFactory.CloseLines(new[] { line1InNewFactory });
			AssertHasError("Line should show location overflow error.", line1InNewFactory.WU_LastCountInfo, "The Stocktake quantity exceeds the maximum available quantity for location A by 20.000 unit(s).");
			AssertNoExceptionThrown("Location overflow should be validated pre-save.", Factory.Save);
		}

		public void TestLocationOverflow_LocationCapacityNotExceeded_NoLocationInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var locA1 = data.Whs1.FindLocation("A");
			locA1.WLV_MaxQuantity = 100m;
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, locA1);
			Factory.Save();

			var line1 = stocktake.Lines.AddNew();
			line1.WU_OP = data.Part1.PK;
			line1.WU_WL = locA1.PK;
			line1.WU_SystemUnits = 0m;
			line1.WU_LastCount = 90m;
			Factory.Save();

			stocktake.CloseLines(new[] { line1 });
			AssertNoErrors("Line should not cause location overflow error.", line1.WU_LastCountInfo);
			AssertNoExceptionThrown("There should be no save errors.", Factory.Save);
		}

		public void TestLocationOverflow_LocationCapacityExceeded_MultipleLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locA1 = data.Whs1.FindLocation("A-1-1");
			var locA2 = data.Whs1.FindLocation("A-2-1");
			locA1.WLV_MaxQuantity = 100m;
			locA2.WLV_MaxQuantity = 100m;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 60m, locA1, "a");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 60m, locA2, "b");

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			Factory.Save();

			var line1 = stocktake.Lines.AddNew();
			line1.WU_OP = data.Part1.PK;
			line1.WU_WL = locA1.PK;
			line1.WU_SystemUnits = 60m;
			line1.WU_LastCount = 90m;
			Factory.Save();
			AssertNoErrors("Precondition: Line should not cause overflow.", line1.WU_LastCountInfo);

			var line2 = stocktake.Lines.AddNew();
			line2.WU_OP = data.Part1.PK;
			line2.WU_WL = locA2.PK;
			line2.WU_SystemUnits = 60m;
			line2.WU_LastCount = 90m;
			Factory.Save();

			// Need to assert both lines to ensure overfill count only counts one location
			AssertNoErrors("Precondition: Line should not cause overflow.", line1.WU_LastCountInfo);
			AssertNoErrors("Precondition: Line should not cause overflow.", line2.WU_LastCountInfo);

			line1.WU_LastCount = 110m;
			AssertHasError("Line should show location overflow error.", line1.WU_LastCountInfo, "The Stocktake quantity exceeds the maximum available quantity for location A-1-1 by 10.000 unit(s).");
			AssertNoErrors("Line should not be affected by other locations.", line2.WU_LastCountInfo);

			line2.WU_LastCount = 110m;
			AssertHasError("Overflow error should not be affected by other locations.", line1.WU_LastCountInfo, "The Stocktake quantity exceeds the maximum available quantity for location A-1-1 by 10.000 unit(s).");
			AssertHasError("Line should show location overflow error.", line2.WU_LastCountInfo, "The Stocktake quantity exceeds the maximum available quantity for location A-2-1 by 10.000 unit(s).");

			AssertNoExceptionThrown("Location overflow should be validated pre-save.", Factory.Save);
		}

		public void TestLocationOverflow_LocationCapacityExceeded_OverflowQtyUpdated()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var locA1 = data.Whs1.FindLocation("A");
			locA1.WLV_MaxQuantity = 200m;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 60m, locA1, "a");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 60m, locA1, "a");

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			Factory.Save();

			var line1 = stocktake.Lines.AddNew();
			line1.WU_OP = data.Part1.PK;
			line1.WU_WL = locA1.PK;
			line1.WU_SystemUnits = 60m;
			line1.WU_LastCount = 90m;
			Factory.Save();
			AssertNoErrors("Precondition: Line should not cause overflow.", line1.WU_LastCountInfo);

			var line2 = stocktake.Lines.AddNew();
			line2.WU_OP = data.Part1.PK;
			line2.WU_WL = locA1.PK;
			line2.WU_SystemUnits = 60m;
			line2.WU_LastCount = 90m;
			Factory.Save();

			// Need to assert both lines to ensure overfill count only counts one location
			AssertNoErrors("Precondition: Line should not cause overflow.", line1.WU_LastCountInfo);
			AssertNoErrors("Precondition: Line should not cause overflow.", line2.WU_LastCountInfo);

			line1.WU_LastCount = 120m;
			AssertHasError("Line should show location overflow error.", line1.WU_LastCountInfo, "The Stocktake quantity exceeds the maximum available quantity for location A by 10.000 unit(s).");

			line2.WU_LastCount = 120m;
			AssertHasError("Overflow error should include change from other line.", line2.WU_LastCountInfo, "The Stocktake quantity exceeds the maximum available quantity for location A by 40.000 unit(s).");

			line1.WU_LastCount = 110m;
			AssertHasError("Overflow error should include change from other line.", line1.WU_LastCountInfo, "The Stocktake quantity exceeds the maximum available quantity for location A by 30.000 unit(s).");
		}

		#endregion

		#region WeightOverflow

		public void TestLocationOverflow_LocationWeightExceededInOneStocktakeLine_Count1()
		{
			AssertLocationOverflow_LocationPropertyExceeded(WhsLocationViewSchema.WLV_MaxWeight, WhsLocationViewSchema.WLV_MaxWeightUnit, Core.Constants.Weight.Kilograms, 200, 1, 70, WhsStocktakeLineSchema.WU_LastCount, 110);
		}

		public void TestLocationOverflow_LocationWeightExceededInSumOfStocktakeLines_Count1()
		{
			AssertLocationOverflow_LocationPropertyExceeded(WhsLocationViewSchema.WLV_MaxWeight, WhsLocationViewSchema.WLV_MaxWeightUnit, Core.Constants.Weight.Kilograms, 200, 2, 30, WhsStocktakeLineSchema.WU_LastCount, 60);
		}

		public void TestLocationOverflow_LocationWeightExceededInOneStocktakeLine_Count2()
		{
			AssertLocationOverflow_LocationPropertyExceeded(WhsLocationViewSchema.WLV_MaxWeight, WhsLocationViewSchema.WLV_MaxWeightUnit, Core.Constants.Weight.Kilograms, 200, 1, 70, WhsStocktakeLineSchema.WU_Count2, 110);
		}

		public void TestLocationOverflow_LocationWeightExceededInSumOfStocktakeLines_Count2()
		{
			AssertLocationOverflow_LocationPropertyExceeded(WhsLocationViewSchema.WLV_MaxWeight, WhsLocationViewSchema.WLV_MaxWeightUnit, Core.Constants.Weight.Kilograms, 200, 2, 30, WhsStocktakeLineSchema.WU_Count2, 60);
		}

		public void TestLocationOverflow_LocationWeightExceededInOneStocktakeLine_Count3()
		{
			AssertLocationOverflow_LocationPropertyExceeded(WhsLocationViewSchema.WLV_MaxWeight, WhsLocationViewSchema.WLV_MaxWeightUnit, Core.Constants.Weight.Kilograms, 200, 1, 70, WhsStocktakeLineSchema.WU_Count3, 110);
		}

		public void TestLocationOverflow_LocationWeightExceededInSumOfStocktakeLines_Count3()
		{
			AssertLocationOverflow_LocationPropertyExceeded(WhsLocationViewSchema.WLV_MaxWeight, WhsLocationViewSchema.WLV_MaxWeightUnit, Core.Constants.Weight.Kilograms, 200, 2, 30, WhsStocktakeLineSchema.WU_Count3, 60);
		}

		#endregion

		#region VolumeOverflow

		public void TestLocationOverflow_LocationVolumeExceededInOneStocktakeLine_Count1()
		{
			AssertLocationOverflow_LocationPropertyExceeded(WhsLocationViewSchema.WLV_MaxCubic, WhsLocationViewSchema.WLV_MaxCubicUnit, Core.Constants.Volume.CubicMetres, 1.5, 1, 70, WhsStocktakeLineSchema.WU_LastCount, 110);
		}

		public void TestLocationOverflow_LocationVolumeExceededInSumOfStocktakeLines_Count1()
		{
			AssertLocationOverflow_LocationPropertyExceeded(WhsLocationViewSchema.WLV_MaxCubic, WhsLocationViewSchema.WLV_MaxCubicUnit, Core.Constants.Volume.CubicMetres, 1.5, 2, 30, WhsStocktakeLineSchema.WU_LastCount, 60);
		}

		public void TestLocationOverflow_LocationVolumeExceededInOneStocktakeLine_Count2()
		{
			AssertLocationOverflow_LocationPropertyExceeded(WhsLocationViewSchema.WLV_MaxCubic, WhsLocationViewSchema.WLV_MaxCubicUnit, Core.Constants.Volume.CubicMetres, 1.5, 1, 70, WhsStocktakeLineSchema.WU_Count2, 110);
		}

		public void TestLocationOverflow_LocationVolumeExceededInSumOfStocktakeLines_Count2()
		{
			AssertLocationOverflow_LocationPropertyExceeded(WhsLocationViewSchema.WLV_MaxCubic, WhsLocationViewSchema.WLV_MaxCubicUnit, Core.Constants.Volume.CubicMetres, 1.5, 2, 30, WhsStocktakeLineSchema.WU_Count2, 60);
		}

		public void TestLocationOverflow_LocationVolumeExceededInOneStocktakeLine_Count3()
		{
			AssertLocationOverflow_LocationPropertyExceeded(WhsLocationViewSchema.WLV_MaxCubic, WhsLocationViewSchema.WLV_MaxCubicUnit, Core.Constants.Volume.CubicMetres, 1.5, 1, 70, WhsStocktakeLineSchema.WU_Count3, 110);
		}

		public void TestLocationOverflow_LocationVolumeExceededInSumOfStocktakeLines_Count3()
		{
			AssertLocationOverflow_LocationPropertyExceeded(WhsLocationViewSchema.WLV_MaxCubic, WhsLocationViewSchema.WLV_MaxCubicUnit, Core.Constants.Volume.CubicMetres, 1.5, 2, 30, WhsStocktakeLineSchema.WU_Count3, 60);
		}

		#endregion

		#region OverflowShared

		void AssertLocationOverflow_LocationPropertyExceeded(SchemaColumn propertyExceededColumn, SchemaColumn proprtyExceededUnitsColumn, ZString propertyExceededUnits, ZDecimal locationPropertyLimit, int receivesToCreate, ZDecimal receiveQty, SchemaColumn countColumn, ZDecimal stocktakeLineQty)
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var locA1 = data.Whs1.FindLocation("A");
			locA1[propertyExceededColumn] = locationPropertyLimit;
			locA1[proprtyExceededUnitsColumn] = propertyExceededUnits;
			Factory.Save();

			var parts = new List<OrgSupplierPart>();
			for (int i = 1; i <= receivesToCreate; i++)
			{
				var part = Helper.CreateProduct(data.Org1, "PR" + i);
				parts.Add(part);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R" + i, part, receiveQty, locA1, "a");
			}

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, locA1);
			Factory.Save();

			foreach (var part in parts)
			{
				var line = stocktake.Lines.AddNew();
				line.WU_OP = part.PK;
				line.WU_WL = locA1.PK;
				line.WU_SystemUnits = receiveQty;
				line[countColumn] = stocktakeLineQty;

				if (countColumn == WhsStocktakeLineSchema.WU_Count2)
				{
					line.WU_TotalCounts = 2;
				}
				else if (countColumn == WhsStocktakeLineSchema.WU_Count3)
				{
					line.WU_TotalCounts = 3;
				}
			}

			Factory.Save();
			stocktake.CloseLines(stocktake.Lines.ToArray());

			foreach (var line in stocktake.Lines)
			{
				ZPropertyInfo errorPropertyInfo;
				if (countColumn == WhsStocktakeLineSchema.WU_LastCount)
				{
					errorPropertyInfo = line.WU_LastCountInfo;
				}
				else if (countColumn == WhsStocktakeLineSchema.WU_Count2)
				{
					errorPropertyInfo = line.WU_Count2Info;
				}
				else
				{
					errorPropertyInfo = line.WU_Count3Info;
				}
				if (propertyExceededColumn == WhsLocationViewSchema.WLV_MaxQuantity)
				{
					var expectedDifference = ((ZDecimal)((stocktakeLineQty * receivesToCreate) - locationPropertyLimit)).ToString(3);
					AssertHasError("Line(s) should show an overflow error.", errorPropertyInfo, $@"The Stocktake quantity exceeds the maximum available quantity for location {locA1.WLV_LocationString} by {expectedDifference} unit(s).");
				}
				else if (propertyExceededColumn == WhsLocationViewSchema.WLV_MaxWeight)
				{
					var expectedDifference = ((ZDecimal)((stocktakeLineQty * receivesToCreate) * 2 - locationPropertyLimit)).ToString(2);
					AssertHasWarning("Line(s) should show an overflow warning.", errorPropertyInfo, $@"The Stocktake weight exceeds the maximum available weight for location {locA1.WLV_LocationString} by {expectedDifference} {propertyExceededUnits}.");
				}
				else
				{
					var expectedDifference = ((ZDecimal)((stocktakeLineQty * receivesToCreate) * 0.02m - locationPropertyLimit)).ToString(3);
					AssertHasWarning("Line(s) should show an overflow warning.", errorPropertyInfo, $@"The Stocktake volume exceeds the maximum available volume for location {locA1.WLV_LocationString} by {expectedDifference} {propertyExceededUnits}.");
				}
			}
			AssertNoExceptionThrown("Location overflow should be validated pre-save.", Factory.Save);
		}

		#endregion

		#endregion

		#endregion

		#region TestGetLocationAvailableCapacityCache

		public void TestGetLocationAvailalbeCapacityCache()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var locA1 = data.Whs1.FindLocation("A");
			locA1.WLV_MaxQuantity = 100m;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 60m, locA1, "a");

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, locA1);
			Factory.Save();

			var line1 = stocktake.Lines.AddNew();
			line1.WU_OP = data.Part1.PK;
			line1.WU_WL = locA1.PK;
			line1.WU_SystemUnits = 60m;
			line1.WU_LastCount = 90m;
			Factory.Save();

			int dbHits = Factory.DatabaseLoadCount;

			for (int i = 0; i < 10; i++)
			{
				line1.WU_LastCount = i;
			}

			AssertEquals("Too many DB hits.", 0, Factory.DatabaseLoadCount - dbHits);
		}

		#endregion

		#region TestCheckWU_SerialNumber

		public void TestCheckWU_SerialNumber_QtyGreaterThanOne_LastCount()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1m, new ZByte(1), StocktakeLineStatus.Codes.Open);
			line.WU_LastCount = 1m;
			line.WU_SerialNumber = "TSDWE";

			AssertNoErrors("Precondition: Line with Serial Number and 1 Qty has no error", line.WU_LastCountInfo);

			line.WU_LastCount = 15m;
			AssertHasError("Should error for serial Number + 15 qty", line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			line.WU_SerialNumber = "";
			AssertNoErrors("Line with no Serial Number and 15 Qty has no error", line.WU_LastCountInfo);
		}

		public void TestCheckWU_SerialNumber_QtyGreaterThanOne_Count2()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1m, new ZByte(2), StocktakeLineStatus.Codes.Open);
			line.WU_Count2 = 1m;
			line.WU_SerialNumber = "TSDWE";

			AssertNoErrors("Precondition: Line with Serial Number and 1 Qty has no error", line.WU_Count2Info);

			line.WU_Count2 = 15m;
			AssertHasError("Should error for serial Number + 15 qty", line.WU_Count2Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			line.WU_SerialNumber = "";
			AssertNoErrors("Line with no Serial Number and 15 Qty has no error", line.WU_Count2Info);
		}

		public void TestCheckWU_SerialNumber_QtyGreaterThanOne_Count3()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1m, new ZByte(3), StocktakeLineStatus.Codes.Open);
			line.WU_Count3 = 1m;
			line.WU_SerialNumber = "TSDWE";

			AssertNoErrors("Precondition: Line with Serial Number and 1 Qty has no error", line.WU_Count3Info);

			line.WU_Count3 = 15m;
			AssertHasError("Should error for serial Number + 15 qty", line.WU_Count3Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			line.WU_SerialNumber = "";
			AssertNoErrors("Line with no Serial Number and 15 Qty has no error", line.WU_Count3Info);
		}

		#endregion

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("C1", "Login1");
			var inactiveStaff = Helper.CreateGlbStaff("CA1", "Login2", false);

			Factory.Save();

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1m, 1, StocktakeLineStatus.Codes.Open);

			var validation = new TestWhsStocktakeLineValidation(line);

			var list = new string[]
			{
				WhsStocktakeLineSchema.Constants.WU_WL,
				WhsStocktakeLineSchema.Constants.WU_WS,
				WhsStocktakeLineSchema.Constants.WU_F3_NKPackType
			};

			foreach (var propertyInfo in line.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (list.Contains(propertyInfo.Name))
				{
					AssertEquals("NK/FK which cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
				else
				{
					AssertEquals("All other properties should just return base condition of true.", true, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
			}
		}

		#endregion

		#region TestWhsStocktakeLineValidation

		class TestWhsStocktakeLineValidation : WhsStocktakeLineValidation
		{
			public TestWhsStocktakeLineValidation(WhsStocktakeLine parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion

		#region Implementation

		void AssertCountDoesNotErrorWhenSerialAttributeIsReleaseCaptured(ZByte currentCount, SchemaColumn countColumn, Func<WhsStocktakeLine, ZPropertyInfo> getPropertyInfo)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktake and line
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1m, currentCount, StocktakeLineStatus.Codes.Open);
			var propertyInfo = getPropertyInfo(line);

			AssertNoError("Precondition:", propertyInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line[countColumn] = 2m;
			AssertHasError(propertyInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line[countColumn] = 1m;
			AssertNoError(propertyInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			AssertNoError(propertyInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line[countColumn] = 2m;
			AssertNoError(propertyInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
		}

		void AssertCountIsDivisibleByPerPackageQty(SchemaDecimalColumn countColumn, ZByte currentCount, Func<WhsStocktakeLine, ZPropertyInfo> getPropertyInfo)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			AssertCountIsDivisibleByPerPackageQty(data, countColumn, currentCount, getPropertyInfo, isManuallyAdded: true);
			AssertCountIsDivisibleByPerPackageQty(data, countColumn, currentCount, getPropertyInfo, isManuallyAdded: false);
		}

		void AssertCountIsDivisibleByPerPackageQty(TestDataSimpleEnvironment data, SchemaDecimalColumn countColumn, ZByte currentCount, Func<WhsStocktakeLine, ZPropertyInfo> getPropertyInfo, bool isManuallyAdded)
		{
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, isManuallyAdded);
			stocktakeLine.WU_TotalCounts = currentCount;
			AssertNoErrors("Precondition", getPropertyInfo(stocktakeLine));

			stocktakeLine[countColumn] = 10m;
			AssertNoErrors(getPropertyInfo(stocktakeLine));

			stocktakeLine.WU_PerPackageQty = 5m;
			AssertNoErrors(getPropertyInfo(stocktakeLine));
			AssertNoErrors(stocktakeLine.WU_PerPackageQtyInfo);

			var expectedErrorMessage = "Current Units must be divisible by Per Group Quantity.";
			stocktakeLine.WU_PerPackageQty = 4m;

			// Per Package Qty is uneditable on System Stocktake lines so there is no reason to validate it
			if (isManuallyAdded)
			{
				AssertHasError(stocktakeLine.WU_PerPackageQtyInfo, expectedErrorMessage);
			}

			stocktakeLine[countColumn] = 8m;
			AssertNoErrors(stocktakeLine.WU_PerPackageQtyInfo);

			stocktakeLine[countColumn] = 10m;
			AssertHasError(getPropertyInfo(stocktakeLine), expectedErrorMessage);

			stocktakeLine.WU_PerPackageQty = 5m;
			AssertNoErrors(getPropertyInfo(stocktakeLine));
		}

		void AssertCount_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupID(SchemaDecimalColumn countColumn, ZByte currentCount, Func<WhsStocktakeLine, ZPropertyInfo> getPropertyInfo)
		{
			AssertCount_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupID(countColumn, currentCount, getPropertyInfo, isManuallyAdded: true);
			AssertCount_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupID(countColumn, currentCount, getPropertyInfo, isManuallyAdded: false);
		}

		void AssertCount_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupID(SchemaDecimalColumn countColumn, ZByte currentCount, Func<WhsStocktakeLine, ZPropertyInfo> getPropertyInfo, bool isManuallyAdded)
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, isManuallyAdded);
			var stocktakeLine2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, isManuallyAdded);
			var stocktakeLine3 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, isManuallyAdded);
			stocktakeLine1.WU_TotalCounts = currentCount;
			stocktakeLine2.WU_TotalCounts = currentCount;
			stocktakeLine3.WU_TotalCounts = currentCount;
			stocktakeLine1[countColumn] = 10m;
			stocktakeLine2[countColumn] = 10m;
			stocktakeLine3[countColumn] = 4m;
			stocktakeLine1.WU_PartAttrib1 = "Red";
			stocktakeLine2.WU_PartAttrib1 = "Red";
			stocktakeLine3.WU_PartAttrib1 = "Red";
			stocktakeLine1.WU_PerPackageQty = 2m;
			stocktakeLine2.WU_PerPackageQty = 5m;
			AssertNoErrors("Precondition", getPropertyInfo(stocktakeLine1));
			AssertNoErrors("Precondition", getPropertyInfo(stocktakeLine2));

			var expectedErrorMessage = "Total Package Count inconsistent for Package Group ID within location. Check the Quantity and Per Group Quantity.";
			stocktakeLine1.WU_PackageGroupId = "123";
			AssertNoErrors(getPropertyInfo(stocktakeLine1));

			// stocktake line 1 has 10 / 2 = 5 packs
			// stocktake line 2 has 10 / 5 = 2 packs
			stocktakeLine2.WU_PackageGroupId = "123";
			AssertHasError(getPropertyInfo(stocktakeLine2), expectedErrorMessage);

			// only one stocktake line with Package Group ID '123' should be no error
			stocktakeLine2.WU_PackageGroupId = "";
			AssertNoErrors(getPropertyInfo(stocktakeLine2));

			// no package groups
			stocktakeLine1.WU_PackageGroupId = "";
			AssertNoErrors(getPropertyInfo(stocktakeLine1));

			// only one stocktake line with Package Group ID '123'
			stocktakeLine1.WU_PackageGroupId = "123";
			AssertNoErrors("Precondition", getPropertyInfo(stocktakeLine2));

			// stocktake line 1 has (10 / 2) = 5 packs
			// stocktake line 2 has (10 / 5) = 2 packs
			stocktakeLine2.WU_PackageGroupId = "123";
			AssertHasError(getPropertyInfo(stocktakeLine2), expectedErrorMessage);

			// stocktake line 1 has (10 / 2) = 5 packs
			// stocktake line 2 has (10 / 2) = 5 packs
			stocktakeLine2.WU_PerPackageQty = 2m;
			AssertNoErrors(getPropertyInfo(stocktakeLine2));

			// stocktake line 1 has (10 / 2) = 5 packs
			// stocktake line 2 has (10 / 5) = 2 packs
			stocktakeLine2.WU_PerPackageQty = 5m;
			AssertHasError(getPropertyInfo(stocktakeLine2), expectedErrorMessage);

			// stocktake line 1 has (10 / 2) = 5 packs
			// stocktake line 2 has (25 / 5) = 5 packs
			stocktakeLine2[countColumn] = 25m;
			AssertNoErrors(getPropertyInfo(stocktakeLine2));

			// stocktake line 1 + stocktake line 3 has (10 + 4 / 2) = 7 packs
			// stocktake line 2 has (25 / 5) = 5 packs
			stocktakeLine3.WU_PerPackageQty = 2m;
			stocktakeLine3.WU_PackageGroupId = "123";
			AssertNoErrors(stocktakeLine3.WU_PerPackageQtyInfo);
			AssertHasError(getPropertyInfo(stocktakeLine3), expectedErrorMessage);

			// stocktake line 1 + stocktake line 3 has ([10 + 4] / 2) = 7 packs
			// stocktake line 2 has (35 / 5) = 7 packs
			stocktakeLine2[countColumn] = 35m;
			stocktakeLine3.Validation.ValidateWU_LastCount();
			stocktakeLine3.Validation.ValidateWU_Count2();
			stocktakeLine3.Validation.ValidateWU_Count3();
			AssertNoErrors(getPropertyInfo(stocktakeLine3));

			// stocktake line 1 has (10 / 2) = 5 packs
			// stocktake line 2 has (25 / 5) = 5 packs
			// stocktake line 3 has (4 / 2) = 2 packs
			stocktakeLine3.WU_PartAttrib1 = "Green";
			stocktakeLine2[countColumn] = 25m;
			stocktakeLine3.Validation.ValidateWU_LastCount();
			stocktakeLine3.Validation.ValidateWU_Count2();
			stocktakeLine3.Validation.ValidateWU_Count3();
			AssertHasError(getPropertyInfo(stocktakeLine3), expectedErrorMessage);

			// stocktake line 1 has (10 / 2) = 5 packs
			// stocktake line 2 has (25 / 5) = 5 packs
			// stocktake line 3 has (10 / 2) = 5 packs
			stocktakeLine3[countColumn] = 10m;
			AssertNoErrors(getPropertyInfo(stocktakeLine3));
		}

		void AssertCount_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupIDPerLocation(SchemaDecimalColumn countColumn, ZByte currentCount, Func<WhsStocktakeLine, ZPropertyInfo> getPropertyInfo)
		{
			AssertCount_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupIDPerLocation(countColumn, currentCount, getPropertyInfo, isManuallyAdded: true);
			AssertCount_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupIDPerLocation(countColumn, currentCount, getPropertyInfo, isManuallyAdded: false);
		}

		void AssertCount_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupIDPerLocation(SchemaDecimalColumn countColumn, ZByte currentCount, Func<WhsStocktakeLine, ZPropertyInfo> getPropertyInfo, bool isManuallyAdded)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1, saveFactory_doNotUseForNewTests: false);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";

			var location1 = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations.Single(l => l.WLV_Column == 1 && l.WLV_Level == 1 && l.WLV_Tray == 1);
			var location2 = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations.Single(l => l.WLV_Column == 2 && l.WLV_Level == 1 && l.WLV_Tray == 1);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, location1, "", isManuallyAdded); //20m
			var stocktakeLine2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, location1, "", isManuallyAdded); //10m
			var stocktakeLine3 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, location1, "", isManuallyAdded); //20m
			var stocktakeLine4 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, location1, "", isManuallyAdded); // 10m 'ABC' 5m
			stocktakeLine1.WU_TotalCounts = currentCount;
			stocktakeLine2.WU_TotalCounts = currentCount;
			stocktakeLine3.WU_TotalCounts = currentCount;
			stocktakeLine4.WU_TotalCounts = currentCount;
			stocktakeLine1.WU_PackageGroupId = "ABC";
			stocktakeLine2.WU_PackageGroupId = "ABC";
			stocktakeLine3.WU_PackageGroupId = "ABC";
			stocktakeLine4.WU_PackageGroupId = "ABC";
			stocktakeLine1.WU_PerPackageQty = 5m;
			stocktakeLine2.WU_PerPackageQty = 5m;
			stocktakeLine3.WU_PerPackageQty = 5m;
			stocktakeLine4.WU_PerPackageQty = 5m;
			stocktakeLine1[countColumn] = 20m;
			stocktakeLine2[countColumn] = 10m;
			stocktakeLine3[countColumn] = 20m;
			stocktakeLine4[countColumn] = 10m;

			stocktakeLine1.Validation.ValidateWU_LastCount();
			stocktakeLine1.Validation.ValidateWU_Count2();
			stocktakeLine1.Validation.ValidateWU_Count3();
			AssertNoErrors(getPropertyInfo(stocktakeLine1));

			stocktakeLine2[countColumn] = 30m;
			stocktakeLine1.WU_WL = location2.PK;
			stocktakeLine1.Validation.ValidateWU_LastCount();
			stocktakeLine1.Validation.ValidateWU_Count2();
			stocktakeLine1.Validation.ValidateWU_Count3();
			AssertHasError(getPropertyInfo(stocktakeLine1), "Total Package Count inconsistent for Package Group ID within location. Check the Quantity and Per Group Quantity.");
			stocktakeLine2[countColumn] = 10m; // clean up

			stocktakeLine4.WU_WL = location2.PK;
			stocktakeLine1.Validation.ValidateWU_LastCount();
			stocktakeLine1.Validation.ValidateWU_Count2();
			stocktakeLine1.Validation.ValidateWU_Count3();
			AssertHasError(getPropertyInfo(stocktakeLine1), "Total Package Count inconsistent for Package Group ID within location. Check the Quantity and Per Group Quantity.");
			stocktakeLine4.WU_WL = location1.PK; // clean up

			stocktakeLine3.WU_WL = location2.PK;
			stocktakeLine1.Validation.ValidateWU_LastCount();
			stocktakeLine1.Validation.ValidateWU_Count2();
			stocktakeLine1.Validation.ValidateWU_Count3();
			AssertNoErrors(getPropertyInfo(stocktakeLine1));
		}

		#endregion
	}
}
