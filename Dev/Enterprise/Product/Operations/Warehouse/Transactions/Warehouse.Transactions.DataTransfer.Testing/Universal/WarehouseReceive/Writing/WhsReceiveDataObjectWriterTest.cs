using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	sealed class WhsReceiveDataObjectWriterTest : WhsOrderAndReceiveDataObjectWriterTest<WhsReceive, WhsReceiveDataObjectWriter>
	{
		#region TestBasicReceiveLevelFieldMappings

		public void TestBasicReceiveLevelFieldMappings()
		{
			#region Setup whsReceiveBO

			var whsReceiveBO = Factory.New<WhsReceive>();

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Ware this!";
			whsReceiveBO.WD_WW_Whs = warehouse.PK;

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var warehouseDockDoorLocation = Helper.CreateRowAndGenerateLocations(warehouse, "DockA", 1, 1).Locations.Single();
			warehouseDockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			whsReceiveBO.WD_WL_InboundDockDoor = warehouseDockDoorLocation.PK;

			var serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_Code = "TSL";
			serviceLevel.RS_Description = "Test Service Level";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "ABC";
			carrierServiceLevel.PL_CarrierServiceLevelDescription = "service level description";

			var client = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "FREDDIE";
			client.MiscServ.OM_OC_EXDefaultDGContact = contact.PK;
			whsReceiveBO.WD_AddPalletWeightToOrder = true;
			whsReceiveBO.WD_OH_Client = client.PK;
			whsReceiveBO.WD_OH_Forwarder = Factory.NewWithValidTestData<OrgHeader>().PK;
			whsReceiveBO.TransportCoPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			whsReceiveBO.WD_TotalWeightUnit = "LB";
			whsReceiveBO.WD_TotalCubicUnit = "CF";
			whsReceiveBO.WD_BOLNo = "BILL";
			whsReceiveBO.WD_PackagesSent = 5;
			whsReceiveBO.WD_CustomerReference = "CUSTOMER";
			whsReceiveBO.WD_DocketID = "W10001011";
			whsReceiveBO.WD_DocketStatus = "ENT";
			whsReceiveBO.WD_DocketSubType = "CUS";
			whsReceiveBO.WD_DropMode = "HSL";
			whsReceiveBO.WD_ExternalReference = "ORDER123";
			whsReceiveBO.WD_ExternalReferenceSplit = 1;
			whsReceiveBO.WD_PickOption = "XXX";
			whsReceiveBO.WD_PL_NKCarrierServiceLevel = "STD";
			whsReceiveBO.WD_RS_NKServiceLevel = "TSL";
			whsReceiveBO.WD_BookingDate = new ZDateTimeOffset(2011, 1, 1);
			whsReceiveBO.WD_ETD = new ZDateTimeOffset(2011, 1, 2);
			whsReceiveBO.WD_ETA = new ZDateTimeOffset(2011, 1, 3);
			whsReceiveBO.WD_ArrivalDate = new ZDateTimeOffset(2011, 1, 4);
			whsReceiveBO.WD_TotalCubic = 23.3m;
			whsReceiveBO.WD_TotalUnits = 7.1m;
			whsReceiveBO.WD_TotalWeight = 45.8m;
			whsReceiveBO.WD_TransportReference = "TREEE";
			whsReceiveBO.WD_F3_NKTotalPackType = "CTN";
			whsReceiveBO.WD_CODPayMethod = "XXX";
			whsReceiveBO.WD_INCO = "XXX";
			whsReceiveBO.TransportCoPK = carrier.PK;
			whsReceiveBO.WD_HoldPalletIDPutaway = true;

			#endregion

			var whsReceiveData = new WhsReceiveDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsReceiveBO))).GetDataObject(whsReceiveBO);

			AssertNotNull("whsReceiveData", whsReceiveData);

			#region Check Contents of whsReceiveData object

			CombineAssertions(delegate
			{
				AssertEquals("whsOrderData.ActualChargeable", null, whsReceiveData.ActualChargeable);
				AssertEquals("whsOrderData.AdditionalTerms", null, whsReceiveData.AdditionalTerms);
				AssertEquals("whsOrderData.AgentsReference", null, whsReceiveData.AgentsReference);
				AssertEquals("whsOrderData.AWBServiceLevel", null, whsReceiveData.AWBServiceLevel);
				AssertEquals("whsOrderData.BookingConfirmationReference", null, whsReceiveData.BookingConfirmationReference);
				AssertEquals("whsOrderData.CarrierServiceLevel.Code", "STD", whsReceiveData.CarrierServiceLevel.Code);
				AssertEquals("whsOrderData.CarrierServiceLevel.Description", "Standard", whsReceiveData.CarrierServiceLevel.Description);
				AssertEquals("whsOrderData.CartageWaybillNumber", null, whsReceiveData.CartageWaybillNumber);
				AssertEquals("whsOrderData.CFSReference", null, whsReceiveData.CFSReference);
				AssertEquals("whsOrderData.ConsolidatedCargoStatus", null, whsReceiveData.ConsolidatedCargoStatus);
				AssertEquals("whsOrderData.ContainerCount", null, whsReceiveData.ContainerCount);
				AssertEquals("whsOrderData.ContainerMode", "LSE", whsReceiveData.ContainerMode.Code);
				AssertEquals("whsOrderData.CountryOfSupply", null, whsReceiveData.CountryOfSupply);
				AssertEquals("whsOrderData.DocumentedChargeable", null, whsReceiveData.DocumentedChargeable);
				AssertEquals("whsOrderData.DocumentedVolume", null, whsReceiveData.DocumentedVolume);
				AssertEquals("whsOrderData.DocumentedWeight", null, whsReceiveData.DocumentedWeight);
				AssertEquals("whsOrderData.EFTMode", null, whsReceiveData.EFTMode);
				AssertEquals("whsOrderData.EntryStatus", null, whsReceiveData.EntryStatus);
				AssertEquals("whsOrderData.ExportGoodsType", null, whsReceiveData.ExportGoodsType);
				AssertEquals("whsOrderData.FirstBuyerContact", null, whsReceiveData.FirstBuyerContact);
				AssertEquals("whsOrderData.Folio", null, whsReceiveData.Folio);

				AssertEquals("whsOrderData.FreightRate", null, whsReceiveData.FreightRate);
				AssertEquals("whsOrderData.FreightRateCurrency", null, whsReceiveData.FreightRateCurrency);
				AssertEquals("whsOrderData.GoodsDescription", "", whsReceiveData.GoodsDescription);
				AssertEquals("whsOrderData.GoodsValue", 0m, whsReceiveData.GoodsValue);
				AssertEquals("whsOrderData.GoodsValueCurrency", "", whsReceiveData.GoodsValueCurrency.Code);
				AssertEquals("whsOrderData.HBLAWBChargesDisplay", null, whsReceiveData.HBLAWBChargesDisplay);
				AssertEquals("whsOrderData.HBLContainerPackModeOverride", null, whsReceiveData.HBLContainerPackModeOverride);

				AssertEquals("whsOrderData.InsuranceValue", null, whsReceiveData.InsuranceValue);
				AssertEquals("whsOrderData.InsuranceValueCurrency", null, whsReceiveData.InsuranceValueCurrency);
				AssertEquals("whsOrderData.InterimReceiptNumber", null, whsReceiveData.InterimReceiptNumber);
				AssertEquals("whsOrderData.IsBooking", null, whsReceiveData.IsBooking);
				AssertEquals("whsOrderData.IsCFSRegistered", null, whsReceiveData.IsCFSRegistered);
				AssertEquals("whsOrderData.IsDirectBooking", null, whsReceiveData.IsDirectBooking);
				AssertEquals("whsOrderData.IsForwardRegistered", null, whsReceiveData.IsForwardRegistered);
				AssertEquals("whsOrderData.IsNeutralMaster", null, whsReceiveData.IsNeutralMaster);
				AssertEquals("whsOrderData.IsPersonalEffects", null, whsReceiveData.IsPersonalEffects);
				AssertEquals("whsOrderData.IsShipping", null, whsReceiveData.IsShipping);
				AssertEquals("whsOrderData.IsSplitShipment", null, whsReceiveData.IsSplitShipment);
				AssertEquals("whsOrderData.LloydsIMO", null, whsReceiveData.LloydsIMO);
				AssertEquals("whsOrderData.ManifestedChargeable", null, whsReceiveData.ManifestedChargeable);
				AssertEquals("whsOrderData.ManifestedVolume", null, whsReceiveData.ManifestedVolume);
				AssertEquals("whsOrderData.ManifestedWeight", null, whsReceiveData.ManifestedWeight);
				AssertEquals("whsOrderData.MergeBy", null, whsReceiveData.MergeBy);
				AssertEquals("whsOrderData.MessageStatus", null, whsReceiveData.MessageStatus);
				AssertEquals("whsOrderData.MessageSubType", null, whsReceiveData.MessageSubType);
				AssertEquals("whsOrderData.MessageType", null, whsReceiveData.MessageType);
				AssertEquals("whsOrderData.NoCopyBills", null, whsReceiveData.NoCopyBills);
				AssertEquals("whsOrderData.NoOriginalBills", null, whsReceiveData.NoOriginalBills);
				AssertEquals("whsOrderData.OperationalStatus", null, whsReceiveData.OperationalStatus);
				AssertEquals("whsOrderData.OuterPacks", 5, whsReceiveData.OuterPacks);
				AssertEquals("whsOrderData.OuterPacksPackageType.Code", "CTN", whsReceiveData.OuterPacksPackageType.Code);
				AssertEquals("whsOrderData.OuterPacksPackageType.Description", "Carton", whsReceiveData.OuterPacksPackageType.Description);

				AssertEquals("whsOrderData.OwnerRef", null, whsReceiveData.OwnerRef);
				AssertEquals("whsOrderData.PackingOrder", null, whsReceiveData.PackingOrder);
				AssertEquals("whsOrderData.PaymentMethod", null, whsReceiveData.PaymentMethod);
				AssertEquals("whsOrderData.QuoteNumber", null, whsReceiveData.QuoteNumber);
				AssertEquals("whsOrderData.ReleaseType", null, whsReceiveData.ReleaseType);
				AssertEquals("whsOrderData.ScreeningStatus.Code", ScreeningStatusesList.Codes.NotScreened, whsReceiveData.ScreeningStatus.Code);
				AssertEquals("whsOrderData.ScreeningStatus.Description", ScreeningStatusesList.Descriptions.NotScreened, whsReceiveData.ScreeningStatus.Description);
				AssertEquals("whsOrderData.SecondBuyerContact", null, whsReceiveData.SecondBuyerContact);
				AssertEquals("whsOrderData.ServiceLevel.Code", "TSL", whsReceiveData.ServiceLevel.Code);
				AssertEquals("whsOrderData.ServiceLevel.Description", "Test Service Level", whsReceiveData.ServiceLevel.Description);
				AssertEquals("whsOrderData.ShipmentIncoTerm", null, whsReceiveData.ShipmentIncoTerm);
				AssertEquals("whsOrderData.ShipmentStatus", null, whsReceiveData.ShipmentStatus);
				AssertEquals("whsOrderData.ShipmentType", null, whsReceiveData.ShipmentType);
				AssertEquals("whsOrderData.ShippedOnBoard", null, whsReceiveData.ShippedOnBoard);

				AssertEquals("whsOrderData.ShipperCODAmount", null, whsReceiveData.ShipperCODAmount);
				AssertEquals("whsOrderData.ShipperCODPayMethod", null, whsReceiveData.ShipperCODPayMethod);
				AssertEquals("whsOrderData.TotalNoOfPacks", null, whsReceiveData.TotalNoOfPacks);
				AssertEquals("whsOrderData.TotalNoOfPacksDecimal", null, whsReceiveData.TotalNoOfPacksDecimal);
				AssertEquals("whsOrderData.TotalNoOfPacksPackageType", null, whsReceiveData.TotalNoOfPacksPackageType);
				AssertEquals("whsOrderData.TotalNoOfPieces", null, whsReceiveData.TotalNoOfPieces);
				AssertEquals("whsOrderData.TotalVolume", 23.3m, whsReceiveData.TotalVolume);
				AssertEquals("whsOrderData.TotalVolumeUnit.Code", "CF", whsReceiveData.TotalVolumeUnit.Code);
				AssertEquals("whsOrderData.TotalVolumeUnit.Description", "Cubic Feet", whsReceiveData.TotalVolumeUnit.Description);
				AssertEquals("whsOrderData.TotalWeight", 45.8m, whsReceiveData.TotalWeight);
				AssertEquals("whsOrderData.TotalWeightUnit.Code", "LB", whsReceiveData.TotalWeightUnit.Code);
				AssertEquals("whsOrderData.TotalWeightUnit.Description", "Pounds", whsReceiveData.TotalWeightUnit.Description);
				AssertEquals("whsOrderData.TranshipToOtherCFS", null, whsReceiveData.TranshipToOtherCFS);

				AssertEquals("whsOrderData.TransportMode", null, whsReceiveData.TransportMode);
				AssertEquals("whsOrderData.PortOfOrigin", null, whsReceiveData.PortOfOrigin);
				AssertEquals("whsOrderData.PortOfLoading", null, whsReceiveData.PortOfLoading);
				AssertEquals("whsOrderData.PortOfFirstArrival", null, whsReceiveData.PortOfFirstArrival);
				AssertEquals("whsOrderData.PortOfDischarge", null, whsReceiveData.PortOfDischarge);
				AssertEquals("whsOrderData.PortOfDestination", null, whsReceiveData.PortOfDestination);

				AssertEquals("whsOrderData.VesselName", null, whsReceiveData.VesselName);
				AssertEquals("whsOrderData.VoyageFlightNo", null, whsReceiveData.VoyageFlightNo);
				AssertEquals("whsOrderData.WarehouseLocation", null, whsReceiveData.WarehouseLocation);
				AssertEquals("whsOrderData.WarehouseReleaseStatus", null, whsReceiveData.WarehouseReleaseStatus);
				AssertEquals("whsOrderData.WayBillNumber", "BILL", whsReceiveData.WayBillNumber);
				AssertEquals("whsOrderData.WayBillType.Code", "HWB", whsReceiveData.WayBillType.Code);
				AssertEquals("whsOrderData.WayBillType.Description", "House Waybill", whsReceiveData.WayBillType.Description);

				var order = whsReceiveData.Order;
				AssertEquals("order.AddPalletWeightToOrder", null, order.AddPalletWeightToOrder);
				AssertEquals("order.ClientReference", "CUSTOMER", order.ClientReference);
				AssertEquals("order.TotalLineVolume", 23.3m, order.TotalLineVolume);
				AssertEquals("order.DropMode.Code", "HSL", order.DropMode.Code);
				AssertEquals("order.DropMode.Description", "Haulier Supplies Lift", order.DropMode.Description);
				AssertEquals("order.LocalCartageInsuranceValue", null, order.LocalCartageInsuranceValue);
				AssertEquals("order.TotalNetWeightSent", null, order.TotalNetWeightSent);
				AssertEquals("order.OrderNumber", "ORDER123", order.OrderNumber);
				AssertEquals("order.OrderNumberSplit", new ZByte(1), order.OrderNumberSplit);
				AssertEquals("order.PickOption", null, order.PickOption);
				AssertEquals("order.StagingArea", "DockA", order.StagingArea);

				AssertEquals("order.Status.Code", "ENT", order.Status.Code);
				AssertEquals("order.Status.Description", "Entered (Saved)", order.Status.Description);
				AssertEquals("order.TotalUnits", 7.1m, order.TotalUnits);
				AssertEquals("order.TotalLineWeight", 45.8m, order.TotalLineWeight);
				AssertEquals("order.TransportReference", "TREEE", order.TransportReference);
				AssertEquals("order.Type.Code", "CUS", order.Type.Code);
				AssertEquals("order.Type.Description", "CUSTOMS RECEIPT", order.Type.Description);
				AssertEquals("order.UnitsSent", null, order.UnitsSent);
				AssertEquals("order.Warehouse.Code", "WHS", order.Warehouse.Code);
				AssertEquals("order.Warehouse.Name", "Ware this!", order.Warehouse.Name);
				AssertEquals("order.FulfillmentRule", null, order.FulfillmentRule);
				AssertEquals("order.HoldPalletIDPutaway", ZBool.True, order.HoldPalletIDPutaway);

				AssertEquals("order.DateCollection.Count", 4, order.DateCollection.Count);
				order.DateCollection.AssertDateExists(DateType.BookingConfirmed, ZBool.False, new ZDateTime(2011, 1, 1));
				order.DateCollection.AssertDateExists(DateType.Departure, ZBool.True, new ZDateTime(2011, 1, 2));
				order.DateCollection.AssertDateExists(DateType.Arrival, ZBool.True, new ZDateTime(2011, 1, 3));
				order.DateCollection.AssertDateExists(DateType.Arrival, ZBool.False, new ZDateTime(2011, 1, 4));
				AssertEquals("Checking dates left, and found dates not expected.", 0, order.DateCollection.Count);
			});

			#endregion
		}

		#endregion

		#region TestCollections

		#region TestPalletsSent

		public void TestPalletsSent()
		{
			var whsReceiveBO = Factory.New<WhsReceive>();
			whsReceiveBO.WD_TotalPallets = new ZShort(3);

			var whsReceiveData = new WhsReceiveDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsReceiveBO))).GetDataObject(whsReceiveBO);
			AssertEquals("receive.PalletsSent", new ZShort(3), whsReceiveData.Order.PalletsSent);
		}

		#endregion

		#region TestInventoryLines

		public void TestInventoryLines()
		{
			var line = WhsReceiveLineDataObjectWriterTest.GetReceiveLine(Factory);
			var whsReceiveBO = line.Docket;

			var whsReceiveData = new WhsReceiveDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsReceiveBO))).GetDataObject(whsReceiveBO);

			AssertNotNull("whsReceiveData", whsReceiveData);
			AssertEquals("whsReceiveData.Order.OrderLineCollection.Count", 1, whsReceiveData.Order.OrderLineCollection.Count);

			CombineAssertions(delegate
			{
				var receiveLineDataObject = whsReceiveData.Order.OrderLineCollection[0];
				WhsReceiveLineDataObjectWriterTest.AssertContents(receiveLineDataObject);
			});
		}

		#endregion

		#region TestCreateTransportBooking

		public void TestCreateTransportBooking()
		{
			var receiveQty = 20m;
			var org = Data.CreateClientOrgCRAHOLSYDInDB();
			var whs = Data.GetOrCreateWarehouseInDB();
			var product1 = Data.CreateProduct("P1");

			var receive = Helper.CreateWhsReceive(org, whs, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, product1, receiveQty);

			Factory.SaveForTesting();

			var factory = new BusinessObjectFactory();
			using (factory.AddDisposableService())
			{
				UniversalXmlWorkflowProcessor.PublishUniversalShipment(factory, GlbCompany.CurrentCompany.OrgProxy, new RecipientRoleType[1] { RecipientRoleType.CTG }, receive);
				factory.Save();
			}

			var booking = (IDtbBooking)TransportBookingLoader.GetRelatedTransportBookingEvents(receive).Single();
			var result = ((PkgPackageJob)booking.PackageJob).GetAllPackagesOnJob();

			AssertEquals("TB should have 20", 1, result.Count(p => p.KP_PackageQty == 20));
		}

		#endregion

		#region TestReceiveCategoryCode

		public void TestReceiveCategoryCode()
		{
			var whsReceiveBO = Factory.New<WhsReceive>();
			whsReceiveBO.WD_ReceiveCategory = "ABC";

			var whsReceiveData = new WhsReceiveDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsReceiveBO))).GetDataObject(whsReceiveBO);
			AssertEquals("receive.ReceiveCategory", "ABC", whsReceiveData.Order.Category);
		}

		#endregion

		#endregion

		#region TestContainerMode

		public void TestContainerMode()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var whsReceiveBO = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);

			var whsReceiveData1 = new WhsReceiveDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsReceiveBO))).GetDataObject(whsReceiveBO);
			AssertEquals("LSE", whsReceiveData1.ContainerMode.Code);

			whsReceiveBO.Containers.AddNew();
			var whsReceiveData2 = new WhsReceiveDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, whsReceiveBO))).GetDataObject(whsReceiveBO);
			AssertEquals("FCL", whsReceiveData2.ContainerMode.Code);
		}

		#endregion

		#region Implementation

		protected override WhsReceive GetNewDocket()
		{
			return Factory.NewWithValidTestData<WhsReceive>();
		}

		protected override WhsReceiveDataObjectWriter GetNewDataObjectWriter(BusinessObject topLevelBO, INotifications notifications)
			=> new WhsReceiveDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, topLevelBO) { Notifications = notifications }));

		protected override void SetPalletsSent(WhsReceive whsReceiveBO, ZShort palletsSent)
		{
			whsReceiveBO.WD_TotalPallets = palletsSent;
		}

		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseOrder);

		#endregion
	}
}
