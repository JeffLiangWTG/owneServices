using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsAdjustmentLineDataObjectReaderTest : WhsAdjustmentAndPickableDocketLineDataObjectReaderTest<WhsAdjustment, WhsAdjustmentLine, WhsAdjustmentLineDataObjectReader>
	{
		#region TestBasicAdjustmentLineLevelFieldMappings

		public void TestBasicAdjustmentLineLevelFieldMappings_WithSerialNumber()
		{
			var adjustment = GetNewDocketLineParent(Factory);
			var row = adjustment.Warehouse.Rows.AddNew();
			row.WR_Name = "A";
			row.WR_Columns = 10;
			row.WR_Levels = 8;
			row.WR_Trays = 5;
			EnableAllAttributeUse(adjustment.Client, "BOWLHAT");
			Helper.SetClientAttributeType(adjustment.Client, AttributeNumber.Serial, true);

			Factory.SaveForTesting();

			var adjustmentLineDataObject = GetNewAdjustmentLine();
			adjustmentLineDataObject.SerialNumber = "SNNN";
			var reader = new WhsAdjustmentLineDataObjectReader(adjustmentLineDataObject, Logger, Factory, adjustment);
			var adjustmentLine = reader.ReadIntoBusinessObject();
			AssertNotNull("whsAdjustmentBO", adjustmentLine);
			CombineAssertions(() =>
			{
				AssertContents(adjustmentLine, useSerial: true);
			});
		}

		#endregion

		#region TestLocationPopulationIfLocationNotProvided

		public void TestLocationPopulationIfLocationNotProvided()
		{
			var adjustment = GetNewDocketLineParent(Factory);
			Helper.CreateRowAndGenerateLocations(adjustment.Warehouse, "A", 1, 1);
			Factory.SaveForTesting();

			var adjustmentLineDataObject = GetNewAdjustmentLine();
			adjustmentLineDataObject.Location = null; // import file should have no location

			var reader = GetNewReader(adjustmentLineDataObject, Logger, adjustment);
			var adjustmentLine = reader.ReadIntoBusinessObject();

			if (UseDefaultLocationIfNoneProvided)
			{
				AssertEquals(adjustment.Warehouse.DefaultLocation.PK, adjustmentLine.Location.PK);
			}
			else
			{
				AssertNull(adjustmentLine.Location);
			}
		}

		protected virtual bool UseDefaultLocationIfNoneProvided => false;

		#endregion

		#region TestLocationPopulation_OutOfRange

		public void TestLocationPopulation_OutOfRange()
		{
			var adjustment = GetNewDocketLineParent(Factory);
			Helper.CreateRowAndGenerateLocations(adjustment.Warehouse, "A", 1, 1);
			Factory.SaveForTesting();

			var adjustmentLineDataObject = GetNewAdjustmentLine();
			adjustmentLineDataObject.Location = new Location { Row = "A", Column = 0, Level = 1, Tray = 1 };

			var reader = GetNewReader(adjustmentLineDataObject, Logger, adjustment);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot Import Adjustment Line 2:\r\nInvalid Location: Row = A, Column = 0, Level = 1, Tray = 1.", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestNullCustomsDataDoesNotThrowException

		public void TestNullCustomsDataDoesNotThrowException()
		{
			var adjustment = GetNewDocketLineParent(Factory);
			Helper.CreateRowAndGenerateLocations(adjustment.Warehouse, "A", 4, 4, 4);
			var dataObject = GetNewAdjustmentLine();
			dataObject.CustomsData = null;

			var reader = new WhsAdjustmentLineDataObjectReader(dataObject, Logger, Factory, adjustment);
			AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestNullLocationRow_HandledGracefully

		public void TestNullLocationRow_HandledGracefully()
		{
			var adjustment = GetNewDocketLineParent(Factory);
			var row = adjustment.Warehouse.Rows.AddNew();
			row.WR_Name = "A";
			row.WR_Columns = 10;
			row.WR_Levels = 8;
			row.WR_Trays = 5;
			Factory.SaveForTesting();

			var adjustmentLineDataObject = GetNewAdjustmentLine();
			adjustmentLineDataObject.Location.Row = null;

			var reader = new WhsAdjustmentLineDataObjectReader(adjustmentLineDataObject, Logger, Factory, adjustment);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot Import Adjustment Line 2:\r\nInvalid Location: Row = , Column = 2, Level = 4, Tray = 3.", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPopulateHoldCodes

		public void TestPopulateHoldCodes()
		{
			var adjustment = GetNewDocketLineParent(Factory);
			var customHoldCode = Helper.CreateInventoryHeldCode("CUSTOM", "Custom Hold code");
			var lineDataObjectWithHoldCodeExist = new OrderLine();
			var lineDataObjectWithCustomHoldCode = new OrderLine();
			var lineDataObjectWithHoldCodeNotExist = new OrderLine();
			var lineDataObjectWithNoHoldCode = new OrderLine();
			lineDataObjectWithHoldCodeExist.OriginalHoldCode = new CodeDescriptionPair9Char() { Code = InventoryHoldCodes.Codes.Damaged, Description = InventoryHoldCodes.Descriptions.Damaged };
			lineDataObjectWithHoldCodeExist.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };

			lineDataObjectWithCustomHoldCode.OriginalHoldCode = new CodeDescriptionPair9Char() { Code = customHoldCode.WHC_Code, Description = customHoldCode.WHC_Description };
			lineDataObjectWithCustomHoldCode.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };

			lineDataObjectWithHoldCodeNotExist.OriginalHoldCode = new CodeDescriptionPair9Char() { Code = "WXYZ", Description = "ABCDE" };
			lineDataObjectWithHoldCodeNotExist.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };

			lineDataObjectWithNoHoldCode.OriginalHoldCode = new CodeDescriptionPair9Char() { Code = "", Description = "" };
			lineDataObjectWithNoHoldCode.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };

			var readerForExistingHoldCode = new WhsAdjustmentLineDataObjectReader(lineDataObjectWithHoldCodeExist, Logger, Factory, adjustment);
			var adjustmentLineWithHoldCode = readerForExistingHoldCode.ReadIntoBusinessObject();
			AssertEquals(InventoryHoldCodes.Codes.Damaged, adjustmentLineWithHoldCode.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals(InventoryHoldCodes.Codes.Damaged, adjustmentLineWithHoldCode.WE_WHC_NKCurrentInventoryHeldCode);

			var readerForCustomHoldCode = new WhsAdjustmentLineDataObjectReader(lineDataObjectWithCustomHoldCode, Logger, Factory, adjustment);
			var adjustmentLineWithCustomHoldCode = readerForCustomHoldCode.ReadIntoBusinessObject();
			AssertEquals(customHoldCode.WHC_Code, adjustmentLineWithCustomHoldCode.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals(customHoldCode.WHC_Code, adjustmentLineWithCustomHoldCode.WE_WHC_NKCurrentInventoryHeldCode);

			var readerForHoldCodeNotExist = new WhsAdjustmentLineDataObjectReader(lineDataObjectWithHoldCodeNotExist, Logger, Factory, adjustment);
			var adjustmentLineWithHoldCodeNotExist = readerForHoldCodeNotExist.ReadIntoBusinessObject();
			AssertEquals("WXYZ", adjustmentLineWithHoldCodeNotExist.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals("WXYZ", adjustmentLineWithHoldCodeNotExist.WE_WHC_NKCurrentInventoryHeldCode);

			var readerForNoHoldCode = new WhsAdjustmentLineDataObjectReader(lineDataObjectWithNoHoldCode, Logger, Factory, adjustment);
			var adjustmentLineWithNoHoldCode = readerForNoHoldCode.ReadIntoBusinessObject();
			AssertEquals("", adjustmentLineWithNoHoldCode.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals("", adjustmentLineWithNoHoldCode.WE_WHC_NKCurrentInventoryHeldCode);
		}

		public void TestHoldCodesWhenUniversalXMLCreatesFromOldSchema()
		{
			var adjustment = GetNewDocketLineParent(Factory);
			var heldLine = new OrderLine();
			var damagedLine = new OrderLine();
			var availableLine = new OrderLine();
			heldLine.InventoryStatus = new CodeDescriptionPair() { Code = InventoryStatus.Codes.Held, Description = InventoryStatus.Descriptions.Held };
			heldLine.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			damagedLine.InventoryStatus = new CodeDescriptionPair() { Code = "DAM", Description = "Damaged" };
			damagedLine.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			availableLine.InventoryStatus = new CodeDescriptionPair() { Code = InventoryStatus.Codes.Available, Description = InventoryStatus.Descriptions.Available };
			availableLine.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };

			// Held
			var readerForHeldLine = new WhsAdjustmentLineDataObjectReader(heldLine, Logger, Factory, adjustment);
			var heldAdjustmentLineFromReader = readerForHeldLine.ReadIntoBusinessObject();
			AssertEquals(InventoryHoldCodes.Codes.Held, heldAdjustmentLineFromReader.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals(InventoryHoldCodes.Codes.Held, heldAdjustmentLineFromReader.WE_WHC_NKCurrentInventoryHeldCode);

			// Damaged
			var readerForDamagedLine = new WhsAdjustmentLineDataObjectReader(damagedLine, Logger, Factory, adjustment);
			var damagedAdjustmentLineFromReader = readerForDamagedLine.ReadIntoBusinessObject();
			AssertEquals(InventoryStatus.Codes.Held, damagedAdjustmentLineFromReader.WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Held, damagedAdjustmentLineFromReader.WE_CurrentInventoryStatus);
			AssertEquals(InventoryHoldCodes.Codes.Damaged, damagedAdjustmentLineFromReader.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals(InventoryHoldCodes.Codes.Damaged, damagedAdjustmentLineFromReader.WE_WHC_NKCurrentInventoryHeldCode);

			// Available
			var readerForAvailableLine = new WhsAdjustmentLineDataObjectReader(availableLine, Logger, Factory, adjustment);
			var availableAdjustmentLineFromReader = readerForAvailableLine.ReadIntoBusinessObject();
			AssertEquals("", availableAdjustmentLineFromReader.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals("", availableAdjustmentLineFromReader.WE_WHC_NKCurrentInventoryHeldCode);
		}

		#endregion

		#region TestReadOnlyFields

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestReadOnlyFieldsForUnfinalizedAdjustment()
		{
			TestDateAttribute.UseUNLOCO = true;
			var whs1 = Helper.CreateWarehouse("WHS1");
			Helper.CreateRowAndGenerateLocations(whs1, "A", 2, 4, 3);
			var adjustment = GetNewDocketLineParent(Factory, whs1.PK);
			Factory.SaveForTesting();

			var adjustmentLineBefore = GetNewAdjustmentLine();
			var readerLine = GetNewReader(adjustmentLineBefore, Logger, adjustment, useCleanFactory: false);
			var adjustmentLineBO1 = readerLine.ReadIntoBusinessObject();
			AssertNotNull("Precondition: Adjustment line must be imported into BO.", adjustmentLineBO1);

			Factory.SaveForTesting();

			var adjustmentLineAfter = GetNewAdjustmentLine();
			ChangeAdjustmentLineDataValues(adjustmentLineAfter);

			var readerLineChanged = GetNewReader(adjustmentLineAfter, Logger, adjustment, useCleanFactory: false);
			var adjustmentLineBO2 = readerLineChanged.ReadIntoBusinessObject();

			AssertEquals("Same line must be modified instead of creating a new one.", adjustmentLineBO1.PK, adjustmentLineBO2.PK);
			AssertReadOnlyFields(adjustmentLineBefore, adjustmentLineAfter, adjustmentLineBO2);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestReadOnlyFieldsForFinalizedAdjustment()
		{
			TestDateAttribute.UseUNLOCO = true;
			var whs1 = Helper.CreateWarehouse("WHS1");
			Helper.CreateRowAndGenerateLocations(whs1, "A", 2, 4, 3);
			var adjustment = GetNewDocketLineParent(Factory, whs1.PK);
			Factory.SaveForTesting();

			var adjustmentLineBefore = GetNewAdjustmentLine();
			var readerLine = GetNewReader(adjustmentLineBefore, Logger, adjustment, useCleanFactory: false);
			var adjustmentLineBO1 = readerLine.ReadIntoBusinessObject();

			AssertNotNull("Precondition: Adjustment line must be imported into BO.", adjustmentLineBO1);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			Assert("Adjustment must be finalized", adjustment.IsFinalised);
			Assert("Adjustment line must be finalized", adjustmentLineBO1.IsFinalised);

			var adjustmentLineAfter = GetNewAdjustmentLine();
			ChangeAdjustmentLineDataValues(adjustmentLineAfter);

			var readerLineChanged = GetNewReader(adjustmentLineAfter, Logger, adjustment, useCleanFactory: false);
			var adjustmentLineBO2 = readerLineChanged.ReadIntoBusinessObject();

			AssertEquals("Same line must be modified instead of creating a new one.", adjustmentLineBO1.PK, adjustmentLineBO2.PK);
			AssertReadOnlyFields(adjustmentLineBefore, adjustmentLineAfter, adjustmentLineBO2);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestReadOnlyFieldsForCancelledAdjustment()
		{
			TestDateAttribute.UseUNLOCO = true;
			var whs1 = Helper.CreateWarehouse("WHS1");
			Helper.CreateRowAndGenerateLocations(whs1, "A", 2, 4, 3);
			var adjustment = GetNewDocketLineParent(Factory, whs1.PK);
			Factory.SaveForTesting();

			var adjustmentLineBefore = GetNewAdjustmentLine();
			var readerLine = GetNewReader(adjustmentLineBefore, Logger, adjustment, useCleanFactory: false);
			var adjustmentLineBO1 = readerLine.ReadIntoBusinessObject();
			AssertNotNull("Precondition: Adjustment line must be imported into BO.", adjustmentLineBO1);

			Factory.SaveForTesting();

			adjustment.CancelReactivateDocket();
			Assert("Adjustment must be cancelled", adjustment.IsCancelled);
			Assert("Adjustment Line must be cancelled", adjustmentLineBO1.IsDocketCancelled);

			var adjustmentLineAfter = GetNewAdjustmentLine();
			ChangeAdjustmentLineDataValues(adjustmentLineAfter);

			var readerLineChanged = GetNewReader(adjustmentLineAfter, Logger, adjustment, useCleanFactory: false);
			var adjustmentLineBO2 = readerLineChanged.ReadIntoBusinessObject();

			AssertEquals("Same line must be modified instead of creating a new one.", adjustmentLineBO1.PK, adjustmentLineBO2.PK);
			AssertReadOnlyFields(adjustmentLineBefore, adjustmentLineAfter, adjustmentLineBO2);
		}

		#endregion

		#region TestCustomsSource_ImportWhsBondedWarehouseAttribute

		public void TestCustomsSource_ImportWhsBondedWarehouseAttribute()
		{
			var whs1 = Helper.CreateWarehouse("WHS1");
			var row = Helper.CreateRowAndGenerateLocations(whs1, "A", 4, 4, 4);
			var adjustment = GetNewDocketLineParent(Factory, whs1.PK);
			Factory.SaveForTesting();

			var adjustmentLineDataObject = GetNewAdjustmentLine();
			adjustmentLineDataObject.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo(true);
			var reader = new WhsAdjustmentLineDataObjectReader(adjustmentLineDataObject, Logger, Factory, adjustment);
			var adjustmentLine = reader.ReadIntoBusinessObject();
			AssertNotNull("Precondition: Adjustment line must be imported into BO.", adjustmentLine);

			CombineAssertions(() => { WhsBondedWarehouseAttributeReadingHelperTest.AssertContents(adjustmentLine.CustomsData); });
		}

		#endregion

		#region TestZeroOrderedQty

		public void TestZeroOrderedQty()
		{
			var adjustment = GetNewDocketLineParent(Factory);
			Helper.CreateRowAndGenerateLocations(adjustment.Warehouse, "A", 10, 8, 5);
			Factory.SaveForTesting();

			var adjustmentLineDataObject = GetNewAdjustmentLine();
			adjustmentLineDataObject.OrderedQty = 0m;

			var reader = GetNewReader(adjustmentLineDataObject, Logger, adjustment);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot Import Adjustment Line 2:\r\nInvalid Quantity: Can be negative or positive, but not zero.", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region Implementation

		internal static OrderLine GetNewAdjustmentLine()
		{
			var adjustmentLineDataObject = new OrderLine();

			adjustmentLineDataObject.ArrivalDate = new ZDateTimeOffset(2011, 1, 1);
			adjustmentLineDataObject.Commodity = new Commodity { Code = "HAZ", Description = "HAZARDOUS GOODS" };
			adjustmentLineDataObject.ExpiryDate = new ZDate(2011, 1, 2);
			adjustmentLineDataObject.LineComment = "SO LARGE";
			adjustmentLineDataObject.LineNumber = new ZShort(2);
			adjustmentLineDataObject.Location = new Location();
			adjustmentLineDataObject.Location.Column = new ZShort(2);
			adjustmentLineDataObject.Location.Level = new ZShort(4);
			adjustmentLineDataObject.Location.Tray = new ZShort(3);
			adjustmentLineDataObject.Location.Row = "A";
			adjustmentLineDataObject.OrderedQty = 22.2m;
			adjustmentLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "NO", Description = "Number" };
			adjustmentLineDataObject.PackageQty = 22.2m;
			adjustmentLineDataObject.PackageQtyUnit = new PackageType { Code = "PLT", Description = "Pallet" };
			adjustmentLineDataObject.PackingDate = new ZDate(2011, 1, 3);
			adjustmentLineDataObject.PalletID = "PALLET~1";
			adjustmentLineDataObject.PartAttribute1 = "Zise";
			adjustmentLineDataObject.PartAttribute2 = "Siwe";
			adjustmentLineDataObject.PartAttribute3 = "Locour";
			adjustmentLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			adjustmentLineDataObject.SubLineNumber = new ZShort(4);
			adjustmentLineDataObject.AdjustmentReason = new CodeDescriptionPair { Code = "CLI", Description = "Client Instructed" };
			adjustmentLineDataObject.InventoryStatus = new CodeDescriptionPair { Code = "AVL", Description = "Available" };
			adjustmentLineDataObject.CustomsData = new CustomsEntryInfo { AllDutiesAmount = 10m, VATAmount = 2.3m };
			return adjustmentLineDataObject;
		}

		internal static void ChangeAdjustmentLineDataValues(OrderLine adjustmentLineDataObject)
		{
			adjustmentLineDataObject.AdjustmentReason = new CodeDescriptionPair { Code = "INV", Description = "Inventory Count" };
			adjustmentLineDataObject.ArrivalDate = new ZDateTimeOffset(2015, 1, 1);
			adjustmentLineDataObject.ExpiryDate = new ZDate(2015, 12, 31);
			adjustmentLineDataObject.LineComment = "ChangedComment";
			adjustmentLineDataObject.Location = new Location();
			adjustmentLineDataObject.Location.Column = new ZShort(1);
			adjustmentLineDataObject.Location.Level = new ZShort(2);
			adjustmentLineDataObject.Location.Tray = new ZShort(1);
			adjustmentLineDataObject.Location.Row = "A";
			adjustmentLineDataObject.OrderedQty = 20.2m;
			adjustmentLineDataObject.PackageQty = 30.3m;
			adjustmentLineDataObject.PackageQtyUnit = new PackageType { Code = "BOX", Description = "Box" };
			adjustmentLineDataObject.PackingDate = new ZDateTime(2015, 11, 30);
			adjustmentLineDataObject.PalletID = "PALLET_CHG";
			adjustmentLineDataObject.PartAttribute1 = "Changed01";
			adjustmentLineDataObject.PartAttribute2 = "Changed02";
			adjustmentLineDataObject.PartAttribute3 = "Changed03";
			adjustmentLineDataObject.SerialNumber = "Changed04";
			adjustmentLineDataObject.ReservedQuantity = 40.4m;
			adjustmentLineDataObject.SplitQuantity = 50.5m;
		}

		internal static void AssertContents(WhsAdjustmentLine adjustmentLine, bool useSerial)
		{
			AssertEquals("WE_AdjustmentArrivalDate", new ZDateTimeOffset(2011, 1, 1), adjustmentLine.WE_AdjustmentArrivalDate);
			AssertEquals("WE_ExpiryDate", new ZDate(2011, 1, 2), adjustmentLine.WE_ExpiryDate);
			AssertEquals("WE_F3_NKPackType", "PLT", adjustmentLine.WE_F3_NKPackType);
			AssertEquals("WE_LineComment", "SO LARGE", adjustmentLine.WE_LineComment);
			AssertEquals("WE_LineNo", new ZShort(2), adjustmentLine.WE_LineNo);
			AssertEquals("ProductCode", "BOWLHAT", adjustmentLine.ProductCode);
			AssertEquals("ProductDesc", "Bowler Hat", adjustmentLine.ProductDesc);
			AssertEquals("WE_PackingDate", new ZDateTime(2011, 1, 3), adjustmentLine.WE_PackingDate);
			AssertEquals("WE_PackQuantity", 22.2m, adjustmentLine.WE_PackQuantity);
			AssertEquals("WE_PalletID", "PALLET~1", adjustmentLine.WE_PalletID);
			AssertEquals("WE_PartAttrib1", "Zise", adjustmentLine.WE_PartAttrib1);
			AssertEquals("WE_PartAttrib2", "Siwe", adjustmentLine.WE_PartAttrib2);
			AssertEquals("WE_PartAttrib3", "Locour", adjustmentLine.WE_PartAttrib3);

			var expectedSerialNumber = "SNNN";
			AssertEquals("WE_SerialNumber", useSerial ? expectedSerialNumber : "", adjustmentLine.WE_SerialNumber);
			AssertEquals("WE_SubLineNo", new ZShort(4), adjustmentLine.WE_SubLineNo);
			AssertEquals("WE_TransactionQuantity", 22.2m, adjustmentLine.WE_TransactionQuantity);
			AssertEquals("WE_UnitsUQ", "UNT", adjustmentLine.ProductUQ);

			AssertEquals("Location.RowName", "A", adjustmentLine.Location.RowName);
			AssertEquals("Location.WLV_Column", new ZShort(2), adjustmentLine.Location.WLV_Column);
			AssertEquals("Location.WLV_Level", new ZShort(4), adjustmentLine.Location.WLV_Level);
			AssertEquals("Location.WLV_Tray", new ZShort(3), adjustmentLine.Location.WLV_Tray);

			AssertEquals("WE_ReasonCode", "CLI", adjustmentLine.WE_ReasonCode);
			AssertEquals("WE_OriginalInventoryStatus", "AVL", adjustmentLine.WE_OriginalInventoryStatus);
			AssertEquals("WE_CurrentInventoryStatus", "AVL", adjustmentLine.WE_CurrentInventoryStatus);

			AssertEquals("WB_BondedWhsQty", 22.2m, adjustmentLine.CustomsData.WB_BondedWhsQty);
			AssertEquals("WB_AllDutiesAmount", 10m, adjustmentLine.CustomsData.WB_AllDutiesAmount);
			AssertEquals("WB_VATAmount", 2.3m, adjustmentLine.CustomsData.WB_VATAmount);
		}

		protected override void AssertReadOnlyFields(OrderLine originalDataObject, OrderLine changedDataObject, WhsAdjustmentLine modifiedDocketLine, bool isCustoms = false)
		{
			base.AssertReadOnlyFields(originalDataObject, changedDataObject, modifiedDocketLine, isCustoms);
			CombineAssertions("Following assertions failed for adjustment line specific fields:", () =>
			{
				AssertReadOnlyFieldIsUnchanged(originalDataObject.AdjustmentReason?.Code, changedDataObject.AdjustmentReason?.Code, modifiedDocketLine.WE_ReasonCodeInfo, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.ArrivalDate, changedDataObject.ArrivalDate, modifiedDocketLine.WE_AdjustmentArrivalDateInfo, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.Location.Column, changedDataObject.Location.Column, modifiedDocketLine.Location.WLV_ColumnInfo, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.Location.Level, changedDataObject.Location.Level, modifiedDocketLine.Location.WLV_LevelInfo, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.Location.Tray, changedDataObject.Location.Tray, modifiedDocketLine.Location.WLV_TrayInfo, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.Location.Row, changedDataObject.Location.Row, modifiedDocketLine.Location.Row.WR_NameInfo, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.PalletID, changedDataObject.PalletID, modifiedDocketLine.WE_PalletIDInfo, isCustoms);
			});
		}

		protected override OrderLine GetNewDocketLineDataObject()
		{
			return GetNewAdjustmentLine();
		}

		protected override void AssertDocketLineContents(WhsAdjustmentLine docketLine)
		{
			AssertContents(docketLine, useSerial: false);
		}

		protected override string GetDocketType()
		{
			return "Adjustment";
		}

		protected override WhsAdjustment GetNewDocket(OrgHeader client, WhsWarehouse warehouse)
		{
			return Helper.CreateWhsAdjustment(client, warehouse);
		}

		protected override WhsAdjustmentLineDataObjectReader GetNewReader(OrderLine docketLineDataObject, IXmlImportLogger logger, WhsAdjustment adjustment, bool useCleanFactory = true)
		{
			return new WhsAdjustmentLineDataObjectReader(docketLineDataObject, logger, useCleanFactory ? new UniversalObjectFactory() : Factory, adjustment);
		}

		protected override bool SupportsProductCreation => false;
		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseOrder);

		#endregion
	}
}
