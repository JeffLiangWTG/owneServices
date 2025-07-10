using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
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
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsWorkOrderDataObjectReaderTest : WhsDocketDataObjectReaderTest<WhsWorkOrder, WhsWorkOrderLine, WhsWorkOrderDataObjectReader>
	{
		#region TestDataContextType

		public void TestDataContextType()
		{
			var reader = GetNewReader(ShipmentDataObject, Logger);
			AssertEquals(DataContextType.WarehouseWorkOrder, reader.DataContextType);
		}

		#endregion

		#region TestLoadingDocket

		#region TestWhsDocketsOfDifferentTypesAreNotPickedUpByContextMatchingForTheTypeOfDocketWeWantToMatch

		public void TestWhsDocketsOfDifferentTypesAreNotPickedUpByContextMatchingForTheTypeOfDocketWeWantToMatch()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var whsDocketOfDifferentType = GetNewDocketOfDifferentType();
			AssertNotEquals("Docket to test against should be of a different type", typeof(WhsWorkOrder), whsDocketOfDifferentType.GetType());

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
".Trim(), nameof(WhsWorkOrder), GetDocketType()), Logger.Logs);
			});
		}

		#endregion

		#region TestLoadsLatestDocketWhenHasEqualMatches

		public void TestLoadsLatestDocketWhenHasEqualMatches()
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
			ShipmentDataObject.Order.OrderNumberSplit = null;
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
".Trim(), nameof(WhsWorkOrder), GetDocketType(), docketBOToLoad.WD_DocketID), Logger.Logs);
			});
		}

		#endregion

		#region TestDoesNotMatchExistingDocketWithOrderNumberOnly

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
			var whsDocketBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsDocketBO);

			CombineAssertions(() =>
			{
				AssertEquals("whsDocketBO.WD_TotalUnits", 11m, whsDocketBO.WD_TotalUnits);
				AssertNotEquals("Does not load docket", nonMatchingClientDocketBO.PK, whsDocketBO.PK);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Warehouse {1} from UniversalShipment.
".Trim(), nameof(WhsWorkOrder), GetDocketType()), Logger.Logs);
			});
		}

		#endregion

		#region TestMatchingExternalReferenceConsiderSplitNo

		public void TestMatchingExternalReferenceConsiderSplitNo()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.Order.OrderNumber = "ExternalRef";
			ShipmentDataObject.Order.OrderNumberSplit = 0;
			ShipmentDataObject.Order.TotalUnits = 5m;

			// create 
			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocket0 = reader.ReadIntoBusinessObject();
			AssertNotNull(whsDocket0);
			CombineAssertions(() =>
			{
				AssertEquals("whsDocket0.WD_ExternalReferenceSplit", (byte)0, whsDocket0.WD_ExternalReferenceSplit);
				AssertEquals("whsDocket0.WD_TotalUnits", 5m, whsDocket0.WD_TotalUnits);
			});

			// update
			ShipmentDataObject.Order.TotalUnits = 15m;
			reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocket0Updated = reader.ReadIntoBusinessObject();
			AssertNotNull(whsDocket0Updated);
			CombineAssertions(() =>
			{
				AssertEquals("whsDocket0_updated.WD_ExternalReferenceSplit", (byte)0, whsDocket0Updated.WD_ExternalReferenceSplit);
				AssertEquals("whsDocket0_updated.WD_TotalUnits", 15m, whsDocket0Updated.WD_TotalUnits);
				AssertEquals("Should find whsDocket0 and updated.", whsDocket0.PK, whsDocket0Updated.PK);
			});

			// create new with split 1
			ShipmentDataObject.Order.OrderNumberSplit = 1;
			reader = GetNewReader(ShipmentDataObject, Logger);
			var whsDocket1 = reader.ReadIntoBusinessObject();
			AssertNotNull(whsDocket1);
			CombineAssertions(() =>
			{
				AssertEquals("whsDocket1.WD_ExternalReferenceSplit", (byte)1, whsDocket1.WD_ExternalReferenceSplit);
				AssertEquals("whsDocket1.WD_TotalUnits", 15m, whsDocket1.WD_TotalUnits);
				AssertNotEquals("Should not find existing docket with split no 0.", whsDocket0.PK, whsDocket1.PK);
			});

			AssertEquals("Matches based on split no.", whsDocket1, GetNewReader(ShipmentDataObject, Logger).ReadIntoBusinessObject());
		}

		#endregion

		#region TestMatchingExistingDocketThroughClientAddressPlusOrderNumberPlusZeroSplitNumber

		public void TestMatchingExistingDocketThroughClientAddressPlusOrderNumberPlusZeroSplitNumber()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			var dummyDocket = GetNewDocket(new WhsTestHelperFunctions(Factory.BOFactory).CreateClient(), warehouse);
			dummyDocket.WD_ExternalReference = "ORDERME";
			Factory.SaveForTesting();

			var docketBOToLoad = GetNewDocket(client, warehouse);
			docketBOToLoad.WD_ExternalReference = "ORDERME";
			Factory.SaveForTesting();

			ShipmentDataObject.Order.OrderNumber = "ORDERME";
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
".Trim(), nameof(WhsWorkOrder), GetDocketType(), docketBOToLoad.WD_DocketID), Logger.Logs);
			});
		}

		#endregion

		#region TestMatchingExistingDocketThroughClientAddressPlusOrderNumberPlusSplitNumber

		public void TestMatchingExistingDocketThroughClientAddressPlusOrderNumberPlusSplitNumber()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			var dummyDocket = GetNewDocket(new WhsTestHelperFunctions(Factory.BOFactory).CreateClient(), warehouse);
			dummyDocket.WD_ExternalReference = "ORDERME";
			dummyDocket.WD_ExternalReferenceSplit = new ZByte(2);
			Factory.SaveForTesting();

			var docketBOToLoad = GetNewDocket(client, warehouse);
			docketBOToLoad.WD_ExternalReference = "ORDERME";
			docketBOToLoad.WD_ExternalReferenceSplit = new ZByte(2);
			docketBOToLoad.WD_TotalUnits = 10m;
			Factory.SaveForTesting();

			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.Order.OrderNumberSplit = new ZByte(2);
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
".Trim(), nameof(WhsWorkOrder), GetDocketType(), docketBOToLoad.WD_DocketID), Logger.Logs);
			});
		}

		#endregion

		#endregion

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

			var reader = new WhsWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsWorkOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsWorkOrderBO);

			CombineAssertions(() =>
			{
				AssertEquals("whsWorkOrderBO.Warehouse.WW_WarehouseCode", "WHS", whsWorkOrderBO.Warehouse.WW_WarehouseCode);
				AssertEquals("whsWorkOrderBO.WD_DocketSubType", "ASS", whsWorkOrderBO.WD_DocketSubType);
				AssertEquals("whsWorkOrderBO.WD_RequiredDate", new ZDateTimeOffset(2011, 1, 1), whsWorkOrderBO.WD_RequiredDate);
				AssertEquals("whsWorkOrderBO.WD_ExternalReference", "ORDERME", whsWorkOrderBO.WD_ExternalReference);
				AssertEquals("whsWorkOrderBO.WD_ExternalReferenceSplit", new ZByte(1), whsWorkOrderBO.WD_ExternalReferenceSplit);
				AssertEquals("whsWorkOrderBO.WD_PickOption", "AUT", whsWorkOrderBO.WD_PickOption);

				AssertEquals("whsWorkOrderBO.WD_TotalPallets", new ZShort(4), whsWorkOrderBO.WD_TotalPallets);
				AssertEquals("whsWorkOrderBO.WD_TotalUnits", 12.3m, whsWorkOrderBO.WD_TotalUnits);
				AssertEquals("whsWorkOrderBO.WD_PackagesSent", 3, whsWorkOrderBO.WD_PackagesSent);
				AssertEquals("whsWorkOrderBO.WD_TotalWeight", 32.6m, whsWorkOrderBO.WD_TotalWeight);
				AssertEquals("whsWorkOrderBO.WD_TotalWeightUnit", "LB", whsWorkOrderBO.WD_TotalWeightUnit);
				AssertEquals("whsWorkOrderBO.WD_TotalCubic", 12.5m, whsWorkOrderBO.WD_TotalCubic);
				AssertEquals("whsWorkOrderBO.WD_TotalCubicUnit", "CY", whsWorkOrderBO.WD_TotalCubicUnit);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsWorkOrder found, creating new WhsWorkOrder.
Information - Populating WhsWorkOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsDocketReference found, creating new WhsDocketReference.
Information - Populating WhsDocketReference...
Information - Added Warehouse Work Order from UniversalShipment.
".Trim(), Logger.Logs);
			});

			Factory.SaveForTesting(); // Assert can save to the database
			AssertEquals("whsWorkOrderBO.WD_DocketStatus", "ENT", whsWorkOrderBO.WD_DocketStatus);
		}

		#endregion

		#region TestReadIntoBusinessObject_Lines

		public void TestReadIntoBusinessObject_Lines()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var product = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			Helper.SetClientAllAttributeType(order.Client, false);
			Helper.SetProductAllAttributeUse(order.Client, product, true);
			Helper.SetClientAttributeType(order.Client, AttributeNumber.Serial, true);
			Factory.SaveForTesting();

			var orderLineDataObject = WhsWorkOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.Lines.Count", 1, whsOrderBO.Lines.Count);

			CombineAssertions(() =>
			{
				var orderLineBO = whsOrderBO.Lines[0];
				WhsWorkOrderLineDataObjectReaderTest.AssertStandardOrderLineContents(orderLineBO, useSerial: true);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching WhsWorkOrder found, creating new WhsWorkOrder.
Information - Populating WhsWorkOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsWorkOrderLine found, creating new WhsWorkOrderLine.
Information - Populating WhsWorkOrderLine...
Information - Added Warehouse Work Order from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestReadIntoBusinessObject_Lines_UpdatingLines()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var product = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			Helper.SetClientAllAttributeType(order.Client, false);
			Helper.SetProductAllAttributeUse(order.Client, product, true);
			Helper.SetClientAttributeType(order.Client, AttributeNumber.Serial, true);
			Factory.SaveForTesting();

			var orderLineDataObject1 = WhsWorkOrderLineDataObjectReaderTest.SetupStandardOrderLine();
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

			var orderLineDataObject2 = WhsWorkOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject2.LineNumber = 2;
			orderLineDataObject2.SubLineNumber = 2;
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
			var order = WhsOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var product = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			Helper.SetClientAllAttributeType(order.Client, false);
			Helper.SetProductAllAttributeUse(order.Client, product, true);
			Helper.SetClientAttributeType(order.Client, AttributeNumber.Serial, true);
			Factory.SaveForTesting();

			var orderLineDataObject1 = WhsWorkOrderLineDataObjectReaderTest.SetupStandardOrderLine();
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

			var orderLineDataObject2 = WhsWorkOrderLineDataObjectReaderTest.SetupStandardOrderLine();
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
				AssertEquals("Should have updated the existing docket.", 42, whsDocket0Updated.WD_PackagesSent);
				AssertEquals("Should *not* have updated the lines.", 1, whsDocket0Updated.Lines.Count);
				AssertNotEquals("Should *not* have updated lines.", "OTHER", whsDocket0Updated.Lines[0].WE_PartAttrib1);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsWorkOrder found, creating new WhsWorkOrder.
Information - Populating WhsWorkOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsWorkOrderLine found, creating new WhsWorkOrderLine.
Information - Populating WhsWorkOrderLine...
Information - Added Warehouse Work Order from UniversalShipment.
Information - Successfully loaded matching WhsWorkOrder.
Information - Populating WhsWorkOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Warning - Cannot update Work Order Lines on a Finalized or In Picking Work Order.
Information - Updated Warehouse Work Order from UniversalShipment.

".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestReadIntoBusinessObject_TotalVolumeUnit

		public void TestReadIntoBusinessObject_TotalVolumeUnit_IsEmpty() => TestReadIntoBusinessObject_TotalVolumeUnit(isNull: false);
		public void TestReadIntoBusinessObject_TotalVolumeUnit_IsNull() => TestReadIntoBusinessObject_TotalVolumeUnit(isNull: true);

		void TestReadIntoBusinessObject_TotalVolumeUnit(bool isNull)
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			Factory.SaveForTesting();

			ShipmentDataObject.Order.TotalLineVolume = 12.5m;
			ShipmentDataObject.TotalVolumeUnit = isNull ? null : new UnitOfVolume { Code = "", Description = "" };

			var reader = new WhsWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsWorkOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsWorkOrderBO);

			CombineAssertions(() =>
			{
				AssertEquals("whsWorkOrderBO.WD_TotalCubic", 12.5m, whsWorkOrderBO.WD_TotalCubic);
				AssertEquals("whsWorkOrderBO.WD_TotalCubicUnit", "M3", whsWorkOrderBO.WD_TotalCubicUnit);
			});
		}

		#endregion

		#region TestReadIntoBusinessObject_TotalWeightUnit

		public void TestReadIntoBusinessObject_TotalWeightUnit_IsEmpty() => TestReadIntoBusinessObject_TotalWeightUnit(isNull: false);
		public void TestReadIntoBusinessObject_TotalWeightUnit_IsNull() => TestReadIntoBusinessObject_TotalWeightUnit(isNull: true);

		void TestReadIntoBusinessObject_TotalWeightUnit(bool isNull)
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			Factory.SaveForTesting();

			ShipmentDataObject.Order.TotalLineWeight = 12.5m;
			ShipmentDataObject.TotalWeightUnit = isNull ? null : new UnitOfWeight { Code = "", Description = "" };

			var reader = new WhsWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsWorkOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsWorkOrderBO);

			CombineAssertions(() =>
			{
				AssertEquals("whsWorkOrderBO.WD_TotalWeight", 12.5m, whsWorkOrderBO.WD_TotalWeight);
				AssertEquals("whsWorkOrderBO.WD_TotalWeightUnit", "KG", whsWorkOrderBO.WD_TotalWeightUnit);
			});
		}

		#endregion

		#region TestPickPriority

		public void TestPickPriority()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.Order.PickPriority = 3;

			var reader = new WhsWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsWorkOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsWorkOrderBO);
			AssertEquals((byte)3, whsWorkOrderBO.WD_PickPriority);
		}

		public void TestPickPriority_InvalidValue()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.Order.PickPriority = 0;
			AssertNoExceptionThrown(() => new WhsWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());

			ShipmentDataObject.Order.PickPriority = 21;
			AssertExceptionThrown<DataObjectReadFailureException>("Failed to import",
				@"Cannot Import Work Order, Pick Priority should be in range 0 to 20.",
				() => new WhsWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());

			ShipmentDataObject.Order.PickPriority = 20;
			AssertNoExceptionThrown(() => new WhsWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestPickPriority_NoWarningsIfValueIsNotProvided()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var reader = new WhsWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsWorkOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsWorkOrderBO);
			AssertEquals((byte)0, whsWorkOrderBO.WD_PickPriority);
			AssertEquals("Should have no warnings.", false, Logger.HasWarnings);
		}

		#endregion

		#region  TestReadIntoBusinessObject_CalculateTotalsIfNeeded

		public void TestReadIntoBusinessObject_CalculateTotalsIfNeeded_TotalUnits()
		{
			var factory2 = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory2);
			var data = new TestDataSimpleEnvironment(factory2);
			var mainProduct = helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1 = helper.CreateProduct("WHEEL", data.Org1);
			helper.CreateProductBOM(mainProduct, componentProduct1, 2, Constants.PkgUnit.Unit);
			var componentProduct2 = helper.CreateProduct("ENGINE", data.Org1);
			helper.CreateProductBOM(mainProduct, componentProduct2, 1, Constants.PkgUnit.Unit);
			factory2.Save();

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct1, 10m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", componentProduct2, 10m);
			factory2.Save();

			var workOrder = helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", mainProduct, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			factory2.Save();

			AssertEquals("Precondition", 0m, workOrder.WD_TotalUnits);

			var shipment = new WhsWorkOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, workOrder))).GetDataObject(workOrder);
			workOrder.Lines.DeleteAll(); // To ensure we are definitely re-reading lines

			shipment.Order.TotalUnits = null;
			var reader1 = GetNewReader(shipment, Logger);
			var whsOrderBO1 = reader1.ReadIntoBusinessObject();
			AssertNotNull(whsOrderBO1);
			AssertEquals(workOrder.PK, whsOrderBO1.PK);
			AssertEquals("Should have calculated total units.", 5m, whsOrderBO1.WD_TotalUnits);

			shipment.Order.TotalUnits = 42;
			var reader2 = GetNewReader(shipment, Logger);
			var whsOrderBO2 = reader2.ReadIntoBusinessObject();
			AssertNotNull(whsOrderBO2);
			AssertEquals(workOrder.PK, whsOrderBO2.PK);
			AssertEquals("Should *not* have calculated total units.", 42m, whsOrderBO2.WD_TotalUnits);
		}

		public void TestReadIntoBusinessObject_CalculateTotalsIfNeeded_TotalUnits_Disassembly()
		{
			var factory2 = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory2);
			var data = new TestDataSimpleEnvironment(factory2);
			var mainProduct = helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1 = helper.CreateProduct("WHEEL", data.Org1);
			helper.CreateProductBOM(mainProduct, componentProduct1, 2, Constants.PkgUnit.Unit);
			var componentProduct2 = helper.CreateProduct("ENGINE", data.Org1);
			helper.CreateProductBOM(mainProduct, componentProduct2, 1, Constants.PkgUnit.Unit);
			factory2.Save();

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct1, 10m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", componentProduct2, 10m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", mainProduct, 10m);
			factory2.Save();

			var workOrder = helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", mainProduct, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			factory2.Save();

			AssertEquals("Precondition", 0m, workOrder.WD_TotalUnits);

			var shipment = new WhsWorkOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, workOrder))).GetDataObject(workOrder);
			workOrder.Lines.DeleteAll(); // To ensure we are definitely re-reading lines

			shipment.Order.TotalUnits = null;
			var reader1 = GetNewReader(shipment, Logger);
			var whsOrderBO1 = reader1.ReadIntoBusinessObject();
			AssertNotNull(whsOrderBO1);
			AssertEquals(workOrder.PK, whsOrderBO1.PK);
			AssertEquals("Should have calculated total units.", 15m, whsOrderBO1.WD_TotalUnits);

			shipment.Order.TotalUnits = 42;
			var reader2 = GetNewReader(shipment, Logger);
			var whsOrderBO2 = reader2.ReadIntoBusinessObject();
			AssertNotNull(whsOrderBO2);
			AssertEquals(workOrder.PK, whsOrderBO2.PK);
			AssertEquals("Should *not* have calculated total units.", 42m, whsOrderBO2.WD_TotalUnits);
		}

		public void TestReadIntoBusinessObject_CalculateTotalsIfNeeded_TotalUnits_Assembly_IPROrder_IncludesSecondaryProducts()
			=> TestReadIntoBusinessObject_CalculateTotalsIfNeeded_TotalUnits_Assembly_IPROrder_IncludesSecondaryProducts(totalUnitsProvided: false);

		public void TestReadIntoBusinessObject_CalculateTotalsIfNeeded_TotalUnits_Assembly_IPROrder_IncludesSecondaryProducts_TotalUnitsProvided()
			=> TestReadIntoBusinessObject_CalculateTotalsIfNeeded_TotalUnits_Assembly_IPROrder_IncludesSecondaryProducts(totalUnitsProvided: true);

		void TestReadIntoBusinessObject_CalculateTotalsIfNeeded_TotalUnits_Assembly_IPROrder_IncludesSecondaryProducts(bool totalUnitsProvided)
		{
			var factory2 = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory2);
			var data = new TestDataSimpleEnvironment(factory2);
			helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var secondaryPart1 = helper.CreateProduct("P3", data.Org1);
			var secondaryPart2 = helper.CreateProduct("P4", data.Org1);
			helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			helper.CreateSecondaryProduct(data.Part2, secondaryPart1, 3m);
			helper.CreateSecondaryProduct(data.Part2, secondaryPart2, 4m);

			var area = helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			factory2.Save();

			var componentReceive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, "BEK-1", allocateLocations: false, finalise: false);
			componentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive.WD_IsInwardsProcessingJob = true;
			componentReceive.Lines[0].WE_WL = location.PK;
			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);
			factory2.Save();

			var workOrder = helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;

			var productParam = workOrder.Lines[0].Product.ParamsByWhsAndClient.AddNew();
			productParam.W3_OH = data.Org1.PK;
			productParam.W3_WW = data.Whs1.PK;
			productParam.W3_WL_InwardsProcessingStagingLocationBOM = location.PK;
			factory2.Save();

			AssertEquals("Precondition", 0m, workOrder.WD_TotalUnits);

			var shipment = new WhsWorkOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, workOrder))).GetDataObject(workOrder);
			workOrder.Lines.DeleteAll(); // To ensure we are definitely re-reading lines

			shipment.Order.TotalUnits = totalUnitsProvided ? (ZDecimal?)42m : null;
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				var reader1 = GetNewReader(shipment, Logger);
				var whsOrderBO1 = reader1.ReadIntoBusinessObject();

				AssertNotNull(whsOrderBO1);
				AssertEquals(workOrder.PK, whsOrderBO1.PK);
				AssertEquals("Total Units.", totalUnitsProvided ? 42m : 40m, whsOrderBO1.WD_TotalUnits);
			}
		}

		public void TestReadIntoBusinessObject_CalculateTotalsIfNeeded_TotalUnits_Assembly_ExcludesSecondaryProducts()
		{
			var factory2 = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory2);
			var data = new TestDataSimpleEnvironment(factory2);
			var secondaryPart1 = helper.CreateProduct("P3", data.Org1);
			var secondaryPart2 = helper.CreateProduct("P4", data.Org1);
			helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			helper.CreateSecondaryProduct(data.Part2, secondaryPart1, 3m);
			helper.CreateSecondaryProduct(data.Part2, secondaryPart2, 4m);
			factory2.Save();

			var componentReceive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			factory2.Save();

			var workOrder = helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			factory2.Save();

			AssertEquals("Precondition", 0m, workOrder.WD_TotalUnits);

			var shipment = new WhsWorkOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, workOrder))).GetDataObject(workOrder);
			workOrder.Lines.DeleteAll(); // To ensure we are definitely re-reading lines

			shipment.Order.TotalUnits = null;
			var reader1 = GetNewReader(shipment, Logger);
			var whsOrderBO1 = reader1.ReadIntoBusinessObject();
			AssertNotNull(whsOrderBO1);
			AssertEquals(workOrder.PK, whsOrderBO1.PK);
			AssertEquals("Should have calculated total units, excluding secondary parts.", 5m, whsOrderBO1.WD_TotalUnits);

			shipment.Order.TotalUnits = 42;
			var reader2 = GetNewReader(shipment, Logger);
			var whsOrderBO2 = reader2.ReadIntoBusinessObject();
			AssertNotNull(whsOrderBO2);
			AssertEquals(workOrder.PK, whsOrderBO2.PK);
			AssertEquals("Should *not* have calculated total units.", 42m, whsOrderBO2.WD_TotalUnits);
		}

		public void TestReadIntoBusinessObject_CalculateTotalsIfNeeded_WeightAndVolume_Assembly()
		{
			var factory2 = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory2);
			var org = helper.CreateClient();
			var whs = helper.CreateWarehouse("A");
			var mainPart = helper.CreateProduct(org, "MainPart");
			var subPart1 = helper.CreateProduct(org, "SubPart1");
			var subPart2 = helper.CreateProduct(org, "SubPart2");
			var level3Part1 = helper.CreateProduct(org, "Level3Part1");
			var level3Part2 = helper.CreateProduct(org, "Level3Part2");
			helper.SetProductWeightAndVolume(subPart1, 5, "KG", 0.5, "M3");
			helper.SetProductWeightAndVolume(subPart2, 10, "KG", 0.1, "M3");
			helper.SetProductWeightAndVolume(level3Part1, 6, "KG", 0.3, "M3");
			helper.SetProductWeightAndVolume(level3Part2, 5, "KG", 0.2, "M3");
			helper.CreateProductBOM(mainPart, subPart1, 2m, Constants.PkgUnit.Unit);
			helper.CreateProductBOM(mainPart, subPart2, 1m, Constants.PkgUnit.Unit);
			helper.CreateProductBOM(subPart1, level3Part1, 1m, Constants.PkgUnit.Unit);
			helper.CreateProductBOM(subPart1, level3Part2, 1m, Constants.PkgUnit.Unit);

			var workOrder = helper.CreateWhsWorkOrder(org, whs);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;

			var workOrderLine1 = workOrder.Lines.AddNew();
			workOrderLine1.WE_OP = mainPart.PK;
			workOrderLine1.WE_TransactionQuantity = 1m;

			var workOrderLine2 = workOrder.Lines.AddNew();
			workOrderLine2.WE_OP = mainPart.PK;
			workOrderLine2.WE_TransactionQuantity = 1m;
			AssertEquals("Precondition: Total Cubic.", 2.2m, workOrder.WD_TotalCubic);
			AssertEquals("Precondition: Total Weight.", 40m, workOrder.WD_TotalWeight);

			workOrder.WD_TotalWeight = 1m;
			workOrder.WD_TotalCubic = 2m;
			factory2.Save();

			var shipment = new WhsWorkOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, workOrder))).GetDataObject(workOrder);
			workOrder.Lines.DeleteAll(); // To ensure we are definitely re-reading lines

			shipment.TotalVolumeUnit = new UnitOfVolume { Code = "M3" };
			shipment.TotalWeightUnit = new UnitOfWeight { Code = "KG" };
			shipment.Order.TotalLineVolume = null;
			shipment.Order.TotalLineWeight = null;
			var reader1 = GetNewReader(shipment, Logger);
			var whsOrderBO1 = reader1.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertNotNull(whsOrderBO1);
				AssertEquals(workOrder.PK, whsOrderBO1.PK);
				AssertEquals("Total Cubic should be updated.", 2.2m, whsOrderBO1.WD_TotalCubic);
				AssertEquals("Total Weight should be updated.", 40m, whsOrderBO1.WD_TotalWeight);
			});

			shipment.TotalVolumeUnit = new UnitOfVolume { Code = "CF" };
			shipment.TotalWeightUnit = new UnitOfWeight { Code = "G" };
			var reader2 = GetNewReader(shipment, Logger);
			var whsOrderBO2 = reader2.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertNotNull(whsOrderBO2);
				AssertEquals(workOrder.PK, whsOrderBO2.PK);
				AssertEquals("Total Cubic should be updated.", 77.690m, whsOrderBO2.WD_TotalCubic);
				AssertEquals("Total Cubic Unit should be updated.", "CF", whsOrderBO2.WD_TotalCubicUnit);
				AssertEquals("Total Weight should be updated.", 40000m, whsOrderBO2.WD_TotalWeight);
				AssertEquals("Total Weight Unit should be updated.", "G", whsOrderBO2.WD_TotalWeightUnit);
			});

			shipment.Order.TotalLineVolume = 10m;
			shipment.Order.TotalLineWeight = 5m;
			var reader3 = GetNewReader(shipment, Logger);
			var whsOrderBO3 = reader3.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertNotNull(whsOrderBO3);
				AssertEquals(workOrder.PK, whsOrderBO3.PK);
				AssertEquals("Should *not* have calculated cubic units.", 10m, whsOrderBO3.WD_TotalCubic);
				AssertEquals("Should *not* have calculated weight units.", 5m, whsOrderBO3.WD_TotalWeight);

				AssertEquals("Total Cubic Unit should *not* be updated.", "CF", whsOrderBO2.WD_TotalCubicUnit);
				AssertEquals("Total Weight Unit should *not* be updated.", "G", whsOrderBO2.WD_TotalWeightUnit);
			});
		}

		public void TestReadIntoBusinessObject_CalculateTotalsIfNeeded_WeightAndVolume_Disassembly()
		{
			var factory2 = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory2);
			var org = helper.CreateClient();
			var whs = helper.CreateWarehouse("A");
			var mainPart = helper.CreateProduct(org, "MainPart");
			var subPart1 = helper.CreateProduct(org, "SubPart1");
			var subPart2 = helper.CreateProduct(org, "SubPart2");
			var level3Part1 = helper.CreateProduct(org, "Level3Part1");
			var level3Part2 = helper.CreateProduct(org, "Level3Part2");
			helper.SetProductWeightAndVolume(mainPart, 30, "KG", 1, "M3");
			helper.CreateProductBOM(mainPart, subPart1, 2m, Constants.PkgUnit.Unit);
			helper.CreateProductBOM(mainPart, subPart2, 1m, Constants.PkgUnit.Unit);
			helper.CreateProductBOM(subPart1, level3Part1, 1m, Constants.PkgUnit.Unit);
			helper.CreateProductBOM(subPart1, level3Part2, 1m, Constants.PkgUnit.Unit);

			var workOrder = helper.CreateWhsWorkOrder(org, whs);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			var workOrderLine1 = workOrder.Lines.AddNew();
			workOrderLine1.WE_OP = mainPart.PK;
			workOrderLine1.WE_TransactionQuantity = 1m;

			var workOrderLine2 = workOrder.Lines.AddNew();
			workOrderLine2.WE_OP = mainPart.PK;
			workOrderLine2.WE_TransactionQuantity = 1m;
			AssertEquals("Precondition: Total Cubic.", 2m, workOrder.WD_TotalCubic);
			AssertEquals("Precondition: Total Weight.", 60m, workOrder.WD_TotalWeight);

			workOrder.WD_TotalWeight = 1m;
			workOrder.WD_TotalCubic = 2m;
			factory2.Save();

			var shipment = new WhsWorkOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, workOrder))).GetDataObject(workOrder);
			workOrder.Lines.DeleteAll(); // To ensure we are definitely re-reading lines

			shipment.TotalVolumeUnit = new UnitOfVolume { Code = "M3" };
			shipment.TotalWeightUnit = new UnitOfWeight { Code = "KG" };
			shipment.Order.TotalLineVolume = null;
			shipment.Order.TotalLineWeight = null;
			var reader1 = GetNewReader(shipment, Logger);
			var whsOrderBO1 = reader1.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertNotNull(whsOrderBO1);
				AssertEquals(workOrder.PK, whsOrderBO1.PK);
				AssertEquals("Total Cubic should be updated.", 2m, whsOrderBO1.WD_TotalCubic);
				AssertEquals("Total Weight should be updated.", 60m, whsOrderBO1.WD_TotalWeight);
			});

			shipment.TotalVolumeUnit = new UnitOfVolume { Code = "CF" };
			shipment.TotalWeightUnit = new UnitOfWeight { Code = "G" };
			var reader2 = GetNewReader(shipment, Logger);
			var whsOrderBO2 = reader2.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertNotNull(whsOrderBO2);
				AssertEquals(workOrder.PK, whsOrderBO2.PK);
				AssertEquals("Total Cubic should be updated.", 70.628m, whsOrderBO2.WD_TotalCubic);
				AssertEquals("Total Cubic Unit should be updated.", "CF", whsOrderBO2.WD_TotalCubicUnit);
				AssertEquals("Total Weight should be updated.", 60000m, whsOrderBO2.WD_TotalWeight);
				AssertEquals("Total Weight Unit should be updated.", "G", whsOrderBO2.WD_TotalWeightUnit);
			});

			shipment.Order.TotalLineVolume = 10m;
			shipment.Order.TotalLineWeight = 5m;
			var reader3 = GetNewReader(shipment, Logger);
			var whsOrderBO3 = reader3.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertNotNull(whsOrderBO3);
				AssertEquals(workOrder.PK, whsOrderBO3.PK);
				AssertEquals("Should *not* have calculated total weight.", 10m, whsOrderBO3.WD_TotalCubic);
				AssertEquals("Should *not* have calculated total cubic.", 5m, whsOrderBO3.WD_TotalWeight);
			});
		}

		public void TestReadIntoBusinessObject_CalculateTotalsIfNeeded_Picked_Assembly()
		{
			var factory2 = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory2);
			var org = helper.CreateClient();
			var whs = helper.CreateWarehouse("A", "A");
			var mainPart = helper.CreateProduct(org, "MainPart");
			var subPart1 = helper.CreateProduct(org, "SubPart1");
			var subPart2 = helper.CreateProduct(org, "SubPart2");
			var level3Part1 = helper.CreateProduct(org, "Level3Part1");
			var level3Part2 = helper.CreateProduct(org, "Level3Part2");
			helper.SetProductWeightAndVolume(subPart1, 5, "KG", 0.5, "M3");
			helper.SetProductWeightAndVolume(subPart2, 10, "KG", 0.1, "M3");
			helper.SetProductWeightAndVolume(level3Part1, 6, "KG", 0.3, "M3");
			helper.SetProductWeightAndVolume(level3Part2, 5, "KG", 0.2, "M3");
			helper.CreateProductBOM(mainPart, subPart1, 2m, Constants.PkgUnit.Unit);
			helper.CreateProductBOM(mainPart, subPart2, 1m, Constants.PkgUnit.Unit);
			helper.CreateProductBOM(subPart1, level3Part1, 1m, Constants.PkgUnit.Unit);
			helper.CreateProductBOM(subPart1, level3Part2, 1m, Constants.PkgUnit.Unit);
			factory2.Save();

			helper.CreateWhsReceiveWithInventory(org, whs, "R1", subPart1, 100m);
			helper.CreateWhsReceiveWithInventory(org, whs, "R2", subPart2, 100m);
			factory2.Save();

			var workOrder = helper.CreateWhsWorkOrder(org, whs);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;

			var workOrderLine1 = workOrder.Lines.AddNew();
			workOrderLine1.WE_OP = mainPart.PK;
			workOrderLine1.WE_TransactionQuantity = 1m;

			var workOrderLine2 = workOrder.Lines.AddNew();
			workOrderLine2.WE_OP = mainPart.PK;
			workOrderLine2.WE_TransactionQuantity = 1m;
			factory2.Save();

			helper.CreatePickNew(workOrder);
			workOrder.WD_TotalCubic = 1m;
			workOrder.WD_TotalWeight = 2m;
			workOrder.WD_TotalUnits = 3m;
			factory2.Save();

			var shipment = new WhsWorkOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, workOrder))).GetDataObject(workOrder);

			shipment.TotalVolumeUnit = new UnitOfVolume { Code = "M3" };
			shipment.TotalWeightUnit = new UnitOfWeight { Code = "KG" };
			shipment.Order.TotalLineVolume = null;
			shipment.Order.TotalLineWeight = null;
			shipment.Order.TotalUnits = null;
			var reader1 = GetNewReader(shipment, Logger);
			var whsOrderBO1 = reader1.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertNotNull(whsOrderBO1);
				AssertEquals(workOrder.PK, whsOrderBO1.PK);
				AssertEquals("Total Cubic should *not* be updated.", 1m, whsOrderBO1.WD_TotalCubic);
				AssertEquals("Total Weight should *not* be updated.", 2m, whsOrderBO1.WD_TotalWeight);
				AssertEquals("Total Units should *not* be updated.", 3m, whsOrderBO1.WD_TotalUnits);
			});

			shipment.Order.TotalLineVolume = 5m;
			shipment.Order.TotalLineWeight = 10m;
			shipment.Order.TotalUnits = 20m;
			var reader2 = GetNewReader(shipment, Logger);
			var whsOrderBO2 = reader2.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertNotNull(whsOrderBO2);
				AssertEquals(workOrder.PK, whsOrderBO2.PK);
				AssertEquals("Should have updated total cubic.", 5m, whsOrderBO2.WD_TotalCubic);
				AssertEquals("Should have updated total weight.", 10m, whsOrderBO2.WD_TotalWeight);
				AssertEquals("Should have updated total units.", 20m, whsOrderBO2.WD_TotalUnits);
			});
		}

		public void TestReadIntoBusinessObject_CalculateTotalsIfNeeded_Picked_Disassembly()
		{
			var factory2 = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory2);
			var org = helper.CreateClient();
			var whs = helper.CreateWarehouse("A", "A");
			var mainPart = helper.CreateProduct(org, "MainPart");
			var subPart1 = helper.CreateProduct(org, "SubPart1");
			var subPart2 = helper.CreateProduct(org, "SubPart2");
			var level3Part1 = helper.CreateProduct(org, "Level3Part1");
			var level3Part2 = helper.CreateProduct(org, "Level3Part2");
			helper.SetProductWeightAndVolume(mainPart, 30, "KG", 1, "M3");
			helper.CreateProductBOM(mainPart, subPart1, 2m, Constants.PkgUnit.Unit);
			helper.CreateProductBOM(mainPart, subPart2, 1m, Constants.PkgUnit.Unit);
			helper.CreateProductBOM(subPart1, level3Part1, 1m, Constants.PkgUnit.Unit);
			helper.CreateProductBOM(subPart1, level3Part2, 1m, Constants.PkgUnit.Unit);
			factory2.Save();

			helper.CreateWhsReceiveWithInventory(org, whs, "R1", mainPart, 100m);
			factory2.Save();

			var workOrder = helper.CreateWhsWorkOrder(org, whs);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			var workOrderLine1 = workOrder.Lines.AddNew();
			workOrderLine1.WE_OP = mainPart.PK;
			workOrderLine1.WE_TransactionQuantity = 1m;

			var workOrderLine2 = workOrder.Lines.AddNew();
			workOrderLine2.WE_OP = mainPart.PK;
			workOrderLine2.WE_TransactionQuantity = 1m;
			factory2.Save();

			helper.CreatePickNew(workOrder);
			workOrder.WD_TotalCubic = 1m;
			workOrder.WD_TotalWeight = 2m;
			workOrder.WD_TotalUnits = 3m;
			factory2.Save();

			var shipment = new WhsWorkOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, workOrder))).GetDataObject(workOrder);

			shipment.TotalVolumeUnit = new UnitOfVolume { Code = "M3" };
			shipment.TotalWeightUnit = new UnitOfWeight { Code = "KG" };
			shipment.Order.TotalLineVolume = null;
			shipment.Order.TotalLineWeight = null;
			shipment.Order.TotalUnits = null;
			var reader1 = GetNewReader(shipment, Logger);
			var whsOrderBO1 = reader1.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertNotNull(whsOrderBO1);
				AssertEquals(workOrder.PK, whsOrderBO1.PK);
				AssertEquals("Total Cubic should *not* be updated.", 1m, whsOrderBO1.WD_TotalCubic);
				AssertEquals("Total Weight should *not* be updated.", 2m, whsOrderBO1.WD_TotalWeight);
				AssertEquals("Total Units should *not* be updated.", 3m, whsOrderBO1.WD_TotalUnits);
			});

			shipment.Order.TotalLineVolume = 5m;
			shipment.Order.TotalLineWeight = 10m;
			shipment.Order.TotalUnits = 20m;
			var reader2 = GetNewReader(shipment, Logger);
			var whsOrderBO2 = reader2.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertNotNull(whsOrderBO2);
				AssertEquals(workOrder.PK, whsOrderBO2.PK);
				AssertEquals("Should have updated total cubic.", 5m, whsOrderBO2.WD_TotalCubic);
				AssertEquals("Should have updated total weight.", 10m, whsOrderBO2.WD_TotalWeight);
				AssertEquals("Should have updated total units.", 20m, whsOrderBO2.WD_TotalUnits);
			});
		}

		#endregion

		#region TestReadIntoBusinessObject_DocketSubType

		public void TestReadIntoBusinessObject_DocketSubType_Assembly()
			=> TestReadIntoBusinessObject_DocketSubType("ASS", "Assemble");

		public void TestReadIntoBusinessObject_DocketSubType_Disassembly()
			=> TestReadIntoBusinessObject_DocketSubType("DIS", "Disassemble");

		public void TestReadIntoBusinessObject_DocketSubType(string code, string description)
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			Factory.SaveForTesting();

			var order = ShipmentDataObject.Order;
			order.Type = new CodeDescriptionPair { Code = code, Description = description };

			var reader = new WhsWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsWorkOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsWorkOrderBO);

			CombineAssertions(() =>
			{
				AssertEquals("whsWorkOrderBO.WD_DocketSubType", code, whsWorkOrderBO.WD_DocketSubType);
			});
		}

		#endregion

		#region TestReadIntoBusinessObject_AutoFinaliseBOMIntoInventory

		public void TestReadIntoBusinessObject_AutoFinaliseBOMIntoInventory_False()
			=> TestReadIntoBusinessObject_AutoFinaliseBOMIntoInventory(autoFinaliseBOMIntoInventory: false);

		public void TestReadIntoBusinessObject_AutoFinaliseBOMIntoInventory_True()
			=> TestReadIntoBusinessObject_AutoFinaliseBOMIntoInventory(autoFinaliseBOMIntoInventory: true);

		public void TestReadIntoBusinessObject_AutoFinaliseBOMIntoInventory(bool autoFinaliseBOMIntoInventory)
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			Factory.SaveForTesting();

			var order = ShipmentDataObject.Order;
			order.AutoFinaliseBOMIntoInventory = autoFinaliseBOMIntoInventory;

			var reader = new WhsWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsWorkOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsWorkOrderBO);

			CombineAssertions(() =>
			{
				AssertEquals("whsWorkOrderBO.WD_AutoFinaliseBOMIntoInventory", autoFinaliseBOMIntoInventory, whsWorkOrderBO.WD_AutoFinaliseBOMIntoInventory);
			});
		}

		#endregion

		#region TestReadIntoBusinessObject_VirtualWarehouse

		public void TestReadIntoBusinessObject_VirtualWarehouse()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();
			warehouse.WW_IsVirtualWarehouse = true;
			Factory.SaveForTesting();

			var order = ShipmentDataObject.Order;

			var reader = new WhsWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsWorkOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsWorkOrderBO);

			CombineAssertions(() =>
			{
				AssertEquals("whsWorkOrderBO.WD_IsInwardsProcessingJob", false, whsWorkOrderBO.WD_IsInwardsProcessingJob);
				AssertEquals("Should not be finalized.", false, whsWorkOrderBO.IsFinalised);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Warehouse {1} from UniversalShipment.
".Trim(), nameof(WhsWorkOrder), GetDocketType()), Logger.Logs);
			});
		}

		#endregion

		#region TestReadIntoBusinessObject_IsInwardProcessingJob

		public void TestReadIntoBusinessObject_IsInwardProcessingJob_False()
			=> TestReadIntoBusinessObject_IsInwardProcessingJob(isInwardProcessingJob: false);

		public void TestReadIntoBusinessObject_IsInwardProcessingJob_True()
			=> TestReadIntoBusinessObject_IsInwardProcessingJob(isInwardProcessingJob: true);

		void TestReadIntoBusinessObject_IsInwardProcessingJob(bool isInwardProcessingJob)
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			Factory.SaveForTesting();

			var order = ShipmentDataObject.Order;
			order.IsInwardsProcessingJob = isInwardProcessingJob;

			var reader = new WhsWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsWorkOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsWorkOrderBO);

			CombineAssertions(() =>
			{
				AssertEquals("whsWorkOrderBO.WD_IsInwardsProcessingJob", isInwardProcessingJob, whsWorkOrderBO.WD_IsInwardsProcessingJob);
				AssertEquals("Should not be finalized.", false, whsWorkOrderBO.IsFinalised);
				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Warehouse {1} from UniversalShipment.
".Trim(), nameof(WhsWorkOrder), GetDocketType()), Logger.Logs);
			});
		}

		#endregion

		#region TestReadIntoBusinessObject_IsInwardProcessingJob_Virtual_Finalizes

		public void TestReadIntoBusinessObject_IsInwardProcessingJob_Virtual_Finalizes()
			=> TestReadIntoBusinessObject_IsInwardProcessingJob_Virtual_Finalizes(autoFinaliseBOMIntoInventory: false);

		[TestDate(2022, 12, 22)]
		public void TestReadIntoBusinessObject_IsInwardsProcessingJob_Virtual_Finalizes_RequiredDateEmpty()
			=> TestReadIntoBusinessObject_IsInwardProcessingJob_Virtual_Finalizes(autoFinaliseBOMIntoInventory: false, requiredDateSet: false);

		public void TestReadIntoBusinessObject_IsInwardProcessingJob_Virtual_Finalizes_ReceiveFinalized()
			=> TestReadIntoBusinessObject_IsInwardProcessingJob_Virtual_Finalizes(autoFinaliseBOMIntoInventory: true);

		void TestReadIntoBusinessObject_IsInwardProcessingJob_Virtual_Finalizes(bool autoFinaliseBOMIntoInventory, bool requiredDateSet = true)
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			WhsWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			Factory.SaveForTesting();
			AssertEquals("Precondition", true, warehouse.IsInwardProcessingEnabled);

			var hatRibbon = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "HATRIB"));
			var receive = Helper.CreateWhsReceiveWithInventory(Data.Orgs.CRAHOLSYD, warehouse, "R1", hatRibbon, 10m, allocateLocations: false, finalise: false);
			receive.WD_DocketSubType = "CUS";
			receive.WD_IsInwardsProcessingJob = true;
			receive.Lines[0].WE_WL = warehouse.DefaultLocationInInwardProcessingArea.PK;
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.SaveForTesting();

			var order = ShipmentDataObject.Order;
			order.IsInwardsProcessingJob = true;
			order.AutoFinaliseBOMIntoInventory = autoFinaliseBOMIntoInventory;
			ShipmentDataObject.LocalProcessing = requiredDateSet ? new LocalProcessing { DeliveryRequiredBy = new ZDateTime(2021, 1, 1) } : null;

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.OrderedQty = 10m;
			orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });

			var reader = new WhsWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory);

			WhsWorkOrder whsWorkOrderBO;
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				whsWorkOrderBO = reader.ReadIntoBusinessObject();
			}

			AssertNotNull(whsWorkOrderBO);

			CombineAssertions(() =>
			{
				AssertEquals("whsWorkOrderBO.WD_IsInwardsProcessingJob", true, whsWorkOrderBO.WD_IsInwardsProcessingJob);
				AssertEquals("whsWorkOrderBO.WD_AutoFinaliseBOMIntoInventory", autoFinaliseBOMIntoInventory, whsWorkOrderBO.WD_AutoFinaliseBOMIntoInventory);
				AssertEquals("whsWorkOrderBO.WD_RequiredDate", requiredDateSet ? new ZDateTimeOffset(2021, 1, 1) : ZDateTimeOffset.Now, whsWorkOrderBO.WD_RequiredDate);

				AssertEquals("Should be finalized.", true, whsWorkOrderBO.IsFinalised);
				AssertEquals("Should be finalized.", true, whsWorkOrderBO.Pick.IsFinalised);
				AssertNotNull("Should have created a receive.", whsWorkOrderBO.Receive);
				AssertEquals("Receive finalization.", autoFinaliseBOMIntoInventory, whsWorkOrderBO.Receive.IsFinalised);

				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - No matching {0} found, creating new {0}.
Information - Populating {0}...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching {2} found, creating new {2}.
Information - Populating {2}...
Information - Added Warehouse {1} from UniversalShipment.
".Trim(), nameof(WhsWorkOrder), GetDocketType(), nameof(WhsWorkOrderLine)), Logger.Logs);
			});
		}

		public void TestReadIntoBusinessObject_IsInwardProcessingJob_Virtual_Finalizes_RejectsImportIfShortfall()
			=> TestReadIntoBusinessObject_IsInwardProcessingJob_Virtual_Finalizes_RejectsImportIfShortfall(completelyShort: false);

		public void TestReadIntoBusinessObject_IsInwardProcessingJob_Virtual_Finalizes_RejectsImportIfShortfall_CompletelyShort()
			=> TestReadIntoBusinessObject_IsInwardProcessingJob_Virtual_Finalizes_RejectsImportIfShortfall(completelyShort: true);

		void TestReadIntoBusinessObject_IsInwardProcessingJob_Virtual_Finalizes_RejectsImportIfShortfall(bool completelyShort)
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			WhsWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			Factory.SaveForTesting();
			AssertEquals("Precondition", true, warehouse.IsInwardProcessingEnabled);

			if (!completelyShort)
			{
				var hatRibbon = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "HATRIB"));
				var receive = Helper.CreateWhsReceiveWithInventory(Data.Orgs.CRAHOLSYD, warehouse, "R1", hatRibbon, 1m, allocateLocations: false, finalise: false);
				receive.WD_DocketSubType = "CUS";
				receive.WD_IsInwardsProcessingJob = true;
				receive.Lines[0].WE_WL = warehouse.DefaultLocationInInwardProcessingArea.PK;
				receive.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(receive);
				Factory.SaveForTesting();
			}

			var order = ShipmentDataObject.Order;
			order.IsInwardsProcessingJob = true;
			ShipmentDataObject.LocalProcessing = new LocalProcessing { DeliveryRequiredBy = new ZDateTime(2021, 1, 1) };

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.OrderedQty = 10m;
			orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				var reader = new WhsWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
				AssertExceptionThrown<DataObjectReadFailureException>(
					"Should have thrown an exception.",
					$@"Cannot Import Work Order
Work Order could not be created for Warehouse Work Order because there are errors:
You do not have enough stock to fulfill shortfalls on this order
Product HATRIB/Bowler Hat Ribbon can not be ordered due to lack of stock. 10 was ordered, but {(completelyShort ? "0" : "1")} is available",
					() => reader.ReadIntoBusinessObject());
			}
		}

		public void TestReadIntoBusinessObject_IsInwardProcessingJob_Virtual_Finalizes_DocketValidation()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			WhsWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			Factory.SaveForTesting();
			AssertEquals("Precondition", true, warehouse.IsInwardProcessingEnabled);

			var hatRibbon = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "HATRIB"));
			var receive = Helper.CreateWhsReceiveWithInventory(Data.Orgs.CRAHOLSYD, warehouse, "R1", hatRibbon, 10m, allocateLocations: false, finalise: false);
			receive.WD_DocketSubType = "CUS";
			receive.WD_IsInwardsProcessingJob = true;
			receive.Lines[0].WE_WL = warehouse.DefaultLocationInInwardProcessingArea.PK;
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.SaveForTesting();

			var order = ShipmentDataObject.Order;
			order.IsInwardsProcessingJob = true;
			order.TotalUnits = 20m;
			ShipmentDataObject.LocalProcessing = new LocalProcessing { DeliveryRequiredBy = new ZDateTime(2021, 1, 1) };

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.OrderedQty = 10m;
			orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });

			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				var reader = new WhsWorkOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
				AssertExceptionThrown<DataObjectReadFailureException>(
					"Should have thrown an exception.",
					@"Cannot Import Work Order
Work Order could not be finalized into the Warehouse for Warehouse Work Order because of the following error(s):
Error: Finalise
Error - WD_TotalUnits: Total Units 20 does not equal the total of all assembly line units (including secondary products) 10.",
					() => reader.ReadIntoBusinessObject());
			}
		}

		public void TestReadIntoBusinessObject_IsInwardsProcessingJob_Virtual_Finalizes_DocketValidation_DoesNotFinaliseExistingDockets()
		{
			var factory2 = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory2);
			var data = new TestDataSimpleEnvironment(factory2);
			helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var secondaryPart1 = helper.CreateProduct("P3", data.Org1);
			var secondaryPart2 = helper.CreateProduct("P4", data.Org1);
			helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			helper.CreateSecondaryProduct(data.Part2, secondaryPart1, 3m);
			helper.CreateSecondaryProduct(data.Part2, secondaryPart2, 4m);

			var area = helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			factory2.Save();

			var componentReceive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, "BEK-1", allocateLocations: false, finalise: false);
			componentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive.WD_IsInwardsProcessingJob = true;
			componentReceive.Lines[0].WE_WL = location.PK;
			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);
			factory2.Save();

			var workOrder = helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;

			var productParam = workOrder.Lines[0].Product.ParamsByWhsAndClient.AddNew();
			productParam.W3_OH = data.Org1.PK;
			productParam.W3_WW = data.Whs1.PK;
			productParam.W3_WL_InwardsProcessingStagingLocationBOM = location.PK;
			factory2.Save();

			AssertEquals("Precondition", 0m, workOrder.WD_TotalUnits);

			var shipment = new WhsWorkOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, workOrder))).GetDataObject(workOrder);
			workOrder.Lines.DeleteAll(); // To ensure we are definitely re-reading lines

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				var reader1 = GetNewReader(shipment, Logger);
				var whsOrderBO1 = reader1.ReadIntoBusinessObject();

				AssertNotNull(whsOrderBO1);
				AssertEquals(workOrder.PK, whsOrderBO1.PK);
				AssertEquals("Should not have finalized existing docket.", false, whsOrderBO1.IsFinalised);
			}
		}

		public void TestReadIntoBusinessObject_IsInwardProcessingJob_Virtual_Finalizes_WhenPickIsNotFinalisedAfterFinaliseDocket()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			WhsWorkOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			Factory.SaveForTesting();
			AssertEquals("Precondition", true, warehouse.IsInwardProcessingEnabled);

			var hatRibbon = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "HATRIB"));
			var receive = Helper.CreateWhsReceiveWithInventory(Data.Orgs.CRAHOLSYD, warehouse, "R1", hatRibbon, 10m, allocateLocations: false, finalise: false);
			receive.WD_DocketSubType = "CUS";
			receive.WD_IsInwardsProcessingJob = true;
			receive.Lines[0].WE_WL = warehouse.DefaultLocationInInwardProcessingArea.PK;
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.SaveForTesting();

			var order = ShipmentDataObject.Order;
			order.IsInwardsProcessingJob = true;
			order.OrderNumber = "B123";
			ShipmentDataObject.LocalProcessing = new LocalProcessing { DeliveryRequiredBy = new ZDateTime(2021, 1, 1) };

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.OrderedQty = 10m;
			orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				var reader = new TestWhsWorkOrderDataObjectReaderWithPickFailedToFinalise(ShipmentDataObject, Logger, Factory);
				AssertExceptionThrown<DataObjectReadFailureException>(
					"Should have thrown an exception.",
					@"Cannot finalize pick
Order External Reference: B123 Failed to Finalize Pick.
Order Line Product: BOWLHAT
Error - Docket Line: Something is wrong, that's the reason why pick is not finalised.",
					() => reader.ReadIntoBusinessObject());
			}
		}

		class TestWhsWorkOrderDataObjectReaderWithPickFailedToFinalise : WhsWorkOrderDataObjectReader
		{
			internal TestWhsWorkOrderDataObjectReaderWithPickFailedToFinalise(Shipment whsOrderDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
				: base(whsOrderDataObject, logger, factory)
			{
			}

			protected override WhsWorkOrder GetNewBusinessObject()
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

		#region Implementation

		protected override string GetDocketType() => "Work Order";

		protected override string GetProcessType() => WorkflowDescriptors.WhsWorkOrderWorkflowDescriptorCode;

		protected override bool SupportsWorkflowCustomFieldImport => false;

		protected override DataContextType DataContext => DataContextType.WarehouseWorkOrder;

		protected override WhsWorkOrder CreateDocketWithLine(OrgHeader client, WhsWarehouse warehouse, OrgSupplierPart product, ZDecimal units)
		{
			return Helper.CreateWhsWorkOrderWithLine(client, warehouse, "WO1", product, units);
		}

		protected override WhsWorkOrder GetNewDocket(OrgHeader client, WhsWarehouse warehouse, string externalReference = "")
		{
			return Helper.CreateWhsWorkOrder(client, warehouse, externalReference);
		}

		protected override WhsDocket GetNewDocketOfDifferentType() => Factory.NewWithValidTestData<WhsOrder>();

		protected override WhsWorkOrderDataObjectReader GetNewReader(Shipment shipmentDataObject, IXmlImportLogger logger, bool useCleanFactory = false)
		{
			return new WhsWorkOrderDataObjectReader(shipmentDataObject, logger, useCleanFactory ? new UniversalObjectFactory() : Factory);
		}

		protected override Shipment GetNewDocketDataObject(WhsWorkOrder docket)
		{
			var writer = new WhsWorkOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, docket)));
			var result = writer.GetDataObject(docket);
			result.DataContext.AddDataTarget(((ITopLevelDataObjectWriter)writer).TopLevelDataContextType, docket.WD_DocketID);
			Logger.TopLevelDataObject = result;
			return result;
		}

		#endregion
	}
}
