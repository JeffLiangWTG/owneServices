using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business.US;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsStocktakeLine))]
	class WhsStocktakeLineTestCase : WhsBusinessObjectTestCase
	{
		#region TestSaveAndDeleteBusinessObject

		[ExpectNoExceptions]
		public override void TestSaveAndDeleteBusinessObject()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0m, 5m, 1, StocktakeLineStatus.Codes.Open);
			Factory.Save();

			line.Delete();
			Factory.Save();
		}

		#endregion

		#region Related Entities

		#region TestProduct

		public void TestProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktakeLine = Factory.New<WhsStocktakeLine>();
			stocktakeLine.WU_OP = ZGuid.Empty;
			AssertNull("Product should be null.", stocktakeLine.Product);

			stocktakeLine.WU_OP = data.Part1.PK;
			AssertNotNull("Product should not be null.", stocktakeLine.Product);
		}

		#endregion

		#endregion

		#region Properties

		#region TestStatusDesc

		public void TestStatusDesc()
		{
			var line = Factory.New<WhsStocktakeLine>();
			line.WU_InventoryStatus = StocktakeInventoryStatus.Codes.Available;
			AssertEquals(StocktakeInventoryStatus.Descriptions.Available, line.StatusDesc);

			line.WU_InventoryStatus = StocktakeInventoryStatus.Codes.Held;
			AssertEquals(StocktakeInventoryStatus.Descriptions.Held, line.StatusDesc);

			line.WU_InventoryStatus = StocktakeInventoryStatus.Codes.Damaged;
			AssertEquals(StocktakeInventoryStatus.Descriptions.Damaged, line.StatusDesc);
		}

		public void TestStatusDescInfo()
		{
			var line = Factory.New<WhsStocktakeLine>();
			AssertEquals("StatusDesc", line.StatusDescInfo.Name);
		}

		#endregion

		#region DecimalPlaces

		public void TestDecimalPlaces()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var line1 = Factory.New<WhsStocktakeLine>();
			line1.WU_OP = ZGuid.Empty;
			AssertEquals("Expecting default decimal places when supplier part is not valid", 0, line1.DecimalPlaces);

			var line2 = Factory.New<WhsStocktakeLine>();
			line2.WU_OP = data.Part1.PK;
			data.Part1.OP_CountDecimalPlaces = 42;
			AssertEquals("Expecting correct decimal places when supplier part is valid", 42, line2.DecimalPlaces);

			data.Part1.OP_CountDecimalPlaces = 1;
			AssertEquals("Expecting correct decimal places when supplier part is valid", 1, line2.DecimalPlaces);
		}

		public void TestDecimalPlaces_PrecisionAndScale()
		{
			WhsStocktakeLineSchema
				.All
				.OfType<SchemaDecimalColumn>()
				.ForEach(column =>
					{
						AssertEquals("Expecting precision be in sync with WhsDocketLine one", WhsDocketLineSchema.WE_TransactionQuantity.Precision, column.Precision);
						AssertEquals("Expecting scale be in sync with WhsDocketLine one", WhsDocketLineSchema.WE_TransactionQuantity.Scale, column.Scale);
					});
		}

		#endregion

		#region TestWU_OP

		[TestDate(2012, 05, 16)]
		public void TestWU_OP()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			//data.Part2 doesn't use attributes.

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Factory.New<WhsStocktakeLine>();
			line.WU_WS = stocktake.PK;
			line.WU_OH_Client = data.Org1.PK;
			line.WU_OP = data.Part1.PK;
			line.WU_WL = data.Whs1.DefaultLocation.PK;
			line.WU_PartAttrib1 = "P1";
			line.WU_PartAttrib2 = "P2";
			line.WU_PartAttrib3 = "P3";
			line.WU_SerialNumber = "SN";
			line.WU_ExpiryDate = ZDate.Today;
			line.WU_PackingDate = ZDate.Today.AddDays(-1);

			line.WU_OP = data.Part2.PK;
			AssertEquals("", line.WU_PartAttrib1);
			AssertEquals("", line.WU_PartAttrib2);
			AssertEquals("", line.WU_PartAttrib3);
			AssertEquals("", line.WU_SerialNumber);
			AssertEquals(ZDateTime.Empty, line.WU_ExpiryDate);
			AssertEquals(ZDateTime.Empty, line.WU_PackingDate);
		}

		public void TestWU_OP_SetsClientIfItWasEmpty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client2 = Helper.CreateClient("C3");
			var part3 = Helper.CreateProduct(client2, "P3");

			var client3 = Helper.CreateClient("C3");
			var partOnlySupplier = Helper.CreateProduct(client3, "P4", OrgPartRelation.RelationshipTypes.Supplier);

			var stockTake = Helper.CreateWhsStocktake(null, data.Whs1);
			var stockTakeLine = Helper.CreateWhsStocktakeLine(stockTake, null, null);
			stockTakeLine.WU_IsManuallyAdded = true;

			Assert(stockTakeLine.WU_OH_Client.IsEmpty);
			stockTakeLine.WU_OP = partOnlySupplier.PK;
			stockTakeLine.WU_InventoryStatus = InventoryStatus.Codes.Available;
			Assert("As partOnlySupplier has no Owner or Both relationships - client should not be populated on stocktakeline.", stockTakeLine.WU_OH_Client.IsEmpty);

			stockTakeLine.WU_OP = data.Part1.PK;
			AssertEquals("When product is set, an empty client will be replaced with a valid client for a product.", data.Org1.PK, stockTakeLine.WU_OH_Client);
			stockTakeLine.Validation.ValidateWU_OH_Client();
			AssertNoErrors(stockTakeLine.WU_OH_ClientInfo);

			stockTakeLine.WU_OP = part3.PK;
			AssertEquals("As client was set - it should not be changed automatically afterwards.", data.Org1.PK, stockTakeLine.WU_OH_Client);
			stockTakeLine.Validation.ValidateWU_OH_Client();
			AssertHasErrors("There should be an error on client as the product does not match it.", stockTakeLine.WU_OH_ClientInfo);
		}

		#endregion

		#region TestLocation

		public void TestLocation()
		{
			// create environment
			var whs = Helper.CreateWarehouse("AAAA");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2);
			Factory.Save();

			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "AAAA");

			// create stocktake
			var docket = Factory.New<WhsStocktake>();
			var parent = docket.Lines.AddNew();
			docket.WS_WW_Whs = whs.PK;
			parent.WU_OP = part.PK;
			parent.WU_IsManuallyAdded = true;

			// test setting components
			parent.LocationString = "A-2-1";
			AssertEquals("LocationString should not have errors", false, parent.LocationStringInfo.HasErrors());
			AssertEquals("FK must be valid after setting location string", row.Locations[2].PK, parent.WU_WL);

			parent.LocationString = "";
			AssertEquals("LocationString should have error", true, parent.LocationStringInfo.HasErrors());
			AssertEquals("FK should be empty", ZGuid.Empty, parent.WU_WL);
			AssertEquals("Warehouse PK should be set", whs.PK, parent.LocationWhsGuid);

			parent.LocationString = "A";
			AssertEquals("LocationString should not have errors", false, parent.LocationStringInfo.HasErrors());
			AssertEquals("LocationString should = 'A-1-1'", "A-1-1", parent.LocationString);
			AssertEquals("FK must be valid after setting location string", row.Locations[0].PK, parent.WU_WL);
			AssertEquals("Warehouse PK should be set", whs.PK, parent.LocationWhsGuid);

			parent.LocationString = "A-50-50";
			AssertEquals("LocationString should have errors", true, parent.LocationStringInfo.HasErrors());
			AssertEquals("LocationString should = 'A-50-50'", "A-50-50", parent.LocationString);
			AssertEquals("FK should be empty", ZGuid.Empty, parent.WU_WL);
			AssertEquals("Warehouse PK should be set", whs.PK, parent.LocationWhsGuid);

			parent.LocationString = "SHEEP";
			AssertEquals("LocationString should have errors", true, parent.LocationStringInfo.HasErrors());
			AssertEquals("LocationString should = 'SHEEP'", "SHEEP", parent.LocationString);
			AssertEquals("FK should be empty", ZGuid.Empty, parent.WU_WL);
			AssertEquals("Warehouse PK should be set", whs.PK, parent.LocationWhsGuid);
		}

		public void TestLocation_FixedWidthLocation()
		{
			var whs = Helper.CreateFixedWidthLocationWarehouse("1", 3, 3, 2);
			Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2);
			Factory.Save();

			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "AAAA");

			// create stocktake
			var docket = Factory.New<WhsStocktake>();
			var parent = docket.Lines.AddNew();
			docket.WS_WW_Whs = whs.PK;
			parent.WU_OP = part.PK;
			parent.WU_IsManuallyAdded = true;

			var location = whs.FindLocation("A002001");
			// test setting components
			parent.LocationString = "A002001";
			AssertEquals("LocationString should not have errors", false, parent.LocationStringInfo.HasErrors());
			AssertEquals("LocationString is the user friendly string.", "A-002-001", parent.LocationString);
			AssertEquals("FK must be valid after setting location string", location.PK, parent.WU_WL);

			parent.LocationString = "";
			AssertEquals("LocationString should have error", true, parent.LocationStringInfo.HasErrors());
			AssertEquals("FK should be empty", ZGuid.Empty, parent.WU_WL);
			AssertEquals("Warehouse PK should be set", whs.PK, parent.LocationWhsGuid);

			parent.LocationString = "A002";
			AssertEquals("LocationString should not have errors", false, parent.LocationStringInfo.HasErrors());
			AssertEquals("LocationString is the user friendly string.", "A-002-001", parent.LocationString);
			AssertEquals("FK must be valid after setting location string", location.PK, parent.WU_WL);
			AssertEquals("Warehouse PK should be set", whs.PK, parent.LocationWhsGuid);

			parent.LocationString = "A-50-50";
			AssertEquals("LocationString should have errors", true, parent.LocationStringInfo.HasErrors());
			AssertEquals("LocationString should = 'A-50-50'", "A-50-50", parent.LocationString);
			AssertEquals("FK should be empty", ZGuid.Empty, parent.WU_WL);
			AssertEquals("Warehouse PK should be set", whs.PK, parent.LocationWhsGuid);

			parent.LocationString = "A-002";
			AssertEquals("LocationString should not have errors", false, parent.LocationStringInfo.HasErrors());
			AssertEquals("LocationString is the user friendly string.", "A-002-001", parent.LocationString);
			AssertEquals("FK must be valid after setting location string", location.PK, parent.WU_WL);
			AssertEquals("Warehouse PK should be set", whs.PK, parent.LocationWhsGuid);

			parent.LocationString = "SHEEP";
			AssertEquals("LocationString should have errors", true, parent.LocationStringInfo.HasErrors());
			AssertEquals("LocationString should = 'SHEEP'", "SHEEP", parent.LocationString);
			AssertEquals("FK should be empty", ZGuid.Empty, parent.WU_WL);
			AssertEquals("Warehouse PK should be set", whs.PK, parent.LocationWhsGuid);

			parent.LocationString = "A-002-001";
			AssertEquals("LocationString should not have errors", false, parent.LocationStringInfo.HasErrors());
			AssertEquals("LocationString is the user friendly string.", "A-002-001", parent.LocationString);
			AssertEquals("FK must be valid after setting location string", location.PK, parent.WU_WL);
			AssertEquals("Warehouse PK should be set", whs.PK, parent.LocationWhsGuid);
		}

		#endregion

		#region TestCommodityCode

		public void TestCommodityCode()
		{
			var line = (WhsStocktakeLine)GetNewBusinessObject();
			line.WU_OP = Factory.New<OrgSupplierPart>().PK;
			line.SupplierPart.OP_RH_NKCommodityCode = "CC1";
			AssertEquals("CC1", line.CommodityCode);
		}

		#endregion

		#region TestLocationValidation

		public void TestLocationValidation()
		{
			// create environment
			var whs = Helper.CreateWarehouse("AAAA");
			var client = Helper.CreateClient();
			var product = Helper.CreateProduct(client, "P1");
			Factory.Save();

			// create stcoktake line
			var stocktake = Helper.CreateWhsStocktake(client, whs);
			var line = Helper.CreateWhsStocktakeLine(stocktake, client, product);
			line.WU_IsManuallyAdded = true;

			// Assign location string which does not exists
			line.LocationString = "A-1";
			AssertHasErrors(line.LocationStringInfo);

			// Run validate all after creating location
			Helper.CreateRowAndGenerateLocations(whs, "A", 2, 1);
			line.Validation.ValidateAll();
			AssertHasErrors(line.LocationStringInfo);

			line.WU_WL = whs.Rows.Single(r => r.WR_Name == "A").Locations[1].PK;
			Factory.Save();

			// Reset the locationstring
			line.LocationString = "A-1";
			AssertNoErrors(line.LocationStringInfo);
		}

		#endregion

		#region TestCurrentCount_LastCountColumnNumberOne

		[TestDate(2012, 1, 1)]
		public void TestCurrentCount_LastCountColumnNumberOne()
		{
			SetupEnvironnemntForCurrentCount();

			Line1.WU_TotalCounts = 1;
			Line1.WU_LastCount = 2;
			Line1.WU_DateVerified = ZDateTime.Now;
			Line1.WU_GS_NKVerifiedBy = "TS";

			AssertEquals(Line1.WU_LastCount, Line1.CurrentCount);
			AssertEquals(Line1.WU_GS_NKVerifiedBy, Line1.CurrentCountVerifiedBy);
			AssertEquals(Line1.WU_DateVerified, Line1.CurrentCountVerifiedDate);
		}

		#endregion

		#region TestCurrentCount_LastCountColumnNumberTwo

		[TestDate(2012, 1, 1)]
		public void TestCurrentCount_LastCountColumnNumberTwo()
		{
			SetupEnvironnemntForCurrentCount();

			Line1.WU_TotalCounts = 2;
			Line1.WU_Count2 = 2;
			Line1.WU_Count2DateVerified = ZDateTime.Now;
			Line1.WU_Count2VerifiedBy = "TS";

			AssertEquals(Line1.WU_Count2, Line1.CurrentCount);
			AssertEquals(Line1.WU_Count2VerifiedBy, Line1.CurrentCountVerifiedBy);
			AssertEquals(Line1.WU_Count2DateVerified, Line1.CurrentCountVerifiedDate);
		}

		#endregion

		#region TestCurrentCount_LastCountColumnNumberThree

		[TestDate(2012, 1, 1)]
		public void TestCurrentCount_LastCountColumnNumberThree()
		{
			SetupEnvironnemntForCurrentCount();

			Line1.WU_TotalCounts = 3;
			Line1.WU_Count3 = 2;
			Line1.WU_Count3DateVerified = ZDateTime.Now;
			Line1.WU_Count3VerifiedBy = "TS";

			AssertEquals(Line1.WU_Count3, Line1.CurrentCount);
			AssertEquals(Line1.WU_Count3VerifiedBy, Line1.CurrentCountVerifiedBy);
			AssertEquals(Line1.WU_Count3DateVerified, Line1.CurrentCountVerifiedDate);
		}

		#endregion

		#region TestWU_LastCount

		[TestDate(2012, 2, 1)]
		public void TestWU_LastCount()
		{
			// Setup test data
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation);

			//line.WU_DateVerified is empty
			line.WU_LastCount = 1;
			AssertEquals("WU_DateVerified is empty, WU_DateVerified should be updated to ZDatetime.Now", line.WU_DateVerified, now);

			line.WU_DateVerified = now.AddDays(2); // Change the datetime so that we are sure we do not update it to the current datetime.
			line.WU_LastCount = 2;
			AssertEquals("Since WU_DateVerified is not empty WU_DateVerified should not be changed.", line.WU_DateVerified, now.AddDays(2));
		}

		#endregion

		#region TestWU_2ndLastCount

		[TestDate(2012, 2, 1)]
		public void TestWU_2ndLastCount()
		{
			// Setup test data
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation);

			//line.WU_DateVerified is empty
			line.WU_Count2 = 1;
			AssertEquals("Since WU_Count2 is empty, WU_Count2DateVerified should be updated to ZDatetime.Now", line.WU_Count2DateVerified, now);

			line.WU_Count2DateVerified = now.AddDays(2); // Change the datetime so that we are sure we do not update it to the current datetime.
			line.WU_Count2 = 2;
			AssertEquals("Since WU_Count2 is not empty WU_Count2DateVerified should not be changed.", line.WU_Count2DateVerified, now.AddDays(2));
		}

		#endregion

		#region TestWU_3rdLastCount

		[TestDate(2012, 2, 1)]
		public void TestWU_3rdLastCount()
		{
			// Setup test data
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation);

			//line.WU_DateVerified is empty
			line.WU_Count3 = 1;
			AssertEquals("WU_Count3 is empty, WU_Count3DateVerified should be updated to ZDatetime.Now", line.WU_Count3DateVerified, now);

			line.WU_Count3DateVerified = now.AddDays(2); // Change the datetime so that we are sure we do not update it to the current datetime.
			line.WU_Count3 = 2;
			AssertEquals("Since WU_Count3 is not empty WU_Count3DateVerified should not be changed.", line.WU_Count3DateVerified, now.AddDays(2));
		}

		#endregion

		#region StocktakeExpiryDateNumberOfFutureYears

		public void TestCreateStocktakeWithExpiryDate5YearsLater()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation);
			line.WU_ExpiryDate = ZDate.Today.AddYears(6);
			AssertNoErrors("Should not have any errors, expiry date 6 years in the future should not be a problem", line.WU_ExpiryDateInfo);

			line.WU_ExpiryDate = ZDate.Today.AddYears(DateRangeValidation.MaximumFutureYears + 1);
			AssertHasErrors("Error too far into the future", line.WU_ExpiryDateInfo);
		}

		#endregion

		#region Attributes

		#region TestWU_PartAttrib1

		[TestDate(2013, 2, 26)]
		public void TestWU_PartAttrib1_SetExpiryDateIfJulianBatchNumberIsUsed()
		{
			TestWU_PartAttrib_SetExpiryDateAndPackingDateIfJulianBatchNumberIsUsedCore(WhsStocktakeLineSchema.WU_PartAttrib1, AttributeNumber.One);
		}

		#endregion

		#region TestWU_PartAttrib2

		[TestDate(2013, 2, 26)]
		public void TestWU_PartAttrib2_SetExpiryDateIfJulianBatchNumberIsUsed()
		{
			TestWU_PartAttrib_SetExpiryDateAndPackingDateIfJulianBatchNumberIsUsedCore(WhsStocktakeLineSchema.WU_PartAttrib2, AttributeNumber.Two);
		}

		#endregion

		#region TestWU_PartAttrib3

		[TestDate(2013, 2, 26)]
		public void TestWU_PartAttrib3_SetExpiryDateIfJulianBatchNumberIsUsed()
		{
			TestWU_PartAttrib_SetExpiryDateAndPackingDateIfJulianBatchNumberIsUsedCore(WhsStocktakeLineSchema.WU_PartAttrib3, AttributeNumber.Three);
		}

		#endregion

		#region TestWU_PartAttrib_SetExpiryDateIfJulianBatchNumberIsUsedCore

		void TestWU_PartAttrib_SetExpiryDateAndPackingDateIfJulianBatchNumberIsUsedCore(SchemaStringColumn partAttributeColumn, AttributeNumber attributeNumber)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true);
			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;

			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParam.W3_MaximumShelfLife = 10;

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1);
			stocktakeLine.WU_IsManuallyAdded = true;

			// Normal Attribute
			AssertEquals("Precondition", ZDateTime.Empty, stocktakeLine.WU_ExpiryDate);

			stocktakeLine[partAttributeColumn] = "ABC1005";
			AssertEquals("Expiry date should not be modified when normal attribute is set.", ZDateTime.Empty, stocktakeLine.WU_ExpiryDate);

			// Julian Batch Number Attribute
			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.JulianBatchNumber); // should turn on Expiry Date
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true); // should turn on Expiry Date
			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;

			stocktakeLine[partAttributeColumn] = "";
			AssertEquals("Expiry date should be empty when Julian Batch Number is not set.", ZDateTime.Empty, stocktakeLine.WU_ExpiryDate);
			AssertEquals("Packing date should be empty when Julian Batch Number is not set.", ZDateTime.Empty, stocktakeLine.WU_PackingDate);

			stocktakeLine[partAttributeColumn] = "ABC1005";
			AssertEquals("Expiry date should be set when Julian Batch Number is set.", new ZDateTime(2010 + 1, 1, 1).AddDays((005 - 1) + 10), stocktakeLine.WU_ExpiryDate);
			AssertEquals("Packing date should be set when Julian Batch Number is set.", new ZDateTime(2010 + 1, 1, 1).AddDays((005 - 1)), stocktakeLine.PackingDate);

			stocktakeLine[partAttributeColumn] = "ABC4050";
			AssertEquals("Expiry date should be set when Julian Batch Number is set.", new ZDateTime(2000 + 4, 1, 1).AddDays((050 - 1) + 10), stocktakeLine.WU_ExpiryDate);
			AssertEquals("Packing date should be set when Julian Batch Number is set.", new ZDateTime(2000 + 4, 1, 1).AddDays((050 - 1)), stocktakeLine.PackingDate);

			stocktakeLine[partAttributeColumn] = "1005ABC";
			AssertEquals("Expiry date should be empty when Julian Batch Number is incorrect.", ZDateTime.Empty, stocktakeLine.WU_ExpiryDate);
			AssertEquals("Packing date should be empty when Julian Batch Number is incorrect.", ZDateTime.Empty, stocktakeLine.WU_PackingDate);
		}

		#endregion

		#endregion

		#region ReadOnly

		#region TestNonStandardOrEmptyReadOnly

		public void TestNonStandardOrEmptyReadOnly()
		{
			// Setup test data
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var openLine = CreateDummyStocktakeLine(stocktake, false, StocktakeLineStatus.Codes.Open);
			var closedLine = CreateDummyStocktakeLine(stocktake, false, StocktakeLineStatus.Codes.Closed);
			var emptyLine = CreateDummyStocktakeLine(stocktake, false, StocktakeLineStatus.Codes.Closed);
			emptyLine.WU_OP = Guid.Empty;
			var lineWithoutStocktake = Factory.New<DummyStocktakeLine>();

			// test with loaded stocktake

			AssertEquals(false, openLine.NonStandardOrEmptyReadOnlyExposed);
			AssertEquals(true, closedLine.NonStandardOrEmptyReadOnlyExposed);
			AssertEquals(true, lineWithoutStocktake.NonStandardOrEmptyReadOnlyExposed);
			AssertEquals(true, emptyLine.NonStandardOrEmptyReadOnlyExposed);

			// test with finalised stocktake

			stocktake.WS_StocktakeStatus = StocktakeStatus.Codes.Finalised;
			AssertEquals(true, openLine.NonStandardOrEmptyReadOnlyExposed);
			AssertEquals(true, lineWithoutStocktake.NonStandardOrEmptyReadOnlyExposed);
			AssertEquals(true, lineWithoutStocktake.NonStandardOrEmptyReadOnlyExposed);
			AssertEquals(true, emptyLine.NonStandardOrEmptyReadOnlyExposed);
		}

		#endregion

		#region TestNonStandardReadOnly

		public void TestNonStandardReadOnly()
		{
			// Setup test data
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var openLine = CreateDummyStocktakeLine(stocktake, false, StocktakeLineStatus.Codes.Open);
			var closedLine = CreateDummyStocktakeLine(stocktake, false, StocktakeLineStatus.Codes.Closed);
			var lineWithoutStocktake = Factory.New<DummyStocktakeLine>();

			// test with loaded stocktake

			AssertEquals(false, openLine.NonStandardReadOnlyExposed);
			AssertEquals(true, closedLine.NonStandardReadOnlyExposed);
			AssertEquals(true, lineWithoutStocktake.NonStandardReadOnlyExposed);

			// test with finalised stocktake

			stocktake.WS_StocktakeStatus = StocktakeStatus.Codes.Finalised;
			AssertEquals(true, openLine.NonStandardReadOnlyExposed);
			AssertEquals(true, lineWithoutStocktake.NonStandardReadOnlyExposed);
			AssertEquals(true, lineWithoutStocktake.NonStandardReadOnlyExposed);
		}

		#endregion

		#region TestAutoLoadedStandardReadOnly

		public void TestAutoLoadedStandardReadOnly()
		{
			// Setup test data
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);

			var autoLoadedOpenLine = CreateDummyStocktakeLine(stocktake, false, StocktakeLineStatus.Codes.Open);
			var manuallyLoadedOpenLine = CreateDummyStocktakeLine(stocktake, true, StocktakeLineStatus.Codes.Open);
			var autoLoadedClosedLine = CreateDummyStocktakeLine(stocktake, false, StocktakeLineStatus.Codes.Closed);
			var manuallyLoadedClosedLine = CreateDummyStocktakeLine(stocktake, true, StocktakeLineStatus.Codes.Closed);

			AssertEquals(true, autoLoadedOpenLine.AutoLoadedReadOnlyExposed);
			AssertEquals(false, manuallyLoadedOpenLine.AutoLoadedReadOnlyExposed);
			AssertEquals(true, autoLoadedClosedLine.AutoLoadedReadOnlyExposed);
			AssertEquals(true, manuallyLoadedClosedLine.AutoLoadedReadOnlyExposed);
		}

		#endregion

		#region TestClientReadOnly

		public void TestClientReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(null, data.Whs1, StocktakeStatus.Codes.Loaded);

			var autoLoadedOpenLine = CreateDummyStocktakeLine(stocktake, false, StocktakeLineStatus.Codes.Open);
			var manuallyLoadedOpenLine = CreateDummyStocktakeLine(stocktake, true, StocktakeLineStatus.Codes.Open);
			var autoLoadedClosedLine = CreateDummyStocktakeLine(stocktake, false, StocktakeLineStatus.Codes.Closed);
			var manuallyLoadedClosedLine = CreateDummyStocktakeLine(stocktake, true, StocktakeLineStatus.Codes.Closed);

			// when there is no client on Stocktake - Client on line level is editable only for manually added open lines.
			AssertEquals(true, autoLoadedOpenLine.ClientReadOnlyExposed);
			AssertEquals(false, manuallyLoadedOpenLine.ClientReadOnlyExposed);
			AssertEquals(true, autoLoadedClosedLine.ClientReadOnlyExposed);
			AssertEquals(true, manuallyLoadedClosedLine.ClientReadOnlyExposed);

			// when there is a client on Stocktake - Client on line level is always readonly
			stocktake.WS_OH_Client = data.Org1.PK;
			AssertEquals(true, autoLoadedOpenLine.ClientReadOnlyExposed);
			AssertEquals(true, manuallyLoadedOpenLine.ClientReadOnlyExposed);
			AssertEquals(true, autoLoadedClosedLine.ClientReadOnlyExposed);
			AssertEquals(true, manuallyLoadedClosedLine.ClientReadOnlyExposed);
		}

		#endregion

		#region TestPartAttribute1ReadOnly

		public void TestPartAttribute1ReadOnly()
		{
			// Setup test data
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, false);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);

			var lineWithEmptyWU_OP = CreateDummyStocktakeLine(stocktake, ZGuid.Empty, true, StocktakeLineStatus.Codes.Open);
			var lineWithInvalidProduct = CreateDummyStocktakeLine(stocktake, ZGuid.Invalid, true, StocktakeLineStatus.Codes.Open);
			var lineWithProductAttribute1 = CreateDummyStocktakeLine(stocktake, data.Part1.PK, true, StocktakeLineStatus.Codes.Open);
			var lineWithoutProductAttribute1 = CreateDummyStocktakeLine(stocktake, data.Part2.PK, true, StocktakeLineStatus.Codes.Open);
			var closedLine = CreateDummyStocktakeLine(stocktake, data.Part1.PK, true, StocktakeLineStatus.Codes.Closed);
			var autoLoadedOpenLineLine = CreateDummyStocktakeLine(stocktake, data.Part1.PK, false, StocktakeLineStatus.Codes.Open);

			AssertEquals(true, lineWithEmptyWU_OP.PartAttribute1ReadOnlyExposed);
			AssertEquals(true, lineWithInvalidProduct.PartAttribute1ReadOnlyExposed);
			AssertEquals(false, lineWithProductAttribute1.PartAttribute1ReadOnlyExposed);
			AssertEquals(true, lineWithoutProductAttribute1.PartAttribute1ReadOnlyExposed);
			AssertEquals(true, closedLine.PartAttribute1ReadOnlyExposed);
			AssertEquals(true, autoLoadedOpenLineLine.ExpiryDateReadOnlyExposed);
		}

		#endregion

		#region TestPartAttribute2ReadOnly

		public void TestPartAttribute2ReadOnly()
		{
			// Setup test data
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Two, false);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);

			var lineWithEmptyWU_OP = CreateDummyStocktakeLine(stocktake, ZGuid.Empty, true, StocktakeLineStatus.Codes.Open);
			var lineWithInvalidProduct = CreateDummyStocktakeLine(stocktake, ZGuid.Invalid, true, StocktakeLineStatus.Codes.Open);
			var lineWithProductAttribute2 = CreateDummyStocktakeLine(stocktake, data.Part1.PK, true, StocktakeLineStatus.Codes.Open);
			var lineWithoutProductAttribute2 = CreateDummyStocktakeLine(stocktake, data.Part2.PK, true, StocktakeLineStatus.Codes.Open);
			var closedLine = CreateDummyStocktakeLine(stocktake, data.Part1.PK, true, StocktakeLineStatus.Codes.Closed);
			var autoLoadedOpenLineLine = CreateDummyStocktakeLine(stocktake, data.Part1.PK, false, StocktakeLineStatus.Codes.Open);

			AssertEquals(true, lineWithEmptyWU_OP.PartAttribute2ReadOnlyExposed);
			AssertEquals(true, lineWithInvalidProduct.PartAttribute2ReadOnlyExposed);
			AssertEquals(false, lineWithProductAttribute2.PartAttribute2ReadOnlyExposed);
			AssertEquals(true, lineWithoutProductAttribute2.PartAttribute2ReadOnlyExposed);
			AssertEquals(true, closedLine.PartAttribute2ReadOnlyExposed);
			AssertEquals(true, autoLoadedOpenLineLine.ExpiryDateReadOnlyExposed);
		}

		#endregion

		#region TestPartAttribute3ReadOnly

		public void TestPartAttribute3ReadOnly()
		{
			// Setup test data
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Three, false);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);

			var lineWithEmptyWU_OP = CreateDummyStocktakeLine(stocktake, ZGuid.Empty, true, StocktakeLineStatus.Codes.Open);
			var lineWithInvalidProduct = CreateDummyStocktakeLine(stocktake, ZGuid.Invalid, true, StocktakeLineStatus.Codes.Open);
			var lineWithProductAttribute3 = CreateDummyStocktakeLine(stocktake, data.Part1.PK, true, StocktakeLineStatus.Codes.Open);
			var lineWithoutProductAttribute3 = CreateDummyStocktakeLine(stocktake, data.Part2.PK, true, StocktakeLineStatus.Codes.Open);
			var closedLine = CreateDummyStocktakeLine(stocktake, data.Part1.PK, true, StocktakeLineStatus.Codes.Closed);
			var autoLoadedOpenLineLine = CreateDummyStocktakeLine(stocktake, data.Part1.PK, false, StocktakeLineStatus.Codes.Open);

			AssertEquals(true, lineWithEmptyWU_OP.PartAttribute3ReadOnlyExposed);
			AssertEquals(true, lineWithInvalidProduct.PartAttribute3ReadOnlyExposed);
			AssertEquals(false, lineWithProductAttribute3.PartAttribute3ReadOnlyExposed);
			AssertEquals(true, lineWithoutProductAttribute3.PartAttribute3ReadOnlyExposed);
			AssertEquals(true, closedLine.PartAttribute3ReadOnlyExposed);
			AssertEquals(true, autoLoadedOpenLineLine.ExpiryDateReadOnlyExposed);
		}

		#endregion

		#region TestSerialNumberReadOnly

		public void TestSerialNumberReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, false);

			var stocktake1 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);

			var lineWithEmptyWU_OP = CreateDummyStocktakeLine(stocktake1, ZGuid.Empty, true, StocktakeLineStatus.Codes.Open);
			var lineWithInvalidProduct = CreateDummyStocktakeLine(stocktake1, ZGuid.Invalid, true, StocktakeLineStatus.Codes.Open);
			var lineWithProductSerialNumber = CreateDummyStocktakeLine(stocktake1, data.Part1.PK, true, StocktakeLineStatus.Codes.Open);
			var lineWithoutProductSerialNumber = CreateDummyStocktakeLine(stocktake1, data.Part2.PK, true, StocktakeLineStatus.Codes.Open);
			var closedLine = CreateDummyStocktakeLine(stocktake1, data.Part1.PK, true, StocktakeLineStatus.Codes.Closed);
			var autoLoadedOpenLineLine = CreateDummyStocktakeLine(stocktake1, data.Part1.PK, false, StocktakeLineStatus.Codes.Open);

			var stocktake2 = Helper.CreateWhsStocktake(null, data.Whs1, StocktakeStatus.Codes.Loaded);
			var lineOnStocktake2WithProductSerialNumber = CreateDummyStocktakeLine(stocktake1, data.Part1.PK, true, StocktakeLineStatus.Codes.Open);
			lineOnStocktake2WithProductSerialNumber.WU_OH_Client = data.Org1.PK;
			AssertEquals("Readonly lineWithEmptyWU_OP", true, lineWithEmptyWU_OP.WU_SerialNumberInfo.ReadOnly);
			AssertEquals("Readonly lineWithInvalidProduct", true, lineWithInvalidProduct.WU_SerialNumberInfo.ReadOnly);
			AssertEquals("Readonly lineWithoutProductSerialNumber ", true, lineWithoutProductSerialNumber.WU_SerialNumberInfo.ReadOnly);
			AssertEquals("Readonly closedLine", true, closedLine.WU_SerialNumberInfo.ReadOnly);
			AssertEquals("Readonly autoLoadedOpenLineLine", true, autoLoadedOpenLineLine.WU_SerialNumberInfo.ReadOnly);
			AssertEquals("Not-readonly lineWithProductSerialNumber", false, lineWithProductSerialNumber.WU_SerialNumberInfo.ReadOnly);
			AssertEquals("Not-readonly lineOnStocktake2WithProductSerialNumber", false, lineOnStocktake2WithProductSerialNumber.WU_SerialNumberInfo.ReadOnly);
		}

		#endregion

		#region TestExpiryDateReadOnly

		#region TestExpiryDateReadOnly

		public void TestExpiryDateReadOnly()
		{
			// Setup test data
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.ExpiryDate, false);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);

			var lineWithEmptyWU_OP = CreateDummyStocktakeLine(stocktake, ZGuid.Empty, true, StocktakeLineStatus.Codes.Open);
			var lineWithInvalidProduct = CreateDummyStocktakeLine(stocktake, ZGuid.Invalid, true, StocktakeLineStatus.Codes.Open);
			var lineWithExpiryDate = CreateDummyStocktakeLine(stocktake, data.Part1.PK, true, StocktakeLineStatus.Codes.Open);
			var lineWithoutExpiryDate = CreateDummyStocktakeLine(stocktake, data.Part2.PK, true, StocktakeLineStatus.Codes.Open);
			var closedLine = CreateDummyStocktakeLine(stocktake, data.Part1.PK, true, StocktakeLineStatus.Codes.Closed);
			var autoLoadedOpenLineLine = CreateDummyStocktakeLine(stocktake, data.Part1.PK, false, StocktakeLineStatus.Codes.Open);

			AssertEquals(true, lineWithEmptyWU_OP.ExpiryDateReadOnlyExposed);
			AssertEquals(true, lineWithInvalidProduct.ExpiryDateReadOnlyExposed);
			AssertEquals(false, lineWithExpiryDate.ExpiryDateReadOnlyExposed);
			AssertEquals(true, lineWithoutExpiryDate.ExpiryDateReadOnlyExposed);
			AssertEquals(true, closedLine.ExpiryDateReadOnlyExposed);
			AssertEquals(true, autoLoadedOpenLineLine.ExpiryDateReadOnlyExposed);
		}

		#endregion

		#region TestExpiryDateReadOnly_WithJulianBatchNumberAttribute

		public void TestExpiryDateReadOnly_WithJulianBatchNumberAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, true);
			TestExpiryDateAndPackingDateReadOnly_WithJulianBatchNumberAttributeCore(data.Org1, data.Part1, stocktakeLine.WU_ExpiryDateInfo, AttributeNumber.One);
			TestExpiryDateAndPackingDateReadOnly_WithJulianBatchNumberAttributeCore(data.Org1, data.Part1, stocktakeLine.WU_ExpiryDateInfo, AttributeNumber.Two);
			TestExpiryDateAndPackingDateReadOnly_WithJulianBatchNumberAttributeCore(data.Org1, data.Part1, stocktakeLine.WU_ExpiryDateInfo, AttributeNumber.Three);
		}

		public void TestPackingDateReadOnly_WithJulianBatchNumberAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, true);
			TestExpiryDateAndPackingDateReadOnly_WithJulianBatchNumberAttributeCore(data.Org1, data.Part1, stocktakeLine.WU_PackingDateInfo, AttributeNumber.One);
			TestExpiryDateAndPackingDateReadOnly_WithJulianBatchNumberAttributeCore(data.Org1, data.Part1, stocktakeLine.WU_PackingDateInfo, AttributeNumber.Two);
			TestExpiryDateAndPackingDateReadOnly_WithJulianBatchNumberAttributeCore(data.Org1, data.Part1, stocktakeLine.WU_PackingDateInfo, AttributeNumber.Three);
		}

		void TestExpiryDateAndPackingDateReadOnly_WithJulianBatchNumberAttributeCore(OrgHeader client, OrgSupplierPart part, ZPropertyInfo propertyInfo, AttributeNumber attributeNumber)
		{
			Helper.SetClientAttributeType(client, attributeNumber, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(client, part, attributeNumber, true);
			AssertEquals(false, propertyInfo.ReadOnly);

			Helper.SetClientAttributeType(client, attributeNumber, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(client, part, attributeNumber, true);
			AssertEquals(true, propertyInfo.ReadOnly);
			Helper.SetProductAttributeUse(client, part, attributeNumber, false); // clean up
		}

		#endregion

		#endregion

		#region TestPackingDateReadOnly

		public void TestPackingDateReadOnly()
		{
			// Setup test data
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.PackingDate, false);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);

			var lineWithEmptyWU_OP = CreateDummyStocktakeLine(stocktake, ZGuid.Empty, true, StocktakeLineStatus.Codes.Open);
			var lineWithInvalidProduct = CreateDummyStocktakeLine(stocktake, ZGuid.Invalid, true, StocktakeLineStatus.Codes.Open);
			var lineWithPackingDate = CreateDummyStocktakeLine(stocktake, data.Part1.PK, true, StocktakeLineStatus.Codes.Open);
			var lineWithoutPackingDate = CreateDummyStocktakeLine(stocktake, data.Part2.PK, true, StocktakeLineStatus.Codes.Open);
			var closedLine = CreateDummyStocktakeLine(stocktake, data.Part1.PK, true, StocktakeLineStatus.Codes.Closed);

			AssertEquals(true, lineWithEmptyWU_OP.PackingDateReadOnlyExposed);
			AssertEquals(true, lineWithInvalidProduct.PackingDateReadOnlyExposed);
			AssertEquals(false, lineWithPackingDate.PackingDateReadOnlyExposed);
			AssertEquals(true, lineWithoutPackingDate.PackingDateReadOnlyExposed);
			AssertEquals(true, closedLine.PackingDateReadOnlyExposed);
		}

		#endregion

		#region TestPartAttributeReadOnly_NoClientOnStocktake

		public void TestPartAttributeReadOnly_NoClientOnStocktake()
		{
			AssertPartAttributeReadOnly_NoClientOnStocktake(AttributeNumber.One, "Attribute 1", d => d.WU_PartAttrib1Info.ReadOnly);
			AssertPartAttributeReadOnly_NoClientOnStocktake(AttributeNumber.Two, "Attribute 2", d => d.WU_PartAttrib2Info.ReadOnly);
			AssertPartAttributeReadOnly_NoClientOnStocktake(AttributeNumber.Three, "Attribute 3", d => d.WU_PartAttrib3Info.ReadOnly);
			AssertPartAttributeReadOnly_NoClientOnStocktake(AttributeNumber.ExpiryDate, "Expiry Date", d => d.WU_ExpiryDateInfo.ReadOnly);
			AssertPartAttributeReadOnly_NoClientOnStocktake(AttributeNumber.PackingDate, "Packing Date", d => d.WU_PackingDateInfo.ReadOnly);
		}

		void AssertPartAttributeReadOnly_NoClientOnStocktake(AttributeNumber number, string attributeNumber, Func<DummyStocktakeLine, bool> attributeReadonlyExposed)
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			Helper.SetClientAttributeType(data.Org1, number, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, number, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, number, false);

			var stocktake = Helper.CreateWhsStocktake(null, data.Whs1, StocktakeStatus.Codes.Loaded);

			var lineWithEmptyWU_OP = CreateDummyStocktakeLine(stocktake, ZGuid.Empty, true, StocktakeLineStatus.Codes.Open);
			var lineWithInvalidProduct = CreateDummyStocktakeLine(stocktake, ZGuid.Invalid, true, StocktakeLineStatus.Codes.Open);
			var lineWithProductAttribute = CreateDummyStocktakeLine(stocktake, data.Part1.PK, true, StocktakeLineStatus.Codes.Open);
			var lineWithoutProductAttribute = CreateDummyStocktakeLine(stocktake, data.Part2.PK, true, StocktakeLineStatus.Codes.Open);
			var closedLine = CreateDummyStocktakeLine(stocktake, data.Part1.PK, true, StocktakeLineStatus.Codes.Closed);
			var autoLoadedOpenLine = CreateDummyStocktakeLine(stocktake, data.Part1.PK, false, StocktakeLineStatus.Codes.Open);

			AssertEquals($"Line does not have a product, therefore {attributeNumber} must be readonly.", true, attributeReadonlyExposed(lineWithEmptyWU_OP));
			AssertEquals($"Line has an invalid product, therefore {attributeNumber} must be readonly.", true, attributeReadonlyExposed(lineWithInvalidProduct));
			AssertEquals($"Line has an attribute specified, therefore {attributeNumber} must be editable.", false, attributeReadonlyExposed(lineWithProductAttribute));
			AssertEquals($"Line does not have an attribute, therefore {attributeNumber} must be readonly.", true, attributeReadonlyExposed(lineWithoutProductAttribute));
			AssertEquals($"Line is closed, therefore {attributeNumber} must be readonly.", true, attributeReadonlyExposed(closedLine));
			AssertEquals($"Line is automatically loaded, therefore {attributeNumber} must be readonly.", true, attributeReadonlyExposed(autoLoadedOpenLine));
		}

		#endregion

		#endregion

		#endregion

		#region Infos

		#region TestWU_DateClosedInfo

		public void TestWU_DateClosedInfo()
		{
			AssertEquals(true, Factory.New<WhsStocktakeLine>().WU_DateClosedInfo.ReadOnly);
		}

		#endregion

		#region TestWU_F3_NKPackTypeInfo

		public void TestWU_F3_NKPackTypeInfo()
		{
			AssertEquals(true, Factory.New<WhsStocktakeLine>().WU_F3_NKPackTypeInfo.ReadOnly);
		}

		#endregion

		#region TestWU_LineNoInfo

		public void TestWU_LineNoInfo()
		{
			AssertEquals(true, Factory.New<WhsStocktakeLine>().WU_LineNoInfo.ReadOnly);
		}

		#endregion

		#region TestWU_StatusInfo

		public void TestWU_StatusInfo()
		{
			AssertEquals(true, Factory.New<WhsStocktakeLine>().WU_StatusInfo.ReadOnly);
		}

		#endregion

		#region TestWU_SystemUnitsInfo

		public void TestWU_SystemUnitsInfo()
		{
			AssertEquals(true, Factory.New<WhsStocktakeLine>().WU_SystemUnitsInfo.ReadOnly);
		}

		#endregion

		#endregion

		#region TestIsClosing

		public void TestIsClosing()
		{
			var stocktake = Factory.New<WhsStocktake>();
			var line = Factory.New<DummyStocktakeLine>();
			line.WU_WS = stocktake.PK;

			AssertEquals(false, line.IsClosing);

			using (new SemaphoreManager(line.Stocktake.LineClosingSemaphore))
			{
				AssertEquals(true, line.IsClosing);
			}
			AssertEquals(false, line.IsClosing);
		}

		#endregion

		#region TestGetNewValidation

		public void TestGetNewValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var autoLoadedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, false);
			var manuallyLoadedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, true);
			var emptyLine = Helper.CreateEmptyWhsStocktakeLine(stocktake, data.Whs1.DefaultLocation);

			// Test before line closing

			AssertEquals(typeof(WhsStocktakeLineValidation), autoLoadedLine.Validation.GetType());
			AssertEquals(typeof(WhsStocktakeLineValidationForManuallyAddedLines), manuallyLoadedLine.Validation.GetType());
			AssertEquals(typeof(EmptyWhsStocktakeLineValidation), emptyLine.Validation.GetType());

			// test when lines are closing

			using (new SemaphoreManager(stocktake.LineClosingSemaphore))
			{
				AssertEquals(typeof(WhsStocktakeLineValidation), autoLoadedLine.Validation.GetType());
				AssertEquals(typeof(WhsStocktakeLineValidationForManuallyAddedLines), manuallyLoadedLine.Validation.GetType());
			}

			// US

			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			AssertEquals(typeof(WhsStocktakeLineValidationForManuallyAddedLinesUS), manuallyLoadedLine.Validation.GetType());

			using (new SemaphoreManager(stocktake.LineClosingSemaphore))
			{
				AssertEquals(typeof(WhsStocktakeLineValidationForManuallyAddedLinesUS), manuallyLoadedLine.Validation.GetType());
			}
		}

		#endregion

		#region TestCloseLine

		public void TestCloseLine()
		{
			// Setup test data

			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.One, true);
			Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.Three, true);
			Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.PackingDate, true);
			Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.ExpiryDate, true);

			Helper.SetProductAttributeUse(stocktake.Client, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(stocktake.Client, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(stocktake.Client, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(stocktake.Client, data.Part1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(stocktake.Client, data.Part1, AttributeNumber.ExpiryDate, true);

			var line = stocktake.Lines.AddNew();
			line.WU_Status = StocktakeLineStatus.Codes.Open;
			AssertEquals("Precondition", false, line.HasErrors);

			using (new SemaphoreManager(line.Stocktake.LineClosingSemaphore))
			{
				line.CloseLine();
				AssertEquals("Since there are no product code or location for this line. There are validation errors.", true, line.HasErrors);
				AssertEquals(StocktakeLineStatus.Codes.Open, line.WU_Status);

				line.WU_OP = data.Part1.PK;
				line.LocationString = "A";
				line.CloseLine();
				AssertEquals("Since product attributes are not specified there are validation errors.", true, line.HasErrors);
				AssertEquals(StocktakeLineStatus.Codes.Open, line.WU_Status);

				line.WU_PartAttrib1 = "1";
				line.WU_PartAttrib2 = "2";
				line.WU_PartAttrib3 = "3";
				line.WU_ExpiryDate = ZDate.Today;
				line.WU_PackingDate = ZDate.Today;
				AssertEquals("Precondition", false, line.HasErrors);
				line.CloseLine();
				AssertEquals(false, line.HasErrors);
				AssertEquals(StocktakeLineStatus.Codes.Closed, line.WU_Status);
			}
		}

		#endregion

		#region TestIsEmptyLocation

		public void TestIsEmptyLocation()
		{
			var emptyLocation = Factory.NewWithValidTestData<WhsStocktakeLine>();
			AssertEquals(false, emptyLocation.IsEmptyLocation);
			emptyLocation.WU_InventoryStatus = "EMP";
			AssertEquals(true, emptyLocation.IsEmptyLocation);
		}

		#endregion

		#region TestIsEqualLine

		[TestDate(2012, 05, 15)]
		public void TestIsEqualLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(null, data.Whs1);
			var client2 = Helper.CreateClient("C2");
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var lineWithAllAttributes = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1), "PA1", "PA2", "PA3", StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Available);
			var lineWithNoAttributes = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "", "", "", StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var similarLineWithAllAttributes = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1), "PA1", "PA2", "PA3", StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Available);
			var similarLineWithAllAttributesDifferentClient = Helper.CreateWhsStocktakeLine(stocktake, client2, data.Part1, data.Whs1.DefaultLocation, ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1), "PA1", "PA2", "PA3", StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Available);
			var similarLineWithNoAttributes = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "", "", "", StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var differentLineWithAllAttributes = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, ZDate.Today.AddDays(2), ZDate.Today.AddDays(-2), "A1", "A2", "A3", StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Damaged);
			var differentLineWithNoAttributes = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "", "", "", StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Damaged);

			AssertEquals(true, lineWithAllAttributes.IsEqualLine(similarLineWithAllAttributes));
			AssertEquals(false, lineWithAllAttributes.IsEqualLine(similarLineWithAllAttributesDifferentClient));
			AssertEquals(false, lineWithAllAttributes.IsEqualLine(differentLineWithAllAttributes));

			AssertEquals(true, lineWithNoAttributes.IsEqualLine(similarLineWithNoAttributes));
			AssertEquals(false, lineWithNoAttributes.IsEqualLine(differentLineWithNoAttributes));
		}

		#endregion

		#region TestIsMatchingStocktakeLine

		[TestDate(2013, 06, 10)]
		public void TestIsMatchingStocktakeLine()
		{
			var line1 = Factory.New<WhsStocktakeLine>();
			var line2 = Factory.New<WhsStocktakeLine>();
			AssertEquals(true, line1.IsMatchingStocktakeLine(line2, "", "", 0m));

			line1.WU_ExpiryDate = ZDate.Today;
			AssertEquals(false, line1.IsMatchingStocktakeLine(line2, "", "", 0m));

			line2.WU_ExpiryDate = ZDate.Today;
			AssertEquals(true, line1.IsMatchingStocktakeLine(line2, "", "", 0m));

			line1.WU_PackingDate = ZDate.Today;
			AssertEquals(false, line1.IsMatchingStocktakeLine(line2, "", "", 0m));

			line2.WU_PackingDate = ZDate.Today;
			AssertEquals(true, line1.IsMatchingStocktakeLine(line2, "", "", 0m));

			line1.WU_PartAttrib1 = "A1";
			AssertEquals(false, line1.IsMatchingStocktakeLine(line2, "", "", 0m));

			line2.WU_PartAttrib1 = "A1";
			AssertEquals(true, line1.IsMatchingStocktakeLine(line2, "", "", 0m));

			line1.WU_PartAttrib2 = "A2";
			AssertEquals(false, line1.IsMatchingStocktakeLine(line2, "", "", 0m));

			line2.WU_PartAttrib2 = "A2";
			AssertEquals(true, line1.IsMatchingStocktakeLine(line2, "", "", 0m));

			line1.WU_PartAttrib3 = "A3";
			AssertEquals(false, line1.IsMatchingStocktakeLine(line2, "", "", 0m));

			line2.WU_PartAttrib3 = "A3";
			AssertEquals(true, line1.IsMatchingStocktakeLine(line2, "", "", 0m));

			line1.WU_SerialNumber = "SN";
			AssertEquals(false, line1.IsMatchingStocktakeLine(line2, "", "", 0m));

			line2.WU_SerialNumber = "SN";
			AssertEquals(true, line1.IsMatchingStocktakeLine(line2, "", "", 0m));

			line1.WU_PalletID = "PLT1";
			AssertEquals(false, line1.IsMatchingStocktakeLine(line2, "", "", 0m));
			AssertEquals(true, line1.IsMatchingStocktakeLine(line2, "PLT1", "", 0m));

			line1.WU_PackageGroupId = "ABC";
			AssertEquals(false, line1.IsMatchingStocktakeLine(line2, "PLT1", "", 0m));
			AssertEquals(true, line1.IsMatchingStocktakeLine(line2, "PLT1", "ABC", 0m));

			line1.WU_PerPackageQty = 2m;
			AssertEquals(false, line1.IsMatchingStocktakeLine(line2, "PLT1", "ABC", 0m));
			AssertEquals(true, line1.IsMatchingStocktakeLine(line2, "PLT1", "ABC", 2m));
		}

		#endregion

		#region TestCountShouldBe0ForEmptyLines

		public void TestCountShouldBe0ForEmptyLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, "", true);

			AssertNoExceptionThrown("Precondition", () => Factory.Save());

			stocktakeLine.WU_OP = ZGuid.Empty;
			AssertCannotSaveInvalidLine(stocktakeLine, WhsStocktakeLineSchema.WU_LastCount);
			AssertCannotSaveInvalidLine(stocktakeLine, WhsStocktakeLineSchema.WU_Count2);
			AssertCannotSaveInvalidLine(stocktakeLine, WhsStocktakeLineSchema.WU_Count3);

			stocktakeLine.WU_OP = data.Part1.PK;
			stocktakeLine.WU_OH_Client = ZGuid.Empty;
			AssertCannotSaveInvalidLine(stocktakeLine, WhsStocktakeLineSchema.WU_LastCount);
			AssertCannotSaveInvalidLine(stocktakeLine, WhsStocktakeLineSchema.WU_Count2);
			AssertCannotSaveInvalidLine(stocktakeLine, WhsStocktakeLineSchema.WU_Count3);
		}

		void AssertCannotSaveInvalidLine(WhsStocktakeLine stocktakeLine, SchemaDecimalColumn column)
		{
			stocktakeLine.WU_LastCount = 0;
			stocktakeLine.WU_Count2 = 0;
			stocktakeLine.WU_Count3 = 0;
			stocktakeLine[column] = 1;
			AssertExceptionThrown("CK_Client_Product_Count should not allow to save", typeof(ZSaveException), () => Factory.Save());
		}

		#endregion

		// interfaces

		#region IPartAttributeValidationConsumer Members

		#region TestIsRegisteredForUniqueSerialNumberChecking

		public void TestIsRegisteredForUniqueSerialNumberChecking()
		{
			var line = Factory.New<WhsStocktakeLine>();
			AssertEquals(true, line.IsRegisteredForUniqueSerialNumberChecking);
		}

		#endregion

		#region TestIsValidForUniqueSerialNumberChecking

		public void TestIsValidForUniqueSerialNumberChecking()
		{
			TestIsValidForUniqueSerialNumberCheckingCore(setClientOnStocktake: true);
		}

		public void TestIsValidForUniqueSerialNumberChecking_StocktakeWithoutClient()
		{
			TestIsValidForUniqueSerialNumberCheckingCore(setClientOnStocktake: false);
		}

		void TestIsValidForUniqueSerialNumberCheckingCore(bool setClientOnStocktake)
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var stocktake = Helper.CreateWhsStocktake(setClientOnStocktake ? data.Org1 : null, data.Whs1);
			var lineWithSerialNumber = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var lineWithoutSerialNumber = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);

			AssertEquals("Serial number is empty therefore must return false.", false, lineWithSerialNumber.IsValidForUniqueSerialNumberChecking(lineWithSerialNumber.WU_SerialNumber));
			AssertEquals("Serial number is not used for this product therefore must return false.", false, lineWithoutSerialNumber.IsValidForUniqueSerialNumberChecking(lineWithoutSerialNumber.WU_SerialNumber));

			lineWithSerialNumber.WU_SerialNumber = "ABC";
			lineWithoutSerialNumber.WU_SerialNumber = "ABC";

			AssertEquals("Serial number is not empty and this product does use serial number, therefore must be valid.", true, lineWithSerialNumber.IsValidForUniqueSerialNumberChecking(lineWithSerialNumber.WU_SerialNumber));
			AssertEquals("Although serial number is entered, this product does not use serial number, therefore must return false.", false, lineWithoutSerialNumber.IsValidForUniqueSerialNumberChecking(lineWithoutSerialNumber.WU_SerialNumber));

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			AssertEquals("Serial number attribute is release captured therefore must return false.", false, lineWithSerialNumber.IsValidForUniqueSerialNumberChecking(lineWithSerialNumber.WU_SerialNumber));
		}

		#endregion

		#region TestIsSerialNumberUsedOnThis

		#region TestIsSerialNumberUsedOnThis

		public void TestIsSerialNumberUsedOnThis()
		{
			TestIsSerialNumberUsedOnThisCore(setClientOnStocktake: true);
		}

		public void TestIsSerialNumberUsedOnThis_StocktakeWithoutClient()
		{
			TestIsSerialNumberUsedOnThisCore(setClientOnStocktake: false);
		}

		void TestIsSerialNumberUsedOnThisCore(bool setClientOnStocktake)
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var stocktake = Helper.CreateWhsStocktake(setClientOnStocktake ? data.Org1 : null, data.Whs1);
			var lineWithSerialNumber = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			lineWithSerialNumber.WU_SerialNumber = "SER";

			AssertEquals("Serial Number matches, therefore true", true, lineWithSerialNumber.IsSerialNumberUsedOnThis("SER"));
			AssertEquals("Serial Number does not match, therefore false", false, lineWithSerialNumber.IsSerialNumberUsedOnThis("BGT"));
		}

		#endregion

		#region TestIsSerialNumberUsedOnThis_NonSerialNumberProduct

		public void TestIsSerialNumberUsedOnThis_NonSerialNumberProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			line.WU_PartAttrib1 = "P1";
			line.WU_PartAttrib2 = "P2";
			line.WU_PartAttrib3 = "P3";

			AssertEquals(false, line.IsSerialNumberUsedOnThis("P1"));
			AssertEquals(false, line.IsSerialNumberUsedOnThis("P2"));
			AssertEquals(false, line.IsSerialNumberUsedOnThis("P3"));
			AssertEquals(false, line.IsSerialNumberUsedOnThis("P"));
		}

		#endregion

		#endregion

		#region TestIsSerialNumberUsedOnSiblings

		#region TestIsSerialNumberUsedOnSiblings

		public void TestIsSerialNumberUsedOnSiblings()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true);

			WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CLI"); // Serial number is not unique by product

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var lineWithProduct1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var lineWithDuplicateSerialNumberAndProduct1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var lineWithDuplicateSerialNumberAndProduct2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var lineWithUniqueSerialNumberAndProduct2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			lineWithProduct1.WU_SerialNumber = "P1";
			lineWithDuplicateSerialNumberAndProduct1.WU_SerialNumber = "P1";
			lineWithDuplicateSerialNumberAndProduct2.WU_SerialNumber = "P1";
			lineWithUniqueSerialNumberAndProduct2.WU_SerialNumber = "P";

			AssertEquals(true, lineWithProduct1.IsSerialNumberUsedOnSiblings("P1"));
			AssertEquals(true, lineWithDuplicateSerialNumberAndProduct1.IsSerialNumberUsedOnSiblings("P1"));
			AssertEquals(true, lineWithDuplicateSerialNumberAndProduct2.IsSerialNumberUsedOnSiblings("P1"));
			AssertEquals(false, lineWithUniqueSerialNumberAndProduct2.IsSerialNumberUsedOnSiblings("P"));

			WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PRO"); // Serial number is unique by product

			AssertEquals(true, lineWithProduct1.IsSerialNumberUsedOnSiblings("P1"));
			AssertEquals(true, lineWithDuplicateSerialNumberAndProduct1.IsSerialNumberUsedOnSiblings("P1"));
			AssertEquals(false, lineWithDuplicateSerialNumberAndProduct2.IsSerialNumberUsedOnSiblings("P1"));
			AssertEquals(false, lineWithUniqueSerialNumberAndProduct2.IsSerialNumberUsedOnSiblings("P"));
		}

		#endregion

		#region TestIsSerialNumberUsedOnSiblings_StocktakeWithoutClient

		public void TestIsSerialNumberUsedOnSiblings_StocktakeWithoutClient()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true);

			WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CLI"); // Serial number is not unique by product

			var stocktake = Helper.CreateWhsStocktake(null, data.Whs1);
			var lineWithProduct1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var lineWithDuplicateSerialNumberAndProduct1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var lineWithDuplicateSerialNumberAndProduct2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var lineWithUniqueSerialNumberAndProduct2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			lineWithProduct1.WU_SerialNumber = "AB";
			lineWithDuplicateSerialNumberAndProduct1.WU_SerialNumber = "AB";
			lineWithDuplicateSerialNumberAndProduct2.WU_SerialNumber = "AB";
			lineWithUniqueSerialNumberAndProduct2.WU_SerialNumber = "A";

			AssertEquals(true, lineWithProduct1.IsSerialNumberUsedOnSiblings("AB"));
			AssertEquals(true, lineWithDuplicateSerialNumberAndProduct1.IsSerialNumberUsedOnSiblings("AB"));
			AssertEquals(true, lineWithDuplicateSerialNumberAndProduct2.IsSerialNumberUsedOnSiblings("AB"));
			AssertEquals(false, lineWithUniqueSerialNumberAndProduct2.IsSerialNumberUsedOnSiblings("A"));

			WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PRO"); // Serial number is unique by product

			AssertEquals(true, lineWithProduct1.IsSerialNumberUsedOnSiblings("AB"));
			AssertEquals(true, lineWithDuplicateSerialNumberAndProduct1.IsSerialNumberUsedOnSiblings("AB"));
			AssertEquals(false, lineWithDuplicateSerialNumberAndProduct2.IsSerialNumberUsedOnSiblings("AB"));
			AssertEquals(false, lineWithUniqueSerialNumberAndProduct2.IsSerialNumberUsedOnSiblings("A"));
		}

		#endregion

		#region TestIsSerialNumberUsedOnSiblings_NonSerialNumberProducts

		public void TestIsSerialNumberUsedOnSiblings_NonSerialNumberProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "P1", "P2", "P3", StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var line2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "P1", "P2", "P3", StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);

			AssertEquals(false, line1.IsSerialNumberUsedOnSiblings("P1"));
			AssertEquals(false, line1.IsSerialNumberUsedOnSiblings("P2"));
			AssertEquals(false, line1.IsSerialNumberUsedOnSiblings("P3"));

			AssertEquals(false, line2.IsSerialNumberUsedOnSiblings("P1"));
			AssertEquals(false, line2.IsSerialNumberUsedOnSiblings("P2"));
			AssertEquals(false, line2.IsSerialNumberUsedOnSiblings("P3"));
		}

		#endregion

		#endregion

		#region TestIsInventoryAdjustedOutOnSiblings

		public void TestIsInventoryAdjustedOutOnSiblings()
		{
			var line = Factory.New<WhsStocktakeLine>();
			AssertEquals(false, line.IsInventoryAdjustedOutOnSiblings(null));
		}

		#endregion

		#endregion

		#region ILineAttribute Members

		#region TestILineAttributeMembers

		public void TestILineAttributeMembers()
		{
			var expiryDate = ZDate.Today.AddDays(1);
			var packingDate = ZDate.Today.AddDays(-1);

			var line = Factory.New<WhsStocktakeLine>();
			line.WU_BondedEntryKey = "BEK";
			line.WU_ExpiryDate = expiryDate;
			line.WU_PackingDate = packingDate;
			line.WU_PartAttrib1 = "PA1";
			line.WU_PartAttrib2 = "PA2";
			line.WU_PartAttrib3 = "PA3";

			AssertEquals("BEK", line.BondedEntryKey);
			AssertEquals(expiryDate, line.ExpiryDate);
			AssertEquals(packingDate, line.PackingDate);
			AssertEquals("PA1", line.PartAttrib1);
			AssertEquals("PA2", line.PartAttrib2);
			AssertEquals("PA3", line.PartAttrib3);
			AssertEquals(string.Empty, ((ILineAttributes)line).AllocationKey);
		}

		#endregion

		#region TestSetAttributes

		[TestDate(2012, 05, 15)]
		public void TestSetAttributes()
		{
			var line = Factory.New<WhsStocktakeLine>();
			var line1 = Factory.New<WhsStocktakeLine>();
			line.WU_BondedEntryKey = "BEK";
			line.WU_ExpiryDate = ZDate.Today.AddDays(1);
			line.WU_PackingDate = ZDate.Today.AddDays(-1);
			line.WU_PartAttrib1 = "PA1";
			line.WU_PartAttrib2 = "PA2";
			line.WU_PartAttrib3 = "PA3";

			line1.SetAttributes(line);

			AssertEquals("BEK", line1.WU_BondedEntryKey);
			AssertEquals(ZDateTime.Now.AddDays(1), line1.WU_ExpiryDate);
			AssertEquals(ZDateTime.Now.AddDays(-1), line1.WU_PackingDate);
			AssertEquals("PA1", line1.WU_PartAttrib1);
			AssertEquals("PA2", line1.WU_PartAttrib2);
			AssertEquals("PA3", line1.WU_PartAttrib3);
		}

		#endregion

		#endregion

		#region ILineAssigner Members

		#region TestILineAssigner_AssignLine

		public void TestILineAssigner_AssignLine()
		{
			var user = Helper.CreateGlbStaff("T1", "T1");
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);

			var lineUseCount1 = stocktake.Lines.AddNew();
			var lineUseCount2 = stocktake.Lines.AddNew();
			var lineUseCount3 = stocktake.Lines.AddNew();
			lineUseCount1.WU_TotalCounts = 1;
			lineUseCount2.WU_TotalCounts = 2;
			lineUseCount3.WU_TotalCounts = 3;

			AssertEquals("", lineUseCount1.WU_GS_NKVerifiedBy);
			AssertEquals("", lineUseCount2.WU_Count2VerifiedBy);
			AssertEquals("", lineUseCount3.WU_Count3VerifiedBy);

			((ILineStaffAssigner)lineUseCount1).AssignLine(user);
			((ILineStaffAssigner)lineUseCount2).AssignLine(user);
			((ILineStaffAssigner)lineUseCount3).AssignLine(user);

			AssertEquals("T1", lineUseCount1.WU_GS_NKVerifiedBy);
			AssertEquals("T1", lineUseCount2.WU_Count2VerifiedBy);
			AssertEquals("T1", lineUseCount3.WU_Count3VerifiedBy);
		}

		#endregion

		#region TestILineAssigner_CanAssignLine

		public void TestILineAssigner_CanAssignLine()
		{
			var user = Helper.CreateGlbStaff("T1", "T1");
			var data = new TestDataSimpleEnvironment(Factory);

			// Test with Loaded stocktake

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var lineUseCount1 = stocktake.Lines.AddNew();
			var lineUseCount2 = stocktake.Lines.AddNew();
			var lineUseCount3 = stocktake.Lines.AddNew();
			lineUseCount1.WU_TotalCounts = 1;
			lineUseCount2.WU_TotalCounts = 2;
			lineUseCount3.WU_TotalCounts = 3;
			lineUseCount1.WU_OP = ZGuid.NewZGuid();
			lineUseCount2.WU_OP = ZGuid.NewZGuid();
			lineUseCount3.WU_OP = ZGuid.NewZGuid();
			lineUseCount1.WU_Status = lineUseCount2.WU_Status = lineUseCount3.WU_Status = StocktakeLineStatus.Codes.Open;

			AssertEquals(true, ((ILineStaffAssigner)lineUseCount1).CanAssignOrUnAssignLine());
			AssertEquals(true, ((ILineStaffAssigner)lineUseCount2).CanAssignOrUnAssignLine());
			AssertEquals(true, ((ILineStaffAssigner)lineUseCount3).CanAssignOrUnAssignLine());

			lineUseCount1.WU_Status = lineUseCount2.WU_Status = lineUseCount3.WU_Status = StocktakeLineStatus.Codes.Closed;
			AssertEquals(false, ((ILineStaffAssigner)lineUseCount1).CanAssignOrUnAssignLine());
			AssertEquals(false, ((ILineStaffAssigner)lineUseCount2).CanAssignOrUnAssignLine());
			AssertEquals(false, ((ILineStaffAssigner)lineUseCount3).CanAssignOrUnAssignLine());

			lineUseCount1.WU_Status = lineUseCount2.WU_Status = lineUseCount3.WU_Status = StocktakeLineStatus.Codes.Open;
			lineUseCount1.WU_DateVerified = lineUseCount2.WU_Count2DateVerified = lineUseCount3.WU_Count3DateVerified = ZDateTime.Now;
			AssertEquals(false, ((ILineStaffAssigner)lineUseCount1).CanAssignOrUnAssignLine());
			AssertEquals(false, ((ILineStaffAssigner)lineUseCount2).CanAssignOrUnAssignLine());
			AssertEquals(false, ((ILineStaffAssigner)lineUseCount3).CanAssignOrUnAssignLine());

			// Test with Finalized stocktake

			stocktake.WS_StocktakeStatus = StocktakeStatus.Codes.Finalised;
			lineUseCount1.WU_Status = lineUseCount2.WU_Status = lineUseCount3.WU_Status = StocktakeLineStatus.Codes.Closed;
			lineUseCount1.WU_DateVerified = lineUseCount2.WU_Count2DateVerified = lineUseCount3.WU_Count3DateVerified = ZDateTime.Empty;
			AssertEquals(false, ((ILineStaffAssigner)lineUseCount1).CanAssignOrUnAssignLine());
			AssertEquals(false, ((ILineStaffAssigner)lineUseCount2).CanAssignOrUnAssignLine());
			AssertEquals(false, ((ILineStaffAssigner)lineUseCount3).CanAssignOrUnAssignLine());

			// Test with Empty Location stocktake

			var emptyLocation = Helper.CreateEmptyWhsStocktakeLine(stocktake, data.Whs1.DefaultLocation);
			AssertEquals(false, ((ILineStaffAssigner)emptyLocation).CanAssignOrUnAssignLine());
		}

		#endregion

		#region TestILineAssigner_UnAssignLine

		public void TestILineAssigner_UnAssignLine()
		{
			var user = Helper.CreateGlbStaff("T1", "T1");
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);

			var stocktakeLine = stocktake.Lines.AddNew();
			AssertExceptionThrown(typeof(NotImplementedException), () => ((ILineStaffAssigner)stocktakeLine).UnAssignLine(user));
		}

		#endregion

		#endregion

		#region TestILocationConsumer

		public void TestILocationConsumer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Factory.Save();

			var stocktake = Factory.New<WhsStocktake>();
			var stocktakeLine = stocktake.Lines.AddNew();
			var locationPK = data.Whs1.DefaultLocation.PK;
			stocktakeLine.WU_WL = locationPK;
			var locationConsumer = (ILocationConsumer)stocktakeLine;
			AssertEquals(locationConsumer.LocationTypeForMessages, "Stocktake");
			AssertEquals(locationConsumer.LocationPK, locationPK);

			locationConsumer.LocationTitle = "Test";
			AssertEquals(locationConsumer.LocationTitle, "Test");
		}

		#endregion

		#region ILineAttributes Members

		public void TestILineAttributesExpiryDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = stocktake.Lines.AddNew();

			var date = ZDate.Today.AddDays(10);
			stocktakeLine.WU_ExpiryDate = date;
			AssertEquals(date, ((ILineAttributes)stocktakeLine).ExpiryDate);
		}

		public void TestILineAttributesPackingDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = stocktake.Lines.AddNew();

			var date = ZDate.Today.AddDays(-10);
			stocktakeLine.WU_PackingDate = date;
			AssertEquals(date, ((ILineAttributes)stocktakeLine).PackingDate);
		}

		public void TestILineAttributesBondedEntryKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = stocktake.Lines.AddNew();

			stocktakeLine.WU_BondedEntryKey = "BEK-1";
			AssertEquals("BEK-1", ((ILineAttributes)stocktakeLine).BondedEntryKey);
		}

		public void TestILineAttributesPartAttrib1()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = stocktake.Lines.AddNew();

			stocktakeLine.WU_PartAttrib1 = "PA1";
			AssertEquals("PA1", ((ILineAttributes)stocktakeLine).PartAttrib1);
		}

		public void TestILineAttributesPartAttrib2()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = stocktake.Lines.AddNew();

			stocktakeLine.WU_PartAttrib2 = "PA2";
			AssertEquals("PA2", ((ILineAttributes)stocktakeLine).PartAttrib2);
		}

		public void TestILineAttributesPartAttrib3()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = stocktake.Lines.AddNew();

			stocktakeLine.WU_PartAttrib3 = "PA3";
			AssertEquals("PA3", ((ILineAttributes)stocktakeLine).PartAttrib3);
		}

		public void TestILineAttributesSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = stocktake.Lines.AddNew();

			stocktakeLine.WU_SerialNumber = "SNN";
			AssertEquals("SNN", ((ILineAttributes)stocktakeLine).SerialNumber);
		}

		public void TestILineAttributesSetAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = stocktake.Lines.AddNew();

			stocktakeLine.WU_ExpiryDate = ZDate.Today.AddDays(1);
			stocktakeLine.WU_PackingDate = ZDate.Today.AddDays(-1);
			stocktakeLine.WU_BondedEntryKey = "BEK-1";
			stocktakeLine.WU_PartAttrib1 = "PA1";
			stocktakeLine.WU_PartAttrib2 = "PA2";
			stocktakeLine.WU_PartAttrib3 = "PA3";
			stocktakeLine.WU_SerialNumber = "SNN";

			var line2 = (WhsStocktakeLine)GetNewBusinessObject();
			line2.SetAttributes(stocktakeLine);
			AssertEquals(true, AttributeComparer.Compare(line2, stocktakeLine));
		}

		#endregion

		//

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return Factory.New<WhsStocktakeLine>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var stocktakeLine = CreateDummyStocktakeLine(stocktake, data.Part1.PK, false, StocktakeLineStatus.Codes.Open);
			stocktakeLine.WU_OH_Client = data.Org1.PK;
			stocktakeLine.WU_WL = data.Whs1.DefaultLocation.PK;

			return stocktakeLine;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var stocktake = Factory.New<WhsStocktake>();
			return stocktake.Lines.AddNew();
		}

		void SetupEnvironnemntForCurrentCount()
		{
			// create environment
			var whs = Helper.CreateWarehouse("AAAA");
			var client = Helper.CreateClient();
			var product = Helper.CreateProduct(client, "P1");
			// create stocktake line
			Stocktake = Helper.CreateWhsStocktake(client, whs);
			Line1 = Helper.CreateWhsStocktakeLine(Stocktake, client, product);
		}

		DummyStocktakeLine CreateDummyStocktakeLine(WhsStocktake stocktake, ZBool manuallyLoaded, ZString status)
		{
			return CreateDummyStocktakeLine(stocktake, ZGuid.NewZGuid(), manuallyLoaded, status);
		}

		DummyStocktakeLine CreateDummyStocktakeLine(WhsStocktake stocktake, ZGuid productId, ZBool manuallyLoaded, ZString status)
		{
			var line = Factory.New<DummyStocktakeLine>();
			line.WU_WS = stocktake.PK;
			line.WU_OP = productId;
			line.WU_IsManuallyAdded = manuallyLoaded;
			line.WU_Status = status;
			return line;
		}

		WhsStocktake Stocktake;
		WhsStocktakeLine Line1;

		#endregion

		#region DummyStocktakeLine

		class DummyStocktakeLine : WhsStocktakeLine
		{
			public DummyStocktakeLine(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool NonStandardOrEmptyReadOnlyExposed
			{
				get { return NonStandardOrEmptyReadOnly; }
			}

			public bool NonStandardReadOnlyExposed
			{
				get { return NonStandardReadOnly; }
			}

			public bool AutoLoadedReadOnlyExposed
			{
				get { return AutoLoadedReadOnly; }
			}

			public bool ClientReadOnlyExposed
			{
				get { return ClientReadOnly; }
			}

			public bool PartAttribute1ReadOnlyExposed
			{
				get { return PartAttribute1ReadOnly; }
			}

			public bool PartAttribute2ReadOnlyExposed
			{
				get { return PartAttribute2ReadOnly; }
			}

			public bool PartAttribute3ReadOnlyExposed
			{
				get { return PartAttribute3ReadOnly; }
			}

			public bool ExpiryDateReadOnlyExposed
			{
				get { return ExpiryDateReadOnly; }
			}

			public bool PackingDateReadOnlyExposed
			{
				get { return PackingDateReadOnly; }
			}
		}

		#endregion
	}
}
