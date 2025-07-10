using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsAdjustmentDataObjectReaderTest : WhsDocketDataObjectReaderTest<WhsAdjustment, WhsAdjustmentLine, WhsAdjustmentDataObjectReader>
	{
		// matching

		#region TestMatchingOnClientAndExternalReference

		protected override bool DocketSupportsSplitNo => false;

		#endregion

		// create / update job

		#region TestBasicAdjustmentLevelFieldMappings

		public void TestBasicAdjustmentLevelFieldMappings()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.Order.Status = new CodeDescriptionPair { Code = "FIN", Description = "Finalized" };

			var reader = new WhsAdjustmentDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsAdjustmentBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);

			AssertNotNull("whsAdjustmentBO", whsAdjustmentBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsAdjustmentBO.WD_ExternalReference", "ADJUSTME", whsAdjustmentBO.WD_ExternalReference);
				AssertEquals("whsAdjustmentBO.WD_DocketStatus was not set", "ENT", whsAdjustmentBO.WD_DocketStatus);
				AssertEquals("whsAdjustmentBO.Warehouse.WW_WarehouseCode", "WHS", whsAdjustmentBO.Warehouse.WW_WarehouseCode);
				AssertLocalClientAddress(whsAdjustmentBO.Client.MainAddress);
			});
		}

		#endregion

		#region TestAdjustmentLines

		#region TestLineCollectionContentIsPartial

		protected override void SetupAndAssertDocketLinePrecondition(WhsDocketLine docketLine)
		{
			docketLine.WE_PalletID = "Pallet1";
			AssertEquals("Pre-condition: WE_PalletID", "Pallet1", docketLine.WE_PalletID);
		}

		protected override void UpdateExistingDocketLineDataObject(OrderLine docketLineDataObject)
		{
			docketLineDataObject.PalletID = "Pallet2";
		}

		protected override void AssertExistingDocketLineAfterImport(WhsDocketLine docketLine)
		{
			AssertEquals("WE_PalletID get updated.", "Pallet2", docketLine.WE_PalletID);
		}

		protected override WhsAdjustment CreateDocketWithLine(OrgHeader client, WhsWarehouse warehouse, OrgSupplierPart product, ZDecimal units)
		{
			var adjustment = Helper.CreateWhsAdjustment(client, warehouse);
			adjustment.IsUniqueExternalReferenceCreatedOnSave = false;
			Helper.CreateWhsAdjustmentLine(adjustment, product, units, warehouse.DefaultLocation);

			return adjustment;
		}

		protected override UniversalShipment GetNewDocketDataObject(WhsAdjustment docket)
		{
			var writer = new WhsAdjustmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, docket)));
			var adjustmentDataObject = writer.GetDataObject(docket);

			return adjustmentDataObject;
		}

		#endregion

		#region TestWithAdjustmentLines

		public void TestWithAdjustmentLines()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var adjustment = WhsAdjustmentLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var product = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			Helper.SetClientAllAttributeType(adjustment.Client, false);
			Helper.SetClientAttributeType(adjustment.Client, AttributeNumber.Serial, true);
			Helper.SetProductAllAttributeUse(adjustment.Client, product, true);

			Factory.SaveForTesting();

			var adjustmentLineDataObject = WhsAdjustmentLineDataObjectReaderTest.GetNewAdjustmentLine();
			adjustmentLineDataObject.SerialNumber = "SNNN";
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { adjustmentLineDataObject });

			var reader = new WhsAdjustmentDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsAdjustmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsAdjustmentBO);
			AssertEquals("whsAdjustmentBO.Lines.Count", 1, whsAdjustmentBO.Lines.Count);

			CombineAssertions(delegate
			{
				var adjustLineBO = whsAdjustmentBO.Lines[0];
				WhsAdjustmentLineDataObjectReaderTest.AssertContents(adjustLineBO, useSerial: true);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsAdjustment found, creating new WhsAdjustment.
Information - Populating WhsAdjustment...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsAdjustmentLine found, creating new WhsAdjustmentLine.
Information - Populating WhsAdjustmentLine...
Information - Added Warehouse Adjustment from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestReader_DuplicateAdjustmentLineNumber

		public void TestReader_DuplicateAdjustmentLineNumber()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			WhsAdjustmentLineDataObjectReaderTest.GetNewDocketLineParent(Factory);

			Factory.SaveForTesting();

			var adjustmentLineDataObject1 = WhsAdjustmentLineDataObjectReaderTest.GetNewAdjustmentLine();
			SetLineSubLineNumber(adjustmentLineDataObject1, new ZShort(2), new ZShort(1));
			var adjustmentLineDataObject2 = WhsAdjustmentLineDataObjectReaderTest.GetNewAdjustmentLine();
			SetLineSubLineNumber(adjustmentLineDataObject2, new ZShort(2), new ZShort(2));
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { adjustmentLineDataObject1, adjustmentLineDataObject2 });

			var reader = new WhsAdjustmentDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsAdjustmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsAdjustmentBO);
			AssertEquals("Order should have 2 order lines.", 2, whsAdjustmentBO.Lines.Count);
			AssertAdjustmentLine(whsAdjustmentBO.Lines[0], 2, 1);
			AssertAdjustmentLine(whsAdjustmentBO.Lines[1], 2, 2);
		}

		void SetLineSubLineNumber(OrderLine line, ZShort lineNo, ZShort subLineNo)
		{
			line.LineNumber = lineNo;
			line.SubLineNumber = subLineNo;
		}

		void AssertAdjustmentLine(WhsAdjustmentLine line, ZShort lineNo, ZShort subLineNo)
		{
			AssertEquals("LineNo should be " + lineNo.ToString(), lineNo, line.WE_LineNo);
			AssertEquals("SubLineNo should be " + subLineNo.ToString(), subLineNo, line.WE_SubLineNo);
		}

		#endregion

		#region TestReader_AdjustmentOut

		public void TestReader_AdjustmentOut()
		{
			// create inventory to adjust out
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();
			var part = Data.ProductCRAHOLSYD;
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", part, 10m, whs.FindLocation("A-1-1-1"), "");
			Factory.SaveForTesting();

			// build dataObject to import as Adjustment Out
			var adjustmentLineDataObject = Data.CreateOrderLine(part, -10m);
			adjustmentLineDataObject.AdjustmentReason = new CodeDescriptionPair { Code = "CLI", Description = "Client Instructed" };
			adjustmentLineDataObject.Location = new Location { Row = "A", Column = 1, Level = 1, Tray = 1 };

			var adjustmentDataObject = Data.ShipmentDataObject;
			adjustmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>(new[] { adjustmentLineDataObject }));

			// import and save
			var reader = new WhsAdjustmentDataObjectReader(adjustmentDataObject, Logger, Factory);
			var adjustmentBO = reader.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals("Precondition - ensure adjustment out line is created.", -10m, adjustmentBO.Lines.Single().WE_TransactionQuantity);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsAdjustment found, creating new WhsAdjustment.
Information - Populating WhsAdjustment...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsAdjustmentLine found, creating new WhsAdjustmentLine.
Information - Populating WhsAdjustmentLine...
Information - Added Warehouse Adjustment from UniversalShipment.
".Trim(), Logger.Logs);
				AssertNoExceptionThrown("Import should be successful, and results should be saved.", () => Factory.SaveAtEndOfImport(Logger));
			});
		}

		#endregion

		#region TestReader_AdjustmentOut_NotEnoughStock

		public void TestReader_AdjustmentOut_NotEnoughStock()
		{
			// create inventory to adjust out
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();
			var part = Data.ProductCRAHOLSYD;
			Factory.SaveForTesting();

			// build dataObject to import as Adjustment Out
			var adjustmentLineDataObject = Data.CreateOrderLine(part, -10m);
			adjustmentLineDataObject.AdjustmentReason = new CodeDescriptionPair { Code = "CLI", Description = "Client Instructed" };
			adjustmentLineDataObject.Location = new Location { Row = "A", Column = 1, Level = 1, Tray = 1 };

			var adjustmentDataObject = Data.ShipmentDataObject;
			adjustmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>(new[] { adjustmentLineDataObject }));

			// import and save
			var reader = new WhsAdjustmentDataObjectReader(adjustmentDataObject, Logger, Factory);
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				string.Format(@"Cannot Import Adjustment
Adjustment could not be imported into the Warehouse because there is not enough stock for some of the adjustment out line(s) to commit."),
				() => reader.ReadIntoBusinessObject());
		}

		#endregion

		// to be uncommented and perhaps moved to base when we decide to FIX import properly
		//		#region TestReader_RunsValidationBeforeSave

		//		public void TestReader_RunsValidationBeforeSave()
		//		{
		//			 create inventory to adjust out
		//			var client = Data.CreateClientOrgCRAHOLSYDInDB();
		//			var whs = Data.GetOrCreateWarehouseInDB();
		//			var part = Data.ProductCRAHOLSYD;
		//			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", part, 10m, whs.FindLocation("A-1-1-1"), "PLT-1");
		//			Factory.SaveForTesting();

		//			 build dataObject to import as Adjustment In of same Pallet ID into different location
		//			var adjustmentLineDataObject = Data.CreateOrderLine(part, 10m);
		//			adjustmentLineDataObject.AdjustmentReason = new CodeDescriptionPair { Code = "CLI", Description = "Client Instructed" };
		//			adjustmentLineDataObject.Location = new Location { Row = "A", Column = 2, Level = 1, Tray = 1 }; // different location
		//			adjustmentLineDataObject.PalletID = "PLT-1";

		//			var adjustmentDataObject = Data.ShipmentDataObject;
		//			adjustmentDataObject.Order.OrderLineCollection = new DataObjectList<OrderLine>(new[] { adjustmentLineDataObject });

		//			 import and save
		//			var reader = new WhsAdjustmentDataObjectReader(adjustmentDataObject, Logger, Factory);
		//			AssertExceptionThrown(typeof(DataObjectReadFailureException),
		//				string.Format(@"Cannot Import Adjustment
		//Adjustment could not be imported into the Warehouse because of the following error(s):
		//Error - WE_PalletID: Another location (A-1-1-1) was already used for the same Pallet ID. Please select another location or Pallet ID."),
		//() => reader.ReadIntoBusinessObject());
		//		}

		//		#endregion

		#region TestAdjustmentLinesGetCorrectIndexForExceptionMessage

		public void TestAdjustmentLinesGetCorrectIndexForExceptionMessage()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			WhsAdjustmentLineDataObjectReaderTest.GetNewDocketLineParent(Factory);

			Factory.SaveForTesting();

			var adjustmentLineDataObject = WhsAdjustmentLineDataObjectReaderTest.GetNewAdjustmentLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { adjustmentLineDataObject, new OrderLine() });

			var reader = new WhsAdjustmentDataObjectReader(ShipmentDataObject, Logger, Factory);
			AssertExceptionThrown("Cannot import AdjustmentLine without valid Product Code.", typeof(DataObjectReadFailureException),
				"Cannot Import Adjustment Line 2\r\nNo Product was provided.", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestAdjustmentLinesAreNotUpdatedOnAFinalizedAdjustment

		public void TestAdjustmentLinesAreNotUpdatedOnAFinalizedAdjustment()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var whsAdjustment = WhsAdjustmentLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var line = whsAdjustment.Lines.AddNew();
			line.FillWithValidTestData();
			line.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			line.WE_TransactionQuantity = 1m;
			whsAdjustment.WD_DocketID = "W00000001";
			whsAdjustment.Warehouse.WW_WarehouseName = "CoolShack";
			whsAdjustment.Warehouse.WW_WarehouseCode = "WSS";
			whsAdjustment.WD_ExternalReference = "ORDERME";
			whsAdjustment.IsUniqueExternalReferenceCreatedOnSave = false;
			whsAdjustment.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(whsAdjustment);
			Factory.SaveForTesting();

			var adjustmentLineDataObject = WhsAdjustmentLineDataObjectReaderTest.GetNewAdjustmentLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { adjustmentLineDataObject, adjustmentLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsAdjustmentDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsAdjustmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsAdjustmentBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsAdjustmentBO.Lines.Count", 1, whsAdjustmentBO.Lines.Count);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsAdjustment.
Information - Populating WhsAdjustment...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Warning - Cannot update read-only Field 'Warehouse' [WD_WW_Whs]. Cannot change 'WSS' (CoolShack) to 'WHS' (CoolHouse).
Warning - Cannot update Adjustment Lines on a Finalized Adjustment.
Information - Updated Warehouse Adjustment W00000001 from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		public void TestAdjustment_WithJobCosting()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			Factory.SaveForTesting();

			ShipmentDataObject.Order.Status = new CodeDescriptionPair { Code = "FIN", Description = "Finalized" };
			ShipmentDataObject.DataContext.CodesMappedToTarget = true; // Required to import JobCosting
			ShipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			ShipmentDataObject.JobCosting = new JobCosting();
			ShipmentDataObject.JobCosting.Branch = new Branch();
			ShipmentDataObject.JobCosting.Branch.Code = "SYD";
			ShipmentDataObject.JobCosting.Department = new Department();
			ShipmentDataObject.JobCosting.Department.Code = "FES";

			var reader = new WhsAdjustmentDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsAdjustmentBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);

			AssertNotNull("whsAdjustmentBO", whsAdjustmentBO);
			AssertNull("whsAdjustmentBO.JobHeader", whsAdjustmentBO.JobHeader);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			ShipmentDataObject.Order.OrderNumber = "ADJUSTME";
		}

		protected override bool SupportsAdditionalReferences => false;

		protected override string GetProcessType() => "WAJ";

		protected override WhsAdjustmentDataObjectReader GetNewReader(UniversalShipment shipmentDataObject, IXmlImportLogger logger, bool useCleanFactory = false)
		{
			return new WhsAdjustmentDataObjectReader(shipmentDataObject, logger, useCleanFactory ? new UniversalObjectFactory() : Factory);
		}

		protected override string GetDocketType() => "Adjustment";

		protected override WhsAdjustment GetNewDocket(OrgHeader client, WhsWarehouse warehouse, string externalReference = "")
		{
			var adjustment = Helper.CreateWhsAdjustment(client, warehouse, externalReference);
			adjustment.IsUniqueExternalReferenceCreatedOnSave = false;
			return adjustment;
		}

		protected override WhsDocket GetNewDocketOfDifferentType() => Factory.NewWithValidTestData<WhsOrder>();

		protected override DataContextType DataContext => DataContextType.WarehouseAdjustment;

		#endregion
	}
}
