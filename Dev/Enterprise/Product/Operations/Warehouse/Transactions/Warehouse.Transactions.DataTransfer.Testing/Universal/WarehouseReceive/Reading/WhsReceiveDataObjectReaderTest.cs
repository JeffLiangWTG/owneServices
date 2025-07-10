using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsReceiveDataObjectReaderTest : WhsOrderAndReceiveDataObjectReaderTest<WhsReceive, WhsReceiveLine, WhsReceiveDataObjectReader>
	{
		#region TestBasicReceiveLevelFieldMappings

		public void TestWD_CustomerReferenceFallbackMapping()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var whsReceiveBO = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject();

			AssertEquals("", whsReceiveBO.WD_CustomerReference);

			ShipmentDataObject.OwnerRef = "OwnerRef";

			var newWhsOrderBO = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject();

			AssertEquals("OwnerRef", newWhsOrderBO.WD_CustomerReference);

			ShipmentDataObject.Order.ClientReference = "ClientRef";
			var newWhsOrderBO2 = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject();

			AssertEquals("ClientRef", newWhsOrderBO2.WD_CustomerReference);
		}

		public void TestBasicReceiveLevelFieldMappings()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.CarrierServiceLevel = new ServiceLevel { Code = "SET", Description = "SITTLE" };
			ShipmentDataObject.ServiceLevel = new ServiceLevel { Code = "TSL", Description = "Test Service Level" };
			ShipmentDataObject.OuterPacks = 3;
			ShipmentDataObject.OuterPacksPackageType = new PackageType { Code = "CTN", Description = "CTN" };
			ShipmentDataObject.TotalVolume = 32.1m;
			ShipmentDataObject.TotalVolumeUnit = new UnitOfVolume { Code = "CY", Description = "Cubic Yards" };
			ShipmentDataObject.TotalWeight = 41.2m;
			ShipmentDataObject.TotalWeightUnit = new UnitOfWeight { Code = "LB", Description = "Pounds" };
			ShipmentDataObject.WayBillNumber = "BILL";
			ShipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };
			ShipmentDataObject.DataContext.CodesMappedToTarget = true;
			ShipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>() { new AdditionalReference { Type = new EntryType { Code = "HSB" }, ReferenceNumber = "BILL" } });

			var receive = ShipmentDataObject.Order;
			receive.ClientReference = "CUSTOMER";
			receive.SetDateCollection(() => new List<Date>());
			receive.DateCollection.Add(Date.New(DateType.Arrival, ZBool.True, new ZDateTime(2011, 1, 1)));
			receive.DateCollection.Add(Date.New(DateType.Arrival, ZBool.False, new ZDateTime(2011, 1, 2)));
			receive.DateCollection.Add(Date.New(DateType.Departure, ZBool.True, new ZDateTime(2011, 1, 3)));
			receive.DateCollection.Add(Date.New(DateType.BookingConfirmed, ZBool.False, new ZDateTime(2011, 1, 4)));
			receive.DropMode = new DropMode { Code = "DRO", Description = "Drop" };
			receive.OrderNumber = "ORDERME";
			receive.OrderNumberSplit = new ZByte(1);
			receive.PalletsSent = new ZShort(4);
			receive.Status = new CodeDescriptionPair { Code = "PUT", Description = "Put Away" };
			receive.TotalLineVolume = 11.2m;
			receive.TotalLineWeight = 15.3m;
			receive.TotalUnits = 12.3m;
			receive.TransportReference = "TRANS";
			receive.Type = new CodeDescriptionPair { Code = "CUS", Description = "CUSTOMS RECEIPT" };
			receive.HoldPalletIDPutaway = true;

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsReceiveBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);

			AssertNotNull(whsReceiveBO);

			CombineAssertions(delegate
			{
				AssertContents(whsReceiveBO);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsReceive found, creating new WhsReceive.
Information - Populating WhsReceive...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsDocketReference found, creating new WhsDocketReference.
Information - Populating WhsDocketReference...
Information - Added Warehouse Receipt from UniversalShipment.
Information - Successfully saved Warehouse Receipt W00000001 with 1 x WhsDocketReference.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestTotalWeightAndVolumeCalculationsFromLines

		#region TestTotalWeightAndVolume_UXMLEmpty_NoExistingDocket

		public void TestTotalWeightAndVolume_UXMLEmpty_NoExistingDocket()
		{
			var product = SetupProductWithWeightAndVolume();
			Factory.SaveForTesting();

			SetupUXMLWithTwoLinesTotaling_11_Units(product, 0, 0);

			RunAndAssertTotals(0.004m * 11, 0.5m * 11);
		}

		#endregion

		#region TestTotalWeightAndVolume_UXMLEmpty_ExistingDocketZeroTotals

		public void TestTotalWeightAndVolume_UXMLEmpty_ExistingDocketZeroTotals()
		{
			var product = SetupProductWithWeightAndVolume();
			SetupExistingReceive(0, 0, "W1");
			Factory.SaveForTesting();

			SetupUXMLWithTwoLinesTotaling_11_Units(product, 0, 0, "W1");

			RunAndAssertTotals(0.004m * 11, 0.5m * 11);
		}

		#endregion

		#region TestTotalWeightAndVolume_UXMLEmpty_ExistingDocketPositiveTotals

		public void TestTotalWeightAndVolume_UXMLEmpty_ExistingDocketPositiveTotals()
		{
			var product = SetupProductWithWeightAndVolume();
			SetupExistingReceive(8, 15, "W1");
			Factory.SaveForTesting();

			SetupUXMLWithTwoLinesTotaling_11_Units(product, 0, 0, "W1");

			RunAndAssertTotals(0.004m * 11, 0.5m * 11);
		}

		#endregion

		#region TestTotalWeightAndVolume_UXMLFilled_NoExistingDocket

		public void TestTotalWeightAndVolume_UXMLFilled_NoExistingDocket()
		{
			var product = SetupProductWithWeightAndVolume();
			Factory.SaveForTesting();

			SetupUXMLWithTwoLinesTotaling_11_Units(product, 3, 12);

			RunAndAssertTotals(3, 12);
		}

		#endregion

		#region TestTotalWeightAndVolume_UXMLFilled_ExistingDocketZeroTotals

		public void TestTotalWeightAndVolume_UXMLFilled_ExistingDocketZeroTotals()
		{
			var product = SetupProductWithWeightAndVolume();
			SetupExistingReceive(0, 0, "W1");
			Factory.SaveForTesting();

			SetupUXMLWithTwoLinesTotaling_11_Units(product, 3, 12, "W1");

			RunAndAssertTotals(3, 12);
		}

		#endregion

		#region TestTotalWeightAndVolume_UXMLFilled_ExistingDocketPositiveTotals

		public void TestTotalWeightAndVolume_UXMLFilled_ExistingDocketPositiveTotals()
		{
			var product = SetupProductWithWeightAndVolume();
			SetupExistingReceive(8, 15, "W1");
			Factory.SaveForTesting();

			SetupUXMLWithTwoLinesTotaling_11_Units(product, 3, 12, "W1");

			RunAndAssertTotals(3, 12);
		}

		#endregion

		#region TestTotalUnits

		public void TestTotalUnits_OrderNotExists()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);
			var product = Data.Product;
			AddCommericalInvoiceLines(Data, product, 10m, 5m);

			using (ObjectFactory.Substitute(PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive().Object))
			{
				AssertNull("Procondition:", ShipmentDataObject.Order);
				var importResults = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
				var receive = (WhsReceive)importResults.Single(r => r.DataContextType == DataContextType.WarehouseReceive).GetBizOForTesting(Data.ShipmentDataObject, new BusinessObjectFactory());
				AssertEquals("Docket total units should be the sum of lines' units.", 31m, receive.WD_TotalUnits);
			}
		}

		public void TestTotalUnits_OrderExists()
		{
			var product = SetupProductWithWeightAndVolume();
			SetupUXMLWithTwoLinesTotaling_11_Units(product, 3, 12, "W1");
			ShipmentDataObject.Order.TotalUnits = 10m;

			AssertNotNull("Procondition:", ShipmentDataObject.Order);
			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsReceiveBO = reader.ReadIntoBusinessObject();
			AssertNotNull(whsReceiveBO);
			AssertEquals("Receive total units should be populated from the Shipment.Order.TotalUnits", 10m, whsReceiveBO.WD_TotalUnits);
		}

		#endregion

		#region Helpers

		OrgSupplierPart SetupProductWithWeightAndVolume()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var product = Data.Product; // poke to save to database

			product.OP_Cubic = 4m;
			product.OP_CubicUQ = "L";
			product.OP_Weight = 500m;
			product.OP_WeightUQ = "G";

			return product;
		}

		void SetupExistingReceive(ZDecimal wD_TotalCubic, ZDecimal wD_TotalWeight, string reference)
		{
			var receive = WhsReceiveLineDataObjectReaderTest.GetReceiveLineParent(Factory);
			receive.WD_TotalCubic = wD_TotalCubic;
			receive.WD_TotalWeight = wD_TotalWeight;
			receive.WD_ExternalReference = reference;
		}

		void SetupUXMLWithTwoLinesTotaling_11_Units(OrgSupplierPart product, ZDecimal totalVolume, ZDecimal totalWeight, string reference = "")
		{
			var inventoryLineDataObject = WhsReceiveLineDataObjectReaderTest.SetupInventoryLine();
			inventoryLineDataObject.OrderedQty = 3;
			inventoryLineDataObject.ExpectedQuantity = 3;
			inventoryLineDataObject.Product.Code = product.OP_PartNum;
			var inventoryLineDataObject2 = WhsReceiveLineDataObjectReaderTest.SetupInventoryLine();
			inventoryLineDataObject2.OrderedQty = 8;
			inventoryLineDataObject2.ExpectedQuantity = 8;
			inventoryLineDataObject2.Product.Code = product.OP_PartNum;

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine> { inventoryLineDataObject, inventoryLineDataObject2 });
			ShipmentDataObject.Order.Type = new CodeDescriptionPair { Code = "CUS" };
			// set totals and reference according test requirements
			ShipmentDataObject.Order.TotalLineVolume = totalVolume;
			ShipmentDataObject.Order.TotalLineWeight = totalWeight == 0 ? null : totalWeight;
			ShipmentDataObject.Order.OrderNumber = reference;
			Logger.ClearLogs();
		}

		void RunAndAssertTotals(ZDecimal expectedWD_TotalCubic, ZDecimal expecteWD_TotalWeight)
		{
			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsReceiveBO = reader.ReadIntoBusinessObject();
			AssertNotNull(whsReceiveBO);
			AssertEquals("whsReceiveBO.Lines.Count", 2, whsReceiveBO.Lines.Count);
			AssertEquals(expectedWD_TotalCubic, whsReceiveBO.WD_TotalCubic);
			AssertEquals("whsReceiveBO.WD_TotalCubicUnit remains Default", "M3", whsReceiveBO.WD_TotalCubicUnit);
			AssertEquals(expecteWD_TotalWeight, whsReceiveBO.WD_TotalWeight);
			AssertEquals("whsReceiveBO.WD_TotalWeightUnit remains Default", "KG", whsReceiveBO.WD_TotalWeightUnit);
		}

		#endregion

		#endregion

		#region TestReadIntoBusinessObjectFromShipment

		public void TestReadIntoBusinessObjectFromShipment_Create()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();

			var whs1 = Data.GetOrCreateWarehouseInDB();
			var whsAddress = whs1.WarehouseAddress;
			whsAddress.Header.OH_IsWarehouseClient = true;

			ShipmentDataObject.CarrierServiceLevel = new ServiceLevel { Code = "SET", Description = "SITTLE" };
			ShipmentDataObject.ServiceLevel = new ServiceLevel { Code = "TSL", Description = "Test Service Level" };
			ShipmentDataObject.OuterPacks = 3;
			ShipmentDataObject.TotalVolume = 32.1m;
			ShipmentDataObject.TotalVolumeUnit = new UnitOfVolume() { Code = "M3" };
			ShipmentDataObject.TotalWeight = 41.2m;
			ShipmentDataObject.TotalWeightUnit = new UnitOfWeight() { Code = "KG" };
			ShipmentDataObject.DataContext.CodesMappedToTarget = true;
			ShipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>() { new AdditionalReference { Type = new EntryType { Code = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference }, ReferenceNumber = "REF00000001" } });
			ShipmentDataObject.OrganizationAddressCollection.Clear();
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ClientAddressDataObject_CRAHOLSYD);
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ConsigneeAddressDataObject_CRAHOLSYD);
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.WarehouseAddressDataObject_INTHEMSYD);
			ShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S1234");

			var clientCRAHOLSYD = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var clientINTHEMSYD = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);

			var product1 = Data.CreateProduct("P1");
			var relation1 = product1.RelatedOrganisations.AddNew();
			relation1.OU_Relationship = "OWN";
			relation1.OU_OP = product1.PK;
			relation1.OU_OH = clientINTHEMSYD.PK;
			var product2 = Data.CreateProduct("P2");
			var relation2 = product2.RelatedOrganisations.AddNew();
			relation2.OU_Relationship = "OWN";
			relation2.OU_OP = product2.PK;
			relation2.OU_OH = clientINTHEMSYD.PK;

			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine1.PackingLineID = "PackingLine0001";
			packingLine1.PackType = new PackageType { Code = "PLT" };
			packingLine1.ReferenceNumber = "PL00000001";
			packingLine1.PackQty = 5L;
			var packItem11 = new PackedItem();
			packItem11.Product = new Product { Code = product1.OP_PartNum };
			packItem11.UnitOfQuantity = new PackageType { Code = "BOX" };
			packItem11.PackedQuantity = 3m;
			packingLine1.SetPackedItemCollection(() => new List<PackedItem> { packItem11 });

			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine2.PackingLineID = "PackingLine0002";
			packingLine2.PackType = new PackageType { Code = "PLT" };
			packingLine2.ReferenceNumber = "PL00000002";
			packingLine2.PackQty = 10L;
			var packItem21 = new PackedItem();
			packItem21.Product = new Product { Code = product1.OP_PartNum };
			packItem21.UnitOfQuantity = new PackageType { Code = "BOX" };
			packItem21.PackedQuantity = 6m;
			var packItem22 = new PackedItem();
			packItem22.Product = new Product { Code = product2.OP_PartNum };
			packItem22.UnitOfQuantity = new PackageType { Code = "UNT" };
			packItem22.PackedQuantity = 9m;
			packingLine2.SetPackedItemCollection(() => new List<PackedItem> { packItem21, packItem22 });

			ShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine1, packingLine2 });
			ShipmentDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			ShipmentDataObject.LocalProcessing.FCLDeliveryEquipmentNeeded = new CodeDescriptionPair { Code = "WUP", Description = "Wait for Pack/Unpack" };

			var shipmentPostCarriage = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			ShipmentDataObject.SetPostCarriageShipmentCollection(() => new List<UniversalShipment>(new[] { shipmentPostCarriage }));

			var shipmentSubShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentPostCarriage.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { shipmentSubShipment });

			var localProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			localProcessing.ArrivalCartageRef = "Cartage Ref";
			shipmentSubShipment.LocalProcessing = localProcessing;

			Factory.SaveForTesting();

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);

			var receive = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);
			AssertNotNull(receive);

			AssertEquals(3, receive.Lines.Count);
			AssertEquals("REF00000001", receive.WD_CustomerReference);
			AssertEquals("WUP", receive.WD_DropMode);
			AssertEquals(41.2m, receive.WD_TotalWeight);
			AssertEquals("KG", receive.WD_TotalWeightUnit);
			AssertEquals(32.1m, receive.WD_TotalCubic);
			AssertEquals("M3", receive.WD_TotalCubicUnit);
			AssertEquals("S1234", receive.WD_ExternalReference);
			AssertEquals(clientCRAHOLSYD.PK, receive.SupplierPK);
			AssertEquals("Cartage Ref", receive.WD_TransportReference);

			var receiveLine1 = receive.Lines.Single(x => x.WE_PalletID == "PL00000001" && x.WE_OP == product1.PK);
			var receiveLine2 = receive.Lines.Single(x => x.WE_PalletID == "PL00000002" && x.WE_OP == product1.PK);
			var receiveLine3 = receive.Lines.Single(x => x.WE_PalletID == "PL00000002" && x.WE_OP == product2.PK);

			AssertEquals("PLT", receiveLine1.WE_F3_NKPackType);
			AssertEquals(5m, receiveLine1.WE_PackQuantity);

			AssertEquals("BOX", receiveLine2.WE_F3_NKPackType);
			AssertEquals(6m, receiveLine2.WE_PackQuantity);

			AssertEquals("UNT", receiveLine3.WE_F3_NKPackType);
			AssertEquals(9m, receiveLine3.WE_PackQuantity);
		}

		public void TestReadIntoBusinessObjectFromShipment_WithUnitConversions()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			ShipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>() { new AdditionalReference { Type = new EntryType { Code = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference }, ReferenceNumber = "REF00000001" } });
			ShipmentDataObject.OrganizationAddressCollection.Clear();
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ClientAddressDataObject_CRAHOLSYD);
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ConsigneeAddressDataObject_CRAHOLSYD);
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.WarehouseAddressDataObject_INTHEMSYD);
			ShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S1234");

			var clientCRAHOLSYD = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var clientINTHEMSYD = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);

			var product1 = Data.CreateProduct(clientCRAHOLSYD, "P1");
			Helper.CreateProductUnit(product1, "BOX", 10m);
			Helper.CreateProductUnit(product1, "BOX", "PLT", 10m);
			var product2 = Data.CreateProduct(clientCRAHOLSYD, "P2");
			Helper.CreateProductUnit(product2, "BOX", 20m);

			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine1.PackingLineID = "PackingLine0001";
			packingLine1.PackType = new PackageType { Code = "PLT" };
			packingLine1.ReferenceNumber = "PL00000001";
			packingLine1.PackQty = 5L;
			var packItem11 = new PackedItem
			{
				Product = new Product { Code = product1.OP_PartNum },
				UnitOfQuantity = new PackageType { Code = "BOX" },
				PackedQuantity = 3m
			};
			packingLine1.SetPackedItemCollection(() => new List<PackedItem> { packItem11 });

			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine2.PackingLineID = "PackingLine0002";
			packingLine2.PackType = new PackageType { Code = "PLT" };
			packingLine2.ReferenceNumber = "PL00000002";
			packingLine2.PackQty = 10L;
			var packItem21 = new PackedItem
			{
				Product = new Product { Code = product1.OP_PartNum },
				UnitOfQuantity = new PackageType { Code = "BOX" },
				PackedQuantity = 6m
			};
			var packItem22 = new PackedItem
			{
				Product = new Product { Code = product2.OP_PartNum },
				UnitOfQuantity = new PackageType { Code = "UNT" },
				PackedQuantity = 9m
			};
			packingLine2.SetPackedItemCollection(() => new List<PackedItem> { packItem21, packItem22 });

			ShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine1, packingLine2 });

			var shipmentPostCarriage = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			ShipmentDataObject.SetPostCarriageShipmentCollection(() => new List<UniversalShipment>(new[] { shipmentPostCarriage }));

			var shipmentSubShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentPostCarriage.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { shipmentSubShipment });

			var localProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			localProcessing.ArrivalCartageRef = "Cartage Ref";
			shipmentSubShipment.LocalProcessing = localProcessing;

			Factory.SaveForTesting();

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);

			var receive = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);
			AssertNotNull(receive);

			AssertEquals(3, receive.Lines.Count);
			AssertEquals("REF00000001", receive.WD_CustomerReference);
			AssertEquals("S1234", receive.WD_ExternalReference);
			AssertEquals(clientCRAHOLSYD.PK, receive.SupplierPK);
			AssertEquals("Cartage Ref", receive.WD_TransportReference);

			var receiveLine1 = receive.Lines.Single(x => x.WE_PalletID == "PL00000001" && x.WE_OP == product1.PK);
			var receiveLine2 = receive.Lines.Single(x => x.WE_PalletID == "PL00000002" && x.WE_OP == product1.PK);
			var receiveLine3 = receive.Lines.Single(x => x.WE_PalletID == "PL00000002" && x.WE_OP == product2.PK);

			AssertEquals("PLT", receiveLine1.WE_F3_NKPackType);
			AssertEquals("Pack Quantity should be correct.", 5m, receiveLine1.WE_PackQuantity);
			AssertEquals("SKU Quantity should be correct based on Pack Type.", 500m, receiveLine1.WE_TransactionQuantity);

			AssertEquals("BOX", receiveLine2.WE_F3_NKPackType);
			AssertEquals("Pack Quantity should be correct.", 6m, receiveLine2.WE_PackQuantity);
			AssertEquals("SKU Quantity should be correct based on Pack Type.", 60m, receiveLine2.WE_TransactionQuantity);

			AssertEquals("UNT", receiveLine3.WE_F3_NKPackType);
			AssertEquals("Pack Quantity and SKU Quantity should be the same for SKU Pack Type.", 9m, receiveLine3.WE_PackQuantity);
			AssertEquals("Pack Quantity and SKU Quantity should be the same for SKU Pack Type.", 9m, receiveLine3.WE_TransactionQuantity);
		}

		public void TestReadIntoBusinessObjectFromShipment_Override()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();

			var whs1 = Data.GetOrCreateWarehouseInDB();
			var whsAddress = whs1.WarehouseAddress;
			whsAddress.Header.OH_IsWarehouseClient = true;

			var madReference1 = new AdditionalReference { Type = new EntryType { Code = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference }, ReferenceNumber = "REF00000001" };
			var madReference2 = new AdditionalReference { Type = new EntryType { Code = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference }, ReferenceNumber = "REF00000002" };

			ShipmentDataObject.CarrierServiceLevel = new ServiceLevel { Code = "SET", Description = "SITTLE" };
			ShipmentDataObject.ServiceLevel = new ServiceLevel { Code = "TSL", Description = "Test Service Level" };
			ShipmentDataObject.OuterPacks = 3;
			ShipmentDataObject.TotalVolume = 32.1m;
			ShipmentDataObject.TotalVolumeUnit = new UnitOfVolume() { Code = "TE" };
			ShipmentDataObject.TotalWeight = 41.2m;
			ShipmentDataObject.TotalWeightUnit = new UnitOfWeight() { Code = "KG" };
			ShipmentDataObject.DataContext.CodesMappedToTarget = true;
			ShipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>() { madReference1 });
			ShipmentDataObject.OrganizationAddressCollection.Clear();
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ClientAddressDataObject_CRAHOLSYD);
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ConsigneeAddressDataObject_CRAHOLSYD);
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.WarehouseAddressDataObject_INTHEMSYD);
			ShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S1234");

			var clientINTHEMSYD = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);

			var product1 = Data.CreateProduct("P1");
			var relation1 = product1.RelatedOrganisations.AddNew();
			relation1.OU_Relationship = "OWN";
			relation1.OU_OP = product1.PK;
			relation1.OU_OH = clientINTHEMSYD.PK;
			var product2 = Data.CreateProduct("P2");
			var relation2 = product2.RelatedOrganisations.AddNew();
			relation2.OU_Relationship = "OWN";
			relation2.OU_OP = product2.PK;
			relation2.OU_OH = clientINTHEMSYD.PK;

			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine1.PackingLineID = "PackingLine0001";
			packingLine1.PackType = new PackageType { Code = "PLT" };
			packingLine1.ReferenceNumber = "PL00000001";
			packingLine1.PackQty = 5L;
			var packItem11 = new PackedItem();
			packItem11.Product = new Product { Code = product1.OP_PartNum };
			packItem11.UnitOfQuantity = new PackageType { Code = "BOX" };
			packItem11.PackedQuantity = 3m;
			packingLine1.SetPackedItemCollection(() => new List<PackedItem> { packItem11 });

			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine2.PackingLineID = "PackingLine0002";
			packingLine2.PackType = new PackageType { Code = "PLT" };
			packingLine2.ReferenceNumber = "PL00000002";
			packingLine2.PackQty = 10L;
			var packItem21 = new PackedItem();
			packItem21.Product = new Product { Code = product1.OP_PartNum };
			packItem21.UnitOfQuantity = new PackageType { Code = "BOX" };
			packItem21.PackedQuantity = 6m;
			var packItem22 = new PackedItem();
			packItem22.Product = new Product { Code = product2.OP_PartNum };
			packItem22.UnitOfQuantity = new PackageType { Code = "UNT" };
			packItem22.PackedQuantity = 9m;
			packingLine2.SetPackedItemCollection(() => new List<PackedItem> { packItem21, packItem22 });

			ShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine1, packingLine2 });
			ShipmentDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			ShipmentDataObject.LocalProcessing.FCLDeliveryEquipmentNeeded = new CodeDescriptionPair { Code = "WUP", Description = "Wait for Pack/Unpack" };

			Factory.SaveForTesting();

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);

			var receive = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);
			AssertNotNull(receive);

			AssertEquals(3, receive.Lines.Count);
			AssertEquals("REF00000001", receive.WD_CustomerReference);
			AssertEquals("WUP", receive.WD_DropMode);
			AssertEquals(41.2m, receive.WD_TotalWeight);
			AssertEquals("KG", receive.WD_TotalWeightUnit);
			AssertEquals(32.1m, receive.WD_TotalCubic);
			AssertEquals("TE", receive.WD_TotalCubicUnit);
			AssertEquals("S1234", receive.WD_ExternalReference);

			var receiveLine1 = receive.Lines.Single(x => x.WE_PalletID == "PL00000001" && x.WE_OP == product1.PK);
			var receiveLine2 = receive.Lines.Single(x => x.WE_PalletID == "PL00000002" && x.WE_OP == product1.PK);
			var receiveLine3 = receive.Lines.Single(x => x.WE_PalletID == "PL00000002" && x.WE_OP == product2.PK);

			AssertEquals("PLT", receiveLine1.WE_F3_NKPackType);
			AssertEquals(5m, receiveLine1.WE_PackQuantity);

			AssertEquals("BOX", receiveLine2.WE_F3_NKPackType);
			AssertEquals(6m, receiveLine2.WE_PackQuantity);

			AssertEquals("UNT", receiveLine3.WE_F3_NKPackType);
			AssertEquals(9m, receiveLine3.WE_PackQuantity);

			ShipmentDataObject.PackingLineCollection.Remove(packingLine2);
			ShipmentDataObject.TotalWeight = 10m;
			ShipmentDataObject.TotalWeightUnit = new UnitOfWeight() { Code = "T" };
			ShipmentDataObject.TotalVolume = 15m;
			ShipmentDataObject.TotalVolumeUnit = new UnitOfVolume() { Code = "M3" };
			ShipmentDataObject.AdditionalReferenceCollection.Remove(madReference1);
			ShipmentDataObject.AdditionalReferenceCollection.Add(madReference2);
			packingLine1.PackQty = 10L;
			packItem11.Product = new Product { Code = product2.OP_PartNum };

			Factory.SaveForTesting();

			var reader2 = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);

			var receive2 = reader2.ReadIntoBusinessObject();

			AssertEquals(receive.PK, receive2.PK);
			AssertEquals(1, receive2.Lines.Count);
			AssertEquals("REF00000001", receive2.WD_CustomerReference);
			AssertEquals("WUP", receive2.WD_DropMode);
			AssertEquals(10m, receive.WD_TotalWeight);
			AssertEquals("T", receive.WD_TotalWeightUnit);
			AssertEquals(15m, receive.WD_TotalCubic);
			AssertEquals("M3", receive.WD_TotalCubicUnit);
			AssertEquals("S1234", receive2.WD_ExternalReference);

			var receiveLine4 = receive.Lines[0];

			AssertEquals(product2.PK, receiveLine4.WE_OP);
			AssertEquals("PL00000001", receiveLine4.WE_PalletID);
			AssertEquals("PLT", receiveLine4.WE_F3_NKPackType);
			AssertEquals(10m, receiveLine4.WE_PackQuantity);
		}

		public void TestReadIntoBusinessObjectFromShipment_ForContainer()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();

			var whs1 = Data.GetOrCreateWarehouseInDB();
			var whsAddress = whs1.WarehouseAddress;
			whsAddress.Header.OH_IsWarehouseClient = true;

			ShipmentDataObject.CarrierServiceLevel = new ServiceLevel { Code = "SET", Description = "SITTLE" };
			ShipmentDataObject.ServiceLevel = new ServiceLevel { Code = "TSL", Description = "Test Service Level" };
			ShipmentDataObject.OuterPacks = 3;
			ShipmentDataObject.TotalVolume = 32.1m;
			ShipmentDataObject.TotalWeight = 41.2m;
			ShipmentDataObject.DataContext.CodesMappedToTarget = true;
			ShipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>() { new AdditionalReference { Type = new EntryType { Code = "MAD" }, ReferenceNumber = "REF00000001" } });
			ShipmentDataObject.OrganizationAddressCollection.Clear();
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ClientAddressDataObject_CRAHOLSYD);
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ConsigneeAddressDataObject_CRAHOLSYD);
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.WarehouseAddressDataObject_INTHEMSYD);
			ShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S1234");

			var containerType1 = new ContainerType { Code = "AAA", Description = "Container Type A" };
			var containerType2 = new ContainerType { Code = "AAB", Description = "Container Type B" };
			var container1 = new Container() { Link = 1, ContainerNumber = "CONT0001", ContainerType = containerType1, Seal = "ContainerSeal1" };
			var container2 = new Container() { Link = 2, ContainerNumber = "CONT0002", ContainerType = containerType2, Seal = "ContainerSeal2" };
			ShipmentDataObject.SetContainerCollection(() => new DataObjectList<Container> { container1, container2 });
			ShipmentDataObject.ContainerCollection.Content = CollectionContent.Complete;

			var clientINTHEMSYD = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);

			var product1 = Data.CreateProduct("P1");
			var relation1 = product1.RelatedOrganisations.AddNew();
			relation1.OU_Relationship = "OWN";
			relation1.OU_OP = product1.PK;
			relation1.OU_OH = clientINTHEMSYD.PK;

			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine1.PackingLineID = "PackingLine0001";
			packingLine1.PackType = new PackageType { Code = "PLT" };
			packingLine1.ReferenceNumber = "PL00000001";
			packingLine1.PackQty = 5L;
			packingLine1.ContainerNumber = "CONT0001";
			var packItem11 = new PackedItem();
			packItem11.Product = new Product { Code = product1.OP_PartNum };
			packItem11.UnitOfQuantity = new PackageType { Code = "BOX" };
			packItem11.PackedQuantity = 3m;
			packingLine1.SetPackedItemCollection(() => new List<PackedItem> { packItem11 });

			ShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine1 });

			Factory.SaveForTesting();

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);

			var receive = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);
			AssertNotNull(receive);

			AssertEquals(1, receive.Containers.Count);
			var container = receive.Containers[0];
			AssertEquals("CONT0001", container.WC_ContainerNum);
			AssertEquals(Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "AAA").PK, container.WC_RC);
			AssertEquals("ContainerSeal1", container.WC_SealNum);
		}

		public void TestReadIntoBusinessObjectFromShipment_BookingCollectionNull()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();

			var whs1 = Data.GetOrCreateWarehouseInDB();
			var whsAddress = whs1.WarehouseAddress;
			whsAddress.Header.OH_IsWarehouseClient = true;

			ShipmentDataObject.OrganizationAddressCollection.Clear();
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ClientAddressDataObject_CRAHOLSYD);
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ConsigneeAddressDataObject_CRAHOLSYD);
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.WarehouseAddressDataObject_INTHEMSYD);
			ShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S1234");

			var shipmentP = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			ShipmentDataObject.SetPostCarriageShipmentCollection(() => new List<UniversalShipment>(new[] { shipmentP }));

			Factory.SaveForTesting();

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());
		}

		public void TestReadIntoBusinessObjectFromShipment_BookingCollection_LocalProcessingNull()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();

			var whs1 = Data.GetOrCreateWarehouseInDB();
			var whsAddress = whs1.WarehouseAddress;
			whsAddress.Header.OH_IsWarehouseClient = true;

			ShipmentDataObject.OrganizationAddressCollection.Clear();
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ClientAddressDataObject_CRAHOLSYD);
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ConsigneeAddressDataObject_CRAHOLSYD);
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.WarehouseAddressDataObject_INTHEMSYD);
			ShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S1234");

			var shipmentP = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			ShipmentDataObject.SetPostCarriageShipmentCollection(() => new List<UniversalShipment>(new[] { shipmentP }));

			var shipmentL = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentP.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { shipmentL });

			Factory.SaveForTesting();

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());
		}

		public void TestReadIntoBusinessObjectFromShipment_SubShipmentDateCollection()
		{
			var shipmentPostCarriage = TestReadIntoBusinessObjectFromShipment_SubShipmentDateCollectionCore();

			var shipmentSubShipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentSubShipment1.SetDateCollection(() => new List<Date>());
			shipmentSubShipment1.DateCollection.Add(Date.New(DateType.JobCreated, ZBool.False, new ZDateTime(2011, 1, 2)));
			var shipmentSubShipment2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentSubShipment2.SetDateCollection(() => new List<Date>());
			shipmentSubShipment2.DateCollection.Add(Date.New(DateType.JobCreated, ZBool.False, new ZDateTime(2011, 1, 1)));
			shipmentPostCarriage.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { shipmentSubShipment1, shipmentSubShipment2 });

			var localProcessing1 = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			localProcessing1.ArrivalCartageRef = "Cartage Ref1";
			shipmentSubShipment1.LocalProcessing = localProcessing1;

			var localProcessing2 = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			localProcessing2.ArrivalCartageRef = "Cartage Ref2";
			shipmentSubShipment2.LocalProcessing = localProcessing2;

			Factory.SaveForTesting();

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);

			var receive = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);
			AssertNotNull(receive);

			AssertEquals("Cartage Ref1", receive.WD_TransportReference);
		}

		public void TestReadIntoBusinessObjectFromShipment_SubShipmentDateCollection_Empty()
		{
			var shipmentPostCarriage = TestReadIntoBusinessObjectFromShipment_SubShipmentDateCollectionCore();

			var shipmentSubShipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentSubShipment1.SetDateCollection(() => new List<Date>());
			var shipmentSubShipment2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentSubShipment2.SetDateCollection(() => new List<Date>());
			shipmentPostCarriage.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { shipmentSubShipment1, shipmentSubShipment2 });

			var localProcessing1 = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			localProcessing1.ArrivalCartageRef = "Cartage Ref1";
			shipmentSubShipment1.LocalProcessing = localProcessing1;

			var localProcessing2 = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			localProcessing2.ArrivalCartageRef = "Cartage Ref2";
			shipmentSubShipment2.LocalProcessing = localProcessing2;

			Factory.SaveForTesting();

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);

			WhsReceive receive = null;
			AssertNoExceptionThrown(() => receive = reader.ReadIntoBusinessObject());
			Factory.SaveAtEndOfImport(Logger);
			AssertNotNull(receive);

			AssertCollectionContains("Should set to one of the ArrivalCartageRef", receive.WD_TransportReference, new ZString[] { "Cartage Ref1", "Cartage Ref2" });
		}

		public void TestReadIntoBusinessObjectFromShipment_SubShipmentDateCollection_SameCreateTime()
		{
			var shipmentPostCarriage = TestReadIntoBusinessObjectFromShipment_SubShipmentDateCollectionCore();

			var shipmentSubShipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentSubShipment1.SetDateCollection(() => new List<Date>());
			shipmentSubShipment1.DateCollection.Add(Date.New(DateType.JobCreated, ZBool.False, new ZDateTime(2011, 1, 1)));
			var shipmentSubShipment2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentSubShipment2.SetDateCollection(() => new List<Date>());
			shipmentSubShipment2.DateCollection.Add(Date.New(DateType.JobCreated, ZBool.False, new ZDateTime(2011, 1, 1)));
			shipmentPostCarriage.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { shipmentSubShipment1, shipmentSubShipment2 });

			var localProcessing1 = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			localProcessing1.ArrivalCartageRef = "Cartage Ref1";
			shipmentSubShipment1.LocalProcessing = localProcessing1;

			var localProcessing2 = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			localProcessing2.ArrivalCartageRef = "Cartage Ref2";
			shipmentSubShipment2.LocalProcessing = localProcessing2;

			Factory.SaveForTesting();

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);

			WhsReceive receive = null;
			AssertNoExceptionThrown(() => receive = reader.ReadIntoBusinessObject());
			Factory.SaveAtEndOfImport(Logger);
			AssertNotNull(receive);

			AssertCollectionContains("Should set to one of the ArrivalCartageRef", receive.WD_TransportReference, new ZString[] { "Cartage Ref1", "Cartage Ref2" });
		}

		public void TestReadIntoBusinessObjectFromShipment_SubShipmentDateCollection_WithoutCreateTime()
		{
			var shipmentPostCarriage = TestReadIntoBusinessObjectFromShipment_SubShipmentDateCollectionCore();

			var shipmentSubShipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentSubShipment1.SetDateCollection(() => new List<Date>());
			shipmentSubShipment1.DateCollection.Add(Date.New(DateType.Accepted, ZBool.False, new ZDateTime(2011, 1, 1)));
			var shipmentSubShipment2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentSubShipment2.SetDateCollection(() => new List<Date>());
			shipmentSubShipment2.DateCollection.Add(Date.New(DateType.Accepted, ZBool.False, new ZDateTime(2011, 1, 2)));
			shipmentPostCarriage.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { shipmentSubShipment1, shipmentSubShipment2 });

			var localProcessing1 = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			localProcessing1.ArrivalCartageRef = "Cartage Ref1";
			shipmentSubShipment1.LocalProcessing = localProcessing1;

			var localProcessing2 = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			localProcessing2.ArrivalCartageRef = "Cartage Ref2";
			shipmentSubShipment2.LocalProcessing = localProcessing2;

			Factory.SaveForTesting();

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);

			WhsReceive receive = null;
			AssertNoExceptionThrown(() => receive = reader.ReadIntoBusinessObject());
			Factory.SaveAtEndOfImport(Logger);
			AssertNotNull(receive);
		}

		public void TestReadIntoBusinessObjectFromShipment_SubShipmentDateCollection_PartialEmpty()
		{
			var shipmentPostCarriage = TestReadIntoBusinessObjectFromShipment_SubShipmentDateCollectionCore();

			var shipmentSubShipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentSubShipment1.SetDateCollection(() => new List<Date>());
			shipmentSubShipment1.DateCollection.Add(Date.New(DateType.Accepted, ZBool.False, new ZDateTime(2011, 1, 1)));
			shipmentSubShipment1.DateCollection.Add(Date.New(DateType.JobCreated, ZBool.False, new ZDateTime(2011, 1, 2)));
			shipmentSubShipment1.DateCollection.Add(Date.New(DateType.LocalTransportPickup, ZBool.False, new ZDateTime(2011, 1, 3)));
			var shipmentSubShipment2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentSubShipment2.SetDateCollection(() => new List<Date>());
			shipmentPostCarriage.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { shipmentSubShipment1, shipmentSubShipment2 });

			var localProcessing1 = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			localProcessing1.ArrivalCartageRef = "Cartage Ref1";
			shipmentSubShipment1.LocalProcessing = localProcessing1;

			var localProcessing2 = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			localProcessing2.ArrivalCartageRef = "Cartage Ref2";
			shipmentSubShipment2.LocalProcessing = localProcessing2;

			Factory.SaveForTesting();

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);

			WhsReceive receive = null;
			AssertNoExceptionThrown(() => receive = reader.ReadIntoBusinessObject());
			Factory.SaveAtEndOfImport(Logger);
			AssertNotNull(receive);

			AssertEquals("Cartage Ref1", receive.WD_TransportReference);
		}

		public void TestReadIntoBusinessObjectFromShipment_SubShipmentDateCollection_EmptyArrivalCartageRef()
		{
			var shipmentPostCarriage = TestReadIntoBusinessObjectFromShipment_SubShipmentDateCollectionCore();

			var shipmentSubShipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentSubShipment1.SetDateCollection(() => new List<Date>());
			shipmentSubShipment1.DateCollection.Add(Date.New(DateType.Accepted, ZBool.False, new ZDateTime(2011, 1, 1)));
			var shipmentSubShipment2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentSubShipment2.SetDateCollection(() => new List<Date>());
			shipmentSubShipment2.DateCollection.Add(Date.New(DateType.Accepted, ZBool.False, new ZDateTime(2011, 1, 2)));
			shipmentPostCarriage.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { shipmentSubShipment1, shipmentSubShipment2 });

			var localProcessing1 = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			localProcessing1.ArrivalCartageRef = "Cartage Ref1";
			shipmentSubShipment1.LocalProcessing = localProcessing1;

			Factory.SaveForTesting();

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);

			WhsReceive receive = null;
			AssertNoExceptionThrown(() => receive = reader.ReadIntoBusinessObject());
			Factory.SaveAtEndOfImport(Logger);
			AssertNotNull(receive);

			AssertEquals("Cartage Ref1", receive.WD_TransportReference);
		}

		UniversalShipment TestReadIntoBusinessObjectFromShipment_SubShipmentDateCollectionCore()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();

			var whs1 = Data.GetOrCreateWarehouseInDB();
			var whsAddress = whs1.WarehouseAddress;
			whsAddress.Header.OH_IsWarehouseClient = true;

			ShipmentDataObject.CarrierServiceLevel = new ServiceLevel { Code = "SET", Description = "SITTLE" };
			ShipmentDataObject.ServiceLevel = new ServiceLevel { Code = "TSL", Description = "Test Service Level" };
			ShipmentDataObject.OuterPacks = 3;
			ShipmentDataObject.TotalVolume = 32.1m;
			ShipmentDataObject.TotalVolumeUnit = new UnitOfVolume() { Code = "M3" };
			ShipmentDataObject.TotalWeight = 41.2m;
			ShipmentDataObject.TotalWeightUnit = new UnitOfWeight() { Code = "KG" };
			ShipmentDataObject.DataContext.CodesMappedToTarget = true;
			ShipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>() { new AdditionalReference { Type = new EntryType { Code = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference }, ReferenceNumber = "REF00000001" } });
			ShipmentDataObject.OrganizationAddressCollection.Clear();
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ClientAddressDataObject_CRAHOLSYD);
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ConsigneeAddressDataObject_CRAHOLSYD);
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.WarehouseAddressDataObject_INTHEMSYD);
			ShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S1234");

			var clientCRAHOLSYD = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var clientINTHEMSYD = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);

			var product1 = Data.CreateProduct("P1");
			var relation1 = product1.RelatedOrganisations.AddNew();
			relation1.OU_Relationship = "OWN";
			relation1.OU_OP = product1.PK;
			relation1.OU_OH = clientINTHEMSYD.PK;
			var product2 = Data.CreateProduct("P2");
			var relation2 = product2.RelatedOrganisations.AddNew();
			relation2.OU_Relationship = "OWN";
			relation2.OU_OP = product2.PK;
			relation2.OU_OH = clientINTHEMSYD.PK;

			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine1.PackingLineID = "PackingLine0001";
			packingLine1.PackType = new PackageType { Code = "PLT" };
			packingLine1.ReferenceNumber = "PL00000001";
			packingLine1.PackQty = 5L;
			var packItem11 = new PackedItem();
			packItem11.Product = new Product { Code = product1.OP_PartNum };
			packItem11.UnitOfQuantity = new PackageType { Code = "BOX" };
			packItem11.PackedQuantity = 3m;
			packingLine1.SetPackedItemCollection(() => new List<PackedItem> { packItem11 });

			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine2.PackingLineID = "PackingLine0002";
			packingLine2.PackType = new PackageType { Code = "PLT" };
			packingLine2.ReferenceNumber = "PL00000002";
			packingLine2.PackQty = 10L;
			var packItem21 = new PackedItem();
			packItem21.Product = new Product { Code = product1.OP_PartNum };
			packItem21.UnitOfQuantity = new PackageType { Code = "BOX" };
			packItem21.PackedQuantity = 6m;
			var packItem22 = new PackedItem();
			packItem22.Product = new Product { Code = product2.OP_PartNum };
			packItem22.UnitOfQuantity = new PackageType { Code = "UNT" };
			packItem22.PackedQuantity = 9m;
			packingLine2.SetPackedItemCollection(() => new List<PackedItem> { packItem21, packItem22 });

			ShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine1, packingLine2 });
			ShipmentDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			ShipmentDataObject.LocalProcessing.FCLDeliveryEquipmentNeeded = new CodeDescriptionPair { Code = "WUP", Description = "Wait for Pack/Unpack" };

			var shipmentPostCarriage = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			ShipmentDataObject.SetPostCarriageShipmentCollection(() => new List<UniversalShipment>(new[] { shipmentPostCarriage }));

			return shipmentPostCarriage;
		}

		#endregion

		#region TestHoldPalletIDPutaway

		public void TestHoldPalletIDPutaway_IsHeld()
		{
			TestHoldPalletIDPutaway(true);
		}

		public void TestHoldPalletIDPutaway_IsNotHeld()
		{
			TestHoldPalletIDPutaway(false);
		}

		void TestHoldPalletIDPutaway(bool isHeld)
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.Order.HoldPalletIDPutaway = isHeld;
			var reader = GetNewReader(ShipmentDataObject, Logger);
			var receive = reader.ReadIntoBusinessObject();
			AssertNotNull("Should have imported the receive.", receive);
			AssertEquals("Should have set WD_HoldPalletIDPutaway.", isHeld, receive.WD_HoldPalletIDPutaway);
		}

		#endregion

		#region TestWD_ETA

		public void TestWD_ETA_IsSetFromDeliveryRequiredByAsFallback()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.LocalProcessing = new LocalProcessing { DeliveryRequiredBy = new ZDateTime(2013, 1, 1) };
			var reader = GetNewReader(ShipmentDataObject, Logger);
			var receive = reader.ReadIntoBusinessObject();
			AssertNotNull(receive);
			AssertEquals(new ZDateTimeOffset(2013, 1, 1), receive.WD_ETA);
		}

		public void TestWD_ETA_AlwaysSetFromEstimatedArrivalDate()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var initialETA = ZDateTime.Today;
			var newETA = initialETA.AddDays(7);

			var whsReceive = WhsReceiveLineDataObjectReaderTest.GetReceiveLineParent(Factory);
			whsReceive.WD_ExternalReference = "ORDERME";
			whsReceive.WD_ETA = initialETA.ToOffset();
			Factory.SaveForTesting();

			var inventoryLineDataObject = WhsReceiveLineDataObjectReaderTest.SetupInventoryLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { inventoryLineDataObject, inventoryLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			AssertEquals("Precondition", initialETA.ToOffset(), whsReceive.WD_ETA);
			var receive = ShipmentDataObject.Order;
			receive.ClientReference = "CUSTOMER";
			receive.SetDateCollection(() => new List<Date>());
			receive.DateCollection.Add(Date.New(DateType.Arrival, ZBool.True, newETA));

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsReceiveBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);

			AssertEquals("WD_ETA updated at the end of the import.", newETA.ToOffset(), whsReceiveBO.WD_ETA);
			AssertEquals("WD_ETA updated at the end of the import.", newETA.ToOffset(), whsReceive.WD_ETA);
		}

		public void TestWD_ETA_NoEstimatedArrivalDate_SetFromDeliveryRequiredByDate()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var initialETA = ZDateTime.Today;
			var newETA = initialETA.AddDays(7);

			var whsReceive = WhsReceiveLineDataObjectReaderTest.GetReceiveLineParent(Factory);
			whsReceive.WD_ExternalReference = "ORDERME";
			whsReceive.WD_ETA = initialETA.ToOffset();
			Factory.SaveForTesting();

			var inventoryLineDataObject = WhsReceiveLineDataObjectReaderTest.SetupInventoryLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { inventoryLineDataObject, inventoryLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			AssertEquals("Precondition", initialETA.ToOffset(), whsReceive.WD_ETA);
			ShipmentDataObject.LocalProcessing = new LocalProcessing { DeliveryRequiredBy = newETA };

			AssertEquals("Precondition: Shipment data object's order has no date collection.", null, ShipmentDataObject.Order.DateCollection);
			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsReceiveBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);

			AssertEquals("WD_ETA updated at the end of the import.", newETA.ToOffset(), whsReceiveBO.WD_ETA);
			AssertEquals("WD_ETA updated at the end of the import.", newETA.ToOffset(), whsReceive.WD_ETA);
		}

		public void TestWD_ETA_NoEstimatedArrivalDate_NoDeliveryRequiredByDate()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var initialETA = ZDateTimeOffset.Today;
			var whsReceive = WhsReceiveLineDataObjectReaderTest.GetReceiveLineParent(Factory);
			whsReceive.WD_ExternalReference = "ORDERME";
			whsReceive.WD_ETA = initialETA;
			Factory.SaveForTesting();

			var inventoryLineDataObject = WhsReceiveLineDataObjectReaderTest.SetupInventoryLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { inventoryLineDataObject, inventoryLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			AssertEquals("Precondition", initialETA, whsReceive.WD_ETA);
			ShipmentDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.Instance);

			AssertEquals("Precondition: No DeliveryRequiredBy date.", false, ShipmentDataObject.LocalProcessing.DeliveryRequiredBy.HasValue);
			AssertEquals("Precondition: No date collection.", null, ShipmentDataObject.Order.DateCollection);
			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsReceiveBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);

			AssertEquals("WD_ETA is not updated at the end of the import.", initialETA, whsReceiveBO.WD_ETA);
			AssertEquals("WD_ETA is not updated at the end of the import.", initialETA, whsReceive.WD_ETA);
		}

		public void TestWD_ETA_NoEstimatedArrivalDate_EmptyDeliveryRequiredByDate()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var initialETA = ZDateTimeOffset.Today;

			var whsReceive = WhsReceiveLineDataObjectReaderTest.GetReceiveLineParent(Factory);
			whsReceive.WD_ExternalReference = "ORDERME";
			whsReceive.WD_ETA = initialETA;
			Factory.SaveForTesting();

			var inventoryLineDataObject = WhsReceiveLineDataObjectReaderTest.SetupInventoryLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { inventoryLineDataObject, inventoryLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			AssertEquals("Precondition", initialETA, whsReceive.WD_ETA);
			ShipmentDataObject.LocalProcessing = new LocalProcessing { DeliveryRequiredBy = ZDateTime.Empty };
			AssertEquals("Precondition: DeliveryRequiredBy date has value.", true, ShipmentDataObject.LocalProcessing.DeliveryRequiredBy.HasValue);
			AssertEquals("Precondition: DeliveryRequiredBy date is empty.", true, ShipmentDataObject.LocalProcessing.DeliveryRequiredBy.Value.IsEmpty);
			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsReceiveBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);

			AssertEquals("WD_ETA is updated at the end of the import.", ZDateTimeOffset.Empty, whsReceiveBO.WD_ETA);
			AssertEquals("WD_ETA is updated at the end of the import.", ZDateTimeOffset.Empty, whsReceive.WD_ETA);
		}

		public void TestWD_ETA_NoEstimatedArrivalDate_ETAIsReadOnly()
		{
			Env.Security.WhsReceivePostFinaliseEdit.IsAllowed = false;
			Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();

			var initialETA = ZDateTimeOffset.Today;
			var newETA = initialETA.AddDays(7);

			var whsReceive = WhsReceiveLineDataObjectReaderTest.GetReceiveLineParent(Factory);
			var line = whsReceive.Lines.AddNew();
			line.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			line.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			line.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			whsReceive.WD_ExternalReference = "ORDERME";
			whsReceive.WD_DocketID = "W00000001";
			whsReceive.Warehouse.WW_WarehouseName = "CoolShack";
			whsReceive.Warehouse.WW_WarehouseCode = "WSS";
			whsReceive.Warehouse.RelatedCompanyBranch.GB_RL_NKHomePort = whs.RelatedCompanyBranch.GB_RL_NKHomePort;
			whsReceive.WD_ArrivalDate = ZDateTimeOffset.Today; // A receive cannot be finalized without an informed Arrival Date.
			whsReceive.WD_ETA = initialETA;
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(whsReceive);
			Factory.SaveForTesting();

			var inventoryLineDataObject = WhsReceiveLineDataObjectReaderTest.SetupInventoryLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { inventoryLineDataObject, inventoryLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			AssertEquals("Precondition", initialETA, whsReceive.WD_ETA);
			Assert("Precondition: WD_ETA is readonly.", whsReceive.WD_ETAInfo.ReadOnly);
			ShipmentDataObject.LocalProcessing = new LocalProcessing { DeliveryRequiredBy = newETA.ToDateTime() };

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsReceiveBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);

			AssertEquals("WD_ETA is not updated at the end of the import.", initialETA, whsReceiveBO.WD_ETA);
			AssertEquals("WD_ETA is not updated at the end of the import.", initialETA, whsReceive.WD_ETA);
		}

		public void TestWD_ETA_UseTimezoneOfWarehouse()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();
			var company = Factory.Load<GlbCompany>(Env.CurrentCompanyPK);
			var branchCNNJI = company.Branches.AddNew();
			branchCNNJI.GB_Code = "NJ";
			branchCNNJI.GB_RL_NKHomePort = "CNNJI";
			whs.WW_GB_RelatedCompanyBranch = branchCNNJI.PK;

			ShipmentDataObject.LocalProcessing = new LocalProcessing { DeliveryRequiredBy = new ZDateTime(2013, 1, 1) };
			var reader = GetNewReader(ShipmentDataObject, Logger);
			var receive = reader.ReadIntoBusinessObject();
			AssertNotNull(receive);
			AssertEquals(new ZDateTimeOffset(2013, 1, 1, 0, 0, 0, TimeSpan.FromHours(8)), receive.WD_ETA);
		}

		#endregion

		#region TestWD_ETD

		public void TestWD_ETD_DateIsSetFromExWorksRequiredByAsFallback()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.SetDateCollection(() => new List<Date> { new Date { Type = DateType.ExWorksRequiredBy, Value = new ZDateTime(2013, 1, 1) } });
			var reader = GetNewReader(ShipmentDataObject, Logger);
			var receive = reader.ReadIntoBusinessObject();
			AssertNotNull(receive);
			AssertEquals(new ZDateTimeOffset(2013, 1, 1), receive.WD_ETD);
		}

		public void TestWD_ETD_UseTimezoneOfWarehouse()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();
			var company = Factory.Load<GlbCompany>(Env.CurrentCompanyPK);
			var branchCNNJI = company.Branches.AddNew();
			branchCNNJI.GB_Code = "NJ";
			branchCNNJI.GB_RL_NKHomePort = "CNNJI";
			whs.WW_GB_RelatedCompanyBranch = branchCNNJI.PK;

			ShipmentDataObject.SetDateCollection(() => new List<Date> { new Date { Type = DateType.ExWorksRequiredBy, Value = new ZDateTime(2013, 1, 1) } });
			var reader = GetNewReader(ShipmentDataObject, Logger);
			var receive = reader.ReadIntoBusinessObject();
			AssertNotNull(receive);
			AssertEquals(new ZDateTimeOffset(2013, 1, 1, 0, 0, 0, TimeSpan.FromHours(8)), receive.WD_ETD);
		}

		#endregion

		#region TestWD_BookingDate

		public void TestWD_BookingDate_UseTimezoneOfWarehouse()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();
			var company = Factory.Load<GlbCompany>(Env.CurrentCompanyPK);
			var branchCNNJI = company.Branches.AddNew();
			branchCNNJI.GB_Code = "NJ";
			branchCNNJI.GB_RL_NKHomePort = "CNNJI";
			whs.WW_GB_RelatedCompanyBranch = branchCNNJI.PK;

			ShipmentDataObject.Order.SetDateCollection(() => new List<Date> { new Date { Type = DateType.BookingConfirmed, Value = new ZDateTime(2013, 1, 1) } });
			var reader = GetNewReader(ShipmentDataObject, Logger);
			var receive = reader.ReadIntoBusinessObject();
			AssertNotNull(receive);
			AssertEquals(new ZDateTimeOffset(2013, 1, 1, 0, 0, 0, TimeSpan.FromHours(8)), receive.WD_BookingDate);
		}

		#endregion

		#region TestLinkDocketWithParent

		protected override ZString DataContextKeyForLinking
		{
			get { return base.DataContextKeyForLinking + "~0~CIULMS74UH23"; }
		}

		public void TestOrderManagerOrderLinking()
		{
			var order = (BusinessObject)Factory.BOFactory.New<Forwarding.IOrder>();
			order.FillWithValidTestData();
			order[JobOrderHeaderSchema.JD_OrderNumber] = base.DataContextKeyForLinking;
			AssertLinkDocketWithParent(order, DataContextType.OrderManagerOrder);
		}

		#endregion

		#region TestWithInventoryLines

		public void TestWithInventoryLines()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var consigneeAddress = new OrganisationDataObjectReader(GetNewAddressData_WUFSHIJNB(nameof(DocAddressType.ConsigneeAddress)), Logger, Factory).GetMatchedOrNewForTesting();
			var receiveForTest = WhsReceiveLineDataObjectReaderTest.GetReceiveLineParent(Factory);

			var manufacturerAddressDataObject = OrganizationAddressTestHelper.GetAddressData(nameof(DocAddressType.Manufacturer), "test", "AUSYD");
			manufacturerAddressDataObject.CompanyName = "MANUFACTURER1";
			manufacturerAddressDataObject.Email = "a1@manufacturer.com";
			var manufacturerAddress = new OrganisationDataObjectReader(manufacturerAddressDataObject, Logger, Factory).GetMatchedOrNewForTesting();
			var product = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			Helper.SetClientAllAttributeType(receiveForTest.Client, false);
			Helper.SetProductAllAttributeUse(receiveForTest.Client, product, use: true, setReleaseCaptured: false, useSerialNumber: false);
			Factory.SaveForTesting();

			var inventoryLineDataObject = WhsReceiveLineDataObjectReaderTest.SetupInventoryLine(includeManufacturer: true);
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { inventoryLineDataObject });
			ShipmentDataObject.Order.Type = new CodeDescriptionPair { Code = "CUS" };
			Logger.ClearLogs();

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			var receive = reader.ReadIntoBusinessObject();
			AssertNotNull(receive);
			AssertEquals("whsReceiveBO.Lines.Count", 1, receive.Lines.Count);

			CombineAssertions(() =>
			{
				var inventory = receive.Lines[0].Inventory[0];
				WhsReceiveLineDataObjectReaderTest.AssertContents(inventory, includeManufacturerAddress: true);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsReceive found, creating new WhsReceive.
Information - Populating WhsReceive...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsReceiveLine found, creating new WhsReceiveLine.
Information - Populating WhsReceiveLine...
Information - Matching 'ConsigneeAddress':- Matched to 'WUFSHIJNB' by code, address '' (only address).
Information - Matching 'Manufacturer':- Matched to 'TESTSYD' by code, address '' (only address).
Information - Added Warehouse Receipt from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestInventoryLinesGetCorrectIndexForExceptionMessage

		public void TestInventoryLinesGetCorrectIndexForExceptionMessage()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			WhsReceiveLineDataObjectReaderTest.GetReceiveLineParent(Factory);

			Factory.SaveForTesting();

			var inventoryLineDataObject = WhsReceiveLineDataObjectReaderTest.SetupInventoryLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { inventoryLineDataObject, new OrderLine() });
			ShipmentDataObject.Order.Type = new CodeDescriptionPair { Code = "CUS" };

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			AssertExceptionThrown("Cannot import OrderLine without valid Product Code.", typeof(DataObjectReadFailureException),
				"Cannot Import Receipt Line 2\r\nNo Product was provided.", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestInventoryLinesAreNotUpdatedOnAFinalizedReceive

		public void TestInventoryLinesAreNotUpdatedOnAFinalizedReceive()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var whsReceive = WhsReceiveLineDataObjectReaderTest.GetReceiveLineParent(Factory);
			var line = whsReceive.Lines.AddNew();
			line.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			line.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			line.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			whsReceive.WD_ExternalReference = "ORDERME";
			whsReceive.WD_DocketID = "W00000001";
			whsReceive.Warehouse.WW_WarehouseName = "CoolShack";
			whsReceive.Warehouse.WW_WarehouseCode = "WSS";
			whsReceive.WD_ArrivalDate = ZDateTimeOffset.Today; // A receive cannot be finalized without an informed Arrival Date.
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(whsReceive);

			Factory.SaveForTesting();

			var inventoryLineDataObject = WhsReceiveLineDataObjectReaderTest.SetupInventoryLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { inventoryLineDataObject, inventoryLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsReceiveBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsReceiveBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsReceiveBO.Lines.Count", 1, whsReceiveBO.Lines.Count);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsReceive.
Information - Populating WhsReceive...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Warning - Cannot update read-only Field 'Warehouse' [WD_WW_Whs]. Cannot change 'WSS' (CoolShack) to 'WHS' (CoolHouse).
Warning - Cannot update Receipt Lines on a Finalized Receipt.
Information - Updated Warehouse Receipt W00000001 from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestInventoryLinesAreAlwaysReplaced

		public void TestInventoryLinesAreAlwaysReplaced()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();

			var whsReceive = WhsReceiveLineDataObjectReaderTest.GetReceiveLineParent(Factory);
			var line = whsReceive.Lines.AddNew();
			line.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			whsReceive.WD_WW_Whs = whs.PK;
			whsReceive.WD_ExternalReference = "ORDERME";
			whsReceive.WD_DocketID = "W00000001";
			Helper.SetClientAllAttributeType(whsReceive.Client, false);
			Helper.SetProductAllAttributeUse(whsReceive.Client, line.SupplierPart, use: true, setReleaseCaptured: false, useSerialNumber: false);
			Factory.SaveForTesting();

			var inventoryLineDataObject = WhsReceiveLineDataObjectReaderTest.SetupInventoryLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { inventoryLineDataObject, inventoryLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsReceiveBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsReceiveBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsReceiveBO.Lines.Count", 2, whsReceiveBO.Lines.Count);
				AssertEquals("whsReceiveBO.Inventory.Count", 2, whsReceiveBO.Inventory.Count);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsReceive.
Information - Populating WhsReceive...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsReceiveLine found, creating new WhsReceiveLine.
Information - Populating WhsReceiveLine...
Warning - Matching 'ConsigneeAddress':- No match found for '[Org. Code: WUFSHIJNB; Company Name: WUFU SHIPPING LINE; Address 1: Level 2, Building G; Address 2: 34 Dock Lane; City: Johannesburg]'.
Information - No matching WhsReceiveLine found, creating new WhsReceiveLine.
Information - Populating WhsReceiveLine...
Warning - Matching 'ConsigneeAddress':- No match found for '[Org. Code: WUFSHIJNB; Company Name: WUFU SHIPPING LINE; Address 1: Level 2, Building G; Address 2: 34 Dock Lane; City: Johannesburg]'.
Information - Updated Warehouse Receipt W00000001 from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestInventoryLinesAreAlwaysReplacedAndOnlyMatchesTheSameLineOnce

		public void TestInventoryLinesAreAlwaysReplacedAndOnlyMatchesTheSameLineOnce()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();

			var bowlHatReceive = WhsReceiveLineDataObjectReaderTest.GetReceiveLineParent(Factory);
			bowlHatReceive.WD_WW_Whs = Data.GetOrCreateWarehouseInDB().PK;
			bowlHatReceive.WD_ExternalReference = "ORDERME";
			bowlHatReceive.WD_DocketID = "W00000001";
			var bowlHatLine = bowlHatReceive.Lines.AddNew();
			bowlHatLine.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			Helper.SetClientAllAttributeType(bowlHatReceive.Client, false);
			Helper.SetProductAllAttributeUse(bowlHatReceive.Client, bowlHatLine.SupplierPart, use: true, setReleaseCaptured: false, useSerialNumber: false);
			Factory.SaveForTesting();

			var inventoryLineDataObject = WhsReceiveLineDataObjectReaderTest.SetupInventoryLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { inventoryLineDataObject, inventoryLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsReceiveBO = reader.ReadIntoBusinessObject();
			AssertNotNull(whsReceiveBO);
			CombineAssertions(delegate
			{
				AssertEquals("whsReceiveBO.Lines.Count", 2, whsReceiveBO.Lines.Count);
				AssertEquals("whsReceiveBO.Inventory.Count", 2, whsReceiveBO.Inventory.Count);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsReceive.
Information - Populating WhsReceive...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsReceiveLine found, creating new WhsReceiveLine.
Information - Populating WhsReceiveLine...
Warning - Matching 'ConsigneeAddress':- No match found for '[Org. Code: WUFSHIJNB; Company Name: WUFU SHIPPING LINE; Address 1: Level 2, Building G; Address 2: 34 Dock Lane; City: Johannesburg]'.
Information - No matching WhsReceiveLine found, creating new WhsReceiveLine.
Information - Populating WhsReceiveLine...
Warning - Matching 'ConsigneeAddress':- No match found for '[Org. Code: WUFSHIJNB; Company Name: WUFU SHIPPING LINE; Address 1: Level 2, Building G; Address 2: 34 Dock Lane; City: Johannesburg]'.
Information - Updated Warehouse Receipt W00000001 from UniversalShipment.
".Trim(), Logger.Logs);
			});
			Factory.SaveForTesting();

			Logger.ClearLogs();
			var reader2 = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsReceiveBO2 = reader.ReadIntoBusinessObject();
			AssertNotNull(whsReceiveBO2);
			AssertEquals(whsReceiveBO, whsReceiveBO2);
			CombineAssertions(delegate
			{
				AssertEquals("whsReceiveBO.Lines.Count", 2, whsReceiveBO2.Lines.Count);
				AssertEquals("whsReceiveBO.Inventory.Count", 2, whsReceiveBO2.Inventory.Count);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching WhsReceive.
Information - Populating WhsReceive...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsReceiveLine.
Information - Populating WhsReceiveLine...
Warning - Matching 'ConsigneeAddress':- No match found for '[Org. Code: WUFSHIJNB; Company Name: WUFU SHIPPING LINE; Address 1: Level 2, Building G; Address 2: 34 Dock Lane; City: Johannesburg]'.
Information - Successfully loaded matching WhsReceiveLine.
Information - Populating WhsReceiveLine...
Warning - Matching 'ConsigneeAddress':- No match found for '[Org. Code: WUFSHIJNB; Company Name: WUFU SHIPPING LINE; Address 1: Level 2, Building G; Address 2: 34 Dock Lane; City: Johannesburg]'.
Information - Updated Warehouse Receipt W00000001 from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestLineCollectionContentIsPartial

		protected override void SetupAndAssertDocketLinePrecondition(WhsDocketLine docketLine)
		{
			docketLine.WE_PalletID = "Pallet1";
			AssertEquals("Pre-condition: WE_PalletID", "Pallet1", docketLine.WE_PalletID);
		}

		protected override void UpdateExistingDocketLineDataObject(OrderLine docketLineDataObject)
		{
			docketLineDataObject.PalletID = "Pallet2";
			Factory.SaveForTesting();
		}

		protected override void AssertExistingDocketLineAfterImport(WhsDocketLine docketLine)
		{
			AssertEquals("WE_PalletID get updated.", "Pallet2", docketLine.WE_PalletID);
		}

		protected override WhsReceive CreateDocketWithLine(OrgHeader client, WhsWarehouse warehouse, OrgSupplierPart product, ZDecimal units)
		{
			var receive = Helper.CreateWhsReceive(client, warehouse);
			Helper.CreateWhsReceiveLine(receive, product, units);
			return receive;
		}

		#endregion

		#region TestReceiveSubType

		public void TestReceiveSubType_ValidReceiveSubType_CUS()
		{
			TestReceiveSubTypeCore(new CodeDescriptionPair { Code = ReceiveType.Codes.Customs, Description = ReceiveType.Descriptions.Customs }, ReceiveType.Codes.Customs);
		}

		public void TestReceiveSubType_ValidReceiveSubType_REC()
		{
			TestReceiveSubTypeCore(new CodeDescriptionPair { Code = ReceiveType.Codes.Receipt, Description = ReceiveType.Descriptions.Receipt }, ReceiveType.Codes.Receipt);
		}

		public void TestReceiveSubType_ValidReceiveSubType_RET()
		{
			TestReceiveSubTypeCore(new CodeDescriptionPair { Code = ReceiveType.Codes.Returns, Description = ReceiveType.Descriptions.Returns }, ReceiveType.Codes.Returns);
		}

		public void TestReceiveSubType_InvalidReceiveSubType()
		{
			TestReceiveSubTypeCore(new CodeDescriptionPair { Code = OrderType.Codes.Order, Description = OrderType.Descriptions.Order }, ReceiveType.Codes.Receipt);
		}

		void TestReceiveSubTypeCore(CodeDescriptionPair orderSubType, string expectedReceiveSubType)
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var receive = ShipmentDataObject.Order;
			receive.ClientReference = "CUSTOMER";
			receive.SetDateCollection(() => new List<Date>());
			receive.DateCollection.Add(Date.New(DateType.Arrival, ZBool.True, new ZDateTime(2011, 1, 1)));
			receive.DateCollection.Add(Date.New(DateType.Arrival, ZBool.False, new ZDateTime(2011, 1, 2)));
			receive.DateCollection.Add(Date.New(DateType.Departure, ZBool.True, new ZDateTime(2011, 1, 3)));
			receive.DateCollection.Add(Date.New(DateType.BookingConfirmed, ZBool.False, new ZDateTime(2011, 1, 4)));
			receive.DropMode = new DropMode { Code = "DRO", Description = "Drop" };
			receive.OrderNumber = "ORDERME";
			receive.OrderNumberSplit = new ZByte(1);
			receive.PalletsSent = new ZShort(4);
			receive.Status = new CodeDescriptionPair { Code = "PUT", Description = "Put Away" };
			receive.TotalLineVolume = 11.2m;
			receive.TotalLineWeight = 15.3m;
			receive.TotalUnits = 12.3m;
			receive.TransportReference = "TRANS";
			receive.Type = orderSubType;

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsReceiveBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);

			AssertNotNull(whsReceiveBO);
			AssertEquals("Docket sub type is correct.", expectedReceiveSubType, whsReceiveBO.WD_DocketSubType);
		}

		#endregion

		#region TestGetReasonForNotAbleToUpdateMatchedDocket

		public void TestGetReasonForNotAbleToUpdateMatchedDocket_ReceiveIsReadyForPlanning()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var whsReceive = WhsReceiveLineDataObjectReaderTest.GetReceiveLineParent(Factory);
			whsReceive.WD_ExternalReference = "ORDERME";
			Factory.SaveForTesting();
			whsReceive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			var inventoryLineDataObject = WhsReceiveLineDataObjectReaderTest.SetupInventoryLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { inventoryLineDataObject, inventoryLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsReceiveBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsReceiveBO);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsReceive.
Error - Cannot populate WhsReceive because:
The Receive is Ready For Planning or Planned.
".Trim(), Logger.Logs);
			});
		}

		public void TestGetReasonForNotAbleToUpdateMatchedDocket_ReceiveIsPlanned()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var whsReceive = WhsReceiveLineDataObjectReaderTest.GetReceiveLineParent(Factory);
			whsReceive.WD_ExternalReference = "ORDERME";
			Factory.SaveForTesting();
			whsReceive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;

			var inventoryLineDataObject = WhsReceiveLineDataObjectReaderTest.SetupInventoryLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { inventoryLineDataObject, inventoryLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsReceiveBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsReceiveBO);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsReceive.
Error - Cannot populate WhsReceive because:
The Receive is Ready For Planning or Planned.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestReceiveCategoryCode

		public void TestReceiveCategoryCode()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var receive = ShipmentDataObject.Order;
			receive.ClientReference = "CUSTOMER";
			receive.SetDateCollection(() => new List<Date>());
			receive.DateCollection.Add(Date.New(DateType.Arrival, ZBool.True, new ZDateTime(2011, 1, 1)));
			receive.DateCollection.Add(Date.New(DateType.Arrival, ZBool.False, new ZDateTime(2011, 1, 2)));
			receive.DateCollection.Add(Date.New(DateType.Departure, ZBool.True, new ZDateTime(2011, 1, 3)));
			receive.DateCollection.Add(Date.New(DateType.BookingConfirmed, ZBool.False, new ZDateTime(2011, 1, 4)));
			receive.DropMode = new DropMode { Code = "DRO", Description = "Drop" };
			receive.OrderNumber = "ORDERME";
			receive.OrderNumberSplit = new ZByte(1);
			receive.PalletsSent = new ZShort(4);
			receive.Status = new CodeDescriptionPair { Code = "PUT", Description = "Put Away" };
			receive.TotalLineVolume = 11.2m;
			receive.TotalLineWeight = 15.3m;
			receive.TotalUnits = 12.3m;
			receive.TransportReference = "TRANS";

			receive.Category = "ABC";

			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("ABC", (ZArchitecture.Core.NoResString)"Receive Category 1");

			WarehouseDataRegistry.Instance.ReceiveCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsReceiveBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);

			AssertNotNull(whsReceiveBO);
			AssertEquals("Receive category is correct.", "ABC", whsReceiveBO.WD_ReceiveCategory);
		}

		public void TestReceiveCategoryCode_InvalidCode()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var receive = ShipmentDataObject.Order;
			receive.ClientReference = "CUSTOMER";
			receive.SetDateCollection(() => new List<Date>());
			receive.DateCollection.Add(Date.New(DateType.Arrival, ZBool.True, new ZDateTime(2011, 1, 1)));
			receive.DateCollection.Add(Date.New(DateType.Arrival, ZBool.False, new ZDateTime(2011, 1, 2)));
			receive.DateCollection.Add(Date.New(DateType.Departure, ZBool.True, new ZDateTime(2011, 1, 3)));
			receive.DateCollection.Add(Date.New(DateType.BookingConfirmed, ZBool.False, new ZDateTime(2011, 1, 4)));
			receive.DropMode = new DropMode { Code = "DRO", Description = "Drop" };
			receive.OrderNumber = "ORDERME";
			receive.OrderNumberSplit = new ZByte(1);
			receive.PalletsSent = new ZShort(4);
			receive.Status = new CodeDescriptionPair { Code = "PUT", Description = "Put Away" };
			receive.TotalLineVolume = 11.2m;
			receive.TotalLineWeight = 15.3m;
			receive.TotalUnits = 12.3m;
			receive.TransportReference = "TRANS";

			receive.Category = "ABC";

			var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsReceiveBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);

			AssertNotNull(whsReceiveBO);
			AssertEquals("Receive category is honoured.", "ABC", whsReceiveBO.WD_ReceiveCategory);
		}

		#endregion

		#region TestRejectImportDueToInvalidCharacters

		public void TestRejectImportDueToInvalidCharacters_OwnerRef()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.OwnerRef = "你好";
			var order = ShipmentDataObject.Order;
			order.ClientReference = "";

			var reader = GetNewReader(ShipmentDataObject, Logger);

			var expectedErrorString = @"Cannot perform import due to invalid characters in field: OwnerRef.";
			AssertExceptionThrown(typeof(DataObjectReadFailureException), expectedErrorString, () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestInboundDockDoor

		public void TestInboundDockDoor_ExposeWarehouseTaskManagement()
		{
			TestInboundDockDoor_Core();
		}

		public void TestInboundDockDoor_NotExposeWarehouseTaskManagement()
		{
			TestInboundDockDoor_Core(exposeWarehouseTaskManagement: false);
		}

		public void TestInboundDockDoor_NoStagingArea()
		{
			TestInboundDockDoor_Core(hasStagingArea: false);
		}

		public void TestInboundDockDoor_LocationIsNotFound()
		{
			TestInboundDockDoor_Core(locationExist: false);
		}

		public void TestInboundDockDoor_LocationIsNotDockDoorType()
		{
			TestInboundDockDoor_Core(locationIsDockDoorType: false);
		}

		void TestInboundDockDoor_Core(bool hasStagingArea = true, bool exposeWarehouseTaskManagement = true, bool locationExist = true, bool locationIsDockDoorType = true)
		{
			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, exposeWarehouseTaskManagement))
			{
				Data.CreateClientOrgCRAHOLSYDInDB();
				Data.GetOrCreateWarehouseInDB();

				var receive = ShipmentDataObject.Order;

				if (hasStagingArea)
				{
					var locationString = locationIsDockDoorType ? "DockA" : "A-1-1-1";
					receive.StagingArea = locationExist ? locationString : "NotALocation";
				}

				var reader = new WhsReceiveDataObjectReader(ShipmentDataObject, Logger, Factory);
				WhsReceive whsReceiveBO = null;
				if (!hasStagingArea || !exposeWarehouseTaskManagement)
				{
					AssertNoExceptionThrown(() => whsReceiveBO = reader.ReadIntoBusinessObject());
					AssertNull(whsReceiveBO.InboundDockDoor);
				}
				else if (!locationExist || !locationIsDockDoorType)
				{
					AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot import receive - The StagingArea is not a dock door location.", () => reader.ReadIntoBusinessObject());
				}
				else
				{
					AssertNoExceptionThrown(() => whsReceiveBO = reader.ReadIntoBusinessObject());
					AssertEquals("Inbound dock door is correct.", exposeWarehouseTaskManagement ? "DockA" : "", whsReceiveBO.InboundDockDoor.ToLocationString());
				}
			}
		}

		#endregion

		// Order Manager tests

		#region TestClientIsConsigneeDocumentaryAddressWhenDataSourceIsOrderManager

		public void TestClientIsConsigneeDocumentaryAddressWhenDataSourceIsOrderManager()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			ShipmentDataObject.DataContext.AddDataSource(DataContextType.OrderManagerOrder, "");

			var reader1 = GetNewReader(ShipmentDataObject, Logger);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot Import Receipt\r\nNo Client Address was provided.", () => reader1.ReadIntoBusinessObject());

			Logger.ClearLogs();
			ShipmentDataObject.OrganizationAddressCollection.Clear();
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ConsigneeAddressDataObject_CRAHOLSYD);
			var reader2 = GetNewReader(ShipmentDataObject, Logger);
			var receive = reader2.ReadIntoBusinessObject();
			AssertNotNull(receive);
			AssertEquals("When Data Source is Order Manager, should take the Warehouse Client from ConsigneeDocumentaryAddress.", client.PK, receive.Client.PK);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);
		}

		#endregion

		#region TestConsignorDocumentaryAddressIsImportedAsSupplierIfSupplierDocumentaryAddressNotPresent

		public void TestConsignorDocumentaryAddressIsImportedAsSupplierIfSupplierDocumentaryAddressNotPresent()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.DataContext.AddDataSource(DataContextType.OrderManagerOrder, "");
			ShipmentDataObject.OrganizationAddressCollection.Clear();
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ConsigneeAddressDataObject_CRAHOLSYD);
			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_CRAHOLSYD(DocAddressType.SupplierDocumentaryAddress));
			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_INTHEMSYD(DocAddressType.ConsignorDocumentaryAddress));

			var reader1 = GetNewReader(ShipmentDataObject, Logger);
			var order1 = reader1.ReadIntoBusinessObject();

			AssertNotNull(order1);

			CombineAssertions(delegate
			{
				AssertJobDocAddressContentMatches_CRAHOLSYD(order1.SupplierDocAddress);
				AssertEquals(false, Logger.HasErrors);
			});

			ShipmentDataObject.OrganizationAddressCollection.Clear();
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ConsigneeAddressDataObject_CRAHOLSYD);
			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_INTHEMSYD(DocAddressType.ConsignorDocumentaryAddress));
			var reader2 = GetNewReader(ShipmentDataObject, Logger);
			var order2 = reader2.ReadIntoBusinessObject();

			AssertNotNull(order2);

			CombineAssertions(delegate
			{
				AssertJobDocAddressContentMatches_INTHEMSYD(order2.SupplierDocAddress);
				AssertEquals(false, Logger.HasErrors);
			});
		}

		#endregion

		// Customs Import tests

		protected override ZBool PopulatesCustomsInfoFromUXML => false;

		#region Test_CustomsSource_ImportOfCommercialInvoiceLines

		protected override ZPropertyInfo[] GetLineInfosForCommercialInvoiceLineTest(WhsReceiveLine line)
		{
			var inventory = line.Inventory[0];
			return new ZPropertyInfo[]
			{
				inventory.WI_OP_PartNumInfo,
				inventory.WI_InDocketLineUnitsInfo,
				inventory.WI_F3_NKPackTypeInfo,

				inventory.CustomsData.WB_AddInfoInfo,
				inventory.CustomsData.WB_CustomsQtyInfo,
				inventory.CustomsData.WB_CustomsUnitOfQtyInfo,
				inventory.CustomsData.WB_EntryKeyInfo,
				inventory.CustomsData.WB_EntryLineNoInfo,
				inventory.CustomsData.WB_RN_NKCountryOfOriginInfo,
				inventory.CustomsData.WB_TILVInfo,
				inventory.CustomsData.WB_ValueForDutyInfo,

				null,
				inventory.WI_LineNoInfo,
				inventory.WI_PartAttrib1Info,
				inventory.WI_PartAttrib2Info,
				inventory.WI_PartAttrib3Info,
				inventory.WI_SerialNumberInfo,
				inventory.WI_BondedEntryKeyInfo,

				inventory.CustomsData.WB_DeclarationReferenceInfo,

				inventory.CustomsData.WB_CustomsSecondQuantityInfo,
				inventory.CustomsData.WB_CustomsSecondUnitQtyInfo,
				inventory.CustomsData.WB_TariffInfo,
				inventory.CustomsData.WB_PrimaryPreferenceInfo,
				inventory.CustomsData.WB_CustomsThirdQuantityInfo,
				inventory.CustomsData.WB_CustomsThirdUnitQtyInfo,
				// ManufacturerAddress need to be tested after customs team implement it
				inventory.CustomsData.WB_ZoneStatusInfo,
				inventory.CustomsData.WB_IsFromAnotherFTZWhsInfo,
				inventory.CustomsData.WB_OutwardTypeInfo,
				inventory.CustomsData.WB_CustomsDeadlineInfo,
				inventory.CustomsData.WB_InwardStyleInfo,
				inventory.CustomsData.WB_InwardProcedureInfo,
			};
		}

		#endregion

		#region Test_CustomsSource_ImportOfCommercialInvoiceLinesWithExtraClassificationDetails

		public void Test_CustomsSource_ImportOfCommercialInvoiceLinesWithExtraClassificationDetails()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
				// to make classification details appear, we need to have parent line number pointing to a valid line
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].LineNo = 3;
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].ParentLineNo = 3;

				var reader = GetNewReader(ShipmentDataObject, Logger);
				var docket = reader.ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(Logger);

				// check what saved to DB
				var query = new ZQuery(CusAddInfoSchema.B7_ParentID, docket.Lines[0].PK);

				var result = Factory.BOFactory.LoadTop1<IWarehouseCustomsAddInfo>(query);

				AssertNotNull(result);
				AssertEquals("WCA", result.B7_Type);
				AssertNotNull(result.B7_AddInfoData);
				AssertEquals(docket.Lines[0].PK, result.B7_ParentID);
				AssertEquals("WE", result.B7_ParentTableCode);
			}
		}

		#endregion

		#region Test_CustomsSource_ImportOfCommercialInvoiceLinesWithExtraCustomsDetails

		public void Test_CustomsSource_ImportOfCommercialInvoiceLinesWithExtraCustomsDetails()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
				var commercialInvoiceLine1DataObject = Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
				commercialInvoiceLine1DataObject.CommercialChargeCollection = new List<CommercialCharge>(new[]
				{
					new CommercialCharge()
					{
						ChargeType = new CodeDescriptionPair() { Code = "ONS" },
						Amount = new ZDecimal(1500.51m),
						Currency = new Currency() { Code = Core.Constants.CurrencyCodes.NewZealand },
						IsDutiable = ZBool.True,
						IsGSTApplicable = ZBool.False,
						IsIncludedInITOT = ZBool.True,
						IsStatisticalValueApplicable = ZBool.True,
						PrepaidCollect = new CodeDescriptionPair() { Code = "PPC" }
					},
					new CommercialCharge()
					{
						ChargeType = new CodeDescriptionPair() { Code = "OFT" },
						Amount = new ZDecimal(25000.50m),
						Currency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia },
						IsDutiable = ZBool.False,
						IsGSTApplicable = ZBool.True,
						IsIncludedInITOT = ZBool.True,
						IsStatisticalValueApplicable = ZBool.False,
						PrepaidCollect = new CodeDescriptionPair() { Code = "CLL" }
					}
				});

				var reader = GetNewReader(ShipmentDataObject, Logger);
				var docket = reader.ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(Logger);
				var customsData = docket.Lines[0].CustomsData;
				// check what saved to DB
				var query = new ZQuery(CusAddInfoSchema.B7_ParentID, customsData.PK);
				query.OrderBy = CusAddInfoSchema.Constants.B7_AddInfoData;
				var addInfos = Factory.BOFactory.Load<IWarehouseCustomsAttributeAddInfo>(query);
				AssertEquals(2, addInfos.Length);
				AssertCusAddInfo(addInfos[0], "Amount=1500.51*ChargeType=ONS*Currency=NZD*IsDutiable=Y*IsGSTApplicable=*IsIncludedInITOT=Y*IsStatisticalValueApplicable=Y");
				AssertCusAddInfo(addInfos[1], "Amount=25000.5*ChargeType=OFT*Currency=AUD*IsDutiable=*IsGSTApplicable=Y*IsIncludedInITOT=Y*IsStatisticalValueApplicable=");
			}
		}

		void AssertCusAddInfo(IWarehouseCustomsAttributeAddInfo warehouseCustomsAdditionalAddInfo, ZString addInfoData)
		{
			CombineAssertions(() =>
			{
				AssertEquals("warehouseCustomsAdditionalAddInfo.B7_ParentTableCode", WhsBondedWarehouseAttributeSchema.Constants.Prefix, warehouseCustomsAdditionalAddInfo.B7_ParentTableCode);
				AssertEquals("warehouseCustomsAdditionalAddInfo.B7_Type", "CCT", warehouseCustomsAdditionalAddInfo.B7_Type);
				AssertEquals("warehouseCustomsAdditionalAddInfo.B7_AddInfoData", addInfoData, warehouseCustomsAdditionalAddInfo.B7_AddInfoData);
			});
		}

		#endregion

		#region Test_CustomsSource_ImportOfCommercialInvoiceLines_MultiplePackages

		public void Test_CustomsSource_ImportOfCommercialInvoiceLines_MultiplePackages()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

				var product2 = Data.CreateProduct("P2");
				Helper.CreateWhsReceiveWithInventory(Data.Orgs.CRAHOLSYD, Data.GetOrCreateWarehouseInDB(isBondedWarehouse: true), "R2", product2, 100m, "ENTRYNUMBER123-3");
				Factory.SaveForTesting();

				var entryLineNo = Data.LastUsedEntryLineNo;
				var commercialInvoiceLine1 = Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
				var commercialInvoiceLine2 = Data.AddCommercialInvoiceLineWithRelatedEntryLine(20m, Data.CustomsCommInvLineNoOnOrder + 1, product2);
				var commercialInvoiceLine3 = Data.AddCommercialInvoiceLineWithRelatedEntryLine(1m, Data.CustomsCommInvLineNoOnOrder + 2, Data.Product, entryLineNo, "EntryNumber123", 0, "");
				commercialInvoiceLine1.BondedWarehouseQuantity = 10m;
				commercialInvoiceLine2.CustomizedFieldCollection = null;
				SetupPerPackageQtyAndPackageGroupId(Data.ShipmentDataObject, commercialInvoiceLine1, packageGroupID: 1, packageAmount: 5, packedQty: 5m);
				SetupPerPackageQtyAndPackageGroupId(Data.ShipmentDataObject, commercialInvoiceLine1, packageGroupID: 2, packageAmount: 2, packedQty: 5m);
				SetupPerPackageQtyAndPackageGroupId(Data.ShipmentDataObject, commercialInvoiceLine2, packageGroupID: 1, packageAmount: 5, packedQty: 20m);
				SetupPerPackageQtyAndPackageGroupId(Data.ShipmentDataObject, commercialInvoiceLine3, packageGroupID: 2, packageAmount: 2, packedQty: 1m);

				var reader = GetNewReader(ShipmentDataObject, Logger);
				var docket = reader.ReadIntoBusinessObject();
				AssertEquals(4, docket.Lines.Count);
				Factory.SaveForTesting();

				CombineAssertions(delegate
				{
					var line1 = docket.Lines.Single(l => l.WE_PackageGroupId == "W00000003-001" && l.WE_PerPackageQty == 1m);
					AssertProductAndUnits((WhsReceiveLine)line1, Data.Product, 5m);

					var line2 = docket.Lines.Single(l => l.WE_PackageGroupId == "W00000003-002" && l.WE_PerPackageQty == 2.5m);
					AssertProductAndUnits((WhsReceiveLine)line2, Data.Product, 5m);

					var line3 = docket.Lines.Single(l => l.WE_PackageGroupId == "W00000003-001" && l.WE_PerPackageQty == 4m);
					AssertProductAndUnits((WhsReceiveLine)line3, product2, 20m);

					var line4 = docket.Lines.Single(l => l.WE_PackageGroupId == "" && l.WE_PerPackageQty == 0m);
					AssertProductAndUnits((WhsReceiveLine)line4, Data.Product, 1m);

					AssertEquals(false, Logger.HasErrors);
				});
			}
		}

		void SetupPerPackageQtyAndPackageGroupId(UniversalShipment shipment, CommercialInvoiceLine invoiceLine, ZShort packageGroupID, int packageAmount, decimal packedQty)
		{
			if (shipment.CustomsReferenceCollection == null)
			{
				shipment.SetCustomsReferenceCollection(() => new List<CustomsReference>());
			}

			if (!shipment.CustomsReferenceCollection.Any(r => r.Order == packageGroupID))
			{
				shipment.CustomsReferenceCollection.Add(new CustomsReference
				{
					Order = packageGroupID,
					Type = new CodeDescriptionPair { Code = "WHP" }, // Whs Pack
					Reference = packageAmount.ToString(),
				});
			}

			if (invoiceLine.AddInfoGroupCollection == null)
			{
				invoiceLine.AddInfoGroupCollection = new List<AddInfoGroup>();
			}
			var invoiceAddInfoGroup = new AddInfoGroup
			{
				Type = new CodeDescriptionPair { Code = "WPL" }, // US Pack line
				AddInfoCollection = new List<AddInfo>
				{
					new AddInfo { Key = "PackageID", Value = packageGroupID.ToString() },
					new AddInfo { Key = "PackedQty", Value = packedQty.ToString() }
				}
			};
			invoiceLine.AddInfoGroupCollection.Add(invoiceAddInfoGroup);

			if (shipment.AddInfoGroupCollection != null || shipment.SetAddInfoGroupCollection(() => new List<AddInfoGroup>()))
			{
				var shipmentAddInfoGroup = new AddInfoGroup
				{
					Type = new CodeDescriptionPair { Code = "WPK" }, // US Pack
					AddInfoCollection = new List<AddInfo>
					{
						new AddInfo { Key = "PackageQty", Value = packageAmount.ToString() },
						new AddInfo { Key = "PackageReference", Value = packageGroupID.ToString() }
					}
				};

				shipment.AddInfoGroupCollection.Add(shipmentAddInfoGroup);
			}
		}

		void AssertProductAndUnits(WhsReceiveLine line, OrgSupplierPart part, ZDecimal units)
		{
			GetLineInfosForCommercialInvoiceLineTest(line);
			AssertEquals("Product", part.OP_PartNum, line.SupplierPart.OP_PartNum);
			AssertEquals("Units", units, line.WE_TransactionQuantity);
		}

		#endregion

		#region Test_CustomsSource_ImportOfWarehouse_DoesNotMatchTransitWarehouses

		protected override void AssertTransitWarehousesAreNotMatchedOnImport(Func<WhsReceive> attemptImport)
		{
			AssertExceptionThrown("Can't read in the Docket without a valid Warehouse Address.", typeof(DataObjectReadFailureException),
				string.Format(@"
Cannot Import {0}
Unable to match Warehouse for Organization: In The Moment Address: Unit 12, Level 3.".Trim(), GetDocketType()), () => attemptImport());
		}

		#endregion

		#region Test_CustomsSource_RejectsImportIfWhsAddressIsUnmatched

		public void Test_CustomsSource_RejectsImportIfWhsAddressIsUnmatched()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(addWarehouseToXML: false);

			// set an address on the import file
			var whsAddressDataObject = GetNewAddressData_WUFSHIJNB(DocAddressType.CustomsWarehouseAddress);
			ShipmentDataObject.OrganizationAddressCollection.Add(whsAddressDataObject);

			// address is unmatched, should reject import
			AssertExceptionThrown("Can't read in the Docket without a valid Warehouse Address.", typeof(DataObjectReadFailureException),
			string.Format(@"
Cannot Import {0}
Unable to match Warehouse Address, please make sure the supplied Warehouse Address is valid. Details were:
Address1: Level 2, Building G
Address2: 34 Dock Lane
City: Johannesburg
CompanyName: WUFU SHIPPING LINE
Country: ZA - South Africa
Email: benny.banana@wufu.co.za
Fax: 0011 54 392 2921
GovRegNum: TAXME
GovRegNumType: SAM - Uncle Sam
Mobile: 0011 289 392 2900
OrganizationCode: WUFSHIJNB
Phone: 0011 54 392 2900
Postcode: 12345
UniversalNettingCode: GOFISH
UniversalOfficeCode: AWESOME
RegistrationNumber 1:
CountryOfIssue: ZA - South Africa
Type: CCC - Carrier Code
Value: FLOG".Trim(), GetDocketType()), () => GetNewReader(ShipmentDataObject, Logger, useCleanFactory: true).ReadIntoBusinessObject());
		}

		#endregion

		#region Test_CustomsSource_ImportIntoVirtualWhsFinalisesDocket_RejectsImportIfUnableToFinalise

		protected override void Test_CustomsSource_ImportIntoVirtualWhsFinalisesDocket_RejectsImportIfUnableToFinaliseCore()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			using (ObjectFactory.Substitute(PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive().Object))
			{
				// remove lines so that finalise fails
				Data.ShipmentDataObject.CommercialInfo = null;

				// no lines, cannot finalise, should reject import
				AssertExceptionThrown("Docket cannot be finalised thus the import should rejected.", typeof(DataObjectReadFailureException),
				string.Format(@"
Cannot Import {0}
{0} could not be finalized into the Warehouse for Customs Job B123 because of the following error(s):
Error: No lines have been entered. A Docket must have lines before it can be finalized.
Error: Finalise".Trim(), GetDocketType()), () => GetNewReader(ShipmentDataObject, Logger).ReadIntoBusinessObject());
			}
		}

		#endregion

		#region Test_CustomsSource_ImportIntoRealWhs / Test_CustomsSource_ImportIntoVirtualWhsFinalisesDocket

		protected override void Assert_CustomsSource_ImportFinalisesDocketIfVirtualWhsCore(WhsReceive receive, bool useVirtualWhs, bool finaliseAllowed)
		{
			if (useVirtualWhs)
			{
				AssertEquals(new ZDateTimeOffset(2013, 1, 1, 7, 7, 30), receive.WD_ArrivalDate);
			}
			else
			{
				AssertEquals(false, receive.IsFinaliseAllowed);
				AssertEquals(true, receive.WD_ArrivalDate.IsEmpty);
			}
		}

		#endregion

		// Customs Import tests - Amendments

		protected override ZDecimal GetExpectedDocketNewLineUnits(int customsAmendedAmount)
		{
			return customsAmendedAmount; // receiving amended units into stock
		}

		protected override void AssertDocketWasCancelledOut(WhsReceive docketThatWasAdjusted, ZDecimal amendedValue)
		{
			var adjustment = GetBondedAdjustmentFromExternalReference(docketThatWasAdjusted.WD_ExternalReference, useCleanFactory: true);
			AssertNotNull(adjustment);
			AssertEquals(docketThatWasAdjusted.WD_ExternalReference, adjustment.WD_ExternalReference);
			AssertEquals(docketThatWasAdjusted.WD_ExternalReferenceSplit, adjustment.WD_ExternalReferenceSplit);
			AssertEquals(true, adjustment.IsFinalised);
		}

		protected override ZString ErrorMessageForAttemptingAmendingFinalisedDocketInRealWarehouse => @"Cannot populate WhsReceive because:
Warehouse Receipt could not be amended for Customs Job B123 because it is already finalized.";

		protected override void AssertPreviousJobInVirtualWhsWasCancelledOut(OrgSupplierPart expectedProduct, int originalCustomsQty, int amendedCustomsQty, byte expectedDocketSplitNo)
		{
			var adjustment = GetBondedAdjustmentFromExternalReference("B123");
			AssertEquals("B123", adjustment.WD_ExternalReference);
			AssertEquals("Adjustment does not need Customs Parent Reference.", "", adjustment.WD_CustomsParentReference);
			AssertEquals(expectedDocketSplitNo - 1, adjustment.WD_ExternalReferenceSplit);
			AssertEquals(true, adjustment.IsFinalised);

			var lineForAdjustedStock = adjustment.Lines.Single();
			var adjustedStockAmount = GetExpectedAdjustmentResetLineUnits(originalCustomsQty);
			CombineAssertions(() =>
			{
				AssertEquals("Adjustment Line Units.", adjustedStockAmount, lineForAdjustedStock.WE_TransactionQuantity);
				AssertEquals("Adjustment Line Product.", expectedProduct.PK, lineForAdjustedStock.WE_OP);
				AssertEquals("Adjustment Line WE_BondedEntryKey", "ENTRYNUMBER123-2", lineForAdjustedStock.WE_BondedEntryKey);
				AssertEquals("Adjustment Line CustomsData.WB_EntryKey", "EntryNumber123".ToUpper(), lineForAdjustedStock.CustomsData.WB_EntryKey.ToUpper());
				AssertEquals("Adjustment Line CustomsData.WB_EntryLineNo", new ZShort(2), lineForAdjustedStock.CustomsData.WB_EntryLineNo);
			});
		}

		WhsAdjustment GetBondedAdjustmentFromExternalReference(ZString externalReference, bool useCleanFactory = false)
		{
			return GetBondedAdjustmentFromExternalReference(externalReference, useCleanFactory ? new BusinessObjectFactory() : Factory.BOFactory);
		}

		WhsAdjustment GetBondedAdjustmentFromExternalReference(ZString externalReference, BusinessObjectFactory factory)
		{
			var adjustmentQuery = new ZQuery();
			adjustmentQuery.AddToFilter(WhsDocketSchema.WD_ExternalReference, externalReference);
			adjustmentQuery.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Adjustment);
			adjustmentQuery.OrderBy = WhsDocketSchema.Constants.WD_ExternalReferenceSplit + OrderByClause.Descending;

			return factory.LoadTop1<WhsAdjustment>(adjustmentQuery);
		}

		ZDecimal GetExpectedAdjustmentResetLineUnits(int originalDocketAmount)
		{
			return originalDocketAmount * -1; // removing previously received units from stock
		}

		#region Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCorrectlyAmendsData

		public void Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCorrectlyAmendsData()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);

			var docket = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 10m, "EntryNumber123", 2, "EntryNumber456", 102, finalise: true);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(docket);

			// amend qty from 10 to 7
			Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 7;
			AssertNewDocketAndCancellingOutOfPreviousDocketFromCustomsAmendment(Data.ShipmentDataObject, Data.Product, 10, 7, 0);

			// amend qty from 7 to 9 (test a second customs amendment)
			Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 9;
			AssertNewDocketAndCancellingOutOfPreviousDocketFromCustomsAmendment(Data.ShipmentDataObject, Data.Product, 7, 9, 0);
		}

		#endregion

		#region Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCorrectlyAmendsData_WhenCusDecIsSubShipment

		public void Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCorrectlyAmendsData_WhenCusDecIsSubShipment()
		{
			var topLevelShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			topLevelShipment.DataContext = DataContextFactory.New();
			topLevelShipment.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C1234");
			topLevelShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S1234");
			topLevelShipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			topLevelShipment.DataContext.AddDataTarget(DataContext, null);
			topLevelShipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = GetRecipientRoleType() } } });

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.DataContext = DataContextFactory.New();
			subShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S1235");

			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);
			topLevelShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			topLevelShipment.SubShipmentCollection.Add(Data.ShipmentDataObject);
			Data.ShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S1234");

			// Clear out company details as subshipment would not contain them
			Data.ShipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(Factory.New<GlbCompany>());

			var docket = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, Data.GetOrCreateWarehouseInDB(true), "B123", "B123-EDIDATEDI", Data.Product, 10m, "EntryNumber123", 2, "EntryNumber456", 102, finalise: true);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(docket);

			Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 9;
			AssertNewDocketAndCancellingOutOfPreviousDocketFromCustomsAmendment(topLevelShipment, Data.Product, 10, 9, 0);
		}

		#endregion

		#region Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCreatesAdmendmentEvenWhenFirstJobWasCancelled

		protected override void AssertDocketCancelledByCustomsEventIsNotCancelledAgainWhenAmending(WhsReceive docketThatWasCancelledOut)
		{
			var adjustmentQuery = new ZQuery();
			adjustmentQuery.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Adjustment);
			adjustmentQuery.AddToFilter(WhsDocketSchema.WD_ExternalReference, docketThatWasCancelledOut.WD_ExternalReference);
			AssertEquals(1, new BusinessObjectFactory().Load<WhsAdjustment>(adjustmentQuery).Length);
		}

		#endregion

		#region Test_CustomsSource_ImportOfJobInVirtualWhsCorrectlyAmendsAllCustomsQuantities

		public void Test_CustomsSource_ImportOfJobInVirtualWhsCorrectlyAmendsAllCustomsQuantities()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);
			Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			Factory.SaveForTesting();

			using (ObjectFactory.Substitute(PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive().Object))
			{
				// receive 10 units
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 10;
				var importResults = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
				var receive = (WhsReceive)importResults.Single().GetBizOForTesting(Data.ShipmentDataObject, Factory.BOFactory);
				AssertEquals("Precondition", "B123", receive.WD_ExternalReference);
				AssertEquals("Precondition", "B123-EDIDATEDI", receive.WD_CustomsParentReference);
				AssertEquals("Precondition", ZByte.Zero, receive.WD_ExternalReferenceSplit);
				AssertIsFinalisedPrecondition(receive);

				// precondition
				AssertInventory("TILV4Warehouse=999*Moo=50", 6m, 12m, 22m, 888m, 777m);

				// amend customs quantities
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].AddInfoCollection[1].Value = "60";
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 7;
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CustomsQuantity = 8m;
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CustomsSecondQuantity = 12m;
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CustomsThirdQuantity = 22m;
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CustomsValue = 555m;
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].AddInfoCollection[0].Value = "333";
				AssertNewDocketAndCancellingOutOfPreviousDocketFromCustomsAmendment(Data.ShipmentDataObject, Data.Product, 10, 7, 0);
				AssertInventory("TILV4Warehouse=999*Moo=60", 8m, 12m, 22m, 555m, 333m);

				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].AddInfoCollection[1].Value = "70";
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 9;
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CustomsQuantity = 4m;
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CustomsSecondQuantity = 6m;
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CustomsThirdQuantity = 13m;
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CustomsValue = 123m;
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].AddInfoCollection[0].Value = "689";
				AssertNewDocketAndCancellingOutOfPreviousDocketFromCustomsAmendment(Data.ShipmentDataObject, Data.Product, 7, 9, 0);
				AssertInventory("TILV4Warehouse=999*Moo=70", 4m, 6m, 13m, 123m, 689m);
			}
		}

		void AssertInventory(ZString expectedAddInfo, ZDecimal expectedCustomsQuantity, ZDecimal expectedCustomsSecondQuantity, ZDecimal expectedCustomsThirdQuantity, ZDecimal expectedValueForDuty, ZDecimal expectedTILV)
		{
			var query = new ZQuery();
			query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0);

			var inventory = new BusinessObjectFactory().Load<WhsInventoryView>(query);
			var amendedInventory = inventory.Single(i => i.InDocketLine.ReceiptReference != "R1");
			AssertEquals(expectedAddInfo, amendedInventory.CustomsData.WB_AddInfo);
			AssertEquals(expectedCustomsQuantity, amendedInventory.CustomsData.WB_CustomsQty);
			AssertEquals(expectedValueForDuty, amendedInventory.CustomsData.WB_ValueForDuty);
			AssertEquals(expectedTILV, amendedInventory.CustomsData.WB_TILV);
			AssertEquals(expectedCustomsSecondQuantity, amendedInventory.CustomsData.WB_CustomsSecondQuantity);
			AssertEquals(expectedCustomsThirdQuantity, amendedInventory.CustomsData.WB_CustomsThirdQuantity);
		}

		#endregion

		#region Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCreatesAdjustment_IsRejectedIfPriorOrderTookStock

		// TODO test will be removed
		public void Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCreatesAdjustment_IsRejectedIfPriorOrderTookStock()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create an existing order originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);

			var receive = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 10m, "EntryNumber123", 2, "EntryNumber456", 102, finalise: true);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(Data.Orgs.CRAHOLSYD, Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true), Data.Product, 107m); // then, only 3 left
			PickAndFinaliseInDB(order);

			// Amend qty received from 10 to 6, but do this *after* an order for the stock was placed.
			// This is not a valid Amendment and therefore will be rejected.
			Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 6;

			var reader = GetNewReader(Data.ShipmentDataObject, Logger);
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				"Cannot do an amendment. Stock related properties affected.", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCreatesAdjustment_IsNotRejectedIfOrderForStockIsCancelled

		// TODO test will be removed
		public void Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCreatesAdjustment_IsNotRejectedIfOrderForStockIsCancelled()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true, createInventory: false);
			Factory.SaveForTesting();

			using (ObjectFactory.Substitute(PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive().Object))
			{
				// receive in 10 stock
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 10m;
				var importResults = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
				var receive = (WhsReceive)importResults.Single().GetBizOForTesting(Data.ShipmentDataObject, Factory.BOFactory);
				AssertEquals("Precondition", "B123", receive.WD_ExternalReference);
				AssertEquals("Precondition", "B123-EDIDATEDI", receive.WD_CustomsParentReference);
				AssertEquals("Precondition", ZByte.Zero, receive.WD_ExternalReferenceSplit);
				AssertIsFinalisedPrecondition(receive);

				// order 5 of the 10 stock
				var order = Helper.CreateWhsOrderWithOrderLine(Data.Orgs.CRAHOLSYD, Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true), "B456", Data.Product, 5m);
				order.WD_CustomsParentReference = "B456-EDIDATEDI";
				PickAndFinaliseInDB(order);
				Factory.SaveForTesting();

				// amend order
				var otherFactory = new UniversalObjectFactory();
				var dataForOrder = new TestDataForUniversal(otherFactory, Logger, DataContextType.WarehouseOrder);
				dataForOrder.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true, createInventory: false);
				dataForOrder.ShipmentDataObject.DataContext.DataSourceCollection.First().Key = "B456";
				dataForOrder.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 8m;
				WhsOrder amendedOrder;
				using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
				{
					amendedOrder = new WhsOrderDataObjectReader(dataForOrder.ShipmentDataObject, Logger, otherFactory).ReadIntoBusinessObject();
				}
				otherFactory.SaveForTesting();

				// cancel order (by sending a cancel event)
				var eventDataObject = dataForOrder.GetEventDataObject(Events.CancelTheWarehouseJob);
				eventDataObject.DataContext = dataForOrder.ShipmentDataObject.DataContext;
				Logger.TopLevelDataObject = dataForOrder.ShipmentDataObject;
				ImportEventViaDataContextManager(eventDataObject);

				// precondition: ensure order was cancelled through unfinalising of the order
				var orderInOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(amendedOrder.PK);
				AssertOrderWasCancelledOutInVirtualWhs(orderInOtherFactory);
				AssertInventoryWasAmendedBackToOriginalQuantities("B123", 10m, 10m);

				// amend qty received from 10 to 5, this is done *after* an order took the stock and was then cancelled
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 5m;
				var reader = GetNewReader(Data.ShipmentDataObject, Logger, useCleanFactory: true);

				// ensure the amendment was allowed (because there were no *non-cancelled* orders for the receipt)
				WhsReceive amendedReceive = null;
				AssertNoExceptionThrown(() => amendedReceive = reader.ReadIntoBusinessObject());
				AssertEquals(true, amendedReceive.IsFinalised);

				// finally, the inventory balance should be updated to reflect the receipt amended from 10 to 5 units
				var inventoryQuery = new ZQuery();
				inventoryQuery.AddToFilter(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.GreaterThan, 0m);
				inventoryQuery.AddToFilter(WhsDocketLineSchema.WE_OP, Data.Product.PK);
				AssertEquals(5m, amendedReceive.Factory.Load<WhsReceiveLine>(inventoryQuery).Sum(i => i.WE_StockOnHand));
			}
		}

		#endregion

		#region Test_CustomsSource_CancellingOfFinalisedJobInVirtualWhs_IsRejected

		public void Test_CustomsSource_CancellingOfFinalisedJobInVirtualWhs_IsRejected_PriorOrderTookStock()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true, createInventory: false);

			// create an existing order originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);

			var receive = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 10m, "EntryNumber123", 2, "EntryNumber456", 102, finalise: true);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(Data.Orgs.CRAHOLSYD, Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true), Data.Product, 7m); // then, only 3 left
			PickAndFinaliseInDB(order);

			// Attempt to cancel picked receive should fail
			var eventDataObject = Data.GetEventDataObject(Events.CancelTheWarehouseJob);
			var message = GetQueuedUniversalEventMessage(eventDataObject);
			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var results = manager.Process(message).ImportResults;

			Assert("Should say that cancelling is not allowed.", results.Single(DataContextMatchesWarehouseReceive).Logs.Any(l => l.Type == LogType.Error
				&& l.Message == "Cannot Import Receipt\r\nCannot amend Receipt W00000001 as some of its stock has been released."));
		}

		public void Test_CustomsSource_CancellingOfFinalisedJobInVirtualWhs_IsRejected_InvalidReceiveForCancelling()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true, createInventory: false);

			// create an existing order originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);

			var receive = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 10m, "EntryNumber123", 2, "EntryNumber456", 102);
			Factory.SaveForTesting();
			AssertEquals(false, receive.IsFinalised);

			// Attempt to cancel receive should fail
			var eventDataObject = Data.GetEventDataObject(Events.CancelTheWarehouseJob);
			var message = GetQueuedUniversalEventMessage(eventDataObject);
			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			IEnumerable<IImportResult> results = null;
			AssertNoExceptionThrown(() => results = manager.Process(message).ImportResults);

			Assert("Should say that cancelling is not allowed.", results.Single(DataContextMatchesWarehouseReceive).Logs.Any(l => l.Type == LogType.Error
				&& l.Message == "Cannot Import Receipt\r\nOnly Bonded Dockets that are Finalized (in virtual warehouse or for change of ownership) can be Adjusted out."));
		}

		static bool DataContextMatchesWarehouseReceive(IImportResult r)
		{
			try
			{
				return r.DataContextType == DataContextType.WarehouseReceive;
			}
			catch (InvalidOperationException) // A nulled ImportLogger will throw an IOE.
			{
				return false;
			}
		}

		#endregion

		#region Test_CustomsSource_CancellingOfFinalisedJobInVirtualWhs_IsNotRejectedIfOrderForStockIsCancelled

		public void Test_CustomsSource_CancellingOfFinalisedJobInVirtualWhs_IsNotRejectedIfOrderForStockIsCancelled()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true, createInventory: false);
			Factory.SaveForTesting();

			using (ObjectFactory.Substitute(PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive().Object))
			{
				// receive in 10 stock
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 10m;
				var importResults = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
				var receive = (WhsReceive)importResults.Single().GetBizOForTesting(Data.ShipmentDataObject, Factory.BOFactory);
				AssertEquals("Precondition", "B123", receive.WD_ExternalReference);
				AssertEquals("Precondition", "B123-EDIDATEDI", receive.WD_CustomsParentReference);
				AssertEquals("Precondition", ZByte.Zero, receive.WD_ExternalReferenceSplit);
				AssertIsFinalisedPrecondition(receive);

				// order 5 of the 10 stock
				var order = Helper.CreateWhsOrderWithOrderLine(Data.Orgs.CRAHOLSYD, Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true), "B456", Data.Product, 5m);
				order.WD_CustomsParentReference = "B456-EDIDATEDI";
				PickAndFinaliseInDB(order);
				Factory.SaveForTesting();

				using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
				{
					// amend order
					var otherFactory = new UniversalObjectFactory();
					var dataForOrder = new TestDataForUniversal(otherFactory, Logger, DataContextType.WarehouseOrder);
					dataForOrder.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true, createInventory: false);
					dataForOrder.ShipmentDataObject.DataContext.DataSourceCollection.First().Key = "B456";
					dataForOrder.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 8m;
					var amendedOrder = new WhsOrderDataObjectReader(dataForOrder.ShipmentDataObject, Logger, otherFactory).ReadIntoBusinessObject();
					otherFactory.SaveForTesting();

					// cancel order (by sending a cancel event)
					var eventDataObject = dataForOrder.GetEventDataObject(Events.CancelTheWarehouseJob);
					eventDataObject.DataContext = dataForOrder.ShipmentDataObject.DataContext;
					Logger.TopLevelDataObject = dataForOrder.ShipmentDataObject;
					ImportEventViaDataContextManager(eventDataObject);

					// precondition: ensure order was cancelled through unfinalising of the order
					var orderInOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(amendedOrder.PK);
					AssertOrderWasCancelledOutInVirtualWhs(orderInOtherFactory);
					AssertInventoryWasAmendedBackToOriginalQuantities("B123", 10m, 10m);
				}

				// amend qty received from 10 to 5, this is done *after* an order took the stock and was then cancelled
				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 5m;
				var reader = GetNewReader(Data.ShipmentDataObject, Logger, useCleanFactory: true);

				// ensure the amendment was allowed (because there were no *non-cancelled* orders for the receipt)
				WhsReceive amendedReceive = null;
				AssertNoExceptionThrown(() => amendedReceive = reader.ReadIntoBusinessObject());
				AssertEquals(true, amendedReceive.IsFinalised);

				// finally, the inventory balance should be updated to reflect the receipt amended from 10 to 5 units
				var inventoryQuery = new ZQuery();
				inventoryQuery.AddToFilter(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.GreaterThan, 0m);
				inventoryQuery.AddToFilter(WhsDocketLineSchema.WE_OP, Data.Product.PK);
				AssertEquals(5m, amendedReceive.Factory.Load<WhsReceiveLine>(inventoryQuery).Sum(i => i.WE_StockOnHand));

				// Attempt to cancel receive should be allowed now
				var message = GetQueuedUniversalEventMessage(Data.GetEventDataObject(Events.CancelTheWarehouseJob));
				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				var result = manager.Process(message).ImportResults.Single(r => r.DataContextType == DataContextType.WarehouseReceive);
				AssertEquals("Cancel should be successful.", true, result.WasSuccessful);
				AssertDocketWasCancelledOut(new BusinessObjectFactory().Load<WhsReceive>(amendedReceive.PK), 5m);

				// ensure the adjustment (amendment) was correctly created
				var receiveAdjustment = GetBondedAdjustmentFromExternalReference("B123", new BusinessObjectFactory());
				AssertNotNull(receiveAdjustment);
				AssertEquals((ZByte)0, receiveAdjustment.WD_ExternalReferenceSplit);
				AssertEquals(true, receiveAdjustment.IsFinalised);
			}
		}

		#endregion

		#region Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsWithHoldCode_HoldsStockAfterImport

		public void Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsWithHoldCode_HoldsStockAfterImport()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true, createInventory: false);

			var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWI, ServiceCode = ServiceCodeType.HLD } } };
			Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);
			AssertEquals("Precondition - hold service code is set for recipient role ", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First().ServiceCode);

			using (ObjectFactory.Substitute(PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive().Object))
			{
				var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
				Assert("Importing of a new receive should be a success.", importResult.WasSuccessful);

				var docketInOtherFactory = (WhsReceive)importResult.GetBizOForTesting(Data.ShipmentDataObject, new BusinessObjectFactory());
				Assert("Lines created", docketInOtherFactory.Lines.Count > 0);
				Assert("All stock must become Held.", docketInOtherFactory.Lines.All(l => l.WE_WHC_NKCurrentInventoryHeldCode == InventoryHoldCodes.Codes.Held));
			}
		}

		#endregion

		#region Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsWithHoldCode_ThenCancelJob

		public void Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsWithHoldCode_ThenCancelJob()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true, createInventory: false);

			var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWI, ServiceCode = ServiceCodeType.HLD } } };
			Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);
			AssertEquals("Precondition - hold service code is set for recipient role ", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First().ServiceCode);

			using (ObjectFactory.Substitute(PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive().Object))
			{
				var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
				Assert("Importing of a new receive should be a success.", importResult.WasSuccessful);

				var receiveInOtherFactory = (WhsReceive)importResult.GetBizOForTesting(Data.ShipmentDataObject, new BusinessObjectFactory());
				Assert("Lines created", receiveInOtherFactory.Lines.Count > 0);
				Assert("All stock must become Held.", receiveInOtherFactory.Lines.All(l => l.WE_WHC_NKCurrentInventoryHeldCode == InventoryHoldCodes.Codes.Held));

				var eventDataObject = Data.GetEventDataObject(Events.CancelTheWarehouseJob);
				var message = GetQueuedUniversalEventMessage(eventDataObject);
				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				var results = manager.Process(message).ImportResults;

				var result = results.Single(r => r.DataContextType == DataContextType.WarehouseReceive);
				AssertEquals("Cancel should be successful.", true, result.WasSuccessful);
				AssertDocketWasCancelledOut(new BusinessObjectFactory().Load<WhsReceive>(receiveInOtherFactory.PK), 10m);
			}
		}

		#endregion

		#region Test_CustomsSource_ImportOfExistingFinalisedJobWithMultipleLineChangesInVirtualWhs

		public void Test_CustomsSource_ImportOfExistingFinalisedJobWithMultipleLineChangesInVirtualWhs()
		{
			// create an existing receive originating from customs universal with multiple receive lines, and finalised it (virtual warehouse)
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true, createInventory: false);
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);

			var receive = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 1m, "EntryNumber123", 1, "EntryNumber456", 102);
			var line1 = receive.Lines[0];
			line1.WE_SerialNumber = "";
			var line2 = Helper.CreateWhsReceiveLine(receive, Data.Product, 2m, "EntryNumber123-2");
			var line3 = Helper.CreateWhsReceiveLine(receive, Data.Product, 3m, "EntryNumber123-3");
			Helper.AllocateLocationsWithMock(receive);

			FinaliseDocket(receive);

			// change Finalised Date to be 2 days ago
			var originalFinaliseDate = ZDateTimeOffset.Today.AddDays(-2);
			receive.WD_FinalisedDate = originalFinaliseDate;
			receive.Lines.ForEach(l => l.WE_FinalisedDate = originalFinaliseDate);
			Factory.SaveForTesting();

			AssertIsFinalisedPrecondition(receive);
			AssertEquals("Pre-condition: receive should have 3 lines.", 3, receive.Lines.Count);
			AssertReceiveHasLineWithDetails(receive, "FIN", originalFinaliseDate, 1m);
			AssertReceiveHasLineWithDetails(receive, "FIN", originalFinaliseDate, 2m);
			AssertReceiveHasLineWithDetails(receive, "FIN", originalFinaliseDate, 3m);

			// create a new customs amendment, change one of existing line (WE_TransactionQuantity change) as well as add a new line
			var invLineNo = Data.CustomsCommInvLineNoOnOrder;
			var commercialInvoiceLine1 = Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
			commercialInvoiceLine1.BondedWarehouseQuantity = 11m;
			commercialInvoiceLine1.EntryLineNumber = 1;
			var commercialInvoiceLine2 = Data.AddCommercialInvoiceLineWithRelatedEntryLine(2m, invLineNo + 1, Data.Product, 2, "EntryNumber123", 0, "");
			var commercialInvoiceLine3 = Data.AddCommercialInvoiceLineWithRelatedEntryLine(3m, invLineNo + 2, Data.Product, 3, "EntryNumber123", 0, "");
			var commercialInvoiceLine4 = Data.AddCommercialInvoiceLineWithRelatedEntryLine(14m, invLineNo + 3, Data.Product, 4, "EntryNumber123", 0, "");

			using (ObjectFactory.Substitute(PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive().Object))
			{
				// import above amendment, assert results (line units changed, and new line saved and whole docket is finalised)
				var result = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
				var importResult = result.Single(r => r.DataContextType == DataContextType.WarehouseReceive);
				var receive2 = (WhsReceive)importResult.GetBizOForTesting(Data.ShipmentDataObject, new BusinessObjectFactory());

				AssertEquals("Import should be successful.", true, importResult.WasSuccessful);
				AssertEquals("Should match existing Receive.", receive.PK, receive2.PK);
				AssertEquals("After amendment receive should have 4 lines.", 4, receive2.Lines.Count);
				AssertEquals("After amendment receive should be Finalised.", true, receive2.IsFinalised);
				AssertEquals("After amendment receive's Finalised Date should be original finalised date.", originalFinaliseDate, receive2.WD_FinalisedDate);

				AssertReceiveHasLineWithDetails(receive2, "FIN", originalFinaliseDate, 11m);
				AssertReceiveHasLineWithDetails(receive2, "FIN", originalFinaliseDate, 2m);
				AssertReceiveHasLineWithDetails(receive2, "FIN", originalFinaliseDate, 3m);
				AssertReceiveHasLineWithDetails(receive2, "FIN", originalFinaliseDate, 14m);
			}
		}

		void AssertReceiveHasLineWithDetails(WhsReceive receive, string expectedLineStatus, ZDateTimeOffset expectedFinalisedDate, decimal expectedUnits)
		{
			var message = $"Should find receive line with Status = {expectedLineStatus}, Finalised Date = {expectedFinalisedDate} and Units = {expectedUnits}";
			AssertEquals(message, 1, receive.Lines.Count(l => l.WE_DocketLineStatus == expectedLineStatus && l.WE_FinalisedDate == expectedFinalisedDate && l.WE_TransactionQuantity == expectedUnits));
		}

		#endregion

		#region Test_CustomsSource_ImportOfExistingFinalisedJobWithImportantChangesInVirtualWhsGetsRejected

		public void Test_CustomsSource_ImportOfExistingFinalisedJobWithImportantChangesInVirtualWhsGetsRejected()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);

			var docket = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 10m, "EntryNumber123", 2, "EntryNumber456", 102, finalise: true);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(docket);

			var mock = new Mock<IWhsReceiveCustomsAmendmentChecker>();
			mock.Setup(s => s.CanDoAnAmendment(It.IsAny<WhsReceive>(), It.IsAny<bool>())).Returns(false); // pretend that we can't do an amendment
			using (ObjectFactory.Substitute(nameof(IWhsReceiveCustomsAmendmentChecker), mock.Object))
			{
				var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).First();
				CombineAssertions(() =>
				{
					Assert("Changing stock-related properties should fail the import.", !importResult.WasSuccessful);
					Assert("Should be an explanation why amendment declined.", importResult.Logs.Any(l => l.Type == LogType.Error && l.Message == "Cannot do an amendment. Stock related properties affected."));
					Assert("Should say that changes were not saved.", importResult.Logs.Any(l => l.Message == "No changes were made due to the above errors. Please fix the errors and try again."));
				});
			}
		}

		#endregion

		#region Test_CustomsSource_ImportOfExistingFinalisedJobWithNonImportantChangesInVirtualWhsPutsStockOnHoldWhenHoldRoleProvided

		public void Test_CustomsSource_ImportOfExistingFinalisedJobWithNonImportantChangesInVirtualWhsPutsStockOnHoldWhenHoldRoleProvided()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			var docket = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 10m, "EntryNumber123", 2, "EntryNumber456", 102, finalise: true);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(docket);

			var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWI, ServiceCode = ServiceCodeType.HLD } } };
			Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);
			AssertEquals("Precondition - hold service code is set for recipient role ", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First().ServiceCode);

			Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].LineNo = 123;

			var mock = new Mock<IWhsReceiveCustomsAmendmentChecker>();
			mock.Setup(s => s.CanDoAnAmendment(It.IsAny<WhsReceive>(), It.IsAny<bool>())).Returns(true); // pretend that we can do an amendment of non-important fields
			using (ObjectFactory.Substitute(nameof(IWhsReceiveCustomsAmendmentChecker), mock.Object))
			{
				var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
				Assert("Changing irrelevant properties should NOT fail the import.", importResult.WasSuccessful);
				Assert("Should say that amendment is allowed.", importResult.Logs.Any(l => l.Type == LogType.Information && l.Message == "Amendment allowed - no immediate changes made, but relevant stock put on hold."));
			}

			var docketInOtherFactory = new BusinessObjectFactory().Load<WhsReceive>(docket.PK);
			Assert("Lines are still there", docketInOtherFactory.Lines.Count > 0);
			Assert("Shouldn't save any changes", docketInOtherFactory.Lines.All(line => line.WE_LineNo != 123));
			Assert("All stock must become Held.", docketInOtherFactory.Lines.All(l => l.WE_WHC_NKCurrentInventoryHeldCode == InventoryHoldCodes.Codes.Held));
		}

		#endregion

		#region Test_CustomsSource_ImportOfExistingFinalisedJobWithNonImportantChangesInVirtualWhsAppliedWhenNoHoldRoleProvided

		public void Test_CustomsSource_ImportOfExistingFinalisedJobWithNonImportantChangesInVirtualWhsAppliedWhenNoHoldRoleProvided()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);

			var docket = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 10m, "EntryNumber123", 2, "EntryNumber456", 102, finalise: true);
			Factory.SaveForTesting();
			var docketLine = docket.Lines.Single();
			docketLine.HeldCodeChangeQuantity = docketLine.WE_TransactionQuantity;
			docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			docketLine.ChangeInventoryHeldCode(true);
			AssertIsFinalisedPrecondition(docket);

			Assert("Precondition - hold service code is NOT set for recipient role", Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.All(r => !r.ServiceCode.HasValue));
			// this is an amendment:
			Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWHSOrderLineNumber = 123;

			var mock = new Mock<IWhsReceiveCustomsAmendmentChecker>();
			mock.Setup(s => s.CanDoAnAmendment(It.IsAny<WhsReceive>(), It.IsAny<bool>())).Returns(true); // pretend that we can do an amendment of non-important fields
			using (ObjectFactory.Substitute(nameof(IWhsReceiveCustomsAmendmentChecker), mock.Object))
			{
				var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
				Assert("Changing irrelevant properties should NOT fail the import.", importResult.WasSuccessful);
			}

			var docketInOtherFactory = new BusinessObjectFactory().Load<WhsReceive>(docket.PK);
			Assert("Should apply the change", docketInOtherFactory.Lines.Any(line => line.WE_LineNo == 123));
			Assert("All stock must become UnHeld.", docketInOtherFactory.Lines.All(l => l.WE_WHC_NKCurrentInventoryHeldCode == ""));
		}

		#endregion

		#region Test_CustomsSource_ImportOfExistingFinalisedJobWithImportantChangesInVirtualWhsGetsRejectedIfStockIsWithdrawn_EndToEnd

		public void Test_CustomsSource_ImportOfExistingFinalisedJobWithImportantChangesInVirtualWhsGetsRejectedIfStockIsWithdrawn_EndToEnd()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true, createInventory: false);

			using (ObjectFactory.Substitute(PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive().Object))
			{
				var results = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
				var receive1 = (WhsReceive)results.Single(DataContextMatchesWarehouseReceive).GetBizOForTesting(Data.ShipmentDataObject, new BusinessObjectFactory());
				AssertIsFinalisedPrecondition(receive1);

				var order = Helper.CreateWhsOrderWithOrderLine(Data.Orgs.CRAHOLSYD, Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true), Data.Product, 3m);
				PickAndFinaliseInDB(order);

				var invoiceLine = Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
				AssertEquals("Precondition - attribute value", "Red", invoiceLine.CustomizedFieldCollection[0].Value);
				// amend attribute value. It is not used in matching, so import will try to update existing line. This is important field, so amendment will be rejected as stock has been withdrawn.
				invoiceLine.CustomizedFieldCollection[0].Value = "Blue";

				var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single(DataContextMatchesWarehouseReceive);
				CombineAssertions(() =>
				{
					Assert("Changing stock-related properties should fail the import.", !importResult.WasSuccessful);
					Assert("Should be an explanation why amendment declined.", importResult.Logs.Any(l => l.Type == LogType.Error && l.Message == "Cannot do an amendment. Stock related properties affected."));
					Assert("Should say that changes were not saved.", importResult.Logs.Any(l => l.Message == "No changes were made due to the above errors. Please fix the errors and try again."));
				});
			}
		}

		#endregion

		#region Test_CustomsSource_ImportOfExistingFinalisedJobWithImportantChangesInVirtualWhsDoesNotGetRejectedIfStockOnAnotherLineIsWithdrawn_EndToEnd

		public void Test_CustomsSource_ImportOfExistingFinalisedJobWithImportantChangesInVirtualWhsDoesNotGetRejectedIfStockOnAnotherLineIsWithdrawn_EndToEnd()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true, createInventory: false);

			using (ObjectFactory.Substitute(PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive().Object))
			{
				var lineThatCanChange = Data.AddCommercialInvoiceLineWithRelatedEntryLine(10, Data.CustomsCommInvLineNoOnOrder + 1, Data.Product, 3, "EntryNumber123", 0, "");

				var results = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
				var receive1 = (WhsReceive)results.Single(r => r.DataContextType == DataContextType.WarehouseReceive).GetBizOForTesting(Data.ShipmentDataObject, new BusinessObjectFactory());
				AssertIsFinalisedPrecondition(receive1);

				var order = Helper.CreateWhsOrder(Data.Orgs.CRAHOLSYD, Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true));
				var orderLine = Helper.CreateWhsOrderLine(order, Data.Product, 3m, "EntryNumber123-2", "DummyOutward-1", "");
				PickAndFinaliseInDB(order);

				AssertEquals("Precondition - attribute value", "Red", lineThatCanChange.CustomizedFieldCollection[0].Value);
				// amend attribute value. It is not used in matching, so import will try to update existing line. This is important field, so amendment will be rejected as stock has been withdrawn.
				lineThatCanChange.CustomizedFieldCollection[0].Value = "Blue";

				var results2 = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
				var importResult = results2.Single(r => r.DataContextType == DataContextType.WarehouseReceive);
				var receive2 = (WhsReceive)importResult.GetBizOForTesting(Data.ShipmentDataObject, new BusinessObjectFactory());
				AssertEquals("Changing stock-related properties should not fail the import if no Stock Withdrawn.", true, importResult.WasSuccessful);
				AssertEquals("Should match existing Receive.", receive1.PK, receive2.PK);
				AssertEquals("Attribute Value should be updated.", "Blue", receive2.Lines.Single(l => l.WE_BondedEntryKey == "ENTRYNUMBER123-3").WE_PartAttrib1);
			}
		}

		#endregion

		#region Test_CustomsSource_ImportOfExistingFinalisedJobWithImportantChangesInVirtualWhsDoesNotGetRejectedIfStockIsNotWithdrawn_EndToEnd

		public void Test_CustomsSource_ImportOfExistingFinalisedJobWithImportantChangesInVirtualWhsDoesNotGetRejectedIfStockIsNotWithdrawn_EndToEnd()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			using (ObjectFactory.Substitute(PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive().Object))
			{
				var results1 = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
				var receive1 = (WhsReceive)results1.Single(r => r.DataContextType == DataContextType.WarehouseReceive).GetBizOForTesting(Data.ShipmentDataObject, new BusinessObjectFactory());
				AssertIsFinalisedPrecondition(receive1);

				var invoiceLine = Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
				AssertEquals("Precondition - attribute value", "Red", invoiceLine.CustomizedFieldCollection[0].Value);
				// amend attribute value. It is not used in matching, so import will try to update existing line. This is important field, but not stock is withdrawn so amendment should not be rejected.
				invoiceLine.CustomizedFieldCollection[0].Value = "Blue";

				var results2 = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
				var importResult = results2.Single(r => r.DataContextType == DataContextType.WarehouseReceive);
				var receive2 = (WhsReceive)importResult.GetBizOForTesting(Data.ShipmentDataObject, new BusinessObjectFactory());
				AssertEquals("Changing stock-related properties should not fail the import if no Stock Withdrawn.", true, importResult.WasSuccessful);
				AssertEquals("Should match existing Receive.", receive1.PK, receive2.PK);
				AssertEquals("Attribute Value should be updated.", "Blue", receive2.Lines.Single().WE_PartAttrib1);
			}
		}

		#endregion

		#region Test_CustomsSource_ImportOfExistingFinalisedJobWithUnImportantChangesInVirtualWhsGetsAccepted_EndToEnd

		public void Test_CustomsSource_ImportOfExistingFinalisedJobWithUnImportantChangesInVirtualWhsGetsAccepted_EndToEnd()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create an existing job originating from Customs universal
			Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);

			using (ObjectFactory.Substitute(PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive().Object))
			{
				var results = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
				var docket = (WhsReceive)results.Single(r => r.DataContextType == DataContextType.WarehouseReceive).GetBizOForTesting(Data.ShipmentDataObject, Factory.BOFactory);
				AssertIsFinalisedPrecondition(docket);
				AssertEquals("Precondition - Docket should be in database.", true, docket.IsInDatabase);
				var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWI, ServiceCode = ServiceCodeType.HLD } } };
				Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);
				AssertEquals("Precondition - hold service code is set for recipient role ", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First().ServiceCode);

				// Unimportant change:
				Data.ShipmentDataObject.SetDateCollection(() => new List<Date>());
				Data.ShipmentDataObject.DateCollection.Add(DateType.ExWorksRequiredBy, true, ZDateTime.Today);

				var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
				Assert("Changing irrelevant properties should NOT fail the import.", importResult.WasSuccessful);
				Assert("Should say that amendment is allowed.", importResult.Logs.Any(l => l.Type == LogType.Information && l.Message == "Amendment allowed - no immediate changes made, but relevant stock put on hold."));

				var docketInOtherFactory = (WhsReceive)importResult.GetBizOForTesting(Data.ShipmentDataObject, new BusinessObjectFactory());
				Assert("Shouldn't populate data from ShipmentDataObject", docketInOtherFactory.WD_ETD.IsEmpty); // DateType.ExWorksRequiredBy turned into this.
				Assert("Lines are still there", docketInOtherFactory.Lines.Count > 0);
				Assert("All stock must become Held.", docketInOtherFactory.Lines.All(l => l.WE_WHC_NKCurrentInventoryHeldCode == InventoryHoldCodes.Codes.Held));
			}
		}

		#endregion

		#region Test_CustomsSource_ImportOfExistingFinalisedJobWithUnImportantChangesInVirtualWhsAppliesChanges_EndToEnd

		[TestDate(2016, 10, 13)]
		public void Test_CustomsSource_ImportOfExistingFinalisedJobWithUnImportantChangesInVirtualWhsAppliesChanges_EndToEnd()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			using (ObjectFactory.Substitute(PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive().Object))
			{
				var results = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
				var docket = (WhsReceive)results.Single(r => r.DataContextType == DataContextType.WarehouseReceive).GetBizOForTesting(Data.ShipmentDataObject, Factory.BOFactory);
				AssertEquals("Precondition - Docket should be in database.", true, docket.IsInDatabase);
				var docketLine = docket.Lines.Single();
				docketLine.HeldCodeChangeQuantity = docketLine.WE_TransactionQuantity;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
				docketLine.ChangeInventoryHeldCode(true);
				Factory.SaveForTesting();
				AssertIsFinalisedPrecondition(docket);

				// Unimportant change:
				Data.ShipmentDataObject.SetDateCollection(() => new List<Date>());
				Data.ShipmentDataObject.DateCollection.Add(DateType.ExWorksRequiredBy, true, ZDateTime.Today);

				var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
				Assert("Changing irrelevant properties should NOT fail the import.", importResult.WasSuccessful);

				var docketInOtherFactory = (WhsReceive)importResult.GetBizOForTesting(Data.ShipmentDataObject, new BusinessObjectFactory());
				AssertEquals("Should populate data from ShipmentDataObject", new ZDateTimeOffset(2016, 10, 13, 0, 0, 0, TimeSpan.FromHours(10)), docketInOtherFactory.WD_ETD); // DateType.ExWorksRequiredBy turned into this.
				Assert("Lines are still there", docketInOtherFactory.Lines.Count > 0);
				Assert("All stock must become Un-held.", docketInOtherFactory.Lines.All(l => l.WE_WHC_NKCurrentInventoryHeldCode == ""));
			}
		}

		#endregion

		#region Test_CustomsSource_ImportOfExistingFinalisedJobWithStockIncreaseInVirtualWhsAppliesChanges_EndToEnd

		public void Test_CustomsSource_ImportOfExistingFinalisedJobWithStockIncreaseInVirtualWhsAppliesChanges_EndToEnd()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			using (ObjectFactory.Substitute(PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive().Object))
			{
				var results = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
				var docket = (WhsReceive)results.Single(r => r.DataContextType == DataContextType.WarehouseReceive).GetBizOForTesting(Data.ShipmentDataObject, Factory.BOFactory);
				AssertIsFinalisedPrecondition(docket);
				AssertEquals("Precondition - Docket should be in database.", true, docket.IsInDatabase);

				var docketLine = docket.Lines.Single();
				var originalWE_TransactionQuantity = docketLine.WE_TransactionQuantity;
				var originalWE_StockOnHand = docketLine.WE_StockOnHand;
				docketLine.HeldCodeChangeQuantity = docketLine.WE_TransactionQuantity;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
				docketLine.ChangeInventoryHeldCode(true);
				Factory.SaveForTesting();

				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity += 2; // increasing received Qty by 2

				var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
				Assert("Incresing stock should NOT fail the import.", importResult.WasSuccessful);

				var docketInOtherFactory = (WhsReceive)importResult.GetBizOForTesting(Data.ShipmentDataObject, new BusinessObjectFactory());
				var docketLineInOtherFactory = docketInOtherFactory.Lines.Single();
				AssertEquals("WE_TransactionQuantity increased by 2", originalWE_TransactionQuantity + 2, docketLineInOtherFactory.WE_TransactionQuantity);
				AssertEquals("WE_StockOnHand increased by 2", originalWE_StockOnHand + 2, docketLineInOtherFactory.WE_StockOnHand);
				AssertEquals("Stock must become Un-held.", ZString.Empty, docketLineInOtherFactory.WE_WHC_NKCurrentInventoryHeldCode);
			}
		}

		#endregion

		#region Test_CustomsSource_ImportOfExistingFinalisedJobWithStockDecreaseInVirtualWhsAppliesChanges_EndToEnd

		public void Test_CustomsSource_ImportOfExistingFinalisedJobWithStockDecreaseInVirtualWhsAppliesChanges_EndToEnd()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			using (ObjectFactory.Substitute(PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive().Object))
			{
				var results = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
				var docket = (WhsReceive)results.Single(r => r.DataContextType == DataContextType.WarehouseReceive).GetBizOForTesting(Data.ShipmentDataObject, Factory.BOFactory);
				AssertIsFinalisedPrecondition(docket);
				AssertEquals("Precondition - Docket should be in database.", true, docket.IsInDatabase);

				var docketLine = docket.Lines.Single();
				docketLine.HeldCodeChangeQuantity = docketLine.WE_TransactionQuantity;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
				docketLine.ChangeInventoryHeldCode(true);
				AssertEquals("Precondition", 5m, docketLine.WE_StockOnHand);
				Factory.SaveForTesting();

				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 2; // decreasing received qty from 5 to 2

				var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
				Assert("Decreasing stock should NOT fail the import (because it is not picked).", importResult.WasSuccessful);

				var docketInOtherFactory = (WhsReceive)importResult.GetBizOForTesting(Data.ShipmentDataObject, new BusinessObjectFactory());
				var docketLineInOtherFactory = docketInOtherFactory.Lines.Single();
				AssertEquals("WE_TransactionQuantity decreased by 3 (from 5 to 2)", 2m, docketLineInOtherFactory.WE_TransactionQuantity);
				AssertEquals("WE_StockOnHand decreased by 3 (from 5 to 2)", 2m, docketLineInOtherFactory.WE_StockOnHand);
				AssertEquals("Stock must become Un-held.", ZString.Empty, docketLineInOtherFactory.WE_WHC_NKCurrentInventoryHeldCode);
			}
		}

		#endregion

		#region Test_CustomsSource_ImportOfExistingFinalisedJobWithStockDecreaseBelowAvailableInVirtualWhsFails_EndToEnd

		public void Test_CustomsSource_ImportOfExistingFinalisedJobWithStockDecreaseBelowAvailableInVirtualWhsFails_EndToEnd()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true, createInventory: false);

			// create an existing job originating from Customs universal
			Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);

			using (ObjectFactory.Substitute(PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive().Object))
			{
				var results = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
				var docket = (WhsReceive)results.Single(DataContextMatchesWarehouseReceive).GetBizOForTesting(Data.ShipmentDataObject, Factory.BOFactory);
				AssertIsFinalisedPrecondition(docket);
				AssertEquals("Precondition - Docket should be in database.", true, docket.IsInDatabase);

				var docketLine = docket.Lines.Single();
				AssertEquals("Precondition", 5m, docketLine.WE_StockOnHand);

				var order = Helper.CreateWhsOrderWithOrderLine(docket.Client, docket.Warehouse, docketLine.SupplierPart, 3m);
				PickAndFinaliseInDB(order);
				AssertEquals("Precondition: pickline created", 1, order.Lines.Single().PickLines.Count);
				AssertEquals("Precondition: stock is Picked.", 2m, docketLine.AvailableToPickQuantity);

				Factory.SaveForTesting();

				Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 2; // decreasing received qty from 5 to 2

				var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single(DataContextMatchesWarehouseReceive);
				AssertEquals("Decreasing stock below available should fail the import.", false, importResult.WasSuccessful);
				Assert("Should be an explanation why amendment declined.", importResult.Logs.Any(l => l.Type == LogType.Error && l.Message == "Cannot do an amendment. Stock related properties affected."));
			}
		}

		#endregion

		#region Test_CustomsSource_ImportOfFinalisedJobInVirtualWhs_ChangingPerPackageQtyIsAllowedIfNoStockTakenFromInwards

		/// <summary>
		/// This tests the "can we make this amendment?" process that results in a hold if the amendment is ok.
		/// </summary>
		public void Test_CustomsSource_ImportOfFinalisedJobInVirtualWhs_ChangingPerPackageQtyIsAllowedIfNoStockTakenFromInwards_HoldCodeProvided()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

				// change warehouse to be in the US.
				var warehouseAddressDO1 = Data.ShipmentDataObject.OrganizationAddressCollection.Single(o => o.AddressType.GetValueOrDefault() == nameof(DocAddressType.CustomsWarehouseAddress));
				warehouseAddressDO1.Port = new UNLOCO { Code = "USLAX" };
				var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true, isBondedWarehouse: true);
				warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
				var product2 = Data.CreateProduct("P2");
				Helper.CreateWhsReceiveWithInventory(Data.Orgs.CRAHOLSYD, warehouse, "R2", product2, 100m, "ENTRYNUMBER123-3");
				Data.Product.OP_CountDecimalPlaces = 1;
				Factory.SaveForTesting();

				AddCommericalInvoiceLines(Data, product2, 10m, 5m);

				using (ObjectFactory.Substitute(PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive().Object))
				{
					var importResults1 = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
					var receive1 = (WhsReceive)importResults1.Single(r => r.DataContextType == DataContextType.WarehouseReceive).GetBizOForTesting(Data.ShipmentDataObject, new BusinessObjectFactory());
					AssertEquals(4, receive1.Lines.Count);
					Factory.SaveForTesting();

					CombineAssertions(delegate
					{
						var line1 = receive1.Lines.Single(l => l.WE_PackageGroupId == "W00000003-001" && l.WE_PerPackageQty == 1m);
						AssertProductAndUnits((WhsReceiveLine)line1, Data.Product, 5m);

						var line2 = receive1.Lines.Single(l => l.WE_PackageGroupId == "W00000003-002" && l.WE_PerPackageQty == 2.5m);
						AssertProductAndUnits((WhsReceiveLine)line2, Data.Product, 5m);

						var line3 = receive1.Lines.Single(l => l.WE_PackageGroupId == "W00000003-001" && l.WE_PerPackageQty == 4m);
						AssertProductAndUnits((WhsReceiveLine)line3, product2, 20m);

						var line4 = receive1.Lines.Single(l => l.WE_PackageGroupId == "" && l.WE_PerPackageQty == 0m);
						AssertProductAndUnits((WhsReceiveLine)line4, Data.Product, 1m);

						AssertEquals(false, Logger.HasErrors);
					});

					var newData = new TestDataForUniversal(Factory, Logger, DataContext);
					newData.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true, createInventory: false);

					var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.BWI, ServiceCode = ServiceCodeType.HLD } } };
					Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);

					// change warehouse to be in the US.
					var warehouseAddressDO2 = newData.ShipmentDataObject.OrganizationAddressCollection.Single(o => o.AddressType.GetValueOrDefault() == nameof(DocAddressType.CustomsWarehouseAddress));
					warehouseAddressDO2.Port = new UNLOCO { Code = "USLAX" };
					AddCommericalInvoiceLines(newData, product2, 15m, 10m);

					var importResults2 = GetImportResultsViaDataContextManager(newData.ShipmentDataObject);
					var receive2 = (WhsReceive)importResults2.Single(r => r.DataContextType == DataContextType.WarehouseReceive).GetBizOForTesting(newData.ShipmentDataObject, new BusinessObjectFactory());
					AssertEquals("Should hold the existing receive.", receive1.PK, receive2.PK);
					AssertEquals(4, receive2.Lines.Count);
					Factory.SaveForTesting();

					CombineAssertions(delegate
					{
						var line1 = receive2.Lines.Single(l => l.WE_PackageGroupId == "W00000003-001" && l.WE_PerPackageQty == 1m);
						AssertProductAndUnits((WhsReceiveLine)line1, Data.Product, 5m);
						AssertEquals("Inventory should be held.", InventoryStatus.Codes.Held, line1.WE_CurrentInventoryStatus);

						var line2 = receive2.Lines.Single(l => l.WE_PackageGroupId == "W00000003-002" && l.WE_PerPackageQty == 2.5m);
						AssertProductAndUnits((WhsReceiveLine)line2, Data.Product, 5m);
						AssertEquals("Inventory should be held.", InventoryStatus.Codes.Held, line2.WE_CurrentInventoryStatus);

						var line3 = receive2.Lines.Single(l => l.WE_PackageGroupId == "W00000003-001" && l.WE_PerPackageQty == 4m);
						AssertProductAndUnits((WhsReceiveLine)line3, product2, 20m);
						AssertEquals("Inventory should be held.", InventoryStatus.Codes.Held, line3.WE_CurrentInventoryStatus);

						var line4 = receive2.Lines.Single(l => l.WE_PackageGroupId == "" && l.WE_PerPackageQty == 0m);
						AssertProductAndUnits((WhsReceiveLine)line4, Data.Product, 1m);
						AssertEquals("Inventory should be held.", InventoryStatus.Codes.Held, line4.WE_CurrentInventoryStatus);

						AssertEquals(false, Logger.HasErrors);
					});
				}
			}
		}

		/// <summary>
		/// This tests the "make this amendment" process that results in cancellation of the original receipt, and replacement with a new receipt (with the new quantities/data).
		/// </summary>
		public void Test_CustomsSource_ImportOfFinalisedJobInVirtualWhs_ChangingPerPackageQtyIsAllowedIfNoStockTakenFromInwards_NoHoldCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

				// change warehouse to be in the US.
				var warehouseAddressDO1 = Data.ShipmentDataObject.OrganizationAddressCollection.Single(o => o.AddressType.GetValueOrDefault() == nameof(DocAddressType.CustomsWarehouseAddress));
				warehouseAddressDO1.Port = new UNLOCO { Code = "USLAX" };
				var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true, isBondedWarehouse: true);
				warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
				var product2 = Data.CreateProduct("P2");
				Helper.CreateWhsReceiveWithInventory(Data.Orgs.CRAHOLSYD, warehouse, "R2", product2, 100m, "ENTRYNUMBER123-3");
				Data.Product.OP_CountDecimalPlaces = 1;
				Factory.SaveForTesting();

				AddCommericalInvoiceLines(Data, product2, 10m, 5m);

				using (ObjectFactory.Substitute(PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive().Object))
				{
					var importResults1 = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
					var receive1 = (WhsReceive)importResults1.Single(r => r.DataContextType == DataContextType.WarehouseReceive).GetBizOForTesting(Data.ShipmentDataObject, new BusinessObjectFactory());
					AssertEquals(4, receive1.Lines.Count);
					Factory.SaveForTesting();

					CombineAssertions(delegate
					{
						var line1 = receive1.Lines.Single(l => l.WE_PackageGroupId == "W00000003-001" && l.WE_PerPackageQty == 1m);
						AssertProductAndUnits((WhsReceiveLine)line1, Data.Product, 5m);

						var line2 = receive1.Lines.Single(l => l.WE_PackageGroupId == "W00000003-002" && l.WE_PerPackageQty == 2.5m);
						AssertProductAndUnits((WhsReceiveLine)line2, Data.Product, 5m);

						var line3 = receive1.Lines.Single(l => l.WE_PackageGroupId == "W00000003-001" && l.WE_PerPackageQty == 4m);
						AssertProductAndUnits((WhsReceiveLine)line3, product2, 20m);

						var line4 = receive1.Lines.Single(l => l.WE_PackageGroupId == "" && l.WE_PerPackageQty == 0m);
						AssertProductAndUnits((WhsReceiveLine)line4, Data.Product, 1m);

						AssertEquals(false, Logger.HasErrors);
					});

					var newData = new TestDataForUniversal(Factory, Logger, DataContext);
					newData.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true, createInventory: false);

					// change warehouse to be in the US.
					var warehouseAddressDO2 = newData.ShipmentDataObject.OrganizationAddressCollection.Single(o => o.AddressType.GetValueOrDefault() == nameof(DocAddressType.CustomsWarehouseAddress));
					warehouseAddressDO2.Port = new UNLOCO { Code = "USLAX" };
					AddCommericalInvoiceLines(newData, product2, 15m, 10m);

					var importResults2 = GetImportResultsViaDataContextManager(newData.ShipmentDataObject);
					var receive2 = (WhsReceive)importResults2.Single(r => r.DataContextType == DataContextType.WarehouseReceive).GetBizOForTesting(newData.ShipmentDataObject, new BusinessObjectFactory());
					AssertNotEquals("Should create a new Receive.", receive1.PK, receive2.PK);
					AssertDocketWasCancelledOut(new BusinessObjectFactory().Load<WhsReceive>(receive1.PK), 0);
					AssertEquals("Should create a new Receive.", (ZByte)1, receive2.WD_ExternalReferenceSplit);
					AssertEquals(4, receive2.Lines.Count);
					Factory.SaveForTesting();

					CombineAssertions(delegate
					{
						var line1 = receive2.Lines.Single(l => l.WE_PackageGroupId == "W00000004-001" && l.WE_PerPackageQty == 2m);
						AssertProductAndUnits((WhsReceiveLine)line1, newData.Product, 10m);

						var line2 = receive2.Lines.Single(l => l.WE_PackageGroupId == "W00000004-002" && l.WE_PerPackageQty == 2.5m);
						AssertProductAndUnits((WhsReceiveLine)line2, newData.Product, 5m);

						var line3 = receive2.Lines.Single(l => l.WE_PackageGroupId == "W00000004-001" && l.WE_PerPackageQty == 4m);
						AssertProductAndUnits((WhsReceiveLine)line3, product2, 20m);

						var line4 = receive2.Lines.Single(l => l.WE_PackageGroupId == "" && l.WE_PerPackageQty == 0m);
						AssertProductAndUnits((WhsReceiveLine)line4, newData.Product, 1m);

						AssertEquals(false, Logger.HasErrors);
					});
				}
			}
		}

		void AddCommericalInvoiceLines(TestDataForUniversal data, OrgSupplierPart product, ZDecimal line1OrderedQuantity, ZDecimal line1PackedQuantity)
		{
			var entryLineNo = data.LastUsedEntryLineNo;
			var commercialInvoiceLine1 = data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
			var commercialInvoiceLine2 = data.AddCommercialInvoiceLineWithRelatedEntryLine(20m, data.CustomsCommInvLineNoOnOrder + 1, product);
			var commercialInvoiceLine3 = data.AddCommercialInvoiceLineWithRelatedEntryLine(1m, data.CustomsCommInvLineNoOnOrder + 2, data.Product, entryLineNo, "EntryNumber123", 0, "");
			commercialInvoiceLine1.BondedWarehouseQuantity = line1OrderedQuantity;
			commercialInvoiceLine2.CustomizedFieldCollection = null;
			SetupPerPackageQtyAndPackageGroupId(data.ShipmentDataObject, commercialInvoiceLine1, packageGroupID: 1, packageAmount: 5, packedQty: line1PackedQuantity);
			SetupPerPackageQtyAndPackageGroupId(data.ShipmentDataObject, commercialInvoiceLine1, packageGroupID: 2, packageAmount: 2, packedQty: 5m);
			SetupPerPackageQtyAndPackageGroupId(data.ShipmentDataObject, commercialInvoiceLine2, packageGroupID: 1, packageAmount: 5, packedQty: 20m);
			SetupPerPackageQtyAndPackageGroupId(data.ShipmentDataObject, commercialInvoiceLine3, packageGroupID: 2, packageAmount: 2, packedQty: 1m);
		}

		#endregion

		#region Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsWithUpdatedEntryNumber

		public void Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsWithUpdatedEntryNumber()
		{
			var recipientRoles1 = new[] { new RecipientRoleDetail { Type = GetRecipientRoleType(), ServiceCode = ServiceCodeType.HLD } };
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true, recipientRoles: recipientRoles1);
			Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].EntryNumber = "<PENDINGCUSTOMSRESPONSE>";

			// create an existing job originating from Customs universal
			var importResult1 = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
			AssertEquals(true, importResult1.WasSuccessful);

			var docket = Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketID, importResult1.DataContextKey));
			AssertEquals("Precondition - Docket should be Finalised.", true, docket.IsFinalised);

			// changing Entry Number
			Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].EntryNumber = "2300018079";

			var recipientRoles2 = new[] { new RecipientRoleDetail { Type = GetRecipientRoleType(), ServiceCode = ServiceCodeType.AMD } };
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = recipientRoles2 });

			var importResult2 = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
			AssertEquals(true, importResult2.WasSuccessful);

			var docketInOtherFactory = new BusinessObjectFactory().Load<WhsReceive>(docket.PK);
			AssertEquals("Original EntryNumber should be changed by this amendment because it should amend virtual warehouse successfully.", "2300018079", docketInOtherFactory.Lines[0].CustomsData.WB_EntryKey);
		}

		#endregion

		// Customs Import tests - Events

		#region Test_CustomsSource_OnUniversalEventAdded_CancelsDocketWhenCancelEventIsAdded

		protected override void TestDocketIsCancelledInRealWhs(WhsReceive receive)
		{
			var inventory = receive.Inventory[0];
			AssertEquals("Cancelling Receive should have cleared all locations.", true, inventory.WI_WL.IsEmpty);
			AssertEquals("Cancelling Receive should have reduced all stock to Zero.", 0m, inventory.WI_TotalUnits);
		}

		#endregion

		#region Test_CustomsSource_OnUniversalEventAdded_WhenCancelFailsJobIsNotCancelled

		public void Test_CustomsSource_OnUniversalEventAdded_WhenCancelFailsJobIsNotCancelled()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create an existing receive originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			var receive = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 10m, "EntryNumber123", 2, "EntryNumber456", 102, finalise: true);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(Data.Orgs.CRAHOLSYD, warehouse, Data.Product, 107m); // only 3 left
			PickAndFinaliseInDB(order);

			// End to End Test to have the entire universal import done in a separate factory (test would pass with bug otherwise)
			try
			{
				DummyWithWorkflow.TypeDecider.TypeForLoadOverride = typeof(DummyWithWorkflow);
				var poke = DummyWorkflowDescriptor.Instance;
				var dummyDeclaration = Factory.New<DummyWithWorkflow>();

				using (DummyWithWorkflowDataContextManager.SetWriterDecider(new DummyCustomsWriterDecider(Data)))
				{
					// this is what Customs do in production:
					var factory = new BusinessObjectFactory();
					PublishUniversalXmlResult events;
					using (factory.AddDisposableService())
					{
						events = UniversalXmlWorkflowProcessor.PublishUniversalEvent(factory, new[] { RecipientRoleType.BWI }, dummyDeclaration, dummyDeclaration.GetLogs().AddNew(Events.CancelTheWarehouseJob));
						factory.Save();
					}
					var result = PublishToUniversalResult.New(events, DataContextType.WarehouseReceive, "");
					AssertEquals("Cancel of Receive should have failed because stock is already allocated to an Order.", UniversalResult.HadErrors, result.ResultType);

					var otherFactory = new BusinessObjectFactory();
					var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);
					AssertNotNull("Even though Cancel fails the customs event should still be applied.", receiveInOtherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelTheWarehouseJobCode).SingleOrDefault());
					AssertNull("The Job Cancelled Event should *not* be applied because the Cancel failed.", receiveInOtherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).SingleOrDefault());
					AssertEquals("Receive should *not* be cancelled.", false, receiveInOtherFactory.IsCancelled);

					var adjustment = GetBondedAdjustmentFromExternalReference("B123", otherFactory);
					AssertNull("No adjustment should have been created as the cancel failed.", adjustment);
				}
			}
			finally
			{
				DummyWithWorkflow.TypeDecider.TypeForLoadOverride = null;
			}
		}

		#region class DummyCustomsWriterDecider

		class DummyCustomsWriterDecider : DummyWithWorkflowDataContextManager.DummyWriterDecider
		{
			public DummyCustomsWriterDecider(TestDataForUniversal data)
			{
				Data = data;
			}

			readonly TestDataForUniversal Data;

			public override IEventDataObjectWriter GetEventWriter(IDataWritingManager writeManager)
			{
				return new DummyCustomsEventWriter(writeManager, Data);
			}

			class DummyCustomsEventWriter : EventDataObjectWriter, IEventDataObjectWriter
			{
				public DummyCustomsEventWriter(IDataWritingManager writeManager, TestDataForUniversal data)
					: base(writeManager)
				{
					Data = data;
				}

				readonly TestDataForUniversal Data;

				ITopLevelDataObject ITopLevelDataObjectWriter.GetDataObject(BusinessObject parentBO)
				{
					var log = (StmALog)parentBO;
					return Data.GetEventDataObject(Events.All[log.SL_SE_NKEvent]);
				}
			}
		}

		#endregion

		#endregion

		//

		#region TestWarehouse_DestinationWarhouseAddressSpecified

		public void TestWarehouse_DestinationWarhouseAddressSpecified()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs1 = Data.GetOrCreateWarehouseInDB();
			var whsAddress = whs1.WarehouseAddress;
			whsAddress.Header.OH_IsWarehouseClient = true;

			var whs2 = Helper.CreateWarehouse("ZZZ", "ZZZ Warehouse");

			Factory.SaveForTesting();
			AssertEquals("Warehouse 1 is active.", true, whs1.WW_IsActive);
			AssertEquals("Warehouse 2 is active.", true, whs2.WW_IsActive);

			// Add Warehouse address to the order.
			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_INTHEMSYD(DocAddressType.DestinationWarehouse));

			Assert("Precondition: there is a destination warehouse address.", ShipmentDataObject.OrganizationAddressCollection.Any(address => address.AddressType.Value == "DestinationWarehouse"));

			var order = ShipmentDataObject.Order;
			order.Warehouse = new UniversalDataBuss.DataObjects.Universal.Warehouse { Code = "ZZZ" };
			order.OrderNumber = "ORDERME";
			order.TotalUnits = 10m;
			order.UnitsSent = 10m;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertNotNull("Order is read into business object", whsOrderBO);
				AssertEquals("Correct warehouse is assigned.", whs1.WW_WarehouseCode, whsOrderBO.Warehouse.WW_WarehouseCode);
				AssertEquals("Correct warehouse is assigned.", whs1.PK, whsOrderBO.WD_WW_Whs);
			});
		}

		public void TestWarehouse_DestinationWarhouseAddressSpecified_NotValidWarehouse()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs1 = Data.GetOrCreateWarehouseInDB();
			var whsAddress = whs1.WarehouseAddress;
			whsAddress.Header.OH_IsWarehouseClient = true;

			var whs2 = Helper.CreateWarehouse("ZZZ", "ZZZ Warehouse", client.MainAddress, client.Branch);
			whs2.WW_IsActive = false;

			Factory.SaveForTesting();
			AssertEquals("Warehouse 1 is active.", true, whs1.WW_IsActive);
			AssertEquals("Warehouse 2 is inactive.", false, whs2.WW_IsActive);

			// Add Warehouse address to the order.
			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_INTHEMSYD(DocAddressType.Warehouse));
			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_CRAHOLSYD(DocAddressType.DestinationWarehouse));

			Assert("Precondition: there is a destination warehouse address.", ShipmentDataObject.OrganizationAddressCollection.Any(address => address.AddressType.Value == "DestinationWarehouse"));
			Assert("Precondition: there is a warehouse address.", ShipmentDataObject.OrganizationAddressCollection.Any(address => address.AddressType.Value == "Warehouse"));

			var order = ShipmentDataObject.Order;
			order.Warehouse = new UniversalDataBuss.DataObjects.Universal.Warehouse { Code = "ABC" };
			order.OrderNumber = "ORDERME";
			order.TotalUnits = 10m;
			order.UnitsSent = 10m;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertNotNull("Order is read into business object", whsOrderBO);
				AssertEquals("Correct warehouse is assigned.", whs1.WW_WarehouseCode, whsOrderBO.Warehouse.WW_WarehouseCode);
				AssertEquals("Correct warehouse is assigned.", whs1.PK, whsOrderBO.WD_WW_Whs);
			});
		}

		public void TestWarehouse_NoDestinationWarhouseAddressSpecified()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs1 = Data.GetOrCreateWarehouseInDB();
			var whsAddress = whs1.WarehouseAddress;
			whsAddress.Header.OH_IsWarehouseClient = true;

			Factory.SaveForTesting();
			AssertEquals("Warehouse 1 is active.", true, whs1.WW_IsActive);

			AssertEquals("Precondition: there is no destination warehouse address.", false, ShipmentDataObject.OrganizationAddressCollection.Any(address => address.AddressType.Value == "DestinationWarehouse"));

			var order = ShipmentDataObject.Order;
			order.Warehouse = new UniversalDataBuss.DataObjects.Universal.Warehouse { Code = whs1.WW_WarehouseCode };
			order.OrderNumber = "ORDERME";
			order.TotalUnits = 10m;
			order.UnitsSent = 10m;

			var reader = GetNewReader(ShipmentDataObject, Logger);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertNotNull("Order is read into business object", whsOrderBO);
				AssertEquals("Correct warehouse is assigned.", whs1.WW_WarehouseCode, whsOrderBO.Warehouse.WW_WarehouseCode);
				AssertEquals("Correct warehouse is assigned.", whs1.PK, whsOrderBO.WD_WW_Whs);
			});
		}

		#endregion

		#region TestCustomsSourceImportFinalisesDocket_RejectsImportIfUnableToFinalise_DPSMatched

		protected override string ExpectedDPSMatchedExceptionMessage => @"
Cannot Import Receipt
Receipt could not be finalized into the Warehouse for Customs Job B123 because of the following error(s):
Error: Finalise
Error - Warehouse Receipt: Cannot finalize when Denied Party Screening is matched.".Trim();

		#endregion

		#region TestImport_CannotCancelWarehouseNonBondedJob

		public void TestImport_CannotCancelWarehouseNonBondedJob_SetCancelStatusFail()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();
			var part = Helper.CreateProduct(client, "P1");

			var dockDoorLocation = whs.FindLocation("DockA");
			var nonDockDoorLocation = whs.FindLocation("A-1-1-1");

			var receive = Helper.CreateWhsReceive(client, whs, "ORDER123");
			var receiveLine = Helper.CreateInventoryForDockDoorLocation(receive, part, dockDoorLocation, "A", 10m);
			receive.WD_DocketStatus = DocketStatus.Codes.Entered;
			receive.WD_DocketID = "W00000002";
			receive.WD_TransportReference = "TRANS123";

			var transfer = Helper.CreateWhsTransfer(client, whs);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, part, dockDoorLocation, nonDockDoorLocation, "A", 10m);
			transfer.RunPreSaveValidation();
			transferLine.PickedTime = ZDateTimeOffset.Now;

			Factory.SaveForTesting();
			AssertTestImport_CannotCancelWarehouseNonBondedJob_SetCancelStatusFail(receive, "The warehouse WhsReceive - W00000002 failed set to canceled because You cannot cancel receives that have an active putaway transfer.");
		}

		#endregion

		#region Implementation

		protected override WhsReceiveDataObjectReader GetNewReader(UniversalShipment shipmentDataObject, IXmlImportLogger logger, bool useCleanFactory = false)
		{
			return new WhsReceiveDataObjectReader(shipmentDataObject, logger, useCleanFactory ? new UniversalObjectFactory() : Factory);
		}

		protected override WhsOrderAndReceiveDataObjectWriter<WhsReceive> GetNewWriter(WhsReceive receive)
		{
			return new WhsReceiveDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receive)));
		}

		protected override string GetDocketType()
		{
			return "Receipt";
		}

		protected override WhsReceive GetNewDocket(OrgHeader client, WhsWarehouse warehouse, string externalReference = "")
		{
			return Helper.CreateWhsReceive(client, warehouse, externalReference);
		}

		protected override WhsDocket GetNewDocketOfDifferentType()
		{
			return Factory.NewWithValidTestData<WhsOrder>();
		}

		protected override string GetProcessType()
		{
			return "WIN";
		}

		protected override WhsReceive GetNewDocketWithLineCore(OrgHeader org, WhsWarehouse whs, ZString externalRef, OrgSupplierPart part, ZDecimal qty)
		{
			var receive = Helper.CreateWhsReceiveWithInventory(org, whs, externalRef, ZDateTimeOffset.Now, part, qty, finalise: false);
			receive.SupplierDocAddress.OrganisationPK = Data.Orgs.CRAHOLSYD.PK;
			return receive;
		}

		protected override void SetupDocketLineForCustoms(WhsReceiveLine line, ZString inwardsBondedEntryKey, ZShort inwardsEntryLineNo, ZString outwardsBondedEntryKey, ZShort outwardsEntryLineNo)
		{
			var inventory = line.Inventory[0];
			inventory.WI_BondedEntryKey = inwardsBondedEntryKey + "-" + inwardsEntryLineNo;
			inventory.CustomsData.WB_EntryKey = inwardsBondedEntryKey; // ignore outwards
			inventory.CustomsData.WB_EntryLineNo = inwardsEntryLineNo; // ignore outwards

			// this would normally be done by Receipt.RunPreSaveValidation (yuck)
			line.WE_BondedEntryKey = inventory.WI_BondedEntryKey;
			line.CustomsData.WB_EntryKey = inventory.CustomsData.WB_EntryKey;
			line.CustomsData.WB_EntryLineNo = inventory.CustomsData.WB_EntryLineNo;
			line.WE_F3_NKPackType = "BOX";

			var relationship = line.SupplierPart.RelatedOrganisations[0];
			if (relationship.OU_UsePartAttrib1)
			{
				line.WE_PartAttrib1 = "Red";
			}
			if (relationship.OU_UsePartAttrib2)
			{
				line.WE_PartAttrib2 = "Medium";
			}
			if (relationship.OU_UsePartAttrib3)
			{
				line.WE_PartAttrib3 = "S1234";
			}
			if (relationship.OU_UseSerialNumber)
			{
				line.WE_SerialNumber = "SERNUM";
			}
		}

		protected override void FinaliseDocket(WhsReceive receive)
		{
			receive.FinaliseDocketWithoutUserConfirmation();
		}

		protected override RecipientRoleType GetRecipientRoleType()
		{
			return RecipientRoleType.BWI;
		}

		internal static void AssertContents(WhsReceive whsReceiveBO)
		{
			AssertEquals("whsReceiveBO.WD_F3_NKTotalPackType", "CTN", whsReceiveBO.WD_F3_NKTotalPackType);
			AssertEquals("whsReceiveBO.WD_PackagesSent", 3, whsReceiveBO.WD_PackagesSent);
			AssertEquals("whsReceiveBO.WD_TotalPallets", new ZShort(4), whsReceiveBO.WD_TotalPallets);
			AssertEquals("whsReceiveBO.WD_BOLNo", "BILL", whsReceiveBO.WD_BOLNo);
			AssertEquals("whsReceiveBO.WD_CustomerReference", "CUSTOMER", whsReceiveBO.WD_CustomerReference);
			AssertEquals("whsReceiveBO.WD_ArrivalDate", new ZDateTimeOffset(2011, 1, 2), whsReceiveBO.WD_ArrivalDate);
			AssertEquals("whsReceiveBO.WD_BookingDate", new ZDateTimeOffset(2011, 1, 4), whsReceiveBO.WD_BookingDate);
			AssertEquals("whsReceiveBO.WD_ETA", new ZDateTimeOffset(2011, 1, 1), whsReceiveBO.WD_ETA);
			AssertEquals("whsReceiveBO.WD_ETD", new ZDateTimeOffset(2011, 1, 3), whsReceiveBO.WD_ETD);

			AssertEquals("whsReceiveBO.WD_DocketStatus was not set", "ENT", whsReceiveBO.WD_DocketStatus);
			AssertEquals("whsReceiveBO.WD_DocketSubType", "CUS", whsReceiveBO.WD_DocketSubType);
			AssertEquals("whsReceiveBO.WD_DropMode", "DRO", whsReceiveBO.WD_DropMode);
			AssertEquals("whsReceiveBO.WD_ExternalReference", "ORDERME", whsReceiveBO.WD_ExternalReference);
			AssertEquals("whsReceiveBO.WD_ExternalReferenceSplit", new ZByte(1), whsReceiveBO.WD_ExternalReferenceSplit);
			AssertEquals("whsReceiveBO.WD_PL_NKCarrierServiceLevel", "SET", whsReceiveBO.WD_PL_NKCarrierServiceLevel);
			AssertEquals("whsReceiveBO.WD_RS_NKServiceLevel", "TSL", whsReceiveBO.WD_RS_NKServiceLevel);
			AssertEquals("whsReceiveBO.WD_TotalCubic", 11.2m, whsReceiveBO.WD_TotalCubic);
			AssertEquals("whsReceiveBO.WD_TotalCubicUnit stayed as Default", "M3", whsReceiveBO.WD_TotalCubicUnit);
			AssertEquals("whsReceiveBO.WD_TotalUnits", 12.3m, whsReceiveBO.WD_TotalUnits);
			AssertEquals("whsReceiveBO.WD_TotalWeight was not set", 15.3m, whsReceiveBO.WD_TotalWeight);
			AssertEquals("whsReceiveBO.WD_TotalWeightUnit stayed as Default", "KG", whsReceiveBO.WD_TotalWeightUnit);
			AssertEquals("whsReceiveBO.WD_TransportReference", "TRANS", whsReceiveBO.WD_TransportReference);
			AssertEquals("whsReceiveBO.Warehouse.WW_WarehouseCode", "WHS", whsReceiveBO.Warehouse.WW_WarehouseCode);
		}

		protected override DataContextType DataContext
		{
			get { return DataContextType.WarehouseReceive; }
		}

		protected override OrgHeader GetOwnerForChangeOfOwnershipTests(TestDataForUniversal data)
		{
			return data.Orgs.WUFSHIJNB;
		}

		protected override OrgSupplierPart GetProductForChangeOfOwnershipTests(TestDataForUniversal data)
		{
			return data.ProductWUFSHIJNB;
		}

		#endregion
	}
}
