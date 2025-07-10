using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
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
	class WhsStocktakeLineForManuallyAddedLinesTestCase : WhsStocktakeLineValidationTestCase
	{
		#region TestValidateLocationString

		public void TestValidateLocationString()
		{
			var whs = Helper.CreateWarehouse("1");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			Factory.Save();

			var stocktake = Factory.New<WhsStocktake>();
			stocktake.WS_WW_Whs = whs.PK;
			var stocktakeLine = stocktake.Lines.AddNew();
			stocktakeLine.WU_OP = ZGuid.NewZGuid();
			stocktakeLine.LocationString = "A";
			stocktakeLine.WU_IsManuallyAdded = true;

			AssertNoErrors(stocktakeLine.LocationStringInfo);
			AssertEquals("WL should equal warehouse location", row.Locations[0].PK, stocktakeLine.WU_WL);

			stocktakeLine.LocationString = "";
			AssertEquals("Location should have errors", true, stocktakeLine.LocationStringInfo.HasErrors());
			AssertEquals("WL should equal empty", true, stocktakeLine.WU_WL.IsEmpty);
			AssertEquals("Location string should be empty", "", stocktakeLine.LocationString);
		}

		#endregion

		#region TestCheckWU_BondedEntryKey

		public void TestCheckWU_BondedEntryKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, "", true);
			AssertNoErrors("Precondition", stocktakeLine.WU_BondedEntryKeyInfo);

			stocktakeLine.Location.WLV_WA_PickingArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON").PK;
			stocktakeLine.Validation.ValidateWU_BondedEntryKey();
			AssertHasError(stocktakeLine.WU_BondedEntryKeyInfo, "Entry Number is mandatory for Locations in a Bonded Area.");

			stocktakeLine.Location.WLV_WA_PickingArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "FRE").PK;
			stocktakeLine.Validation.ValidateWU_BondedEntryKey();
			AssertNoErrors(stocktakeLine.WU_BondedEntryKeyInfo);
		}

		#endregion

		#region TestCheckWU_OP

		public void TestCheckWU_OP()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(null, data.Whs1, data.Part1);
			stocktake.WS_StocktakeStatus = CodeLists.StocktakeStatus.Codes.Loaded;
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1);
			line.WU_IsManuallyAdded = true;

			var org1 = Helper.CreateClient("C1");

			// Setup one product with different client and two products belong to the same client but one is inactive and one with invalid part number

			var productWithDifferentClient = Helper.CreateProduct(org1, "PR1");

			var productWithSameClientAndInActive = Helper.CreateProduct(data.Org1, "PR2");
			productWithSameClientAndInActive.OP_IsActive = ZBool.False;

			var productWithSameClientAndInvalidPartNumber = Helper.CreateProduct(data.Org1, "PR3");
			productWithSameClientAndInvalidPartNumber.OP_PartNum = ProductType.Codes.Invalid;

			// test the validation

			AssertNoErrors("Since data.Part1 belongs to data.Org1, there are no errors.", line.WU_OPInfo);

			line.WU_OP = productWithDifferentClient.PK;
			AssertHasError("Since product belongs to a different client, there should be an error.", line.WU_OPInfo, WhsValidationHelper.ProductRelationshipsErrorMessage);

			line.WU_OP = productWithSameClientAndInActive.PK;
			AssertHasError("Since product is inactive, there should be an error.", line.WU_OPInfo, OrgSupplierPartCollection.ProductIsNotActiveErrorMessage);

			line.WU_OP = productWithSameClientAndInvalidPartNumber.PK;
			AssertHasError("Since product has an invalid part number, there should be an error.", line.WU_OPInfo, OrgSupplierPartCollection.InvalidProductErrorMessage);

			line.WU_OP = data.Part2.PK;
			AssertNoErrors("Since data.Part2 belongs to data.Org1, there are no errors.", line.WU_OPInfo);
		}

		#endregion

		#region TestChekcWU_OP_CanCalculateExpiryDateIfJulianBatchNumberIsUsed

		public void TestChekcWU_OP_CanCalculateExpiryDateIfJulianBatchNumberIsUsed()
		{
			var client = Helper.CreateClient("CLIENT");
			var whs = Helper.CreateWarehouse("WHS");
			Helper.SetClientAttributeType(client, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(client, AttributeNumber.Two, PartAttributeTypeList.Codes.JulianBatchNumber); // should set use Expiry Date.

			var stocktake = Helper.CreateWhsStocktake(client, whs);
			stocktake.WS_StocktakeStatus = StocktakeStatus.Codes.Loaded;
			var stocktakeLine = stocktake.Lines.AddNew();
			stocktakeLine.WU_IsManuallyAdded = true;

			AssertNoError("Precondition", stocktakeLine.WU_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

			// Product with Normal Attribute - NO ERROR
			var partWithNormalAttribute = Helper.CreateProduct(client, "P1");
			Helper.SetProductAttributeUse(client, partWithNormalAttribute, AttributeNumber.One, true);
			stocktakeLine.WU_OP = partWithNormalAttribute.PK;
			AssertNoError(stocktakeLine.WU_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

			// Product with Julian Batch Number Attribute but without Product-Client-Warehouse parameter - ERROR
			var partWithJulianAttributeButWithoutClientWhsParam = Helper.CreateProduct(client, "P2");
			Helper.SetProductAttributeUse(client, partWithJulianAttributeButWithoutClientWhsParam, AttributeNumber.Two, true); // should set use Expiry Date.
			stocktakeLine.WU_OP = partWithJulianAttributeButWithoutClientWhsParam.PK;
			AssertHasError(stocktakeLine.WU_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

			// Product with Julian Batch Number Attribute but with wrong client Product-Client-Warehouse parameter - ERROR
			var partWithJulianAttributeAndClientWhsParam_InvalidClient = Helper.CreateProduct(client, "P3");
			var incorrectClient = Helper.CreateClient("CLIENT2");
			Helper.SetProductAttributeUse(client, partWithJulianAttributeAndClientWhsParam_InvalidClient, AttributeNumber.Two, true); // should set use Expiry Date.
			Helper.CreateProductParamsByWhsAndClient(partWithJulianAttributeAndClientWhsParam_InvalidClient, incorrectClient, whs);
			stocktakeLine.WU_OP = partWithJulianAttributeAndClientWhsParam_InvalidClient.PK;
			AssertHasError(stocktakeLine.WU_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

			// Product with Julian Batch Number Attribute but with wrong warehouse Product-Client-Warehouse parameter - ERROR
			var partWithJulianAttributeAndClientWhsParam_InvalidWhs = Helper.CreateProduct(client, "P4");
			var incorrectWhs = Helper.CreateWarehouse("WH2");
			Helper.SetProductAttributeUse(client, partWithJulianAttributeAndClientWhsParam_InvalidWhs, AttributeNumber.Two, true); // should set use Expiry Date.
			Helper.CreateProductParamsByWhsAndClient(partWithJulianAttributeAndClientWhsParam_InvalidWhs, client, incorrectWhs);
			stocktakeLine.WU_OP = partWithJulianAttributeAndClientWhsParam_InvalidWhs.PK;
			AssertHasError(stocktakeLine.WU_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

			// Product with Julian Batch Number Attribute and with Product-Client-Warehouse parameter that has Maximum Shelf Life = 0 - ERROR
			var partWithJulianAttributeAndClientWhsParam_ZeroMaxShelfLife = Helper.CreateProduct(client, "P5");
			Helper.SetProductAttributeUse(client, partWithJulianAttributeAndClientWhsParam_ZeroMaxShelfLife, AttributeNumber.Two, true); // should set use Expiry Date.
			Helper.CreateProductParamsByWhsAndClient(partWithJulianAttributeAndClientWhsParam_ZeroMaxShelfLife, client, whs);
			stocktakeLine.WU_OP = partWithJulianAttributeAndClientWhsParam_ZeroMaxShelfLife.PK;
			AssertHasError(stocktakeLine.WU_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

			// Product with Julian Batch Number Attribute and with Product-Client-Warehouse parameter that has Maximum Shelf Life > 0 - NO ERROR
			var partWithJulianAttributeAndClientWhsParam_Correct = Helper.CreateProduct(client, "P6");
			Helper.SetProductAttributeUse(client, partWithJulianAttributeAndClientWhsParam_Correct, AttributeNumber.Two, true); // should set use Expiry Date.
			Helper.CreateProductParamsByWhsAndClient(partWithJulianAttributeAndClientWhsParam_Correct, client, whs).W3_MaximumShelfLife = 5;
			stocktakeLine.WU_OP = partWithJulianAttributeAndClientWhsParam_Correct.PK;
			AssertNoError(stocktakeLine.WU_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);
		}

		#endregion

		#region TestCheckWU_OH_Client

		public void TestCheckWU_OH_Client()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(null, data.Whs1);
			var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, null, data.Part1);
			stocktakeLine.WU_IsManuallyAdded = true;
			AssertNoErrors(stocktakeLine.WU_OH_ClientInfo);

			var client = Helper.CreateClient("Z1");
			stocktakeLine.WU_OH_Client = client.PK;
			AssertHasError("", stocktakeLine.WU_OH_ClientInfo, WhsStocktakeLineValidationForManuallyAddedLines.MustHaveProperClient);

			Helper.CreateProductClientRelationShip(client, data.Part1, OrgPartRelation.RelationshipTypes.Supplier);
			stocktakeLine.Validation.ValidateWU_OH_Client();
			AssertHasError("", stocktakeLine.WU_OH_ClientInfo, WhsStocktakeLineValidationForManuallyAddedLines.MustHaveProperClient);

			Helper.CreateProductClientRelationShip(client, data.Part1, OrgPartRelation.RelationshipTypes.Owner);
			stocktakeLine.Validation.ValidateWU_OH_Client();
			AssertNoErrors(stocktakeLine.WU_OH_ClientInfo);

			var clientBoth = Helper.CreateClient("Z2");
			Helper.CreateProductClientRelationShip(clientBoth, data.Part1, OrgPartRelation.RelationshipTypes.Both);
			stocktakeLine.WU_OH_Client = clientBoth.PK;
			AssertNoErrors(stocktakeLine.WU_OH_ClientInfo);
		}

		#endregion

		#region TestCheckWU_InventoryStatus

		public void TestCheckWU_InventoryStatus()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, true);

			// test the validation

			//AssertHasErrors("Since there is no inventory status, there should be errors.", line.WU_InventoryStatusInfo); // unavailble until we show inventory status column 

			line.WU_InventoryStatus = InventoryStatus.Codes.Available;
			AssertNoErrors("Since there is no inventory status is Available, there should be no errors.", line.WU_InventoryStatusInfo);

			line.WU_InventoryStatus = StocktakeInventoryStatus.Codes.Damaged;
			AssertNoErrors("Since there is no inventory status is Damaged, there should be no errors.", line.WU_InventoryStatusInfo);

			line.WU_InventoryStatus = "AAA";
			AssertHasErrors("Since inventory status is invalid, there should be errors.", line.WU_InventoryStatusInfo);

			line.WU_InventoryStatus = "";
			AssertHasErrors("Since there is no inventory status, there should be errors.", line.WU_InventoryStatusInfo);
		}

		#endregion

		#region TestValidateDuplicateLines

		[TestDate(2012, 03, 01)]
		public void TestValidateDuplicateLines()
		{
			// setup test data
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);

			// Setup stocktake and a line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			var closedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, today.AddDays(3), today.AddDays(4), "PA2", "PA3", "PA4", StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available);
			var openLineWithAvailableInventoryStatus = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, today.AddDays(2), today.AddDays(3), "PA1", "PA2", "PA3", StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var lineWithDamagedStatus = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, today.AddDays(2), today.AddDays(3), "PA1", "PA2", "PA3", StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Damaged);

			var newLineWithMatchingClosedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, today.AddDays(3), today.AddDays(4), "PA2", "PA3", "PA4", StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available, true);
			var newLineWithMatchingOpenLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, today.AddDays(2), today.AddDays(3), "PA1", "PA2", "PA3", StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available, true);
			var newLineWithoutMatchingLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, today.AddDays(5), today.AddDays(6), "PA2", "PA3", "PA4", StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available, true);
			var newlineWithDamagedStatus = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, today.AddDays(2), today.AddDays(3), "PA1", "PA2", "PA3", StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Damaged, true);

			// Test the validation

			closedLine.Validation.ValidateAll();
			AssertNoErrors(closedLine);

			openLineWithAvailableInventoryStatus.Validation.ValidateAll();
			AssertNoErrors(openLineWithAvailableInventoryStatus);

			lineWithDamagedStatus.Validation.ValidateAll();
			AssertNoErrors(lineWithDamagedStatus);

			newLineWithMatchingClosedLine.Validation.ValidateAll();
			AssertNoErrors(newLineWithMatchingClosedLine);

			newLineWithMatchingOpenLine.Validation.ValidateAll();
			AssertHasRowError(newLineWithMatchingOpenLine, "Product P1 in Location A with status AVL is already on this stocktake on Line 2. You must edit the existing stock take line. If you cannot see the line clear all filters.");

			newLineWithMatchingOpenLine.WU_Status = StocktakeLineStatus.Codes.All;
			newLineWithMatchingOpenLine.Validation.ValidateAll();
			AssertNoErrors(newLineWithMatchingOpenLine);

			newlineWithDamagedStatus.Validation.ValidateAll();
			AssertHasRowError(newlineWithDamagedStatus, "Product P1 in Location A with status DAM is already on this stocktake on Line 3. You must edit the existing stock take line. If you cannot see the line clear all filters.");

			newlineWithDamagedStatus.WU_Status = StocktakeLineStatus.Codes.All;
			newlineWithDamagedStatus.Validation.ValidateAll();
			AssertNoErrors(newlineWithDamagedStatus);

			newLineWithoutMatchingLine.AddRowError("Test Error");
			newLineWithoutMatchingLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(newLineWithoutMatchingLine, "You must edit the existing stocktake line. If you cannot see the line clear all filters.");
			AssertHasRowError("Should not clear hack error", newLineWithoutMatchingLine, "Test Error");
		}

		#endregion

		#region Part Attributes

		#region Test WU_PartAttrib1

		#region TestCheckWU_PartAttrib1

		public void TestCheckWU_PartAttrib1()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2m, new ZByte(3), StocktakeLineStatus.Codes.Open, true);

			AssertNoError(line.WU_PartAttrib1Info, WhsStocktakeLineValidation.ValueHasToBeTrimmed);

			line.WU_PartAttrib1 = "  Att1";
			AssertHasError(line.WU_PartAttrib1Info, WhsStocktakeLineValidation.ValueHasToBeTrimmed);
		}

		#endregion

		#region TestCheckWU_PartAttrib1_WithReleaseCapturedAttribute

		public void TestCheckWU_PartAttrib1_WithReleaseCapturedAttribute()
		{
			AssertCheckPartAttribWithReleaseCapturedAttribs(AttributeNumber.One, WhsStocktakeLineSchema.WU_PartAttrib1, line => line.WU_PartAttrib1Info);
		}

		public void TestCheckWU_PartAttrib1_WithReleaseCapturedAttribute_EmptyAttributeNotChecked()
		{
			AssertCheckPartAttribWithReleaseCapturedAttribs_EmptyAttributeNotChecked(AttributeNumber.One, line => line.WU_PartAttrib1Info);
		}

		#endregion

		#region TestCheckWU_PartAttrib1_AttributeCallCheck

		public void TestCheckWU_PartAttrib1_AttributeCallCheck()
		{
			var stocktake = Factory.New<WhsStocktake>();

			using (new SemaphoreManager(stocktake.LineClosingSemaphore))
			{
				stocktake.WS_OH_Client = Helper.CreateClient().PK;
				var stocktakeLine = stocktake.Lines.AddNew();
				stocktakeLine.WU_OP = Helper.CreateProduct(stocktake.Client, "P1").PK;
				Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.One, true);
				Helper.SetProductAttributeUse(stocktake.Client, stocktakeLine.SupplierPart, AttributeNumber.One, true);

				using (new PartAttributeValidationChecker.AttributeCallChecker(true, stocktake.Client, stocktakeLine.SupplierPart, stocktakeLine.WU_PartAttrib1Info, 1))
				{
					stocktakeLine.WU_PartAttrib1 = "AAA";
				}
			}
		}

		#endregion

		#region TestCheckWU_PartAttrib1_NotLineClosing

		public void TestCheckWU_PartAttrib1_NotLineClosing()
		{
			var stocktake = Factory.New<WhsStocktake>();
			stocktake.WS_OH_Client = Helper.CreateClient().PK;

			var stocktakeLine = stocktake.Lines.AddNew();
			stocktakeLine.WU_OP = Helper.CreateProduct(stocktake.Client, "P1").PK;
			Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(stocktake.Client, stocktakeLine.SupplierPart, AttributeNumber.One, true);

			stocktakeLine.WU_PartAttrib1 = "";
			AssertHasError(stocktakeLine.WU_PartAttrib1Info, "Please enter a Part Attrib. 1.");

			stocktakeLine.WU_PartAttrib1 = "AAA";
			AssertNoErrors(stocktakeLine.WU_PartAttrib1Info);
		}

		#endregion

		#region TestCheckWU_PartAttrib1_JulianBatchNumber

		public void TestCheckWU_PartAttrib1_JulianBatchNumber()
		{
			TestCheckWE_PartAttrib_JulianBatchNumberCore(WhsStocktakeLineSchema.WU_PartAttrib1, AttributeNumber.One, line => line.WU_PartAttrib1Info);
		}

		#endregion

		#endregion

		#region Test WU_PartAttrib2

		#region TestCheckWU_PartAttrib2

		public void TestCheckWU_PartAttrib2()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2m, new ZByte(3), StocktakeLineStatus.Codes.Open, true);

			AssertNoError(line.WU_PartAttrib2Info, WhsStocktakeLineValidation.ValueHasToBeTrimmed);

			line.WU_PartAttrib2 = "  Att2";
			AssertHasError(line.WU_PartAttrib2Info, WhsStocktakeLineValidation.ValueHasToBeTrimmed);
		}

		#endregion

		#region TestCheckWU_PartAttrib2_WithReleaseCapturedAttribute

		public void TestCheckWU_PartAttrib2_WithReleaseCapturedAttribute()
		{
			AssertCheckPartAttribWithReleaseCapturedAttribs(AttributeNumber.Two, WhsStocktakeLineSchema.WU_PartAttrib2, line => line.WU_PartAttrib2Info);
		}

		public void TestCheckWU_PartAttrib2_WithReleaseCapturedAttribute_EmptyAttributeNotChecked()
		{
			AssertCheckPartAttribWithReleaseCapturedAttribs_EmptyAttributeNotChecked(AttributeNumber.Two, line => line.WU_PartAttrib2Info);
		}

		#endregion

		#region TestCheckWU_PartAttrib2_AttributeCallCheck

		public void TestCheckWU_PartAttrib2_AttributeCallCheck()
		{
			var stocktake = Factory.New<WhsStocktake>();
			using (new SemaphoreManager(stocktake.LineClosingSemaphore))
			{
				stocktake.WS_OH_Client = Helper.CreateClient().PK;
				var stocktakeLine = stocktake.Lines.AddNew();
				stocktakeLine.WU_OP = Helper.CreateProduct(stocktake.Client, "P1").PK;
				Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.Two, true);
				Helper.SetProductAttributeUse(stocktake.Client, stocktakeLine.SupplierPart, AttributeNumber.Two, true);

				using (new PartAttributeValidationChecker.AttributeCallChecker(true, stocktake.Client, stocktakeLine.SupplierPart, stocktakeLine.WU_PartAttrib2Info, 2))
				{
					stocktakeLine.WU_PartAttrib2 = "AAA";
				}
			}
		}

		#endregion

		#region TestCheckWU_PartAttrib2_NotLineClosing

		public void TestCheckWU_PartAttrib2_NotLineClosing()
		{
			var stocktake = Factory.New<WhsStocktake>();
			stocktake.WS_OH_Client = Helper.CreateClient().PK;

			var stocktakeLine = stocktake.Lines.AddNew();
			stocktakeLine.WU_OP = Helper.CreateProduct(stocktake.Client, "P1").PK;
			Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(stocktake.Client, stocktakeLine.SupplierPart, AttributeNumber.Two, true);

			stocktakeLine.WU_PartAttrib2 = "";
			AssertHasError(stocktakeLine.WU_PartAttrib2Info, "Please enter a Part Attrib. 2.");

			stocktakeLine.WU_PartAttrib2 = "AAA";
			AssertNoErrors(stocktakeLine.WU_PartAttrib2Info);
		}

		#endregion

		#region TestCheckWU_PartAttrib2_JulianBatchNumber

		public void TestCheckWU_PartAttrib2_JulianBatchNumber()
		{
			TestCheckWE_PartAttrib_JulianBatchNumberCore(WhsStocktakeLineSchema.WU_PartAttrib2, AttributeNumber.Two, line => line.WU_PartAttrib2Info);
		}

		#endregion

		#endregion

		#region Test WU_PartAttrib3

		#region TestCheckWU_PartAttrib3

		public void TestCheckWU_PartAttrib3()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2m, new ZByte(3), StocktakeLineStatus.Codes.Open, true);

			AssertNoError(line.WU_PartAttrib3Info, WhsStocktakeLineValidation.ValueHasToBeTrimmed);

			line.WU_PartAttrib3 = "  Att3";
			AssertHasError(line.WU_PartAttrib3Info, WhsStocktakeLineValidation.ValueHasToBeTrimmed);
		}

		#endregion

		#region TestCheckWU_PartAttrib3_WithReleaseCapturedAttribute

		public void TestCheckWU_PartAttrib3_WithReleaseCapturedAttribute()
		{
			AssertCheckPartAttribWithReleaseCapturedAttribs(AttributeNumber.Three, WhsStocktakeLineSchema.WU_PartAttrib3, line => line.WU_PartAttrib3Info);
		}

		public void TestCheckWU_PartAttrib3_WithReleaseCapturedAttribute_EmptyAttributeNotChecked()
		{
			AssertCheckPartAttribWithReleaseCapturedAttribs_EmptyAttributeNotChecked(AttributeNumber.Three, line => line.WU_PartAttrib3Info);
		}

		#endregion

		#region TestCheckWU_PartAttrib3_AttributeCallCheck

		public void TestCheckWU_PartAttrib3_AttributeCallCheck()
		{
			var stocktake = Factory.New<WhsStocktake>();
			using (new SemaphoreManager(stocktake.LineClosingSemaphore))
			{
				stocktake.WS_OH_Client = Helper.CreateClient().PK;
				var stocktakeLine = stocktake.Lines.AddNew();
				stocktakeLine.WU_OP = Helper.CreateProduct(stocktake.Client, "P1").PK;
				Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.Three, true);
				Helper.SetProductAttributeUse(stocktake.Client, stocktakeLine.SupplierPart, AttributeNumber.Three, true);

				using (new PartAttributeValidationChecker.AttributeCallChecker(true, stocktake.Client, stocktakeLine.SupplierPart, stocktakeLine.WU_PartAttrib3Info, 3))
				{
					stocktakeLine.WU_PartAttrib3 = "AAA";
				}
			}
		}

		#endregion

		#region TestCheckWU_PartAttrib3_NotLineClosing

		public void TestCheckWU_PartAttrib3_NotLineClosing()
		{
			var stocktake = Factory.New<WhsStocktake>();
			stocktake.WS_OH_Client = Helper.CreateClient().PK;

			var stocktakeLine = stocktake.Lines.AddNew();
			stocktakeLine.WU_OP = Helper.CreateProduct(stocktake.Client, "P1").PK;
			Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(stocktake.Client, stocktakeLine.SupplierPart, AttributeNumber.Three, true);

			stocktakeLine.WU_PartAttrib3 = "";
			AssertHasError(stocktakeLine.WU_PartAttrib3Info, "Please enter a Part Attrib. 3.");

			stocktakeLine.WU_PartAttrib3 = "AAA";
			AssertNoErrors(stocktakeLine.WU_PartAttrib3Info);
		}

		#endregion

		#region TestCheckWU_PartAttrib3_JulianBatchNumber

		public void TestCheckWU_PartAttrib3_JulianBatchNumber()
		{
			TestCheckWE_PartAttrib_JulianBatchNumberCore(WhsStocktakeLineSchema.WU_PartAttrib3, AttributeNumber.Three, line => line.WU_PartAttrib3Info);
		}

		#endregion

		#endregion

		#region TestCheck WU_PackingDate

		#region	TestCheckWU_PackingDate

		public void TestCheckWU_PackingDate()
		{
			var stocktake = Factory.New<WhsStocktake>();
			using (new SemaphoreManager(stocktake.LineClosingSemaphore))
			{
				stocktake.WS_OH_Client = Helper.CreateClient().PK;
				var stocktakeLine = stocktake.Lines.AddNew();
				stocktakeLine.WU_OP = Helper.CreateProduct(stocktake.Client, "P1").PK;
				Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.ExpiryDate, true);
				Helper.SetProductAttributeUse(stocktake.Client, stocktakeLine.SupplierPart, AttributeNumber.ExpiryDate, true);

				using (new PartAttributeValidationChecker.AttributeCallChecker(true, stocktake.Client, stocktakeLine.SupplierPart, stocktakeLine.WU_PackingDateInfo, -2))
				{
					stocktakeLine.WU_PackingDate = ZDate.Today.AddDays(-1);
				}
			}
		}

		#endregion

		#region TestCheckWU_PackingDate_NotLineClosing

		public void TestCheckWU_PackingDate_NotLineClosing()
		{
			var stocktake = Factory.New<WhsStocktake>();
			stocktake.WS_OH_Client = Helper.CreateClient().PK;

			var stocktakeLine = stocktake.Lines.AddNew();
			stocktakeLine.WU_OP = Helper.CreateProduct(stocktake.Client, "P1").PK;
			Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(stocktake.Client, stocktakeLine.SupplierPart, AttributeNumber.PackingDate, true);

			stocktakeLine.WU_PackingDate = ZDate.Empty;
			AssertHasError(stocktakeLine.WU_PackingDateInfo, "Please enter a Packing date.");

			stocktakeLine.WU_PackingDate = ZDate.Today.AddDays(-1);
			AssertNoErrors(stocktakeLine.WU_PackingDateInfo);
		}

		#endregion

		#endregion

		#region TestCheck WU_ExpiryDate

		#region TestCheckWU_ExpiryDate

		public void TestCheckWU_ExpiryDate()
		{
			var stocktake = Factory.New<WhsStocktake>();
			using (new SemaphoreManager(stocktake.LineClosingSemaphore))
			{
				stocktake.WS_OH_Client = Helper.CreateClient().PK;

				var stocktakeLine = stocktake.Lines.AddNew();
				stocktakeLine.WU_OP = Helper.CreateProduct(stocktake.Client, "P1").PK;
				Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.PackingDate, true);
				Helper.SetProductAttributeUse(stocktake.Client, stocktakeLine.SupplierPart, AttributeNumber.PackingDate, true);

				using (new PartAttributeValidationChecker.AttributeCallChecker(true, stocktake.Client, stocktakeLine.SupplierPart, stocktakeLine.WU_ExpiryDateInfo, -1))
				{
					stocktakeLine.WU_ExpiryDate = ZDate.Today.AddDays(1);
				}

				using (new PartAttributeValidationChecker.AttributeCallChecker(true, stocktake.Client, stocktakeLine.SupplierPart, stocktakeLine.WU_ExpiryDateInfo, -1))
				{
					stocktakeLine.WU_ExpiryDate = ZDate.Today.AddYears(20);
				}
			}
		}

		#endregion

		#region TestCheckWU_ExpiryDate_NotClosingLines

		public void TestCheckWU_ExpiryDate_NotClosingLines()
		{
			var stocktake = Factory.New<WhsStocktake>();
			stocktake.WS_OH_Client = Helper.CreateClient().PK;

			var stocktakeLine = stocktake.Lines.AddNew();
			stocktakeLine.WU_OP = Helper.CreateProduct(stocktake.Client, "P1").PK;
			Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(stocktake.Client, stocktakeLine.SupplierPart, AttributeNumber.ExpiryDate, true);

			stocktakeLine.WU_ExpiryDate = ZDate.Empty;
			AssertHasError(stocktakeLine.WU_ExpiryDateInfo, "Please enter an Expiry date.");

			stocktakeLine.WU_ExpiryDate = ZDate.Today.AddDays(-1);
			AssertNoErrors(stocktakeLine.WU_ExpiryDateInfo);
		}

		#endregion

		#endregion

		#region AssertCheckPartAttribWithReleaseCapturedAttribs

		void AssertCheckPartAttribWithReleaseCapturedAttribs(AttributeNumber attributeNumber, SchemaColumn attributeColumn, Func<WhsStocktakeLine, ZPropertyInfo> getPropertyInfo)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, isManuallyAdded: true);
			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true, setReleaseCaptured: true);

			using (new SemaphoreManager(stocktake.LineClosingSemaphore))
			{
				var propertyInfo = getPropertyInfo(stocktakeLine);
				AssertNoErrors("Precondition:", propertyInfo);

				stocktakeLine[attributeColumn] = "SomeData";
				AssertHasError(propertyInfo, "This attribute is specified as Release Captured for this Product, no value should be entered.");
			}
		}

		void AssertCheckPartAttribWithReleaseCapturedAttribs_EmptyAttributeNotChecked(AttributeNumber attributeNumber, Func<WhsStocktakeLine, ZPropertyInfo> getPropertyInfo)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, isManuallyAdded: true);
			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true, setReleaseCaptured: true);

			var propertyInfo = getPropertyInfo(stocktakeLine);
			AssertNoErrors("Precondition:", propertyInfo);

			stocktakeLine.RunPreSaveValidation();
			AssertNoErrors("There should be no errors as the attribute is release captured.", propertyInfo);
		}

		#endregion

		#region TestCheckWE_PartAttrib_JulianBatchNumberCore

		void TestCheckWE_PartAttrib_JulianBatchNumberCore(SchemaStringColumn partAttributeSchemaColumn, AttributeNumber attributeNumber, Func<WhsStocktakeLine, ZPropertyInfo> getPropertyInfo)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true);
			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YYDDD;

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1);
			stocktakeLine.WU_IsManuallyAdded = true;
			var info = getPropertyInfo(stocktakeLine);

			using (new SemaphoreManager(stocktake.LineClosingSemaphore))
			{
				AssertNoError("Precondition", info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);

				stocktakeLine[partAttributeSchemaColumn] = "ABCD";
				AssertNoError(info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);

				Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.JulianBatchNumber);
				stocktakeLine[partAttributeSchemaColumn] = "ABCD";
				AssertHasError(info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);

				stocktakeLine[partAttributeSchemaColumn] = "12345ABCD";
				AssertHasError(info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);

				stocktakeLine[partAttributeSchemaColumn] = "ABCD12345";
				AssertNoError(info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);
			}
		}

		#endregion

		#region TestCheckWU_SerialNumber

		public void TestCheckWU_SerialNumber_QtyGreaterThanOne_ManuallyEntered()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1m, new ZByte(1), StocktakeLineStatus.Codes.Open, true);
			line.WU_LastCount = 1m;
			line.WU_SerialNumber = "TSDWE";

			AssertNoErrors("Precondition: Line with Serial Number and 1 Qty has no error", line.WU_LastCountInfo);

			line.WU_LastCount = 15m;
			AssertHasError("Should error for serial Number + 15 qty", line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line.WU_SerialNumber = "";
			AssertHasError("Line with no Serial Number and 15 Qty still has error", line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
		}

		public void TestCheckWU_SerialNumber_Trimmed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1m, new ZByte(3), StocktakeLineStatus.Codes.Open, true);

			AssertNoError(line.WU_SerialNumberInfo, WhsStocktakeLineValidation.ValueHasToBeTrimmed);

			line.WU_SerialNumber = "  Att3";
			AssertHasError(line.WU_SerialNumberInfo, WhsStocktakeLineValidation.ValueHasToBeTrimmed);
		}

		public void TestCheckWU_SerialNumber_WithReleaseCapturedAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, isManuallyAdded: true);

			var propertyInfo = stocktakeLine.WU_SerialNumberInfo;
			using (new SemaphoreManager(stocktake.LineClosingSemaphore))
			{
				AssertNoErrors("Precondition: WU_SerialNumberInfo", propertyInfo);

				stocktakeLine.WU_SerialNumber = "SomeData";
				AssertHasError(propertyInfo, "Serial Number is specified as Release Captured for this Product, no value should be entered.");
			}

			stocktakeLine.WU_SerialNumber = "";
			AssertNoErrors("Precondition: WU_SerialNumberInfo", propertyInfo);
			stocktakeLine.RunPreSaveValidation();
			AssertNoErrors("There should be no errors as the attribute is release captured.", propertyInfo);
		}

		public void TestCheckWU_SerialNumber_Required()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1m, new ZByte(3), StocktakeLineStatus.Codes.Open, true);

			line.WU_SerialNumber = "Att3";
			AssertNoErrors(line.WU_SerialNumberInfo);

			line.WU_SerialNumber = "";
			AssertHasError(line.WU_SerialNumberInfo, "Please enter a Serial Number.");
		}

		public void TestCheckWU_SerialNumber_UniqueSerialNumberProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Reference1", ZDateTimeOffset.Today);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, locations[0]);
			receiveLine.WE_SerialNumber = "SERIAL3";
			receive.FinaliseDocket();
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[1], StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			line.WU_SerialNumber = "S3";
			var lineWithDuplicatedSerialNumbers = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[2], StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available, true);
			lineWithDuplicatedSerialNumbers.WU_SerialNumber = "S3";
			var lineWithDuplicatedSerialNumbersInInventory = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[2], StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available, true);
			lineWithDuplicatedSerialNumbersInInventory.WU_SerialNumber = "SERIAL3";

			using (new SemaphoreManager(stocktake.LineClosingSemaphore))
			{
				lineWithDuplicatedSerialNumbers.CloseLine();
				AssertHasError("S3 is already in stocktake", lineWithDuplicatedSerialNumbers.WU_SerialNumberInfo, "Serial # already used.");

				lineWithDuplicatedSerialNumbersInInventory.CloseLine();
				AssertHasError("SERIAL3 is already in Inventory", lineWithDuplicatedSerialNumbersInInventory.WU_SerialNumberInfo, "Serial # already used.");
			}
		}

		#endregion

		#endregion

		#region Test WU_PalletID

		#region TestWU_PalletID

		public void TestWU_PalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2m, new ZByte(3), StocktakeLineStatus.Codes.Open, true);

			AssertNoError(line.WU_PalletIDInfo, WhsStocktakeLineValidation.ValueHasToBeTrimmed);

			line.WU_PalletID = "  P1";
			AssertHasError(line.WU_PalletIDInfo, WhsStocktakeLineValidation.ValueHasToBeTrimmed);
		}

		#endregion

		#region TestWU_PalletID_UniquePalletID

		public void TestWU_PalletID_UniquePalletID()
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

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);

			var lineWithEmptyPalletId = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[0], "", true);
			var lineWithCorrectPalletId = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[0], "PLT1", true);
			var lineWithCorrectPalletIdWithSameProduct = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, locations[0], "PLT1", StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Damaged, true);
			var lineWithDifferentPalletIdNotExistInInvLoc = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[0], "PLT2", true);

			var lineWithPalletIdDuplicatedInDifferentInvLoc = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[1], "PLT1", true);
			var lineWithPaletIdDuplicatedWithinStocktake = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[1], "PLT2", true);
			var lineWithCorrectPalletIdWithDifferentProduct = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, locations[0], "PLT1", true);

			AssertNoErrors("Since pallet id is empty, no erros expected.", lineWithEmptyPalletId.WU_PalletIDInfo);
			AssertNoErrors("Since pallet id exists in the inventory, no errors expected.", lineWithCorrectPalletId.WU_PalletIDInfo);
			AssertNoErrors("Since pallet id is correct, no errors expected.", lineWithCorrectPalletIdWithSameProduct.WU_PalletIDInfo);
			AssertNoErrors("Since pallet id is correct, no errors expected.", lineWithCorrectPalletIdWithDifferentProduct.WU_PalletIDInfo);
			AssertNoErrors("Since pallet id is not duplicated, no errors expected.", lineWithDifferentPalletIdNotExistInInvLoc.WU_PalletIDInfo);

			AssertHasError(lineWithPalletIdDuplicatedInDifferentInvLoc.WU_PalletIDInfo, string.Format("Another location ({0}) was already used for the same Pallet ID. Please select another location or Pallet ID.", locations[0].ToLocationString()));
			AssertHasError(lineWithPaletIdDuplicatedWithinStocktake.WU_PalletIDInfo, string.Format("Another location ({0}) was already used for the same Pallet ID. Please select another location or Pallet ID.", locations[0].ToLocationString()));
		}

		#endregion

		#region TestWU_PalletIDForReleasedProducts

		public void TestWU_PalletIDForReleasedProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var locationWithPalletA = data.Whs1.FindLocation("A-1-1");
			var differentLocation = data.Whs1.FindLocation("A-1-2");

			// create receive, order and pick
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationWithPalletA, "A");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, differentLocation, "A", true);
			AssertNoErrors("No errors should be WU_PalletIDInfo since all products in pallet A was released.", line.WU_PalletIDInfo);
		}

		#endregion

		#region TestWU_PalletID_InTransitInventory

		public void TestWU_PalletID_InTransitInventory()
		{
			// Objective: Test that PalletID validation avoids using same pallet ID when an inTransit transfer is being using it previously.
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");
			var expectedErrorMsg = "Another location (A-2-1) was already used for the same Pallet ID. Please select another location or Pallet ID.";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT123");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
			var transferLine1 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 100m, locationA, "PLT123", locationB, "PLT123", picker);
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locationA, "PLT123", isManuallyAddedLine: true);

			AssertHasError("Should show errors since pallet is already being used.", stocktakeLine.WU_PalletIDInfo, expectedErrorMsg);

			stocktakeLine.WU_WL = locationB.PK;
			AssertHasError("Should show errors since inventory didn't arrive yet to destination location.", stocktakeLine.WU_PalletIDInfo, expectedErrorMsg);
		}

		#endregion

		#region TestWU_PalletID_Staged

		public void TestWU_PalletID_Staged()
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

			var expectedErrorMsg = "Another location (DOCKDOOR) was already used for the same Pallet ID. Please select another location or Pallet ID.";
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, "PLT-123", isManuallyAddedLine: true);
			AssertHasError("Should show errors since pallet is already being used.", stocktakeLine.WU_PalletIDInfo, expectedErrorMsg);

			stocktakeLine.WU_WL = data.Whs1.WW_DefaultOutboundDockDoor;
			stocktakeLine.RunPreSaveValidation();
			AssertNoError("Should have no errors.", stocktakeLine.WU_PalletIDInfo, expectedErrorMsg);
		}

		#endregion

		#region TestWU_PalletID_MaxLength

		public void TestWU_PalletID_MaxLength()
		{
			var stocktakeLine = Factory.New<WhsStocktakeLine>();
			AssertNoExceptionThrown(() => stocktakeLine.WU_PalletID = "123456789012345678901234567890");
		}

		#endregion

		#region TestWU_PartAttrib1_MaxLength

		public void TestWU_PartAttrib1_MaxLength()
		{
			var stocktakeLine = Factory.New<WhsStocktakeLine>();
			AssertNoExceptionThrown(() => stocktakeLine.WU_PartAttrib1 = "".PadLeft(WhsStocktakeLineSchema.WU_PartAttrib1.MaxLength, 'A'));
		}

		#endregion

		#region TestWU_PartAttrib1_Exceed_MaxLength

		public void TestWU_PartAttrib1_Exceed_MaxLength()
		{
			var stocktakeLine = Factory.New<WhsStocktakeLine>();
			try
			{
				AssertExceptionThrown<MaxLengthExceededException>(() =>
					stocktakeLine.WU_PartAttrib1 = ZString.Replicate('A', WhsStocktakeLineSchema.WU_PartAttrib1.MaxLength + 1));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestWU_PartAttrib2_MaxLength

		public void TestWU_PartAttrib2_MaxLength()
		{
			var stocktakeLine = Factory.New<WhsStocktakeLine>();
			AssertNoExceptionThrown(() => stocktakeLine.WU_PartAttrib2 = "".PadLeft(WhsStocktakeLineSchema.WU_PartAttrib2.MaxLength, 'A'));
		}

		#endregion

		#region TestWU_PartAttrib2_Exceed_MaxLength

		public void TestWU_PartAttrib2_Exceed_MaxLength()
		{
			var stocktakeLine = Factory.New<WhsStocktakeLine>();
			try
			{
				AssertExceptionThrown<MaxLengthExceededException>(() =>
					stocktakeLine.WU_PartAttrib2 = ZString.Replicate('A', WhsStocktakeLineSchema.WU_PartAttrib2.MaxLength + 1));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestWU_PartAttrib3_MaxLength

		public void TestWU_PartAttrib3_MaxLength()
		{
			var stocktakeLine = Factory.New<WhsStocktakeLine>();
			AssertNoExceptionThrown(() => stocktakeLine.WU_PartAttrib3 = "".PadLeft(WhsStocktakeLineSchema.WU_PartAttrib3.MaxLength, 'A'));
		}

		#endregion

		#region TestWU_PartAttrib3_Exceed_MaxLength

		public void TestWU_PartAttrib3_Exceed_MaxLength()
		{
			var stocktakeLine = Factory.New<WhsStocktakeLine>();
			try
			{
				AssertExceptionThrown<MaxLengthExceededException>(() =>
					stocktakeLine.WU_PartAttrib3 = ZString.Replicate('A', WhsStocktakeLineSchema.WU_PartAttrib3.MaxLength + 1));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#endregion

		#region Test Check Last Counts

		#region Check Last count for Attribute neutral products

		#region TestCheckWU_LastCount_AttributeNeutralProducts

		public void TestCheckWU_LastCount_AttributeNeutralProductsForManuallyAddedLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0m, new ZByte(1), StocktakeLineStatus.Codes.Open, true);

			AssertNoError("Pre-condition", line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			data.Org1.PartAttributeManager.SetProductToUseAttribute(data.Part1, 6, true);
			data.Org1.MiscServ.OM_IMUseSerialNumber = true;
			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			line.WU_LastCount = 1m;
			AssertNoError(line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line.WU_LastCount = 2m;
			AssertHasError(line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
		}

		#endregion

		#region TestCheckWU_Count2_AttributeNeutralProducts

		public void TestCheckWU_Count2_AttributeNeutralProductsForManuallyAddedLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2m, new ZByte(2), StocktakeLineStatus.Codes.Open, true);

			AssertNoError("Pre-condition", line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			data.Org1.PartAttributeManager.SetProductToUseAttribute(data.Part1, 6, true);
			data.Org1.MiscServ.OM_IMUseSerialNumber = true;
			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			line.WU_Count2 = 1m;
			AssertNoError(line.WU_Count2Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line.WU_Count2 = 2m;
			AssertHasError(line.WU_Count2Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
		}

		#endregion

		#region TestCheckWU_Count3_AttributeNeutralProducts

		public void TestCheckWU_Count3_AttributeNeutralProductsForManuallyAddedLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktake and line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2m, new ZByte(3), StocktakeLineStatus.Codes.Open, true);

			AssertNoError("Pre-condition", line.WU_LastCountInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			data.Org1.PartAttributeManager.SetProductToUseAttribute(data.Part1, 6, true);
			data.Org1.MiscServ.OM_IMUseSerialNumber = true;
			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			line.WU_Count3 = 1m;
			AssertNoError(line.WU_Count3Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			line.WU_Count3 = 2m;
			AssertHasError(line.WU_Count3Info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
		}

		#endregion

		#endregion

		#region TestCheckWU_LastCount_NonNegative

		public void TestCheckWU_LastCount_NonNegativeManuallyAddedLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var manuallyAddedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0m, new ZByte(1), StocktakeLineStatus.Codes.Open, true);

			manuallyAddedLine.WU_LastCount = -1;
			AssertHasErrors("Should be a non-negative value.", manuallyAddedLine.WU_LastCountInfo);

			manuallyAddedLine.WU_LastCount = 1;
			AssertNoErrors("Should no errors.", manuallyAddedLine.WU_LastCountInfo);
		}

		#endregion

		#region TestCheckWU_Count2_NonNegative

		public void TestCheckWU_Count2_NonNegativeManuallyAddedLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var manuallyAddedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0m, new ZByte(2), StocktakeLineStatus.Codes.Open, true);

			manuallyAddedLine.WU_Count2 = -1;
			AssertHasErrors("Should be a non-negative value.", manuallyAddedLine.WU_Count2Info);

			manuallyAddedLine.WU_Count2 = 1;
			AssertNoErrors("Should no errors.", manuallyAddedLine.WU_Count2Info);
		}

		#endregion

		#region TestCheckWU_Count3_NonNegative

		public void TestCheckWU_Count3_NonNegativeManuallyAddedLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var manuallyAddedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0m, new ZByte(3), StocktakeLineStatus.Codes.Open, true);

			manuallyAddedLine.WU_Count3 = -1;
			AssertHasErrors("Should be a non-negative value.", manuallyAddedLine.WU_Count3Info);

			manuallyAddedLine.WU_Count3 = 1;
			AssertNoErrors("Should no errors.", manuallyAddedLine.WU_Count3Info);
		}

		#endregion

		#endregion
	}
}
