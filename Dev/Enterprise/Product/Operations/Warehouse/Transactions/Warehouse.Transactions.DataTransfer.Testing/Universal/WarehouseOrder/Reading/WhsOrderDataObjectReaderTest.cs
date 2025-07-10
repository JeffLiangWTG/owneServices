using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Bonded;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing.WhsBondedChangeOfInventoryDataObjectReaderTest;
using Bonded = Enterprise.Warehouse.Integration.BondedWarehouse;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsOrderDataObjectReaderTest : WhsOrderAndReceiveDataObjectReaderTest<WhsOrder, WhsOrderLine, WhsOrderDataObjectReader>
	{
		#region PopulateIsResidential

		public void TestPopulateIsResidential()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.ConsigneeAddress),
				OrganizationCode = "CONSIGNEE",
				CompanyName = "CONSIGNEE",
				IsResidential = true
			});
			ShipmentDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.DropOffAddress),
				OrganizationCode = "DROPOFF11",
				CompanyName = "DropOffAddress",
				IsResidential = true
			});

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			Assert("ConsigneeAddress is residential", whsOrderBO.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeAddress).E2_IsResidential);
			Assert("DropOffAddress is residential", whsOrderBO.DocAddresses.FindByDocAddressType(DocAddressType.DropOffAddress).E2_IsResidential);
		}

		public void TestPopulateIsResidential_WithDifferentSourceAddresses()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			ShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "REFERENCE");

			ShipmentDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress),
				OrganizationCode = "CONSIGNEE",
				CompanyName = "CONSIGNEE",
				IsResidential = true
			});
			ShipmentDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.DepartureCFSAddress),
				OrganizationCode = "DEPCFSADD",
				CompanyName = "Departure CFS Address",
				IsResidential = true
			});
			ShipmentDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = AddressTypes.PickupLocalCartage,
				OrganizationCode = "PICLOCCAR",
				CompanyName = "Pickup Local Cartage",
				IsResidential = true
			});

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			Assert("ConsigneeAddress is residential", whsOrderBO.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeAddress).E2_IsResidential);
			Assert("DropOffAddress is residential", whsOrderBO.DocAddresses.FindByDocAddressType(DocAddressType.DropOffAddress).E2_IsResidential);
			Assert("TransportCompanyDocumentaryAddress is residential", whsOrderBO.DocAddresses.FindByDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress).E2_IsResidential);
		}

		#endregion

		#region TestPopulateSalesChannel

		public void TestPopulateSalesChannel()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			Helper.CreateWhsSalesChannel("TST", "Fishing");
			Helper.CreateWhsSalesChannel("CCC", "CICOCUC");
			Factory.SaveForTesting();

			ShipmentDataObject.Order.SalesChannel = new CodeDescriptionPair { Code = "CCC", Description = "CICOCUC" };

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("WhsOrderBO.SalesChannel.WSH_Code", "CCC", whsOrderBO.SalesChannel.WSH_Code);
			AssertEquals("WhsOrderBO.SalesChannel.WSH_Description", "CICOCUC", whsOrderBO.SalesChannel.WSH_Description);
		}

		public void TestPopulateSalesChannel_Error()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			Factory.SaveForTesting();

			ShipmentDataObject.Order.SalesChannel = new CodeDescriptionPair { Code = "CCC", Description = "CICOCUC" };

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			AssertExceptionThrown("Cannot import Order without valid Sales Channel Code.", typeof(DataObjectReadFailureException),
				"Cannot Import Warehouse Order Job -- as the provided Sales Channel Code (CCC) does not match any known Sales Channels. Please check value or add the required one.", () => reader.ReadIntoBusinessObject());
		}

		public void TestPopulateSalesChannel_Empty_Clear()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			Helper.CreateWhsSalesChannel("CCC", "CICOCUC");
			Factory.SaveForTesting();

			ShipmentDataObject.Order.SalesChannel = new CodeDescriptionPair { Code = "CCC", Description = "CICOCUC" };

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("WhsOrderBO.SalesChannel.WSH_Code", "CCC", whsOrderBO.SalesChannel.WSH_Code);
			AssertEquals("WhsOrderBO.SalesChannel.WSH_Description", "CICOCUC", whsOrderBO.SalesChannel.WSH_Description);

			ShipmentDataObject.Order.SalesChannel = new CodeDescriptionPair { Code = "", Description = "" };
			reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertNull(whsOrderBO.SalesChannel);
		}

		#endregion

		#region TestPopulateAddress_UpdatesRateTransportZone

		public void TestPopulateConsigneeAddress_SetsRateTransportZone()
		{
			var org = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();
			var part = Data.Product;

			var whsOrderBOToUpdate = Helper.CreateWhsOrderWithOrderLine(org, whs, part, 10m);
			whsOrderBOToUpdate.WD_DocketID = "W00000002";
			whsOrderBOToUpdate.WD_ExternalReference = "ORDERME";
			Factory.SaveForTesting();

			whsOrderBOToUpdate.TransportCoPK = org.PK;
			AssertEquals(ZGuid.Empty, whsOrderBOToUpdate.WD_TZ_TransportZone);
			Factory.SaveForTesting();

			SetupMainOrgAddress(org, "20 Maxwell Street", "", "PERTH", "6162", "WA", "AUBYW");
			var rateTransportProvider = Helper.SetUpRateTransportProvider(true, "OPS", "AU", "ALL", org);
			var rateTransportZonePerth = Helper.SetUpRateTransportZone(rateTransportProvider, true, "Perth");
			Helper.SetUpRateTransportZoneItem(rateTransportZonePerth, "AU", "6162");
			AssertEquals(ZGuid.Empty, whsOrderBOToUpdate.WD_TZ_TransportZone);
			Factory.SaveForTesting();

			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "REFERENCE");
			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsigneeAddress)));

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("W00000002", whsOrderBO.WD_DocketID);
			var consigneeAddress = whsOrderBO.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeAddress);
			AssertNotNull(consigneeAddress);
			AssertEquals(org.PK, consigneeAddress.OrganisationPK);
			AssertEquals(org.OH_FullName, consigneeAddress.CompanyName);
			AssertEquals(rateTransportZonePerth.PK, whsOrderBO.WD_TZ_TransportZone);
		}

		public void TestPopulateConsigneeAddress_NoChanges_DoesNotSetRateTransportZone()
		{
			var org = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();
			var part = Data.Product;

			var whsOrderBOToUpdate = Helper.CreateWhsOrderWithOrderLine(org, whs, part, 10m);
			whsOrderBOToUpdate.WD_DocketID = "W00000002";
			whsOrderBOToUpdate.WD_ExternalReference = "ORDERME";
			Factory.SaveForTesting();

			whsOrderBOToUpdate.TransportCoPK = org.PK;
			whsOrderBOToUpdate.ConsigneeAddressPK = org.Addresses.MainAddress.PK;
			AssertEquals(ZGuid.Empty, whsOrderBOToUpdate.WD_TZ_TransportZone);
			Factory.SaveForTesting();

			SetupMainOrgAddress(org, "20 Maxwell Street", "", "PERTH", "6162", "WA", "AUBYW");
			var rateTransportProvider = Helper.SetUpRateTransportProvider(true, "OPS", "AU", "ALL", org);
			var rateTransportZonePerth = Helper.SetUpRateTransportZone(rateTransportProvider, true, "Perth");
			Helper.SetUpRateTransportZoneItem(rateTransportZonePerth, "AU", "6162");
			AssertEquals(ZGuid.Empty, whsOrderBOToUpdate.WD_TZ_TransportZone);
			Factory.SaveForTesting();

			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "REFERENCE");
			ShipmentDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.ConsigneeAddress),
				OrganizationCode = org.OH_Code,
			});

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("W00000002", whsOrderBO.WD_DocketID);
			AssertEquals(ZGuid.Empty, whsOrderBO.WD_TZ_TransportZone);
		}

		public void TestPopulateConsigneeDocumentaryAddress_SetsRateTransportZone()
		{
			var org = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();
			var part = Data.Product;

			var whsOrderBOToUpdate = Helper.CreateWhsOrderWithOrderLine(org, whs, part, 10m);
			whsOrderBOToUpdate.WD_DocketID = "W00000002";
			whsOrderBOToUpdate.WD_ExternalReference = "ORDERME";
			Factory.SaveForTesting();

			whsOrderBOToUpdate.TransportCoPK = org.PK;
			AssertEquals(ZGuid.Empty, whsOrderBOToUpdate.WD_TZ_TransportZone);
			Factory.SaveForTesting();

			SetupMainOrgAddress(org, "20 Maxwell Street", "", "PERTH", "6162", "WA", "AUBYW");
			var rateTransportProvider = Helper.SetUpRateTransportProvider(true, "OPS", "AU", "ALL", org);
			var rateTransportZonePerth = Helper.SetUpRateTransportZone(rateTransportProvider, true, "Perth");
			Helper.SetUpRateTransportZoneItem(rateTransportZonePerth, "AU", "6162");
			AssertEquals(ZGuid.Empty, whsOrderBOToUpdate.WD_TZ_TransportZone);
			Factory.SaveForTesting();

			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "REFERENCE");
			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress)));

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("W00000002", whsOrderBO.WD_DocketID);
			var consigneeAddress = whsOrderBO.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeAddress);
			AssertNotNull(consigneeAddress);
			AssertEquals(org.PK, consigneeAddress.OrganisationPK);
			AssertEquals(org.OH_FullName, consigneeAddress.CompanyName);
			AssertEquals(rateTransportZonePerth.PK, whsOrderBO.WD_TZ_TransportZone);
		}

		public void TestPopulateConsigneeDocumentaryAddress_NoChanges_DoesNotSetRateTransportZone()
		{
			var org = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();
			var part = Data.Product;

			var whsOrderBOToUpdate = Helper.CreateWhsOrderWithOrderLine(org, whs, part, 10m);
			whsOrderBOToUpdate.WD_DocketID = "W00000002";
			whsOrderBOToUpdate.WD_ExternalReference = "ORDERME";
			Factory.SaveForTesting();

			whsOrderBOToUpdate.TransportCoPK = org.PK;
			whsOrderBOToUpdate.ConsigneeAddressPK = org.Addresses.MainAddress.PK;
			AssertEquals(ZGuid.Empty, whsOrderBOToUpdate.WD_TZ_TransportZone);
			Factory.SaveForTesting();

			SetupMainOrgAddress(org, "20 Maxwell Street", "", "PERTH", "6162", "WA", "AUBYW");
			var rateTransportProvider = Helper.SetUpRateTransportProvider(true, "OPS", "AU", "ALL", org);
			var rateTransportZonePerth = Helper.SetUpRateTransportZone(rateTransportProvider, true, "Perth");
			Helper.SetUpRateTransportZoneItem(rateTransportZonePerth, "AU", "6162");
			AssertEquals(ZGuid.Empty, whsOrderBOToUpdate.WD_TZ_TransportZone);
			Factory.SaveForTesting();

			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "REFERENCE");
			ShipmentDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress),
				OrganizationCode = org.OH_Code,
			});

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("W00000002", whsOrderBO.WD_DocketID);
			AssertEquals(ZGuid.Empty, whsOrderBO.WD_TZ_TransportZone);
		}

		public void TestPopulateCarrierAddress_SetsRateTransportZone()
		{
			var org = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();
			var part = Data.Product;

			var whsOrderBOToUpdate = Helper.CreateWhsOrderWithOrderLine(org, whs, part, 10m);
			whsOrderBOToUpdate.WD_DocketID = "W00000002";
			whsOrderBOToUpdate.WD_ExternalReference = "ORDERME";
			Factory.SaveForTesting();

			SetupMainOrgAddress(org, "20 Maxwell Street", "", "PERTH", "6162", "WA", "AUBYW");
			var rateTransportProvider = Helper.SetUpRateTransportProvider(true, "OPS", "AU", "ALL", org);
			var rateTransportZonePerth = Helper.SetUpRateTransportZone(rateTransportProvider, true, "Perth");
			Helper.SetUpRateTransportZoneItem(rateTransportZonePerth, "AU", "6162");
			Factory.SaveForTesting();

			whsOrderBOToUpdate.ConsigneeAddressPK = org.Addresses.MainAddress.PK;
			AssertEquals(ZGuid.Empty, whsOrderBOToUpdate.WD_TZ_TransportZone);
			Factory.SaveForTesting();

			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "REFERENCE");
			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_CRAHOLSYD(AddressTypes.PickupLocalCartage));

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("W00000002", whsOrderBO.WD_DocketID);
			var carrierAddress = whsOrderBO.DocAddresses.FindByDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress);
			AssertNotNull(carrierAddress);
			AssertEquals(org.PK, carrierAddress.OrganisationPK);
			AssertEquals(org.OH_FullName, carrierAddress.CompanyName);
			AssertEquals(rateTransportZonePerth.PK, whsOrderBO.WD_TZ_TransportZone);
		}

		public void TestPopulateCarrierAddress_NoChanges_DoesNotSetRateTransportZone()
		{
			var org = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();
			var part = Data.Product;

			var whsOrderBOToUpdate = Helper.CreateWhsOrderWithOrderLine(org, whs, part, 10m);
			whsOrderBOToUpdate.WD_DocketID = "W00000002";
			whsOrderBOToUpdate.WD_ExternalReference = "ORDERME";
			Factory.SaveForTesting();

			whsOrderBOToUpdate.TransportCoPK = org.PK;
			whsOrderBOToUpdate.ConsigneeAddressPK = org.Addresses.MainAddress.PK;
			AssertEquals(ZGuid.Empty, whsOrderBOToUpdate.WD_TZ_TransportZone);
			Factory.SaveForTesting();

			SetupMainOrgAddress(org, "20 Maxwell Street", "", "PERTH", "6162", "WA", "AUBYW");
			var rateTransportProvider = Helper.SetUpRateTransportProvider(true, "OPS", "AU", "ALL", org);
			var rateTransportZonePerth = Helper.SetUpRateTransportZone(rateTransportProvider, true, "Perth");
			Helper.SetUpRateTransportZoneItem(rateTransportZonePerth, "AU", "6162");
			AssertEquals(ZGuid.Empty, whsOrderBOToUpdate.WD_TZ_TransportZone);
			Factory.SaveForTesting();

			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			ShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "REFERENCE");
			ShipmentDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = AddressTypes.PickupLocalCartage,
				OrganizationCode = org.OH_Code,
			});

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("W00000002", whsOrderBO.WD_DocketID);
			AssertEquals(ZGuid.Empty, whsOrderBO.WD_TZ_TransportZone);
		}

		#endregion

		#region TestBasicOrderLevelFieldMappings

		public void TestBasicOrderLevelFieldMappings()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			Helper.CreateWhsSalesChannel("TST", "Fishing");
			Factory.SaveForTesting();

			ShipmentDataObject.ContainerMode = new ContainerMode { Code = "LCL", Description = "Less Container Load" };
			ShipmentDataObject.GoodsDescription = "Special Sauce";
			ShipmentDataObject.CarrierServiceLevel = new ServiceLevel { Code = "SET", Description = "SITTLE" };
			ShipmentDataObject.ServiceLevel = new ServiceLevel { Code = "TSL", Description = "Test Service Level" };
			ShipmentDataObject.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm { Code = "INC", Description = "IKL" };
			ShipmentDataObject.ShipperCODAmount = 43.2m;
			ShipmentDataObject.ShipperCODPayMethod = new CodeDescriptionPair { Code = "CCC", Description = "CICOCUC" };
			ShipmentDataObject.TotalNoOfPacks = 3;
			ShipmentDataObject.TotalNoOfPacksPackageType = new PackageType { Code = "CTN", Description = "CTN" };
			ShipmentDataObject.TotalVolume = 32.1m;
			ShipmentDataObject.TotalVolumeUnit = new UnitOfVolume { Code = "CY", Description = "Cubic Yards" };
			ShipmentDataObject.TotalWeight = 41.2m;
			ShipmentDataObject.TotalWeightUnit = new UnitOfWeight { Code = "LB", Description = "Pounds" };
			ShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "SEA", Description = "Sea Freight" };
			ShipmentDataObject.WayBillNumber = "BILL";
			ShipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };
			ShipmentDataObject.DataContext.CodesMappedToTarget = true;
			ShipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>() { new AdditionalReference { Type = new EntryType { Code = "HSB" }, ReferenceNumber = "BILL" } });
			ShipmentDataObject.LocalProcessing = new LocalProcessing { DeliveryRequiredBy = new ZDateTime(2011, 1, 1) };
			ShipmentDataObject.IsAuthorizedToLeave = true;

			var order = ShipmentDataObject.Order;
			order.AddPalletWeightToOrder = true;
			order.RequiresQualityAudit = true;
			order.RequiresPacking = true;
			order.ExcludeFromTotePicking = true;
			order.ClientReference = "CUSTOMER";
			order.TotalLineVolume = 12.5m;
			order.DropMode = new DropMode { Code = "DRO", Description = "Drop" };
			order.FulfillmentRule = new CodeDescriptionPair { Code = "NON", Description = "None" };
			order.LocalCartageInsuranceValue = 22.2m;
			order.TotalNetWeightSent = 20.1m;
			order.OrderNumber = "ORDERME";
			order.OrderNumberSplit = new ZByte(1);
			order.PalletsSent = new ZShort(4);
			order.PickOption = new CodeDescriptionPair { Code = "AUT", Description = "Auto" };
			order.StagingArea = "DockA";
			order.Status = new CodeDescriptionPair { Code = "HEL", Description = "Held" };
			order.TotalUnits = 12.3m;
			order.TotalLineWeight = 32.6m;
			order.TransportReference = "TRANS";
			order.Type = new CodeDescriptionPair { Code = "CUS", Description = "CUSTOMS RELEASE" };
			order.SalesChannel = new CodeDescriptionPair { Code = "TST", Description = "Fishing" };

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertContents(whsOrderBO);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsOrder found, creating new WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsDocketReference found, creating new WhsDocketReference.
Information - Populating WhsDocketReference...
Information - Added Warehouse Order from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestUseDirectedPackingConsolidation

		public void TestUseDirectedPackingConsolidation_ThenShouldBeImported() => TestUseDirectedPackingConsolidationCore(isUseDirectedPackingConsolidation: true);

		public void TestNotUseDirectedPackingConsolidation_ThenShouldNotBeImported() => TestUseDirectedPackingConsolidationCore(isUseDirectedPackingConsolidation: false);

		void TestUseDirectedPackingConsolidationCore(bool isUseDirectedPackingConsolidation)
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			Helper.CreateWhsSalesChannel("TST", "Fishing");
			Factory.SaveForTesting();
			var order = ShipmentDataObject.Order;
			order.UseDirectedPackingConsolidation = isUseDirectedPackingConsolidation;
			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			var expectedResult = isUseDirectedPackingConsolidation;
			AssertEquals("whsOrderBO.WD_UseDirectedPackingConsolidation should be set based on registry", expectedResult, whsOrderBO.WD_UseDirectedPackingConsolidation);
		}

		public void TestUseDirectedPackingConsolidation_WhenFieldIsNotSpecified_ThenShouldFallbackToDefaultValueTrue() => TestUseDirectedPackingConsolidation_WhenFieldIsNotSpecified_ThenShouldFallbackToDefaultValueCore(true);

		public void TestUseDirectedPackingConsolidation_WhenFieldIsNotSpecified_ThenShouldFallbackToDefaultValueFalse() => TestUseDirectedPackingConsolidation_WhenFieldIsNotSpecified_ThenShouldFallbackToDefaultValueCore(false);

		void TestUseDirectedPackingConsolidation_WhenFieldIsNotSpecified_ThenShouldFallbackToDefaultValueCore(bool defaultValue)
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();
			Helper.CreateWhsSalesChannel("TST", "Fishing");
			var pickParams = WhsClientPickingParams.GetClientPickingParams(client).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = whs.PK;
			pickParams.WPP_UseDirectedPackingConsolidation = defaultValue;
			Factory.SaveForTesting();

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.WD_UseDirectedPackingConsolidation should fall back to default value", defaultValue, whsOrderBO.WD_UseDirectedPackingConsolidation);
		}

		public void TestUseDirectedPackingConsolidation_WhenFieldIsFalse_ThenShouldBeSpecifiedValue()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();
			Helper.CreateWhsSalesChannel("TST", "Fishing");
			var pickParams = WhsClientPickingParams.GetClientPickingParams(client).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = whs.PK;
			pickParams.WPP_UseDirectedPackingConsolidation = true;
			Factory.SaveForTesting();

			ShipmentDataObject.Order.UseDirectedPackingConsolidation = false;
			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.WD_UseDirectedPackingConsolidation should be specified value", false, whsOrderBO.WD_UseDirectedPackingConsolidation);
		}

		public void TestUseDirectedPackingConsolidation_WhenFieldIsTrue_ThenShouldBeSpecifiedValue()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();
			Helper.CreateWhsSalesChannel("TST", "Fishing");
			var pickParams = WhsClientPickingParams.GetClientPickingParams(client).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = whs.PK;
			pickParams.WPP_UseDirectedPackingConsolidation = false;
			Factory.SaveForTesting();

			ShipmentDataObject.Order.UseDirectedPackingConsolidation = true;
			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.WD_UseDirectedPackingConsolidation should be specified value", true, whsOrderBO.WD_UseDirectedPackingConsolidation);
		}

		#endregion

		#region TestUnitsSent

		public void TestUnitsSent_IsIgnoredOnCreatingOrder_CreatingPickShouldAutoCalculateUnitsSent()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var product = Data.ProductCRAHOLSYD;
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 8m);

			Factory.SaveForTesting();

			ShipmentDataObject.LocalProcessing = new LocalProcessing { DeliveryRequiredBy = new ZDateTime(2011, 1, 1) };
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ConsigneeAddressDataObject_CRAHOLSYD);
			var order = ShipmentDataObject.Order;
			order.ClientReference = "CUSTOMER";
			order.OrderNumber = "ORDERME";
			order.UnitsSent = 11m;

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.OrderedQty = 10m;
			orderLineDataObject.Product = new Product { Code = product.OP_PartNum };
			order.SetOrderLineCollection(() => new DataObjectList<OrderLine> { orderLineDataObject });

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();
			AssertEquals("Should have warning that UnitsSent was ignored.", "Element 'UnitsSent' with value '11' was ignored as the Order is not allocated.", Logger.GetWarnings());
			PickOrder(whsOrderBO);
			AssertEquals("Units Sent should not have come from the XML when order was created.", 8m, whsOrderBO.WD_UnitsSent);
			AssertEquals(PickType.Codes.Order, whsOrderBO.Pick.WP_PickType);
		}

		public void TestUnitsSent_NoWarningsIfValueIsNotProvided()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var product = Data.ProductCRAHOLSYD;
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 8m);

			Factory.SaveForTesting();

			ShipmentDataObject.LocalProcessing = new LocalProcessing { DeliveryRequiredBy = new ZDateTime(2011, 1, 1) };
			ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ConsigneeAddressDataObject_CRAHOLSYD);
			var order = ShipmentDataObject.Order;
			order.ClientReference = "CUSTOMER";
			order.OrderNumber = "ORDERME";
			order.UnitsSent = null;

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.OrderedQty = 10m;
			orderLineDataObject.Product = new Product { Code = product.OP_PartNum };
			order.SetOrderLineCollection(() => new DataObjectList<OrderLine> { orderLineDataObject });

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();
			AssertEquals("Should have no warnings.", false, Logger.HasWarnings);
		}

		public void TestUnitsSent_IsSetIfOrderIsPicked()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var product = Data.ProductCRAHOLSYD;
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 10m);

			Factory.SaveForTesting();

			var orderBO = Helper.CreateWhsOrderWithOrderLine(client, warehouse, product, 10m);
			PickOrder(orderBO);
			AssertEquals("Precondition: Units Sent should be set.", 10m, orderBO.WD_UnitsSent);

			var order = ShipmentDataObject.Order;
			order.OrderNumber = orderBO.WD_ExternalReference;
			order.UnitsSent = 11m;

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var updatedOrder = reader.ReadIntoBusinessObject();
			AssertEquals("Order should be updated.", orderBO, updatedOrder);
			AssertEquals("Units Sent should be overriden.", 11m, updatedOrder.WD_UnitsSent);
			AssertEquals(PickType.Codes.Order, orderBO.Pick.WP_PickType);
		}

		#endregion

		#region TestPickPriority

		public void TestPickPriority()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.Order.PickPriority = 3;

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals((byte)3, whsOrderBO.WD_PickPriority);
		}

		public void TestPickPriority_InvalidValue()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.Order.PickPriority = 0;
			AssertNoExceptionThrown(() => new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());

			ShipmentDataObject.Order.PickPriority = 21;
			AssertExceptionThrown<DataObjectReadFailureException>("Failed to import",
				@"Cannot Import Order, Pick Priority should be in range 0 to 20.",
				() => new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());

			ShipmentDataObject.Order.PickPriority = 20;
			AssertNoExceptionThrown(() => new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestPickPriority_NoWarningsIfValueIsNotProvided()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals((byte)0, whsOrderBO.WD_PickPriority);
			AssertEquals("Should have no warnings.", false, Logger.HasWarnings);
		}

		#endregion

		#region TestWhenTotalUnitsAreNotAvailable

		public void TestWhenTotalUnitsAreNotAvailable()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.DataContext.CodesMappedToTarget = true;
			ShipmentDataObject.LocalProcessing = new LocalProcessing { DeliveryRequiredBy = new ZDateTime(2011, 1, 1) };

			var order = ShipmentDataObject.Order;
			order.AddPalletWeightToOrder = true;
			order.ClientReference = "CUSTOMER";

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals(0m, whsOrderBO.WD_UnitsSent);
		}

		#endregion

		#region TestCustomerReferenceFromEntryNumberCollection

		public void TestCustomerReferenceFromEntryNumberCollection()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Data.CreateClientOrgCRAHOLSYDInDB();
				Data.GetOrCreateWarehouseInDB();
				Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
				ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo() { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWR } } });
				ShipmentDataObject.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "DB3234");
				ShipmentDataObject.SetEntryNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.EntryNumber>());
				ShipmentDataObject.EntryNumberCollection.Add(new UniversalDataBuss.DataObjects.Universal.EntryNumber { Number = "TestOutwardsEntry#", Type = new EntryType() { Code = "ENS" } });
				ShipmentDataObject.SetAddInfoCollection(() => new List<AddInfo>());
				ShipmentDataObject.AddInfoCollection.Add(new AddInfo { Key = "EntryFilerCode", Value = "TestFiler" });
				ShipmentDataObject.Order = new Order();
				Assert("Precondition - client reference not set.", string.IsNullOrEmpty(ShipmentDataObject.Order.ClientReference));

				var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
				var whsOrderBO = GetDocketWithAllocateMock(reader);

				AssertNotNull(whsOrderBO);
				AssertEquals("TestFiler-TestOutwardsEntry#", whsOrderBO.WD_CustomerReference);
			}
		}

		#endregion

		#region TestImportOrder_WithInactiveWarehouse_UseOnlyActive

		public void TestImportOrder_WithInactiveWarehouse_UseOnlyActive()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs1 = Data.GetOrCreateWarehouseInDB();
			var whsAddress = whs1.WarehouseAddress;

			// Create 2 inactive warehouses at beginning and end of table since they are ordered by name.
			var inactiveWhs001 = Helper.CreateWarehouse("001");
			inactiveWhs001.WW_IsActive = false;

			var inactiveWhsZZZ = Helper.CreateWarehouse("ZZZ");
			inactiveWhsZZZ.WW_IsActive = false;

			Factory.SaveForTesting();
			AssertEquals("Warehouse 001 must be inactive.", false, inactiveWhs001.WW_IsActive);
			AssertEquals("Warehouse ZZZ must be inactive.", false, inactiveWhsZZZ.WW_IsActive);

			var order = ShipmentDataObject.Order;
			order.Warehouse = null;
			order.OrderNumber = "ORDERME";
			order.TotalUnits = 10m;
			order.UnitsSent = 10m;

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertNotNull("Order could not be read into business object", whsOrderBO);
				AssertEquals("Code: Expected to use active Warehouse but used the in-active Whs.", whs1.WW_WarehouseCode, whsOrderBO.Warehouse.WW_WarehouseCode);
				AssertEquals("PK: Expected to use active Warehouse but used the in-active Whs.", whs1.PK, whsOrderBO.WD_WW_Whs);
			});
		}

		#endregion

		#region Forwarding Shipment Related

		#region TestShipmentFieldsMapCorrectlyToOrder

		public void TestShipmentFieldsMapCorrectlyToOrder()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.WayBillNumber = "HOUSEBILL";
			ShipmentDataObject.BookingConfirmationReference = "BOOK ME";

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();
			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsOrderBO.WD_BOLNo", "HOUSEBILL", whsOrderBO.WD_BOLNo);
				AssertEquals("whsOrderBO.WD_CustomerReference", "BOOK ME", whsOrderBO.WD_CustomerReference);
			});

			ShipmentDataObject.Order.ClientReference = "CUSTOMER IS RIGHT";

			var newReader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var newWhsOrderBO = newReader.ReadIntoBusinessObject();
			AssertNotNull(newWhsOrderBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsOrderBO.WD_BOLNo", "HOUSEBILL", newWhsOrderBO.WD_BOLNo);
				AssertEquals("whsOrderBO.WD_CustomerReference", "CUSTOMER IS RIGHT", newWhsOrderBO.WD_CustomerReference);
			});
		}

		#endregion

		#region TestGetRelatedWarehouseFromClientWarehouseParamsOrLoadTopOne

		public void TestGetRelatedWarehouseFromClientWarehouseParamsOrLoadTopOne()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var newWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			newWarehouse.WW_WarehouseCode = "NEW";
			Helper.CreateArea(newWarehouse, "AREA");

			ShipmentDataObject.Order.Warehouse = null;
			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			WhsOrder whsOrderBO = null;

			AssertNoExceptionThrown("Should grab first warehouse", () => whsOrderBO = reader.ReadIntoBusinessObject());
			AssertNotNull(whsOrderBO);
			AssertEquals("WHS", whsOrderBO.Warehouse.WW_WarehouseCode);

			var clientParams = WhsClientParams.GetClientParams(whsOrderBO.Client);
			var warehouseParams = clientParams.ClientParametersByWarehouse.AddNew();
			warehouseParams.WY_WW_Whs = newWarehouse.PK;
			Factory.SaveForTesting();

			var newReader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			WhsOrder whsOrderBOWithClientWarehouse = null;
			AssertNoExceptionThrown("Should grab client warehouse", () => whsOrderBOWithClientWarehouse = newReader.ReadIntoBusinessObject());
			AssertNotNull(whsOrderBOWithClientWarehouse);
			AssertEquals("NEW", whsOrderBOWithClientWarehouse.Warehouse.WW_WarehouseCode);
		}

		#endregion

		#region TestGetRelatedWarehouseLoadTopOne_NotCYDWarehouse

		public void TestGetRelatedWarehouseLoadTopOne_NotCYDWarehouse()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();

			// CYD warehouse
			var whs1 = Data.GetOrCreateWarehouseInDB();
			whs1.WW_WarehouseType = "CYD";

			// Create PRW warehouse
			var productWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			productWarehouse.WW_WarehouseCode = "AAA";

			ShipmentDataObject.Order.Warehouse = null;
			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			WhsOrder whsOrderBO = null;

			AssertNoExceptionThrown("Should grab first warehouse", () => whsOrderBO = reader.ReadIntoBusinessObject());
			AssertNotNull(whsOrderBO);
			AssertEquals("AAA", whsOrderBO.Warehouse.WW_WarehouseCode);
		}

		#endregion

		#region TestTransportCompanyDocumentaryAddressTakesPrecedenceOverPickupLocalCartageAddress

		public void TestTransportCompanyDocumentaryAddressTakesPrecedenceOverPickupLocalCartageAddress()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			new OrganisationDataObjectReader(GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.TransportCompanyDocumentaryAddress)), Logger, Factory).GetMatchedOrNewForTesting();
			new OrganisationDataObjectReader(GetNewAddressData_INTHEMSYD(AddressTypes.PickupLocalCartage), Logger, Factory).GetMatchedOrNewForTesting();
			Logger.ClearLogs();

			Factory.SaveForTesting();

			ShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "");
			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_INTHEMSYD(AddressTypes.PickupLocalCartage));
			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.TransportCompanyDocumentaryAddress)));

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertJobDocAddressContentMatches_CRAHOLSYD(whsOrderBO.TransportCoDocAddress);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsOrder found, creating new WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'PickupLocalCartage':- Matched to 'INTHEMSYD' by code, address '' (only address).
Warning - Unknown Address Type [PickupLocalCartage] found. Job Document Address not imported.
Information - Matching 'TransportCompanyDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Warehouse Order from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestPickupLocalCartageAddress

		public void TestPickupLocalCartageAddress()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var address = new OrganisationDataObjectReader(GetNewAddressData_INTHEMSYD(AddressTypes.PickupLocalCartage), Logger, Factory).GetMatchedOrNewForTesting();
			Logger.ClearLogs();

			Factory.SaveForTesting();

			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_INTHEMSYD(AddressTypes.PickupLocalCartage));

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertEquals(ZGuid.Empty, whsOrderBO.TransportCoDocAddress.E2_OA_Address);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsOrder found, creating new WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'PickupLocalCartage':- Matched to 'INTHEMSYD' by code, address '' (only address).
Warning - Unknown Address Type [PickupLocalCartage] found. Job Document Address not imported.
Information - Added Warehouse Order from UniversalShipment.
".Trim(), Logger.Logs);
			});

			Logger.ClearLogs();
			ShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "");

			var newReader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var newWhsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertJobDocAddressContentMatches_INTHEMSYD(newWhsOrderBO.TransportCoDocAddress);
				AssertEquals(address.PK, newWhsOrderBO.TransportCoDocAddress.E2_OA_Address);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching WhsOrder found, creating new WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'PickupLocalCartage':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Warehouse Order from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestDropOffAddressTakesPrecedenceOverDepartureCFSAddress

		public void TestDropOffAddressTakesPrecedenceOverDepartureCFSAddress()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			new OrganisationDataObjectReader(GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.DropOffAddress)), Logger, Factory).GetMatchedOrNewForTesting();
			new OrganisationDataObjectReader(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.DepartureCFSAddress)), Logger, Factory).GetMatchedOrNewForTesting();
			Logger.ClearLogs();

			Factory.SaveForTesting();

			ShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "");
			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.DepartureCFSAddress)));
			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.DropOffAddress)));

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertJobDocAddressContentMatches_CRAHOLSYD(whsOrderBO.DropOffDocAddress);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsOrder found, creating new WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'DepartureCFSAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Warning - Unknown Address Type [DepartureCFSAddress] found. Job Document Address not imported.
Information - Matching 'DropOffAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Warehouse Order from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestDepartureCFSAddress

		public void TestDepartureCFSAddress()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var address = new OrganisationDataObjectReader(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.DepartureCFSAddress)), Logger, Factory).GetMatchedOrNewForTesting();
			Logger.ClearLogs();

			Factory.SaveForTesting();

			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.DepartureCFSAddress)));

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertEquals(ZGuid.Empty, whsOrderBO.DropOffAddressPK);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsOrder found, creating new WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'DepartureCFSAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Warning - Unknown Address Type [DepartureCFSAddress] found. Job Document Address not imported.
Information - Added Warehouse Order from UniversalShipment.
".Trim(), Logger.Logs);
			});

			Logger.ClearLogs();
			ShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "");

			var newReader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var newWhsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertJobDocAddressContentMatches_INTHEMSYD(newWhsOrderBO.DropOffDocAddress);
				AssertEquals(address.PK, newWhsOrderBO.DropOffAddressPK);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching WhsOrder found, creating new WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'DepartureCFSAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Warehouse Order from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestConsigneeAddressTakesPrecedenceOverConsigneeDocumentaryAddress

		public void TestConsigneeAddressTakesPrecedenceOverConsigneeDocumentaryAddress()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			new OrganisationDataObjectReader(GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsigneeAddress)), Logger, Factory).GetMatchedOrNewForTesting();
			new OrganisationDataObjectReader(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress)), Logger, Factory).GetMatchedOrNewForTesting();
			Logger.ClearLogs();

			Factory.SaveForTesting();

			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress)));
			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsigneeAddress)));

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertJobDocAddressContentMatches_CRAHOLSYD(whsOrderBO.ConsigneeDocAddress);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsOrder found, creating new WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Warning - Unknown Address Type [ConsigneeDocumentaryAddress] found. Job Document Address not imported.
Information - Matching 'ConsigneeAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Warehouse Order from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestConsigneeAddress

		public void TestConsigneeAddress()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			new OrganisationDataObjectReader(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeAddress)), Logger, Factory).GetMatchedOrNewForTesting();
			Logger.ClearLogs();

			Factory.SaveForTesting();

			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeAddress)));

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertJobDocAddressContentMatches_INTHEMSYD(whsOrderBO.ConsigneeDocAddress);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsOrder found, creating new WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'ConsigneeAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Warehouse Order from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestConsigneeDocumentaryAddress

		public void TestConsigneeDocumentaryAddress()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			new OrganisationDataObjectReader(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress)), Logger, Factory).GetMatchedOrNewForTesting();
			Logger.ClearLogs();

			Factory.SaveForTesting();

			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress)));

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertJobDocAddressContentMatches_INTHEMSYD(whsOrderBO.ConsigneeDocAddress);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsOrder found, creating new WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Warehouse Order from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestReturnAddress

		public void TestReturnAddress()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			new OrganisationDataObjectReader(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ReturnAddress)), Logger, Factory).GetMatchedOrNewForTesting();
			Logger.ClearLogs();

			Factory.SaveForTesting();

			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ReturnAddress)));

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertJobDocAddressContentMatches_INTHEMSYD(whsOrderBO.ReturnDocAddress);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsOrder found, creating new WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'ReturnAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Warehouse Order from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#endregion

		#region TestForwarder

		public void TestForwarder()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			new OrganisationDataObjectReader(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.SendingForwarderAddress)), Logger, Factory).GetMatchedOrNewForTesting();
			Logger.ClearLogs();

			Factory.SaveForTesting();

			ShipmentDataObject.OrganizationAddressCollection.Add(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.SendingForwarderAddress)));

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertAddressContentMatches_INTHEMSYD(whsOrderBO.Forwarder.MainAddress);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsOrder found, creating new WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'SendingForwarderAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Warehouse Order from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestLinkDocketWithParent

		public void TestForwardingShipmentForLinking()
		{
			var shipment = (BusinessObject)Factory.BOFactory.New<Forwarding.IForwardingShipment>();
			shipment.FillWithValidTestData();
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = DataContextKeyForLinking;

			AssertLinkDocketWithParent(shipment, DataContextType.ForwardingShipment);
		}

		public void TestCustomsDeclarationForLinking()
		{
			var declaration = (BusinessObject)Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.FillWithValidTestData();
			declaration[JobDeclarationSchema.JE_DeclarationReference] = DataContextKeyForLinking;
			AssertLinkDocketWithParent(declaration, DataContextType.CustomsDeclaration);
		}

		public void TestEntryHeaderForLinking()
		{
			var declaration = Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			((BusinessObject)declaration).FillWithValidTestData();
			var entryHeader = (BusinessObject)Factory.BOFactory.New<Enterprise.Integration.Customs.ICusEntryHeader>();
			entryHeader[CusEntryHeaderSchema.CH_JE] = declaration.PK;
			entryHeader.FillWithValidTestData();
			entryHeader[CusEntryHeaderSchema.CH_BGMReference] = DataContextKeyForLinking;
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			AssertLinkDocketWithParent(entryHeader, DataContextType.WarehouseCustomsEntry);
		}

		public void TestNctsHeaderForLinking()
		{
			using var countryDisposable = GlbCompany.CurrentCompany.TemporarilySetCountry("LV"); // EU country
			var nctsHeader = Factory.BOFactory.New<Enterprise.Integration.Customs.EU.NCTS.ICusInBondHeader>();
			((BusinessObject)nctsHeader).FillWithValidTestData();
			nctsHeader.BH_JobReference = DataContextKeyForLinking;
			nctsHeader.BH_ApplicationCode = "NCT";
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			AssertLinkDocketWithParent((BusinessObject)nctsHeader, DataContextType.NctsHeader);
		}

		#endregion

		#region TestOrderStatus

		#region TestOrderStatusIsNotChangedIfUpdatingAFinalizedOrPickedOrderIfXMLContainedAHeldStatus

		public void TestOrderStatusIsNotChangedIfUpdatingAFinalizedOrPickedOrderIfXMLContainedAHeldStatus_Finalized()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var whsOrderBOToLoad = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);

			var pick = Helper.CreatePickNew();
			pick.PickOrders(whsOrderBOToLoad);
			whsOrderBOToLoad.WD_DocketStatus = "PIC";
			whsOrderBOToLoad.WD_DocketID = "W00000002";
			whsOrderBOToLoad.WD_ExternalReference = "ORDERME";
			whsOrderBOToLoad.WD_OH_Client = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "CRAHOLSYD")).PK;
			whsOrderBOToLoad.Warehouse.WW_WarehouseName = "CoolShack";
			whsOrderBOToLoad.Warehouse.WW_WarehouseCode = "WSS";

			Factory.SaveForTesting();

			ShipmentDataObject.Order.Status = new CodeDescriptionPair { Code = "HEL", Description = "Held" };
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertEquals("Loads same order.", whsOrderBO.PK, whsOrderBOToLoad.PK);
				AssertEquals("whsOrderBO.WD_DocketStatus", "PIC", whsOrderBO.WD_DocketStatus);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Warning - Cannot update read-only Field 'Warehouse' [WD_WW_Whs]. Cannot change 'WSS' (CoolShack) to 'WHS' (CoolHouse).
Information - Updated Warehouse Order W00000002 from UniversalShipment.
".Trim(), Logger.Logs);
			});

			var part = Helper.CreateProduct(whsOrderBO.Client, "P1");
			Helper.CreateWhsReceiveWithInventory(whsOrderBO.Client, whsOrderBO.Warehouse, "R1", part, 100m);
			Factory.SaveForTesting();

			Helper.CreateWhsOrderLine(whsOrderBO, part, 10m);

			pick.AutoAllocateItemsWithMock();
			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(whsOrderBO);

			Factory.SaveForTesting();

			var newLogger = new TestErrorLogger();
			newLogger.TopLevelDataObject = ShipmentDataObject;
			reader = new WhsOrderDataObjectReader(ShipmentDataObject, newLogger, Factory);
			whsOrderBO = GetDocketWithAllocateMock(reader);

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertEquals("Loads same order.", whsOrderBO.PK, whsOrderBOToLoad.PK);
				AssertEquals("whsOrderBO.IsFinalised", true, whsOrderBO.IsFinalised);
				AssertMultilineASCIIEquals("newLogger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Warning - Cannot update read-only Field 'Warehouse' [WD_WW_Whs]. Cannot change 'WSS' (CoolShack) to 'WHS' (CoolHouse).
Information - Updated Warehouse Order W00000002 from UniversalShipment.
".Trim(), newLogger.Logs);
			});
		}

		public void TestOrderStatusIsNotChangedIfUpdatingAFinalizedOrPickedOrderIfXMLContainedAHeldStatus_Picked()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var whsOrderBOToLoad = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);

			var pick = Helper.CreatePickNew();
			pick.PickOrders(whsOrderBOToLoad);
			whsOrderBOToLoad.WD_DocketStatus = "PIC";
			whsOrderBOToLoad.WD_DocketID = "W00000002";
			whsOrderBOToLoad.WD_ExternalReference = "ORDERME";
			whsOrderBOToLoad.WD_OH_Client = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "CRAHOLSYD")).PK;
			whsOrderBOToLoad.Warehouse.WW_WarehouseName = "CoolShack";
			whsOrderBOToLoad.Warehouse.WW_WarehouseCode = "WSS";

			Factory.SaveForTesting();

			ShipmentDataObject.Order.Status = new CodeDescriptionPair { Code = "HEL", Description = "Held" };
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertEquals("Loads same order.", whsOrderBO.PK, whsOrderBOToLoad.PK);
				AssertEquals("whsOrderBO.WD_DocketStatus", "PIC", whsOrderBO.WD_DocketStatus);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Warning - Cannot update read-only Field 'Warehouse' [WD_WW_Whs]. Cannot change 'WSS' (CoolShack) to 'WHS' (CoolHouse).
Information - Updated Warehouse Order W00000002 from UniversalShipment.
".Trim(), Logger.Logs);
			});

			var part = Helper.CreateProduct(whsOrderBO.Client, "P1");
			Helper.CreateWhsReceiveWithInventory(whsOrderBO.Client, whsOrderBO.Warehouse, "R1", part, 100m);
			Factory.SaveForTesting();

			Helper.CreateWhsOrderLine(whsOrderBO, part, 10m);

			pick.AutoAllocateItemsWithMock();
			Factory.SaveForTesting();

			var newLogger = new TestErrorLogger();
			newLogger.TopLevelDataObject = ShipmentDataObject;
			reader = new WhsOrderDataObjectReader(ShipmentDataObject, newLogger, Factory);
			whsOrderBO = GetDocketWithAllocateMock(reader);

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertEquals("Loads same order.", whsOrderBO.PK, whsOrderBOToLoad.PK);
				AssertMultilineASCIIEquals("newLogger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Warning - Cannot update read-only Field 'Warehouse' [WD_WW_Whs]. Cannot change 'WSS' (CoolShack) to 'WHS' (CoolHouse).
Information - Updated Warehouse Order W00000002 from UniversalShipment.
".Trim(), newLogger.Logs);
			});

			Factory.SaveAtEndOfImport(newLogger);
		}

		#endregion

		#region TestOrderStatusIsSetToHeldIfUpdatingAnOrderThatIsEnteredOrCreatingAnOrderIfXMLContainedAHeldStatus

		public void TestOrderStatusIsSetToHeldIfUpdatingAnOrderThatIsEnteredOrCreatingAnOrderIfXMLContainedAHeldStatus()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.Order.Status = new CodeDescriptionPair { Code = "HEL", Description = "Held" };
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsOrderBO.WD_DocketStatus", "HEL", whsOrderBO.WD_DocketStatus);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsOrder found, creating new WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Warehouse Order from UniversalShipment.
".Trim(), Logger.Logs);
			});

			whsOrderBO.WD_DocketStatus = "ENT";

			Factory.SaveForTesting();

			var newLogger = new TestErrorLogger();
			newLogger.TopLevelDataObject = ShipmentDataObject;
			reader = new WhsOrderDataObjectReader(ShipmentDataObject, newLogger, Factory);
			var whsOrderBOLoaded = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBOLoaded);

			CombineAssertions(delegate
			{
				AssertEquals("Loads same order.", whsOrderBO.PK, whsOrderBOLoaded.PK);
				AssertEquals("whsOrderBO.WD_DocketStatus", "HEL", whsOrderBOLoaded.WD_DocketStatus);
				AssertMultilineASCIIEquals("newLogger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Updated Warehouse Order W00000001 from UniversalShipment.
".Trim(), newLogger.Logs);
			});
		}

		#endregion

		#region TestOrderStatusIsNotUpdatedIfUpdatingAnOrderAndStatusInXMLIsNotHeld

		public void TestOrderStatusIsNotUpdatedIfUpdatingAnOrderAndStatusInXMLIsNotHeld()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var whsOrderBOToLoad = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);

			Helper.CreatePickNew(whsOrderBOToLoad);
			whsOrderBOToLoad.WD_DocketStatus = "PIC";
			whsOrderBOToLoad.WD_DocketID = "W00000001";
			whsOrderBOToLoad.WD_ExternalReference = "ORDERME";
			whsOrderBOToLoad.WD_OH_Client = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "CRAHOLSYD")).PK;
			whsOrderBOToLoad.Warehouse.WW_WarehouseName = "CoolShack";
			whsOrderBOToLoad.Warehouse.WW_WarehouseCode = "WSS";

			Factory.SaveForTesting();

			ShipmentDataObject.Order.Status = new CodeDescriptionPair { Code = "FIN", Description = "Finalized" };
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertEquals("Loads same order.", whsOrderBO.PK, whsOrderBOToLoad.PK);
				AssertEquals("whsOrderBO.WD_DocketStatus", "PIC", whsOrderBO.WD_DocketStatus);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Warning - Cannot update read-only Field 'Warehouse' [WD_WW_Whs]. Cannot change 'WSS' (CoolShack) to 'WHS' (CoolHouse).
Information - Updated Warehouse Order W00000001 from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestOrderStatusIsSetToEnteredWhenCreatingANewOrder

		public void TestOrderStatusIsSetToEnteredWhenCreatingANewOrder()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.Order.Status = new CodeDescriptionPair { Code = "PIC", Description = "Picking" };
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsOrderBO.WD_DocketStatus", "ENT", whsOrderBO.WD_DocketStatus);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsOrder found, creating new WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Warehouse Order from UniversalShipment.
".Trim(), Logger.Logs);
			});

			ShipmentDataObject.Order.OrderNumber = "DONTORDERME";
			ShipmentDataObject.Order.Status = new CodeDescriptionPair { Code = "FIN", Description = "Finalized" };

			var newLogger = new TestErrorLogger();
			newLogger.TopLevelDataObject = ShipmentDataObject;
			reader = new WhsOrderDataObjectReader(ShipmentDataObject, newLogger, Factory);
			whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsOrderBO.WD_DocketStatus", "ENT", whsOrderBO.WD_DocketStatus);
				AssertMultilineASCIIEquals("newLogger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsOrder found, creating new WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Warehouse Order from UniversalShipment.
".Trim(), newLogger.Logs);
			});
		}

		#endregion

		#endregion

		#region TestGetReasonForNotAbleToUpdateMatchedDocket

		public void TestGetReasonForNotAbleToUpdateMatchedDocket_PickIsReadyForPlanning()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var whsOrder = WhsOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var line = whsOrder.Lines.AddNew();
			line.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			line.WE_TransactionQuantity = 10m;
			whsOrder.WD_ExternalReference = "ORDERME";
			whsOrder.WD_DocketID = "W00000002";
			whsOrder.Warehouse.WW_WarehouseName = "CoolShack";
			whsOrder.Warehouse.WW_WarehouseCode = "WSS";

			Helper.CreateWhsReceiveWithInventory(whsOrder.Client, whsOrder.Warehouse, "R1", line.SupplierPart, 100m, whsOrder.Warehouse.DefaultOutboundDockDoorLocation, "");
			Factory.SaveForTesting();

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(whsOrder);
			pick.AutoAllocateItemsWithMock();
			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(whsOrder);
			Factory.SaveForTesting();

			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			var orderLineDataObject = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject, orderLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.Lines.Count", 1, whsOrderBO.Lines.Count);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsOrder.
Error - Cannot populate WhsOrder because:
The Order's Pick is Ready For Planning or Planned.
".Trim(), Logger.Logs);
			});
		}

		public void TestGetReasonForNotAbleToUpdateMatchedDocket_PickIsPlanned()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var whsOrder = WhsOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var line = whsOrder.Lines.AddNew();
			line.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			line.WE_TransactionQuantity = 10m;
			whsOrder.WD_ExternalReference = "ORDERME";
			whsOrder.WD_DocketID = "W00000002";
			whsOrder.Warehouse.WW_WarehouseName = "CoolShack";
			whsOrder.Warehouse.WW_WarehouseCode = "WSS";

			Helper.CreateWhsReceiveWithInventory(whsOrder.Client, whsOrder.Warehouse, "R1", line.SupplierPart, 100m, whsOrder.Warehouse.DefaultOutboundDockDoorLocation, "");
			Factory.SaveForTesting();

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(whsOrder);
			pick.AutoAllocateItemsWithMock();
			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(whsOrder);
			Factory.SaveForTesting();

			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;

			var orderLineDataObject = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject, orderLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.Lines.Count", 1, whsOrderBO.Lines.Count);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsOrder.
Error - Cannot populate WhsOrder because:
The Order's Pick is Ready For Planning or Planned.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestImport_CannotCancelWarehouseNonBondedJob

		#region TestImport_CannotCancelWarehouseNonBondedJob_AttachedToPickStatus

		public void TestImport_CannotCancelWarehouseNonBondedJob_AttachedToPickStatus()
		{
			var whsOrderBOToLoad = CreateDocketWithStatusCode(DocketStatus.Codes.Entered);
			var orderLine = whsOrderBOToLoad.Lines.AddNew();
			orderLine.WE_OP = Data.Product.PK;
			orderLine.WE_TransactionQuantity = 10m;
			var pick = Helper.CreatePickNew(whsOrderBOToLoad);
			pick.PickOrders();

			Factory.SaveForTesting();

			ShipmentDataObject.Order.Status = new CodeDescriptionPair { Code = "CAN", Description = "Cancelled" };
			ShipmentDataObject.Order.OrderNumber = "ORDER123";

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsOrderBO.WD_DocketStatus", DocketStatus.Codes.AttachedToPick, whsOrderBO.WD_DocketStatus);
				AssertEquals("Docket is not cancelled", false, whsOrderBO.IsCancelled);
				AssertEquals("No fields gets updated as the data object is cancelled.", "TRANS123", whsOrderBOToLoad.WD_TransportReference);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsOrder.
Error - Cannot populate WhsOrder because:
You can only cancel dockets with Entered (Saved) status
".Trim(), Logger.Logs);
			});
		}

		#endregion

		public void TestImport_CannotCancelWarehouseNonBondedJob_SetCancelStatusFail()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();
			var part = Helper.CreateProduct(client, "P1");

			var order = Helper.CreateWhsOrder(client, whs, "ORDER123");
			Helper.CreateWhsOrderLine(order, part, 50m);
			order.WD_DocketStatus = DocketStatus.Codes.Entered;
			order.WD_DocketID = "W00000002";
			order.WD_TransportReference = "TRANS123";

			var load = Helper.CreateWhsLoad(client, whs.DefaultOutboundDockDoorLocation);
			order.WD_WLO_PlannedLoad = load.PK;

			Factory.SaveForTesting();
			AssertTestImport_CannotCancelWarehouseNonBondedJob_SetCancelStatusFail(order, "The warehouse WhsOrder - W00000002 failed set to canceled because The order cannot be canceled while it is assigned to a Load.");
		}

		#endregion

		#region TestTotalWeightAndVolumeCalculationsFromLines

		#region TestTotalWeightAndVolume_UXMLEmpty_NoExistingDocket

		public void TestTotalWeightAndVolume_UXMLEmpty_NoExistingDocket()
		{
			var product = SetupProductWithWeightAndVolume();
			Factory.SaveForTesting();

			SetupUXMLWithTwoOrderLinesTotaling_10_Units(product, 0, 0);
			RunAndAssertTotals(0.004m * 10, 0.5m * 10);
		}

		#endregion

		#region TestTotalWeightAndVolume_UXMLEmpty_ExistingDocketPositiveTotals

		public void TestTotalWeightAndVolume_UXMLEmpty_ExistingDocketPositiveTotals()
		{
			var product = SetupProductWithWeightAndVolume();
			SetupExistingOrder(8, 15, "W1");
			Factory.SaveForTesting();

			SetupUXMLWithTwoOrderLinesTotaling_10_Units(product, 0, 0, "W1");

			RunAndAssertTotals(0.004m * 10, 0.5m * 10);
		}

		#endregion

		#region TestTotalWeightAndVolume_UXMLFilled_NoExistingDocket

		public void TestTotalWeightAndVolume_UXMLFilled_NoExistingDocket()
		{
			var product = SetupProductWithWeightAndVolume();
			Factory.SaveForTesting();

			SetupUXMLWithTwoOrderLinesTotaling_10_Units(product, 3, 12);

			RunAndAssertTotals(3, 12);
		}

		#endregion

		#region TestTotalWeightAndVolume_UXMLFilled_ExistingDocketZeroTotals

		public void TestTotalWeightAndVolume_UXMLFilled_ExistingDocketZeroTotals()
		{
			var product = SetupProductWithWeightAndVolume();
			SetupExistingOrder(0, 0, "W1");
			Factory.SaveForTesting();

			SetupUXMLWithTwoOrderLinesTotaling_10_Units(product, 3, 12, "W1");

			RunAndAssertTotals(3, 12);
		}

		#endregion

		#region TestTotalWeightAndVolume_UXMLFilled_ExistingDocketPositiveTotals

		public void TestTotalWeightAndVolume_UXMLFilled_ExistingDocketPositiveTotals()
		{
			var product = SetupProductWithWeightAndVolume();
			SetupExistingOrder(8, 15, "W1");
			Factory.SaveForTesting();

			SetupUXMLWithTwoOrderLinesTotaling_10_Units(product, 3, 12, "W1");

			RunAndAssertTotals(3, 12);
		}

		#endregion

		#region Helpers

		OrgSupplierPart SetupProductWithWeightAndVolume()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var product = Data.Product;

			product.OP_Cubic = 4m;
			product.OP_CubicUQ = "L";
			product.OP_Weight = 500m;
			product.OP_WeightUQ = "G";

			return product;
		}

		void RunAndAssertTotals(ZDecimal expectedWD_TotalCubic, ZDecimal expecteWD_TotalWeight)
		{
			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var orderBO = reader.ReadIntoBusinessObject();
			AssertNotNull(orderBO);
			AssertEquals("orderBO.Lines.Count", 2, orderBO.Lines.Count);
			AssertEquals(expectedWD_TotalCubic, orderBO.WD_TotalCubic);
			AssertEquals("orderBO.WD_TotalCubicUnit remains Default", "M3", orderBO.WD_TotalCubicUnit);
			AssertEquals(expecteWD_TotalWeight, orderBO.WD_TotalWeight);
			AssertEquals("orderBO.WD_TotalWeightUnit remains Default", "KG", orderBO.WD_TotalWeightUnit);
		}

		WhsOrder SetupExistingOrder(ZDecimal wD_TotalCubic, ZDecimal wD_TotalWeight, string reference)
		{
			Data.CreateClientOrgCRAHOLSYDInDB();

			var whsOrder = WhsOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory, Data.GetOrCreateWarehouseInDB().PK);
			var line = whsOrder.Lines.AddNew();
			line.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			line.WE_TransactionQuantity = 10m;

			whsOrder.WD_DocketID = "W00000002";
			whsOrder.WD_TotalCubic = wD_TotalCubic;
			whsOrder.WD_TotalWeight = wD_TotalWeight;
			whsOrder.WD_ExternalReference = reference;

			Helper.CreateWhsReceiveWithInventory(whsOrder.Client, whsOrder.Warehouse, "R1", line.SupplierPart, 100m);
			Factory.SaveForTesting();

			return whsOrder;
		}

		void SetupUXMLWithTwoOrderLinesTotaling_10_Units(OrgSupplierPart product, ZDecimal totalVolume, ZDecimal totalWeight, string reference = "")
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, product, 6m);
			Helper.CreateWhsOrderLine(order, product, 4m);
			var shipmentDataObject = GetNewDocketDataObject(order);

			ShipmentDataObject.Order = shipmentDataObject.Order;
			ShipmentDataObject.Order.OrderNumber = "ORDERME";
			// set totals and reference according test requirements
			ShipmentDataObject.Order.TotalLineVolume = totalVolume;
			ShipmentDataObject.Order.TotalLineWeight = totalWeight == 0 ? null : totalWeight;
			ShipmentDataObject.Order.OrderNumber = reference;
			Logger.ClearLogs();
		}

		#endregion

		#endregion

		#region TestReader_DuplicateOrderLineNumber

		public void TestReader_DuplicateOrderLineNumber()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			WhsOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);

			Factory.SaveForTesting();

			var orderLineDataObject1 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			SetLineSubLineNumber(orderLineDataObject1, new ZShort(2), new ZShort(1));
			var orderLineDataObject2 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			SetLineSubLineNumber(orderLineDataObject2, new ZShort(2), new ZShort(2));

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1, orderLineDataObject2 });
			ShipmentDataObject.Order.Type = new CodeDescriptionPair { Code = "CUS" };

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.Lines.Count", 2, whsOrderBO.Lines.Count);
			AssertOrderLine(whsOrderBO.Lines[0], 2, 1);
			AssertOrderLine(whsOrderBO.Lines[1], 2, 2);
		}

		public void TestReader_DuplicateOrderLineNumber_ImportMultipleLines()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			WhsOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);

			Factory.SaveForTesting();

			var orderLineDataObject1 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject1.LineNumber = new ZShort(1);
			orderLineDataObject1.SubLineNumber = null;
			orderLineDataObject1.PalletID = "JustForTest1";

			var orderLineDataObject2 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject2.LineNumber = new ZShort(1);
			orderLineDataObject2.SubLineNumber = 0;
			orderLineDataObject2.PalletID = "JustForTest2";

			var orderLineDataObject3 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject3.LineNumber = new ZShort(1);
			orderLineDataObject3.SubLineNumber = 1;
			orderLineDataObject3.PalletID = "JustForTest3";

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1, orderLineDataObject2, orderLineDataObject3 });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.Lines.Count", 3, whsOrderBO.Lines.Count);
			var orderLine1 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest2");
			AssertOrderLine(orderLine1, 1, 0);
			var orderLine2 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest3");
			AssertOrderLine(orderLine2, 1, 1);
			var orderLine3 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest1");
			AssertOrderLine(orderLine3, 1, 2);
		}

		public void TestReader_DuplicateOrderLineNumber_ImportMultipleLines_WithDifferentLineNumbers()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			WhsOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);

			Factory.SaveForTesting();

			var orderLineDataObject1 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject1.LineNumber = new ZShort(1);
			orderLineDataObject1.SubLineNumber = null;
			orderLineDataObject1.PalletID = "JustForTest1";

			var orderLineDataObject2 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject2.LineNumber = new ZShort(2);
			orderLineDataObject2.SubLineNumber = 0;
			orderLineDataObject2.PalletID = "JustForTest2";

			var orderLineDataObject3 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject3.LineNumber = new ZShort(1);
			orderLineDataObject3.SubLineNumber = 1;
			orderLineDataObject3.PalletID = "JustForTest3";

			var orderLineDataObject4 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject4.LineNumber = new ZShort(2);
			orderLineDataObject4.SubLineNumber = null;
			orderLineDataObject4.PalletID = "JustForTest4";

			var orderLineDataObject5 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject5.LineNumber = new ZShort(1);
			orderLineDataObject5.SubLineNumber = 0;
			orderLineDataObject5.PalletID = "JustForTest5";

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1, orderLineDataObject2, orderLineDataObject3, orderLineDataObject4, orderLineDataObject5 });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.Lines.Count", 5, whsOrderBO.Lines.Count);
			var orderLine1 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest5");
			AssertOrderLine(orderLine1, 1, 0);
			var orderLine2 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest3");
			AssertOrderLine(orderLine2, 1, 1);
			var orderLine3 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest1");
			AssertOrderLine(orderLine3, 1, 2);
			var orderLine4 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest2");
			AssertOrderLine(orderLine4, 2, 0);
			var orderLine5 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest4");
			AssertOrderLine(orderLine5, 2, 1);
		}

		public void TestReader_DuplicateOrderLineNumber_ImportMultipleLines_AllHaveNoSubLineNumber()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			WhsOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);

			Factory.SaveForTesting();

			var orderLineDataObject1 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject1.LineNumber = new ZShort(1);
			orderLineDataObject1.SubLineNumber = null;
			orderLineDataObject1.PalletID = "JustForTest1";

			var orderLineDataObject2 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject2.LineNumber = new ZShort(2);
			orderLineDataObject2.SubLineNumber = null;
			orderLineDataObject2.PalletID = "JustForTest2";

			var orderLineDataObject3 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject3.LineNumber = new ZShort(1);
			orderLineDataObject3.SubLineNumber = null;
			orderLineDataObject3.PalletID = "JustForTest3";

			var orderLineDataObject4 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject4.LineNumber = new ZShort(2);
			orderLineDataObject4.SubLineNumber = null;
			orderLineDataObject4.PalletID = "JustForTest4";

			var orderLineDataObject5 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject5.LineNumber = new ZShort(1);
			orderLineDataObject5.SubLineNumber = null;
			orderLineDataObject5.PalletID = "JustForTest5";

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1, orderLineDataObject2, orderLineDataObject3, orderLineDataObject4, orderLineDataObject5 });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.Lines.Count", 5, whsOrderBO.Lines.Count);
			var orderLine1 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest1");
			AssertOrderLine(orderLine1, 1, 0);
			var orderLine2 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest3");
			AssertOrderLine(orderLine2, 1, 1);
			var orderLine3 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest5");
			AssertOrderLine(orderLine3, 1, 2);
			var orderLine4 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest2");
			AssertOrderLine(orderLine4, 2, 0);
			var orderLine5 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest4");
			AssertOrderLine(orderLine5, 2, 1);
		}

		public void TestReader_DuplicateOrderLineNumber_OrderLineSubNoAlreadyExists()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var whsOrder = WhsOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var line = whsOrder.Lines.AddNew();
			line.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			line.WE_TransactionQuantity = 10m;
			line.WE_LineNo = 1;
			line.WE_SubLineNo = 0;
			whsOrder.WD_ExternalReference = "ORDERME";
			whsOrder.WD_DocketID = "W00000002";
			whsOrder.Warehouse.WW_WarehouseName = "CoolShack";
			whsOrder.Warehouse.WW_WarehouseCode = "WSS";

			Factory.SaveForTesting();

			var orderLineDataObject1 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject1.LineNumber = new ZShort(1);
			orderLineDataObject1.SubLineNumber = null;
			orderLineDataObject1.PalletID = "JustForTest";

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1 });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.Lines.Count", 1, whsOrderBO.Lines.Count);
			var orderLine1 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest");
			AssertOrderLine(orderLine1, 1, 1);
		}

		public void TestReader_DuplicateOrderLineNumber_OrderLineSubNoAlreadyExists_MultipleLineSubNosExist()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var whsOrder = WhsOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var line1 = whsOrder.Lines.AddNew();
			line1.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			line1.WE_TransactionQuantity = 10m;
			line1.WE_LineNo = 1;
			line1.WE_SubLineNo = 0;
			var line2 = whsOrder.Lines.AddNew();
			line2.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			line2.WE_TransactionQuantity = 10m;
			line2.WE_LineNo = 1;
			line2.WE_SubLineNo = 1;
			var line3 = whsOrder.Lines.AddNew();
			line3.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			line3.WE_TransactionQuantity = 10m;
			line3.WE_LineNo = 1;
			line3.WE_SubLineNo = 2;
			whsOrder.WD_ExternalReference = "ORDERME";
			whsOrder.WD_DocketID = "W00000002";
			whsOrder.Warehouse.WW_WarehouseName = "CoolShack";
			whsOrder.Warehouse.WW_WarehouseCode = "WSS";

			Factory.SaveForTesting();

			var orderLineDataObject1 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject1.LineNumber = new ZShort(1);
			orderLineDataObject1.SubLineNumber = null;
			orderLineDataObject1.PalletID = "JustForTest";

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1 });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.Lines.Count", 1, whsOrderBO.Lines.Count);
			var orderLine1 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest");
			AssertOrderLine(orderLine1, 1, 3);
		}

		public void TestReader_DuplicateOrderLineNumber_OrderLineSubNoAlreadyExists_ImportMultipleLines()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var whsOrder = WhsOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var line1 = whsOrder.Lines.AddNew();
			line1.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			line1.WE_TransactionQuantity = 10m;
			line1.WE_LineNo = 1;
			line1.WE_SubLineNo = 0;
			var line2 = whsOrder.Lines.AddNew();
			line2.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			line2.WE_TransactionQuantity = 10m;
			line2.WE_LineNo = 1;
			line2.WE_SubLineNo = 1;
			var line3 = whsOrder.Lines.AddNew();
			line3.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			line3.WE_TransactionQuantity = 10m;
			line3.WE_LineNo = 1;
			line3.WE_SubLineNo = 2;
			whsOrder.WD_ExternalReference = "ORDERME";
			whsOrder.WD_DocketID = "W00000002";
			whsOrder.Warehouse.WW_WarehouseName = "CoolShack";
			whsOrder.Warehouse.WW_WarehouseCode = "WSS";

			Factory.SaveForTesting();

			var orderLineDataObject1 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject1.LineNumber = new ZShort(1);
			orderLineDataObject1.SubLineNumber = null;
			orderLineDataObject1.PalletID = "JustForTest1";

			var orderLineDataObject2 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject2.LineNumber = new ZShort(1);
			orderLineDataObject2.SubLineNumber = 3;
			orderLineDataObject2.PalletID = "JustForTest2";

			var orderLineDataObject3 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject3.LineNumber = new ZShort(1);
			orderLineDataObject3.SubLineNumber = null;
			orderLineDataObject3.PalletID = "JustForTest3";

			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1, orderLineDataObject2, orderLineDataObject3 });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.Lines.Count", 3, whsOrderBO.Lines.Count);
			var orderLine1 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest2");
			AssertOrderLine(orderLine1, 1, 3);
			var orderLine2 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest1");
			AssertOrderLine(orderLine2, 1, 4);
			var orderLine3 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest3");
			AssertOrderLine(orderLine3, 1, 5);
		}

		public void TestReader_DuplicateOrderLineNumber_OrderLineSubNoAlreadyExists_ImportMultipleLines_WithDifferentLineNumbers()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var whsOrder = WhsOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var line1 = whsOrder.Lines.AddNew();
			line1.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			line1.WE_TransactionQuantity = 10m;
			line1.WE_LineNo = 1;
			line1.WE_SubLineNo = 0;
			var line2 = whsOrder.Lines.AddNew();
			line2.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			line2.WE_TransactionQuantity = 10m;
			line2.WE_LineNo = 1;
			line2.WE_SubLineNo = 1;
			var line3 = whsOrder.Lines.AddNew();
			line3.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			line3.WE_TransactionQuantity = 10m;
			line3.WE_LineNo = 2;
			line3.WE_SubLineNo = 0;
			whsOrder.WD_ExternalReference = "ORDERME";
			whsOrder.WD_DocketID = "W00000002";
			whsOrder.Warehouse.WW_WarehouseName = "CoolShack";
			whsOrder.Warehouse.WW_WarehouseCode = "WSS";

			Factory.SaveForTesting();

			var orderLineDataObject1 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject1.LineNumber = new ZShort(1);
			orderLineDataObject1.SubLineNumber = null;
			orderLineDataObject1.PalletID = "JustForTest1";

			var orderLineDataObject2 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject2.LineNumber = new ZShort(2);
			orderLineDataObject2.SubLineNumber = 0;
			orderLineDataObject2.PalletID = "JustForTest2";

			var orderLineDataObject3 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject3.LineNumber = new ZShort(1);
			orderLineDataObject3.SubLineNumber = 1;
			orderLineDataObject3.PalletID = "JustForTest3";

			var orderLineDataObject4 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject4.LineNumber = new ZShort(2);
			orderLineDataObject4.SubLineNumber = null;
			orderLineDataObject4.PalletID = "JustForTest4";

			var orderLineDataObject5 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject5.LineNumber = new ZShort(1);
			orderLineDataObject5.SubLineNumber = 0;
			orderLineDataObject5.PalletID = "JustForTest5";

			var orderLineDataObject6 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject6.LineNumber = new ZShort(2);
			orderLineDataObject6.SubLineNumber = 1;
			orderLineDataObject6.PalletID = "JustForTest6";

			var orderLineDataObject7 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject7.LineNumber = new ZShort(1);
			orderLineDataObject7.SubLineNumber = 2;
			orderLineDataObject7.PalletID = "JustForTest7";

			// | No | LineNumber | SubLineNumber |      | No | LineNumber | SubLineNumber |              [ Updated Lines ]                         [ New Lines ]
			// | 1  |     1      |      null     |      | 5  |     1      |      0        |
			// | 2  |     2      |      0        |      | 3  |     1      |      1        |      | No | LineNumber | SubLineNumber |     | No | LineNumber | SubLineNumber |
			// | 3  |     1      |      1        |      | 7  |     1      |      2        |      | 5  |     1      |      0        |     | 7  |     1      |      2        |
			// | 4  |     2      |      null     |  =>  | 1  |     1      |      null     |  =>  | 3  |     1      |      1        |  +  | 1  |     1      |      3        |
			// | 5  |     1      |      0        |      | 2  |     2      |      0        |      | 2  |     2      |      0        |     | 6  |     2      |      1        |
			// | 6  |     2      |      1        |      | 6  |     2      |      1        |                                              | 4  |     2      |      2        |
			// | 7  |     1      |      2        |      | 4  |     2      |      null     |
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1, orderLineDataObject2, orderLineDataObject3, orderLineDataObject4, orderLineDataObject5, orderLineDataObject6, orderLineDataObject7 });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.Lines.Count", 7, whsOrderBO.Lines.Count);
			var orderLine1 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest5");
			AssertOrderLine(orderLine1, 1, 0);
			var orderLine2 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest3");
			AssertOrderLine(orderLine2, 1, 1);
			var orderLine3 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest2");
			AssertOrderLine(orderLine3, 2, 0);
			var orderLine4 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest7");
			AssertOrderLine(orderLine4, 1, 2);
			var orderLine5 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest1");
			AssertOrderLine(orderLine5, 1, 3);
			var orderLine6 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest6");
			AssertOrderLine(orderLine6, 2, 1);
			var orderLine7 = whsOrderBO.Lines.Single(l => l.WE_PalletID == "JustForTest4");
			AssertOrderLine(orderLine7, 2, 2);
		}

		void SetLineSubLineNumber(OrderLine line, ZShort lineNo, ZShort subLineNo)
		{
			line.LineNumber = lineNo;
			line.SubLineNumber = subLineNo;
		}

		void AssertOrderLine(WhsDocketLine line, ZShort lineNo, ZShort subLineNo)
		{
			AssertEquals("LineNo should be " + lineNo.ToString(), lineNo, line.WE_LineNo);
			AssertEquals("SubLineNo should be " + subLineNo.ToString(), subLineNo, line.WE_SubLineNo);
		}

		#endregion

		#region TestOrderLines

		#region TestLineCollectionContentIsPartial

		protected override void SetupAndAssertDocketLinePrecondition(WhsDocketLine docketLine)
		{
			AssertEquals("Pre-condition: WE_UnitDiscountAmount", 0m, docketLine.WE_UnitDiscountAmount);
		}

		protected override void UpdateExistingDocketLineDataObject(OrderLine docketLineDataObject)
		{
			docketLineDataObject.UnitPriceDiscountAmount = 0.5m;
		}

		protected override void AssertExistingDocketLineAfterImport(WhsDocketLine docketLine)
		{
			AssertEquals("WE_UnitDiscountAmount get updated.", 0.5m, docketLine.WE_UnitDiscountAmount);
		}

		protected override WhsOrder CreateDocketWithLine(OrgHeader client, WhsWarehouse warehouse, OrgSupplierPart product, ZDecimal units)
		{
			return Helper.CreateWhsOrderWithOrderLine(client, warehouse, product, units);
		}

		#endregion

		#region TestExistingOrderLinesAreDeletedOnAnEnteredOrder

		public void TestExistingOrderLinesAreDeletedOnAnEnteredOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var shipmentDataObject = GetNewDocketDataObject(order);
			var orderLineDataObject = shipmentDataObject.Order.OrderLineCollection[0];
			orderLineDataObject.LineNumber = 2;
			orderLineDataObject.OrderedQty = 5m;

			var reader = new WhsOrderDataObjectReader(shipmentDataObject, Logger, Factory);
			var matchedOrder = reader.ReadIntoBusinessObject();

			AssertEquals(order, matchedOrder);
			AssertEquals(1, matchedOrder.Lines.Count);
			var orderLine = matchedOrder.Lines[0];
			AssertEquals((ZShort)2, orderLine.WE_LineNo);
			AssertEquals(5m, orderLine.WE_TransactionQuantity);
		}

		#endregion

		#region TestExistingOrderLinesAreNotDeletedOnAPickingOrder

		public void TestExistingOrderLinesAreNotDeletedOnAPickingOrder()
		{
			var order = CreatePickedOrderInDB();
			var existingOrderLine = order.Lines[0];
			var shipmentDataObject = GetNewDocketDataObject(order);
			var orderLineDataObject = shipmentDataObject.Order.OrderLineCollection[0];
			orderLineDataObject.LineNumber = 2;
			orderLineDataObject.OrderedQty = 5m;

			var reader = new WhsOrderDataObjectReader(shipmentDataObject, Logger, Factory);
			var matchedOrder = reader.ReadIntoBusinessObject();

			AssertEquals(order, matchedOrder);
			AssertEquals(2, matchedOrder.Lines.Count);
			matchedOrder.Lines.ApplySort(WhsDocketLineSchema.Constants.WE_LineNo, ListSortDirection.Ascending);
			var orderLine1 = matchedOrder.Lines[0];
			var orderLine2 = matchedOrder.Lines[1];
			AssertEquals(existingOrderLine, orderLine1);
			AssertEquals((ZShort)1, orderLine1.WE_LineNo);
			AssertEquals(10m, orderLine1.WE_TransactionQuantity);
			AssertEquals((ZShort)2, orderLine2.WE_LineNo);
			AssertEquals(5m, orderLine2.WE_TransactionQuantity);
		}

		#endregion

		#region TestWithOrderLines

		public void TestWithOrderLines()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var product = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			Helper.SetClientAllAttributeType(order.Client, false);
			Helper.SetProductAllAttributeUse(order.Client, product, true);
			Helper.SetClientAttributeType(order.Client, AttributeNumber.Serial, true);
			Factory.SaveForTesting();

			var orderLineDataObject = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject });
			ShipmentDataObject.Order.Type = new CodeDescriptionPair { Code = "CUS" };

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.Lines.Count", 1, whsOrderBO.Lines.Count);

			CombineAssertions(delegate
			{
				var orderLineBO = whsOrderBO.Lines[0];
				WhsOrderLineDataObjectReaderTest.AssertStandardOrderLineContents(orderLineBO, useSerial: true);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsOrder found, creating new WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsOrderLine found, creating new WhsOrderLine.
Information - Populating WhsOrderLine...
Information - Added Warehouse Order from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestWithOrderLines_WhsOrderLineToBeClearedLaterAddedOnIfNoEntryKey()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var product = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			Helper.SetClientAllAttributeType(order.Client, false);
			Helper.SetProductAllAttributeUse(order.Client, product, use: true, setReleaseCaptured: false, useSerialNumber: false);

			Factory.SaveForTesting();

			var orderLineDataObject1 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject1.SerialNumber = "";
			orderLineDataObject1.LineNumber = 2;
			var orderLineDataObject2 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject2.SerialNumber = "";
			orderLineDataObject2.LineNumber = 1;
			orderLineDataObject2.CustomsData.EntryKey = string.Empty;
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1, orderLineDataObject2 });
			ShipmentDataObject.Order.Type = new CodeDescriptionPair { Code = "CUS" };

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.Lines.Count", 2, whsOrderBO.Lines.Count);

			var orderLineBO1 = whsOrderBO.Lines[1];
			AssertEquals("KEYZOR", orderLineBO1.CustomsData.WB_EntryKey);
			WhsOrderLineDataObjectReaderTest.AssertStandardOrderLineContents(orderLineBO1, useSerial: false);
			AssertEquals(ZBool.False, orderLineBO1.WhsOrderLineToBeClearedLater);

			var orderLineBO2 = whsOrderBO.Lines[0];
			AssertEquals(ZString.Empty, orderLineBO2.CustomsData.WB_EntryKey);
			AssertEquals(ZBool.False, orderLineBO2.WhsOrderLineToBeClearedLater);

			AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsOrder found, creating new WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsOrderLine found, creating new WhsOrderLine.
Information - Populating WhsOrderLine...
Information - No matching WhsOrderLine found, creating new WhsOrderLine.
Information - Populating WhsOrderLine...
Information - Added Warehouse Order from UniversalShipment.
".Trim(), Logger.Logs);
		}

		public void TestWithOrderLines_WhsOrderLineToBeClearedLaterAddedOnIfNoEntryKey_IfCustomAllows()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			var order = WhsOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var product = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			Helper.SetClientAllAttributeType(order.Client, false);
			Helper.SetProductAllAttributeUse(order.Client, product, use: true, setReleaseCaptured: false, useSerialNumber: false);

			Factory.SaveForTesting();

			var orderLineDataObject1 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject1.LineNumber = 2;
			orderLineDataObject1.SerialNumber = "";
			var orderLineDataObject2 = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			orderLineDataObject2.LineNumber = 1;
			orderLineDataObject2.SerialNumber = "";
			orderLineDataObject2.CustomsData.EntryKey = string.Empty;
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject1, orderLineDataObject2 });
			ShipmentDataObject.Order.Type = new CodeDescriptionPair { Code = "CUS" };

			WhsOrder whsOrderBO = null;
			using (WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
				whsOrderBO = reader.ReadIntoBusinessObject();
			}
			AssertNotNull(whsOrderBO);
			AssertEquals("whsOrderBO.Lines.Count", 2, whsOrderBO.Lines.Count);

			var orderLineBO1 = whsOrderBO.Lines[1];
			AssertEquals("KEYZOR", orderLineBO1.CustomsData.WB_EntryKey);
			WhsOrderLineDataObjectReaderTest.AssertStandardOrderLineContents(orderLineBO1, useSerial: false);
			AssertEquals(ZBool.False, orderLineBO1.WhsOrderLineToBeClearedLater);

			var orderLineBO2 = whsOrderBO.Lines[0];
			AssertEquals(ZString.Empty, orderLineBO2.CustomsData.WB_EntryKey);
			AssertEquals(ZBool.True, orderLineBO2.WhsOrderLineToBeClearedLater);

			AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsOrder found, creating new WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching WhsOrderLine found, creating new WhsOrderLine.
Information - Populating WhsOrderLine...
Information - No matching WhsOrderLine found, creating new WhsOrderLine.
Information - Populating WhsOrderLine...
Information - Added Warehouse Order from UniversalShipment.
".Trim(), Logger.Logs);
		}

		#endregion

		#region TestOrderLinesGetCorrectIndexForExceptionMessage

		public void TestOrderLinesGetCorrectIndexForExceptionMessage()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			WhsOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);

			Factory.SaveForTesting();

			var orderLineDataObject = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject, new OrderLine() });
			ShipmentDataObject.Order.Type = new CodeDescriptionPair { Code = "CUS" };

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			AssertExceptionThrown("Cannot import OrderLine without valid Product Code.", typeof(DataObjectReadFailureException),
				"Cannot Import Order Line 2\r\nNo Product was provided.", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestOrderLinesAreNotUpdatedOnAFinalizedOrder

		public void TestOrderLinesAreNotUpdatedOnAFinalizedOrder()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			var whsOrder = WhsOrderLineDataObjectReaderTest.GetNewDocketLineParent(Factory);
			var line = whsOrder.Lines.AddNew();
			line.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			line.WE_TransactionQuantity = 10m;
			whsOrder.WD_ExternalReference = "ORDERME";
			whsOrder.WD_DocketID = "W00000002";
			whsOrder.Warehouse.WW_WarehouseName = "CoolShack";
			whsOrder.Warehouse.WW_WarehouseCode = "WSS";

			Helper.CreateWhsReceiveWithInventory(whsOrder.Client, whsOrder.Warehouse, "R1", line.SupplierPart, 100m, whsOrder.Warehouse.DefaultOutboundDockDoorLocation, "");
			Factory.SaveForTesting();

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(whsOrder);
			pick.AutoAllocateItemsWithMock();
			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(whsOrder);

			Factory.SaveForTesting();

			var orderLineDataObject = WhsOrderLineDataObjectReaderTest.SetupStandardOrderLine();
			ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>() { orderLineDataObject, orderLineDataObject });
			ShipmentDataObject.Order.OrderNumber = "ORDERME";

			var reader = new WhsOrderDataObjectReader(ShipmentDataObject, Logger, Factory);
			var whsOrderBO = GetDocketWithAllocateMock(reader);

			AssertNotNull(whsOrderBO);

			CombineAssertions(delegate
			{
				AssertEquals("whsOrderBO.Lines.Count", 1, whsOrderBO.Lines.Count);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Warning - Cannot update read-only Field 'Warehouse' [WD_WW_Whs]. Cannot change 'WSS' (CoolShack) to 'WHS' (CoolHouse).
Warning - Cannot update Order Lines on a Finalized or In Picking Order.
Information - Updated Warehouse Order W00000002 from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestOrderLinesAreNotUpdatedOnAnOrderInPicking

		public void TestOrderLinesAreNotUpdatedOnAnOrderInPicking_RegistryEnabled()
		{
			var order = CreatePickedOrderInDB();
			var existingOrderLine = order.Lines[0];
			var shipmentDataObject = GetNewDocketDataObject(order);
			var orderLineDataObject = shipmentDataObject.Order.OrderLineCollection[0];
			orderLineDataObject.LineNumber = 2;
			orderLineDataObject.OrderedQty = 5m;

			AssertEquals("Precondition: order has 1 line.", 1, order.Lines.Count);

			using (WarehouseDataRegistry.Instance.PreventOrderLinesUpdateWhenOrderIsInPicking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var reader = new WhsOrderDataObjectReader(shipmentDataObject, Logger, Factory);
				var matchedOrder = reader.ReadIntoBusinessObject();

				AssertNotNull(matchedOrder);

				CombineAssertions(delegate
				{
					AssertEquals("Matched order and pre-created order are the same.", order, matchedOrder);
					AssertEquals("Order still has 1 line after import.", 1, matchedOrder.Lines.Count);
					AssertContains("Warning - Cannot update Order Lines on a Finalized or In Picking Order.", Logger.Logs);
				});
			}
		}

		public void TestOrderLinesAreNotUpdatedOnAnOrderInPicking_RegistryDisabled()
		{
			var order = CreatePickedOrderInDB();
			var existingOrderLine = order.Lines[0];
			var shipmentDataObject = GetNewDocketDataObject(order);
			var orderLineDataObject = shipmentDataObject.Order.OrderLineCollection[0];
			orderLineDataObject.LineNumber = 2;
			orderLineDataObject.OrderedQty = 5m;

			AssertEquals("Precondition: order has 1 line.", 1, order.Lines.Count);

			var reader = new WhsOrderDataObjectReader(shipmentDataObject, Logger, Factory);
			var matchedOrder = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				AssertEquals("Matched order and pre-created order are the same.", order, matchedOrder);
				AssertEquals("New order line created.", 2, matchedOrder.Lines.Count);
				AssertNotContains("Warning - Cannot update Order Lines on a Finalized or In Picking Order.", Logger.Logs);
			});
		}

		#endregion

		WhsOrder CreatePickedOrderInDB()
		{
			// create an order and pick it
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			Helper.CreateWarehouse("2");

			var client2 = Helper.CreateClient("2");
			data.Part1.RelatedOrganisations.AddOwner(client2);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_RequiredDate = new ZDateTimeOffset(2013, 3, 17);
			Helper.CreatePickNew(order);
			Factory.SaveForTesting();
			AssertEquals("Precondition", true, order.IsAttachedToPickButNotFinalised);

			// export the order
			return order;
		}

		#endregion

		#region TestRejectImportDueToInvalidCharacters

		public void TestRejectImportDueToInvalidCharacters_BookingConfirmationReference()
		{
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();

			ShipmentDataObject.BookingConfirmationReference = "你好";
			var order = ShipmentDataObject.Order;
			order.ClientReference = "";

			var reader = GetNewReader(ShipmentDataObject, Logger);

			var expectedErrorString = @"Cannot perform import due to invalid characters in field: BookingConfirmationReference.";
			AssertExceptionThrown(typeof(DataObjectReadFailureException), expectedErrorString, () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestInvoicingJobCreatedOnImport

		public void TestInvoicingJobCreatedOnImport()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB();
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.ClientAddressDataObject_CRAHOLSYD);

			var reader = new WhsOrderDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
			var order = reader.ReadIntoBusinessObject();

			var job = order.JobHeader;
			AssertNotNull("Order should have Job Header created on Import.", job);
			job.Dispose(); // Disposes Mutex
		}

		#endregion

		#region TestUniversalImportForRequiredByDateInOrder

		[TestDate(2014, 11, 19)]
		public void TestUniversalImportForRequiredByDateInOrder()
		{
			AssertUniversalImportForRequiredByDateInOrder(false);
		}

		[TestDate(2014, 11, 19)]
		public void TestUniversalImportForRequiredByDateInOrder_RequiredDateIsUsedAsOutwardsFinalisedDate()
		{
			AssertUniversalImportForRequiredByDateInOrder(true);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		void AssertUniversalImportForRequiredByDateInOrder(bool useRequiredDateForOutwardsFinalisedDate)
		{
			TestDateAttribute.UseUNLOCO = true;
			var now = ZDateTimeOffset.Now;
			Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB();

			// entered status
			var order = Helper.CreateWhsOrder(Data.Orgs.CRAHOLSYD, warehouse);
			order.WD_RequiredDate = now.AddDays(-1);
			order.WD_ExternalReference = "EXTO1";
			order.Warehouse.WW_UseRequiredDateForOutwardsFinalisedDate = useRequiredDateForOutwardsFinalisedDate;
			Factory.SaveForTesting();

			ShipmentDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.Instance);
			ShipmentDataObject.Order.OrderNumber = "EXTO1";
			var whsOrderFromImport1 = ReadWhsOrderForRequiredByDate(order, ShipmentDataObject, now.AddDays(2));
			AssertOrderForRequiredByDateInUniversalImport(whsOrderFromImport1, order, DocketStatus.Codes.Entered, now.AddDays(2), true);

			// Cancelled status
			whsOrderFromImport1.CancelReactivateDocket();
			Assert("Precondition", whsOrderFromImport1.IsCancelled);

			Factory.SaveForTesting();
			var whsOrderFromImport2 = ReadWhsOrderForRequiredByDate(order, ShipmentDataObject, now.AddDays(3));
			AssertOrderForRequiredByDateInUniversalImport(whsOrderFromImport2, order, DocketStatus.Codes.Cancelled, now.AddDays(2), false);

			// picked status
			whsOrderFromImport2.WD_DocketStatus = DocketStatus.Codes.Entered;
			var part = Helper.CreateProduct(whsOrderFromImport2.Client, "P1");
			Helper.CreateWhsReceiveWithInventory(whsOrderFromImport2.Client, whsOrderFromImport2.Warehouse, "R1", part, 100m);
			Factory.SaveForTesting();

			Helper.CreateWhsOrderLine(order, part, 10m);
			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			pick.AutoAllocateItems();
			Factory.SaveForTesting();
			AssertEquals("Precondition - Order must be picked", DocketStatus.Codes.AttachedToPick, whsOrderFromImport2.WD_DocketStatus);

			var whsOrderFromImport3 = ReadWhsOrderForRequiredByDate(order, ShipmentDataObject, now.AddDays(3));
			AssertOrderForRequiredByDateInUniversalImport(whsOrderFromImport3, order, DocketStatus.Codes.AttachedToPick, now.AddDays(3), true);

			// Finalised Status
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(pick);
			AssertIsFinalisedPrecondition(whsOrderFromImport3);

			var whsOrderFromImport4 = ReadWhsOrderForRequiredByDate(order, ShipmentDataObject, now.AddDays(4));
			AssertOrderForRequiredByDateInUniversalImport(whsOrderFromImport4, order, WhsOrderStatus.Codes.Departed,
				useRequiredDateForOutwardsFinalisedDate ? now.AddDays(3) : now.AddDays(4), !useRequiredDateForOutwardsFinalisedDate);
		}

		WhsOrder ReadWhsOrderForRequiredByDate(WhsOrder order, UniversalShipment shipment, ZDateTimeOffset shipmentRequiredByDate)
		{
			shipment.LocalProcessing.DeliveryRequiredBy = shipmentRequiredByDate.ToZDateTime();
			using (Res.TemporarilySwitchLanguage(Constants.Languages.EnglishAmerican)) // Canceled/Cancelled etc.
			{
				var reader = new WhsOrderDataObjectReader(shipment, Logger, Factory);
				var whsOrderBO = reader.ReadIntoBusinessObject();
				AssertNotNull(whsOrderBO);
				return whsOrderBO;
			}
		}

		void AssertOrderForRequiredByDateInUniversalImport(WhsOrder order, WhsOrder importedOrder, string expectedStatus, ZDateTimeOffset expectedRequiredByDate, bool shouldRequiredByDateChanged)
		{
			CombineAssertions(delegate
			{
				AssertEquals("Loads same order.", importedOrder.PK, order.PK);
				AssertEquals("whsOrderBO.WD_DocketStatus", expectedStatus, importedOrder.WD_DocketStatus);
				AssertEquals("whsOrderBO.WD_RequiredDate", expectedRequiredByDate, importedOrder.WD_RequiredDate);

				if (expectedStatus == DocketStatus.Codes.Cancelled)
				{
					AssertEquals($"Cannot populate WhsOrder because:\r\n{importedOrder.HumanReadableName} could not be updated because it is Canceled.", Logger.GetErrors());
					AssertEquals(false, Logger.HasWarnings);
				}
				else if (shouldRequiredByDateChanged)
				{
					AssertEquals(false, Logger.HasErrors);
					AssertEquals(false, Logger.HasWarnings);
				}
				else
				{
					AssertEquals(false, Logger.HasErrors);
					AssertEquals($"Cannot update read-only Field 'Required Date' [WD_RequiredDate]. Cannot change '{ZDateTimeOffset.Now.AddDays(3)}' to '{ZDateTimeOffset.Now.AddDays(4)}'.", Logger.GetWarnings());
				}
			});
			Logger.ClearLogs();
		}

		#endregion

		#region TestImportOfWarehouse_WhenDocketHasLinesCore

		protected override void TestImportOfWarehouse_WhenDocketHasLinesCore()
		{
			var otherWhs = Helper.CreateWarehouse("WSS", "CoolShack");
			var part = Helper.CreateProduct("P1", Data.Orgs.CRAHOLSYD);
			var order = Helper.CreateWhsOrder(Data.Orgs.CRAHOLSYD, otherWhs, "123");
			var line = Helper.CreateWhsOrderLine(order, part, 10m);
			Factory.SaveForTesting();

			Data.GetOrCreateWarehouseInDB();
			Factory.SaveForTesting();

			AssertEquals("Precondition: Warehouse", "WSS", order.Warehouse.WW_WarehouseCode);

			Data.ShipmentDataObject.DataContext.AddDataTarget(DataContext, null);
			Data.ShipmentDataObject.Order.OrderNumber = "123";
			Data.ShipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>());

			var reader1 = GetNewReader(Data.ShipmentDataObject, Logger);
			var orderBO = reader1.ReadIntoBusinessObject();
			AssertNotNull(orderBO);
			AssertEquals("Should have updated the docket.", orderBO, order);

			AssertEquals("Should have updated warehouse.", "WHS", order.Warehouse.WW_WarehouseCode);
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
		}

		#endregion

		#region TestImportOfWarehouse_PreventChangingIfWarehouseIsReadOnly

		public void TestImportOfWarehouse_PreventChangingIfWarehouseIsReadOnly()
		{
			var otherWhs = Helper.CreateWarehouse("WSS", "CoolShack");
			Factory.SaveForTesting();

			var order = Helper.CreateWhsOrder(Data.Orgs.CRAHOLSYD, otherWhs, "123");
			var orderLine = Helper.CreateWhsOrderLine(order, Data.Product, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.SaveForTesting();

			Data.GetOrCreateWarehouseInDB();
			Factory.SaveForTesting();

			AssertEquals("Precondition: Warehouse is ReadOnly", true, order.WD_WW_WhsInfo.ReadOnly);

			Data.ShipmentDataObject.DataContext.AddDataTarget(DataContext, null);
			Data.ShipmentDataObject.Order.OrderNumber = "123";

			var reader1 = GetNewReader(Data.ShipmentDataObject, Logger);
			var orderBO = reader1.ReadIntoBusinessObject();
			AssertNotNull(orderBO);

			CombineAssertions(delegate
			{
				AssertEquals("Should have updated the docket.", orderBO, order);
				AssertEquals("Should not have updated warehouse.", "WSS", order.Warehouse.WW_WarehouseCode);
				AssertNoExceptionThrown(() => Factory.SaveForTesting());

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Warning - Cannot update read-only Field 'Warehouse' [WD_WW_Whs]. Cannot change 'WSS' (WSS) to 'WHS' (CoolHouse).
Information - Updated Warehouse Order W00000001 from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestImportOfWarehouse_PreventChangingIfHasCrossDockLocation

		public void TestImportOfWarehouse_PreventChangingIfHasCrossDockLocation()
		{
			var otherWhs = Helper.CreateWarehouse("WSS", "CoolShack");
			var part = Helper.CreateProduct("P1", Data.Orgs.CRAHOLSYD);
			Factory.SaveForTesting();

			var order = Helper.CreateWhsOrder(Data.Orgs.CRAHOLSYD, otherWhs, "123");
			order.WD_WL_CrossDock = otherWhs.WW_DefaultOutboundDockDoor;
			Factory.SaveForTesting();

			Data.GetOrCreateWarehouseInDB();
			Factory.SaveForTesting();

			AssertEquals("Precondition: Order has cross dock location", true, order.WD_WL_CrossDock.IsValid);

			Data.ShipmentDataObject.DataContext.AddDataTarget(DataContext, null);
			Data.ShipmentDataObject.Order.OrderNumber = "123";

			var reader1 = GetNewReader(Data.ShipmentDataObject, Logger);
			var orderBO = reader1.ReadIntoBusinessObject();
			AssertNotNull(orderBO);

			CombineAssertions(delegate
			{
				AssertEquals("Should have updated the docket.", orderBO, order);
				AssertEquals("Should not have updated warehouse.", "WSS", order.Warehouse.WW_WarehouseCode);
				AssertNoExceptionThrown(() => Factory.SaveForTesting());

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Warning - Cannot update warehouse from WSS to WHS as it is not allowed.
Information - Updated Warehouse Order W00000001 from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestImportOfWarehouse_PreventChangingIfHasReservedStock

		public void TestImportOfWarehouse_PreventChangingIfHasReservedStock()
		{
			var otherWhs = Helper.CreateWarehouse("WSS", "CoolShack");
			var part = Helper.CreateProduct("P1", Data.Orgs.CRAHOLSYD);
			var receive = Helper.CreateWhsReceiveWithInventory(Data.Orgs.CRAHOLSYD, otherWhs, "R1", part, 10m);
			Factory.SaveForTesting();

			var order = Helper.CreateWhsOrder(Data.Orgs.CRAHOLSYD, otherWhs, "123");
			var orderLine = Helper.CreateWhsOrderLine(order, part, 10m);
			orderLine.ReserveStockIfAbleTo(receive.Inventory[0], 10m);
			Factory.SaveForTesting();

			Data.GetOrCreateWarehouseInDB();
			Factory.SaveForTesting();

			AssertEquals("Precondition: Order has reserved stock", true, order.HasReservedStock);

			Data.ShipmentDataObject.DataContext.AddDataTarget(DataContext, null);
			Data.ShipmentDataObject.Order.OrderNumber = "123";

			var reader1 = GetNewReader(Data.ShipmentDataObject, Logger);
			var orderBO = reader1.ReadIntoBusinessObject();
			AssertNotNull(orderBO);

			CombineAssertions(delegate
			{
				AssertEquals("Should have updated the docket.", orderBO, order);
				AssertEquals("Should not have updated warehouse.", "WSS", order.Warehouse.WW_WarehouseCode);
				AssertNoExceptionThrown(() => Factory.SaveForTesting());

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Successfully loaded matching WhsOrder.
Information - Populating WhsOrder...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Warning - Cannot update warehouse from WSS to WHS as it is not allowed.
Information - Updated Warehouse Order W00000002 from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestHoldCode_PreventImportMixedHeldAndAvailableInventory

		public void TestHoldCode_PreventImportMixedHeldAndAvailableInventory()
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // Only possible with EnableHeldGoodsForOrders
			{
				var org = Data.CreateClientOrgCRAHOLSYDInDB();
				var whs = Data.GetOrCreateWarehouseInDB();
				var product = Data.Product;
				Factory.SaveForTesting();

				var orderDataObject = ShipmentDataObject.Order;
				orderDataObject.ClientReference = org.OH_Code;
				orderDataObject.OrderNumber = "O1";
				orderDataObject.UnitsSent = 11m;

				var orderLineDataObject1 = new OrderLine();
				orderLineDataObject1.OrderedQty = 10m;
				orderLineDataObject1.Product = new Product { Code = product.OP_PartNum };

				var orderLineDataObject2 = new OrderLine();
				orderLineDataObject2.OrderedQty = 10m;
				orderLineDataObject2.Product = new Product { Code = product.OP_PartNum };
				orderLineDataObject2.CurrentHoldCode = new CodeDescriptionPair9Char { Code = InventoryHoldCodes.Codes.Damaged, Description = InventoryHoldCodes.Descriptions.Damaged };

				orderDataObject.SetOrderLineCollection(() => new DataObjectList<OrderLine> { orderLineDataObject1, orderLineDataObject2 });

				var reader = GetNewReader(ShipmentDataObject, Logger, useCleanFactory: false);
				AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot order both held and available inventory.", () => reader.ReadIntoBusinessObject());
			}
		}

		#endregion

		// Customs Import tests

		protected override bool IsExWarehouse => true;

		protected override ZBool PopulatesCustomsInfoFromUXML => true;
		protected override ZString ExpectedCustomsAddInfo => "WRL=2*WRN=EntryNumber123*TILV4Warehouse=999*Moo=50"; // inwards (previous) entry
		protected override ZString ExpectedCustomsEntryKey => "DummyOutward-1"; // outwards entry stored on customs attributes
		protected override ZShort ExpectedCustomsEntryLineNo => 102; // outwards entry stored on customs attributes
		protected override ZString ExpectedDefaultOutwardType => "CNN";

		#region Test_CustomsSource_ExactOrderDefectFix

		public void Test_CustomsSource_ExactOrderDefectFix()
		{
			SetupWarehouseForUS_Customs();

			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create 2 products each with a per unit qty of 10 and 20 respectively
			var product1 = Data.CreateProduct("G1");
			var product2 = Data.CreateProduct("G2");

			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);

			// Receive stock
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, warehouse, "R2");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, product1, 100.0m, "EntryNumber123-3", "Grouped", 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, product2, 200.0m, "EntryNumber123-4", "Grouped", 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			// Order stock
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(100m, 3, product1).CustomizedFieldCollection.Clear();
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(200m, 4, product2).CustomizedFieldCollection.Clear();

			Factory.SaveForTesting();

			// test import
			var dataObject = Data.ShipmentDataObject;

			AssertNoExceptionThrown("Precondition.", () => GetDocketWithAllocateMock(GetNewReader(dataObject, Logger)));
		}

		#endregion

		#region Test_CustomSource_ContainerDropMode

		public void Test_CustomSource_ContainerDropMode()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);
			Data.Orgs.CRAHOLSYD.BuyerLinks.AddNew(Data.Orgs.CRAHOLSYD);
			Data.Orgs.CRAHOLSYD.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			Factory.SaveForTesting();

			var shipment = Data.ShipmentDataObject;
			var container = new UniversalDataBuss.DataObjects.Universal.Container();
			container.ContainerNumber = "NC";
			container.Link = 1;
			shipment.SetContainerCollection(() => new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>()
			{
				container
			});
			var order = GetDocketWithAllocateMock(GetNewReader(shipment, Logger));
			AssertEquals("Ensure Import Succeeded.", true, order.IsFinalised);
		}

		#endregion

		#region Test_CustomsSource_TransportModeIsIgnored

		public void Test_CustomsSource_TransportModeIsIgnored()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Data.ShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };
			var reader = GetNewReader(Data.ShipmentDataObject, Logger);
			var order = GetDocketWithAllocateMock(reader);
			AssertEquals("Transport mode should not have been set.", "", order.WD_TransportMode);
		}

		#endregion

		#region Test_CustomsSource_ImportIntoVirtualWhsFinalisesDocket_RejectsImportIfUnableToFinalise

		protected override void Test_CustomsSource_ImportIntoVirtualWhsFinalisesDocket_RejectsImportIfUnableToFinaliseCore()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);
			Factory.SaveForTesting();

			// test that if order cannot finalise, should reject import
			AssertExceptionThrown("Docket cannot be finalised thus the import should be rejected.", typeof(DataObjectReadFailureException),
			@"
Cannot Import Order
Order could not be finalized into the Warehouse for Customs Job B123 because of the following error(s):
Error - Warehouse Order: Test Error".Trim(), () => GetDocketWithAllocateMock(new TestWhsOrderDataObjectReader(Data.ShipmentDataObject, Logger, Factory)));
		}

		#region class TestWhsOrderDataObjectReader

		class TestWhsOrderDataObjectReader : WhsOrderDataObjectReader
		{
			public TestWhsOrderDataObjectReader(UniversalShipment orderDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
				: base(orderDataObject, logger, factory)
			{
			}

			protected override WhsOrder GetNewBusinessObject()
			{
				var result = base.GetNewBusinessObject();

				// hack to fail finalisation.
				result.WD_FinalisedDateInfo.ValueChanged += (o, e) =>
				{
					if (result.WD_FinalisedDate.IsValid)
					{
						result.WD_FinalisedDate = ZDateTimeOffset.Empty;
						result.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
						result.AddRowError("Test Error");
					}
				};

				return result;
			}
		}

		#endregion

		#endregion

		#region TestCustomsAmendingFinalised

		const string ExpectedAmendingImportErrorMessage = "Cannot do amendment - Import changed data other than providing Outwards Entry Numbers or Import attempted to mark a line that was already in progress of being Customs Cleared.";
		const string ExpectedAmendingImportSuccessMessage = "Amendments allowed - No immediate changes made, but relevant order lines set for Customs Clearing.";
		const string ExpectedAmendingImportCorrectChangeMessage = "Amendments allowed - Outwards Entry Number set on relevant order lines and Customs Clearing Status removed.";
		const string ExpectedAmendingImportClearingMessage = "Amendments cleared - Relevant order lines' Customs Clearing status removed.";
		const string ExpectedCannotImportMessage = "Warehouse Order could not be amended for Customs Job B123 because it is already finalized, Customs cleared or had changes after finalization that are not allowed.";

		public void TestCustomsAmendingFinalised_MustBeOrderInDB()
		{
			WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB();
			Factory.SaveForTesting();

			var order = Helper.CreateWhsOrderWithOrderLine(Data.Orgs.CRAHOLSYD, warehouse, "B123", Data.Product, 5m);
			order.WD_RequiredDate = new ZDateTimeOffset(2013, 3, 19);
			order.WD_CustomsParentReference = "B123-EDIDATEDI";
			order.WD_DocketSubType = "CUS";
			Assert("Precondition: Order should not be in DB", !order.IsInDatabase);
			Assert("Precondition: Should be no orders in DB", new BusinessObjectFactory().Load<WhsDocket>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order)).Length == 0);

			var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWR, ServiceCode = ServiceCodeType.HLD } } };
			Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);
			AssertEquals("Precondition - hold service code is set for recipient role ", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First().ServiceCode);
			Assert("Should be no orders in DB", new BusinessObjectFactory().Load<WhsDocket>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order)).Length == 0);

			var message = GetQueuedUniversalShipmentMessage(Factory, Data.ShipmentDataObject, false);
			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			IEnumerable<IImportResult> importResults;
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				importResults = manager.Process(message).ImportResults;
			}
			var importResult = importResults.Single();
			Assert("Changing irrelevant properties should NOT fail the import.", importResult.WasSuccessful);

			var docketInOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_ExternalReference, "B123")).Single();
			AssertEquals("Lines correct count", 1, docketInOtherFactory.Lines.Count);
			Assert("Unsaved orders ignored CustomsClearingInProgress false.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().Any(l => !l.CustomsClearingInProgress));
		}

		public void TestCustomsAmendingFinalised_MustBeWhsReal()
		{
			WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			var order = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 5m, "EntryNumber123", 2, "", 0, finalise: true);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Precondition - DockOrderet should be in virtual Whs.", true, order.Warehouse.WW_IsVirtualWarehouse);

			var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWR, ServiceCode = ServiceCodeType.HLD } } };
			Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);
			AssertEquals("Precondition - hold service code is set for recipient role ", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First().ServiceCode);

			var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
			Assert("Changing irrelevant properties should NOT fail the import.", importResult.WasSuccessful);

			var docketInOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			AssertEquals("Lines correct count", 1, docketInOtherFactory.Lines.Count);
			Assert("Virtual Whs orders ignored CustomsClearingInProgress false.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => !l.CustomsClearingInProgress));
		}

		public void TestCustomsAmendingFinalised_NotCustomsCleared_MultipleLines()
		{
			WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var order = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 5m, "EntryNumber123", 2, "EntryNumber456", 102);
			SetUpOrderToMatchCustomsImportInformation(order);
			var orderline1 = order.Lines.Single();

			var orderline2 = Helper.CreateWhsOrderLine(order, Data.Product, 15m);
			orderline2.WE_BondedEntryKey = "ENTRYNUMBER847-5";
			FinaliseDocket(order);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(order);

			var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWR, ServiceCode = ServiceCodeType.HLD } } };
			Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);
			AssertEquals("Precondition - hold service code is set for recipient role", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.Single(r => r.Code == RecipientRoleType.BWR).ServiceCode);

			var commercialLine2 = new CommercialInvoiceLine()
			{
				AddInfoCollection = new List<AddInfo>
				{
					new AddInfo { Key = "TILV4Warehouse", Value = "777" },
					new AddInfo { Key = "WRL", Value = "5" },
					new AddInfo { Key = "WRN", Value = "ENTRYNUMBER847" },
				},
				PartNo = Data.Product.OP_PartNum,
				BondedWarehouseQuantity = 15m,
				EntryNumber = "EntryNumber687",
				EntryLineNumber = 105,
				LineNo = 4,
				BondedWHSOrderLineNumber = 4,
			};

			var shipmentDataObject = Data.ShipmentDataObject;
			shipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection.Add(commercialLine2);

			var stub = new Mock<IWhsOrderCustomsAmendmentChecker>();
			stub.Setup(s => s.CanDoAnAmendment(It.IsAny<WhsOrder>(), It.IsAny<IEnumerable<ZGuid>>(), It.IsAny<bool>())).Returns(true); // Order is valid for customs check.
			using (ObjectFactory.Substitute(nameof(IWhsOrderCustomsAmendmentChecker), stub.Object))
			{
				var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
				Assert("Changing irrelevant properties should NOT fail the import.", importResult.WasSuccessful);
				Assert("Should say orderline is now Customs Clearing in Progress.", importResult.Logs.Any(l => l.Type == LogType.Information && l.Message == ExpectedAmendingImportSuccessMessage));

				var docketInOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
				AssertEquals("Lines correct count", 2, docketInOtherFactory.Lines.Count);
				Assert("Orderline2 should exist and now be CustomsClearingInProgress.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().Any(l => l.CustomsClearingInProgress && l.PK == orderline2.PK));
				Assert("Orderline1 should exist and should NOT be CustomsClearingInProgress.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().Any(l => !l.CustomsClearingInProgress && l.PK == orderline1.PK));
			}
		}

		public void TestCustomsAmendingFinalised_PreventChangesIfOutwardEntryNumAlreadyExists()
		{
			WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var order = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 5m, "EntryNumber123", 2, "EntryNumber456", 102, finalise: true);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(order);

			var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWR, ServiceCode = ServiceCodeType.HLD } } };
			Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);
			AssertEquals("Precondition - hold service code is set for recipient role ", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First().ServiceCode);

			var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);

			var docketInOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			Assert("Lines are still there", docketInOtherFactory.Lines.Count > 0);
			Assert("Orders with Outwards Entry Numbers ignored CustomsClearingInProgress should be false.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => !l.CustomsClearingInProgress));
		}

		public void TestCustomsAmendingFinalised_PickMustBeFinalised()
		{
			WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var order = Helper.CreateWhsOrderWithOrderLine(Data.Orgs.CRAHOLSYD, warehouse, "B123", Data.Product, 5m);
			order.WD_RequiredDate = new ZDateTimeOffset(2013, 3, 19);
			order.WD_CustomsParentReference = "B123-EDIDATEDI";
			order.WD_DocketSubType = "CUS";
			Factory.SaveForTesting();

			var pick = Helper.CreatePickNew();
			pick.AddOrders(new[] { order });
			Factory.SaveForTesting();
			AssertEquals("Precondition - Pick should NOT be Finalised.", false, order.Pick.IsFinalised);

			var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWR, ServiceCode = ServiceCodeType.HLD } } };
			Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);
			AssertEquals("Precondition - hold service code is set for recipient role ", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First().ServiceCode);
			AssertEquals("Precondition - Pick should NOT be Finalised.", false, pick.IsFinalised);

			var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
			Assert("Changing irrelevant properties should NOT fail the import.", importResult.WasSuccessful);

			var docketInOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			Assert("Lines are still there", docketInOtherFactory.Lines.Count > 0);
			Assert("Orders from unfinalised Picks should be ignored therefore CustomsClearingInProgress false.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => !l.CustomsClearingInProgress));
		}

		public void TestCustomsAmendingFinalised_ImportServiceCodeHold_NoLineChanges_NoStub()
		{
			WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var order = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 5m, "EntryNumber123", 2, "", 0, finalise: true);
			SetUpOrderToMatchCustomsImportInformation(order);

			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(order);

			var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWR, ServiceCode = ServiceCodeType.HLD } } };
			Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);
			AssertEquals("Precondition - hold service code is set for recipient role ", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First().ServiceCode);

			var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
			Assert("Changing irrelevant properties should NOT fail the import.", importResult.WasSuccessful);
			Assert("Should say orderline is now Customs Clearing in Progress.", importResult.Logs.Any(l => l.Type == LogType.Information && l.Message == ExpectedAmendingImportSuccessMessage));

			var docketInOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			Assert("Lines are still there", docketInOtherFactory.Lines.Count > 0);
			Assert("No changes to line so CustomsClearingInProgress true.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => l.CustomsClearingInProgress));
		}

		public void TestCustomsAmendingFinalised_ImportServiceCodeHold_NoLineChanges_CustomsClearingInProgress_NoStub()
		{
			WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var order = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 5m, "EntryNumber123", 2, "", 0, finalise: true);
			SetUpOrderToMatchCustomsImportInformation(order);
			order.Lines[0].CustomsClearingInProgress = ZBool.True;
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Orderline should be CustomsClearingInProgress.", ZBool.True, order.Lines[0].CustomsClearingInProgress);

			var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWR, ServiceCode = ServiceCodeType.HLD } } };
			Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);
			AssertEquals("Precondition - hold service code is set for recipient role ", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First().ServiceCode);

			var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).First();
			AssertEquals("Orderline being CustomsClearingInProgress true should fail the import.", false, importResult.WasSuccessful);
			Assert("No changes so import allowed as CustomsClearingInProgress set.", importResult.Logs.Any(l => l.Type == LogType.Error && l.Message == ExpectedAmendingImportErrorMessage));

			var docketInOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			AssertEquals("One orderLine are still there", 1, docketInOtherFactory.Lines.Count);
			Assert("No changes to line so CustomsClearingInProgress true.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => l.CustomsClearingInProgress));
		}

		public void TestCustomsAmendingFinalised_ImportServiceCodeHold_NewOutwardsClearingNumber_NotCustomsClearing()
		{
			WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var order = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 5m, "EntryNumber123", 2, "", 0, finalise: true);
			SetUpOrderToMatchCustomsImportInformation(order);
			order.Lines[0].CustomsClearingInProgress = ZBool.False;
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Orderline should be CustomsClearingInProgress.", ZBool.False, order.Lines[0].CustomsClearingInProgress);

			var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWR, ServiceCode = ServiceCodeType.HLD } } };
			Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);
			AssertEquals("Precondition - hold service code is set for recipient role ", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First().ServiceCode);

			var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
			Assert("Import should be successful.", importResult.WasSuccessful);
			Assert("Should say orderline is now Customs Clearing in Progress.", importResult.Logs.Any(l => l.Type == LogType.Information && l.Message == ExpectedAmendingImportSuccessMessage));

			var docketInOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			Assert("Lines are still there", docketInOtherFactory.Lines.Count > 0);

			var orderLine = docketInOtherFactory.Lines.Cast<WhsOrderLine>().Single();
			Assert("CustomsClearingInProgress should be set to true.", orderLine.CustomsClearingInProgress);
			Assert("Outwards Entry Number expected to still be empty.", orderLine.CustomsData.WB_EntryKey.IsEmpty);
			Assert("Outwards Entry Line No expected to be 0.", orderLine.CustomsData.WB_EntryLineNo == 0);
		}

		public void TestCustomsAmendingFinalised_ImportServiceCodeHold_LineChanged_OtherChange()
		{
			WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var order = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 5m, "EntryNumber123", 2, "", 0);
			SetUpOrderToMatchCustomsImportInformation(order);
			FinaliseDocket(order);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Orderline should be CustomsClearingInProgress.", ZBool.False, order.Lines[0].CustomsClearingInProgress);

			var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWR, ServiceCode = ServiceCodeType.HLD } } };
			Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);
			AssertEquals("Precondition - hold service code is set for recipient role ", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First().ServiceCode);

			// Other change
			Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity += 2; // increasing received Qty by 2

			var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).First();
			AssertEquals("Changing irrelevant properties should fail the import.", false, importResult.WasSuccessful);
			Assert("Should should correct error", importResult.Logs.Any(l => l.Type == LogType.Error && l.Message == ExpectedAmendingImportErrorMessage));

			var docketInOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			Assert("No changes should of added any lines", docketInOtherFactory.Lines.Count == 1);
			Assert("Changes to line so CustomsClearingInProgress false.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => !l.CustomsClearingInProgress));
			Assert("No changes to line so WB_EntryKey should be empty.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => l.CustomsData.WB_EntryKey.IsEmpty));
			Assert("No changes to line so WB_EntryLineNo 0.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => l.CustomsData.WB_EntryLineNo == ZShort.Zero));
		}

		public void TestCustomsAmendingFinalised_ServCodeOther_AddingOutwardsEntryNum()
		{
			WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var order = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 5m, "EntryNumber123", 2, "", 0);
			SetUpOrderToMatchCustomsImportInformation(order);
			order.Lines[0].CustomsClearingInProgress = ZBool.True;
			FinaliseDocket(order);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Orderline should be CustomsClearingInProgress.", ZBool.True, order.Lines[0].CustomsClearingInProgress);

			var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWR, ServiceCode = ServiceCodeType.AMD } } };
			Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);
			AssertEquals("Precondition - Amend service code is set for recipient role ", ServiceCodeType.AMD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First().ServiceCode);

			var importResultini = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
			var importResult = importResultini.Single();
			AssertEquals("Adding Outwards Entry Number properties should NOT fail the import.", true, importResult.WasSuccessful);
			Assert("Should show message that orderline is updated and Customs Clearing status removed.", importResult.Logs.Any(l => l.Type == LogType.Information && l.Message == ExpectedAmendingImportCorrectChangeMessage));

			var docketInOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			Assert("Lines are still there and still only 1.", docketInOtherFactory.Lines.Count == 1);
			Assert("Changes to line so CustomsClearingInProgress removed.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => !l.CustomsClearingInProgress));
			Assert("Changes to line so WB_EntryKey updated.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => l.CustomsData.WB_EntryKey == "DummyOutward-1"));
			Assert("Changes to line so WB_EntryLineNo updated.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => l.CustomsData.WB_EntryLineNo == 102));
		}

		public void TestCustomsAmendingFinalised_ServCodeOther_LineAlreadyHasOutwardsEntryNum_ClearingNotInProgress()
		{
			WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var order = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 5m, "EntryNumber123", 2, "DummyOutwardHere-1", 57);
			SetUpOrderToMatchCustomsImportInformation(order);
			FinaliseDocket(order);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Orderline should be CustomsClearingInProgress.", ZBool.False, order.Lines[0].CustomsClearingInProgress);

			var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWR, ServiceCode = ServiceCodeType.AMD } } };
			Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);
			AssertEquals("Precondition - Amend service code is set for recipient role ", ServiceCodeType.AMD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First().ServiceCode);

			var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).First(); // Imports new Outwards Entry Number of "DummyOutward-1"
			AssertEquals("Overwriting Outwards Entry Number should fail the import.", false, importResult.WasSuccessful);
			Assert("Should correct error.", importResult.Logs.Any(l => l.Type == LogType.Error && l.Message.Contains(ExpectedCannotImportMessage)));

			var docketInOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			Assert("Lines are still there and still only 1.", docketInOtherFactory.Lines.Count == 1);
			Assert("Changes to line so CustomsClearingInProgress removed.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => !l.CustomsClearingInProgress));
			Assert("Changes to line so WB_EntryKey updated.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => l.CustomsData.WB_EntryKey == "DummyOutwardHere-1"));
			Assert("Changes to line so WB_EntryLineNo updated.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => l.CustomsData.WB_EntryLineNo == 57));
		}

		public void TestCustomsAmendingFinalised_ServCodeOther_NoOutwardsNumber()
		{
			WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var order = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 5m, "EntryNumber123", 2, "", 0);
			SetUpOrderToMatchCustomsImportInformation(order);
			order.Lines[0].CustomsClearingInProgress = ZBool.True;
			FinaliseDocket(order);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Orderline should be CustomsClearingInProgress.", ZBool.True, order.Lines[0].CustomsClearingInProgress);

			var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWR, ServiceCode = ServiceCodeType.BRQ } } };
			Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);
			AssertEquals("Precondition - BRQ service code is set for recipient role ", ServiceCodeType.BRQ, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First().ServiceCode);

			// Set up shipment
			var shipmentDataObject = Data.ShipmentDataObject;
			var commercialLine = shipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
			commercialLine.EntryNumber = null; // clear Outwards Entry Number
			commercialLine.EntryLineNumber = null; // clear Outwards Entry Line Number

			var importResult = GetImportResultsViaDataContextManager(shipmentDataObject).Single();
			AssertEquals("Should pass as customs is clearing.", true, importResult.WasSuccessful);
			Assert("Message should say orderline is no longer Customs Clearing in Progress.", importResult.Logs.Any(l => l.Type == LogType.Information && l.Message == ExpectedAmendingImportClearingMessage));

			var docketInOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			Assert("Lines are still there and still only 1.", docketInOtherFactory.Lines.Count == 1);
			Assert("No changes to line so CustomsClearingInProgress is now false.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => !l.CustomsClearingInProgress));
			Assert("Clearing cancelled so WB_EntryKey unchanged.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => l.CustomsData.WB_EntryKey == string.Empty));
			Assert("Clearing cancelled so WB_EntryLineNo unchanged.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => l.CustomsData.WB_EntryLineNo == ZShort.Zero));
		}

		public void TestCustomsAmendingFinalised_ServCodeOther_OtherChange()
		{
			WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var order = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 5m, "EntryNumber123", 2, "", 0);
			SetUpOrderToMatchCustomsImportInformation(order);
			order.Lines[0].CustomsClearingInProgress = ZBool.True;
			FinaliseDocket(order);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Orderline should be CustomsClearingInProgress.", ZBool.True, order.Lines[0].CustomsClearingInProgress);

			var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWR, ServiceCode = ServiceCodeType.BRQ } } };
			Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);
			AssertEquals("Precondition - BRQ service code is set for recipient role ", ServiceCodeType.BRQ, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First().ServiceCode);

			// Other change
			var shipmentDataObject = Data.ShipmentDataObject;
			shipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity += 2; // increasing received Qty by 2

			var importResult = GetImportResultsViaDataContextManager(shipmentDataObject).First();
			AssertEquals("Changing irrelevant properties should fail the import.", false, importResult.WasSuccessful);
			Assert("Should show correct error message.", importResult.Logs.Any(l => l.Type == LogType.Error && l.Message == ExpectedAmendingImportErrorMessage));

			var docketInOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			Assert("Lines are still there and still only 1.", docketInOtherFactory.Lines.Count == 1);
			Assert("No changes to line so CustomsClearingInProgress is still true.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => l.CustomsClearingInProgress));
			Assert("Clearing cancelled so WB_EntryKey unchanged.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => l.CustomsData.WB_EntryKey == string.Empty));
			Assert("Clearing cancelled so WB_EntryLineNo unchanged.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => l.CustomsData.WB_EntryLineNo == ZShort.Zero));
		}

		public void TestCustomsAmendingFinalised_IsNotCustomCleared_RegistryAllowed()
		{
			WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var order = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 5m, "EntryNumber123", 2, "DummyOutward-1", 102);
			SetUpOrderToMatchCustomsImportInformation(order);
			order.Lines[0].CustomsClearingInProgress = ZBool.False;
			FinaliseDocket(order);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Orderline should be CustomsClearingInProgress.", ZBool.False, order.Lines[0].CustomsClearingInProgress);

			var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWR, ServiceCode = ServiceCodeType.AMD } } };
			Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);
			AssertEquals("Precondition - Amend service code is set for recipient role ", ServiceCodeType.AMD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First().ServiceCode);

			var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
			AssertEquals("Adding Outwards Entry Number properties should NOT fail the import.", false, importResult.WasSuccessful);
			Assert("Should show error message.", importResult.Logs.Any(l => l.Type == LogType.Error && l.Message.Contains(ExpectedCannotImportMessage)));

			var docketInOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			Assert("Lines are still there and still only 1.", docketInOtherFactory.Lines.Count == 1);
			Assert("Import rejected no change to CustomsClearingInProgress.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => !l.CustomsClearingInProgress));
			Assert("Import rejected no change to WB_EntryKey.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => l.CustomsData.WB_EntryKey == "DummyOutward-1"));
			Assert("Import rejected no change to WB_EntryLineNo.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => l.CustomsData.WB_EntryLineNo == 102));
		}

		#endregion

		#region TestCustomsAmendingUnFinalisedOrder_CriticalFields

		public void TestCustomsAmendingUnFinalisedOrder_CriticalFields_Product()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			var client = Data.Orgs.CRAHOLSYD;
			var product1 = new OrgSupplierPart.Loader(Factory.BOFactory).Load("P1", client, null);
			var product2 = Helper.CreateProduct(client, "P1");
			Helper.SetProductAllAttributeUse(client, product2, true);
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var receive = Helper.CreateWhsReceive(client, warehouse, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive, product2, 10m, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			product2.OP_IsActive = false;
			Factory.SaveForTesting();

			Assert("Should be able to save.", GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single().WasSuccessful);

			product1.OP_IsActive = false;
			product2.OP_IsActive = true;
			Factory.SaveForTesting();
			Assert("Should be able to update product.", GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single().WasSuccessful);
		}

		public void TestCustomsAmendingUnFinalisedOrder_CriticalFields_PartAttrib1()
		{
			TestCustomsAmendingUnFinalisedOrder_CriticalFields_Core(AttributeNumber.One, "Blue");
		}

		public void TestCustomsAmendingUnFinalisedOrder_CriticalFields_PartAttrib2()
		{
			TestCustomsAmendingUnFinalisedOrder_CriticalFields_Core(AttributeNumber.Two, "Large");
		}

		public void TestCustomsAmendingUnFinalisedOrder_CriticalFields_PartAttrib3()
		{
			TestCustomsAmendingUnFinalisedOrder_CriticalFields_Core(AttributeNumber.Three, "S5678");
		}

		public void TestCustomsAmendingUnFinalisedOrder_CriticalFields_SerialNumber()
		{
			TestCustomsAmendingUnFinalisedOrder_CriticalFields_Core(AttributeNumber.Serial, "SN2");
		}

		public void TestCustomsAmendingUnFinalisedOrder_CriticalFields_Core(AttributeNumber attributeNumber, ZString newValue)
		{
			var attributeKey = "";
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isSerialNumberTest: true);
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, warehouse, "R2");

			Helper.SetClientAttributeType(Data.Orgs.CRAHOLSYD, AttributeNumber.Serial, true);
			var invLine = Helper.CreateWhsReceiveInventoryLine(receive, Data.Product, 1m, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			invLine.WI_SerialNumber = "SNN";
			switch (attributeNumber)
			{
				case AttributeNumber.One:
					attributeKey = "Colour";
					invLine.WI_PartAttrib1 = newValue;
					break;
				case AttributeNumber.Two:
					attributeKey = "Size";
					invLine.WI_PartAttrib2 = newValue;
					break;
				case AttributeNumber.Three:
					attributeKey = "Serial";
					invLine.WI_PartAttrib3 = newValue;
					break;
				case AttributeNumber.Serial:
					attributeKey = "Serial Number";
					invLine.WI_SerialNumber = newValue;
					break;
				default:
					break;
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			var order = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 1m, "EntryNumber123", 2, "DummyOutward-1", 102, finalise: false);
			Factory.SaveForTesting();
			Helper.CreatePickByAttachingOrders(order);
			AssertEquals("Precondition", true, order.WD_WP.IsValid);
			Assert("Changing irrelevant properties should NOT fail the import.", GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single().WasSuccessful);
			Factory.SaveForTesting();

			changeValueInUniversalShipment();
			Assert("Changing irrelevant properties should NOT fail the import.", GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single().WasSuccessful);

			void changeValueInUniversalShipment()
			{
				var commercialInvoiceLine = Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CustomizedFieldCollection;
				commercialInvoiceLine.Remove(commercialInvoiceLine.Single(f => f.Key.Value == attributeKey));
				commercialInvoiceLine.Add(CustomizedField.New(attributeKey, newValue));
			}
		}

		#endregion

		#region TestErrorReporting

		public void TestErrorReporting_WhenPickIsNotFinalisedAfterFinaliseDocket()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);
			Factory.SaveForTesting();
			var existingPickInDB = NewFactory().GetDatabaseCount(typeof(WhsOrder));

			var reader = new TestWhsOrderDataObjectReaderWithPickFailedToFinalise(Data.ShipmentDataObject, Logger, Factory);
			AssertExceptionThrown("Cannot import OrderLine without valid Product Code.", typeof(DataObjectReadFailureException),
	@"Cannot finalize pick
Order External Reference: B123 Failed to Finalize Pick.
Order Line Product: P1
Error - Docket Line: Something is wrong, that's the reason why pick is not finalised.", () => GetDocketWithAllocateMock(reader));

			AssertEquals("Should not create new pick.", existingPickInDB, NewFactory().GetDatabaseCount(typeof(WhsOrder)));
		}

		#region class TestWhsOrderDataObjectReaderWithPickFailedToFinalise

		class TestWhsOrderDataObjectReaderWithPickFailedToFinalise : WhsOrderDataObjectReader
		{
			internal TestWhsOrderDataObjectReaderWithPickFailedToFinalise(UniversalShipment whsOrderDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
				: base(whsOrderDataObject, logger, factory)
			{
			}

			protected override WhsOrder GetNewBusinessObject()
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

		#endregion

		#region Test_CustomsSource_ImportIntoRealWhs / Test_CustomsSource_ImportIntoVirtualWhsFinalisesDocket

		public void Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCancelsOutPreviousJob()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);

			var docket = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 10m, "EntryNumber123", 2, "DummyOutward-1", 102, finalise: true);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(docket);

			// amend qty from 10 to 7
			Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 7;
			AssertNewDocketAndCancellingOutOfPreviousDocketFromCustomsAmendment(Data.ShipmentDataObject, Data.Product, 10, 7, 1);

			var docketInOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(docket.PK);
			AssertEquals("Dockets that are amended should be cancelled.", true, docketInOtherFactory.IsCancelled);
			AssertDocketWasCancelledOut(docketInOtherFactory, 7);

			// amend qty from 7 to 9 (test a second customs amendment)
			Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 9;
			AssertNewDocketAndCancellingOutOfPreviousDocketFromCustomsAmendment(Data.ShipmentDataObject, Data.Product, 7, 9, 2);
		}

		protected override void Assert_CustomsSource_ImportFinalisesDocketIfVirtualWhsCore(WhsOrder order, bool useVirtualWhs, bool finaliseAllowed)
		{
			// ensure order is FINALISED for a virtual whs, and HELD for a real whs.
			var expectedStatus = useVirtualWhs ? WhsOrderStatus.Codes.Departed : DocketStatus.Codes.AttachedToPick;
			AssertEquals("Jobs created in a virtual Whs should be PICKING (with Finalised Date), jobs created in a real warehouse should be PICKING.", expectedStatus, order.WD_DocketStatus);
			AssertEquals("Jobs created in a virtual Whs should be PICKING (with Finalised Date), jobs created in a real warehouse should be HELD For Customs, unless Customs relaxed validation.", useVirtualWhs || finaliseAllowed, order.IsFinaliseAllowed);
			AssertEquals("Jobs created in a virtual Whs should be PICKING (with Finalised Date), jobs created in a real warehouse should be HELD For Customs.", useVirtualWhs, order.IsFinalised);
			AssertEquals("Jobs from customs should populate Consignee from Importer", Data.Orgs.CRAHOLSYD, order.Consignee);

			// ensure Pick is FINALISED for a virtual whs or is Customs link method returns true.
			if (useVirtualWhs)
			{
				AssertEquals("Jobs created in a virtual Whs should fallback Required by Date to today if not provided.", new ZDateTimeOffset(2013, 1, 1, 7, 7, 30), order.WD_RequiredDate);
				AssertEquals("Jobs created in a virtual Whs should have a FINALISED Pick.", true, order.Pick.IsFinalised);
			}
			// ensure Pick is CREATED (reserving stock) for a real whs without Customs allowance.
			else
			{
				AssertEquals("Jobs created in a real Whs should be PICKED but not finalised.", false, order.Pick.IsFinalised);
			}

			// ensure orderlines are correct
			var line = (WhsOrderLine)order.Lines.Single();
			CombineAssertions(() =>
			{
				AssertEquals(Data.Product.PK, line.WE_OP);
				AssertEquals(Data.CustomsQtyOnOrder, line.WE_TransactionQuantity);

				var pickLine = line.PickLines.Single();
				AssertEquals(Data.CustomsQtyOnOrder, pickLine.WZ_Units);
			});
		}

		public void TestCustomsSource_ImportFinalisesDocketIfVirtualWhs_DPSFreightMovementRestrictions()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true, setClearDpsScreeningStatus: true);
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL"))
			using (WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				var reader = GetNewReader(ShipmentDataObject, Logger);
				var order = reader.ReadIntoBusinessObject();
				AssertEquals("Precondition", false, Logger.HasErrors);
				AssertNotNull("order", order);
				AssertEquals("CUS", order.WD_DocketSubType);
				AssertEquals("B123", order.WD_ExternalReference);
				AssertEquals("B123-EDIDATEDI", order.WD_CustomsParentReference);
				AssertEquals(true, order.IsFinalised);
			}
		}

		public void TestCustomsSource_ImportFinalisesDocketIfVirtualWhs_DPSFreightMovementRestrictions_FailsIfScreeningStatusIsNotClear()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL"))
			using (WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				var reader = GetNewReader(ShipmentDataObject, Logger);
				AssertExceptionThrown("Cannot import Order when DPS screening is matched.", typeof(DataObjectReadFailureException),
				@"Cannot Import Order
Order could not be finalized into the Warehouse for Customs Job B123 because of the following error(s):
Error: Finalise
Error - Warehouse Order: Cannot finalize when Denied Party Screening is matched.", () => reader.ReadIntoBusinessObject());
			}
		}

		#endregion

		#region Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCancelsOutPreviousJob_WhenCusDecIsSubShipment

		public void Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCancelsOutPreviousJob_WhenCusDecIsSubShipment()
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

			var docket = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, Data.GetOrCreateWarehouseInDB(true), "B123", "B123-EDIDATEDI", Data.Product, 10m, "EntryNumber123", 2, "DummyOutward-1", 102, finalise: true);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(docket);

			// process the order
			var message = GetQueuedUniversalShipmentMessage(topLevelShipment);
			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			IXmlSessionTracker tracker;
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				tracker = manager.Process(message);
			}

			// should create a new docket for the amendment
			var importResult = tracker.ImportResults.Single();
			var amendedDocket = (WhsOrder)importResult.GetBizOForTesting(topLevelShipment, Factory.BOFactory);
			AssertEquals(true, amendedDocket.IsFinalised);
			AssertNotEquals(docket.PK, amendedDocket.PK);

			// should create a new adjustment to ajust out the original docket
			AssertDocketWasCancelledOut(new BusinessObjectFactory().Load<WhsOrder>(docket.PK), 5m);
		}

		#endregion

		#region Test_CustomsSource_RejectsImportIfShortfall

		public void Test_CustomsSource_RejectsImportIfShortfall()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// try to order 107 + 109, only 100 are in stock
			var dataObject = Data.ShipmentDataObject;
			dataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 107.00m;
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(109.000m, 7, Data.Product); // add a second 'order line'

			// add a non-shortfall item (product2)
			var product2 = Data.CreateProduct("P2");
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(100.00m, 8, product2); // add a third 'order line'

			// receive the 2nd product
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, warehouse, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive, product2, 100.0m, "EntryNumber123-" + Data.LastUsedEntryLineNo);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(receive);

			// lines are in shortfall, should reject import (only 100 units for line 3, and 0 units for line 7 (bonded entry key is xxx-3, not xxx-7)
			AssertExceptionThrown("Docket has shortfalls thus the import should be rejected.", typeof(DataObjectReadFailureException),
			string.Format(@"
Cannot Import {0}
{0} could not be created for Customs Job B123 because there are errors:
You do not have enough stock to fulfill shortfalls on this order
ENTRYNUMBER123-2 Product P1/P1 can not be ordered due to lack of stock. 107 was ordered, but 100 is available
ENTRYNUMBER123-3 Product P1/P1 can not be ordered due to lack of stock. 109 was ordered, but 0 is available".Trim(), GetDocketType()),
			() => GetDocketWithAllocateMock(GetNewReader(dataObject, Logger)));
		}

		#endregion

		#region Test_CustomsSource_RejectsImportIfShortfall_USBonded

		#region Test_CustomsSource_RejectsImportIfShortfall_PerPackageQty

		public void Test_CustomsSource_RejectsImportIfShortfall_PerPackageQty()
		{
			SetupWarehouseForUS_Customs();

			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create 3 products each with a per unit qty of 100
			// product 1 and product 2 order below the per unit qty, product 3 has no shortfalls
			var product1 = Data.CreateProduct("G1");
			var product2 = Data.CreateProduct("G2");
			var product3 = Data.CreateProduct("G3");

			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);

			// Receive stock
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, warehouse, "R2");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, product1, 100.0m, "EntryNumber123-3", "", 100m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, product2, 100.0m, "EntryNumber123-4", "", 100m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, product3, 200.0m, "EntryNumber123-5", "", 100m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			// Order stock
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(40.00m, 3, product1).CustomizedFieldCollection.Clear();
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(60.00m, 4, product2).CustomizedFieldCollection.Clear();
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(100.00m, 5, product3).CustomizedFieldCollection.Clear();

			Factory.SaveForTesting();

			// test import
			var dataObject = Data.ShipmentDataObject;

			AssertExceptionThrown("Docket has shortfalls thus the import should be rejected.", typeof(DataObjectReadFailureException),
			string.Format(@"
Cannot Import {0}
{0} could not be created for Customs Job B123 because there are errors:
In order to fix shortfalls, you will need to modify lines as follows:
ENTRYNUMBER123-3 Product G1/G1 will need 60 to be added to the order
ENTRYNUMBER123-4 Product G2/G2 will need 40 to be added to the order
".Trim(), GetDocketType()),
			() => GetDocketWithAllocateMock(GetNewReader(dataObject, Logger)));
		}

		#endregion

		#region Test_CustomsSource_RejectsImportIfShortfall_PackageGroups_OrderMissingItemsFromGroup

		public void Test_CustomsSource_RejectsImportIfShortfall_PackageGroups_OrderMissingItemsFromGroup()
		{
			SetupWarehouseForUS_Customs();

			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create 7 products each with a per unit qty of 100
			var groupedProduct1 = Data.CreateProduct("G1");
			var groupedProduct2 = Data.CreateProduct("G2");
			var groupedProduct3 = Data.CreateProduct("G3");
			var groupedProduct4 = Data.CreateProduct("G4");
			var groupedProduct5 = Data.CreateProduct("G5");
			var groupedProduct6 = Data.CreateProduct("G6");
			var groupedProduct7 = Data.CreateProduct("G7");

			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);

			// Receive stock
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, warehouse, "R2");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct1, 800.00m, "EntryNumber123-3", "TestGroup1", 8m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct2, 800.00m, "EntryNumber123-4", "TestGroup1", 8m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct3, 800.00m, "EntryNumber123-5", "TestGroup1", 8m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct4, 800.00m, "EntryNumber123-6", "TestGroup1", 8m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct5, 800.00m, "EntryNumber123-7", "TestGroup1", 8m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct6, 800.00m, "EntryNumber123-8", "TestGroup1", 8m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct7, 800.00m, "EntryNumber123-9", "TestGroup1", 8m);

			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct1, 700.0m, "EntryNumber123-10", "TestGroup2", 25m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct2, 700.0m, "EntryNumber123-11", "TestGroup2", 25m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct3, 700.0m, "EntryNumber123-12", "TestGroup2", 25m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct4, 700.0m, "EntryNumber123-13", "TestGroup2", 25m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct5, 700.0m, "EntryNumber123-14", "TestGroup2", 25m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct6, 700.0m, "EntryNumber123-15", "TestGroup2", 25m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct7, 700.0m, "EntryNumber123-16", "TestGroup2", 25m);

			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct1, 200.0m, "EntryNumber123-17", "", 100m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			// Order stock
			// G1 - 700, G2 - 300
			// G3 - 400, G4 - 100
			// G5 - 500, G6 - 0
			// G7 - 0

			Data.AddCommercialInvoiceLineWithRelatedEntryLine(700m, 3, groupedProduct1);
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(50m, 4, groupedProduct2);
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(400m, 5, groupedProduct3);
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(100m, 6, groupedProduct4);
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(250m, 7, groupedProduct5);

			Factory.SaveForTesting();

			// test import
			// Expected to Order all units to match G1 - 700
			// G1 - 4, G2 - 654
			// G3 - 304, G4 - 604
			// G5 - 454, G6 - 704
			// G7 - 704
			var dataObject = Data.ShipmentDataObject;

			AssertExceptionThrown("Docket has shortfalls thus the import should be rejected.", typeof(DataObjectReadFailureException),
			string.Format(@"
Cannot Import {0}
{0} could not be created for Customs Job B123 because there are errors:
In order to fix shortfalls, you will need to modify lines as follows:
ENTRYNUMBER123-3 Product G1/G1 will need 4 to be added to the order
ENTRYNUMBER123-4 Product G2/G2 will need 654 to be added to the order
ENTRYNUMBER123-5 Product G3/G3 will need 304 to be added to the order
ENTRYNUMBER123-6 Product G4/G4 will need 604 to be added to the order
ENTRYNUMBER123-7 Product G5/G5 will need 454 to be added to the order
ENTRYNUMBER123-8 Product G6/G6 will need 704 to be added to the order
ENTRYNUMBER123-9 Product G7/G7 will need 704 to be added to the order".Trim(), GetDocketType()),
			() => GetDocketWithAllocateMock(GetNewReader(dataObject, Logger)));
		}

		#endregion

		#region Test_CustomsSource_RejectsImportIfShortfall_PackageGroups_NotEnoughStock

		public void Test_CustomsSource_RejectsImportIfShortfall_PackageGroups_NotEnoughStock()
		{
			SetupWarehouseForUS_Customs();

			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create 7 products each with a per unit qty of 100
			var groupedProduct1 = Data.CreateProduct("G1");
			var groupedProduct2 = Data.CreateProduct("G2");
			var groupedProduct3 = Data.CreateProduct("G3");
			var groupedProduct4 = Data.CreateProduct("G4");
			var groupedProduct5 = Data.CreateProduct("G5");
			var groupedProduct6 = Data.CreateProduct("G6");
			var groupedProduct7 = Data.CreateProduct("G7");

			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);

			// Receive stock
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, warehouse, "R2");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct1, 100.0m, "EntryNumber123-3", "TestGroup1", 50m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct2, 100.0m, "EntryNumber123-4", "TestGroup1", 50m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct3, 100.0m, "EntryNumber123-5", "TestGroup1", 50m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct4, 100.0m, "EntryNumber123-6", "TestGroup1", 50m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct5, 100.0m, "EntryNumber123-7", "TestGroup1", 50m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct6, 100.0m, "EntryNumber123-8", "TestGroup1", 50m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct7, 100.0m, "EntryNumber123-9", "TestGroup1", 50m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			// Order stock
			// G1 - 700, G2 - 300
			// G3 - 400, G4 - 100
			// G5 - 500, G6 - 0
			// G7 - 0

			Data.AddCommercialInvoiceLineWithRelatedEntryLine(700m, 3, groupedProduct1);
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(50m, 4, groupedProduct2);
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(400m, 5, groupedProduct3);
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(100m, 6, groupedProduct4);
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(250m, 7, groupedProduct5);

			Factory.SaveForTesting();

			// test import
			// Expected to Order all units to match G1 - 700
			// G1 - 0, G2 - 650
			// G3 - 300, G4 - 600
			// G5 - 200, G6 - 700
			// G7 - 700
			var dataObject = Data.ShipmentDataObject;

			AssertExceptionThrown("Docket has shortfalls thus the import should be rejected.", typeof(DataObjectReadFailureException),
			string.Format(@"
Cannot Import {0}
{0} could not be created for Customs Job B123 because there are errors:
You do not have enough stock to fulfill shortfalls on this order
ENTRYNUMBER123-3 Product G1/G1 can not be ordered due to lack of stock. 700 was ordered, but 100 is available
ENTRYNUMBER123-5 Product G3/G3 can not be ordered due to lack of stock. 400 was ordered, but 100 is available
ENTRYNUMBER123-7 Product G5/G5 can not be ordered due to lack of stock. 250 was ordered, but 100 is available
".Trim(), GetDocketType()),
			() => GetDocketWithAllocateMock(GetNewReader(dataObject, Logger)));
		}

		#endregion

		#region Test_CustomsSource_RejectsImportIfShortfall_PackageGroups_OrderMissingItemsFromGroup_MultiGroups

		public void Test_CustomsSource_RejectsImportIfShortfall_PackageGroups_OrderMissingItemsFromGroup_MultiGroups()
		{
			SetupWarehouseForUS_Customs();

			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create 7 products each with a per unit qty of 100
			var groupedProduct1 = Data.CreateProduct("ProductA");
			var groupedProduct2 = Data.CreateProduct("ProductB");
			var groupedProduct3 = Data.CreateProduct("ProductC");

			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);

			// Receive stock
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, warehouse, "R2");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct1, 500m, "EntryNumber123-3", "AB", 100m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct1, 500m, "EntryNumber123-3", "AC", 100m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct2, 400m, "EntryNumber123-4", "AB", 80m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct2, 400m, "EntryNumber123-4", "BC", 80m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct3, 500m, "EntryNumber123-5", "AC", 100m);
			Helper.CreateWhsReceiveInventoryLine(receive, groupedProduct3, 500m, "EntryNumber123-5", "BC", 100m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			// Order stock
			// A - 700, B - 50

			Data.AddCommercialInvoiceLineWithRelatedEntryLine(700m, 3, groupedProduct1);
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(50m, 4, groupedProduct2);

			Factory.SaveForTesting();

			// test import
			// Expected order adjustments
			// A - 0, B - 350, C - 200
			// AB x 5, AC x 2 - AB is more desirable than AC as it over orders less.
			var dataObject = Data.ShipmentDataObject;
			AssertExceptionThrown("Docket has shortfalls thus the import should be rejected.", typeof(DataObjectReadFailureException),
			string.Format(@"
Cannot Import {0}
{0} could not be created for Customs Job B123 because there are errors:
In order to fix shortfalls, you will need to modify lines as follows:
ENTRYNUMBER123-4 Product PRODUCTB/ProductB will need 350 to be added to the order
ENTRYNUMBER123-5 Product PRODUCTC/ProductC will need 200 to be added to the order
".Trim(), GetDocketType()),
			() => GetDocketWithAllocateMock(GetNewReader(dataObject, Logger)));
		}

		#endregion

		#region Test_CustomsSource_RejectsImportIfShortfall_ItemsNotInErrorDoNotHaveErrors

		public void Test_CustomsSource_RejectsImportIfShortfall_ItemsNotInErrorDoNotHaveErrors()
		{
			SetupWarehouseForUS_Customs();

			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create 2 products each with a per unit qty of 100
			// product 1 lacks stock, product 2 has no shortfalls
			var product1 = Data.CreateProduct("G1");
			var product2 = Data.CreateProduct("G2");
			var product3 = Data.CreateProduct("G3");

			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);

			// Receive stock
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, warehouse, "R2");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, product1, 100.0m, "EntryNumber123-3", "", 100m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, product2, 200.0m, "EntryNumber123-4", "", 100m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			// Order stock
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(200.00m, 3, product1).CustomizedFieldCollection.Clear();
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(200.00m, 4, product2).CustomizedFieldCollection.Clear();

			Factory.SaveForTesting();

			// test import
			var dataObject = Data.ShipmentDataObject;

			AssertExceptionThrown("Docket has shortfalls thus the import should be rejected.", typeof(DataObjectReadFailureException),
			string.Format(@"
Cannot Import {0}
{0} could not be created for Customs Job B123 because there are errors:
You do not have enough stock to fulfill shortfalls on this order
ENTRYNUMBER123-3 Product G1/G1 can not be ordered due to lack of stock. 200 was ordered, but 100 is available
".Trim(), GetDocketType()),
			() => GetDocketWithAllocateMock(GetNewReader(dataObject, Logger)));
		}

		#endregion

		#region Test_CustomsSource_RejectsImportIfShortfall_ItemsNotDivisibleByPerPackQty

		public void Test_CustomsSource_RejectsImportIfShortfall_ItemsNotDivisibleByPerPackQty()
		{
			SetupWarehouseForUS_Customs();

			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create 2 products each with a per unit qty of 100
			// product 1 lacks stock, product 2 has no shortfalls
			var product1 = Data.CreateProduct("G1");
			var product2 = Data.CreateProduct("G2");
			var product3 = Data.CreateProduct("G3");

			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);

			// Receive stock
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, warehouse, "R2");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, product1, 200.0m, "EntryNumber123-3", "", 100m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, product2, 200.0m, "EntryNumber123-4", "", 100m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			// Order stock
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(150.00m, 3, product1).CustomizedFieldCollection.Clear();
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(200.00m, 4, product2).CustomizedFieldCollection.Clear();

			Factory.SaveForTesting();

			// test import
			var dataObject = Data.ShipmentDataObject;

			AssertExceptionThrown("Docket has shortfalls thus the import should be rejected.", typeof(DataObjectReadFailureException),
			string.Format(@"
Cannot Import {0}
{0} could not be created for Customs Job B123 because there are errors:
In order to fix shortfalls, you will need to modify lines as follows:
ENTRYNUMBER123-3 Product G1/G1 will need 50 to be added to the order
".Trim(), GetDocketType()),
			() => GetDocketWithAllocateMock(GetNewReader(dataObject, Logger)));
		}

		#endregion

		#region SetupWarehouseForUS_Customs

		void SetupWarehouseForUS_Customs()
		{
			// make the orgs have a US Port for US Bonded rules
			Data.Orgs.CRAHOLSYD.OH_RL_NKClosestPort = "USLAX";
			Data.Orgs.INTHEMSYD.OH_RL_NKClosestPort = "USLAX";
			// return the org codes back to original values for DataObjectReaderTest
			// compared to the auto-generated LAX code.
			Data.Orgs.INTHEMSYD.OH_Code = "INTHEMSYD";
			Data.Orgs.CRAHOLSYD.OH_Code = "CRAHOLSYD";
		}

		#endregion

		#region Test_CustomsSource_BondedWarehouseAttrbute

		public void Test_CustomsSource_BondedWarehouseAttribute()
		{
			var warehouse = Data.GetOrCreateWarehouseInDB(isFTZWarehouse: true, isWarehouseCreatedAsVirtual: true);
			Helper.EnableWarehouseForBond(warehouse, true);
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;

			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isFTZWarehouse: true, isWarehouseCreatedAsVirtual: true);
			var product1 = Data.CreateProduct("G1");

			// Receive stock
			var today = ZDateTime.Today;
			var manufacturer = Helper.CreateClient("MANU", "Manufacturer");
			var manufacturerAddress = manufacturer.MainAddress;

			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, warehouse, "R2");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, product1, 1000.0m, "EntryNumber123-3", "", 100m);
			inventory.CustomsData.WB_AddInfo = "Value stored in inventory";
			inventory.CustomsData.WB_CustomsQty = 123m;
			inventory.CustomsData.WB_CustomsUnitOfQty = "CM";
			inventory.CustomsData.WB_DeclarationReference = "Reference in inventory";
			inventory.CustomsData.WB_EntryDate = today;
			inventory.CustomsData.WB_TILV = 125m;
			inventory.CustomsData.WB_RN_NKCountryOfOrigin = "IR";
			inventory.CustomsData.WB_ValueForDuty = 126m;
			inventory.CustomsData.WB_CustomsSecondQuantity = 127m;
			inventory.CustomsData.WB_CustomsSecondUnitQty = "POND";
			inventory.CustomsData.WB_Tariff = "TFI";
			inventory.CustomsData.WB_PrimaryPreference = "PPFI";
			inventory.CustomsData.WB_CustomsThirdQuantity = 128m;
			inventory.CustomsData.WB_CustomsThirdUnitQty = "MM";
			inventory.CustomsData.WB_OA_ManufacturerAddress = manufacturerAddress.PK;
			inventory.CustomsData.WB_BondedWhsUnitOfQty = "TON";
			inventory.CustomsData.WB_RX_NKTILVCurrency = "EUR";
			inventory.CustomsData.WB_OutwardType = WhsBondedWarehouseAttributeOutwardType.Codes.TOF;
			inventory.CustomsData.WB_ZoneStatus = "P";
			inventory.CustomsData.WB_IsFromAnotherFTZWhs = true;
			inventory.CustomsData.WB_MatchingKey = "cd12345678";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			// Order stock
			var line = Data.AddCommercialInvoiceLineWithRelatedEntryLine(200.00m, 3, product1);
			line.DataImportMatchingKey = "cd12345678";
			line.CustomizedFieldCollection.Clear();

			Factory.SaveForTesting();

			var dataObject = Data.ShipmentDataObject;
			var orderPK = GetDocketWithAllocateMock(GetNewReader(dataObject, Logger)).PK;
			Factory.SaveForTesting();

			var order = new BusinessObjectFactory().Load<WhsOrder>(orderPK);
			var customsData = order.Lines.First(ol => ol.WE_OP == product1.PK).CustomsData;

			CombineAssertions(() =>
			{
				AssertEquals("customsData.WB_EntryKey", "DummyOutward-1", customsData.WB_EntryKey);
				AssertEquals("customsData.WB_EntryLineNo", (ZShort)103, customsData.WB_EntryLineNo);
				AssertEquals("customsData.OutwardType", "CNN", customsData.WB_OutwardType);
				AssertEquals("customsData.WB_AddInfo", "Value stored in inventory", customsData.WB_AddInfo);
				AssertEquals("customsData.WB_BondedWhsQty", 1000m, customsData.WB_BondedWhsQty);
				AssertEquals("customsData.WB_CustomsQty", 123m, customsData.WB_CustomsQty);
				AssertEquals("customsData.WB_CustomsUnitOfQty", "CM", customsData.WB_CustomsUnitOfQty);
				AssertEquals("customsData.WB_DeclarationReference", "B123", customsData.WB_DeclarationReference);
				AssertEquals("customsData.WB_EntryDate", today, customsData.WB_EntryDate);
				AssertEquals("customsData.WB_RN_NKCountryOfOrigin", "IR", customsData.WB_RN_NKCountryOfOrigin);
				AssertEquals("customsData.WB_TILV", 125m, customsData.WB_TILV);
				AssertEquals("customsData.WB_ValueForDuty", 126m, customsData.WB_ValueForDuty);
				AssertEquals("customsData.WB_CustomsSecondQuantity", 127m, customsData.WB_CustomsSecondQuantity);
				AssertEquals("customsData.WB_CustomsSecondUnitQty", "POND", customsData.WB_CustomsSecondUnitQty);
				AssertEquals("customsData.WB_Tariff", "TFI", customsData.WB_Tariff);
				AssertEquals("customsData.WB_PrimaryPreference", "PPFI", customsData.WB_PrimaryPreference);
				AssertEquals("customsData.WB_CustomsThirdQuantity", 128m, customsData.WB_CustomsThirdQuantity);
				AssertEquals("customsData.WB_CustomsThirdUnitQty", "MM", customsData.WB_CustomsThirdUnitQty);
				AssertEquals("customsData.WB_RX_NKTILVCurrency", "EUR", customsData.WB_RX_NKTILVCurrency);
				AssertEquals("customsData.WB_OA_ManufacturerAddress", manufacturerAddress.PK, customsData.WB_OA_ManufacturerAddress);
				AssertEquals("customsData.WB_BondedWhsUnitOfQty", "TON", customsData.WB_BondedWhsUnitOfQty);
				AssertEquals("customsData.WB_ZoneStatus", "P", customsData.WB_ZoneStatus);
				AssertEquals("customsData.WB_IsFromAnotherFTZWhs", true, customsData.WB_IsFromAnotherFTZWhs);
				AssertEquals("customsData.WB_MatchingKey", "cd12345678", customsData.WB_MatchingKey);
			});
		}

		#endregion

		#endregion

		#region Test_CustomsSource_RejectsImportIfOrderIsUnpickable

		public void Test_CustomsSource_RejectsImportIfOrderIsUnpickable()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			var dataObject = Data.ShipmentDataObject;
			dataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.Instance);
			dataObject.LocalProcessing.DeliveryRequiredBy = ZDateTime.Today;
			dataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 0m;
			AssertExceptionThrown("No units ordered, thus the import should be rejected.", typeof(DataObjectReadFailureException),
			string.Format(@"
Cannot Import {0}
This {0} has no Lines.".Trim(), GetDocketType()),
			() => GetNewReader(dataObject, Logger).ReadIntoBusinessObject());
		}

		#endregion

		#region Test_CustomsSource_ImportOfOrderIntoRealWhs_ReplacesExistingPick

		public void Test_CustomsSource_ImportOfOrderIntoRealWhs_ReplacesExistingPick()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			var reader = GetNewReader(Data.ShipmentDataObject, Logger);
			var commercialInvoiceLinesDataObject = Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection;

			// order 90 units across 2 lines (100 are in stock)
			commercialInvoiceLinesDataObject[0].BondedWarehouseQuantity = 80; // line no 3
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(10m, 7, Data.Product, 56, "DummyOutward-1", Data.LastUsedEntryLineNo, "EntryNumber123"); // line no 7

			// import the order
			var order1 = GetDocketWithAllocateMock(reader);
			AssertEquals("Precondition", 10m, order1.Lines[0].SumOfUnitsMet);
			AssertEquals("Precondition", 80m, order1.Lines[1].SumOfUnitsMet);
			Factory.SaveForTesting(); // commit pick to DB
			var pick1 = order1.Pick;

			// amend the order by increasing to 100 units across 2 lines (100 are in stock), and also use different line numbers for one line
			commercialInvoiceLinesDataObject[0].BondedWarehouseQuantity = 92;
			commercialInvoiceLinesDataObject[1].BondedWarehouseQuantity = 8;
			commercialInvoiceLinesDataObject[1].LineNo = 8; // change line no to ensure previous line (7) is removed on import

			// reimport the order
			var order2 = GetDocketWithAllocateMock(reader);
			AssertEquals("Precondition - should update existing Order.", order1.PK, order2.PK);

			// ensure the previous pick was cancelled and that the new pick has correct totals.
			//
			// It is important to amend using values that requires stock from the first pick. This is because no DB save
			// occurs when the first pick is cancelled, so the stock would *not* normally be available to the DBOnlyQuery when
			// re-picking. We want to test the smarts that were added to allow us to include the inventory not yet AVL in the DB.
			AssertEquals(true, pick1.IsCancelled);
			AssertNotEquals(pick1.PK, order2.Pick.PK);
			AssertEquals(2, order2.Lines.Count);
			AssertEquals(92m, order2.Lines[0].SumOfUnitsMet);
			AssertEquals(8m, order2.Lines[1].SumOfUnitsMet);
		}

		#endregion

		#region Test_CustomsSource_ImportOfOrderIntoRealWhs_WithAmendment

		public void Test_CustomsSource_ImportOfOrderIntoRealWhs_WithAmendment()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			var commercialInvoiceLine = Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
			// order 80 units (100 are in stock)
			commercialInvoiceLine.BondedWarehouseQuantity = 80;
			var reader = GetNewReader(Data.ShipmentDataObject, Logger);

			// import the order
			var order1 = GetDocketWithAllocateMock(reader);
			AssertEquals("Precondition", 80m, order1.Lines[0].SumOfUnitsMet);
			Factory.SaveForTesting(); // commit pick to DB
			var pick1 = order1.Pick;

			// publish Customs Accept Event
			var eventDataObject = Data.GetEventDataObject(Events.WarehouseJobCanNowBeFinalised);
			ImportEventViaDataContextManager(eventDataObject);
			Factory.SaveForTesting();
			AssertEquals("Precondition - Finalise of Order should be allowed.", true, order1.IsFinaliseAllowed);

			// amend the order by increasing to 100 units (100 are in stock)
			commercialInvoiceLine.BondedWarehouseQuantity = 100;

			// reimport the order
			var order2 = GetDocketWithAllocateMock(reader);
			AssertEquals("Precondition - should update existing Order.", order1.PK, order2.PK);

			// ensure the previous pick was cancelled and that the new pick has correct totals.
			AssertEquals(true, pick1.IsCancelled);
			AssertNotEquals(pick1.PK, order2.Pick.PK);
			AssertEquals(1, order2.Lines.Count);
			AssertEquals(100m, order2.Lines[0].SumOfUnitsMet);
		}

		#endregion

		#region Test_CustomsSource_ImportOfWarehouse_DoesNotMatchTransitWarehouses

		protected override void AssertTransitWarehousesAreNotMatchedOnImport(Func<WhsOrder> attemptImport)
		{
			AssertExceptionThrown("Should not match to a Transit Warehouse.", typeof(DataObjectReadFailureException),
@"Cannot Import Order
Unable to match Warehouse for Organization: In The Moment Address: Unit 12, Level 3.", () => attemptImport());
		}

		#endregion

		#region Test_CustomsSource_ImportOfOrderDoesNotCheckPermit

		public void Test_CustomsSource_ImportOfOrderDoesNotCheckPermit()
		{
			SetupWarehouseForUS_Customs();
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isFTZWarehouse: true);

			AssertNoExceptionThrown("When imporing orders it should not check for permit.", () => GetImportResultsViaDataContextManager(Data.ShipmentDataObject));
		}

		#endregion

		#region Test_CustomsSource_MultipleEntryNoOnImportShouldNotAffectUSBonded

		public void Test_CustomsSource_MultipleEntryNoOnImportShouldNotAffectUSBonded()
		{
			SetupWarehouseForUS_Customs();

			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			var groupedProduct1 = Data.CreateProduct("G1");
			var groupedProduct2 = Data.CreateProduct("G2");
			var groupedProduct3 = Data.CreateProduct("G3");
			var groupedProduct4 = Data.CreateProduct("G4");
			var groupedProduct5 = Data.CreateProduct("G5");
			var groupedProduct6 = Data.CreateProduct("G6");
			var groupedProduct7 = Data.CreateProduct("G7");
			var groupedProduct8 = Data.CreateProduct("G8");
			var groupedProduct9 = Data.CreateProduct("G9");
			var groupedProduct10 = Data.CreateProduct("G10");

			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);

			// Receive stock
			var receive1 = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, warehouse, "R2");
			var receive2 = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, warehouse, "R3");

			receive1.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive2.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive1, groupedProduct1, 800.00m, "EntryNumber123-3", "TestGroup1", 8m);
			Helper.CreateWhsReceiveInventoryLine(receive1, groupedProduct2, 800.00m, "EntryNumber123-4", "TestGroup1", 8m);
			Helper.CreateWhsReceiveInventoryLine(receive1, groupedProduct3, 800.00m, "EntryNumber123-5", "TestGroup1", 8m);
			Helper.CreateWhsReceiveInventoryLine(receive1, groupedProduct4, 800.00m, "EntryNumber123-6", "TestGroup1", 8m);
			Helper.CreateWhsReceiveInventoryLine(receive1, groupedProduct5, 800.00m, "EntryNumber123-7", "TestGroup1", 8m);

			Helper.CreateWhsReceiveInventoryLine(receive2, groupedProduct6, 800.00m, "EntryNumber456-8", "TestGroup2", 8m);
			Helper.CreateWhsReceiveInventoryLine(receive2, groupedProduct7, 800.00m, "EntryNumber456-9", "TestGroup2", 8m);
			Helper.CreateWhsReceiveInventoryLine(receive2, groupedProduct8, 800.00m, "EntryNumber456-10", "TestGroup2", 8m);
			Helper.CreateWhsReceiveInventoryLine(receive2, groupedProduct9, 800.00m, "EntryNumber456-11", "TestGroup2", 8m);
			Helper.CreateWhsReceiveInventoryLine(receive2, groupedProduct10, 800.00m, "EntryNumber456-12", "TestGroup2", 8m);

			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);

			Data.AddCommercialInvoiceLineWithRelatedEntryLine(80m, 3, groupedProduct1, "EntryNumber123");
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(80m, 4, groupedProduct2, "EntryNumber123");
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(80m, 5, groupedProduct3, "EntryNumber123");
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(80m, 6, groupedProduct4, "EntryNumber123");
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(80m, 7, groupedProduct5, "EntryNumber123");

			Data.AddCommercialInvoiceLineWithRelatedEntryLine(80m, 8, groupedProduct6, "EntryNumber456");
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(80m, 9, groupedProduct7, "EntryNumber456");
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(80m, 10, groupedProduct8, "EntryNumber456");
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(80m, 11, groupedProduct9, "EntryNumber456");
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(80m, 12, groupedProduct10, "EntryNumber456");

			Factory.SaveForTesting();

			var dataObject = Data.ShipmentDataObject;
			AssertNoExceptionThrown(() => GetDocketWithAllocateMock(GetNewReader(dataObject, Logger)));
		}

		#endregion

		#region Test_CustomsSource_MatchOnOrderNumber

		public void Test_CustomsSource_MatchOnOrderNumber()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var order = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123-Original", "B123-CustomsParentRef", Data.Product, 5m, "EntryNumber123", 2, "", 0, finalise: false); //create order that doesn't normally match
			Factory.SaveForTesting();
			AssertEquals("Precondition: SumOfUnitsMet correct", 0m, order.Lines[0].SumOfUnitsMet);

			var shipmentDataObject = Data.ShipmentDataObject;
			var commercialLine = shipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
			commercialLine.BondedWarehouseQuantity += 2;
			var linedetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(newProductCode: "", commercialLine, newAttrib1: "", newAttrib2: "", newAttrib3: "", orderNumber: "B123-Original");
			using (new WarehouseCustomsDetailsProvidersMocks(new[] { linedetails.Object }))
			{
				var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
				Assert("Should import successfully.", importResult.WasSuccessful);

				CombineAssertions(() =>
				{
					var docketInOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
					Assert("Lines are still there", docketInOtherFactory.Lines.Count > 0);
					AssertEquals("External Ref correct", "B123-Original", docketInOtherFactory.WD_ExternalReference);
					AssertEquals("Customs Parent Ref correct", "B123-CustomsParentRef", docketInOtherFactory.WD_CustomsParentReference);
					AssertEquals("Imported Qty change found", 7m, docketInOtherFactory.Lines[0].SumOfUnitsMet);
				});
			}
		}

		public void Test_CustomsSource_MatchOnOrderNumber_MultipleNums_ValueAndEmpty()
		{
			Test_CustomsSource_MatchOnOrderNumber_MultipleNumbersCore("B123", string.Empty);
		}

		public void Test_CustomsSource_MatchOnOrderNumberr_MultipleNums_TwoValues()
		{
			Test_CustomsSource_MatchOnOrderNumber_MultipleNumbersCore("C34f", "B123");
		}

		void Test_CustomsSource_MatchOnOrderNumber_MultipleNumbersCore(string orderNum1, string orderNum2)
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var order = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-Before", Data.Product, 5m, "EntryNumber123", 2, "", 0, finalise: false);
			Factory.SaveForTesting();

			var shipmentDataObject = Data.ShipmentDataObject;
			var commercialLine1 = shipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
			var commercialLine2 = new CommercialInvoiceLine()
			{
				AddInfoCollection = new List<AddInfo>
				{
					new AddInfo { Key = "TILV4Warehouse", Value = "777" },
					new AddInfo { Key = "WRL", Value = "5" },
					new AddInfo { Key = "WRN", Value = "ENTRYNUMBER847" },
				},
				PartNo = Data.Product.OP_PartNum,
				BondedWarehouseQuantity = 15m,
				EntryNumber = "EntryNumber687",
				EntryLineNumber = 105,
				LineNo = 4,
			};

			var linedetails1 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(newProductCode: "", commercialLine1, newAttrib1: "", newAttrib2: "", newAttrib3: "", orderNumber: orderNum1, newSerialNum: "");
			var linedetails2 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(newProductCode: "", commercialLine2, newAttrib1: "", newAttrib2: "", newAttrib3: "", orderNumber: orderNum2, newSerialNum: "");
			using (new WarehouseCustomsDetailsProvidersMocks(new[] { linedetails1.Object, linedetails2.Object }))
			{
				var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).First();
				AssertEquals("Orderlines with different Order Numbers should fail.", false, importResult.WasSuccessful);
				Assert("Order Lines' OrderNumber must be empty or reference 1 Order Number.", importResult.Logs.Any(l => l.Type == LogType.Error && l.Message == "Cannot Import - Import contains order-line Order Numbers that reference multiple different Warehouse Orders."));

				var orderInFactory = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order)).Single();
				AssertEquals("Only current order should exist", "B123-Before", orderInFactory.WD_CustomsParentReference);
			}
		}

		public void Test_CustomsSource_MatchOnOrderNumber_OnlyMatchesOrders()
		{
			// Create data including receive
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			var receive = Factory.Load<WhsReceive>(new ZQuery())[0];
			receive.WD_ExternalReference = "B123"; //to match imported Order
			Factory.SaveForTesting();

			var shipmentDataObject = Data.ShipmentDataObject;
			var commercialLine = shipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
			var linedetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(newProductCode: "", commercialLine, newAttrib1: "", newAttrib2: "", newAttrib3: "", orderNumber: "B123", newSerialNum: "");
			using (new WarehouseCustomsDetailsProvidersMocks(new[] { linedetails.Object }))
			{
				var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
				AssertEquals("Import Should pass", true, importResult.WasSuccessful);

				var order = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order)).Single();
				AssertEquals("New order added correctly as only WhsOrder", "B123-EDIDATEDI", order.WD_CustomsParentReference);
			}
		}

		public void Test_CustomsSource_MatchOnOrderNumber_ClientMustMatch()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB();
			GetDocketWithLineForCustoms(Data.Orgs.WUFSHIJNB, warehouse, "B123", "B123-Other", Data.Product, 5m, "EntryNumber123", 2, "", 0, finalise: false);
			Factory.SaveForTesting();

			var shipmentDataObject = Data.ShipmentDataObject;
			var commercialLine = shipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
			var linedetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(newProductCode: "", commercialLine, newAttrib1: "", newAttrib2: "", newAttrib3: "", orderNumber: "B123", newSerialNum: "");
			using (new WarehouseCustomsDetailsProvidersMocks(new[] { linedetails.Object }))
			{
				var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).Single();
				AssertEquals("Import Should pass", true, importResult.WasSuccessful);

				var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
				AssertEquals("Should be two orders now", 2, orders.Length);
				AssertContainsExactElementsInAnyOrder("WD_CustomsParentReference's correct", new[] { "B123-Other", "B123-EDIDATEDI" }, orders.Select(o => o.WD_CustomsParentReference));
			}
		}

		public void Test_CustomsSource_MatchOnOrderNumber_MultipleMatchedOrders()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB();
			GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123-2", "B123-Other1", Data.Product, 5m, "EntryNumber123", 2, "", 0, finalise: false);
			var order2 = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123-2", "B123-Other1", Data.Product, 5m, "EntryNumber123", 2, "", 0, finalise: false, externalRefSplit: 1);
			Factory.SaveForTesting();

			var shipmentDataObject = Data.ShipmentDataObject;
			var commercialLine = shipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
			var linedetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(newProductCode: "", commercialLine, newAttrib1: "", newAttrib2: "", newAttrib3: "", orderNumber: "B123-2", newSerialNum: "");
			using (new WarehouseCustomsDetailsProvidersMocks(new[] { linedetails.Object }))
			{
				var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).First();
				AssertEquals("Matching multiple order should fail.", false, importResult.WasSuccessful);
				Assert("Order Lines' OrderNumber should only match 1 Order.", importResult.Logs.Any(l => l.Type == LogType.Error && l.Message == "Cannot Import - The Order Number found on the Order Lines matches multiple Warehouse Orders."));

				var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
				AssertEquals("Should be two orders now", 2, orders.Length);
				AssertContainsExactElementsInAnyOrder("WD_CustomsParentReference's correct", new[] { "B123-Other1", "B123-Other1" }, orders.Select(o => o.WD_CustomsParentReference));
			}
		}

		public void Test_CustomsSource_MatchOnOrderNumber_AmendOrderInVirtualWhs()
		{
			using (WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

				// create an existing job originating from Customs universal
				var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
				var order = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123-Original", "B123-CustomsParentRef", Data.Product, 5m, "EntryNumber123", 2, "", 0);
				SetUpOrderToMatchCustomsImportInformation(order);
				order.Lines[0].CustomsClearingInProgress = ZBool.True;
				FinaliseDocket(order);
				Factory.SaveForTesting();
				AssertEquals("Precondition - Docket should be Finalised.", true, order.IsFinalised);
				AssertEquals("Orderline should be CustomsClearingInProgress.", ZBool.True, order.Lines[0].CustomsClearingInProgress);

				var workflowInfo = new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWR, ServiceCode = ServiceCodeType.AMD } } };
				Logger.TopLevelDataObject.DataContext.SetWorkflowInfo(workflowInfo);
				AssertEquals("Precondition - Amend service code is set for recipient role ", ServiceCodeType.AMD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First().ServiceCode);

				var shipmentDataObject = Data.ShipmentDataObject;
				var commercialLine = shipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
				var linedetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(newProductCode: "", commercialLine, newAttrib1: "", newAttrib2: "", newAttrib3: "", orderNumber: "B123-Original", newSerialNum: "");

				linedetails.Setup(s => s.CustomsThirdQuantity).Returns(22m);

				using (new WarehouseCustomsDetailsProvidersMocks(new[] { linedetails.Object }))
				{
					var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).First();
					AssertEquals("Amending virtual warehouse order should succeed.", true, importResult.WasSuccessful);

					var docketInOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
					Assert("Lines are still there and still only 1.", docketInOtherFactory.Lines.Count == 1);
					Assert("Changes to line so CustomsClearingInProgress removed.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => !l.CustomsClearingInProgress));
					Assert("Changes to line so WB_EntryKey updated.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => l.CustomsData.WB_EntryKey == "DummyOutward-1"));
					Assert("Changes to line so WB_EntryLineNo updated.", docketInOtherFactory.Lines.Cast<WhsOrderLine>().All(l => l.CustomsData.WB_EntryLineNo == 102));
				}
			}
		}

		public void Test_CustomsSource_MatchOnOrderNumber_CancelledOrder()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB();
			var order = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123-2", "B123-Other1", Data.Product, 5m, "EntryNumber123", 2, "", 0, finalise: false);
			order.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			Factory.SaveForTesting();

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Precondition: Should be one order now", 1, orders.Length);
			AssertEquals("Precondition: Order should be Cancelled", DocketStatus.Codes.Cancelled, orders[0].WD_DocketStatus);

			var shipmentDataObject = Data.ShipmentDataObject;
			var commercialLine = shipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
			var linedetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(newProductCode: "", commercialLine, newAttrib1: "", newAttrib2: "", newAttrib3: "", orderNumber: "B123-2", newSerialNum: "");
			using (new WarehouseCustomsDetailsProvidersMocks(new[] { linedetails.Object }))
			{
				var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).First();
				AssertEquals("Matching cancelled order should fail.", false, importResult.WasSuccessful);
				Assert("Order Lines' OrderNumber should only match non-cancelled Orders.", importResult.Logs.Any(l => l.Type == LogType.Error && l.Message == "Cannot Import - The Order Number found on the Order Lines is Canceled."));
			}
		}

		#endregion

		#region Test_CustomsSource_PopulatesAllocationKey

		public void Test_CustomsSource_PopulatesAllocationKey()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);

			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Data.GetOrCreateWarehouseInDB(), "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, Data.ProductCRAHOLSYD, 3m, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receiveLine1.WI_AllocationKey = "AllocationKey-1";
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, Data.ProductCRAHOLSYD, 3m, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receiveLine2.WI_AllocationKey = "AllocationKey-2";
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive, Data.ProductCRAHOLSYD, 3m, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receiveLine3.WI_AllocationKey = "AllocationKey-3";

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			var allocationInfo1 = new Mock<IWarehouseCustomsLineAllocationInfo>();
			allocationInfo1.Setup(a => a.AllocationKey).Returns("AllocationKey-1");
			allocationInfo1.Setup(a => a.Quantity).Returns(2m);

			var allocationInfo2 = new Mock<IWarehouseCustomsLineAllocationInfo>();
			allocationInfo2.Setup(a => a.AllocationKey).Returns("AllocationKey-3");
			allocationInfo2.Setup(a => a.Quantity).Returns(3m);

			var line = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine();
			line.BondedWarehouseQuantity = null;

			var linedetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(
				newProductCode: "",
				line,
				allocationInfos: new[] { allocationInfo1.Object, allocationInfo2.Object });

			using (new WarehouseCustomsDetailsProvidersMocks(new[] { linedetails.Object }))
			{
				var reader = GetNewReader(Data.ShipmentDataObject, Logger);
				var order = GetDocketWithAllocateMock(reader);

				var orderLines = order.Lines;
				AssertEquals("Should have read in two order lines.", 2, orderLines.Count);

				var orderLine1 = orderLines.First(l => l.WE_AllocationKey == "AllocationKey-1");
				AssertEquals("Should have read in Allocation Key Info correctly.", 2m, orderLine1.WE_TransactionQuantity);
				AssertEquals("Should have allocated correct inventory.", receiveLine1.PK, orderLine1.PickLines.Single().WZ_WE_InventoryLine);

				var orderLine2 = orderLines.First(l => l.WE_AllocationKey == "AllocationKey-3");
				AssertEquals("Should have read in Allocation Key Info correctly.", 3m, orderLine2.WE_TransactionQuantity);
				AssertEquals("Should have allocated correct inventory.", receiveLine3.PK, orderLine2.PickLines.Single().WZ_WE_InventoryLine);
			}
		}

		public void Test_CustomsSource_PopulatesAllocationKey_BondedWarehouseQtyProvided()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

			var allocationInfo1 = new Mock<IWarehouseCustomsLineAllocationInfo>();
			allocationInfo1.Setup(a => a.AllocationKey).Returns("AllocationKey-1");
			allocationInfo1.Setup(a => a.Quantity).Returns(2m);

			var linedetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(
				newProductCode: "",
				allocationInfos: new[] { allocationInfo1.Object });

			using (new WarehouseCustomsDetailsProvidersMocks(new[] { linedetails.Object }))
			{
				var importResult = GetImportResultsViaDataContextManager(Data.ShipmentDataObject).First();
				Assert("Should not import successfully.", !importResult.WasSuccessful);
				Assert("Should say orderline is now Customs Clearing in Progress.",
					importResult.Logs.Any(l => l.Type == LogType.Error && l.Message == "Cannot Import Customs Job B123 as Order Line Bonded Warehouse Quantity and Allocation Key Infos were both provided."));
			}
		}

		#endregion

		#region TestCustomsImportStrategy

		class WhsOrderDataObjectReaderForTest : WhsOrderDataObjectReader
		{
			public WhsOrderDataObjectReaderForTest(UniversalShipment whsOrderDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
				: base(whsOrderDataObject, logger, factory)
			{
			}

			public WhsImportStrategy ImportStrategyPublic => ImportStrategy;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		class WhsCustomsImportStrategyForTest : WhsOrderDataObjectReaderForTest.OrderImportFromCustomsStrategy
		{
			public WhsCustomsImportStrategyForTest(WhsOrderDataObjectReaderForTest reader)
				: base(reader)
			{
			}

			public bool IsOrderLoadedFromCustomsLinesOrderNumberForTest => IsOrderLoadedFromCustomsLinesOrderNumber;

			protected override string CustomsSubType => throw new NotImplementedException();

			protected override ZString FinalisedCannotUpdateMessage(ZString? customsJobNo)
			{
				throw new NotImplementedException();
			}

			protected override bool ShouldAmendmentCancelOutDocket(WhsOrder docket)
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		protected override void TestWhsImportStrategy_LoadDocketFromCustomsLinesDocketNumbersCore()
		{
			var reader = new WhsOrderDataObjectReaderForTest(ShipmentDataObject, Logger, Factory);
			var importStrategy = new WhsCustomsImportStrategyForTest(reader);

			ShipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { GetNewAddressData_CRAHOLSYD(AddressTypes.WarehouseClient) });

			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			Helper.CreateProductClientRelationShip(Data.Orgs.CRAHOLSYD, data.Part1);
			var whsOrderBOToLoad = Helper.CreateWhsOrderWithOrderLine(Data.Orgs.CRAHOLSYD, data.Whs1, data.Part1, 10m);
			Factory.SaveForTesting();

			var linedetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(newProductCode: "", null, newAttrib1: "", newAttrib2: "", newAttrib3: "", orderNumber: "TEST", newSerialNum: "");
			using (new WarehouseCustomsDetailsProvidersMocks(new[] { linedetails.Object }))
			{
				AssertEquals("LoadDocketFromCustomsLinesDocketNumbers is correct", whsOrderBOToLoad.PK, importStrategy.LoadDocketFromCustomsLinesDocketNumbers.PK);
			}
		}

		// Customs Import tests - Amendments

		#region Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCancelsOutPreviousJob

		protected override ZDecimal GetExpectedDocketNewLineUnits(int customsAmendedAmount)
		{
			return customsAmendedAmount; // taking the units out of stock to put on the original order
		}

		protected override void AssertPreviousJobInVirtualWhsWasCancelledOut(OrgSupplierPart expectedProduct, int originalCustomsQty, int amendedCustomsQty, byte expectedDocketSplitNo)
		{
			AssertInventoryWasAmendedBackToOriginalQuantities("R1", 100m, 100 - amendedCustomsQty);
		}

		protected override void AssertDocketWasCancelledOut(WhsOrder docketThatWasAdjusted, ZDecimal amendedValue)
		{
			AssertInventoryWasAmendedBackToOriginalQuantities("R1", 100m, 100 - amendedValue);
			AssertOrderWasCancelledOutInVirtualWhs(docketThatWasAdjusted);
		}

		protected override ZString ErrorMessageForAttemptingAmendingFinalisedDocketInRealWarehouse => @"Cannot populate WhsOrder because:
Warehouse Order could not be amended for Customs Job B123 because it is already finalized, Customs cleared or had changes after finalization that are not allowed.";

		#endregion

		#region Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCreatesAdmendmentEvenWhenFirstJobWasCancelled

		protected override void AssertDocketCancelledByCustomsEventIsNotCancelledAgainWhenAmending(WhsOrder orderThatWasCancelledOut)
		{
			AssertInventoryWasAmendedBackToOriginalQuantities("R1", 100m, 93m);
		}

		#endregion

		#region Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCancelsOutPreviousJob_WhenAmendmentPutsBackEnoughStockToTakeOut

		public void Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCancelsOutPreviousJob_WhenAmendmentPutsBackEnoughStockToTakeOut()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create an existing order originating from Customs universal, but do *not* specify attributes. this tests that the adjustment is putting back stock
			// based on picklines and not order lines (picklines will have the attribs because the stock that was picked has attribs -- see Data.Setup...())
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			var order = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 10m, "EntryNumber123", 2, "DummyOutward-1", 102, finalise: true);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(order);

			// amend qty ordered from 10 to 100 (only 100 were originally put in stock)
			Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 100;
			AssertNewDocketAndCancellingOutOfPreviousDocketFromCustomsAmendment(Data.ShipmentDataObject, Data.Product, 10, 100, 1);
		}

		#endregion

		#region Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCreatesAdjustment_IsRejectedIfOverAdjusting

		public void Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCreatesAdjustment_IsRejectedIfOverAdjusting()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create an existing order originating from Customs universal for 90 units
			Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 90;
			var importResults = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
			var orderImportResult = importResults.Single(o => o.DataContextType == DataContextType.WarehouseOrder);
			var order = (WhsOrder)orderImportResult.GetBizOForTesting(Data.ShipmentDataObject, Factory.BOFactory);
			AssertIsFinalisedPrecondition(order);

			// create an unrelated order that takes the remaining 10 units, leaving 0
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			var unrelatedOrder = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "Different Ref", "Different Ref", Data.Product, 10m, "EntryNumber123", 2, "DummyOutward-1", 102, finalise: true);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(unrelatedOrder);

			// amend qty on the original order from 90 to 100 (only 90 should be adjusted back into the whs before the amended order is picked)
			Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 100;
			var importResults2 = GetImportResultsViaDataContextManager(Data.ShipmentDataObject);
			var orderImportResult2 = importResults2.Single(o =>
			{
				try
				{
					return o.DataContextType == DataContextType.WarehouseOrder;
				}
				catch (InvalidOperationException) // trying to select a nulled ImportLogger's datacontexttype results in an ioe.
				{
					return false;
				}
			});
			AssertContains(
				"Order could not be created for Customs Job B123 because there are errors:\r\n" +
				"You do not have enough stock to fulfill shortfalls on this order\r\n" +
				"ENTRYNUMBER123-2 Product P1/P1 can not be ordered due to lack of stock. 100 was ordered, but 90 is available", orderImportResult2.ToString());
		}

		#endregion

		#region Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCancelsOutPreviousJob_RoundingIssues

		public void Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCancelsOutPreviousJob_RoundingIssues()
		{
			// Create Inventory with Customs Data where the divisions will result in a precision error for the following scenario:
			//
			// We bring into Stock 130 Units, with a Customs Qty of 140 (The other customs values will be the same to make the
			// test simple). This results in:
			//
			//		Inventory 1 - { Qty = 130, CustomsQty = 140 }
			//
			// Create an Order for 80 units. In a virtual warehouse this will result in the Inventory being reduced by 80.
			//
			//		Inventory 1 - { Qty = 50, CustomsQty = 140 }
			//
			// Cancel the Order. In a virtual warehouse the cancelling of this Order will create an adjustment that will add
			// new inventory with +80 units, to bring back what was originally ordered. This new Inventory will be:
			//
			//		Inventory 1 - { Qty = 50, CustomsQty = 140 }
			//		Inventory 2 - { Qty = 80, CustomsQty = 86.1538 [(OriginalCustomsQty * Units) / OriginalReceiptQty i.e (140 * 80) / 130]  }
			//
			// Now when calculating current total Customs Qty we look at the latest adjustment or receive (order by docket ID lol)
			// and then select the top 1 docketLine which has a matching bonded entry key (this suggests that all customs quantities
			// on lines with the same bonded entry key will be equal?) and then reverse the calculation to get the original customs Qty
			//
			//		Original calculation was (140 * 80) / 130 = 86.1538 - [(OriginalCustomsQty * Units) / OriginalReceiptQty]
			//		New Calculation has 'OriginalCustomsQty' coming from the adjustment's inventory = 86.1538, the 'Units' is the
			//		total stock amount = 130 and the 'OriginalReceiptQty' is the units that were adjusted = 80.
			//		This will effectively be the reverse calculation of the previous. This calculation which *should* give us 140
			//		results in: (86.1538 * 130) / 80 = 139.9999, we have now lost precision.
			//
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true, createInventory: false);
			var lastUsedEntryLineNo = Data.LastUsedEntryLineNo;

			// Receive in 130 Units, with Customs Qty of 140
			var line = Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
			line.BondedWarehouseQuantity = 130m;
			line.CustomsQuantity = 140m;
			line.CustomsValue = 140m;
			line.AddInfoCollection.Find(i => i.Key.Equals("TILV4Warehouse")).Value = "140";

			// line was setup as an Order Line with the outwards Entry Key (not EnterNumber123) stored on the Line. Fix this.
			line.EntryNumber = "EntryNumber123";
			line.EntryLineNumber = 2;

			var putawayEngineMock = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive();

			WhsReceive receive;
			using (ObjectFactory.Substitute(putawayEngineMock.Object))
			{
				receive = new WhsReceiveDataObjectReader(Data.ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			}
			AssertIsFinalisedPrecondition(receive);

			Factory.SaveForTesting();

			// Order 80 Units
			line.BondedWarehouseQuantity = 80m;
			var order = GetDocketWithAllocateMock(new WhsOrderDataObjectReader(Data.ShipmentDataObject, Logger, Factory));
			AssertIsFinalisedPrecondition(order);

			Factory.SaveForTesting();

			// Cancel Order
			ImportEventViaDataContextManager(Data.GetEventDataObject(Events.CancelTheWarehouseJob));
			var reloadedOrder = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			// Precondition: ensure order was cancelled
			AssertOrderWasCancelledOutInVirtualWhs(reloadedOrder);

			// Call Bonded Code that Customs uses to get Total Stock values.
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			var bondedTransaction = new WhsBondedWarehouseTransaction();
			bondedTransaction.Warehouse = warehouse.WarehouseAddress;
			bondedTransaction.Client = Data.Orgs.CRAHOLSYD;
			var transactionLine = new WhsBondedWarehouseTransactionLine
			{
				EntryKey = "EntryNumber123",
				EntryLineNumber = 2,
				Product = Data.Product,
				PartAttrib1 = "Red",
				PartAttrib2 = "Medium",
				PartAttrib3 = "S1234",
				Warehouse = warehouse.WarehouseAddress
			};

			bondedTransaction.Lines.Add(transactionLine);
			var warehouseLink = new Bonded.BondedWarehouseLinkCreator().GetNewBondedWarehouseLink(new BusinessObjectFactory());
			Bonded.IWhsBondedWarehouseTransaction transactionWithAvailableQuantities;
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				transactionWithAvailableQuantities = warehouseLink.GetOutwardMovementDetail(bondedTransaction);
			}
			var lineWithCurrentStockDetails = transactionWithAvailableQuantities.Lines.Cast<Bonded.IWhsBondedWarehouseTransactionLine>().Single();

			CombineAssertions(() =>
			{
				AssertEquals("Stock Quantity should be correct.", 130m, lineWithCurrentStockDetails.Quantity);
				AssertEquals("Customs Quantity should be correct.", 140m, lineWithCurrentStockDetails.CustomsQuantity);
				AssertEquals("Value for Duty should be correct.", 140m, lineWithCurrentStockDetails.ValueForDuty);
				AssertEquals("TILV should be correct.", 140m, lineWithCurrentStockDetails.TILV.Amount);
			});
		}

		#endregion

		#region Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCancelsOutPreviousJob_WithGroupID_NoExceptions

		[ExpectNoExceptions]
		public void Test_CustomsSource_ImportOfFinalisedJobInVirtualWhsCancelsOutPreviousJob_WithGroupID_NoExceptions()
		{
			SetupWarehouseForUS_Customs();

			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);
			var product2 = Data.CreateProduct("P2");
			var product3 = Data.CreateProduct("P3");

			var receive1 = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, warehouse, "R2");
			receive1.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive1, product2, 2m, "Entry-Number-1-1", "Group1", 2m);
			Helper.CreateWhsReceiveInventoryLine(receive1, product3, 3m, "Entry-Number-1-2", "Group1", 3m);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);
			Factory.SaveForTesting();

			Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection.Clear();
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(2m, 1, product2, 11, "DummyOutward-1", 1, "Entry-Number-1");
			Data.AddCommercialInvoiceLineWithRelatedEntryLine(3m, 2, product3, 22, "DummyOutward-1", 2, "Entry-Number-1");

			var dataObject = Data.ShipmentDataObject;
			GetDocketWithAllocateMock(GetNewReader(dataObject, Logger));
			Factory.SaveForTesting();

			GetDocketWithAllocateMock(GetNewReader(dataObject, Logger, useCleanFactory: true));
		}

		#endregion

		// Customs Import tests - Events

		#region Test_CustomsSource_OnUniversalEventAdded_CancelsDocketWhenCancelEventIsAdded

		protected override void SetupDocketToTestCancel(WhsOrder order)
		{
			base.SetupDocketToTestCancel(order);
			PickOrder(order);
			AssertNotNull("Precondition", order.Pick);
			AssertEquals("Precondition", false, order.Pick.IsCancelled);
			AssertEquals("Precondition", PickType.Codes.Order, order.Pick.WP_PickType);
		}

		protected override void TestDocketIsCancelledInRealWhs(WhsOrder order)
		{
			AssertNull("Cancelling Docket should have cancelled Pick.", order.Pick);
		}

		#endregion

		//

		#region Test_IChangeOfOwnershipGetExistingBusinessObject_GetExistingBusinessObject

		public void Test_IChangeOfOwnershipGetExistingBusinessObject_GetExistingBusinessObject()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true);

			// create an existing job originating from Customs universal
			var warehouse = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true);

			var docket = GetDocketWithLineForCustoms(Data.Orgs.CRAHOLSYD, warehouse, "B123", "B123-EDIDATEDI", Data.Product, 10m, "EntryNumber123", 2, "DummyOutward-1", 102, finalise: true);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(docket);

			// amend qty from 10 to 7
			Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 7;
			AssertEquals("Should simply get the Existing Docket.", docket.PK, ((IChangeOfInventoryGetExistingBusinessObject)GetNewReader(Data.ShipmentDataObject, Logger)).GetExistingBusinessObject().PK);
		}

		#endregion

		#region TestCustomsSourceImportFinalisesDocket_RejectsImportIfUnableToFinalise_DPSMatched

		protected override string ExpectedDPSMatchedExceptionMessage => @"
Cannot Import Order
Order could not be finalized into the Warehouse for Customs Job B123 because of the following error(s):
Error: Finalise
Error - Warehouse Order: Cannot finalize when Denied Party Screening is matched.".Trim();

		#endregion

		#region TestAdditionalInfos_Merging

		public void TestMergingAddInfo()
		{
			using (SetupForAdditionalInfoTests(true))
			{
				var readerPreviousOrder = GetNewReader(ShipmentDataObject, Logger);
				var previous = readerPreviousOrder.ReadIntoBusinessObject();

				previous.Lines[0].PickLines[0].Inventory.InDocketLine.CustomsData.WB_AddInfo = "Extra1=aaa*Extra2=bbb";

				var reader = new WhsOrderDataObjectReaderForTest(ShipmentDataObject, Logger, Factory);
				reader.ImportStrategyPublic.SetMostRecentDocketForCustomsIfValid_VirtualFinalisedOrRealCancelled(previous);
				var whsDocketBO = reader.ReadIntoBusinessObject();

				AssertNotNull(whsDocketBO);
				AssertEquals("There is a docket line", 1, whsDocketBO.Lines.Count);
				var docketLine = whsDocketBO.Lines[0];
				AssertNotNull("There is customs data", docketLine.CustomsData);
				var additionalInfo = docketLine.CustomsData[WhsBondedWarehouseAttributeSchema.WB_AddInfo]?.ToString();
				AssertEquals("AddInfo should be merged", "Add info?*Extra1=aaa*Extra2=bbb", additionalInfo);
			}
		}

		WarehouseCustomsDetailsProvidersMocks SetupForAdditionalInfoTests(bool whsIsVirtual = false)
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(recipientRoles: new[] { new RecipientRoleDetail { Type = RecipientRoleType.AAD } }, isWarehouseCreatedAsVirtual: whsIsVirtual);

			var entryHeader = Data.ShipmentDataObject.EntryHeaderCollection[0];
			entryHeader.Reference = "B123";

			var invoice = Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0];
			var invoiceLine = invoice.CommercialInvoiceLineCollection[0];
			invoiceLine.DataImportMatchingKey = $"{entryHeader.Reference}_1";

			Data.CreateClientOrgCRAHOLSYDInDB();
			Data.GetOrCreateWarehouseInDB(whsIsVirtual);

			Data.ShipmentDataObject.DataContext.AddDataSource(DataContextType.WarehouseCustomsEntry, entryHeader.Reference.Value);
			Data.ShipmentDataObject.Branch = Branch.New(GlbBranch.CurrentBranch);
			Data.ShipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			Factory.SaveForTesting();

			var lineDetailsMock = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(invoiceLine: invoiceLine, newProductCode: Data.Product.OP_PartNum, newSerialNum: "");
			lineDetailsMock.Setup(s => s.DataImportMatchingKey).Returns(invoiceLine.DataImportMatchingKey);

			return new WarehouseCustomsDetailsProvidersMocks(new List<IWarehouseCustomsLineDetails>() { lineDetailsMock.Object });
		}

		#endregion TestAdditionalInfos_Merging

		#region Implementation

		protected override WhsOrderAndReceiveDataObjectWriter<WhsOrder> GetNewWriter(WhsOrder order)
		{
			return new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order)));
		}

		protected override bool IsExceptionThrownWhenNoOrInvalidWarehouseSpecifiedButClientIsSpecifiedAndValidWarehouseIsInDB
		{
			get { return false; }
		}

		protected override WhsOrderDataObjectReader GetNewReader(UniversalShipment shipmentDataObject, IXmlImportLogger logger, bool useCleanFactory = false)
		{
			return new WhsOrderDataObjectReader(shipmentDataObject, logger, useCleanFactory ? new UniversalObjectFactory() : Factory);
		}

		protected override string GetDocketType()
		{
			return "Order";
		}

		protected override WhsOrder GetNewDocket(OrgHeader client, WhsWarehouse warehouse, string externalReference = "")
		{
			return Helper.CreateWhsOrder(client, warehouse, externalReference);
		}

		protected override string GetProcessType()
		{
			return "WOU";
		}

		protected override WhsDocket GetNewDocketOfDifferentType()
		{
			return Factory.NewWithValidTestData<WhsReceive>();
		}

		static void SetUpOrderToMatchCustomsImportInformation(WhsOrder order)
		{
			order.WD_CustomerReference = "EntryNumber123";
			order.WD_TotalOrderValue = 10.3m;
			order.WD_RX_NKTotalOrderCurrency = "AUD";

			var orderLine = order.Lines[0];
			orderLine.WE_LineNo = 3;
			orderLine.WE_SubLineNo = 0;
			orderLine.WE_BondedEntryKey = "ENTRYNUMBER123-2";
			orderLine.WE_ExtendedLinePrice = 10.3m;
			orderLine.WE_RX_NKUnitPriceCurrency = "AUD";

			var customsData = order.Lines[0].CustomsData;
			customsData.WB_CustomsQty = 6m;
			customsData.WB_CustomsUnitOfQty = "PCE";
			customsData.WB_CustomsSecondQuantity = 12m;
			customsData.WB_CustomsSecondUnitQty = "BOX";
			customsData.WB_CustomsThirdQuantity = 22m;
			customsData.WB_CustomsThirdUnitQty = "BOX";
			customsData.WB_DeclarationReference = "B123";
			customsData.WB_PrimaryPreference = "PP";
			customsData.WB_RN_NKCountryOfOrigin = "IT";
			customsData.WB_Tariff = "T2";
			customsData.WB_TILV = 777m;
			customsData.WB_ValueForDuty = 888m;
		}

		internal static void AssertContents(WhsOrder whsOrderBO)
		{
			AssertEquals("whsOrderBO.WD_ContainerMode", "LCL", whsOrderBO.WD_ContainerMode);
			AssertEquals("whsOrderBO.WD_TransportMode", "SEA", whsOrderBO.WD_TransportMode);
			AssertEquals("whsOrderBO.WD_GoodsDescription", "Special Sauce", whsOrderBO.WD_GoodsDescription);
			AssertEquals("whsOrderBO.WD_AddPalletWeightToOrder Was Not Set", ZBool.False, whsOrderBO.WD_AddPalletWeightToOrder);
			AssertEquals("whsOrderBO.WD_PackingAfterPickingRequired Was Not Set", ZBool.True, whsOrderBO.WD_PackingAfterPickingRequired);
			AssertEquals("whsOrderBO.WD_QualityAuditRequired Was Set", ZBool.True, whsOrderBO.WD_QualityAuditRequired);
			AssertEquals("whsOrderBO.WD_ExcludeFromTotePicking Was Set", ZBool.True, whsOrderBO.WD_ExcludeFromTotePicking);
			AssertEquals("whsOrderBO.WD_CubicSent Was Not Set", 0m, whsOrderBO.WD_CubicSent);
			AssertEquals("whsOrderBO.WD_F3_NKTotalPackType Was Not Set", "", whsOrderBO.WD_F3_NKTotalPackType);
			AssertEquals("whsOrderBO.WD_PackagesSent Was Not Set", 0, whsOrderBO.WD_PackagesSent);
			AssertEquals("whsOrderBO.WD_PalletsSent Was Not Set", new ZShort(0), whsOrderBO.WD_PalletsSent);
			AssertEquals("whsOrderBO.WD_WeightSent Was Not Set", 0m, whsOrderBO.WD_WeightSent);
			AssertEquals("whsOrderBO.WD_WeightSentUserEntered Was Not Set", 0m, whsOrderBO.WD_WeightSentUserEntered);
			AssertEquals("whsOrderBO.WD_BOLNo", "BILL", whsOrderBO.WD_BOLNo);
			AssertEquals("whsOrderBO.WD_CODPayMethod", "CCC", whsOrderBO.WD_CODPayMethod);
			AssertEquals("whsOrderBO.WD_CustomerReference", "CUSTOMER", whsOrderBO.WD_CustomerReference);

			AssertEquals("whsOrderBO.WD_UnitsSent", 0m, whsOrderBO.WD_UnitsSent);
			AssertEquals("whsOrderBO.WD_DocketStatus", "HEL", whsOrderBO.WD_DocketStatus);
			AssertEquals("whsOrderBO.WD_DocketSubType", "CUS", whsOrderBO.WD_DocketSubType);
			AssertEquals("whsOrderBO.WD_DropMode", "DRO", whsOrderBO.WD_DropMode);
			AssertEquals("whsOrderBO.WD_ExternalReference", "ORDERME", whsOrderBO.WD_ExternalReference);
			AssertEquals("whsOrderBO.WD_ExternalReferenceSplit", new ZByte(1), whsOrderBO.WD_ExternalReferenceSplit);
			AssertEquals("whsOrderBO.WD_INCO", "INC", whsOrderBO.WD_INCO);
			AssertEquals("whsOrderBO.WD_LocalCartInsuranceCost", 22.2m, whsOrderBO.WD_LocalCartInsuranceCost);
			AssertEquals("whsOrderBO.WD_PickOption", "AUT", whsOrderBO.WD_PickOption);
			AssertEquals("whsOrderBO.WD_PL_NKCarrierServiceLevel", "SET", whsOrderBO.WD_PL_NKCarrierServiceLevel);
			AssertEquals("whsOrderBO.WD_RS_NKServiceLevel", "TSL", whsOrderBO.WD_RS_NKServiceLevel);
			AssertEquals("whsOrderBO.WD_RequiredDate", new ZDateTimeOffset(2011, 1, 1), whsOrderBO.WD_RequiredDate);
			AssertEquals("whsOrderBO.WD_ShipperCODAmount", 43.2m, whsOrderBO.WD_ShipperCODAmount);
			AssertEquals("whsOrderBO.WD_TotalCubic", 12.5m, whsOrderBO.WD_TotalCubic);
			AssertEquals("whsOrderBO.WD_TotalCubicUnit Stayed as Default", "M3", whsOrderBO.WD_TotalCubicUnit);
			AssertEquals("whsOrderBO.WD_TotalUnits", 12.3m, whsOrderBO.WD_TotalUnits);
			AssertEquals("whsOrderBO.WD_TotalWeight", 32.6m, whsOrderBO.WD_TotalWeight);
			AssertEquals("whsOrderBO.WD_TotalWeightUnit Stayed as Default", "KG", whsOrderBO.WD_TotalWeightUnit);
			AssertEquals("whsOrderBO.WD_TransportReference", "TRANS", whsOrderBO.WD_TransportReference);
			AssertEquals("whsOrderBO.CrossDockLocation.ToLocationString()", "DockA", whsOrderBO.CrossDockLocation.ToLocationString());
			AssertEquals("whsOrderBO.Warehouse.WW_WarehouseCode", "WHS", whsOrderBO.Warehouse.WW_WarehouseCode);
			AssertEquals("whsOrderBO.WD_WhsOrderFulfillmentRule", "NON", whsOrderBO.WD_WhsOrderFulfillmentRule);
			AssertEquals("WhsOrderBO.WD_IsAuthorisedToLeave", ZBool.True, whsOrderBO.WD_IsAuthorisedToLeave);
			AssertEquals("WhsOrderBO.SalesChannel.WSH_Code", "TST", whsOrderBO.SalesChannel.WSH_Code);
			AssertEquals("WhsOrderBO.SalesChannel.WSH_Description", "Fishing", whsOrderBO.SalesChannel.WSH_Description);
		}

		void SetupMainOrgAddress(OrgHeader org, ZString address1, ZString address2, ZString city, ZString postCode, ZString state, ZString relatedPortCode)
		{
			var address = org.MainAddress;
			address.OA_Address1 = address1;
			address.OA_Address2 = address2;
			address.OA_City = city;
			address.OA_PostCode = postCode;
			address.OA_State = state;
			address.OA_RL_NKRelatedPortCode = relatedPortCode;
		}

		protected override WhsOrder GetNewDocketWithLineCore(OrgHeader org, WhsWarehouse whs, ZString externalRef, OrgSupplierPart part, ZDecimal qty)
		{
			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, externalRef, part, qty);
			order.WD_RequiredDate = new ZDateTimeOffset(2013, 3, 19);

			return order;
		}

		protected override void SetupDocketLineForCustoms(WhsOrderLine line, ZString inwardsBondedEntryKey, ZShort inwardsEntryLineNo, ZString outwardsBondedEntryKey, ZShort outwardsEntryLineNo)
		{
			line.WE_BondedEntryKey = inwardsBondedEntryKey + "-" + inwardsEntryLineNo;
			line.CustomsData.WB_EntryKey = outwardsBondedEntryKey;
			line.CustomsData.WB_EntryLineNo = outwardsEntryLineNo;
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
			Factory.SaveForTesting();
		}

		protected override void FinaliseDocket(WhsOrder order)
		{
			PickAndFinaliseInDB(order);
		}

		protected override RecipientRoleType GetRecipientRoleType() => RecipientRoleType.BWR;

		protected override DataContextType DataContext => DataContextType.WarehouseOrder;

		protected override void SetUp()
		{
			base.SetUp();
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		#endregion
	}
}
