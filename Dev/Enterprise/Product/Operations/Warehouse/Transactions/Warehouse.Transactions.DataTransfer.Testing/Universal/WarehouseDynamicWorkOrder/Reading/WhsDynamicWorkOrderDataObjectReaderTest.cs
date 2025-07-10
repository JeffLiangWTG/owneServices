using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsDynamicWorkOrderDataObjectReaderTest : WhsDocketDataObjectReaderTest<WhsDynamicWorkOrder, WhsDynamicWorkOrderLine, WhsDynamicWorkOrderDataObjectReader>
	{
		#region TestBasicOrderLevelFieldMappings

		public void TestBasicOrderLevelFieldMappings()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			Factory.SaveForTesting();

			ShipmentDataObject.OuterPacks = 3;
			ShipmentDataObject.TotalVolume = 32.1m;
			ShipmentDataObject.TotalVolumeUnit = new UnitOfVolume { Code = "CY", Description = "Cubic Yards" };
			ShipmentDataObject.TotalWeight = 41.2m;
			ShipmentDataObject.TotalWeightUnit = new UnitOfWeight { Code = "LB", Description = "Pounds" };

			var order = ShipmentDataObject.Order;
			order.AutoFinaliseBOMIntoInventory = true;
			order.IsInwardsProcessingJob = true;
			order.Type = new CodeDescriptionPair { Code = "ASS", Description = "Assemble" };
			order.OrderNumber = "ORDERME";
			order.OrderNumberSplit = new ZByte(1);
			order.PickOption = new CodeDescriptionPair { Code = "AUT", Description = "Auto" };
			order.Status = new CodeDescriptionPair { Code = "HEL", Description = "Held" }; // Disregard this
			order.TotalLineVolume = 12.5m;
			order.PalletsSent = new ZShort(4);
			order.TotalUnits = 12.3m;
			order.TotalLineWeight = 32.6m;

			ShipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>() { new AdditionalReference { Type = new EntryType { Code = "HSB" }, ReferenceNumber = "BILL" } });
			ShipmentDataObject.LocalProcessing = new LocalProcessing { DeliveryRequiredBy = new ZDateTime(2011, 1, 1) };

			var reader = new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsDynamicWorkOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDynamicWorkOrderBO);

			CombineAssertions(() =>
			{
				AssertEquals("dynamicWorkOrderBO.Warehouse.WW_WarehouseCode", "WHS", whsDynamicWorkOrderBO.Warehouse.WW_WarehouseCode);
				AssertEquals("dynamicWorkOrderBO.WD_DocketSubType", "ASS", whsDynamicWorkOrderBO.WD_DocketSubType);
				AssertEquals("dynamicWorkOrderBO.WD_RequiredDate", new ZDateTimeOffset(2011, 1, 1), whsDynamicWorkOrderBO.WD_RequiredDate);
				AssertEquals("dynamicWorkOrderBO.WD_ExternalReference", "ORDERME", whsDynamicWorkOrderBO.WD_ExternalReference);
				AssertEquals("dynamicWorkOrderBO.WD_ExternalReferenceSplit", new ZByte(1), whsDynamicWorkOrderBO.WD_ExternalReferenceSplit);
				AssertEquals("dynamicWorkOrderBO.WD_PickOption", "AUT", whsDynamicWorkOrderBO.WD_PickOption);

				AssertEquals("dynamicWorkOrderBO.WD_TotalUnits", 12.3m, whsDynamicWorkOrderBO.WD_TotalUnits);
				AssertEquals("dynamicWorkOrderBO.WD_TotalWeight", 32.6m, whsDynamicWorkOrderBO.WD_TotalWeight);
				AssertEquals("dynamicWorkOrderBO.WD_TotalWeightUnit", "LB", whsDynamicWorkOrderBO.WD_TotalWeightUnit);
				AssertEquals("dynamicWorkOrderBO.WD_TotalCubic", 12.5m, whsDynamicWorkOrderBO.WD_TotalCubic);
				AssertEquals("dynamicWorkOrderBO.WD_TotalCubicUnit", "CY", whsDynamicWorkOrderBO.WD_TotalCubicUnit);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsDynamicWorkOrder found, creating new WhsDynamicWorkOrder.
Information - Populating WhsDynamicWorkOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsDocketReference found, creating new WhsDocketReference.
Information - Populating WhsDocketReference...
Information - Added Warehouse Dynamic Work Order from UniversalShipment.
".Trim(), Logger.Logs);
			});

			Factory.SaveForTesting(); // Assert can save to the database
			AssertEquals("dynamicWorkOrderBO.WD_DocketStatus", "ENT", whsDynamicWorkOrderBO.WD_DocketStatus);
		}

		#endregion

		#region TestReadIntoBusinessObject

		public void TestReadIntoBusinessObject_Lines()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var product = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			Helper.SetClientAllAttributeType(order.Client, false);
			Helper.SetProductAllAttributeUse(order.Client, product, true);
			Helper.SetClientAttributeType(order.Client, AttributeNumber.Serial, true);
			Factory.SaveForTesting();

			var orderLineDataObject = WhsDynamicWorkOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.Lines.Count", 1, whsOrderBO.Lines.Count);

			CombineAssertions(() =>
			{
				var orderLineBO = whsOrderBO.Lines[0];
				WhsDynamicWorkOrderLineDataObjectReaderTest.AssertStandardOrderLineContents(orderLineBO, useSerial: true);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching WhsDynamicWorkOrder found, creating new WhsDynamicWorkOrder.
Information - Populating WhsDynamicWorkOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsDynamicWorkOrderLine found, creating new WhsDynamicWorkOrderLine.
Information - Populating WhsDynamicWorkOrderLine...
Information - No matching Component WhsDynamicWorkOrderLine found, creating new Component WhsDynamicWorkOrderLine.
Information - Populating Component WhsDynamicWorkOrderLine...
Information - Added Warehouse Dynamic Work Order from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestReadIntoBusinessObject_Lines_UpdatingLines()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var product = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			var otherProduct = Helper.CreateProduct("PROD2", order.Client);
			Helper.SetClientAllAttributeType(order.Client, false);
			Helper.SetProductAllAttributeUse(order.Client, product, true);
			Helper.SetClientAttributeType(order.Client, AttributeNumber.Serial, true);
			Factory.SaveForTesting();

			var orderLineDataObject1 = WhsDynamicWorkOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject1.LineNumber = 1;
			orderLineDataObject1.SubLineNumber = 1;

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1 });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = 1;

			// Import
			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("Precondition: whsOrderBO.Lines.Count", 1, whsOrderBO.Lines.Count);

			var orderLineBO = whsOrderBO.Lines[0];
			AssertNotEquals("Precondition.", "OTHER", orderLineBO.WE_PartAttrib1);

			// Update
			orderLineDataObject1.PartAttribute1 = "OTHER";

			var orderLineDataObject2 = new OrderLine();
			orderLineDataObject2.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject2.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject2.OrderedQty = 40m;
			orderLineDataObject2.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject2.Product = new Product { Code = "PROD2", Description = "SECOND PROD" };
			orderLineDataObject2.LineNumber = 2;
			orderLineDataObject2.SubLineNumber = 1;
			orderLineDataObject2.CustomsData.IsMainInwardsProcessedItem = false;
			orderLineDataObject2.CustomsData.IsSecondaryInwardsProcessedItem = true;
			orderLineDataObject2.CustomsData.InwardsEntryKey = "ABC";
			orderLineDataObject2.CustomsData.InwardsEntryLineNumber = new ZShort(2);
			ShipmentDataObject.Order.OrderLineCollection.Add(orderLineDataObject2);

			CombineAssertions(() =>
			{
				AssertEquals("Should have updated docket.", whsOrderBO, reader.ReadIntoBusinessObject());
				AssertEquals("Should have updated the lines.", 2, whsOrderBO.Lines.Count);
				AssertEquals("Should have updated the lines.", "OTHER", whsOrderBO.Lines[0].WE_PartAttrib1);
			});
		}

		public void TestReadIntoBusinessObject_Lines_Picked_RejectsUpdatingLines()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var product = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			Helper.SetClientAllAttributeType(order.Client, false);
			Helper.SetProductAllAttributeUse(order.Client, product, true);
			Helper.SetClientAttributeType(order.Client, AttributeNumber.Serial, true);
			Factory.SaveForTesting();

			var orderLineDataObject1 = WhsDynamicWorkOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject1.LineNumber = 1;
			orderLineDataObject1.SubLineNumber = 1;

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1 });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = 1;

			// Import
			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("Precondition: whsOrderBO.Lines.Count", 1, whsOrderBO.Lines.Count);

			var orderLineBO = whsOrderBO.Lines[0];
			AssertNotEquals("Precondition.", "OTHER", orderLineBO.WE_PartAttrib1);
			AssertNotEquals("Precondition.", 42, whsOrderBO.WD_PackagesSent);

			// Update
			ShipmentDataObject.OuterPacks = 42;
			orderLineDataObject1.PartAttribute1 = "OTHER";

			var orderLineDataObject2 = WhsDynamicWorkOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject2.LineNumber = 2;
			orderLineDataObject2.SubLineNumber = 2;
			ShipmentDataObject.Order.OrderLineCollection.Add(orderLineDataObject2);

			var pick = Factory.New<WhsPick>();
			whsOrderBO.WD_WP = pick.PK;

			var whsDocket0Updated = reader.ReadIntoBusinessObject();
			AssertNotNull(whsDocket0Updated);
			CombineAssertions(() =>
			{
				AssertEquals("Should have updated the existing docket.", whsOrderBO.PK, whsDocket0Updated.PK);
				AssertEquals("Should *not* have updated the lines.", 1, whsDocket0Updated.Lines.Count);
				AssertNotEquals("Should *not* have updated lines.", "OTHER", whsDocket0Updated.Lines[0].WE_PartAttrib1);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsDynamicWorkOrder found, creating new WhsDynamicWorkOrder.
Information - Populating WhsDynamicWorkOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsDynamicWorkOrderLine found, creating new WhsDynamicWorkOrderLine.
Information - Populating WhsDynamicWorkOrderLine...
Information - No matching Component WhsDynamicWorkOrderLine found, creating new Component WhsDynamicWorkOrderLine.
Information - Populating Component WhsDynamicWorkOrderLine...
Information - Added Warehouse Dynamic Work Order from UniversalShipment.
Information - Successfully loaded matching WhsDynamicWorkOrder.
Information - Populating WhsDynamicWorkOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Warning - Cannot update Dynamic Work Order Lines on a Finalized or In Picking Dynamic Work Order.
Information - Updated Warehouse Dynamic Work Order from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestReadIntoBusinessObject_Lines_CustomsData()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var otherProduct = Helper.CreateProduct("PROD2", order.Client);
			Factory.SaveForTesting();

			var orderLineDataObject1 = new OrderLine();
			orderLineDataObject1.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject1.OrderedQty = 28m;
			orderLineDataObject1.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject1.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			orderLineDataObject1.LineNumber = 1;
			orderLineDataObject1.SubLineNumber = 1;
			orderLineDataObject1.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject1.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject1.CustomsData.IsSecondaryInwardsProcessedItem = false;
			orderLineDataObject1.CustomsData.InwardsEntryKey = "ABC";
			orderLineDataObject1.CustomsData.InwardsEntryLineNumber = new ZShort(2);

			var orderLineDataObject2 = new OrderLine();
			orderLineDataObject2.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject2.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject2.OrderedQty = 40m;
			orderLineDataObject2.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject2.Product = new Product { Code = "PROD2", Description = "SECOND PROD" };
			orderLineDataObject2.LineNumber = 2;
			orderLineDataObject2.SubLineNumber = 1;
			orderLineDataObject2.CustomsData.IsMainInwardsProcessedItem = false;
			orderLineDataObject2.CustomsData.IsSecondaryInwardsProcessedItem = true;
			orderLineDataObject2.CustomsData.InwardsEntryKey = "ABC";
			orderLineDataObject2.CustomsData.InwardsEntryLineNumber = new ZShort(2);

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1, orderLineDataObject2 });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = 1;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.Lines.Count", 2, whsOrderBO.Lines.Count);

			var orderLineBO1 = whsOrderBO.Lines.Cast<WhsDynamicWorkOrderLine>().Single(line => line.WE_TransactionQuantity == 28m);
			AssertEquals(true, orderLineBO1.IsMainInwardProcessedItem);
			AssertEquals(false, orderLineBO1.IsSecondaryInwardProcessedItem);
			AssertEquals(string.Empty, orderLineBO1.WE_BondedEntryKey);

			var orderLineBO2 = whsOrderBO.Lines.Cast<WhsDynamicWorkOrderLine>().Single(line => line.WE_TransactionQuantity == 40m);
			AssertEquals(false, orderLineBO2.IsMainInwardProcessedItem);
			AssertEquals(true, orderLineBO2.IsSecondaryInwardProcessedItem);
			AssertEquals(string.Empty, orderLineBO2.WE_BondedEntryKey);
		}

		public void TestReadIntoBusinessObject_ComponentLines_CustomsData()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var otherProduct = Helper.CreateProduct("PROD2", order.Client);
			Factory.SaveForTesting();

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject.OrderedQty = 28m;
			orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			orderLineDataObject.LineNumber = 1;
			orderLineDataObject.SubLineNumber = 1;
			orderLineDataObject.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject.CustomsData.IsMainInwardsProcessedItem = true;

			var componentOrderLine = new OrderLine();
			componentOrderLine.Product = new Product { Code = "PROD2", Description = "Other Prod" };
			componentOrderLine.LineNumber = new ZShort(2);
			componentOrderLine.SubLineNumber = new ZShort(4);
			componentOrderLine.OrderedQty = orderLineDataObject.OrderedQty;
			componentOrderLine.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var componentCustomsData = new CustomsEntryInfo();
			componentCustomsData.InwardsEntryKey = "ABC";
			componentCustomsData.InwardsEntryLineNumber = new ZShort(2);
			componentCustomsData.IsMainInwardsProcessedItem = true;
			componentOrderLine.CustomsData = componentCustomsData;
			orderLineDataObject.SetOrderLineCollection(() => new List<OrderLine>() { componentOrderLine });

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = 1;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.Lines.Count", 1, whsOrderBO.Lines.Count);

			var componentOderLine = (WhsDynamicWorkOrderLine)whsOrderBO.Lines[0].ChildComponentLinesCollection.Single();
			AssertEquals(false, componentOderLine.IsMainInwardProcessedItem);
			AssertEquals(false, componentOderLine.IsSecondaryInwardProcessedItem);
			AssertEquals("ABC-2", componentOderLine.WE_BondedEntryKey);
		}

		public void TestReadIntoBusinessObject_ComponentLines_WithComponentLines()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var otherProduct = Helper.CreateProduct("PROD2", order.Client);
			Factory.SaveForTesting();

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject.OrderedQty = 28m;
			orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			orderLineDataObject.LineNumber = 1;
			orderLineDataObject.SubLineNumber = 1;
			orderLineDataObject.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject.CustomsData.IsMainInwardsProcessedItem = true;

			var componentOrderLine = new OrderLine();
			componentOrderLine.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			componentOrderLine.Product = new Product { Code = "PROD2", Description = "Other Prod" };
			componentOrderLine.LineNumber = new ZShort(2);
			componentOrderLine.SubLineNumber = new ZShort(4);
			componentOrderLine.OrderedQty = orderLineDataObject.OrderedQty;
			componentOrderLine.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var componentCustomsData = new CustomsEntryInfo();
			componentCustomsData.InwardsEntryKey = "ABC";
			componentCustomsData.InwardsEntryLineNumber = new ZShort(2);
			componentCustomsData.IsMainInwardsProcessedItem = true;
			componentOrderLine.CustomsData = componentCustomsData;

			var childComponentOrderLine = new OrderLine();
			childComponentOrderLine.Product = new Product { Code = "PROD2", Description = "Other Prod" };
			childComponentOrderLine.LineNumber = new ZShort(2);
			childComponentOrderLine.SubLineNumber = new ZShort(4);
			childComponentOrderLine.OrderedQty = orderLineDataObject.OrderedQty;
			childComponentOrderLine.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var childComponentCustomsData = new CustomsEntryInfo();
			childComponentCustomsData.InwardsEntryKey = "ABC";
			childComponentCustomsData.InwardsEntryLineNumber = new ZShort(2);
			childComponentCustomsData.IsMainInwardsProcessedItem = true;
			childComponentOrderLine.CustomsData = childComponentCustomsData;

			componentOrderLine.SetOrderLineCollection(() => new List<OrderLine>() { childComponentOrderLine });
			orderLineDataObject.SetOrderLineCollection(() => new List<OrderLine>() { componentOrderLine });
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = 1;

			AssertExceptionThrown<DataObjectReadFailureException>("Failed to import",
				@"Component lines cannot have child component lines.",
				() => new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestMatchesLatestDynamicWorkOrderWhenAllMatches()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			var dummyDocket = GetNewDocket(client, warehouse, "1");
			dummyDocket.WD_ExternalReference = "ORDERME1";
			dummyDocket.WD_ExternalReferenceSplit = new ZByte(2);
			dummyDocket.WD_TotalUnits = 10m;
			Factory.SaveForTesting();

			var docketBOToLoad = GetNewDocket(client, warehouse, "2");
			docketBOToLoad.WD_ExternalReference = "ORDERME1";
			docketBOToLoad.WD_ExternalReferenceSplit = new ZByte(1);
			docketBOToLoad.WD_TotalUnits = 10m;
			Factory.SaveForTesting();

			docketBOToLoad.WD_SystemCreateTimeUtc = docketBOToLoad.WD_SystemCreateTimeUtc.AddDays(1);
			Factory.SaveForTesting();

			ShipmentDataObject.Order.OrderNumber = "ORDERME1";
			ShipmentDataObject.Order.OrderNumberSplit = new ZByte(1);
			ShipmentDataObject.Order.TotalUnits = 11m;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);

			CombineAssertions(() =>
			{
				AssertEquals("whsDocketBO.WD_TotalUnits", 11m, whsDocketBO.WD_TotalUnits);
				AssertEquals("loaded same docket", docketBOToLoad.PK, whsDocketBO.PK);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Updated Warehouse {1} {2} from UniversalShipment.
".Trim(), nameof(WhsDynamicWorkOrder), GetDocketType(), docketBOToLoad.WD_DocketID), Logger.Logs);
			});
		}

		public void TestDocketsOfDifferentTypes()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var whsDocketOfDifferentType = GetNewDocketOfDifferentType();
			AssertNotEquals("Docket to test against should be of a different type", typeof(WhsDynamicWorkOrder), whsDocketOfDifferentType.GetType());

			whsDocketOfDifferentType.WD_OH_Client = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "CRAHOLSYD")).PK;
			whsDocketOfDifferentType.WD_ExternalReference = "CUSTOMER";

			Factory.SaveForTesting();

			ShipmentDataObject.Order.OrderNumber = "CUSTOMER";

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);

			CombineAssertions(() =>
			{
				AssertNotEquals("Created a new docket", whsDocketOfDifferentType.PK, whsDocketBO.PK);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Warehouse {1} from UniversalShipment.
".Trim(), nameof(WhsDynamicWorkOrder), GetDocketType()), Logger.Logs);
			});
		}

		public void TestDoesNotMatchExistingDocketWithOrderNumberOnly()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			var nonMatchingClientDocketBO = GetNewDocket(new WhsTestHelperFunctions(Factory.BOFactory).CreateClient(), warehouse);
			nonMatchingClientDocketBO.WD_DocketID = "W00000002";
			nonMatchingClientDocketBO.WD_ExternalReference = "ORDERME";
			nonMatchingClientDocketBO.WD_ExternalReferenceSplit = new ZByte(2);
			nonMatchingClientDocketBO.WD_TotalUnits = 10m;

			Factory.SaveForTesting();

			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = new ZByte(2);
			ShipmentDataObject.Order.TotalUnits = 11m;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var newDynamicWorkOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(newDynamicWorkOrderBO);

			CombineAssertions(() =>
			{
				AssertEquals("whsDocketBO.WD_TotalUnits", 11m, newDynamicWorkOrderBO.WD_TotalUnits);
				AssertNotEquals("Does not match to dynamic work order with different client", nonMatchingClientDocketBO.PK, newDynamicWorkOrderBO.PK);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Warehouse {1} from UniversalShipment.
".Trim(), nameof(WhsDynamicWorkOrder), GetDocketType()), Logger.Logs);
			});
		}

		public void TestMatchingExternalReferenceConsiderSplitNo()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.Order.OrderNumber = "ExternalRef";
			ShipmentDataObject.Order.OrderNumberSplit = 0;
			ShipmentDataObject.Order.TotalUnits = 5m;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var dynamicWorkOrder = reader.ReadIntoBusinessObject();
			AssertNotNull(dynamicWorkOrder);
			CombineAssertions(() =>
			{
				AssertEquals("whsDocket0.WD_ExternalReferenceSplit", (byte)0, dynamicWorkOrder.WD_ExternalReferenceSplit);
				AssertEquals("whsDocket0.WD_TotalUnits", 5m, dynamicWorkOrder.WD_TotalUnits);
			});

			ShipmentDataObject.Order.TotalUnits = 15m;
			reader = GetNewReader(ShipmentDataObject, Logger);
			var dynamicWorkOrderUpdated = reader.ReadIntoBusinessObject();
			AssertNotNull(dynamicWorkOrderUpdated);
			CombineAssertions(() =>
			{
				AssertEquals("whsDocket0_updated.WD_ExternalReferenceSplit", (byte)0, dynamicWorkOrderUpdated.WD_ExternalReferenceSplit);
				AssertEquals("whsDocket0_updated.WD_TotalUnits", 15m, dynamicWorkOrderUpdated.WD_TotalUnits);
				AssertEquals("Should find whsDocket0 and updated.", dynamicWorkOrder.PK, dynamicWorkOrderUpdated.PK);
			});

			ShipmentDataObject.Order.OrderNumberSplit = 1;
			reader = GetNewReader(ShipmentDataObject, Logger);
			var newDynamicWorkOrder = reader.ReadIntoBusinessObject();
			AssertNotNull(newDynamicWorkOrder);
			CombineAssertions(() =>
			{
				AssertEquals("whsDocket1.WD_ExternalReferenceSplit", (byte)1, newDynamicWorkOrder.WD_ExternalReferenceSplit);
				AssertEquals("whsDocket1.WD_TotalUnits", 15m, newDynamicWorkOrder.WD_TotalUnits);
				AssertNotEquals("Should not find existing docket with split no 0.", dynamicWorkOrder.PK, newDynamicWorkOrder.PK);
			});

			AssertEquals("Matches based on split no.", newDynamicWorkOrder, GetNewReader(ShipmentDataObject, Logger).ReadIntoBusinessObject());
		}

		public void TestMatchingExistingDocketThroughClientAddressPlusOrderNumberPlusSplitNumber()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			var dummyDynamicWorkOrder = GetNewDocket(new WhsTestHelperFunctions(Factory.BOFactory).CreateClient(), warehouse);
			dummyDynamicWorkOrder.WD_ExternalReference = "ORDERME";
			dummyDynamicWorkOrder.WD_ExternalReferenceSplit = new ZByte(2);
			Factory.SaveForTesting();

			var dynamicWorkOrderToMatch = GetNewDocket(client, warehouse);
			dynamicWorkOrderToMatch.WD_ExternalReference = "ORDERME";
			dynamicWorkOrderToMatch.WD_ExternalReferenceSplit = new ZByte(2);
			Factory.SaveForTesting();

			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.TotalUnits = 11m;
			ShipmentDataObject.Order.OrderNumberSplit = new ZByte(2);

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var dynamicWorkOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(dynamicWorkOrderBO);

			CombineAssertions(() =>
			{
				AssertEquals("whsDocketBO.WD_TotalUnits", 11m, dynamicWorkOrderBO.WD_TotalUnits);
				AssertEquals("loaded same docket", dynamicWorkOrderToMatch.PK, dynamicWorkOrderBO.PK);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Updated Warehouse {1} {2} from UniversalShipment.
".Trim(), nameof(WhsDynamicWorkOrder), GetDocketType(), dynamicWorkOrderToMatch.WD_DocketID), Logger.Logs);
			});
		}

		public void TestReadIntoBusinessObject_CalculateTotalsIfNeeded_TotalUnits()
		{
			var factory2 = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory2);
			var org = helper.CreateClient();
			var whs = helper.CreateWarehouse("A");
			var mainPart = helper.CreateProduct(org, "MainPart");
			var secPart = helper.CreateProduct(org, "SecPart");
			var componentPart1 = helper.CreateProduct(org, "ComPart1");
			var componentPart2 = helper.CreateProduct(org, "ComPart2");
			helper.SetProductWeightAndVolume(mainPart, 6, "KG", 0.5, "M3");
			helper.SetProductWeightAndVolume(secPart, 10, "KG", 0.3, "M3");
			helper.SetProductWeightAndVolume(componentPart1, 5, "KG", 0.1, "M3");
			helper.SetProductWeightAndVolume(componentPart2, 2, "KG", 0.2, "M3");
			factory2.Save();

			var dynamicWorkOrder = helper.CreateWhsDynamicWorkOrder(org, whs);
			factory2.Save();

			var shipment = new WhsDynamicWorkOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.WDO, dynamicWorkOrder))).GetDataObject(dynamicWorkOrder);
			shipment.TotalVolumeUnit = new UnitOfVolume { Code = "M3" };
			shipment.TotalWeightUnit = new UnitOfWeight { Code = "KG" };
			shipment.Order.TotalLineVolume = null;
			shipment.Order.TotalLineWeight = null;

			var orderLineDataObject1 = new OrderLine();
			orderLineDataObject1.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject1.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject1.OrderedQty = 10m;
			orderLineDataObject1.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject1.Product = new Product { Code = "MAINPART", Description = "Main Part" };
			orderLineDataObject1.LineNumber = 1;
			orderLineDataObject1.SubLineNumber = 1;
			orderLineDataObject1.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject1.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject1.CustomsData.IsSecondaryInwardsProcessedItem = false;

			var component1OrderLine1 = new OrderLine();
			component1OrderLine1.Product = new Product { Code = "COMPART1", Description = "Component Prod 1" };
			component1OrderLine1.LineNumber = new ZShort(2);
			component1OrderLine1.SubLineNumber = new ZShort(4);
			component1OrderLine1.OrderedQty = 10m;
			component1OrderLine1.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var component1CustomsData = new CustomsEntryInfo();
			component1CustomsData.InwardsEntryKey = "ABC";
			component1CustomsData.InwardsEntryLineNumber = new ZShort(2);
			component1OrderLine1.CustomsData = component1CustomsData;

			var component2OrderLine1 = new OrderLine();
			component2OrderLine1.Product = new Product { Code = "COMPART2", Description = "Component Prod 2" };
			component2OrderLine1.LineNumber = new ZShort(3);
			component2OrderLine1.SubLineNumber = new ZShort(4);
			component2OrderLine1.OrderedQty = 10m;
			component2OrderLine1.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var component2CustomsData = new CustomsEntryInfo();
			component2CustomsData.InwardsEntryKey = "ABC";
			component2CustomsData.InwardsEntryLineNumber = new ZShort(2);
			component2OrderLine1.CustomsData = component2CustomsData;
			orderLineDataObject1.SetOrderLineCollection(() => new List<OrderLine>() { component1OrderLine1, component2OrderLine1 });

			var orderLineDataObject2 = new OrderLine();
			orderLineDataObject2.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject2.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject2.OrderedQty = 20m;
			orderLineDataObject2.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject2.Product = new Product { Code = "SECPART", Description = "Secondary Part" };
			orderLineDataObject2.LineNumber = 2;
			orderLineDataObject2.SubLineNumber = 1;
			orderLineDataObject2.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject2.CustomsData.IsMainInwardsProcessedItem = false;
			orderLineDataObject2.CustomsData.IsSecondaryInwardsProcessedItem = true;

			var componentOrderLine2 = new OrderLine();
			componentOrderLine2.Product = new Product { Code = "COMPART1", Description = "Component Prod 1" };
			componentOrderLine2.LineNumber = new ZShort(3);
			componentOrderLine2.SubLineNumber = new ZShort(4);
			componentOrderLine2.OrderedQty = 5m;
			componentOrderLine2.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var componentCustomsData2 = new CustomsEntryInfo();
			componentCustomsData2.InwardsEntryKey = "ABC";
			componentCustomsData2.InwardsEntryLineNumber = new ZShort(2);
			componentOrderLine2.CustomsData = componentCustomsData2;
			orderLineDataObject2.SetOrderLineCollection(() => new List<OrderLine>() { componentOrderLine2 });

			shipment.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1, orderLineDataObject2 });
			shipment.Order.TotalUnits = null;

			var reader1 = GetNewReader(shipment, Logger);
			var whsOrderBO1 = reader1.ReadIntoBusinessObject();
			AssertNotNull(whsOrderBO1);
			AssertEquals(dynamicWorkOrder.PK, whsOrderBO1.PK);
			AssertEquals("Should have calculated total units from parent lines, excluding component lines.", 30m, whsOrderBO1.WD_TotalUnits);

			shipment.Order.TotalUnits = 42;
			var reader2 = GetNewReader(shipment, Logger);
			var whsOrderBO2 = reader2.ReadIntoBusinessObject();
			AssertNotNull(whsOrderBO2);
			AssertEquals(dynamicWorkOrder.PK, whsOrderBO2.PK);
			AssertEquals("Should *not* have calculated total units.", 42m, whsOrderBO2.WD_TotalUnits);
		}

		public void TestReadIntoBusinessObject_CalculateTotalsIfNeeded_WeightAndVolume()
		{
			var factory2 = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory2);
			var org = helper.CreateClient();
			var whs = helper.CreateWarehouse("A");
			var mainPart = helper.CreateProduct(org, "MainPart");
			var secPart = helper.CreateProduct(org, "SecPart");
			var componentPart1 = helper.CreateProduct(org, "ComPart1");
			var componentPart2 = helper.CreateProduct(org, "ComPart2");
			helper.SetProductWeightAndVolume(mainPart, 6, "KG", 0.5, "M3");
			helper.SetProductWeightAndVolume(secPart, 10, "KG", 0.3, "M3");
			helper.SetProductWeightAndVolume(componentPart1, 5, "KG", 0.1, "M3");
			helper.SetProductWeightAndVolume(componentPart2, 2, "KG", 0.2, "M3");
			factory2.Save();

			var dynamicWorkOrder = helper.CreateWhsDynamicWorkOrder(org, whs);
			factory2.Save();

			var shipment = new WhsDynamicWorkOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.WDO, dynamicWorkOrder))).GetDataObject(dynamicWorkOrder);
			shipment.TotalVolumeUnit = new UnitOfVolume { Code = "M3" };
			shipment.TotalWeightUnit = new UnitOfWeight { Code = "KG" };
			shipment.Order.TotalLineVolume = null;
			shipment.Order.TotalLineWeight = null;

			var orderLineDataObject1 = new OrderLine();
			orderLineDataObject1.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject1.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject1.OrderedQty = 10m;
			orderLineDataObject1.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject1.Product = new Product { Code = "MAINPART", Description = "Main Part" };
			orderLineDataObject1.LineNumber = 1;
			orderLineDataObject1.SubLineNumber = 1;
			orderLineDataObject1.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject1.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject1.CustomsData.IsSecondaryInwardsProcessedItem = false;

			var component1OrderLine1 = new OrderLine();
			component1OrderLine1.Product = new Product { Code = "COMPART1", Description = "Component Prod 1" };
			component1OrderLine1.LineNumber = new ZShort(2);
			component1OrderLine1.SubLineNumber = new ZShort(4);
			component1OrderLine1.OrderedQty = 10m;
			component1OrderLine1.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var component1CustomsData = new CustomsEntryInfo();
			component1CustomsData.InwardsEntryKey = "ABC";
			component1CustomsData.InwardsEntryLineNumber = new ZShort(2);
			component1OrderLine1.CustomsData = component1CustomsData;

			var component2OrderLine1 = new OrderLine();
			component2OrderLine1.Product = new Product { Code = "COMPART2", Description = "Component Prod 2" };
			component2OrderLine1.LineNumber = new ZShort(3);
			component2OrderLine1.SubLineNumber = new ZShort(4);
			component2OrderLine1.OrderedQty = 10m;
			component2OrderLine1.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var component2CustomsData = new CustomsEntryInfo();
			component2CustomsData.InwardsEntryKey = "ABC";
			component2CustomsData.InwardsEntryLineNumber = new ZShort(2);
			component2OrderLine1.CustomsData = component2CustomsData;
			orderLineDataObject1.SetOrderLineCollection(() => new List<OrderLine>() { component1OrderLine1, component2OrderLine1 });

			var orderLineDataObject2 = new OrderLine();
			orderLineDataObject2.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject2.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject2.OrderedQty = 20m;
			orderLineDataObject2.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject2.Product = new Product { Code = "SECPART", Description = "Secondary Part" };
			orderLineDataObject2.LineNumber = 2;
			orderLineDataObject2.SubLineNumber = 1;
			orderLineDataObject2.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject2.CustomsData.IsMainInwardsProcessedItem = false;
			orderLineDataObject2.CustomsData.IsSecondaryInwardsProcessedItem = true;

			var componentOrderLine2 = new OrderLine();
			componentOrderLine2.Product = new Product { Code = "COMPART1", Description = "Component Prod 1" };
			componentOrderLine2.LineNumber = new ZShort(3);
			componentOrderLine2.SubLineNumber = new ZShort(4);
			componentOrderLine2.OrderedQty = 5m;
			componentOrderLine2.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var componentCustomsData2 = new CustomsEntryInfo();
			componentCustomsData2.InwardsEntryKey = "ABC";
			componentCustomsData2.InwardsEntryLineNumber = new ZShort(2);
			componentOrderLine2.CustomsData = componentCustomsData2;
			orderLineDataObject2.SetOrderLineCollection(() => new List<OrderLine>() { componentOrderLine2 });

			shipment.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1, orderLineDataObject2 });
			var reader1 = GetNewReader(shipment, Logger);
			var whsOrderBO1 = reader1.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertNotNull(whsOrderBO1);
				AssertEquals(dynamicWorkOrder.PK, whsOrderBO1.PK);
				AssertEquals("Total Cubic should be updated.", 3m, whsOrderBO1.WD_TotalCubic);
				AssertEquals("Total Weight should be updated.", 70m, whsOrderBO1.WD_TotalWeight);
			});

			shipment.TotalVolumeUnit = new UnitOfVolume { Code = "CF" };
			shipment.TotalWeightUnit = new UnitOfWeight { Code = "G" };
			var reader2 = GetNewReader(shipment, Logger);
			var whsOrderBO2 = reader2.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertNotNull(whsOrderBO2);
				AssertEquals(dynamicWorkOrder.PK, whsOrderBO2.PK);
				AssertEquals("Total Cubic should be updated.", 105.943m, whsOrderBO2.WD_TotalCubic);
				AssertEquals("Total Cubic Unit should be updated.", "CF", whsOrderBO2.WD_TotalCubicUnit);
				AssertEquals("Total Weight should be updated.", 70000m, whsOrderBO2.WD_TotalWeight);
				AssertEquals("Total Weight Unit should be updated.", "G", whsOrderBO2.WD_TotalWeightUnit);
			});

			shipment.Order.TotalLineVolume = 10m;
			shipment.Order.TotalLineWeight = 5m;
			var reader3 = GetNewReader(shipment, Logger);
			var whsOrderBO3 = reader3.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertNotNull(whsOrderBO3);
				AssertEquals(dynamicWorkOrder.PK, whsOrderBO3.PK);
				AssertEquals("Should *not* have calculated cubic units.", 10m, whsOrderBO3.WD_TotalCubic);
				AssertEquals("Should *not* have calculated weight units.", 5m, whsOrderBO3.WD_TotalWeight);

				AssertEquals("Total Cubic Unit should *not* be updated.", "CF", whsOrderBO2.WD_TotalCubicUnit);
				AssertEquals("Total Weight Unit should *not* be updated.", "G", whsOrderBO2.WD_TotalWeightUnit);
			});
		}

		public void TestReadIntoBusinessObject_SecondaryComponentLinesMatchedWithMainComponentLine()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			Helper.SetClientAllAttributeType(order.Client, false);
			Helper.SetClientAttributeType(order.Client, AttributeNumber.Serial, true);
			var prod1 = Helper.CreateProduct("PROD1", order.Client);
			var prod2 = Helper.CreateProduct("PROD2", order.Client);
			Helper.SetProductAllAttributeUse(order.Client, prod2, true);
			var prod3 = Helper.CreateProduct("PROD3", order.Client);
			Helper.SetProductAllAttributeUse(order.Client, prod3, true);
			Factory.SaveForTesting();

			var orderLineDataObject1 = new OrderLine();
			orderLineDataObject1.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject1.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject1.OrderedQty = 1m;
			orderLineDataObject1.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject1.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			orderLineDataObject1.LineNumber = 1;
			orderLineDataObject1.SubLineNumber = 1;
			orderLineDataObject1.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject1.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject1.CustomsData.IsSecondaryInwardsProcessedItem = false;

			var component1OrderLine1 = new OrderLine();
			component1OrderLine1.Product = new Product { Code = "PROD2", Description = "Component Prod 1" };
			component1OrderLine1.LineNumber = new ZShort(2);
			component1OrderLine1.SubLineNumber = new ZShort(4);
			component1OrderLine1.OrderedQty = 1m;
			component1OrderLine1.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			component1OrderLine1.PartAttribute1 = "BLUE";
			component1OrderLine1.PartAttribute2 = "LARGE";
			component1OrderLine1.PartAttribute3 = "MALE";
			component1OrderLine1.ExpiryDate = new ZDateTime(2024, 8, 1);
			component1OrderLine1.PackingDate = new ZDateTime(2023, 8, 1);
			component1OrderLine1.SerialNumber = "SERIAL1";

			var component1CustomsData1 = new CustomsEntryInfo();
			component1CustomsData1.InwardsEntryKey = "ABC";
			component1CustomsData1.InwardsEntryLineNumber = new ZShort(2);
			component1OrderLine1.CustomsData = component1CustomsData1;

			var component2OrderLine1 = new OrderLine();
			component2OrderLine1.Product = new Product { Code = "PROD3", Description = "Component Prod 2" };
			component2OrderLine1.LineNumber = new ZShort(3);
			component2OrderLine1.SubLineNumber = new ZShort(4);
			component2OrderLine1.OrderedQty = 1m;
			component2OrderLine1.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			component2OrderLine1.PartAttribute1 = "RED";
			component2OrderLine1.PartAttribute2 = "SMALL";
			component2OrderLine1.PartAttribute3 = "FEMALE";
			component2OrderLine1.ExpiryDate = new ZDateTime(2024, 9, 1);
			component2OrderLine1.PackingDate = new ZDateTime(2023, 7, 1);
			component2OrderLine1.SerialNumber = "SERIAL2";

			var component2CustomsData1 = new CustomsEntryInfo();
			component2CustomsData1.InwardsEntryKey = "ABC";
			component2CustomsData1.InwardsEntryLineNumber = new ZShort(2);
			component2OrderLine1.CustomsData = component2CustomsData1;
			orderLineDataObject1.SetOrderLineCollection(() => new List<OrderLine>() { component1OrderLine1, component2OrderLine1 });

			var orderLineDataObject2 = new OrderLine();
			orderLineDataObject2.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject2.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject2.OrderedQty = 1m;
			orderLineDataObject2.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject2.Product = new Product { Code = "PROD1", Description = "Other Prod" };
			orderLineDataObject2.LineNumber = 2;
			orderLineDataObject2.SubLineNumber = 1;
			orderLineDataObject2.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject2.CustomsData.IsMainInwardsProcessedItem = false;
			orderLineDataObject2.CustomsData.IsSecondaryInwardsProcessedItem = true;

			var componentOrderLine2 = new OrderLine();
			componentOrderLine2.Product = new Product { Code = "PROD2", Description = "Component Prod 1" };
			componentOrderLine2.LineNumber = new ZShort(3);
			componentOrderLine2.SubLineNumber = new ZShort(4);
			componentOrderLine2.OrderedQty = 1m;
			componentOrderLine2.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			componentOrderLine2.PartAttribute1 = "NOT BLUE";
			componentOrderLine2.PartAttribute2 = "NOT LARGE";
			componentOrderLine2.PartAttribute3 = "NOT MALE";
			componentOrderLine2.ExpiryDate = new ZDateTime(2022, 8, 1);
			componentOrderLine2.PackingDate = new ZDateTime(2021, 8, 1);
			componentOrderLine2.SerialNumber = "NOT SERIAL1";

			var componentCustomsData2 = new CustomsEntryInfo();
			componentCustomsData2.InwardsEntryKey = "ABC";
			componentCustomsData2.InwardsEntryLineNumber = new ZShort(3);
			componentOrderLine2.CustomsData = componentCustomsData2;

			var componentOrderLine3 = new OrderLine();
			componentOrderLine3.Product = new Product { Code = "PROD3", Description = "Component Prod 2" };
			componentOrderLine3.LineNumber = new ZShort(4);
			componentOrderLine3.SubLineNumber = new ZShort(5);
			componentOrderLine3.OrderedQty = 1m;
			componentOrderLine3.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject2.SetOrderLineCollection(() => new List<OrderLine>() { componentOrderLine2, componentOrderLine3 });

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1, orderLineDataObject2 });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = 1;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var dynamicWorkOrderBO = reader.ReadIntoBusinessObject();
			AssertNotNull(dynamicWorkOrderBO);

			CombineAssertions(() =>
			{
				var lines = dynamicWorkOrderBO.Lines.Cast<WhsDynamicWorkOrderLine>();
				var primaryLine = lines.Single(line => line.IsMainInwardProcessedItem);
				var componentLine1PrimaryProd = primaryLine.ChildComponentLines.Single(line => line.WE_OP == prod2.PK);

				var secondaryLine = lines.Single(line => line.IsSecondaryInwardProcessedItem);
				var componentLine1SecondaryProd = secondaryLine.ChildComponentLines.Single(line => line.WE_OP == prod2.PK);
				AssertEquals("Component line attributes copied from matching primary component line.", componentLine1SecondaryProd.WE_PartAttrib1, "BLUE");
				AssertEquals("Component line attributes copied from matching primary component line.", componentLine1SecondaryProd.WE_PartAttrib2, "LARGE");
				AssertEquals("Component line attributes copied from matching primary component line.", componentLine1SecondaryProd.WE_PartAttrib3, "MALE");
				AssertEquals("Component line attributes copied from matching primary component line.", componentLine1SecondaryProd.WE_SerialNumber, "SERIAL1");
				AssertEquals("Component line attributes copied from matching primary component line.", componentLine1SecondaryProd.WE_PackingDate, new ZDateTime(2023, 8, 1));
				AssertEquals("Component line attributes copied from matching primary component line.", componentLine1SecondaryProd.WE_ExpiryDate, new ZDateTime(2024, 8, 1));
				AssertEquals("Component line attributes copied from matching primary component line.", componentLine1SecondaryProd.WE_BondedEntryKey, "ABC-2");
				AssertEquals("Matching line is set.", componentLine1SecondaryProd.WE_WE_MatchingLine, componentLine1PrimaryProd.PK);

				var componentLine2PrimaryProd = primaryLine.ChildComponentLines.Single(line => line.WE_OP == prod3.PK);
				var componentLine2SecondaryProd = secondaryLine.ChildComponentLines.Single(line => line.WE_OP == prod3.PK);
				AssertEquals("Component line attributes copied from matching primary component line.", componentLine2SecondaryProd.WE_PartAttrib1, "RED");
				AssertEquals("Component line attributes copied from matching primary component line.", componentLine2SecondaryProd.WE_PartAttrib2, "SMALL");
				AssertEquals("Component line attributes copied from matching primary component line.", componentLine2SecondaryProd.WE_PartAttrib3, "FEMALE");
				AssertEquals("Component line attributes copied from matching primary component line.", componentLine2SecondaryProd.WE_SerialNumber, "SERIAL2");
				AssertEquals("Component line attributes copied from matching primary component line.", componentLine2SecondaryProd.WE_PackingDate, new ZDateTime(2023, 7, 1));
				AssertEquals("Component line attributes copied from matching primary component line.", componentLine2SecondaryProd.WE_ExpiryDate, new ZDateTime(2024, 9, 1));
				AssertEquals("Component line attributes copied from matching primary component line.", componentLine2SecondaryProd.WE_BondedEntryKey, "ABC-2");
				AssertEquals("Matching line is set.", componentLine2SecondaryProd.WE_WE_MatchingLine, componentLine2PrimaryProd.PK);
			});
		}

		#region TestReadIntoBusinessObject_Disassembly

		public void TestReadIntoBusinessObject_Disassembly_NoLines()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = PrepareVirtualWarehouseForImport();

			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);

			var product = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			Helper.SetClientAllAttributeType(order.Client, false);
			Helper.SetProductAllAttributeUse(order.Client, product, true);
			Helper.SetClientAttributeType(order.Client, AttributeNumber.Serial, true);
			Factory.SaveForTesting();

			ShipmentDataObject.Order.Type = new CodeDescriptionPair { Code = DynamicWorkOrderType.Codes.Disassemble };

			var reader = GetNewReader(ShipmentDataObject, Logger);
			AssertExceptionThrown<DataObjectReadFailureException>("Failed to import",
				@"Cannot Import Dynamic Work Order
This Dynamic Work Order has no Lines.",
				() => reader.ReadIntoBusinessObject());
		}

		public void TestReadIntoBusinessObject_Disassembly_WithNoInventoryAllocated()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = PrepareVirtualWarehouseForImport();
			Factory.SaveForTesting();
			AssertEquals("Precondition", true, warehouse.IsInwardProcessingEnabled);

			WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);

			var mainProduct = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			var component = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "HATCOMP"));

			Factory.SaveForTesting();

			var order = ShipmentDataObject.Order;
			order.IsInwardsProcessingJob = true;
			order.AutoFinaliseBOMIntoInventory = true;
			order.Type = new CodeDescriptionPair { Code = DynamicWorkOrderType.Codes.Disassemble };
			order.OrderNumber = "ExtRef";

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject.OrderedQty = 10m;
			orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			orderLineDataObject.LineNumber = 1;
			orderLineDataObject.SubLineNumber = 1;
			orderLineDataObject.CustomsData = new CustomsEntryInfo();
			orderLineDataObject.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject.CustomsData.IsSecondaryInwardsProcessedItem = false;

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });

			var reader = GetNewReader(ShipmentDataObject, Logger);

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				AssertExceptionThrown<DataObjectReadFailureException>("Failed to import",
					"Dynamic Work Order cannot be finalized until some inventory is allocated.",
					() => reader.ReadIntoBusinessObject());
			}
		}

		public void TestReadIntoBusinessObject_Disassembly_WithValidLine()
		{
			TestReadIntoBusinessObject_Disassembly_WithValidLineCore(10m);
		}

		public void TestReadIntoBusinessObject_Disassembly_WithValidLine_PartiallyAllocated()
		{
			TestReadIntoBusinessObject_Disassembly_WithValidLineCore(1000m);
		}

		void TestReadIntoBusinessObject_Disassembly_WithValidLineCore(decimal orderedUnits)
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = PrepareVirtualWarehouseForImport();
			Factory.SaveForTesting();
			AssertEquals("Precondition", true, warehouse.IsInwardProcessingEnabled);

			WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);

			var mainProduct = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			var component = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "HATCOMP"));

			var receive = Helper.CreateWhsReceiveWithInventory(Data.Orgs.CRAHOLSYD, warehouse, "R1", component, 100m, warehouse.DefaultLocationInInwardProcessingArea, "");
			var inventory = receive.Lines.Single();
			inventory.WE_BondedEntryKey = "ABC-2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.SaveForTesting();

			var assemblyWorkOrder = Helper.CreateWhsDynamicWorkOrder(Data.Orgs.CRAHOLSYD, warehouse, "DW1");
			var mainLine1 = Helper.CreateWhsDynamicWorkOrderLine(assemblyWorkOrder, mainProduct, 10m);
			mainLine1.IsMainInwardProcessedItem = true;

			var childLine1 = assemblyWorkOrder.Lines.AddNew();
			childLine1.WE_OP = component.PK;
			childLine1.WE_TransactionQuantity = 20m;
			childLine1.WE_WE_ParentDocketLine = mainLine1.PK;

			var pick = Factory.New<WhsPick>();
			pick.WP_PickType = PickType.Codes.DynamicWorkOrder;
			pick.WP_PickNo = "P0000001";
			Factory.SaveForTesting();

			pick.AddOrders(new[] { assemblyWorkOrder });
			pick.AutoAllocateItemsWithMock();

			assemblyWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(assemblyWorkOrder);
			AssertIsFinalisedPrecondition(assemblyWorkOrder.Receive);

			Factory.SaveForTesting();

			var order = ShipmentDataObject.Order;
			order.IsInwardsProcessingJob = true;
			order.AutoFinaliseBOMIntoInventory = true;
			order.Type = new CodeDescriptionPair { Code = DynamicWorkOrderType.Codes.Disassemble };
			order.OrderNumber = "ExtRef";

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject.OrderedQty = orderedUnits;
			orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			orderLineDataObject.LineNumber = 1;
			orderLineDataObject.SubLineNumber = 1;
			orderLineDataObject.CustomsData = new CustomsEntryInfo();
			orderLineDataObject.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject.CustomsData.IsSecondaryInwardsProcessedItem = false;

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });

			var reader = GetNewReader(ShipmentDataObject, Logger);

			WhsDynamicWorkOrder whsOrderBO;
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				whsOrderBO = reader.ReadIntoBusinessObject();
			}

			AssertNotNull(whsOrderBO);
			AssertEquals("Set correct docket subtype.", DynamicWorkOrderType.Codes.Disassemble, whsOrderBO.WD_DocketSubType);
			AssertEquals("whsOrderBO.Lines.Count", 1, whsOrderBO.Lines.Count);

			AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsDynamicWorkOrder found, creating new WhsDynamicWorkOrder.
Information - Populating WhsDynamicWorkOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsDynamicWorkOrderLine found, creating new WhsDynamicWorkOrderLine.
Information - Populating WhsDynamicWorkOrderLine...
Information - Added Warehouse Dynamic Work Order from UniversalShipment.
".Trim(), Logger.Logs);

			AssertIsFinalisedPrecondition(whsOrderBO);

			var newReceiveLine = whsOrderBO.Receive.Lines.SingleOrDefault();
			AssertNotNull("ReceiveLine should have been created.", newReceiveLine);
			AssertEquals(component.PK, newReceiveLine.WE_OP);
			AssertEquals(20m, newReceiveLine.WE_TransactionQuantity);
			AssertEquals("ABC-2", newReceiveLine.WE_BondedEntryKey);
			AssertEquals(warehouse.DefaultLocationInInwardProcessingArea.PK, newReceiveLine.WE_WL);
		}

		public void TestReadIntoBusinessObject_Disassembly_RejectsComponentLines()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			order.WD_DocketSubType = DynamicWorkOrderType.Codes.Disassemble;

			var product = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			Helper.SetClientAllAttributeType(order.Client, false);
			Helper.SetProductAllAttributeUse(order.Client, product, true);
			Helper.SetClientAttributeType(order.Client, AttributeNumber.Serial, true);
			Factory.SaveForTesting();

			var orderLineDataObject = WhsDynamicWorkOrderLineDataObjectReaderTest.SetupStandardOrderLine(addComponentLines: true);
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });

			ShipmentDataObject.Order.Type = new CodeDescriptionPair { Code = DynamicWorkOrderType.Codes.Disassemble };

			var reader = GetNewReader(ShipmentDataObject, Logger);
			AssertExceptionThrown<DataObjectReadFailureException>("Failed to import",
				"Component lines cannot be imported for Revert Assembly Dynamic Work Orders.",
				() => reader.ReadIntoBusinessObject());
		}

		public void TestReadIntoBusinessObject_Disassembly_RejectsSecondaryMainProcessingLine()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			order.WD_DocketSubType = DynamicWorkOrderType.Codes.Disassemble;

			var product = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			Helper.SetClientAllAttributeType(order.Client, false);
			Helper.SetProductAllAttributeUse(order.Client, product, true);
			Helper.SetClientAttributeType(order.Client, AttributeNumber.Serial, true);
			Factory.SaveForTesting();

			var orderLineDataObject = WhsDynamicWorkOrderLineDataObjectReaderTest.SetupStandardOrderLine(addComponentLines: false);
			orderLineDataObject.CustomsData.IsMainInwardsProcessedItem = false;
			orderLineDataObject.CustomsData.IsSecondaryInwardsProcessedItem = true;

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });

			ShipmentDataObject.Order.Type = new CodeDescriptionPair { Code = DynamicWorkOrderType.Codes.Disassemble };

			var reader = GetNewReader(ShipmentDataObject, Logger);
			AssertExceptionThrown<DataObjectReadFailureException>("Failed to import",
				"'Is Secondary Inward Processed Item' lines cannot be imported for Revert Assembly Dynamic Work Orders.",
				() => reader.ReadIntoBusinessObject());
		}

		public void TestReadIntoBusinessObject_Disassembly_CalculateTotalsIfNeeded()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = PrepareVirtualWarehouseForImport();
			Factory.SaveForTesting();
			AssertEquals("Precondition", true, warehouse.IsInwardProcessingEnabled);

			WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);

			var mainProduct = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			var component = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "HATCOMP"));

			Helper.SetProductWeightAndVolume(mainProduct, 30m, "KG", 2m, "M3");

			var receive = Helper.CreateWhsReceiveWithInventory(Data.Orgs.CRAHOLSYD, warehouse, "R1", component, 100m, warehouse.DefaultLocationInInwardProcessingArea, "");
			var inventory = receive.Lines.Single();
			inventory.WE_BondedEntryKey = "ABC-2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.SaveForTesting();

			var assemblyWorkOrder = Helper.CreateWhsDynamicWorkOrder(Data.Orgs.CRAHOLSYD, warehouse, "DW1");
			var mainLine1 = Helper.CreateWhsDynamicWorkOrderLine(assemblyWorkOrder, mainProduct, 10m);
			mainLine1.IsMainInwardProcessedItem = true;

			var childLine1 = assemblyWorkOrder.Lines.AddNew();
			childLine1.WE_OP = component.PK;
			childLine1.WE_TransactionQuantity = 20m;
			childLine1.WE_WE_ParentDocketLine = mainLine1.PK;

			var pick = Factory.New<WhsPick>();
			pick.WP_PickType = PickType.Codes.DynamicWorkOrder;
			pick.WP_PickNo = "P0000001";
			Factory.SaveForTesting();

			pick.AddOrders(new[] { assemblyWorkOrder });
			pick.AutoAllocateItemsWithMock();

			assemblyWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(assemblyWorkOrder);
			AssertIsFinalisedPrecondition(assemblyWorkOrder.Receive);

			Factory.SaveForTesting();

			var order = ShipmentDataObject.Order;
			order.IsInwardsProcessingJob = true;
			order.AutoFinaliseBOMIntoInventory = true;
			order.Type = new CodeDescriptionPair { Code = DynamicWorkOrderType.Codes.Disassemble };
			order.OrderNumber = "ExtRef";

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject.OrderedQty = 10m;
			orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			orderLineDataObject.LineNumber = 1;
			orderLineDataObject.SubLineNumber = 1;
			orderLineDataObject.CustomsData = new CustomsEntryInfo();
			orderLineDataObject.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject.CustomsData.IsSecondaryInwardsProcessedItem = false;

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });

			var reader = GetNewReader(ShipmentDataObject, Logger);

			WhsDynamicWorkOrder whsOrderBO;
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				whsOrderBO = reader.ReadIntoBusinessObject();
			}

			AssertNotNull(whsOrderBO);
			AssertIsFinalisedPrecondition(whsOrderBO);
			AssertEquals("Set correct Docket Total Weight.", 300m, whsOrderBO.WD_TotalWeight);
			AssertEquals("Set correct Docket Total Cubic.", 20m, whsOrderBO.WD_TotalCubic);
			AssertEquals("Set correct Docket Total Units.", 20m, whsOrderBO.WD_TotalUnits);
			AssertEquals("whsOrderBO.Lines.Count", 1, whsOrderBO.Lines.Count);

			AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsDynamicWorkOrder found, creating new WhsDynamicWorkOrder.
Information - Populating WhsDynamicWorkOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsDynamicWorkOrderLine found, creating new WhsDynamicWorkOrderLine.
Information - Populating WhsDynamicWorkOrderLine...
Information - Added Warehouse Dynamic Work Order from UniversalShipment.
".Trim(), Logger.Logs);
		}

		public void TestReadIntoBusinessObject_Disassembly_HonourSuppliedTotals()
		{
			TestReadIntoBusinessObject_Disassembly_HonourSuppliedTotalsCore(false);
		}

		public void TestReadIntoBusinessObject_Disassembly_HonourSuppliedTotals_TotalUnitValidation()
		{
			TestReadIntoBusinessObject_Disassembly_HonourSuppliedTotalsCore(true);
		}

		public void TestReadIntoBusinessObject_Disassembly_HonourSuppliedTotalsCore(bool isTotalUnitsValidationEnabled)
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = PrepareVirtualWarehouseForImport();
			Factory.SaveForTesting();
			AssertEquals("Precondition", true, warehouse.IsInwardProcessingEnabled);

			WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);

			var mainProduct = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			var component = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "HATCOMP"));

			Helper.SetProductWeightAndVolume(mainProduct, 30m, "KG", 2m, "M3");

			var receive = Helper.CreateWhsReceiveWithInventory(Data.Orgs.CRAHOLSYD, warehouse, "R1", component, 100m, warehouse.DefaultLocationInInwardProcessingArea, "");
			var inventory = receive.Lines.Single();
			inventory.WE_BondedEntryKey = "ABC-2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.SaveForTesting();

			var assemblyWorkOrder = Helper.CreateWhsDynamicWorkOrder(Data.Orgs.CRAHOLSYD, warehouse, "DW1");
			var mainLine1 = Helper.CreateWhsDynamicWorkOrderLine(assemblyWorkOrder, mainProduct, 10m);
			mainLine1.IsMainInwardProcessedItem = true;

			var childLine1 = assemblyWorkOrder.Lines.AddNew();
			childLine1.WE_OP = component.PK;
			childLine1.WE_TransactionQuantity = 20m;
			childLine1.WE_WE_ParentDocketLine = mainLine1.PK;

			var pick = Factory.New<WhsPick>();
			pick.WP_PickType = PickType.Codes.DynamicWorkOrder;
			pick.WP_PickNo = "P0000001";
			Factory.SaveForTesting();

			pick.AddOrders(new[] { assemblyWorkOrder });
			pick.AutoAllocateItemsWithMock();

			assemblyWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(assemblyWorkOrder);
			AssertIsFinalisedPrecondition(assemblyWorkOrder.Receive);

			Factory.SaveForTesting();

			var order = ShipmentDataObject.Order;
			order.IsInwardsProcessingJob = true;
			order.AutoFinaliseBOMIntoInventory = true;
			order.Type = new CodeDescriptionPair { Code = DynamicWorkOrderType.Codes.Disassemble };
			order.OrderNumber = "ExtRef";

			order.TotalUnits = 5m;
			order.TotalLineWeight = 25m;
			order.TotalLineVolume = 50m;

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject.OrderedQty = 10m;
			orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			orderLineDataObject.LineNumber = 1;
			orderLineDataObject.SubLineNumber = 1;
			orderLineDataObject.CustomsData = new CustomsEntryInfo();
			orderLineDataObject.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject.CustomsData.IsSecondaryInwardsProcessedItem = false;

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });

			var reader = GetNewReader(ShipmentDataObject, Logger);

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, isTotalUnitsValidationEnabled))
			{
				if (isTotalUnitsValidationEnabled)
				{
					AssertExceptionThrown<DataObjectReadFailureException>("Failed to import",
						@"Cannot Import Dynamic Work Order
Dynamic Work Order could not be finalized into the Warehouse for Warehouse Dynamic Work Order because of the following error(s):
Error: Finalise
Error - WD_TotalUnits: Total Units 5 does not equal the total of all components: 20.",
						() => new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());
				}
				else
				{
					var whsOrderBO = reader.ReadIntoBusinessObject();

					AssertNotNull(whsOrderBO);
					AssertIsFinalisedPrecondition(whsOrderBO);
					AssertEquals("Set correct Docket Total Weight.", 25m, whsOrderBO.WD_TotalWeight);
					AssertEquals("Set correct Docket Total Cubic.", 50m, whsOrderBO.WD_TotalCubic);
					AssertEquals("Set correct Docket Total Units.", 5m, whsOrderBO.WD_TotalUnits);
					AssertEquals("whsOrderBO.Lines.Count", 1, whsOrderBO.Lines.Count);

					AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsDynamicWorkOrder found, creating new WhsDynamicWorkOrder.
Information - Populating WhsDynamicWorkOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsDynamicWorkOrderLine found, creating new WhsDynamicWorkOrderLine.
Information - Populating WhsDynamicWorkOrderLine...
Information - Added Warehouse Dynamic Work Order from UniversalShipment.
".Trim(), Logger.Logs);
				}
			}
		}

		WhsWarehouse PrepareVirtualWarehouseForImport()
		{
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			var area = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			return warehouse;
		}

		#endregion

		#endregion

		#region TestUpdatingRestrictedFields

		public void TestUpdatingRestrictedFields_IsInwardsProcessing()
		{
			TestUpdatingRestrictedFieldsCore(@"Cannot Import Dynamic Work Order
Dynamic Work Orders must be an Inward Processing Jobs.", (order) => order.IsInwardsProcessingJob = false);
		}

		public void TestUpdatingRestrictedFields_DocketType()
		{
			TestUpdatingRestrictedFieldsCore(@"Cannot Import Dynamic Work Order
Cannot assign an invalid docket sub type for Dynamic Work Orders.", (order) => order.Type = new CodeDescriptionPair { Code = "ABC" });
		}

		public void TestUpdatingRestrictedFields_AutoFinaliseInventory()
		{
			TestUpdatingRestrictedFieldsCore(@"Cannot Import Dynamic Work Order
Dynamic Work Orders must always auto finalize bill of materials into inventory.", (order) => order.AutoFinaliseBOMIntoInventory = false);
		}

		void TestUpdatingRestrictedFieldsCore(string expectedExceptionMessage, Action<Order> updateRestrictedField)
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			var dynamicWorkOrderToMatch = GetNewDocket(client, warehouse);
			dynamicWorkOrderToMatch.WD_ExternalReference = "ORDERME";
			dynamicWorkOrderToMatch.WD_ExternalReferenceSplit = new ZByte(2);
			Factory.SaveForTesting();

			AssertEquals("Precondition", true, dynamicWorkOrderToMatch.WD_AutoFinaliseBOMIntoInventory);

			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = new ZByte(2);
			updateRestrictedField(ShipmentDataObject.Order);

			AssertExceptionThrown<DataObjectReadFailureException>("Failed to import",
				expectedExceptionMessage,
				() => new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestLinesValidationAfterPopulate

		public void TestLinesValidationAfterPopulate_MainProductCannotBeBOM()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var mainProduct = Helper.CreateProduct("PROD1", order.Client);
			var subProduct = Helper.CreateProduct("PROD2", order.Client);
			Helper.CreateProductBOM(mainProduct, subProduct, 2m, "UNT");
			Factory.SaveForTesting();

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject.OrderedQty = 28m;
			orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject.Product = new Product { Code = "PROD1", Description = "I AM BOM" };
			orderLineDataObject.LineNumber = 1;
			orderLineDataObject.SubLineNumber = 1;
			orderLineDataObject.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject.CustomsData.IsSecondaryInwardsProcessedItem = false;

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = 1;

			AssertExceptionThrown<DataObjectReadFailureException>("Failed to import",
				@"Cannot Import Dynamic Work Order
Main or Secondary Product cannot be a Bill of Materials.",
				() => new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestLinesValidationAfterPopulate_SecondaryProductCannotBeBOM()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var mainProduct = Helper.CreateProduct("PROD1", order.Client);
			var subProduct = Helper.CreateProduct("PROD2", order.Client);
			Helper.CreateProductBOM(mainProduct, subProduct, 2m, "UNT");
			Factory.SaveForTesting();

			var orderLineDataObject1 = new OrderLine();
			orderLineDataObject1.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject1.OrderedQty = 28m;
			orderLineDataObject1.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject1.Product = new Product { Code = "PROD2", Description = "I AM BOM" };
			orderLineDataObject1.LineNumber = 1;
			orderLineDataObject1.SubLineNumber = 1;
			orderLineDataObject1.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject1.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject1.CustomsData.IsSecondaryInwardsProcessedItem = false;

			var orderLineDataObject2 = new OrderLine();
			orderLineDataObject2.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject2.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject2.OrderedQty = 40m;
			orderLineDataObject2.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject2.Product = new Product { Code = "PROD1", Description = "SECOND PROD" };
			orderLineDataObject2.LineNumber = 2;
			orderLineDataObject2.SubLineNumber = 1;
			orderLineDataObject2.CustomsData.IsMainInwardsProcessedItem = false;
			orderLineDataObject2.CustomsData.IsSecondaryInwardsProcessedItem = true;

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1, orderLineDataObject2 });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = 1;

			AssertExceptionThrown<DataObjectReadFailureException>("Failed to import",
				@"Cannot Import Dynamic Work Order
Main or Secondary Product cannot be a Bill of Materials.",
				() => new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestLinesValidationAfterPopulate_DuplicateComponentLines()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var otherProduct = Helper.CreateProduct("PROD2", order.Client);
			Factory.SaveForTesting();

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject.OrderedQty = 28m;
			orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			orderLineDataObject.LineNumber = 1;
			orderLineDataObject.SubLineNumber = 1;
			orderLineDataObject.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject.CustomsData.IsMainInwardsProcessedItem = true;

			var componentOrderLine1 = new OrderLine();
			componentOrderLine1.Product = new Product { Code = "PROD2", Description = "Other Prod" };
			componentOrderLine1.LineNumber = new ZShort(2);
			componentOrderLine1.SubLineNumber = new ZShort(4);
			componentOrderLine1.OrderedQty = orderLineDataObject.OrderedQty;
			componentOrderLine1.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var componentOrderLine2 = new OrderLine();
			componentOrderLine2.Product = new Product { Code = "PROD2", Description = "Other Prod" };
			componentOrderLine2.LineNumber = new ZShort(3);
			componentOrderLine2.SubLineNumber = new ZShort(4);
			componentOrderLine2.OrderedQty = orderLineDataObject.OrderedQty;
			componentOrderLine2.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			orderLineDataObject.SetOrderLineCollection(() => new List<OrderLine>() { componentOrderLine1, componentOrderLine2 });

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = 1;

			AssertExceptionThrown<DataObjectReadFailureException>("Failed to import",
				@"Cannot Import Dynamic Work Order
Each component line must have a unique product.",
				() => new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestLinesValidationAfterPopulate_NoMainProductLine()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var otherProduct = Helper.CreateProduct("PROD2", order.Client);
			Factory.SaveForTesting();

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject.OrderedQty = 28m;
			orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject.Product = new Product { Code = "PROD2", Description = "PROD 2" };
			orderLineDataObject.LineNumber = 1;
			orderLineDataObject.SubLineNumber = 1;
			orderLineDataObject.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject.CustomsData.IsMainInwardsProcessedItem = false;
			orderLineDataObject.CustomsData.IsSecondaryInwardsProcessedItem = true;

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = 1;

			AssertExceptionThrown<DataObjectReadFailureException>("Failed to import",
				@"Cannot Import Dynamic Work Order
Should have at least one 'Is Main Processed Item' per Dynamic Work Order.",
				() => new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestLinesValidationAfterPopulate_MultipleMainProductLine()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var mainProduct = Helper.CreateProduct("PROD1", order.Client);
			var otherProduct = Helper.CreateProduct("PROD2", order.Client);
			Factory.SaveForTesting();

			var orderLineDataObject1 = new OrderLine();
			orderLineDataObject1.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject1.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject1.OrderedQty = 28m;
			orderLineDataObject1.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject1.Product = new Product { Code = "PROD1", Description = "PROD 1" };
			orderLineDataObject1.LineNumber = 1;
			orderLineDataObject1.SubLineNumber = 1;
			orderLineDataObject1.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject1.CustomsData.IsMainInwardsProcessedItem = true;

			var orderLineDataObject2 = new OrderLine();
			orderLineDataObject2.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject2.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject2.OrderedQty = 28m;
			orderLineDataObject2.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject2.Product = new Product { Code = "PROD2", Description = "PROD 2" };
			orderLineDataObject2.LineNumber = 2;
			orderLineDataObject2.SubLineNumber = 1;
			orderLineDataObject2.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject2.CustomsData.IsMainInwardsProcessedItem = true;

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1, orderLineDataObject2 });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = 1;

			AssertExceptionThrown<DataObjectReadFailureException>("Failed to import",
				@"Cannot Import Dynamic Work Order
Should have only one 'Is Main Processed Item' per Dynamic Work Order.",
				() => new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestLinesValidationAfterPopulate_SecondaryComponentLinesTotalQtyMoreThanMainComponentLinesTotalQty()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var otherProduct = Helper.CreateProduct("PROD1", order.Client);
			var componentProduct = Helper.CreateProduct("PROD2", order.Client);
			Factory.SaveForTesting();

			var orderLineDataObject1 = new OrderLine();
			orderLineDataObject1.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject1.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject1.OrderedQty = 28m;
			orderLineDataObject1.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject1.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			orderLineDataObject1.LineNumber = 1;
			orderLineDataObject1.SubLineNumber = 1;
			orderLineDataObject1.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject1.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject1.CustomsData.IsSecondaryInwardsProcessedItem = false;

			var componentOrderLine1 = new OrderLine();
			componentOrderLine1.Product = new Product { Code = "PROD2", Description = "Component Prod" };
			componentOrderLine1.LineNumber = new ZShort(2);
			componentOrderLine1.SubLineNumber = new ZShort(4);
			componentOrderLine1.OrderedQty = 28m;
			componentOrderLine1.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var componentCustomsData1 = new CustomsEntryInfo();
			componentCustomsData1.InwardsEntryKey = "ABC";
			componentCustomsData1.InwardsEntryLineNumber = new ZShort(2);
			componentOrderLine1.CustomsData = componentCustomsData1;
			orderLineDataObject1.SetOrderLineCollection(() => new List<OrderLine>() { componentOrderLine1 });

			var orderLineDataObject2 = new OrderLine();
			orderLineDataObject2.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject2.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject2.OrderedQty = 28m;
			orderLineDataObject2.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject2.Product = new Product { Code = "PROD1", Description = "Other Prod" };
			orderLineDataObject2.LineNumber = 2;
			orderLineDataObject2.SubLineNumber = 1;
			orderLineDataObject2.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject2.CustomsData.IsMainInwardsProcessedItem = false;
			orderLineDataObject2.CustomsData.IsSecondaryInwardsProcessedItem = true;

			var componentOrderLine2 = new OrderLine();
			componentOrderLine2.Product = new Product { Code = "PROD2", Description = "Component Prod" };
			componentOrderLine2.LineNumber = new ZShort(3);
			componentOrderLine2.SubLineNumber = new ZShort(4);
			componentOrderLine2.OrderedQty = 56m;
			componentOrderLine2.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var componentCustomsData2 = new CustomsEntryInfo();
			componentCustomsData2.InwardsEntryKey = "ABC";
			componentCustomsData2.InwardsEntryLineNumber = new ZShort(2);
			componentOrderLine2.CustomsData = componentCustomsData2;
			orderLineDataObject2.SetOrderLineCollection(() => new List<OrderLine>() { componentOrderLine2 });

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1, orderLineDataObject2 });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = 1;

			AssertExceptionThrown<DataObjectReadFailureException>("Failed to import",
				@"Cannot Import Dynamic Work Order
The sum of a component across all secondary products must be less than or equal to the quantity of that component on the main product.
The sum of a component across all secondary products must be less than or equal to the quantity of that component on the main product.",
				() => new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestLinesValidationAfterPopulate_NoInwardProcessingTypeIsSet()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			Helper.CreateProduct("PROD1", order.Client);
			Helper.CreateProduct("PROD2", order.Client);
			Factory.SaveForTesting();

			var orderLineDataObject1 = new OrderLine();
			orderLineDataObject1.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject1.OrderedQty = 28m;
			orderLineDataObject1.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject1.Product = new Product { Code = "PROD1", Description = "OTHER PROD" };
			orderLineDataObject1.LineNumber = 1;
			orderLineDataObject1.SubLineNumber = 1;
			orderLineDataObject1.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject1.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject1.CustomsData.IsSecondaryInwardsProcessedItem = false;

			var orderLineDataObject2 = new OrderLine();
			orderLineDataObject2.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject2.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject2.OrderedQty = 40m;
			orderLineDataObject2.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject2.Product = new Product { Code = "PROD2", Description = "SECOND PROD" };
			orderLineDataObject2.LineNumber = 2;
			orderLineDataObject2.SubLineNumber = 1;
			orderLineDataObject2.CustomsData.IsMainInwardsProcessedItem = false;
			orderLineDataObject2.CustomsData.IsSecondaryInwardsProcessedItem = false;

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1, orderLineDataObject2 });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = 1;

			AssertExceptionThrown<DataObjectReadFailureException>("Failed to import",
				@"Cannot Import Dynamic Work Order
Every line on Dynamic Work Order must have either 'Is Main Inward Processed Item' or 'Is Secondary Inward Processed Item' set.",
				() => new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestLinesValidationAfterPopulate_ZeroQuantity()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			Helper.CreateProduct("PROD1", order.Client);
			Factory.SaveForTesting();

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject.Product = new Product { Code = "PROD1", Description = "OTHER PROD" };
			orderLineDataObject.LineNumber = 1;
			orderLineDataObject.SubLineNumber = 1;
			orderLineDataObject.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject.CustomsData.IsSecondaryInwardsProcessedItem = false;

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = 1;

			AssertExceptionThrown<DataObjectReadFailureException>("Failed to import",
				@"Cannot Import Dynamic Work Order
Quantity must be greater than zero.",
				() => new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestLinesValidationAfterPopulate_MainProductDecimalQuantity()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			Helper.CreateProduct("PROD1", order.Client);
			Factory.SaveForTesting();

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject.Product = new Product { Code = "PROD1", Description = "OTHER PROD" };
			orderLineDataObject.OrderedQty = 1.5m;
			orderLineDataObject.LineNumber = 1;
			orderLineDataObject.SubLineNumber = 1;
			orderLineDataObject.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject.CustomsData.IsSecondaryInwardsProcessedItem = false;

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = 1;

			AssertExceptionThrown<DataObjectReadFailureException>("Failed to import",
				@"Cannot Import Dynamic Work Order
Quantity of main product must be an integer.",
				() => new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestLinesValidationAfterPopulate_SecondaryComponentLineProductNotMatchedToPrimaryComponentProduct()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			Helper.CreateProduct("PROD1", order.Client);
			Helper.CreateProduct("PROD2", order.Client);
			Helper.CreateProduct("PROD3", order.Client);
			Factory.SaveForTesting();

			var orderLineDataObject1 = new OrderLine();
			orderLineDataObject1.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject1.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject1.OrderedQty = 28m;
			orderLineDataObject1.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject1.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			orderLineDataObject1.LineNumber = 1;
			orderLineDataObject1.SubLineNumber = 1;
			orderLineDataObject1.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject1.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject1.CustomsData.IsSecondaryInwardsProcessedItem = false;

			var componentOrderLine1 = new OrderLine();
			componentOrderLine1.Product = new Product { Code = "PROD2", Description = "Component Prod" };
			componentOrderLine1.LineNumber = new ZShort(2);
			componentOrderLine1.SubLineNumber = new ZShort(4);
			componentOrderLine1.OrderedQty = 28m;
			componentOrderLine1.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var componentCustomsData1 = new CustomsEntryInfo();
			componentCustomsData1.InwardsEntryKey = "ABC";
			componentCustomsData1.InwardsEntryLineNumber = new ZShort(2);
			componentOrderLine1.CustomsData = componentCustomsData1;
			orderLineDataObject1.SetOrderLineCollection(() => new List<OrderLine>() { componentOrderLine1 });

			var orderLineDataObject2 = new OrderLine();
			orderLineDataObject2.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject2.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject2.OrderedQty = 28m;
			orderLineDataObject2.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject2.Product = new Product { Code = "PROD1", Description = "Other Prod" };
			orderLineDataObject2.LineNumber = 2;
			orderLineDataObject2.SubLineNumber = 1;
			orderLineDataObject2.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject2.CustomsData.IsMainInwardsProcessedItem = false;
			orderLineDataObject2.CustomsData.IsSecondaryInwardsProcessedItem = true;

			var componentOrderLine2 = new OrderLine();
			componentOrderLine2.Product = new Product { Code = "PROD2", Description = "Component Prod" };
			componentOrderLine2.LineNumber = new ZShort(3);
			componentOrderLine2.SubLineNumber = new ZShort(4);
			componentOrderLine2.OrderedQty = 14m;
			componentOrderLine2.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var componentCustomsData2 = new CustomsEntryInfo();
			componentCustomsData2.InwardsEntryKey = "ABC";
			componentCustomsData2.InwardsEntryLineNumber = new ZShort(2);
			componentOrderLine2.CustomsData = componentCustomsData2;

			var componentOrderLine3 = new OrderLine();
			componentOrderLine3.Product = new Product { Code = "PROD3", Description = "Component Prod" };
			componentOrderLine3.LineNumber = new ZShort(4);
			componentOrderLine3.SubLineNumber = new ZShort(5);
			componentOrderLine3.OrderedQty = 10m;
			componentOrderLine3.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var componentCustomsData3 = new CustomsEntryInfo();
			componentCustomsData3.InwardsEntryKey = "ABC";
			componentCustomsData3.InwardsEntryLineNumber = new ZShort(2);
			componentOrderLine3.CustomsData = componentCustomsData3;
			orderLineDataObject2.SetOrderLineCollection(() => new List<OrderLine>() { componentOrderLine2, componentOrderLine3 });

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1, orderLineDataObject2 });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = 1;

			AssertExceptionThrown<DataObjectReadFailureException>("Failed to import - Component Lines for Secondary Products must have a Product on the Main Component Line Collection.",
				() => new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestOrderLineCollectionContentIsPartial_AddNewLineAndUpdateToExistingDocket

		protected override void SetupAndAssertDocketLinePrecondition(WhsDocketLine docketLine)
		{
			AssertEquals("Pre-condition: WE_PartAttrib1", "", docketLine.WE_PartAttrib1);
		}

		protected override void UpdateExistingDocketLineDataObject(OrderLine docketLineDataObject)
		{
			docketLineDataObject.PartAttribute1 = "RED";
		}

		protected override void AssertExistingDocketLineAfterImport(WhsDocketLine docketLine)
		{
			AssertEquals("WE_PartAttrib1 get updated.", "RED", docketLine.WE_PartAttrib1);
		}

		#endregion

		public void TestReadIntoBusinessObject_Finalizes() => TestReadIntoBusinessObject_FinalizesCore(requiredDateSet: true);

		[TestDate(2023, 1, 31)]
		public void TestReadIntoBusinessObject_Finalizes_RequiredDateNotSet() => TestReadIntoBusinessObject_FinalizesCore(requiredDateSet: false);

		void TestReadIntoBusinessObject_FinalizesCore(bool requiredDateSet)
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var area = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.SaveForTesting();
			AssertEquals("Precondition", true, warehouse.IsInwardProcessingEnabled);

			var component = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "HATCOMP"));
			var receive = Helper.CreateWhsReceiveWithInventory(Data.Orgs.CRAHOLSYD, warehouse, "R1", component, 100m, location, "");
			var inventory = receive.Lines.Single();
			inventory.WE_BondedEntryKey = "ABC-2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.SaveForTesting();

			var order = ShipmentDataObject.Order;
			order.IsInwardsProcessingJob = true;
			order.AutoFinaliseBOMIntoInventory = true;
			ShipmentDataObject.LocalProcessing = requiredDateSet ? new LocalProcessing { DeliveryRequiredBy = new ZDateTime(2023, 1, 1) } : null;

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject.OrderedQty = 10m;
			orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			orderLineDataObject.LineNumber = 1;
			orderLineDataObject.SubLineNumber = 1;
			orderLineDataObject.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject.CustomsData.IsSecondaryInwardsProcessedItem = false;

			var componentOrderLine = new OrderLine();
			componentOrderLine.Product = new Product { Code = "HATCOMP", Description = "Component Prod" };
			componentOrderLine.LineNumber = new ZShort(2);
			componentOrderLine.SubLineNumber = new ZShort(4);
			componentOrderLine.OrderedQty = 10m;
			componentOrderLine.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var componentCustomsData = new CustomsEntryInfo();
			componentCustomsData.InwardsEntryKey = "ABC";
			componentCustomsData.InwardsEntryLineNumber = new ZShort(2);
			componentOrderLine.CustomsData = componentCustomsData;
			orderLineDataObject.SetOrderLineCollection(() => new List<OrderLine>() { componentOrderLine });
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });

			var reader = new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory);

			WhsDynamicWorkOrder whsDynamicWorkOrderBO;
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				whsDynamicWorkOrderBO = reader.ReadIntoBusinessObject();
			}

			AssertNotNull(whsDynamicWorkOrderBO);

			CombineAssertions(() =>
			{
				AssertEquals("whsWorkOrderBO.WD_RequiredDate", requiredDateSet ? new ZDateTimeOffset(2023, 1, 1) : new ZDateTimeOffset(2023, 1, 31), whsDynamicWorkOrderBO.WD_RequiredDate);

				AssertEquals("Should be finalized.", true, whsDynamicWorkOrderBO.IsFinalised);
				AssertEquals("Should be finalized.", true, whsDynamicWorkOrderBO.Pick.IsFinalised);
				AssertNotNull("Should have created a receive.", whsDynamicWorkOrderBO.Receive);
				AssertEquals("Receive finalization.", true, whsDynamicWorkOrderBO.Receive.IsFinalised);

				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching {2} found, creating new {2}.
Information - Populating {2}...
Information - No matching Component {2} found, creating new Component {2}.
Information - Populating Component {2}...
Information - Added Warehouse {1} from UniversalShipment.
".Trim(), nameof(WhsDynamicWorkOrder), GetDocketType(), nameof(WhsDynamicWorkOrderLine)), Logger.Logs);
			});
		}

		public void TestReadIntoBusinessObject_Finalizes_Shortfall_NoStocks() => TestReadIntoBusinessObject_Finalizes_Shortfall(completelyShort: true);

		public void TestReadIntoBusinessObject_Finalizes_Shortfall_NotEnoughStocks() => TestReadIntoBusinessObject_Finalizes_Shortfall(completelyShort: false);

		void TestReadIntoBusinessObject_Finalizes_Shortfall(bool completelyShort)
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var area = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.SaveForTesting();
			AssertEquals("Precondition", true, warehouse.IsInwardProcessingEnabled);

			if (!completelyShort)
			{
				var component = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "HATCOMP"));
				var receive = Helper.CreateWhsReceiveWithInventory(Data.Orgs.CRAHOLSYD, warehouse, "R1", component, 5m, location, "");
				var inventory = receive.Lines.Single();
				inventory.WE_BondedEntryKey = "ABC-2";
				receive.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(receive);
				Factory.SaveForTesting();
			}

			var order = ShipmentDataObject.Order;
			order.IsInwardsProcessingJob = true;
			order.AutoFinaliseBOMIntoInventory = true;

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject.OrderedQty = 10m;
			orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			orderLineDataObject.LineNumber = 1;
			orderLineDataObject.SubLineNumber = 1;
			orderLineDataObject.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject.CustomsData.IsSecondaryInwardsProcessedItem = false;

			var componentOrderLine = new OrderLine();
			componentOrderLine.Product = new Product { Code = "HATCOMP", Description = "Component Prod" };
			componentOrderLine.LineNumber = new ZShort(2);
			componentOrderLine.SubLineNumber = new ZShort(4);
			componentOrderLine.OrderedQty = 10m;
			componentOrderLine.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var componentCustomsData = new CustomsEntryInfo();
			componentCustomsData.InwardsEntryKey = "ABC";
			componentCustomsData.InwardsEntryLineNumber = new ZShort(2);
			componentOrderLine.CustomsData = componentCustomsData;
			orderLineDataObject.SetOrderLineCollection(() => new List<OrderLine>() { componentOrderLine });
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });

			var reader = new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				AssertExceptionThrown<DataObjectReadFailureException>(
					"Should have thrown an exception.",
					@"Cannot Import Dynamic Work Order
Dynamic Work Order could not be finalized into the Warehouse for Warehouse Dynamic Work Order because of the following error(s):
Error: Finalise
Error - SumOfUnitsMet: To finalize a Dynamic Work Order, main product component lines must be fully allocated.",
					() => reader.ReadIntoBusinessObject());
			}
		}

		public void TestReadIntoBusinessObject_Finalizes_FailedDocketValidation()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var area = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.SaveForTesting();
			AssertEquals("Precondition", true, warehouse.IsInwardProcessingEnabled);

			var component = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "HATCOMP"));
			var receive = Helper.CreateWhsReceiveWithInventory(Data.Orgs.CRAHOLSYD, warehouse, "R1", component, 100m, location, "");
			var inventory = receive.Lines.Single();
			inventory.WE_BondedEntryKey = "ABC-2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.SaveForTesting();

			var order = ShipmentDataObject.Order;
			order.TotalUnits = 20m;
			order.IsInwardsProcessingJob = true;
			order.AutoFinaliseBOMIntoInventory = true;

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject.OrderedQty = 10m;
			orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			orderLineDataObject.LineNumber = 1;
			orderLineDataObject.SubLineNumber = 1;
			orderLineDataObject.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject.CustomsData.IsSecondaryInwardsProcessedItem = false;

			var componentOrderLine = new OrderLine();
			componentOrderLine.Product = new Product { Code = "HATCOMP", Description = "Component Prod" };
			componentOrderLine.LineNumber = new ZShort(2);
			componentOrderLine.SubLineNumber = new ZShort(4);
			componentOrderLine.OrderedQty = 10m;
			componentOrderLine.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var componentCustomsData = new CustomsEntryInfo();
			componentCustomsData.InwardsEntryKey = "ABC";
			componentCustomsData.InwardsEntryLineNumber = new ZShort(2);
			componentOrderLine.CustomsData = componentCustomsData;
			orderLineDataObject.SetOrderLineCollection(() => new List<OrderLine>() { componentOrderLine });
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });

			var reader = new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory);

			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				AssertExceptionThrown<DataObjectReadFailureException>(
					"Should have thrown an exception.",
					@"Cannot Import Dynamic Work Order
Dynamic Work Order could not be finalized into the Warehouse for Warehouse Dynamic Work Order because of the following error(s):
Error: Finalise
Error - WD_TotalUnits: Total Units 20 does not equal the total of all assembly line units (including secondary products) 10.",
					() => reader.ReadIntoBusinessObject());
			}
		}

		public void TestReadIntoBusinessObject_Finalizes_DoesNotFinaliseExistingDocket()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var area = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.SaveForTesting();
			AssertEquals("Precondition", true, warehouse.IsInwardProcessingEnabled);

			var component = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "HATCOMP"));
			var receive = Helper.CreateWhsReceiveWithInventory(Data.Orgs.CRAHOLSYD, warehouse, "R1", component, 100m, location, "");
			var inventory = receive.Lines.Single();
			inventory.WE_BondedEntryKey = "ABC-2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var existingWorkOrder = Helper.CreateWhsDynamicWorkOrder(Data.Orgs.CRAHOLSYD, warehouse, "ExtRef");
			Factory.SaveForTesting();

			var order = ShipmentDataObject.Order;
			order.IsInwardsProcessingJob = true;
			order.AutoFinaliseBOMIntoInventory = true;
			order.OrderNumber = "ExtRef";

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject.OrderedQty = 10m;
			orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			orderLineDataObject.LineNumber = 1;
			orderLineDataObject.SubLineNumber = 1;
			orderLineDataObject.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject.CustomsData.IsSecondaryInwardsProcessedItem = false;

			var componentOrderLine = new OrderLine();
			componentOrderLine.Product = new Product { Code = "HATCOMP", Description = "Component Prod" };
			componentOrderLine.LineNumber = new ZShort(2);
			componentOrderLine.SubLineNumber = new ZShort(4);
			componentOrderLine.OrderedQty = 10m;
			componentOrderLine.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var componentCustomsData = new CustomsEntryInfo();
			componentCustomsData.InwardsEntryKey = "ABC";
			componentCustomsData.InwardsEntryLineNumber = new ZShort(2);
			componentOrderLine.CustomsData = componentCustomsData;
			orderLineDataObject.SetOrderLineCollection(() => new List<OrderLine>() { componentOrderLine });
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });

			var reader = new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory);

			WhsDynamicWorkOrder whsDynamicWorkOrderBO;
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				whsDynamicWorkOrderBO = reader.ReadIntoBusinessObject();
			}

			CombineAssertions(() =>
			{
				AssertNotNull(whsDynamicWorkOrderBO);
				AssertEquals(whsDynamicWorkOrderBO.PK, existingWorkOrder.PK);
				AssertEquals("Should not have finalized existing docket.", false, whsDynamicWorkOrderBO.IsFinalised);
			});
		}

		public void TestReadIntoBusinessObject_Finalizes_PickIsNotFinalised()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			WhsDynamicWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var area = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.SaveForTesting();
			AssertEquals("Precondition", true, warehouse.IsInwardProcessingEnabled);

			var component = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "HATCOMP"));
			var receive = Helper.CreateWhsReceiveWithInventory(Data.Orgs.CRAHOLSYD, warehouse, "R1", component, 100m, location, "");
			var inventory = receive.Lines.Single();
			inventory.WE_BondedEntryKey = "ABC-2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.SaveForTesting();

			var order = ShipmentDataObject.Order;
			order.OrderNumber = "ABC123";
			order.IsInwardsProcessingJob = true;
			order.AutoFinaliseBOMIntoInventory = true;

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject.OrderedQty = 10m;
			orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			orderLineDataObject.LineNumber = 1;
			orderLineDataObject.SubLineNumber = 1;
			orderLineDataObject.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject.CustomsData.IsSecondaryInwardsProcessedItem = false;

			var componentOrderLine = new OrderLine();
			componentOrderLine.Product = new Product { Code = "HATCOMP", Description = "Component Prod" };
			componentOrderLine.LineNumber = new ZShort(2);
			componentOrderLine.SubLineNumber = new ZShort(4);
			componentOrderLine.OrderedQty = 10m;
			componentOrderLine.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };

			var componentCustomsData = new CustomsEntryInfo();
			componentCustomsData.InwardsEntryKey = "ABC";
			componentCustomsData.InwardsEntryLineNumber = new ZShort(2);
			componentOrderLine.CustomsData = componentCustomsData;
			orderLineDataObject.SetOrderLineCollection(() => new List<OrderLine>() { componentOrderLine });
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });

			var reader = new TestWhsDynamicWorkOrderDataObjectReaderWithPickFailedToFinalise(ShipmentDataObject, Logger, Factory);
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				AssertExceptionThrown<DataObjectReadFailureException>(
					"Should have thrown an exception.",
					@"Cannot finalize pick
Order External Reference: ABC123 Failed to Finalize Pick.
Order Line Product: BOWLHAT
Error - Docket Line: Something is wrong, that's the reason why pick is not finalised.",
					() => reader.ReadIntoBusinessObject());
			}
		}

		#region TestPickPriority

		public void TestPickPriority()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.Order.PickPriority = 3;

			var reader = new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsDynamicWorkOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDynamicWorkOrderBO);
			AssertEquals((byte)3, whsDynamicWorkOrderBO.WD_PickPriority);
		}

		public void TestPickPriority_InvalidValue()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.Order.PickPriority = 0;
			AssertNoExceptionThrown(() => new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());

			ShipmentDataObject.Order.PickPriority = 21;
			AssertExceptionThrown<DataObjectReadFailureException>("Failed to import",
				@"Cannot Import Dynamic Work Order, Pick Priority should be in range 0 to 20.",
				() => new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());

			ShipmentDataObject.Order.PickPriority = 20;
			AssertNoExceptionThrown(() => new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestPickPriority_NoWarningsIfValueIsNotProvided()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var reader = new WhsDynamicWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsDynamicWorkOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDynamicWorkOrderBO);
			AssertEquals((byte)0, whsDynamicWorkOrderBO.WD_PickPriority);
			AssertEquals("Should have no warnings.", false, Logger.HasWarnings);
		}

		#endregion

		class TestWhsDynamicWorkOrderDataObjectReaderWithPickFailedToFinalise : WhsDynamicWorkOrderDataObjectReader
		{
			internal TestWhsDynamicWorkOrderDataObjectReaderWithPickFailedToFinalise(Shipment whsOrderDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
				: base(whsOrderDataObject, logger, factory)
			{
			}

			protected override WhsDynamicWorkOrder GetNewBusinessObject()
			{
				var result = base.GetNewBusinessObject();

				result.WD_FinalisedDateInfo.ValueChanged += (o, e) =>
				{
					if (result.WD_FinalisedDate.IsValid)
					{
						var pick = result.Pick;
						pick.WP_PickStatusInfo.ValueChanged += (p, pe) =>
						{
							if (pick.WP_PickStatus == PickStatus.Codes.Finalised)
							{
								pick.WP_PickStatus = PickStatus.Codes.Created;

								var orderLine = result.Lines[0];
								orderLine.AddRowError("Something is wrong, that's the reason why pick is not finalised.");
							}
						};
					}
				};

				return result;
			}
		}

		#region Implementation

		protected override DataContextType DataContext => DataContextType.WarehouseDynamicWorkOrder;

		protected override WhsDynamicWorkOrder CreateDocketWithLine(OrgHeader client, WhsWarehouse warehouse, OrgSupplierPart product, ZDecimal units)
		{
			var docket = Helper.CreateWhsDynamicWorkOrderWithLine(client, warehouse, "DO1", product, units);
			var line = docket.Lines[0];
			line.CustomsData.WB_IsMainInwardsProcessedItem = true;
			return docket;
		}

		protected override string GetDocketType() => "Dynamic Work Order";

		protected override WhsDynamicWorkOrder GetNewDocket(OrgHeader client, WhsWarehouse warehouse, string externalReference = "")
			=> Helper.CreateWhsDynamicWorkOrder(client, warehouse, externalReference);

		protected override Shipment GetNewDocketDataObject(WhsDynamicWorkOrder docket)
		{
			var writer = new WhsDynamicWorkOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.WDO, docket)));
			var result = writer.GetDataObject(docket);
			result.DataContext.AddDataTarget(((ITopLevelDataObjectWriter)writer).TopLevelDataContextType, docket.WD_DocketID);
			Logger.TopLevelDataObject = result;
			return result;
		}

		protected override WhsDocket GetNewDocketOfDifferentType()
			=> Factory.NewWithValidTestData<WhsOrder>();

		protected override OrderLine GetSecondOrderLineDataObjectForPartialCollectionImport(Order orderDataObject, bool isNewLine = false)
		{
			var orderLineDataObject = base.GetSecondOrderLineDataObjectForPartialCollectionImport(orderDataObject, isNewLine);
			orderLineDataObject.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject.CustomsData.IsMainInwardsProcessedItem = false;
			orderLineDataObject.CustomsData.IsSecondaryInwardsProcessedItem = true;
			return orderLineDataObject;
		}

		protected override DataObjectList<OrderLine> GetDummyOrderLineList(OrgHeader client)
		{
			Helper.CreateProduct("PROD2", client);
			var orderLineDataObject = new OrderLine();
			orderLineDataObject.Product = new Product { Code = "PROD2", Description = "Other Prod" };
			orderLineDataObject.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject.CustomsData.IsMainInwardsProcessedItem = true;
			orderLineDataObject.OrderedQty = 1;
			return new DataObjectList<OrderLine> { orderLineDataObject };
		}

		protected override WhsDynamicWorkOrderDataObjectReader GetNewReader(Shipment shipmentDataObject, IXmlImportLogger logger, bool useCleanFactory = false)
			=> new WhsDynamicWorkOrderDataObjectReader(shipmentDataObject, logger, useCleanFactory ? new UniversalObjectFactory() : Factory);

		protected override string GetProcessType()
			=> WorkflowDescriptors.WhsDynamicWorkOrderWorkflowDescriptorCode;

		protected override bool SupportsWorkflowCustomFieldImport => false;

		#endregion
	}
}
