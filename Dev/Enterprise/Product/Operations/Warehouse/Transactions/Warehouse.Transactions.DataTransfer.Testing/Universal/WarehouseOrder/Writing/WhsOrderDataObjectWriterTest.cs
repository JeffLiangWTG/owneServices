using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	sealed class WhsOrderDataObjectWriterTest : WhsOrderAndReceiveDataObjectWriterTest<WhsOrder, WhsOrderDataObjectWriter>
	{
		#region TestDefaultingOfTransportContainerModesAndINCOTerms

		public void TestDefaultingOfTransportContainerModesAndINCOTerms()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "SGSIN";

			var whsOrderBO = Factory.New<WhsOrder>();
			whsOrderBO.WD_OH_Client = data.Org1.PK;
			whsOrderBO.WD_WW_Whs = data.Whs1.PK;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "NZAKL";
			whsOrderBO.ConsigneeAddressPK = consignee.MainAddress.PK;
			whsOrderBO.WD_INCO = "";

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);

			AssertNotNull("whsOrderData", whsOrderData);

			CombineAssertions(delegate
			{
				AssertEquals("whsOrderData.ContainerMode.Code", "LTL", whsOrderData.ContainerMode.Code);
				AssertEquals("whsOrderData.ContainerMode.Description", "Less Truck Load", whsOrderData.ContainerMode.Description);
				AssertEquals("whsOrderData.ShipmentIncoTerm.Code", "FOB", whsOrderData.ShipmentIncoTerm.Code);
				AssertEquals("whsOrderData.ShipmentIncoTerm.Description", "Free On Board", whsOrderData.ShipmentIncoTerm.Description);
				AssertEquals("whsOrderData.TransportMode.Code", "ROA", whsOrderData.TransportMode.Code);
				AssertEquals("whsOrderData.TransportMode.Description", "Road Freight", whsOrderData.TransportMode.Description);
			});

			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "NZCHC";

			var newWhsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);
			AssertNotNull("newWhsOrderData", newWhsOrderData);
			AssertEquals("Should not default an Incorrect INCO term", "", newWhsOrderData.ShipmentIncoTerm.Code);
			AssertEquals("Should not default an Incorrect INCO term", null, newWhsOrderData.ShipmentIncoTerm.Description);
		}

		#endregion

		#region TestBasicOrderLevelFieldMappings

		public void TestBasicOrderLevelFieldMappings()
		{
			var requiredByDate = new ZDateTimeOffset(2011, 1, 5, 23, 59, 0);

			#region Setup whsOrderBO

			var whsPick = Factory.New<WhsPick>();
			whsPick.WP_PickNo = "P0000001";

			var whsOrderBO = Factory.New<WhsOrder>();
			whsPick.Orders.Add(whsOrderBO);

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Ware this!";
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUMEL";
			whsOrderBO.WD_WW_Whs = warehouse.PK;

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var warehouseDockDoorLocation = Helper.CreateRowAndGenerateLocations(warehouse, "DockA", 1, 1).Locations.Single();
			warehouseDockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			whsOrderBO.WD_WL_CrossDock = warehouseDockDoorLocation.PK;

			var serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_Code = "TSL";
			serviceLevel.RS_Description = "Test Service Level";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "ABC";
			carrierServiceLevel.PL_CarrierServiceLevelDescription = "service level description";
			carrierServiceLevel.PL_CarrierServiceCode = "1234";
			carrierServiceLevel.PL_ChargeCode = "1212121212";
			carrierServiceLevel.PL_ProductCode = "555555";
			carrierServiceLevel.PL_APProfileID = "AP3333";
			carrierServiceLevel.PL_IsSignatureRequired = true;

			var consignee = Factory.NewWithValidTestData<OrgAddress>();
			consignee.OA_RL_NKRelatedPortCode = "AUBNE";
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "FREDDIE";
			client.MiscServ.OM_OC_EXDefaultDGContact = contact.PK;
			whsOrderBO.ConsigneeAddressPK = consignee.PK;
			whsOrderBO.WD_AddPalletWeightToOrder = true;
			whsOrderBO.WD_QualityAuditRequired = true;
			whsOrderBO.WD_PackingAfterPickingRequired = true;
			whsOrderBO.WD_ExcludeFromTotePicking = true;
			whsOrderBO.WD_OH_Client = client.PK;
			whsOrderBO.WD_OH_Forwarder = Factory.NewWithValidTestData<OrgHeader>().PK;
			whsOrderBO.TransportCoPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			whsOrderBO.WD_TotalWeightUnit = "LB";
			whsOrderBO.WD_TotalCubicUnit = "CF";
			whsOrderBO.WD_BOLNo = "BILL";
			whsOrderBO.WD_CODPayMethod = "COC";
			whsOrderBO.WD_CubicSent = 23.1m;
			whsOrderBO.WD_WeightSentUserEntered = 7.9m;
			whsOrderBO.WD_PackagesSent = 5;
			whsOrderBO.WD_CustomerReference = "CUSTOMER";
			whsOrderBO.WD_DocketID = "W10001011";
			whsOrderBO.WD_DocketStatus = "PIC";
			whsOrderBO.WD_DocketSubType = "CUS";
			whsOrderBO.WD_DocketType = "ORD";
			whsOrderBO.WD_DropMode = "HSL";
			whsOrderBO.WD_ExternalReference = "ORDER123";
			whsOrderBO.WD_ExternalReferenceSplit = 1;
			whsOrderBO.WD_INCO = "FCD";
			whsOrderBO.WD_LocalCartInsuranceCost = 13.4m;
			whsOrderBO.WD_PickOption = "AUT";
			whsOrderBO.WD_PL_NKCarrierServiceLevel = "STD";
			whsOrderBO.WD_RS_NKServiceLevel = "TSL";
			whsOrderBO.WD_RequiredDate = requiredByDate;
			whsOrderBO.WD_ShipperCODAmount = 54.3m;
			whsOrderBO.WD_TotalCubic = 23.3m;
			whsOrderBO.WD_TotalUnits = 7.1m;
			whsOrderBO.WD_TotalWeight = 45.8m;
			whsOrderBO.WD_UnitsSent = 12.1m;
			whsOrderBO.WD_WeightSent = 66.8m;
			whsOrderBO.WD_TransportReference = "TREEE";
			whsOrderBO.WD_WhsOrderFulfillmentRule = "NON";
			whsOrderBO.WD_F3_NKTotalPackType = "CTN";
			whsOrderBO.WD_TransportMode = "SEA";
			whsOrderBO.WD_ContainerMode = "LCL";
			whsOrderBO.WD_GoodsDescription = "Special Sauce";
			whsOrderBO.TransportCoPK = carrier.PK;
			whsOrderBO.WD_IsAuthorisedToLeave = true;

			var whsSalesChannel = Helper.CreateWhsSalesChannel("SAL", "SalesTest");
			whsOrderBO.WD_WSH_SalesChannel = whsSalesChannel.PK;

			var carrierAccount1 = carrier.CarrierAccounts.AddNew();
			carrierAccount1.OAN_AccountNumber = "TR1SONYSYD";
			carrierAccount1.OAN_DepotID = "DPO001";
			carrierAccount1.OAN_MerchantNumber = "MERCH001";

			var whsCANSonySyd = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonySyd.OWC_WW_Warehouse = warehouse.PK;
			whsCANSonySyd.OWC_OAN_CarrierAccount = carrierAccount1.PK;
			whsCANSonySyd.OWC_WSH_SalesChannel = whsSalesChannel.PK;

			#endregion

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);

			AssertNotNull("whsOrderData", whsOrderData);

			#region Check Contents of whsOrderData object

			CombineAssertions(delegate
			{
				AssertEquals("whsOrderData.ActualChargeable", null, whsOrderData.ActualChargeable);
				AssertEquals("whsOrderData.AdditionalTerms", null, whsOrderData.AdditionalTerms);
				AssertEquals("whsOrderData.AgentsReference", null, whsOrderData.AgentsReference);
				AssertEquals("whsOrderData.AWBServiceLevel", null, whsOrderData.AWBServiceLevel);
				AssertEquals("whsOrderData.BookingConfirmationReference", null, whsOrderData.BookingConfirmationReference);
				AssertEquals("whsOrderData.CartageWaybillNumber", null, whsOrderData.CartageWaybillNumber);
				AssertEquals("whsOrderData.CFSReference", null, whsOrderData.CFSReference);
				AssertEquals("whsOrderData.ConsolidatedCargoStatus", null, whsOrderData.ConsolidatedCargoStatus);
				AssertEquals("whsOrderData.ContainerCount", null, whsOrderData.ContainerCount);
				AssertEquals("whsOrderData.ContainerMode.Code", "LCL", whsOrderData.ContainerMode.Code);
				AssertEquals("whsOrderData.ContainerMode.Description", "Less Container Load", whsOrderData.ContainerMode.Description);
				AssertEquals("whsOrderData.CountryOfSupply", null, whsOrderData.CountryOfSupply);
				AssertEquals("whsOrderData.DocumentedChargeable", null, whsOrderData.DocumentedChargeable);
				AssertEquals("whsOrderData.DocumentedVolume", null, whsOrderData.DocumentedVolume);
				AssertEquals("whsOrderData.DocumentedWeight", null, whsOrderData.DocumentedWeight);
				AssertEquals("whsOrderData.EFTMode", null, whsOrderData.EFTMode);
				AssertEquals("whsOrderData.EntryStatus", null, whsOrderData.EntryStatus);
				AssertEquals("whsOrderData.ExportGoodsType", null, whsOrderData.ExportGoodsType);
				AssertEquals("whsOrderData.FirstBuyerContact", null, whsOrderData.FirstBuyerContact);
				AssertEquals("whsOrderData.Folio", null, whsOrderData.Folio);

				AssertEquals("whsOrderData.FreightRate", null, whsOrderData.FreightRate);
				AssertEquals("whsOrderData.FreightRateCurrency", null, whsOrderData.FreightRateCurrency);
				AssertEquals("whsOrderData.GoodsDescription", "Special Sauce", whsOrderData.GoodsDescription);
				AssertEquals("whsOrderData.GoodsValue", 0m, whsOrderData.GoodsValue);
				AssertEquals("whsOrderData.GoodsValueCurrency", "", whsOrderData.GoodsValueCurrency.Code);
				AssertEquals("whsOrderData.HBLAWBChargesDisplay", null, whsOrderData.HBLAWBChargesDisplay);
				AssertEquals("whsOrderData.HBLContainerPackModeOverride", null, whsOrderData.HBLContainerPackModeOverride);

				AssertEquals("whsOrderData.InsuranceValue", null, whsOrderData.InsuranceValue);
				AssertEquals("whsOrderData.InsuranceValueCurrency", null, whsOrderData.InsuranceValueCurrency);
				AssertEquals("whsOrderData.InterimReceiptNumber", null, whsOrderData.InterimReceiptNumber);
				AssertEquals("whsOrderData.IsBooking", null, whsOrderData.IsBooking);
				AssertEquals("whsOrderData.IsCFSRegistered", null, whsOrderData.IsCFSRegistered);
				AssertEquals("whsOrderData.IsDirectBooking", null, whsOrderData.IsDirectBooking);
				AssertEquals("whsOrderData.IsForwardRegistered", null, whsOrderData.IsForwardRegistered);
				AssertEquals("whsOrderData.IsNeutralMaster", null, whsOrderData.IsNeutralMaster);
				AssertEquals("whsOrderData.IsPersonalEffects", null, whsOrderData.IsPersonalEffects);
				AssertEquals("whsOrderData.IsShipping", null, whsOrderData.IsShipping);
				AssertEquals("whsOrderData.IsSplitShipment", null, whsOrderData.IsSplitShipment);
				AssertEquals("whsOrderData.LloydsIMO", null, whsOrderData.LloydsIMO);
				AssertEquals("whsOrderData.ManifestedChargeable", null, whsOrderData.ManifestedChargeable);
				AssertEquals("whsOrderData.ManifestedVolume", null, whsOrderData.ManifestedVolume);
				AssertEquals("whsOrderData.ManifestedWeight", null, whsOrderData.ManifestedWeight);
				AssertEquals("whsOrderData.MergeBy", null, whsOrderData.MergeBy);
				AssertEquals("whsOrderData.MessageStatus", null, whsOrderData.MessageStatus);
				AssertEquals("whsOrderData.MessageSubType", null, whsOrderData.MessageSubType);
				AssertEquals("whsOrderData.MessageType", null, whsOrderData.MessageType);
				AssertEquals("whsOrderData.NoCopyBills", null, whsOrderData.NoCopyBills);
				AssertEquals("whsOrderData.NoOriginalBills", null, whsOrderData.NoOriginalBills);
				AssertEquals("whsOrderData.OperationalStatus", null, whsOrderData.OperationalStatus);
				AssertEquals("whsOrderData.OuterPacks", 5, whsOrderData.OuterPacks);
				AssertEquals("whsOrderData.OuterPacksPackageType.Code", "CTN", whsOrderData.OuterPacksPackageType.Code);
				AssertEquals("whsOrderData.OuterPacksPackageType.Description", "Carton", whsOrderData.OuterPacksPackageType.Description);

				AssertEquals("whsOrderData.OwnerRef", null, whsOrderData.OwnerRef);
				AssertEquals("whsOrderData.PackingOrder", null, whsOrderData.PackingOrder);
				AssertEquals("whsOrderData.PaymentMethod", null, whsOrderData.PaymentMethod);
				AssertEquals("whsOrderData.QuoteNumber", null, whsOrderData.QuoteNumber);
				AssertEquals("whsOrderData.ReleaseType", null, whsOrderData.ReleaseType);
				AssertEquals("whsOrderData.ScreeningStatus.Code", ScreeningStatusesList.Codes.NotScreened, whsOrderData.ScreeningStatus.Code);
				AssertEquals("whsOrderData.ScreeningStatus.Description", ScreeningStatusesList.Descriptions.NotScreened, whsOrderData.ScreeningStatus.Description);
				AssertEquals("whsOrderData.SecondBuyerContact", null, whsOrderData.SecondBuyerContact);
				AssertEquals("whsOrderData.CarrierServiceLevel.Code", "STD", whsOrderData.CarrierServiceLevel.Code);
				AssertEquals("whsOrderData.CarrierServiceLevel.Description", "Standard", whsOrderData.CarrierServiceLevel.Description);
				AssertEquals("whsOrderData.ServiceLevel.Code", "TSL", whsOrderData.ServiceLevel.Code);
				AssertEquals("whsOrderData.ServiceLevel.Description", "Test Service Level", whsOrderData.ServiceLevel.Description);
				AssertEquals("whsOrderData.ShipmentIncoTerm.Code", "FCD", whsOrderData.ShipmentIncoTerm.Code);
				AssertEquals("whsOrderData.ShipmentIncoTerm.Description", "Collect COD", whsOrderData.ShipmentIncoTerm.Description);
				AssertEquals("whsOrderData.ShipmentStatus", null, whsOrderData.ShipmentStatus);
				AssertEquals("whsOrderData.ShipmentType", null, whsOrderData.ShipmentType);
				AssertEquals("whsOrderData.ShippedOnBoard", null, whsOrderData.ShippedOnBoard);

				AssertEquals("whsOrderData.ShipperCODAmount", 54.3m, whsOrderData.ShipperCODAmount);
				AssertEquals("whsOrderData.ShipperCODPayMethod.Code", "COC", whsOrderData.ShipperCODPayMethod.Code);
				AssertEquals("whsOrderData.ShipperCODPayMethod.Description", "Company Check", whsOrderData.ShipperCODPayMethod.Description);
				AssertEquals("whsOrderData.TotalNoOfPacks", 12, whsOrderData.TotalNoOfPacks);
				AssertEquals("whsOrderData.TotalNoOfPacksDecimal", null, whsOrderData.TotalNoOfPacksDecimal);
				AssertEquals("whsOrderData.TotalNoOfPacksPackageType.Code", "PCE", whsOrderData.TotalNoOfPacksPackageType.Code);
				AssertEquals("whsOrderData.TotalNoOfPacksPackageType.Description", "Piece", whsOrderData.TotalNoOfPacksPackageType.Description);
				AssertEquals("whsOrderData.TotalNoOfPieces", null, whsOrderData.TotalNoOfPieces);
				AssertEquals("whsOrderData.TotalVolume", 23.1m, whsOrderData.TotalVolume);
				AssertEquals("whsOrderData.TotalVolumeUnit.Code", "CF", whsOrderData.TotalVolumeUnit.Code);
				AssertEquals("whsOrderData.TotalVolumeUnit.Description", "Cubic Feet", whsOrderData.TotalVolumeUnit.Description);
				AssertEquals("whsOrderData.TotalWeight", 66.8m, whsOrderData.TotalWeight);
				AssertEquals("whsOrderData.TotalWeightUnit.Code", "LB", whsOrderData.TotalWeightUnit.Code);
				AssertEquals("whsOrderData.TotalWeightUnit.Description", "Pounds", whsOrderData.TotalWeightUnit.Description);
				AssertEquals("whsOrderData.TranshipToOtherCFS", null, whsOrderData.TranshipToOtherCFS);

				AssertEquals("whsOrderData.TransportMode.Code", "SEA", whsOrderData.TransportMode.Code);
				AssertEquals("whsOrderData.TransportMode.Description", "Sea Freight", whsOrderData.TransportMode.Description);
				AssertEquals("whsOrderData.PortOfOrigin.Code", "AUMEL", whsOrderData.PortOfOrigin.Code);
				AssertEquals("whsOrderData.PortOfOrigin.Name", "Melbourne", whsOrderData.PortOfOrigin.Name);
				AssertEquals("whsOrderData.PortOfLoading", null, whsOrderData.PortOfLoading);
				AssertEquals("whsOrderData.PortOfFirstArrival", null, whsOrderData.PortOfFirstArrival);
				AssertEquals("whsOrderData.PortOfDischarge", null, whsOrderData.PortOfDischarge);
				AssertEquals("whsOrderData.PortOfDestination.Code", "AUBNE", whsOrderData.PortOfDestination.Code);
				AssertEquals("whsOrderData.PortOfDestination.Name", "Brisbane", whsOrderData.PortOfDestination.Name);

				AssertEquals("whsOrderData.VesselName", null, whsOrderData.VesselName);
				AssertEquals("whsOrderData.VoyageFlightNo", null, whsOrderData.VoyageFlightNo);
				AssertEquals("whsOrderData.WarehouseLocation", null, whsOrderData.WarehouseLocation);
				AssertEquals("whsOrderData.WarehouseReleaseStatus", null, whsOrderData.WarehouseReleaseStatus);
				AssertEquals("whsOrderData.WayBillNumber", "BILL", whsOrderData.WayBillNumber);
				AssertEquals("whsOrderData.WayBillType.Code", "HWB", whsOrderData.WayBillType.Code);
				AssertEquals("whsOrderData.WayBillType.Description", "House Waybill", whsOrderData.WayBillType.Description);
				AssertEquals("whsOrderData.IsAuthorisedToLeave", true, whsOrderData.IsAuthorizedToLeave);

				var localProcessing = whsOrderData.LocalProcessing;
				AssertEquals("localProcessing.DeliveryRequiredBy", requiredByDate.ToZDateTime(), localProcessing.DeliveryRequiredBy);

				var order = whsOrderData.Order;
				AssertEquals("order.AddPalletWeightToOrder", ZBool.True, order.AddPalletWeightToOrder);
				AssertEquals("order.RequiresQualityAudit", ZBool.True, order.RequiresQualityAudit);
				AssertEquals("order.PackingRequired", ZBool.True, order.RequiresPacking);
				AssertEquals("order.ExcludeFromTotePicking", ZBool.True, order.ExcludeFromTotePicking);
				AssertEquals("order.ClientReference", "CUSTOMER", order.ClientReference);
				AssertEquals("order.TotalLineVolume", 23.3m, order.TotalLineVolume);
				AssertEquals("order.DropMode.Code", "HSL", order.DropMode.Code);
				AssertEquals("order.DropMode.Description", "Haulier Supplies Lift", order.DropMode.Description);
				AssertEquals("order.LocalCartageInsuranceValue", 13.4m, order.LocalCartageInsuranceValue);
				AssertEquals("order.TotalNetWeightSent", 7.9m, order.TotalNetWeightSent);
				AssertEquals("order.OrderNumber", "ORDER123", order.OrderNumber);
				AssertEquals("order.OrderNumberSplit", new ZByte(1), order.OrderNumberSplit);
				AssertEquals("order.PickOption.Code", "AUT", order.PickOption.Code);
				AssertEquals("order.PickOption.Description", "Auto Pick", order.PickOption.Description);
				AssertEquals("order.StagingArea", "DockA", order.StagingArea);

				AssertEquals("order.Status.Code", "PIC", order.Status.Code);
				AssertEquals("order.Status.Description", "Picking", order.Status.Description);
				AssertEquals("order.TotalUnits", 7.1m, order.TotalUnits);
				AssertEquals("order.TotalLineWeight", 45.8m, order.TotalLineWeight);
				AssertEquals("order.TransportReference", "TREEE", order.TransportReference);
				AssertEquals("order.Type.Code", "CUS", order.Type.Code);
				AssertEquals("order.Type.Description", "CUSTOMS RELEASE", order.Type.Description);
				AssertEquals("order.UnitsSent", 12.1m, order.UnitsSent);
				AssertEquals("order.Warehouse.Code", "WHS", order.Warehouse.Code);
				AssertEquals("order.Warehouse.Name", "Ware this!", order.Warehouse.Name);
				AssertEquals("order.FulfillmentRule.Code", "NON", order.FulfillmentRule.Code);
				AssertEquals("order.FulfillmentRule.Description", "None", order.FulfillmentRule.Description);

				var salesChannel = order.SalesChannel;
				AssertEquals("order.SalesChannel.Code", "SAL", salesChannel.Code);
				AssertEquals("order.SalesChannel.Description", "SalesTest", salesChannel.Description);

				var carrierAccountNumber = whsOrderData.CarrierAccount;
				AssertEquals("carrierAccountNumber.AccountNumber", "TR1SONYSYD", carrierAccountNumber.AccountNumber);
				AssertEquals("carrierAccountNumber.DepotID", "DPO001", carrierAccountNumber.DepotID);
				AssertEquals("carrierAccountNumber.MerchantNumber", "MERCH001", carrierAccountNumber.MerchantNumber);
			});

			#endregion
		}

		#endregion

		#region TestCarrierAccount

		public void TestCarrierAccount_SalesChannel()
		{
			TestCarrierAccount_SalesChannelCore(hasSalesChannel: true);
		}

		public void TestCarrierAccount_NoSalesChannel()
		{
			TestCarrierAccount_SalesChannelCore(hasSalesChannel: false);
		}

		void TestCarrierAccount_SalesChannelCore(bool hasSalesChannel)
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Ware this!";
			whsOrderBO.WD_WW_Whs = warehouse.PK;

			var client = Factory.NewWithValidTestData<OrgHeader>();
			whsOrderBO.WD_OH_Client = client.PK;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "ABC";
			whsOrderBO.TransportCoPK = carrier.PK;

			var carrierAccount1 = carrier.CarrierAccounts.AddNew();
			carrierAccount1.OAN_AccountNumber = "TestingNum";
			carrierAccount1.OAN_DepotID = "Depot";
			carrierAccount1.OAN_MerchantNumber = "Mech";

			var whsSalesChannel = Helper.CreateWhsSalesChannel("TST", "Testing");
			whsOrderBO.WD_WSH_SalesChannel = whsSalesChannel.PK;

			var whsCANSonySyd = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonySyd.OWC_WW_Warehouse = warehouse.PK;
			whsCANSonySyd.OWC_OAN_CarrierAccount = carrierAccount1.PK;
			if (hasSalesChannel)
			{
				whsCANSonySyd.OWC_WSH_SalesChannel = whsSalesChannel.PK;
			}

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);
			AssertNotNull("whsOrderData", whsOrderData);

			var carrierAccountNumber = whsOrderData.CarrierAccount;
			AssertEquals("carrierAccountNumber.AccountNumber", "TestingNum", carrierAccountNumber.AccountNumber);
			AssertEquals("carrierAccountNumber.DepotID", "Depot", carrierAccountNumber.DepotID);
			AssertEquals("carrierAccountNumber.MerchantNumber", "Mech", carrierAccountNumber.MerchantNumber);
		}

		#endregion

		#region TestSalesChannel

		public void TestSalesChannel()
		{
			TestSalesChannelCore(hasSalesChannel: true);
		}

		public void TestSalesChannel_NoSalesChannel()
		{
			TestSalesChannelCore(hasSalesChannel: false);
		}

		void TestSalesChannelCore(bool hasSalesChannel)
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Ware this!";
			whsOrderBO.WD_WW_Whs = warehouse.PK;

			var client = Factory.NewWithValidTestData<OrgHeader>();
			whsOrderBO.WD_OH_Client = client.PK;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "ABC";
			whsOrderBO.TransportCoPK = carrier.PK;

			var carrierAccount1 = carrier.CarrierAccounts.AddNew();
			carrierAccount1.OAN_AccountNumber = "TestingNum";
			carrierAccount1.OAN_DepotID = "Depot";
			carrierAccount1.OAN_MerchantNumber = "Mech";

			var whsSalesChannel = Helper.CreateWhsSalesChannel("TST", "Testing");
			if (hasSalesChannel)
			{
				whsOrderBO.WD_WSH_SalesChannel = whsSalesChannel.PK;
			}

			var whsCANSonySyd = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonySyd.OWC_WW_Warehouse = warehouse.PK;
			whsCANSonySyd.OWC_OAN_CarrierAccount = carrierAccount1.PK;
			whsCANSonySyd.OWC_WSH_SalesChannel = whsSalesChannel.PK;

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);

			AssertNotNull("whsOrderData", whsOrderData);

			var order = whsOrderData.Order;
			if (hasSalesChannel)
			{
				var salesChannel = order.SalesChannel;
				AssertEquals("order.SalesChannel.Code", "TST", salesChannel.Code);
				AssertEquals("order.SalesChannel.Description", "Testing", salesChannel.Description);
			}
			else
			{
				AssertNull(whsOrderData.Order.SalesChannel);
			}
		}

		#endregion

		#region TestCarrierAccountNumber_WithTPCReference

		public void TestCarrierAccountNumber_WithTPCReference()
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Ware this!";
			whsOrderBO.WD_WW_Whs = warehouse.PK;

			var client = Helper.CreateClient("C1");
			whsOrderBO.WD_OH_Client = client.PK;

			var consignee = Helper.CreateClient("CONSIGNEE");
			whsOrderBO.ConsigneeAddressPK = consignee.MainAddress.PK;

			var carrier = Helper.CreateClient("CARRIER");
			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "ABC";
			whsOrderBO.TransportCoPK = carrier.PK;

			var tpcReference = whsOrderBO.References.AddNew();
			tpcReference.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber;
			tpcReference.WX_Reference = "111222333";

			var tranportBillTo = Helper.CreateClient("BILLER");
			whsOrderBO.TransportBillToDocAddress.E2_AddressOverride = false;
			whsOrderBO.TransportBillToDocAddress.OrganisationPK = tranportBillTo.PK;
			AssertEquals(tranportBillTo.PK, whsOrderBO.TransportBillTo.PK);

			var carrierAccount = carrier.CarrierAccounts.AddNew();
			carrierAccount.OAN_AccountNumber = "TestingNum";
			carrierAccount.OAN_DepotID = "Depot";
			carrierAccount.OAN_MerchantNumber = "Mech";

			var whsSalesChannel = Helper.CreateWhsSalesChannel("TST", "Testing");
			whsOrderBO.WD_WSH_SalesChannel = whsSalesChannel.PK;

			var whsCANSonySyd = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonySyd.OWC_WW_Warehouse = warehouse.PK;
			whsCANSonySyd.OWC_OAN_CarrierAccount = carrierAccount.PK;
			whsCANSonySyd.OWC_WSH_SalesChannel = whsSalesChannel.PK;
			whsCANSonySyd.OWC_BillingType = CarrierBillingType.BillSender;

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);
			AssertNotNull("whsOrderData", whsOrderData);

			var exportedCarrierAccount = whsOrderData.CarrierAccount;
			AssertEquals("carrierAccountNumber.AccountNumber", "TestingNum", exportedCarrierAccount.AccountNumber);
			AssertEquals("carrierAccountNumber.DepotID", "Depot", exportedCarrierAccount.DepotID);
			AssertEquals("carrierAccountNumber.MerchantNumber", "Mech", exportedCarrierAccount.MerchantNumber);

			AssertEquals("Should override CarrierAccountBillingType when TPC specified", CarrierBillingType.BillThirdParty, whsOrderData.CarrierAccountBillingType);
			var payerCarrierAccountNumber = whsOrderData.AdditionalCarrierAccountCollection.Single();
			AssertEquals("Should have a BillToParty when TPC specified", "111222333", payerCarrierAccountNumber.AccountNumber);
			AssertEquals("Should have a BillToParty when TPC specified", tranportBillTo.OH_Code, payerCarrierAccountNumber.BillToParty);
			AssertEquals("Should have a BillToParty when TPC specified", CarrierAccountType.BillPayer, payerCarrierAccountNumber.CarrierAccountType);

			var transportBillToAddresses = whsOrderData.OrganizationAddressCollection.Where(addr => addr.AddressType.Equals(nameof(DocAddressType.TransportBillToAddress)));
			AssertEquals("Count of transportBillToAddress", 1, transportBillToAddresses.Count());
			AssertEquals("Should be a transportBillToAddress when it is specified", whsOrderBO.TransportBillTo.MainAddress.CompanyName, transportBillToAddresses.FirstOrDefault().CompanyName);
		}

		public void TestCarrierAccountNumber_WithTPCReference_OverridenTransportBillToAddress()
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Ware this!";
			whsOrderBO.WD_WW_Whs = warehouse.PK;

			var client = Helper.CreateClient("C1");
			whsOrderBO.WD_OH_Client = client.PK;

			var consignee = Helper.CreateClient("CONSIGNEE");
			whsOrderBO.ConsigneeAddressPK = consignee.MainAddress.PK;

			var carrier = Helper.CreateClient("CARRIER");
			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "ABC";
			whsOrderBO.TransportCoPK = carrier.PK;

			var tpcReference = whsOrderBO.References.AddNew();
			tpcReference.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber;
			tpcReference.WX_Reference = "111222333";

			whsOrderBO.TransportBillToDocAddress.E2_AddressOverride = true;
			whsOrderBO.TransportBillToDocAddress.E2_CompanyName = "Test Company";
			whsOrderBO.TransportBillToDocAddress.E2_Address1 = "Test Address";
			whsOrderBO.TransportBillToDocAddress.E2_City = "Sydney";
			whsOrderBO.TransportBillToDocAddress.E2_State = "NSW";
			whsOrderBO.TransportBillToDocAddress.E2_RN_NKCountryCode = "AU";
			AssertNull(whsOrderBO.TransportBillTo);
			Assert(!whsOrderBO.TransportBillToDocAddress.IsEmpty);

			var carrierAccount = carrier.CarrierAccounts.AddNew();
			carrierAccount.OAN_AccountNumber = "TestingNum";
			carrierAccount.OAN_DepotID = "Depot";
			carrierAccount.OAN_MerchantNumber = "Mech";

			var whsSalesChannel = Helper.CreateWhsSalesChannel("TST", "Testing");
			whsOrderBO.WD_WSH_SalesChannel = whsSalesChannel.PK;

			var whsCANSonySyd = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonySyd.OWC_WW_Warehouse = warehouse.PK;
			whsCANSonySyd.OWC_OAN_CarrierAccount = carrierAccount.PK;
			whsCANSonySyd.OWC_WSH_SalesChannel = whsSalesChannel.PK;
			whsCANSonySyd.OWC_BillingType = CarrierBillingType.BillSender;

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);
			AssertNotNull("whsOrderData", whsOrderData);

			var exportedCarrierAccount = whsOrderData.CarrierAccount;
			AssertEquals("carrierAccountNumber.AccountNumber", "TestingNum", exportedCarrierAccount.AccountNumber);
			AssertEquals("carrierAccountNumber.DepotID", "Depot", exportedCarrierAccount.DepotID);
			AssertEquals("carrierAccountNumber.MerchantNumber", "Mech", exportedCarrierAccount.MerchantNumber);

			AssertEquals("Should override CarrierAccountBillingType when TPC specified", CarrierBillingType.BillThirdParty, whsOrderData.CarrierAccountBillingType);
			var payerCarrierAccountNumber = whsOrderData.AdditionalCarrierAccountCollection.Single();
			AssertEquals("Should have a BillToParty when TPC specified", "111222333", payerCarrierAccountNumber.AccountNumber);
			AssertEquals("Should have a BillToParty when TPC specified", "Test Company", payerCarrierAccountNumber.BillToParty);
			AssertEquals("Should have a BillToParty when TPC specified", CarrierAccountType.BillPayer, payerCarrierAccountNumber.CarrierAccountType);

			var transportBillToAddresses = whsOrderData.OrganizationAddressCollection.Where(addr => addr.AddressType.Equals(nameof(DocAddressType.TransportBillToAddress)));
			AssertEquals("Count of transportBillToAddress", 1, transportBillToAddresses.Count());
			AssertEquals("Should be a transportBillToAddress when it is specified", "Test Company", transportBillToAddresses.First().CompanyName);
		}

		public void TestCarrierAccountNumber_WithTPCReference_OverridesConfiguredBillToParty()
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Ware this!";
			whsOrderBO.WD_WW_Whs = warehouse.PK;

			var client = Helper.CreateClient("C1");
			whsOrderBO.WD_OH_Client = client.PK;

			var consignee = Helper.CreateClient("CONSIGNEE");
			whsOrderBO.ConsigneeAddressPK = consignee.MainAddress.PK;

			var carrier = Helper.CreateClient("CARRIER");
			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "ABC";
			whsOrderBO.TransportCoPK = carrier.PK;

			var tpcReference = whsOrderBO.References.AddNew();
			tpcReference.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber;
			tpcReference.WX_Reference = "111222333";

			var tranportBillTo = Helper.CreateClient("BILLER");
			whsOrderBO.TransportBillToDocAddress.E2_AddressOverride = false;
			whsOrderBO.TransportBillToDocAddress.OrganisationPK = tranportBillTo.PK;
			AssertEquals(tranportBillTo.PK, whsOrderBO.TransportBillTo.PK);

			var carrierAccount = carrier.CarrierAccounts.AddNew();
			carrierAccount.OAN_AccountNumber = "TestingNum";
			carrierAccount.OAN_DepotID = "Depot";
			carrierAccount.OAN_MerchantNumber = "Mech";

			var payerCarrierAccount = carrier.CarrierAccounts.AddNew();
			payerCarrierAccount.OAN_AccountNumber = "YouPay123";
			payerCarrierAccount.OAN_DepotID = "AnyDepot";
			payerCarrierAccount.OAN_MerchantNumber = "AnyMech";
			payerCarrierAccount.OAN_OH_BillToParty = consignee.PK;

			var whsSalesChannel = Helper.CreateWhsSalesChannel("TST", "Testing");
			whsOrderBO.WD_WSH_SalesChannel = whsSalesChannel.PK;

			var whsCANSonySyd = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonySyd.OWC_WW_Warehouse = warehouse.PK;
			whsCANSonySyd.OWC_OAN_CarrierAccount = carrierAccount.PK;
			whsCANSonySyd.OWC_WSH_SalesChannel = whsSalesChannel.PK;
			whsCANSonySyd.OWC_BillingType = CarrierBillingType.BillReceiver;
			whsCANSonySyd.OWC_OAN_BillToCarrierAccount = payerCarrierAccount.PK;

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);
			AssertNotNull("whsOrderData", whsOrderData);

			var exportedCarrierAccount = whsOrderData.CarrierAccount;
			AssertEquals("carrierAccountNumber.AccountNumber", "TestingNum", exportedCarrierAccount.AccountNumber);
			AssertEquals("carrierAccountNumber.DepotID", "Depot", exportedCarrierAccount.DepotID);
			AssertEquals("carrierAccountNumber.MerchantNumber", "Mech", exportedCarrierAccount.MerchantNumber);

			AssertEquals("Should override CarrierAccountBillingType when TPC specified", CarrierBillingType.BillThirdParty, whsOrderData.CarrierAccountBillingType);
			var payerCarrierAccountNumber = whsOrderData.AdditionalCarrierAccountCollection.Single();
			AssertEquals("Should have a BillToParty when TPC specified", "111222333", payerCarrierAccountNumber.AccountNumber);
			AssertEquals("Should have a BillToParty when TPC specified", tranportBillTo.OH_Code, payerCarrierAccountNumber.BillToParty);
			AssertEquals("Should have a BillToParty when TPC specified", CarrierAccountType.BillPayer, payerCarrierAccountNumber.CarrierAccountType);

			var transportBillToAddresses = whsOrderData.OrganizationAddressCollection.Where(addr => addr.AddressType.Equals(nameof(DocAddressType.TransportBillToAddress)));
			AssertEquals("Count of transportBillToAddress", 1, transportBillToAddresses.Count());
			AssertEquals("Should be a transportBillToAddress when it is specified", whsOrderBO.TransportBillTo.MainAddress.CompanyName, transportBillToAddresses.FirstOrDefault().CompanyName);
		}

		public void TestCarrierAccountNumber_WithTPCReference_NoConfiguredOrgWhsClientAccountAssociation()
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Ware this!";
			whsOrderBO.WD_WW_Whs = warehouse.PK;

			var client = Helper.CreateClient("C1");
			whsOrderBO.WD_OH_Client = client.PK;

			var consignee = Helper.CreateClient("CONSIGNEE");
			whsOrderBO.ConsigneeAddressPK = consignee.MainAddress.PK;

			var carrier = Helper.CreateClient("CARRIER");
			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "ABC";
			whsOrderBO.TransportCoPK = carrier.PK;

			var tpcReference = whsOrderBO.References.AddNew();
			tpcReference.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber;
			tpcReference.WX_Reference = "111222333";

			var tranportBillTo = Helper.CreateClient("BILLER");
			whsOrderBO.TransportBillToDocAddress.E2_AddressOverride = false;
			whsOrderBO.TransportBillToDocAddress.OrganisationPK = tranportBillTo.PK;
			AssertEquals(tranportBillTo.PK, whsOrderBO.TransportBillTo.PK);

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);
			AssertNotNull("whsOrderData", whsOrderData);

			AssertNull(whsOrderData.CarrierAccount);
			AssertNull(whsOrderData.AdditionalCarrierAccountCollection);
		}

		#endregion

		public void TestPayerCarrierAccountAndTransportToAddress_WithTransportBillTo_BillReceiver()
		{
			TestPayerCarrierAccountAndTransportToAddress_WithTransportBillToCore(CarrierBillingType.BillReceiver);
		}

		public void TestPayerCarrierAccountAndTransportToAddress_WithTransportBillTo_BillThirdParty()
		{
			TestPayerCarrierAccountAndTransportToAddress_WithTransportBillToCore(CarrierBillingType.BillThirdParty);
		}

		void TestPayerCarrierAccountAndTransportToAddress_WithTransportBillToCore(string billingType)
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Ware this!";
			whsOrderBO.WD_WW_Whs = warehouse.PK;

			var client = Helper.CreateClient("C1");
			whsOrderBO.WD_OH_Client = client.PK;

			var consignee = Helper.CreateClient("PAYER");
			whsOrderBO.ConsigneeAddressPK = consignee.MainAddress.PK;

			var carrier = Helper.CreateClient("CARRIER");
			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "ABC";
			whsOrderBO.TransportCoPK = carrier.PK;

			var carrierAccount = carrier.CarrierAccounts.AddNew();
			carrierAccount.OAN_AccountNumber = "TestingNum";
			carrierAccount.OAN_DepotID = "Depot";
			carrierAccount.OAN_MerchantNumber = "Mech";

			var tranportBillTo = Helper.CreateClient("BILLER");
			whsOrderBO.TransportBillToDocAddress.E2_AddressOverride = false;
			whsOrderBO.TransportBillToDocAddress.OrganisationPK = tranportBillTo.PK;
			AssertEquals(tranportBillTo.PK, whsOrderBO.TransportBillTo.PK);

			var payerCarrierAccount = carrier.CarrierAccounts.AddNew();
			payerCarrierAccount.OAN_AccountNumber = "YouPay123";
			payerCarrierAccount.OAN_DepotID = "AnyDepot";
			payerCarrierAccount.OAN_MerchantNumber = "AnyMech";
			payerCarrierAccount.OAN_OH_BillToParty = consignee.PK;

			var whsSalesChannel = Helper.CreateWhsSalesChannel("TST", "Testing");
			whsOrderBO.WD_WSH_SalesChannel = whsSalesChannel.PK;

			var whsCANSonySyd = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonySyd.OWC_WW_Warehouse = warehouse.PK;
			whsCANSonySyd.OWC_OAN_CarrierAccount = carrierAccount.PK;
			whsCANSonySyd.OWC_WSH_SalesChannel = whsSalesChannel.PK;
			whsCANSonySyd.OWC_BillingType = billingType;
			whsCANSonySyd.OWC_OAN_BillToCarrierAccount = payerCarrierAccount.PK;

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);
			AssertNotNull("whsOrderData", whsOrderData);
			AssertEquals("whsOrderData.CarrierAccountBillingType", billingType, whsOrderData.CarrierAccountBillingType);

			AssertEquals(1, whsOrderData.AdditionalCarrierAccountCollection.Count);
			var payerCarrierAccountNumber = whsOrderData.AdditionalCarrierAccountCollection.Single();
			AssertEquals("payerCarrierAccountNumber.AccountNumber", "YouPay123", payerCarrierAccountNumber.AccountNumber);
			AssertEquals("payerCarrierAccountNumber.DepotID", "AnyDepot", payerCarrierAccountNumber.DepotID);
			AssertEquals("payerCarrierAccountNumber.MerchantNumber", "AnyMech", payerCarrierAccountNumber.MerchantNumber);
			AssertEquals("payerCarrierAccountNumber.ClientReference", "PAYER", payerCarrierAccountNumber.BillToParty);
			AssertEquals("payerCarrierAccountNumber.CarrierAccountType", "BillPayer", payerCarrierAccountNumber.CarrierAccountType);

			var transportBillToAddresses = whsOrderData.OrganizationAddressCollection.Where(addr => addr.AddressType.Equals(nameof(DocAddressType.TransportBillToAddress)));

			AssertEquals("Count of transportBillToAddress", 1, transportBillToAddresses.Count());
			AssertEquals("Should always be TransportBillTo when it is specified", whsOrderBO.TransportBillTo.MainAddress.CompanyName, transportBillToAddresses.FirstOrDefault().CompanyName);
		}

		public void TestPayerCarrierAccountAndTransportToAddress_WithTransportBillTo_BillSender()
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Ware this!";
			whsOrderBO.WD_WW_Whs = warehouse.PK;

			var client = Helper.CreateClient("C1");
			whsOrderBO.WD_OH_Client = client.PK;

			var consignee = Helper.CreateClient("CONSIGNEE");
			whsOrderBO.ConsigneeAddressPK = consignee.MainAddress.PK;

			var carrier = Helper.CreateClient("CARRIER");
			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "ABC";
			whsOrderBO.TransportCoPK = carrier.PK;

			var tranportBillTo = Helper.CreateClient("BILLER");
			whsOrderBO.TransportBillToDocAddress.E2_AddressOverride = false;
			whsOrderBO.TransportBillToDocAddress.OrganisationPK = tranportBillTo.PK;
			AssertEquals(tranportBillTo.PK, whsOrderBO.TransportBillTo.PK);

			var carrierAccount = carrier.CarrierAccounts.AddNew();
			carrierAccount.OAN_AccountNumber = "TestingNum";
			carrierAccount.OAN_DepotID = "Depot";
			carrierAccount.OAN_MerchantNumber = "Mech";

			var whsSalesChannel = Helper.CreateWhsSalesChannel("TST", "Testing");
			whsOrderBO.WD_WSH_SalesChannel = whsSalesChannel.PK;

			var whsCANSonySyd = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonySyd.OWC_WW_Warehouse = warehouse.PK;
			whsCANSonySyd.OWC_OAN_CarrierAccount = carrierAccount.PK;
			whsCANSonySyd.OWC_WSH_SalesChannel = whsSalesChannel.PK;

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);
			AssertNotNull("whsOrderData", whsOrderData);

			AssertEquals("whsOrderData.CarrierAccountBillingType", CarrierBillingType.BillSender, whsOrderData.CarrierAccountBillingType);
			AssertNull("whsOrderData.AdditionalCarrierAccountsCollection", whsOrderData.AdditionalCarrierAccountCollection);

			var transportBillToAddresses = whsOrderData.OrganizationAddressCollection.Where(addr => addr.AddressType.Equals(nameof(DocAddressType.TransportBillToAddress)));

			AssertEquals("Count of transportBillToAddress", 1, transportBillToAddresses.Count());
			AssertEquals("Should always be TransportBillTo when it is specified", whsOrderBO.TransportBillTo.MainAddress.CompanyName, transportBillToAddresses.FirstOrDefault().CompanyName);
		}

		public void TestPayerCarrierAccountAndTransportToAddress_NoTransportBillTo_BillReceiver()
		{
			TestPayerCarrierAccountAndTransportToAddress_NoTransportBillToCore(CarrierBillingType.BillReceiver);
		}

		public void TestPayerCarrierAccountAndTransportToAddress_NoTransportBillTo_BillThirdParty()
		{
			TestPayerCarrierAccountAndTransportToAddress_NoTransportBillToCore(CarrierBillingType.BillThirdParty);
		}

		void TestPayerCarrierAccountAndTransportToAddress_NoTransportBillToCore(string billingType)
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Ware this!";
			whsOrderBO.WD_WW_Whs = warehouse.PK;

			var client = Helper.CreateClient("C1");
			whsOrderBO.WD_OH_Client = client.PK;

			var consignee = Helper.CreateClient("PAYER");
			whsOrderBO.ConsigneeAddressPK = consignee.MainAddress.PK;

			var carrier = Helper.CreateClient("CARRIER");
			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "ABC";
			whsOrderBO.TransportCoPK = carrier.PK;

			var carrierAccount = carrier.CarrierAccounts.AddNew();
			carrierAccount.OAN_AccountNumber = "TestingNum";
			carrierAccount.OAN_DepotID = "Depot";
			carrierAccount.OAN_MerchantNumber = "Mech";

			var payerCarrierAccount = carrier.CarrierAccounts.AddNew();
			payerCarrierAccount.OAN_AccountNumber = "YouPay123";
			payerCarrierAccount.OAN_DepotID = "AnyDepot";
			payerCarrierAccount.OAN_MerchantNumber = "AnyMech";
			payerCarrierAccount.OAN_OH_BillToParty = consignee.PK;

			var whsSalesChannel = Helper.CreateWhsSalesChannel("TST", "Testing");
			whsOrderBO.WD_WSH_SalesChannel = whsSalesChannel.PK;

			var whsCANSonySyd = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonySyd.OWC_WW_Warehouse = warehouse.PK;
			whsCANSonySyd.OWC_OAN_CarrierAccount = carrierAccount.PK;
			whsCANSonySyd.OWC_WSH_SalesChannel = whsSalesChannel.PK;
			whsCANSonySyd.OWC_BillingType = billingType;
			whsCANSonySyd.OWC_OAN_BillToCarrierAccount = payerCarrierAccount.PK;

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);
			AssertNotNull("whsOrderData", whsOrderData);
			AssertEquals("whsOrderData.CarrierAccountBillingType", billingType, whsOrderData.CarrierAccountBillingType);

			AssertEquals(1, whsOrderData.AdditionalCarrierAccountCollection.Count);
			var payerCarrierAccountNumber = whsOrderData.AdditionalCarrierAccountCollection.Single();
			AssertEquals("payerCarrierAccountNumber.AccountNumber", "YouPay123", payerCarrierAccountNumber.AccountNumber);
			AssertEquals("payerCarrierAccountNumber.DepotID", "AnyDepot", payerCarrierAccountNumber.DepotID);
			AssertEquals("payerCarrierAccountNumber.MerchantNumber", "AnyMech", payerCarrierAccountNumber.MerchantNumber);
			AssertEquals("payerCarrierAccountNumber.ClientReference", "PAYER", payerCarrierAccountNumber.BillToParty);
			AssertEquals("payerCarrierAccountNumber.CarrierAccountType", "BillPayer", payerCarrierAccountNumber.CarrierAccountType);

			var transportBillToAddresses = whsOrderData.OrganizationAddressCollection.Where(addr => addr.AddressType.Equals(nameof(DocAddressType.TransportBillToAddress)));
			var expectedTransportBillToAddress = billingType == CarrierBillingType.BillReceiver ? consignee.MainAddress : payerCarrierAccount.BillToParty.MainAddress;
			AssertEquals("TransportBillToAddress should depends on billing type when no transportBillTo is specified", expectedTransportBillToAddress.CompanyName, transportBillToAddresses.FirstOrDefault().CompanyName);
		}

		public void TestPayerCarrierAccountAndTransportToAddress_NoTransportBillTo_BillSender()
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Ware this!";
			whsOrderBO.WD_WW_Whs = warehouse.PK;

			var client = Helper.CreateClient("C1");
			whsOrderBO.WD_OH_Client = client.PK;

			var consignee = Helper.CreateClient("CONSIGNEE");
			whsOrderBO.ConsigneeAddressPK = consignee.MainAddress.PK;

			var carrier = Helper.CreateClient("CARRIER");
			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "ABC";
			whsOrderBO.TransportCoPK = carrier.PK;

			var carrierAccount = carrier.CarrierAccounts.AddNew();
			carrierAccount.OAN_AccountNumber = "TestingNum";
			carrierAccount.OAN_DepotID = "Depot";
			carrierAccount.OAN_MerchantNumber = "Mech";

			var whsSalesChannel = Helper.CreateWhsSalesChannel("TST", "Testing");
			whsOrderBO.WD_WSH_SalesChannel = whsSalesChannel.PK;

			var whsCANSonySyd = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonySyd.OWC_WW_Warehouse = warehouse.PK;
			whsCANSonySyd.OWC_OAN_CarrierAccount = carrierAccount.PK;
			whsCANSonySyd.OWC_WSH_SalesChannel = whsSalesChannel.PK;

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);
			AssertNotNull("whsOrderData", whsOrderData);

			AssertEquals("whsOrderData.CarrierAccountBillingType", CarrierBillingType.BillSender, whsOrderData.CarrierAccountBillingType);
			AssertNull("whsOrderData.AdditionalCarrierAccountsCollection", whsOrderData.AdditionalCarrierAccountCollection);
			var transportBillToAddresses = whsOrderData.OrganizationAddressCollection.Where(addr => addr.AddressType.Equals(nameof(DocAddressType.TransportBillToAddress)));
			AssertEquals("Should be no transportBillToAddress if BillSender and No specified TransportBillTo", 0, transportBillToAddresses.Count());
		}

		public void TestPayerCarrierAccount_BillReceiverBillingType_ConsigneeDifferentFromBillToParty()
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Ware this!";
			whsOrderBO.WD_WW_Whs = warehouse.PK;

			var client = Helper.CreateClient("C1");
			whsOrderBO.WD_OH_Client = client.PK;

			var consignee = Helper.CreateClient("CONSIGNEE");
			whsOrderBO.ConsigneeAddressPK = consignee.MainAddress.PK;

			var carrier = Helper.CreateClient("CARRIER");
			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "ABC";
			whsOrderBO.TransportCoPK = carrier.PK;

			var carrierAccount = carrier.CarrierAccounts.AddNew();
			carrierAccount.OAN_AccountNumber = "TestingNum";
			carrierAccount.OAN_DepotID = "Depot";
			carrierAccount.OAN_MerchantNumber = "Mech";

			var payer = Helper.CreateClient("PAYER");
			var payerCarrierAccount = carrier.CarrierAccounts.AddNew();
			payerCarrierAccount.OAN_AccountNumber = "YouPay123";
			payerCarrierAccount.OAN_DepotID = "AnyDepot";
			payerCarrierAccount.OAN_MerchantNumber = "AnyMech";
			payerCarrierAccount.OAN_OH_BillToParty = payer.PK;

			var whsSalesChannel = Helper.CreateWhsSalesChannel("TST", "Testing");
			whsOrderBO.WD_WSH_SalesChannel = whsSalesChannel.PK;

			var whsCANSonySyd = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonySyd.OWC_WW_Warehouse = warehouse.PK;
			whsCANSonySyd.OWC_OAN_CarrierAccount = carrierAccount.PK;
			whsCANSonySyd.OWC_WSH_SalesChannel = whsSalesChannel.PK;
			whsCANSonySyd.OWC_BillingType = CarrierBillingType.BillReceiver;
			whsCANSonySyd.OWC_OAN_BillToCarrierAccount = payerCarrierAccount.PK;

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);
			AssertNotNull("whsOrderData", whsOrderData);
			AssertEquals("whsOrderData.CarrierAccountBillingType", CarrierBillingType.BillReceiver, whsOrderData.CarrierAccountBillingType);

			AssertEquals(1, whsOrderData.AdditionalCarrierAccountCollection.Count);
			var payerCarrierAccountNumber = whsOrderData.AdditionalCarrierAccountCollection.Single();
			AssertEquals("payerCarrierAccountNumber.AccountNumber", "YouPay123", payerCarrierAccountNumber.AccountNumber);
			AssertEquals("payerCarrierAccountNumber.DepotID", "AnyDepot", payerCarrierAccountNumber.DepotID);
			AssertEquals("payerCarrierAccountNumber.MerchantNumber", "AnyMech", payerCarrierAccountNumber.MerchantNumber);
			AssertEquals("payerCarrierAccountNumber.ClientReference", "CONSIGNEE", payerCarrierAccountNumber.BillToParty);
			AssertEquals("payerCarrierAccountNumber.CarrierAccountType", "BillPayer", payerCarrierAccountNumber.CarrierAccountType);
		}

		public void TestPayerCarrierAccount_BillReceiverBillingType_OverridenConsigneeAddress()
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Ware this!";
			whsOrderBO.WD_WW_Whs = warehouse.PK;

			var client = Helper.CreateClient("C1");
			whsOrderBO.WD_OH_Client = client.PK;
			whsOrderBO.ConsigneeDocAddress.E2_AddressOverride = true;
			whsOrderBO.ConsigneeDocAddress.E2_CompanyName = "Random Consignee";
			whsOrderBO.ConsigneeDocAddress.E2_Address1 = "Test";
			AssertNull(whsOrderBO.Consignee);

			var carrier = Helper.CreateClient("CARRIER");
			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "ABC";
			whsOrderBO.TransportCoPK = carrier.PK;

			var carrierAccount = carrier.CarrierAccounts.AddNew();
			carrierAccount.OAN_AccountNumber = "TestingNum";
			carrierAccount.OAN_DepotID = "Depot";
			carrierAccount.OAN_MerchantNumber = "Mech";

			var payer = Helper.CreateClient("PAYER");
			var payerCarrierAccount = carrier.CarrierAccounts.AddNew();
			payerCarrierAccount.OAN_AccountNumber = "YouPay123";
			payerCarrierAccount.OAN_DepotID = "AnyDepot";
			payerCarrierAccount.OAN_MerchantNumber = "AnyMech";
			payerCarrierAccount.OAN_OH_BillToParty = payer.PK;

			var whsSalesChannel = Helper.CreateWhsSalesChannel("TST", "Testing");
			whsOrderBO.WD_WSH_SalesChannel = whsSalesChannel.PK;

			var whsCANSonySyd = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonySyd.OWC_WW_Warehouse = warehouse.PK;
			whsCANSonySyd.OWC_OAN_CarrierAccount = carrierAccount.PK;
			whsCANSonySyd.OWC_WSH_SalesChannel = whsSalesChannel.PK;
			whsCANSonySyd.OWC_BillingType = CarrierBillingType.BillReceiver;
			whsCANSonySyd.OWC_OAN_BillToCarrierAccount = payerCarrierAccount.PK;

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);
			AssertNotNull("whsOrderData", whsOrderData);
			AssertEquals("whsOrderData.CarrierAccountBillingType", CarrierBillingType.BillReceiver, whsOrderData.CarrierAccountBillingType);

			AssertEquals(1, whsOrderData.AdditionalCarrierAccountCollection.Count);
			var payerCarrierAccountNumber = whsOrderData.AdditionalCarrierAccountCollection.Single();
			AssertEquals("payerCarrierAccountNumber.AccountNumber", "YouPay123", payerCarrierAccountNumber.AccountNumber);
			AssertEquals("payerCarrierAccountNumber.DepotID", "AnyDepot", payerCarrierAccountNumber.DepotID);
			AssertEquals("payerCarrierAccountNumber.MerchantNumber", "AnyMech", payerCarrierAccountNumber.MerchantNumber);
			AssertEquals("payerCarrierAccountNumber.ClientReference", "C1", payerCarrierAccountNumber.BillToParty);
			AssertEquals("payerCarrierAccountNumber.CarrierAccountType", "BillPayer", payerCarrierAccountNumber.CarrierAccountType);
		}

		public void TestDutyPayerCarrierAccount()
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Ware this!";
			whsOrderBO.WD_WW_Whs = warehouse.PK;

			var client = Helper.CreateClient("CLIENT");
			whsOrderBO.WD_OH_Client = client.PK;

			var consignee = Helper.CreateClient("CONSIGNEE");
			whsOrderBO.ConsigneeAddressPK = consignee.MainAddress.PK;

			var carrier = Helper.CreateClient("CARRIER");
			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "ABC";
			whsOrderBO.TransportCoPK = carrier.PK;

			var carrierAccount = carrier.CarrierAccounts.AddNew();
			carrierAccount.OAN_AccountNumber = "TestingNum";
			carrierAccount.OAN_DepotID = "Depot";
			carrierAccount.OAN_MerchantNumber = "Mech";

			var payer = Helper.CreateClient("PAYER");
			var dutyPayerCarrierAccount = carrier.CarrierAccounts.AddNew();
			dutyPayerCarrierAccount.OAN_AccountNumber = "YouPay123";
			dutyPayerCarrierAccount.OAN_DepotID = "AnyDepot";
			dutyPayerCarrierAccount.OAN_MerchantNumber = "AnyMech";
			dutyPayerCarrierAccount.OAN_OH_BillToParty = payer.PK;

			var whsSalesChannel = Helper.CreateWhsSalesChannel("TST", "Testing");
			whsOrderBO.WD_WSH_SalesChannel = whsSalesChannel.PK;

			var whsCANSonySyd = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonySyd.OWC_WW_Warehouse = warehouse.PK;
			whsCANSonySyd.OWC_OAN_CarrierAccount = carrierAccount.PK;
			whsCANSonySyd.OWC_WSH_SalesChannel = whsSalesChannel.PK;
			whsCANSonySyd.OWC_OAN_DutyBillToCarrierAccount = dutyPayerCarrierAccount.PK;

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);
			AssertNotNull("whsOrderData", whsOrderData);
			AssertEquals("whsOrderData.CarrierAccountBillingType", CarrierBillingType.BillSender, whsOrderData.CarrierAccountBillingType);

			AssertEquals(1, whsOrderData.AdditionalCarrierAccountCollection.Count);
			var dutyPayerCarrierAccountNumber = whsOrderData.AdditionalCarrierAccountCollection.Single();
			AssertEquals("dutyPayerCarrierAccountNumber.AccountNumber", "YouPay123", dutyPayerCarrierAccountNumber.AccountNumber);
			AssertEquals("dutyPayerCarrierAccountNumber.DepotID", "AnyDepot", dutyPayerCarrierAccountNumber.DepotID);
			AssertEquals("dutyPayerCarrierAccountNumber.MerchantNumber", "AnyMech", dutyPayerCarrierAccountNumber.MerchantNumber);
			AssertEquals("dutyPayerCarrierAccountNumber.ClientReference", "PAYER", dutyPayerCarrierAccountNumber.BillToParty);
			AssertEquals("dutyPayerCarrierAccountNumber.CarrierAccountType", "DutyBillPayer", dutyPayerCarrierAccountNumber.CarrierAccountType);
		}

		public void TestDutyPayerCarrierAccount_NoDutyPayer()
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Ware this!";
			whsOrderBO.WD_WW_Whs = warehouse.PK;

			var client = Helper.CreateClient("C1");
			whsOrderBO.WD_OH_Client = client.PK;

			var consignee = Helper.CreateClient("CONSIGNEE");
			whsOrderBO.ConsigneeAddressPK = consignee.MainAddress.PK;

			var carrier = Helper.CreateClient("CARRIER");
			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "ABC";
			whsOrderBO.TransportCoPK = carrier.PK;

			var carrierAccount = carrier.CarrierAccounts.AddNew();
			carrierAccount.OAN_AccountNumber = "TestingNum";
			carrierAccount.OAN_DepotID = "Depot";
			carrierAccount.OAN_MerchantNumber = "Mech";

			var whsSalesChannel = Helper.CreateWhsSalesChannel("TST", "Testing");
			whsOrderBO.WD_WSH_SalesChannel = whsSalesChannel.PK;

			var whsCANSonySyd = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonySyd.OWC_WW_Warehouse = warehouse.PK;
			whsCANSonySyd.OWC_OAN_CarrierAccount = carrierAccount.PK;
			whsCANSonySyd.OWC_WSH_SalesChannel = whsSalesChannel.PK;

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);
			AssertNotNull("whsOrderData", whsOrderData);
			AssertEquals("whsOrderData.CarrierAccountBillingType", CarrierBillingType.BillSender, whsOrderData.CarrierAccountBillingType);
			AssertNull("whsOrderData.AdditionalCarrierAccountsCollection", whsOrderData.AdditionalCarrierAccountCollection);
		}

		public void TestPayerCarrierAccountAndDutyPayerCarrierAccountTogether()
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Ware this!";
			whsOrderBO.WD_WW_Whs = warehouse.PK;

			var client = Helper.CreateClient("C1");
			whsOrderBO.WD_OH_Client = client.PK;

			var consignee = Helper.CreateClient("CONSIGNEE");
			whsOrderBO.ConsigneeAddressPK = consignee.MainAddress.PK;

			var carrier = Helper.CreateClient("CARRIER");
			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "ABC";
			whsOrderBO.TransportCoPK = carrier.PK;

			var carrierAccount = carrier.CarrierAccounts.AddNew();
			carrierAccount.OAN_AccountNumber = "TestingNum";
			carrierAccount.OAN_DepotID = "Depot";
			carrierAccount.OAN_MerchantNumber = "Mech";

			var payerCarrierAccount = carrier.CarrierAccounts.AddNew();
			payerCarrierAccount.OAN_AccountNumber = "YouPay123";
			payerCarrierAccount.OAN_DepotID = "AnyDepot";
			payerCarrierAccount.OAN_MerchantNumber = "AnyMech";
			payerCarrierAccount.OAN_OH_BillToParty = consignee.PK;

			var dutyPayer = Helper.CreateClient("PAYER");
			var dutyPayerCarrierAccount = carrier.CarrierAccounts.AddNew();
			dutyPayerCarrierAccount.OAN_AccountNumber = "PayDuty123";
			dutyPayerCarrierAccount.OAN_DepotID = "AnyDutyDepot";
			dutyPayerCarrierAccount.OAN_MerchantNumber = "AnyDutyMech";
			dutyPayerCarrierAccount.OAN_OH_BillToParty = dutyPayer.PK;

			var whsSalesChannel = Helper.CreateWhsSalesChannel("TST", "Testing");
			whsOrderBO.WD_WSH_SalesChannel = whsSalesChannel.PK;

			var whsCANSonySyd = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonySyd.OWC_WW_Warehouse = warehouse.PK;
			whsCANSonySyd.OWC_OAN_CarrierAccount = carrierAccount.PK;
			whsCANSonySyd.OWC_WSH_SalesChannel = whsSalesChannel.PK;
			whsCANSonySyd.OWC_BillingType = CarrierBillingType.BillThirdParty;
			whsCANSonySyd.OWC_OAN_BillToCarrierAccount = payerCarrierAccount.PK;
			whsCANSonySyd.OWC_OAN_DutyBillToCarrierAccount = dutyPayerCarrierAccount.PK;

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);
			AssertNotNull("whsOrderData", whsOrderData);
			AssertEquals("whsOrderData.CarrierAccountBillingType", CarrierBillingType.BillThirdParty, whsOrderData.CarrierAccountBillingType);

			AssertEquals(2, whsOrderData.AdditionalCarrierAccountCollection.Count);
			var payerCarrierAccountNumber = whsOrderData.AdditionalCarrierAccountCollection.Single(additionalCarrierAccount => additionalCarrierAccount.CarrierAccountType.Value == "BillPayer");
			AssertEquals("payerCarrierAccountNumber.AccountNumber", "YouPay123", payerCarrierAccountNumber.AccountNumber);
			AssertEquals("payerCarrierAccountNumber.DepotID", "AnyDepot", payerCarrierAccountNumber.DepotID);
			AssertEquals("payerCarrierAccountNumber.MerchantNumber", "AnyMech", payerCarrierAccountNumber.MerchantNumber);
			AssertEquals("payerCarrierAccountNumber.ClientReference", "CONSIGNEE", payerCarrierAccountNumber.BillToParty);

			var dutyPayerCarrierAccountNumber = whsOrderData.AdditionalCarrierAccountCollection.Single(additionalCarrierAccount => additionalCarrierAccount.CarrierAccountType.Value == "DutyBillPayer");
			AssertEquals("dutyPayerCarrierAccountNumber.AccountNumber", "PayDuty123", dutyPayerCarrierAccountNumber.AccountNumber);
			AssertEquals("dutyPayerCarrierAccountNumber.DepotID", "AnyDutyDepot", dutyPayerCarrierAccountNumber.DepotID);
			AssertEquals("dutyPayerCarrierAccountNumber.MerchantNumber", "AnyDutyMech", dutyPayerCarrierAccountNumber.MerchantNumber);
			AssertEquals("dutyPayerCarrierAccountNumber.ClientReference", "PAYER", dutyPayerCarrierAccountNumber.BillToParty);
		}

		#region TestCollections

		#region TestPalletsSent

		public void TestPalletsSent()
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			whsOrderBO.WD_PackagesSent = 5;
			whsOrderBO.WD_PalletsSent = new ZShort(4);

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);
			AssertEquals("order.PalletsSent", new ZShort(4), whsOrderData.Order.PalletsSent);
			AssertEquals("order.OuterPacks", new ZInt(4), whsOrderData.OuterPacks);
		}
		#endregion

		#region TestPopulateIsResidential

		public void TestPopulateIsResidential()
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			var consigneeAddress = Factory.NewWithValidTestData<JobDocAddress>();
			consigneeAddress.E2_AddressOverride = true;
			consigneeAddress.E2_IsResidential = true;
			whsOrderBO.DocAddresses.Add(consigneeAddress);

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);
			Assert("IsResidential was populated", whsOrderData.OrganizationAddressCollection[0].IsResidential.GetValueOrDefault());
		}

		#endregion

		#region TestConsignee

		public void TestConsignee()
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			whsOrderBO.ConsigneeDocAddress.E2_OA_Address = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).MainAddress.PK;

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);

			AssertNotNull("whsOrderData", whsOrderData);
			AssertEquals("whsOrderData.OrganizationAddressCollection.Count", 1, whsOrderData.OrganizationAddressCollection.Count);

			var consigneeAddress = whsOrderData.OrganizationAddressCollection.Single(o => o.AddressType.Value == "ConsigneeAddress");
			AssertOrganizationBO_CRAHOLSYD("ConsigneeAddress", consigneeAddress, "ConsigneeAddress", true);
		}

		#endregion

		#region TestForwarder

		public void TestForwarder()
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			whsOrderBO.WD_OH_Forwarder = GetOrganizationBO_INTHEMSYD(Factory.BOFactory).PK;

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);

			AssertNotNull("whsOrderData", whsOrderData);

			AssertEquals("whsOrderData.OrganizationAddressCollection.Count", 1, whsOrderData.OrganizationAddressCollection.Count);
			AssertOrganizationBO_INTHEMSYD("SendingForwarderAddress", whsOrderData.OrganizationAddressCollection[0], "SendingForwarderAddress");
		}

		#endregion

		#region TestPickupFrom

		public void TestPickupFrom()
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			whsOrderBO.WD_WW_Whs = Factory.NewWithValidTestData<WhsWarehouse>().PK;
			whsOrderBO.Warehouse.WW_OA_WarehouseAddress = GetOrganizationBO_INTHEMSYD(Factory.BOFactory).MainAddress.PK;

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);

			AssertNotNull("whsOrderData", whsOrderData);

			AssertEquals("whsOrderData.OrganizationAddressCollection.Count", 2, whsOrderData.OrganizationAddressCollection.Count);
			AssertOrganizationBO_INTHEMSYD("ConsignorPickupDeliveryAddress", whsOrderData.OrganizationAddressCollection[0], "ConsignorPickupDeliveryAddress");
			AssertOrganizationBO_INTHEMSYD("CustomsWarehouseAddress", whsOrderData.OrganizationAddressCollection[1], "CustomsWarehouseAddress");
		}

		#endregion

		#region TestOrderLinesWithoutChildLines

		public void TestOrderLinesWithoutChildLines()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, 2, 1, saveFactory_doNotUseForNewTests: false);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.SaveForTesting();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 9m);
			var line1 = order.Lines[0];
			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 12m);
			line2.WE_WE_ParentDocketLine = line1.PK;
			Factory.SaveForTesting();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var helperInOtherFactory = new WhsTestHelperFunctions(factory2);
			helperInOtherFactory.CreatePickNew(factory2.Load<WhsOrder>(order.PK)); // Cant call factory.save using universal factory

			order.Reload();
			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order))).GetDataObject(order);
			AssertNotNull("whsOrderData", whsOrderData);
			AssertEquals("whsOrderData.Order.OrderLineCollection.Count", 1, whsOrderData.Order.OrderLineCollection.Count);

			var lineData1 = whsOrderData.Order.OrderLineCollection[0];
			AssertEquals("lineData1.QuantityMet", 9m, lineData1.QuantityMet);
		}

		#endregion

		#region TestOrderLines

		public void TestOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var line2 = Helper.CreateWhsOrderLine(order, data.Part2, 3.14m);

			var whsOrderData = GetNewDataObjectWriter(order).GetDataObject(order);
			AssertNotNull(whsOrderData);
			AssertEquals("whsOrderData.Order.OrderLineCollection.Count", 2, whsOrderData.Order.OrderLineCollection.Count);

			var lineData1 = whsOrderData.Order.OrderLineCollection[0];
			AssertEquals("lineData.Product.Code", data.Part1.OP_PartNum, lineData1.Product.Code);
			AssertEquals("lineData.OrderedQty", 10m, lineData1.OrderedQty);

			var lineData2 = whsOrderData.Order.OrderLineCollection[1];
			AssertEquals("lineData.Product.Code", data.Part2.OP_PartNum, lineData2.Product.Code);
			AssertEquals("lineData.OrderedQty", 3.14m, lineData2.OrderedQty);
		}

		public void TestOrderLines_ReleaseLines()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, 2, 1, saveFactory_doNotUseForNewTests: false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			Factory.SaveForTesting();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20.2m);
			var line1 = order.Lines[0];
			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 1.2m);
			Factory.SaveForTesting();

			PickOrder(order);
			var releaseLine1 = line1.ReleaseLines[0];
			releaseLine1.PartAttribute1 = "WHOA";
			releaseLine1.PartAttribute2 = "WUP";
			releaseLine1.PartAttribute3 = "BAM";
			releaseLine1.Quantity = 9.1m;

			var releaseLine2 = line1.ReleaseLines.AddNew("WHOA", "WHEE", "HUM", "SER", ZDate.Empty, ZDate.Empty);
			releaseLine2.Quantity = 11.1m;

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order))).GetDataObject(order);
			AssertNotNull("whsOrderData", whsOrderData);
			AssertEquals("whsOrderData.Order.OrderLineCollection.Count", 2, whsOrderData.Order.OrderLineCollection.Count);

			var lineData1 = whsOrderData.Order.OrderLineCollection[0];
			var lineData2 = whsOrderData.Order.OrderLineCollection[1];
			var commercialInvoiceHeader = whsOrderData.CommercialInfo.CommercialInvoiceCollection[0];

			AssertEquals("commercialInvoiceHeader.CommercialInvoiceLineCollection.Count", 3, commercialInvoiceHeader.CommercialInvoiceLineCollection.Count);
			CombineAssertions(() =>
			{
				AssertEquals("lineData1.QuantityMet", 20.2m, lineData1.QuantityMet);
				AssertEquals("lineData1.Link", 0, lineData1.Link);

				AssertEquals("lineData2.QuantityMet", 1.2m, lineData2.QuantityMet);
				AssertEquals("lineData2.Link", 1, lineData2.Link);

				var releaseLineDO1 = commercialInvoiceHeader.CommercialInvoiceLineCollection.Single(o => o.InvoiceQuantity == 9.1m);
				AssertEquals(nameof(releaseLineDO1.OrderLineLink), 0, releaseLineDO1.OrderLineLink);

				var releaseLineDO2 = commercialInvoiceHeader.CommercialInvoiceLineCollection.Single(o => o.InvoiceQuantity == 1.2m);
				AssertEquals(nameof(releaseLineDO2.OrderLineLink), 1, releaseLineDO2.OrderLineLink);

				var releaseLineDO3 = commercialInvoiceHeader.CommercialInvoiceLineCollection.Single(o => o.InvoiceQuantity == 11.1m);
				AssertEquals(nameof(releaseLineDO3.OrderLineLink), 0, releaseLineDO3.OrderLineLink);
			});
		}

		#endregion

		#region TestPackageJob

		public void TestPackageJob()
		{
			var factory2 = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory2);

			var data = new TestDataSimpleEnvironment(factory2);
			data.Part1.OP_Weight = 0m;
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			factory2.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_ContainerMode = "LCL";
			AssertEquals("WarehouseOrderStatus is NEW", DocketStatus.Codes.New, order.WarehouseOrderStatus);

			helper.CreatePickNew(order);
			var pickLine = order.Lines.Single().PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			factory2.Save();

			var packageJob = order.PackageJob;
			packageJob.KJ_JobID = "PJ00000001";

			var containerType20GP = SetupContainer(packageJob);
			var container2 = packageJob.Packages.AddNew("CNT");

			var package = packageJob.Packages.AddNew("BOX");
			package.Pack(order.Lines[0].ReleaseLines[0], 1m);
			package.KP_PackageID = "PACKAGE123";
			package.KP_DimensionUQ = "M";
			package.KP_VolumeUQ = "M3";
			package.KP_WeightUQ = "T";
			package.KP_Height = 1m;
			package.KP_Length = 2m;
			package.KP_PackageQty = 3;
			package.KP_Weight = 5m;
			package.KP_Width = 6m;
			package.KP_MarksAndNumbers = "MARK123";
			package.KP_TransportRef = "TRANSPORT REF";
			package.KP_GoodsDescription = "GOODS DESC";
			package.KP_HSCode = "HARMON CODE";
			package.KP_Volume = 12m;

			var packageJobData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order))).GetDataObject(order);

			AssertEquals("Container Packages should be stored in ContainerCollection", 2, packageJobData.ContainerCollection.Count);

			var containerData1 = packageJobData.ContainerCollection[0];
			var containerData2 = packageJobData.ContainerCollection[1];

			CombineAssertions(() =>
			{
				AssertContainerContents(containerData1);
				AssertEquals("containerData1.Container.K0_RC_ContainerType", containerType20GP.RC_Code, containerData1.ContainerType.Code);
				AssertEquals("containerData1.Container.K0_Seal1", "SEAL-1", containerData1.Seal);
				AssertEquals("Should get container mode from Order.", "LCL", containerData2.FCL_LCL_AIR.Code);
				AssertEquals("Should get container mode from Order.", "Less Container Load", containerData2.FCL_LCL_AIR.Description);

				AssertEquals("Non Container Packages should be stored in PackingLineCollection", 1, packageJobData.PackingLineCollection.Count);
				var packageData = packageJobData.PackingLineCollection[0];
				AssertEquals("packageData.LengthUnit.Code", "M", packageData.LengthUnit.Code);
				AssertEquals("packageData.Height", 1m, packageData.Height);
				AssertEquals("packageData.Length", 2m, packageData.Length);
				AssertEquals("packageData.ReferenceNumber", "PACKAGE123", packageData.ReferenceNumber);
				AssertEquals("packageData.PackQty", 3L, packageData.PackQty);
				AssertEquals("packageData.PackType.Code", "BOX", packageData.PackType.Code);
				AssertEquals("packageData.Volume", 12m, packageData.Volume);
				AssertEquals("packageData.VolumeUnit.Code", "M3", packageData.VolumeUnit.Code);
				AssertEquals("packageData.Weight", 5m, packageData.Weight);
				AssertEquals("packageData.WeightUnit.Code", "T", packageData.WeightUnit.Code);
				AssertEquals("packageData.Width", 6m, packageData.Width);
				AssertEquals("packageData.TransportReference", "TRANSPORT REF", packageData.TransportReference);
				AssertEquals("packageData.MarksAndNos", "MARK123", packageData.MarksAndNos);
				AssertEquals("packageData.GoodsDescription", "GOODS DESC", packageData.GoodsDescription);
				AssertEquals("packageData.HarmonisedCode", "HARMON CODE", packageData.HarmonisedCode);

				AssertEquals("Package should be assigned a Link, so it could be identified by other BizOs", 0, packageData.Link);

				var packedItemsData1 = packageJobData.CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection.Single(invoiceLine => invoiceLine.OrderLineLink == 0);
				AssertEquals("packedItemsData1.Description", "P1", packedItemsData1.PartNo);

				var productCode = packedItemsData1.CustomizedFieldCollection.FirstOrDefault(c => c.Key.HasValue && c.Key.Value == "Product Code");
				AssertNotNull(productCode.Value);
				AssertEquals(productCode.Value, "P1");
			});
		}

		#endregion

		#region TestPackageJob_ReleaseCapuredAttributes

		public void TestPackageJob_ReleaseCapuredAttributes()
		{
			var factory2 = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory2);

			var data = new TestDataSimpleEnvironment(factory2);
			data.Part1.OP_Weight = 0m;
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			factory2.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_ContainerMode = "LCL";
			AssertEquals("WarehouseOrderStatus is NEW", DocketStatus.Codes.New, order.WarehouseOrderStatus);

			helper.CreatePickNew(order);
			var pickLine = order.Lines.Single().PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var releaseLine1 = order.Lines[0].ReleaseLines[0].PartAttribute1 = "RED";
			factory2.Save();

			var packageJob = order.PackageJob;
			packageJob.KJ_JobID = "PJ00000001";

			var containerType20GP = SetupContainer(packageJob);
			var container2 = packageJob.Packages.AddNew("CNT");

			var package = packageJob.Packages.AddNew("BOX");
			package.Pack(order.Lines[0].ReleaseLines[0], 1m);
			package.KP_PackageID = "PACKAGE123";
			package.KP_DimensionUQ = "M";
			package.KP_VolumeUQ = "M3";
			package.KP_WeightUQ = "T";
			package.KP_Height = 1m;
			package.KP_Length = 2m;
			package.KP_PackageQty = 3;
			package.KP_Weight = 5m;
			package.KP_Width = 6m;
			package.KP_MarksAndNumbers = "MARK123";
			package.KP_TransportRef = "TRANSPORT REF";
			package.KP_GoodsDescription = "GOODS DESC";
			package.KP_HSCode = "HARMON CODE";
			package.KP_Volume = 12m;

			var packageJobData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order))).GetDataObject(order);

			AssertEquals("Container Packages should be stored in ContainerCollection", 2, packageJobData.ContainerCollection.Count);

			var containerData1 = packageJobData.ContainerCollection[0];
			var containerData2 = packageJobData.ContainerCollection[1];

			CombineAssertions(() =>
			{
				AssertContainerContents(containerData1);
				AssertEquals("containerData1.Container.K0_RC_ContainerType", containerType20GP.RC_Code, containerData1.ContainerType.Code);
				AssertEquals("containerData1.Container.K0_Seal1", "SEAL-1", containerData1.Seal);
				AssertEquals("Should get container mode from Order.", "LCL", containerData2.FCL_LCL_AIR.Code);
				AssertEquals("Should get container mode from Order.", "Less Container Load", containerData2.FCL_LCL_AIR.Description);

				AssertEquals("Non Container Packages should be stored in PackingLineCollection", 1, packageJobData.PackingLineCollection.Count);
				var packageData = packageJobData.PackingLineCollection[0];
				AssertEquals("packageData.LengthUnit.Code", "M", packageData.LengthUnit.Code);
				AssertEquals("packageData.Height", 1m, packageData.Height);
				AssertEquals("packageData.Length", 2m, packageData.Length);
				AssertEquals("packageData.ReferenceNumber", "PACKAGE123", packageData.ReferenceNumber);
				AssertEquals("packageData.PackQty", 3L, packageData.PackQty);
				AssertEquals("packageData.PackType.Code", "BOX", packageData.PackType.Code);
				AssertEquals("packageData.Volume", 12m, packageData.Volume);
				AssertEquals("packageData.VolumeUnit.Code", "M3", packageData.VolumeUnit.Code);
				AssertEquals("packageData.Weight", 5m, packageData.Weight);
				AssertEquals("packageData.WeightUnit.Code", "T", packageData.WeightUnit.Code);
				AssertEquals("packageData.Width", 6m, packageData.Width);
				AssertEquals("packageData.TransportReference", "TRANSPORT REF", packageData.TransportReference);
				AssertEquals("packageData.MarksAndNos", "MARK123", packageData.MarksAndNos);
				AssertEquals("packageData.GoodsDescription", "GOODS DESC", packageData.GoodsDescription);
				AssertEquals("packageData.HarmonisedCode", "HARMON CODE", packageData.HarmonisedCode);

				AssertEquals("Package should be assigned a Link, so it could be identified by other BizOs", 0, packageData.Link);

				var packedItemsData1 = packageJobData.CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection.Single(invoiceLine => invoiceLine.OrderLineLink == 0);
				AssertEquals("packedItemsData1.Description", "P1", packedItemsData1.PartNo);

				var productCode = packedItemsData1.CustomizedFieldCollection.FirstOrDefault(c => c.Key.HasValue && c.Key.Value == "Product Code");
				AssertNotNull(productCode.Value);
				AssertEquals(productCode.Value, "P1");
			});
		}

		#endregion

		#region TestContainersAreDeDuplicatedFromOrderAndPackageJob

		public void TestContainersAreDeDuplicatedFromOrderAndPackageJob()
		{
			var whsOrderBO = Factory.NewWithValidTestData<WhsOrder>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrderBO);
			packageJob.KJ_JobID = "PJ00000001";

			SetupContainer(packageJob);

			whsOrderBO.Containers.Add(WhsDocketContainerDataObjectWriterTest.GetContainer(Factory.BOFactory));
			var containerOnlyOnOrder = whsOrderBO.Containers.AddNew();
			containerOnlyOnOrder.WC_ContainerNum = "123456";

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO))).GetDataObject(whsOrderBO);

			AssertEquals("ContainerCollection should only have 2 containers.", 2, whsOrderData.ContainerCollection.Count);

			var deDuplicatedContainerData = whsOrderData.ContainerCollection[0];
			var containerOnlyOnOrderData = whsOrderData.ContainerCollection[1];

			CombineAssertions(delegate
			{
				WhsDocketContainerDataObjectWriterTest.AssertContents(deDuplicatedContainerData);
				AssertContainerContents(deDuplicatedContainerData);
				AssertEquals("containerOnlyOnOrderData.ContainerNumber", "123456", containerOnlyOnOrderData.ContainerNumber);
				AssertEquals("containerOnlyOnOrderData.FCL_LCL_AIR.Code", "LTL", containerOnlyOnOrderData.FCL_LCL_AIR.Code);
				AssertEquals("containerOnlyOnOrderData.FCL_LCL_AIR.Description", "Less Truck Load", containerOnlyOnOrderData.FCL_LCL_AIR.Description);
			});
		}

		#endregion

		#region TestExcludeElement_Packages

		public void TestExcludeElement_Packages()
		{
			var otherFactory = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(otherFactory);
			var helperInOtherFactory = new WhsTestHelperFunctions(otherFactory);
			helperInOtherFactory.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			otherFactory.Save();

			var orderInOtherFactory = helperInOtherFactory.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			helperInOtherFactory.CreatePickNew(orderInOtherFactory);

			var order = Factory.Load<WhsOrder>(orderInOtherFactory.PK);
			var package = order.PackageJob.Packages.AddNew("PLT");
			var docketContainer = order.Containers.AddNew();
			docketContainer.WC_ContainerNum = "CONT123";

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order)), ExcludeElement.Packages).GetDataObject(order);
			AssertNull("Should not have Exported Packages.", whsOrderData.PackingLineCollection);
			AssertNull("Should not have Exported Packages.", whsOrderData.ContainerCollection);
		}

		#endregion

		#region TestOuterPacks

		public void TestOuterPacksShouldBeNullWhenOrderHasPackageAndPackagesSentIsNotSet()
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = Factory.New<WhsOrderLine>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew("BOX");
			orderLine.WE_WD = order.PK;
			AssertEquals("Precondition - Packages count must be calculated when adding a new package.", 1, order.WD_PackagesSent);

			order.WD_PackagesSent = 0; // manually change it to zero for test.
			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order))).GetDataObject(order);
			AssertNotNull("whsOrderData", whsOrderData);
			AssertEquals("whsOrderData.Order.OrderLineCollection.Count", 1, whsOrderData.Order.OrderLineCollection.Count);
			AssertNull(whsOrderData.OuterPacks);

			order.WD_PackagesSent = 1;
			whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order))).GetDataObject(order);
			AssertEquals(1, whsOrderData.OuterPacks);
		}

		#endregion

		#region TestCreateTransportBooking

		public void TestCreateTransportBookingWithNoPackage()
		{
			var result = CreateTransportBookingAndGetBookingPackages(createPackage: false, orderQty: 20, orderPackType: "CTN");

			AssertEquals("TB should have 20x CTN", 1, result.Count(p => p.KP_PackageQty == 20 && p.KP_F3_NKPackType == "CTN"));
		}

		public void TestCreateTransportBookingWithPackage()
		{
			var result = CreateTransportBookingAndGetBookingPackages(createPackage: true, orderPackType: "CTN", orderQty: 20m, packageID: "PACK123", packageQuantity: 1);

			AssertEquals("TB should have 1x BOX with package id PACK123", 1, result.Count(p => p.KP_PackageQty == 1 && p.KP_F3_NKPackType == "BOX" && p.KP_PackageID == "PACK123"));
		}

		public void TestCreateTransportBookingOrderHasPackageSentNoPackage()
		{
			var result = CreateTransportBookingAndGetBookingPackages(createPackage: false, packagesSent: 12, orderPackType: "CTN");

			AssertEquals("TB should have 12x CTN", 1, result.Count(p => p.KP_PackageQty == 12 && p.KP_F3_NKPackType == "CTN"));
		}

		public void TestCreateTransportBookingOrderHasPackageSentWithPackage()
		{
			var result = CreateTransportBookingAndGetBookingPackages(createPackage: true, packagesSent: 12, orderPackType: "CTN", packageID: "PACK123");

			AssertEquals(2, result.Length);
			AssertEquals("TB should have 12x CTN", 1, result.Count(p => p.KP_PackageQty == 12 && p.KP_F3_NKPackType == "CTN"));
			AssertEquals("TB should have 1x BOX with package id PACK123", 1, result.Count(p => p.KP_PackageQty == 1 && p.KP_F3_NKPackType == "BOX" && p.KP_PackageID == "PACK123"));
		}

		public void TestCreateTransportBookingOrderHasPalletsSentNoPackage()
		{
			var result = CreateTransportBookingAndGetBookingPackages(createPackage: false, palletsSent: 25, orderPackType: "CTN");

			AssertEquals("TB should have 25x PLT", 1, result.Count(p => p.KP_PackageQty == 25 && p.KP_F3_NKPackType == "PLT"));
		}

		public void TestCreateTransportBookingOrderHasPalletsSentWithPackage()
		{
			var result = CreateTransportBookingAndGetBookingPackages(createPackage: true, palletsSent: 25, orderPackType: "CTN", packageID: "PACK123");

			AssertEquals(2, result.Length);
			AssertEquals("TB should have 25x PLT", 1, result.Count(p => p.KP_PackageQty == 25 && p.KP_F3_NKPackType == "PLT"));
			AssertEquals("TB should have 1x BOX with package id PACK123", 1, result.Count(p => p.KP_PackageQty == 1 && p.KP_F3_NKPackType == "BOX" && p.KP_PackageID == "PACK123"));
		}

		PkgPackage[] CreateTransportBookingAndGetBookingPackages(bool createPackage = false, int packagesSent = 0, int palletsSent = 0, string orderPackType = "CTN", decimal orderQty = 1, string packageID = "", int packageQuantity = 1)
		{
			var org = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();
			var product1 = Data.CreateProduct("P1");

			var receive = Helper.CreateWhsReceive(org, whs, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, product1, orderQty);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(org, whs, "O1");
			Helper.CreateWhsOrderLine(order, product1, orderQty);
			if (createPackage)
			{
				var orderPackageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
				var package = PackingHelper.CreatePackage(orderPackageJob, packageQuantity, Constants.PkgUnit.Box, "SmallBox");
				package.KP_PackageID = packageID;
				package.KP_Weight = 0m;
				package.KP_Volume = 0m;
			}
			order.WD_PackagesSent = packagesSent;
			order.WD_F3_NKTotalPackType = orderPackType;
			order.WD_PalletsSent = (ZShort)palletsSent;

			Factory.SaveForTesting();

			var factory = new BusinessObjectFactory();
			PublishUniversalXmlResult events;
			using (factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(factory, GlbCompany.CurrentCompany.OrgProxy, new RecipientRoleType[1] { RecipientRoleType.CTG }, order);
				factory.Save();
			}
			var booking = (IDtbBooking)TransportBookingLoader.GetRelatedTransportBookingEvents(order).Single();
			return ((PkgPackageJob)booking.PackageJob).GetAllPackagesOnJob();
		}

		#endregion

		#region PackingHelper

		PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory.BOFactory)); }
		}

		PackingTestHelper packingHelper;

		#endregion

		#endregion

		#region TestReplaceUnitsSentOverflowException

		public void TestReplaceUnitsSentOverflowException_DataObjectValidationExceptionInstead()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("WH1");
			var largeNumber = new ZDecimal(9876543210.012m);
			var order = Helper.CreateWhsOrder(client, whs);
			order.WD_UnitsSent = largeNumber;
			AssertEquals("Precondition: Order WD_UnitsSent has correct value", largeNumber, order.WD_UnitsSent);

			var writer = GetNewDataObjectWriter(order);
			AssertExceptionThrown(
				"Correct Exception thrown.",
				typeof(DataObjectValidationException),
				() => writer.GetDataObject(order));
		}

		#endregion

		#region OrderLineDictionary

		public void TestOrderLineDictionary()
		{
			var factory = new BusinessObjectFactory();
			var warehouseHelper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, saveFactory_doNotUseForNewTests: false);
			var receive = warehouseHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			factory.Save();

			Assert("Precondition", receive.IsFinalised);
			var order = warehouseHelper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.CarrierBookingAgentDocAddress.OrganisationPK = warehouseHelper.CreateClient("RTUS").PK;
			var orderLine1 = warehouseHelper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = warehouseHelper.CreateWhsOrderLine(order, data.Part1, 15m);
			warehouseHelper.CreatePickNew(order);
			factory.Save();

			var package = order.PackageJob.Packages.AddNew("BOX");
			var releaseLine1 = orderLine1.ReleaseLines[0];
			package.Pack(releaseLine1, 10m);
			var releaseLine2 = orderLine2.ReleaseLines[0];
			package.Pack(releaseLine2, 15m);

			var orderLineDictionary = new Dictionary<ZGuid, ZInt>();
			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order)), orderLineDictionary: orderLineDictionary).GetDataObject(order);
			AssertEquals(2, orderLineDictionary.Count);
			Assert(orderLineDictionary.ContainsKey(releaseLine1.PK));
			Assert(orderLineDictionary.ContainsKey(releaseLine2.PK));

			var commercialInvoiceHeader = whsOrderData.CommercialInfo.CommercialInvoiceCollection[0];
			AssertEquals("commercialInvoiceHeader.CommercialInvoiceLineCollection.Count", 2, commercialInvoiceHeader.CommercialInvoiceLineCollection.Count);
			CombineAssertions(delegate
			{
				var releaseLineDO1 = commercialInvoiceHeader.CommercialInvoiceLineCollection.Single(o => o.InvoiceQuantity == 10m);
				AssertEquals(nameof(releaseLineDO1.OrderLineLink), 0, releaseLineDO1.OrderLineLink);

				var releaseLineDO2 = commercialInvoiceHeader.CommercialInvoiceLineCollection.Single(o => o.InvoiceQuantity == 15m);
				AssertEquals(nameof(releaseLineDO2.OrderLineLink), 1, releaseLineDO2.OrderLineLink);
			});
		}

		#endregion

		#region OrderStatus

		public void TestOrderStatus_ENT()
		{
			var factory = new BusinessObjectFactory();
			var warehouseHelper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, saveFactory_doNotUseForNewTests: false);
			var receive = warehouseHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			factory.Save();

			Assert("Precondition", receive.IsFinalised);
			var whsOrder = warehouseHelper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			warehouseHelper.CreateWhsOrderLine(whsOrder, data.Part1, 10m);
			factory.Save();

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrder))).GetDataObject(whsOrder);
			var order = whsOrderData.Order;
			AssertEquals(nameof(order.Status.Code), DocketStatus.Codes.Entered, order.Status.Code);
			AssertEquals(nameof(order.Status.Description), DocketStatus.Descriptions.Entered, order.Status.Description);
		}

		public void TestOrderStatus_ATP()
		{
			var factory = new BusinessObjectFactory();
			var warehouseHelper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, saveFactory_doNotUseForNewTests: false);
			var receive = warehouseHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			factory.Save();

			Assert("Precondition", receive.IsFinalised);
			var whsOrder = warehouseHelper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			warehouseHelper.CreateWhsOrderLine(whsOrder, data.Part1, 10m);
			factory.Save();

			warehouseHelper.CreatePickNew(whsOrder);
			factory.Save();

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrder))).GetDataObject(whsOrder);
			var order = whsOrderData.Order;
			AssertEquals(nameof(order.Status.Code), DocketStatus.Codes.AttachedToPick, order.Status.Code);
			AssertEquals(nameof(order.Status.Description), DocketStatus.Descriptions.AttachedToPick, order.Status.Description);
		}

		public void TestOrderStatus_STA()
		{
			var factory = new BusinessObjectFactory();
			var warehouseHelper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, saveFactory_doNotUseForNewTests: false);
			var receive = warehouseHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			factory.Save();

			Assert("Precondition", receive.IsFinalised);
			var whsOrder = warehouseHelper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			warehouseHelper.CreateWhsOrderLine(whsOrder, data.Part1, 5m);
			warehouseHelper.CreateWhsOrderLine(whsOrder, data.Part1, 5m);
			factory.Save();

			warehouseHelper.CreatePickNew(whsOrder);
			var pickLine1 = whsOrder.Lines[0].PickLines.Single();
			var pickLine2 = whsOrder.Lines[1].PickLines.Single();
			Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now).FinaliseDocketLine();
			Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now).FinaliseDocketLine();
			factory.Save();

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrder))).GetDataObject(whsOrder);
			var order = whsOrderData.Order;
			AssertEquals(nameof(order.Status.Code), WhsOrderStatus.Codes.Staged, order.Status.Code);
			AssertEquals(nameof(order.Status.Description), WhsOrderStatus.Descriptions.Staged, order.Status.Description);
		}

		public void TestOrderStatus_LDG()
		{
			var factory = new BusinessObjectFactory();
			var warehouseHelper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, saveFactory_doNotUseForNewTests: false);
			var receive = warehouseHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var truck = warehouseHelper.CreateEquipment("T001", 1m, Constants.Weight.Kilograms, 1m, Constants.Volume.CubicMetres);
			var load = warehouseHelper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);
			factory.Save();

			Assert("Precondition", receive.IsFinalised);
			var whsOrder = warehouseHelper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			warehouseHelper.CreateWhsOrderLine(whsOrder, data.Part1, 5m);
			warehouseHelper.CreateWhsOrderLine(whsOrder, data.Part1, 5m);
			factory.Save();

			warehouseHelper.CreatePickNew(whsOrder);
			var pickLine1 = whsOrder.Lines[0].PickLines.Single();
			var pickLine2 = whsOrder.Lines[1].PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var package1 = whsOrder.PackageJob.Packages.AddNew("CTN");
			package1.Pack(whsOrder.Lines[0].ReleaseLines[0], 5m);
			var loadPkgPackagePivot1 = warehouseHelper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			var package2 = whsOrder.PackageJob.Packages.AddNew("CTN");
			package2.Pack(whsOrder.Lines[1].ReleaseLines[0], 5m);
			var loadPkgPackagePivot2 = warehouseHelper.CreateLoadPkgPackagePivot(package2.PK, load);
			factory.Save();

			var whsOrderInOtherFactory = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsOrder>(whsOrder.PK);
			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderInOtherFactory))).GetDataObject(whsOrderInOtherFactory);
			var order = whsOrderData.Order;
			AssertEquals(nameof(order.Status.Code), WhsOrderStatus.Codes.Loading, order.Status.Code);
			AssertEquals(nameof(order.Status.Description), WhsOrderStatus.Descriptions.Loading, order.Status.Description);
		}

		public void TestOrderStatus_LOA()
		{
			var factory = new BusinessObjectFactory();
			var warehouseHelper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, saveFactory_doNotUseForNewTests: false);
			var receive = warehouseHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var truck = warehouseHelper.CreateEquipment("T001", 1m, Constants.Weight.Kilograms, 1m, Constants.Volume.CubicMetres);
			var load = warehouseHelper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);
			factory.Save();

			Assert("Precondition", receive.IsFinalised);
			var whsOrder = warehouseHelper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			warehouseHelper.CreateWhsOrderLine(whsOrder, data.Part1, 10m);
			factory.Save();

			warehouseHelper.CreatePickNew(whsOrder);
			var pickLine1 = whsOrder.Lines[0].PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var package1 = whsOrder.PackageJob.Packages.AddNew("CTN");
			package1.Pack(whsOrder.Lines[0].ReleaseLines[0], 5m);
			var loadPkgPackagePivot1 = warehouseHelper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			factory.Save();

			var whsOrderInOtherFactory = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsOrder>(whsOrder.PK);
			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderInOtherFactory))).GetDataObject(whsOrderInOtherFactory);
			var order = whsOrderData.Order;
			AssertEquals(nameof(order.Status.Code), WhsOrderStatus.Codes.Loaded, order.Status.Code);
			AssertEquals(nameof(order.Status.Description), WhsOrderStatus.Descriptions.Loaded, order.Status.Description);
		}

		public void TestOrderStatus_DEP()
		{
			var factory = new BusinessObjectFactory();
			var warehouseHelper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, saveFactory_doNotUseForNewTests: false);
			var receive = warehouseHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var truck = warehouseHelper.CreateEquipment("T001", 1m, Constants.Weight.Kilograms, 1m, Constants.Volume.CubicMetres);
			var load = warehouseHelper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck);
			factory.Save();

			Assert("Precondition", receive.IsFinalised);
			var whsOrder = warehouseHelper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			warehouseHelper.CreateWhsOrderLine(whsOrder, data.Part1, 10m);
			factory.Save();

			warehouseHelper.CreatePickNew(whsOrder);
			var pickLine1 = whsOrder.Lines[0].PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var package1 = whsOrder.PackageJob.Packages.AddNew("CTN");
			package1.Pack(whsOrder.Lines[0].ReleaseLines[0], 5m);
			var loadPkgPackagePivot1 = warehouseHelper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			warehouseHelper.DepartPackageNow(loadPkgPackagePivot1);
			factory.Save();

			var whsOrderInOtherFactory = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsOrder>(whsOrder.PK);
			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderInOtherFactory))).GetDataObject(whsOrderInOtherFactory);
			var order = whsOrderData.Order;

			AssertEquals(nameof(order.Status.Code), WhsOrderStatus.Codes.Departed, order.Status.Code);
			AssertEquals(nameof(order.Status.Description), WhsOrderStatus.Descriptions.Departed, order.Status.Description);
		}

		#endregion

		#region TestPickPriority

		public void TestPickPriority()
		{
			var factory = new BusinessObjectFactory();
			var warehouseHelper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, saveFactory_doNotUseForNewTests: false);
			var receive = warehouseHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			factory.Save();

			Assert("Precondition", receive.IsFinalised);
			var whsOrder = warehouseHelper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			warehouseHelper.CreateWhsOrderLine(whsOrder, data.Part1, 10m);
			factory.Save();
			whsOrder.WD_PickPriority = 3;
			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrder))).GetDataObject(whsOrder);
			var order = whsOrderData.Order;
			AssertEquals((byte)3, order.PickPriority);
		}

		#endregion

		#region TestUseDirectedPackingConsolidation

		public void TestUseDirectedPackingConsolidation_ThenShouldBeExported() => TestUseDirectedPackingConsolidationCore(isUseDirectedPackingConsolidation: true, expectedResult: ZBool.True);

		public void TestNotUseDirectedPackingConsolidation_ThenShouldNotBeExported() => TestUseDirectedPackingConsolidationCore(isUseDirectedPackingConsolidation: false, expectedResult: ZBool.False);

		void TestUseDirectedPackingConsolidationCore(bool isUseDirectedPackingConsolidation, ZBool? expectedResult)
		{
			var factory = new BusinessObjectFactory();
			var warehouseHelper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, saveFactory_doNotUseForNewTests: false);
			factory.Save();

			var whsOrder = warehouseHelper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			whsOrder.WD_UseDirectedPackingConsolidation = isUseDirectedPackingConsolidation;
			warehouseHelper.CreateWhsOrderLine(whsOrder, data.Part1, 10m);
			factory.Save();

			var whsOrderData = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrder))).GetDataObject(whsOrder);
			AssertEquals("order.UseDirectedPackingConsolidation", expectedResult, whsOrderData.Order.UseDirectedPackingConsolidation);
		}

		#endregion

		#region TestReturnAddressShouldBePopulatedIntoDataObject

		public void TestReturnAddressShouldBePopulatedIntoDataObject()
		{
			var whsOrderBO = Factory.New<WhsOrder>();
			var orgHeader = Helper.CreateClient("ABC");
			var whsOrderDataWriter = new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsOrderBO)));
			whsOrderBO.ReturnDocAddress.E2_OA_Address = orgHeader.MainAddress.PK;

			var whsOrderData = whsOrderDataWriter.GetDataObject(whsOrderBO);

			AssertNotNull("whsOrderData", whsOrderData);

			var returnAddresses = whsOrderData.OrganizationAddressCollection.Where(addr => addr.AddressType.Equals(nameof(DocAddressType.ReturnAddress)));

			AssertEquals(1, returnAddresses.Count());
			AssertEquals("Return Address in universal shipment should be correct when it is specified", orgHeader.MainAddress.CompanyName, returnAddresses.FirstOrDefault().CompanyName);
		}

		#endregion

		#region Implementation

		void PickOrder(WhsOrder order)
		{
			var bensDodgyField = Factory.BOFactory.GetType().GetField("saveAllowed", BindingFlags.NonPublic | BindingFlags.Instance);
			try
			{
				bensDodgyField.SetValue(Factory.BOFactory, true);

				Helper.CreatePickNew(order);
				AssertEquals("Precondition", true, order.IsAttachedToPickButNotFinalised);
			}
			finally
			{
				bensDodgyField.SetValue(Factory.BOFactory, false);
			}
		}

		RefContainer SetupContainer(PkgPackageJob packageJob)
		{
			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var container = packageJob.Packages.AddNew("CNT");
			container.KP_PackageQty = 3;
			container.Container.K0_AirVentFlowRate = 7m;
			container.Container.K0_AirVentFlowRateUnit = "M2";
			container.Container.K0_ContainerMode = "AIR";
			container.KP_DunnageWeight = 8m;
			container.Container.K0_HumidityPercent = 9;
			container.Container.K0_IsControlledAtmosphere = true;
			container.Container.K0_IsDamaged = true;
			container.Container.K0_IsEmpty = true;
			container.Container.K0_IsSealOk = true;
			container.Container.K0_IsShipperOwned = true;
			container.Container.K0_Quality = "RIC";
			container.Container.K0_RC_ContainerType = containerType20GP.PK;
			container.Container.K0_RefrigGeneratorID = "REFRIG123";
			container.Container.K0_Seal1 = "SEAL-1";
			container.Container.K0_Seal2 = "SEAL-2";
			container.Container.K0_Seal3 = "SEAL-3";
			container.Container.K0_SetPointTemp = 10m;
			container.Container.K0_SetPointTempUnit = "C";
			container.Container.K0_Status = "ARV";
			container.KP_TareWeight = 11m;
			container.Container.K0_TempRecorderSerialNumber = "TEMPSER123";
			container.KP_DimensionUQ = "M";
			container.KP_Height = 1m;
			container.KP_Length = 2m;
			container.KP_PackageID = "OOCCC1111";
			container.KP_VolumeUQ = "M3";
			container.KP_Weight = 5m;
			container.KP_WeightUQ = "T";
			container.KP_Width = 6m;
			container.KP_TransportRef = "TRANSPORT REF";
			container.KP_GoodsDescription = "GOODS DESC";
			container.KP_HSCode = "HARMON CODE";
			container.KP_Volume = 12m;

			return containerType20GP;
		}

		static void AssertContainerContents(Container containerData)
		{
			AssertEquals("containerData.KP_DimensionUQ", "M", containerData.LengthUnit.Code);
			AssertEquals("containerData.KP_Height", 1m, containerData.TotalHeight);
			AssertEquals("containerData.KP_Length", 2m, containerData.TotalLength);
			AssertEquals("containerData.KP_PackageID", "OOCCC1111", containerData.ContainerNumber);
			AssertEquals("containerData.KP_PackageQty", 3, containerData.ContainerCount);
			AssertEquals("containerData.KP_Volume", 12m, containerData.VolumeCapacity);
			AssertEquals("containerData.KP_VolumeUQ", "M3", containerData.VolumeUnit.Code);
			AssertEquals("containerData.KP_Weight", 5m, containerData.GrossWeight);
			AssertEquals("containerData.KP_WeightUQ", "T", containerData.WeightUnit.Code);
			AssertEquals("containerData.KP_Width", 6m, containerData.TotalWidth);
			AssertEquals("containerData.KP_TransportRef", "TRANSPORT REF", containerData.TransportReference);
			AssertEquals("containerData.KP_GoodsDescription", "GOODS DESC", containerData.GoodsDescription);
			AssertEquals("containerData.KP_HSCode", "HARMON CODE", containerData.HarmonisedCode);
			AssertEquals("containerData.Container.K0_AirVentFlowRate", 7m, containerData.AirVentFlow);
			AssertEquals("containerData.Container.K0_AirVentFlowRateUnit", "M2", containerData.AirVentFlowRateUnit.Code);
			AssertEquals("containerData.Container.K0_ContainerMode", "AIR", containerData.FCL_LCL_AIR.Code);
			AssertEquals("containerData.Container.K0_DunnageWeight", 8m, containerData.DunnageWeight);
			AssertEquals("containerData.Container.K0_HumidityPercent", (ZByte)9, containerData.HumidityPercent);
			AssertEquals("containerData.Container.K0_IsControlledAtmosphere", true, containerData.IsControlledAtmosphere);
			AssertEquals("containerData.Container.K0_IsDamaged", true, containerData.IsDamaged);
			AssertEquals("containerData.Container.K0_IsEmpty", true, containerData.IsEmptyContainer);
			AssertEquals("containerData.Container.K0_IsSealOk", true, containerData.IsSealOk);
			AssertEquals("containerData.Container.K0_IsShipperOwned", true, containerData.IsShipperOwned);
			AssertEquals("containerData.Container.K0_Quality", "RIC", containerData.ContainerQuality.Code);
			AssertEquals("containerData.Container.K0_RefrigGeneratorID", "REFRIG123", containerData.RefrigGeneratorID);
			AssertEquals("containerData.Container.K0_Seal2", "SEAL-2", containerData.SecondSeal);
			AssertEquals("containerData.Container.K0_Seal3", "SEAL-3", containerData.ThirdSeal);
			AssertEquals("containerData.Container.K0_SetPointTemp", 10m, containerData.SetPointTemp);
			AssertEquals("containerData.Container.K0_SetPointTempUnit", "C", containerData.SetPointTempUnit);
			AssertEquals("containerData.Container.K0_Status", "ARV", containerData.ContainerStatus.Code);
			AssertEquals("containerData.Container.K0_TareWeight", 11m, containerData.TareWeight);
			AssertEquals("containerData.Container.K0_TempRecorderSerialNumber", "TEMPSER123", containerData.TempRecorderSerialNo);
			AssertEquals("Container should be assigned a Link, so it could be identified by other BizOs", 0, containerData.Link);
		}

		protected override WhsOrder GetNewDocket()
		{
			return Factory.NewWithValidTestData<WhsOrder>();
		}

		protected override WhsOrderDataObjectWriter GetNewDataObjectWriter(BusinessObject topLevelBO, INotifications notifications)
			=> new WhsOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, topLevelBO) { Notifications = notifications }));

		protected override void SetPalletsSent(WhsOrder order, ZShort palletsSent)
		{
			order.WD_PalletsSent = palletsSent;
		}

		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseOrder);

		#endregion
	}
}
