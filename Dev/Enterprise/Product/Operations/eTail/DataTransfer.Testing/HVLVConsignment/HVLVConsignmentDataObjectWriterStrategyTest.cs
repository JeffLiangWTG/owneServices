using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.NZ;
using OrgSupplierPart = Enterprise.MasterFiles.Business.OrgSupplierPart;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class HVLVConsignmentDataObjectWriterStrategyTest : TestCaseWithFactory
	{
		protected virtual IHVLVConsignmentDataExportStrategy DataExportStrategy => DefaultHVLVConsignmentDataExportStrategy.Instance;

		protected TopLevelDataObjectWriter<HVLVConsignment, Shipment> GetDataObjectWriterForTest(BusinessObject topLevelBO)
		{
			var writeManager = new DataWritingManager(new ActionInfo(null, topLevelBO));
			return new HVLVConsignmentDataObjectWriter(writeManager, DataExportStrategy);
		}
	}

	public class DefaultHVLVConsignmentDataExportStrategyTest : HVLVConsignmentDataObjectWriterStrategyTest
	{
		public void TestNoExceptionThrown_WhenWriteInvalidUnit()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_VolumeUQ = "KG";
			consignment.HVC_WeightUQ = "KG";

			consignment.Items.AddNew();

			var writer = GetDataObjectWriterForTest(consignment);
			AssertNoExceptionThrown(() =>
			{
				writer.GetDataObject(consignment);
			});
		}

		public void TestWhenConsignmentDoesNotHasAnyItem_ThrowException()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_WaybillNumber = "testConsignment";

			var writer = GetDataObjectWriterForTest(consignment);

			AssertExceptionThrown(typeof(DataObjectValidationException), string.Format("Item data is missing for {0}", consignment.HumanReadableName), () =>
			{
				writer.GetDataObject(consignment);
			});
		}

		public void TestDataObjectHasOneLevelOfSubShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = ShipmentTypes.StandardHouse;

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment1.PK;
			consignment1.HVC_GoodsDescription = "Test Goods #1";
			consignment1.Items.AddNew();

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_JS_ManifestedOnShipment = shipment1.PK;
			consignment2.HVC_GoodsDescription = "Test Goods #2";
			consignment2.Items.AddNew();

			var writer = GetDataObjectWriterForTest(consignment1);
			var dataObject = writer.GetDataObject(consignment1);

			AssertNotNull("Top level should be consol", dataObject.GetMatchingDataSource(DataContextType.ForwardingConsol));
			AssertEquals("Should contain one and only one 1st level sub-shipment", 1, dataObject.SubShipmentCollection.Count);

			var consignmentData = dataObject.SubShipmentCollection.Single();
			AssertNotNull("1st level sub-shipment should be consignment", consignmentData.GetMatchingDataSource(DataContextType.HVLVConsignment));
			AssertEquals("1st level sub-shipment should contain consignment data", consignment1.HVC_GoodsDescription, consignmentData.GoodsDescription);
		}

		public void TestDataSource_ParentBoIsHVLVConsignment_WhenShipmentAttachedToConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.Items.AddNew();

			var writer = GetDataObjectWriterForTest(consignment);
			var dataObject = writer.GetDataObject(consignment);

			CombineAssertions("data source", () =>
			{
				AssertEquals(3, dataObject.DataContext.DataSourceCollection.Count());
				AssertNotNull("Should have a consol top-level data source", dataObject.GetMatchingDataSource(DataContextType.ForwardingConsol));
				AssertNotNull("Should have a shipment top-level data source", dataObject.GetMatchingDataSource(DataContextType.ForwardingShipment));
				AssertNotNull("Should have a consignment top-level data source", dataObject.GetMatchingDataSource(DataContextType.HVLVConsignment));
			});
		}

		public void TestDataSource_ParentBoIsHVLVConsignment_WhenShipmentDoNotAttachedToConsol()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.Items.AddNew();

			var writer = GetDataObjectWriterForTest(consignment);
			var dataObject = writer.GetDataObject(consignment);

			CombineAssertions("data source", () =>
			{
				AssertEquals(2, dataObject.DataContext.DataSourceCollection.Count());
				AssertNotNull("Should have a shipment top-level data source", dataObject.GetMatchingDataSource(DataContextType.ForwardingShipment));
				AssertNotNull("Should have a consignment top-level data source", dataObject.GetMatchingDataSource(DataContextType.HVLVConsignment));
			});
		}

		public void TestDataSource_ParentBoIsNotHVLVConsignment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Test Goods #1";
			consignment.Items.AddNew();

			var writer = GetDataObjectWriterForTest(shipment);
			var dataObject = writer.GetDataObject(consignment);

			CombineAssertions("data source", () =>
			{
				AssertEquals(1, dataObject.DataContext.DataSourceCollection.Count());
				AssertNotNull("Should have a consignment top-level data source", dataObject.GetMatchingDataSource(DataContextType.HVLVConsignment));
			});

			writer = GetDataObjectWriterForTest(bookingHeader);
			dataObject = writer.GetDataObject(consignment);

			CombineAssertions("data source", () =>
			{
				AssertEquals(1, dataObject.DataContext.DataSourceCollection.Count());
				AssertNotNull("Should have a consignment top-level data source", dataObject.GetMatchingDataSource(DataContextType.HVLVConsignment));
			});
		}

		public void TestWriteToDataObject()
		{
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "&&&", "COVID-19 go away!");
			var hvlShipments = PopulateAndGetHVLShipment();
			var consignment = hvlShipments.HVLVConsignments.Cast<HVLVConsignment>().Single(x => x.HVC_ConsignmentId == "CONSIFIRST");

			var writer = GetDataObjectWriterForTest(consignment);
			var dataObject = writer.GetDataObject(consignment);

			var consignmentData = dataObject.SubShipmentCollection[0];
			AssertEquals("Local Description", "键盘", consignmentData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].LocalDescription);
			AssertEquals("LMC account DepotID", "Test Depot ID", consignmentData.CarrierAccount.DepotID);
			AssertEquals("LMC service code", "999", consignmentData.CarrierServiceLevel.CarrierServiceCode);
			AssertEquals("LMC service level code", "EXP", consignmentData.CarrierServiceLevel.Code);
			AssertEquals("LMC service level description", "99.9% faster service", consignmentData.CarrierServiceLevel.Description);

			AssertEquals("Master waybill type", "MWB", dataObject.WayBillType.Code);
			AssertEquals("Master waybill number", "", dataObject.WayBillNumber);

			AssertEquals("Flight", "SSS111222", dataObject.VoyageFlightNo);
			AssertEquals("Vessel", "HighWind", dataObject.VesselName);

			AssertEquals("Origin port", "AUBNE", dataObject.PortOfLoading.Code);
			AssertEquals("Destination port", "AUSYD", dataObject.PortOfDischarge.Code);

			AssertEquals("Customs clearance status code", "&&&", consignmentData.ConsolidatedCargoStatus.Code);
			AssertEquals("Customs clearance status description", "COVID-19 go away!", consignmentData.ConsolidatedCargoStatus.Description);
		}

		public void TestPopulateEstimateTime()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_E_DEP = ZDateTime.BrettsBirthday;
			shipment.JS_E_ARV = ZDateTime.BrettsBirthday.AddDays(1);

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.Items.AddNew();

			var writer = GetDataObjectWriterForTest(consignment);
			var dataObject = writer.GetDataObject(consignment);

			CombineAssertions("Populate estimate time when write a single consignment XUS", () =>
			{
				AssertEquals(ZDateTime.BrettsBirthday, dataObject.DateCollection.Single(d => d.Type == DateType.Departure).Value);
				AssertEquals(true, dataObject.DateCollection.Single(d => d.Type == DateType.Departure).IsEstimate);
				AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), dataObject.DateCollection.Single(d => d.Type == DateType.Arrival).Value);
				AssertEquals(true, dataObject.DateCollection.Single(d => d.Type == DateType.Arrival).IsEstimate);
			});

			writer = GetDataObjectWriterForTest(shipment);
			dataObject = writer.GetDataObject(consignment);

			AssertNull("Do not populate estimate time when write consignment as subshipment", dataObject.DateCollection);
		}

		public void TestPopulateNotifyParty()
		{
			var notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			var address = notifyParty.Addresses.MainAddress;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = address.PK;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.Items.AddNew();

			var writer = GetDataObjectWriterForTest(consignment);
			var dataObject = writer.GetDataObject(consignment);

			var expectNotifyParty = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.NotifyParty)).OrganizationCode;

			AssertEquals("notify party of consignment are from shipment.NotifyParty", expectNotifyParty, shipment.NotifyParty.OH_Code);
		}

		#region Implementation

		public ForwardingShipment PopulateAndGetHVLShipment()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			#region orgs
			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			billToParty.MainAddress.Address1 = "45 Bill To Street";
			billToParty.Contacts.AddNew().OC_ContactName = "Bill";

			var destinationDepotOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			destinationDepotOrg1.MainAddress.Address1 = "12 Road Nanjing";
			var lastMileDelivery = Factory.NewWithValidTestData<OrgHeader>();
			lastMileDelivery.OH_IsShippingProvider = true;
			lastMileDelivery.OH_IsLocalTransport = true;
			lastMileDelivery.MainAddress.Address1 = "99 Road Shanghai";

			var carrierAccount = lastMileDelivery.CarrierAccounts.AddNew();
			carrierAccount.OAN_AccountNumber = "123456";
			carrierAccount.OAN_DepotID = "Test Depot ID";

			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			lastMileDelivery.MiscServ = miscServ;

			var serviceLevel = miscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "EXP";
			serviceLevel.PL_CarrierServiceCode = "999";
			serviceLevel.PL_CarrierServiceLevelDescription = "99.9% faster service";

			var lastMileCarrierBookingAgent = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrierBookingAgent.MainAddress.Address1 = "66 Road Beijing";

			var destinationDepotOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			destinationDepotOrg2.MainAddress.Address1 = "13 Destination St";

			var cusclass = Factory.New<BaseCusClassification>();
			cusclass.CC_LookupCode = "1234";
			cusclass.CC_ClassificationType = Customs.Common.ClassificationType.EXP;
			cusclass.CC_TariffNum = "4321";
			#region shipment in consignment1 setup(set consignment.DirectionOfTrade = Directions.Import)
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_UniqueConsignRef = "SHIP000001";

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "HighWind";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "SSS111222";
			voyage.JV_AirSeaRoad = "SEA";
			voyage.JV_VoyageType = "MAI";

			var origin = voyage.Origins.AddNew();
			origin.JA_A_DEP = new ZDateTime(2020, 5, 12);
			origin.JA_RL_NKPortOfLoading = "AUBNE";

			var destination = voyage.Destinations.AddNew();
			destination.JB_A_ARV = new ZDateTime(2020, 5, 13);
			destination.JB_RL_NKPortOfDischarge = "AUSYD";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var leg = arrivalConsol.Transports[0];
			leg.JW_ETD = new ZDateTime(2020, 5, 12);
			leg.JW_ETA = new ZDateTime(2020, 5, 13);

			leg.JW_Vessel = vessel.RV_Code;
			leg.JW_VoyageFlight = "SSS111222";
			#endregion

			Factory.Save();
			#endregion

			#region set up
			#region header property
			var bookingHeader = Factory.New<HVLVBookingHeader>();

			bookingHeader.HVH_OA_BillToParty = billToParty.MainAddress.PK;
			bookingHeader.HVH_OC_BillToPartyContact = billToParty.Contacts[0].PK;
			#endregion

			#region consignments
			#region consignment1
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment1 = consignmentHeader.Consignments.AddNew();

			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_ConsignmentId = "CONSIFIRST";
			consignment1.HVC_WaybillNumber = "XA13DQ";
			consignment1.HVC_ShipperReference = "123ABC";
			consignment1.HVC_ItemCount = 0;
			consignment1.HVC_GoodsValue = 200;
			consignment1.HVC_RX_NKGoodsValueCurrency = "BGN";
			consignment1.HVC_WeightUQ = Weight.Kilograms;
			consignment1.HVC_VolumeUQ = Volume.CubicMetres;
			consignment1.HVC_GoodsDescription = "GUNS";
			consignment1.HVC_ConsigneeInstructions = "Leave at front";
			consignment1.HVC_INCO = IncoTerms.DeliveredAtFrontier;
			consignment1.HVC_IsHazardous = true;
			consignment1.HVC_IsSignatureRequired = true;
			consignment1.HVC_AuthorityToLeave = true;
			consignment1.HVC_RequiresFumigation = true;
			consignment1.HVC_IsPersonalEffects = true;
			consignment1.HVC_IsTimber = true;
			consignment1.HVC_IsPerishable = true;
			consignment1.HVC_UndgClass = "3.3D";
			consignment1.HVC_PL_NKLastMileCarrierServiceLevel = "EXP";
			consignment1.HVC_IsTaxPrePaid = true;

			consignment1.HVC_OA_DestinationDepot = destinationDepotOrg1.MainAddress.PK;

			consignment1.HVC_OH_LastMileCarrier = lastMileDelivery.PK;
			consignment1.HVC_CarrierAccountNumber = carrierAccount.OAN_AccountNumber;

			consignment1.HVC_OH_LastMileCarrierBookingAgent = lastMileCarrierBookingAgent.PK;

			consignment1.HVC_ConsigneeName = "Murray";
			consignment1.HVC_ConsigneeAddress1 = "99 Consignee Road";
			consignment1.HVC_ConsigneeAddress2 = "Downtown";
			consignment1.HVC_ConsigneeCity = "New York";
			consignment1.HVC_ConsigneeState = "NY";
			consignment1.HVC_ConsigneePostcode = "12345";
			consignment1.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment1.HVC_ConsigneeContact = "Moo ray";
			consignment1.HVC_ConsigneeEmail = "murray.hewitt@usconsulate.gov.nz";
			consignment1.HVC_ConsigneePhone = "7";
			consignment1.HVC_ConsigneeMobile = "+1234567890";
			consignment1.HVC_ConsigneeFax = "+0987654321";

			consignment1.HVC_ShipperName = "Some Company";
			consignment1.HVC_ShipperAddress1 = "22 Shipper Street";
			consignment1.HVC_ShipperCity = "Wellington";
			consignment1.HVC_ShipperState = "WLG";
			consignment1.HVC_ShipperPostcode = "54321";
			consignment1.HVC_RN_NKShipperCountryCode = "NZ";
			consignment1.HVC_ShipperContact = "Randy";
			consignment1.HVC_ShipperEmail = "randy@randysdomain.com";
			consignment1.HVC_ShipperPhone = "01189998819991197253";
			consignment1.HVC_ShipperMobile = "+555 5555";
			consignment1.HVC_ShipperFax = "8";

			consignment1.HVC_VendorIdentifier = "VID0001";

			consignment1.HVC_ImportCustomsClearanceStatus = "&&&";
			#endregion
			#endregion

			#region Items

			var item1_1 = consignment1.Items.AddNew();
			item1_1.HVI_ItemId = "D9901239028";
			item1_1.HVI_ShipperReference = "S999887";
			item1_1.HVI_CurrentBarcode = "12345678901234567890123456789012345678901234567890";
			item1_1.HVI_F3_NKPackType = "BOX";
			item1_1.HVI_Height = 3m;
			item1_1.HVI_Length = 4m;
			item1_1.HVI_Width = 5m;
			item1_1.HVI_UnitOfDimension = Length.Metres;
			item1_1.HVI_ManifestedWeight = 5;
			item1_1.HVI_ActualWeight = 10;
			item1_1.HVI_ManifestedVolume = 0.5;
			item1_1.HVI_ActualVolume = 0.5;
			item1_1.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;
			item1_1.HVI_IsDamaged = true;
			item1_1.HVI_IsPillaged = true;

			var item1_2 = consignment1.Items.AddNew();
			item1_2.HVI_ItemId = "D5465421481";
			item1_2.HVI_F3_NKPackType = "BOX";
			item1_2.HVI_ManifestedWeight = 10;
			item1_2.HVI_ActualWeight = 10;
			item1_2.HVI_ManifestedVolume = 0.5;
			item1_2.HVI_ActualVolume = 0.5;
			item1_2.HVI_Status = HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot;
			item1_2.HVI_IsDamaged = true;
			item1_2.HVI_IsPillaged = false;
			#endregion

			#region Item Lines

			var line1_1_1 = item1_1.Lines.AddNew();
			line1_1_1.HVS_OriginTariff = "123456";
			line1_1_1.HVS_RN_NKOriginCountryCode = "AU";
			line1_1_1.HVS_CustomsValue = 12.34;
			line1_1_1.HVS_IntrinsicValue = 54.321;
			line1_1_1.HVS_DestinationTariff = "654321";
			line1_1_1.HVS_GoodsDescription = "Keyboard";
			line1_1_1.HVS_OriginGoodsDescription = "键盘";
			line1_1_1.HVS_GrossWeight = 1.23;
			line1_1_1.HVS_ItemURL = "www.google.com/keyboard";
			line1_1_1.HVS_NetWeight = 3.21;
			var product = Factory.New<Customs.Business.OrgSupplierPart>();
			product.OP_PartNum = "KBD001";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = bookingHeader.BillToParty.OA_OH;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			line1_1_1.HVS_ProductCode = "KBD001";
			line1_1_1.HVS_CC_Lookup = cusclass.PK;
			line1_1_1.HVS_Quantity = 2;
			line1_1_1.HVS_WeightUnit = "KG";

			var line1_1_2 = item1_1.Lines.AddNew();
			line1_1_2.HVS_OriginTariff = "12345678";
			line1_1_2.HVS_RN_NKOriginCountryCode = "NZ";
			line1_1_2.HVS_DestinationTariff = "87654321";
			line1_1_2.HVS_GrossWeight = 2.34;
			line1_1_2.HVS_NetWeight = 4.32;
			line1_1_2.HVS_Quantity = 3;
			line1_1_2.HVS_WeightUnit = "T";
			line1_1_2.HVS_GoodsDescription = "Screen";

			var line1_2_1 = item1_2.Lines.AddNew();
			line1_2_1.HVS_OriginTariff = "123123";
			line1_2_1.HVS_RN_NKOriginCountryCode = "FR";
			line1_2_1.HVS_DestinationTariff = "321321";
			line1_2_1.HVS_GrossWeight = 1.2;
			line1_2_1.HVS_NetWeight = 3.4;
			line1_2_1.HVS_Quantity = 4;
			line1_2_1.HVS_WeightUnit = "KG";
			line1_2_1.HVS_GoodsDescription = "Mouse";
			#endregion
			#endregion

			return shipment;
		}

		#endregion
	}

	public class HVLVConsignmentDataExportStrategyWithoutNotesTest : HVLVConsignmentDataObjectWriterStrategyTest
	{
		public void TestDefaultExportStrategy_WithoutNotes()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.Items.AddNew();

			var writeManager = new DataWritingManager(new ActionInfo(null, consignment));
			var writer = new HVLVConsignmentDataObjectWriter(writeManager, HVLVConsignmentDataExportStrategyWithoutNotes.Instance);

			var dataObject = writer.GetDataObject(consignment);
			AssertNull("No notes", dataObject.InstructionCollection);

			consignment.Notes.AddNew(false, "Description", "qwerty");
			dataObject = writer.GetDataObject(consignment);
			AssertNull("Still no notes", dataObject.InstructionCollection);
		}

		protected override IHVLVConsignmentDataExportStrategy DataExportStrategy => HVLVConsignmentDataExportStrategyWithoutNotes.Instance;
	}

	public class HVLVConsignmentToCargoReportDataExportStrategyTest : HVLVConsignmentDataObjectWriterStrategyTest
	{
		public void TestWriteToDataObject()
		{
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "&&&", "COVID-19 go away!", "HLD");
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "&&&", "COVID-19 go away!", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);

			var bookingHeader = PopulateAndGetBookingHeader();
			var consignment1 = bookingHeader.Consignments.Cast<HVLVConsignment>().Single(x => x.HVC_ConsignmentId == "CONSIFIRST");
			var consignment2 = bookingHeader.Consignments.Cast<HVLVConsignment>().Single(x => x.HVC_ConsignmentId == "SECONDCONSIGN");

			var dataObject1 = GetDataObjectWriterForTest(consignment1).GetDataObject(consignment1);
			var dataObject2 = GetDataObjectWriterForTest(consignment2).GetDataObject(consignment2);

			#region assert consignment1
			#region Main properties
			AssertEquals(1, dataObject1.DataContext.DataSourceCollection.Count());
			AssertEquals("XA13DQ", dataObject1.WayBillNumber);
			AssertEquals("House Waybill", dataObject1.WayBillType.Description);
			AssertEquals("HWB", dataObject1.WayBillType.Code);
			AssertEquals("123ABC", dataObject1.OwnerRef);
			AssertEquals(2, dataObject1.TotalNoOfPieces);
			AssertEquals("BOX", dataObject1.TotalNoOfPacksPackageType.Code);
			AssertEquals(2, dataObject1.OuterPacks);
			AssertEquals("BOX", dataObject1.OuterPacksPackageType.Code);
			AssertEquals((ZDecimal)200, dataObject1.GoodsValue);
			AssertEquals("BGN", dataObject1.GoodsValueCurrency.Code);
			AssertEquals((ZDecimal)2342.43, dataObject1.ManifestedWeight);
			AssertEquals((ZDecimal)20, dataObject1.TotalWeight);
			AssertEquals(Weight.Kilograms, dataObject1.TotalWeightUnit.Code);
			AssertEquals((ZDecimal)1, dataObject1.ManifestedVolume);
			AssertEquals((ZDecimal)1, dataObject1.TotalVolume);
			AssertEquals(Volume.CubicMetres, dataObject1.TotalVolumeUnit.Code);
			AssertEquals("GUNS", dataObject1.GoodsDescription);
			AssertEquals(true, dataObject1.IsHazardous);
			AssertEquals(true, dataObject1.IsSignatureRequired);
			AssertEquals(true, dataObject1.IsAuthorizedToLeave);
			AssertEquals("EXP", dataObject1.CarrierServiceLevel.Code);
			AssertEquals("HVL", dataObject1.ShipmentType.Code.Value);
			AssertEquals(IncoTerms.DeliveredAtFrontier, dataObject1.ShipmentIncoTerm.Code);
			AssertEquals("VID0001", dataObject1.VendorIdentifier);
			AssertEquals("CLR", dataObject1.WarehouseReleaseStatus.Code);
			AssertEquals("&&&", dataObject1.ConsolidatedCargoStatus.Code);
			#endregion

			#region add info
			AssertEquals(1, dataObject1.InstructionCollection.Count);
			AssertEquals("Y", dataObject1.AddInfoCollection.Single(a => a.Key.Value == "IsGSTPrePaid").Value.Value);
			AssertEquals("Leave at front", dataObject1.InstructionCollection.Single().ServiceInstruction);
			#endregion

			#region org info
			var arrivalCFSAddress = dataObject1.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.CustomsDepotAddress));
			AssertEquals("12 Road Nanjing", arrivalCFSAddress.Address1);

			var depotAddress = dataObject1.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.ArrivalCFSAddress));
			AssertEquals("12 Road Nanjing", depotAddress.Address1);

			var lastMileCarrier = dataObject1.OrganizationAddressCollection.Single(x => x.AddressType.Value == AddressTypes.DeliveryLocalCartage);
			AssertEquals("99 Road Shanghai", lastMileCarrier.Address1);

			var lastMilePicker = dataObject1.OrganizationAddressCollection.Single(x => x.AddressType.Value == AddressTypes.PickupLocalCartage);
			AssertEquals("99 Road Shanghai", lastMilePicker.Address1);

			var lastMileCarrierAgent = dataObject1.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.CarrierBookingAgent));
			AssertEquals("66 Road Beijing", lastMileCarrierAgent.Address1);

			var consigneeAddress1 = dataObject1.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.ConsigneeDocumentaryAddress));
			AssertEquals("Murray", consigneeAddress1.CompanyName);
			AssertEquals("99 Consignee Road", consigneeAddress1.Address1);
			AssertEquals("Downtown", consigneeAddress1.Address2);
			AssertEquals("New York", consigneeAddress1.City);
			AssertEquals("NY", consigneeAddress1.State);
			AssertEquals("12345", consigneeAddress1.Postcode);
			AssertEquals("AU", consigneeAddress1.Country.Code);
			AssertEquals("Moo ray", consigneeAddress1.Contact);
			AssertEquals("murray.hewitt@usconsulate.gov.nz", consigneeAddress1.Email);
			AssertEquals("7", consigneeAddress1.Phone);
			AssertEquals("+1234567890", consigneeAddress1.Mobile);
			AssertEquals("+0987654321", consigneeAddress1.Fax);
			Assert(consigneeAddress1.AddressOverride.Value);
			var consignorAddress1 = dataObject1.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.ConsignorDocumentaryAddress));
			AssertEquals("Some Company", consignorAddress1.CompanyName);
			AssertEquals("22 Shipper Street", consignorAddress1.Address1);
			AssertEquals("Wellington", consignorAddress1.City);
			AssertEquals("WLG", consignorAddress1.State);
			AssertEquals("54321", consignorAddress1.Postcode);
			AssertEquals("NZ", consignorAddress1.Country.Code);
			AssertEquals("Randy", consignorAddress1.Contact);
			AssertEquals("randy@randysdomain.com", consignorAddress1.Email);
			AssertEquals("01189998819991197253", consignorAddress1.Phone);
			AssertEquals("+555 5555", consignorAddress1.Mobile);
			AssertEquals("8", consignorAddress1.Fax);
			#endregion

			#region packing lines and items

			AssertEquals(2, dataObject1.PackingLineCollection.Count);

			var packingLine1_1 = dataObject1.PackingLineCollection.Single(x => x.OrderReference.Value == "D9901239028");
			AssertEquals("12345678901234567890123456789012345678901234567890", packingLine1_1.Barcode);
			AssertEquals("BOX", packingLine1_1.PackType.Code);
			AssertEquals((ZDecimal)2341.23, packingLine1_1.ManifestedWeight);
			AssertEquals((ZDecimal)10, packingLine1_1.Weight);
			AssertEquals(true, packingLine1_1.RequiresFumigationCertificate);
			AssertEquals(true, packingLine1_1.IsPersonalEffects);
			AssertEquals(true, packingLine1_1.IsTimber);
			AssertEquals(true, packingLine1_1.IsPerishable);
			AssertEquals(0, packingLine1_1.OutturnQty);
			AssertEquals(0, packingLine1_1.OutturnDamagedQty);
			AssertEquals(0, packingLine1_1.OutturnPillagedQty);

			AssertEquals(3m, packingLine1_1.Height);
			AssertEquals(4m, packingLine1_1.Length);
			AssertEquals(5m, packingLine1_1.Width);
			AssertEquals((ZDecimal)0.5, packingLine1_1.ManifestedVolume);
			AssertEquals((ZDecimal)0.5, packingLine1_1.Volume);
			AssertEquals(Length.Metres, packingLine1_1.LengthUnit.Code);
			AssertEquals(Volume.CubicMetres, packingLine1_1.VolumeUnit.Code);

			AssertEquals(1, packingLine1_1.UNDGCollection.Count);
			AssertEquals("3.3D", packingLine1_1.UNDGCollection[0].IMOClass);

			AssertEquals(2, packingLine1_1.PackedItemCollection.Count);
			var packedItem1_1_1 = packingLine1_1.PackedItemCollection.Single(x => x.Description.Value == "Keyboard");
			AssertEquals(54.321M, packedItem1_1_1.GoodsValue);
			AssertEquals(1.23M, packedItem1_1_1.GrossWeight);
			AssertEquals("KG", packedItem1_1_1.GrossWeightUnit.Code);
			AssertEquals(3.21M, packedItem1_1_1.NetWeight);
			AssertEquals("KG", packedItem1_1_1.NetWeightUnit.Code);
			AssertEquals(2M, packedItem1_1_1.PackedQuantity);
			AssertEquals("KBD001", packedItem1_1_1.Product.Code);
			AssertEquals(12.34M, packedItem1_1_1.CIFValue);
			AssertEquals("www.google.com/keyboard", packedItem1_1_1.ItemSpecificationUrl);

			var packedItem1_1_2 = packingLine1_1.PackedItemCollection.Single(x => x.Description.Value == "Screen");
			AssertEquals(2.34M, packedItem1_1_2.GrossWeight);
			AssertEquals("T", packedItem1_1_2.GrossWeightUnit.Code);
			AssertEquals(4.32M, packedItem1_1_2.NetWeight);
			AssertEquals("T", packedItem1_1_2.NetWeightUnit.Code);
			AssertEquals(3M, packedItem1_1_2.PackedQuantity);

			var packingLine1_2 = dataObject1.PackingLineCollection.Single(x => x.OrderReference.Value == "D5465421481");
			AssertEquals("BOX", packingLine1_2.PackType.Code);
			AssertEquals((ZDecimal)1.2, packingLine1_2.ManifestedWeight);
			AssertEquals((ZDecimal)10, packingLine1_2.Weight);
			AssertEquals((ZDecimal)0.5, packingLine1_2.ManifestedVolume);
			AssertEquals((ZDecimal)0.5, packingLine1_2.Volume);
			AssertEquals(true, packingLine1_2.RequiresFumigationCertificate);
			AssertEquals(true, packingLine1_2.IsPersonalEffects);
			AssertEquals(true, packingLine1_2.IsTimber);
			AssertEquals(true, packingLine1_2.IsPerishable);
			AssertEquals(1, packingLine1_2.OutturnQty);
			AssertEquals(1, packingLine1_2.OutturnDamagedQty);
			AssertEquals(0, packingLine1_2.OutturnPillagedQty);

			AssertEquals(1, packingLine1_2.UNDGCollection.Count);
			AssertEquals("3.3D", packingLine1_1.UNDGCollection[0].IMOClass);

			AssertEquals(1, packingLine1_2.PackedItemCollection.Count);
			var packedItem1_2_1 = packingLine1_2.PackedItemCollection.Single(x => x.Description.Value == "Mouse");
			AssertEquals(1.2M, packedItem1_2_1.GrossWeight);
			AssertEquals("KG", packedItem1_2_1.GrossWeightUnit.Code);
			AssertEquals(3.4M, packedItem1_2_1.NetWeight);
			AssertEquals("KG", packedItem1_2_1.NetWeightUnit.Code);
			AssertEquals(4M, packedItem1_2_1.PackedQuantity);
			#endregion

			#region CommercialInvoiceHeader
			var commercialInvoiceHeader1 = dataObject1.CommercialInfo.CommercialInvoiceCollection.Single();
			AssertEquals((ZDecimal)200, commercialInvoiceHeader1.InvoiceAmount);
			AssertEquals("BGN", commercialInvoiceHeader1.InvoiceCurrency.Code);
			AssertEquals((ZDecimal)2342.43, commercialInvoiceHeader1.Weight);
			AssertEquals((ZDecimal)4326.61, commercialInvoiceHeader1.NetWeight);
			AssertEquals(Weight.Kilograms, commercialInvoiceHeader1.NetWeightUQ.Code);
			AssertEquals("Y", commercialInvoiceHeader1.AddInfoCollection.Single(x => x.Key.ToString() == Constants.AddInfoKeys.IsGSTPrePaid).Value);
			AssertEquals("VID0001", commercialInvoiceHeader1.AddInfoCollection.Single(x => x.Key.ToString() == Constants.AddInfoKeys.SupplierGSTNumber).Value);
			#endregion

			#region CommercialInvoiceLine
			AssertEquals(3, commercialInvoiceHeader1.CommercialInvoiceLineCollection.Count);

			var commercialInvoiceLine1_1_1 = commercialInvoiceHeader1.CommercialInvoiceLineCollection.Single(x => x.Link == packedItem1_1_1.CommercialInvoiceLineLink);
			AssertEquals("4321", commercialInvoiceLine1_1_1.HarmonisedCode);
			AssertEquals(12.34M, commercialInvoiceLine1_1_1.CustomsValue);
			AssertEquals("Keyboard", commercialInvoiceLine1_1_1.Description);
			AssertEquals(3.21M, commercialInvoiceLine1_1_1.NetWeight);
			AssertEquals(1.23M, commercialInvoiceLine1_1_1.Weight);
			AssertEquals("KG", commercialInvoiceLine1_1_1.WeightUnit.Code);
			AssertEquals(2M, commercialInvoiceLine1_1_1.CustomsQuantity);
			AssertEquals("KBD001", commercialInvoiceLine1_1_1.PartNo);
			AssertEquals("1234", commercialInvoiceLine1_1_1.ClassificationCode);
			AssertEquals((ZDecimal)2, commercialInvoiceLine1_1_1.InvoiceQuantity);
			AssertEquals(12.34M, commercialInvoiceLine1_1_1.LinePrice);
			AssertEquals("AU", commercialInvoiceLine1_1_1.CountryOfOrigin.Code);

			var commercialInvoiceLine1_1_2 = commercialInvoiceHeader1.CommercialInvoiceLineCollection.Single(x => x.Link == packedItem1_1_2.CommercialInvoiceLineLink);
			AssertEquals("87654321", commercialInvoiceLine1_1_2.HarmonisedCode);
			AssertEquals("Screen", commercialInvoiceLine1_1_2.Description);
			AssertEquals(4.32M, commercialInvoiceLine1_1_2.NetWeight);
			AssertEquals(2.34M, commercialInvoiceLine1_1_2.Weight);
			AssertEquals("T", commercialInvoiceLine1_1_2.WeightUnit.Code);
			AssertEquals("NZ", commercialInvoiceLine1_1_2.CountryOfOrigin.Code);
			AssertEquals(3M, commercialInvoiceLine1_1_2.CustomsQuantity);

			var commercialInvoiceLine1_2_1 = commercialInvoiceHeader1.CommercialInvoiceLineCollection.Single(x => x.Link == packedItem1_2_1.CommercialInvoiceLineLink);
			AssertEquals("321321", commercialInvoiceLine1_2_1.HarmonisedCode);
			AssertEquals("Mouse", commercialInvoiceLine1_2_1.Description);
			AssertEquals(3.4M, commercialInvoiceLine1_2_1.NetWeight);
			AssertEquals(1.2M, commercialInvoiceLine1_2_1.Weight);
			AssertEquals("KG", commercialInvoiceLine1_2_1.WeightUnit.Code);
			AssertEquals("FR", commercialInvoiceLine1_2_1.CountryOfOrigin.Code);
			AssertEquals(4M, commercialInvoiceLine1_2_1.CustomsQuantity);
			#endregion
			#endregion

			#region assert consignment2
			#region main properties
			AssertEquals(1, dataObject2.DataContext.DataSourceCollection.Count());
			AssertEquals("HJK678", dataObject2.OwnerRef);
			AssertEquals(1, dataObject2.TotalNoOfPieces);
			AssertEquals((ZDecimal)5.79, dataObject2.GoodsValue);
			AssertEquals("USD", dataObject2.GoodsValueCurrency.Code);
			AssertEquals((ZDecimal)8010, dataObject2.ManifestedWeight);
			AssertEquals((ZDecimal)10, dataObject2.TotalWeight);
			AssertEquals(Weight.Kilograms, dataObject2.TotalWeightUnit.Code);
			AssertEquals((ZDecimal)0.5, dataObject2.ManifestedVolume);
			AssertEquals((ZDecimal)1, dataObject2.TotalVolume);
			AssertEquals(Volume.CubicMetres, dataObject2.TotalVolumeUnit.Code);
			AssertEquals("Toys", dataObject2.GoodsDescription);
			AssertEquals(false, dataObject2.IsHazardous);
			AssertEquals(false, dataObject2.IsSignatureRequired);
			AssertEquals(false, dataObject2.IsAuthorizedToLeave);
			AssertEquals("VID0002", dataObject2.VendorIdentifier);
			AssertEquals("PKG", dataObject2.TotalNoOfPacksPackageType.Code);
			AssertEquals(IncoTerms.DeliveredAtPlace, dataObject2.ShipmentIncoTerm.Code);
			AssertEquals("HVL", dataObject2.ShipmentType.Code.Value);
			AssertEquals("CLR", dataObject2.WarehouseReleaseStatus.Code);
			AssertEquals("&&&", dataObject2.ConsolidatedCargoStatus.Code);
			#endregion

			#region add info
			AssertEquals(1, dataObject2.InstructionCollection.Count);
			AssertEquals("Leave at back", dataObject2.InstructionCollection.First().ServiceInstruction);
			AssertEquals("N", dataObject2.AddInfoCollection.Single(a => a.Key.Value == "IsGSTPrePaid").Value.Value);
			#endregion

			#region org info
			var arrivalCFSAddress2 = dataObject2.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.ArrivalCFSAddress));
			AssertEquals("13 Destination St", arrivalCFSAddress2.Address1);

			var consigneeAddress2 = dataObject2.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.ConsigneeDocumentaryAddress));
			AssertEquals("Bobs Company", consigneeAddress2.CompanyName);
			AssertEquals("12 Something St", consigneeAddress2.Address1);
			AssertEquals("Sydney", consigneeAddress2.City);
			AssertEquals("NSW", consigneeAddress2.State);
			AssertEquals("2000", consigneeAddress2.Postcode);
			AssertEquals("US", consigneeAddress2.Country.Code);
			AssertEquals("email are from HVC_ConsigneeEmail", "", consigneeAddress2.Email);
			Assert(!consigneeAddress2.AddressOverride.Value);

			var consignorAddress2 = dataObject2.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.ConsignorDocumentaryAddress));
			AssertEquals("Another Company", consignorAddress2.CompanyName);
			AssertEquals("23 Another St", consignorAddress2.Address1);
			AssertEquals("Melbourne", consignorAddress2.City);
			AssertEquals("VIC", consignorAddress2.State);
			AssertEquals("3000", consignorAddress2.Postcode);
			AssertEquals("AU", consignorAddress2.Country.Code);
			AssertEquals("email are from HVC_ShipperEmail", "", consignorAddress2.Email);
			Assert(!consignorAddress2.AddressOverride.Value);
			#endregion

			#region packing lines and items
			AssertEquals(1, dataObject2.PackingLineCollection.Count);

			var packingLine2_1 = dataObject2.PackingLineCollection.Single(x => x.OrderReference.Value == "X0012931292");
			AssertEquals("PKG", packingLine2_1.PackType.Code);
			AssertEquals((ZDecimal)8010, packingLine2_1.ManifestedWeight);
			AssertEquals((ZDecimal)10, packingLine2_1.Weight);
			AssertEquals((ZDecimal)0.5, packingLine2_1.ManifestedVolume);
			AssertEquals((ZDecimal)1, packingLine2_1.Volume);
			AssertEquals(false, packingLine2_1.RequiresFumigationCertificate);
			AssertEquals(false, packingLine2_1.IsPersonalEffects);
			AssertEquals(false, packingLine2_1.IsTimber);
			AssertEquals(false, packingLine2_1.IsPerishable);
			AssertEquals(1, packingLine2_1.OutturnQty);
			AssertEquals(0, packingLine2_1.OutturnDamagedQty);
			AssertEquals(1, packingLine2_1.OutturnPillagedQty);
			AssertEquals("CTNR654321", packingLine2_1.ContainerNumber);

			AssertEquals(2, packingLine2_1.PackedItemCollection.Count);
			var packedItem2_1_1 = packingLine2_1.PackedItemCollection.Single(x => x.Description.Value == "Earphone");
			AssertEquals(1.23M, packedItem2_1_1.CIFValue);
			AssertEquals(2.34M, packedItem2_1_1.GrossWeight);
			AssertEquals("T", packedItem2_1_1.GrossWeightUnit.Code);
			AssertEquals(4.32M, packedItem2_1_1.NetWeight);
			AssertEquals("T", packedItem2_1_1.NetWeightUnit.Code);
			AssertEquals(5M, packedItem2_1_1.PackedQuantity);

			var packedItem2_1_2 = packingLine2_1.PackedItemCollection.Single(x => x.Description.Value == "Iphone");
			AssertEquals(4.56M, packedItem2_1_2.CIFValue);
			AssertEquals(5.67M, packedItem2_1_2.GrossWeight);
			AssertEquals("T", packedItem2_1_2.GrossWeightUnit.Code);
			AssertEquals(7.89M, packedItem2_1_2.NetWeight);
			AssertEquals("T", packedItem2_1_2.NetWeightUnit.Code);
			AssertEquals(7M, packedItem2_1_2.PackedQuantity);

			#endregion
			#region CommercialInvoiceHeader
			var commercialInvoiceHeader2 = dataObject2.CommercialInfo.CommercialInvoiceCollection.Single();
			AssertEquals((ZDecimal)5.79, commercialInvoiceHeader2.InvoiceAmount);
			AssertEquals("USD", commercialInvoiceHeader2.InvoiceCurrency.Code);
			AssertEquals((ZDecimal)8.01, commercialInvoiceHeader2.Weight);
			AssertEquals((ZDecimal)12.21, commercialInvoiceHeader2.NetWeight);
			AssertEquals(Weight.Tonnes, commercialInvoiceHeader2.NetWeightUQ.Code);
			AssertEquals("N", commercialInvoiceHeader2.AddInfoCollection.Single(x => x.Key.ToString() == Constants.AddInfoKeys.IsGSTPrePaid).Value);
			AssertEquals("VID0002", commercialInvoiceHeader2.AddInfoCollection.Single(x => x.Key.ToString() == Constants.AddInfoKeys.SupplierGSTNumber).Value);
			#endregion

			#region CommercialInvoiceLine
			AssertEquals(2, commercialInvoiceHeader2.CommercialInvoiceLineCollection.Count);

			var commercialInvoiceLine2_1_1 = commercialInvoiceHeader2.CommercialInvoiceLineCollection.Single(x => x.Link == packedItem2_1_1.CommercialInvoiceLineLink);
			AssertEquals("123123", commercialInvoiceLine2_1_1.HarmonisedCode);
			AssertEquals("Earphone", commercialInvoiceLine2_1_1.Description);
			AssertEquals(4.32M, commercialInvoiceLine2_1_1.NetWeight);
			AssertEquals(2.34M, commercialInvoiceLine2_1_1.Weight);
			AssertEquals("T", commercialInvoiceLine2_1_1.WeightUnit.Code);
			AssertEquals("DE", commercialInvoiceLine2_1_1.CountryOfOrigin.Code);
			AssertEquals(5M, commercialInvoiceLine2_1_1.InvoiceQuantity);
			AssertEquals(5M, commercialInvoiceLine2_1_1.CustomsQuantity);
			AssertEquals(1.23M, commercialInvoiceLine2_1_1.CustomsValue);
			AssertEquals(1.23M, commercialInvoiceLine2_1_1.LinePrice);

			var commercialInvoiceLine2_1_2 = commercialInvoiceHeader2.CommercialInvoiceLineCollection.Single(x => x.Link == packedItem2_1_2.CommercialInvoiceLineLink);
			AssertEquals("12341234", commercialInvoiceLine2_1_2.HarmonisedCode);
			AssertEquals("Iphone", commercialInvoiceLine2_1_2.Description);
			AssertEquals(7.89M, commercialInvoiceLine2_1_2.NetWeight);
			AssertEquals(5.67M, commercialInvoiceLine2_1_2.Weight);
			AssertEquals("T", commercialInvoiceLine2_1_2.WeightUnit.Code);
			AssertEquals("CN", commercialInvoiceLine2_1_2.CountryOfOrigin.Code);
			AssertEquals(7M, commercialInvoiceLine2_1_2.InvoiceQuantity);
			AssertEquals(7M, commercialInvoiceLine2_1_2.CustomsQuantity);
			AssertEquals(4.56M, commercialInvoiceLine2_1_2.CustomsValue);
			AssertEquals(4.56M, commercialInvoiceLine2_1_2.LinePrice);
			#endregion
			#endregion
		}

		public void TestAdditionalReferences()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.Items.AddNew();
			var customsDeclaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			customsDeclaration1.JE_DeclarationReference = "InnocuousWhitePowder";
			consignment.HVC_JE_ImportDeclaration = customsDeclaration1.PK;

			var writer = GetDataObjectWriterForTest(consignment);
			var dataObject = writer.GetDataObject(consignment);
			AssertNull("No Additional References should be found", dataObject.AdditionalReferenceCollection);

			var referenceNumber = consignment.CustomsReferenceNumbers.AddNew();
			referenceNumber.CE_EntryNum = "12345";
			referenceNumber.CE_EntryType = "ISF";

			dataObject = writer.GetDataObject(consignment);
			AssertEquals("Additional References Added", ShouldAdditionalReferencesBeAdded, dataObject.AdditionalReferenceCollection != null);
		}

		protected virtual bool ShouldAdditionalReferencesBeAdded => false;

		public void TestDataObjectSubShipmentHierarchy()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "TestConsol";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_UniqueConsignRef = "TestHVLShipment";

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.Items.AddNew();

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.Items.AddNew();

			Factory.Save();

			var writerManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment));
			writerManager.SetIsPublishingInternally();
			var writer = new ShipmentDataObjectWriter(writerManager, true, true);
			var dataObject = writer.GetDataObject(shipment);

			AssertDataObjectSubShipmentHierarchy(dataObject);
		}

		public void TestDataObjectSubShipmentHierarchy_WhenHasHVMParentShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "TestConsol";

			var hvmShipment = consol.Shipments.AddNew();
			hvmShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValueMaster;
			hvmShipment.JS_UniqueConsignRef = "TestHVMShipment";

			var hvlShipment = consol.Shipments.AddNew();
			hvlShipment.JS_JS_ColoadMasterShipment = hvmShipment.PK;
			hvlShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			hvlShipment.JS_UniqueConsignRef = "TestHVLShipment";

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = hvlShipment.PK;
			consignment1.Items.AddNew();

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_JS_ManifestedOnShipment = hvlShipment.PK;
			consignment2.Items.AddNew();

			Factory.Save();

			var writerManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, hvlShipment));
			writerManager.SetIsPublishingInternally();
			var writer = new ShipmentDataObjectWriter(writerManager, true, true);
			var dataObject = writer.GetDataObject(hvlShipment);

			AssertDataObjectSubShipmentHierarchy(dataObject);
		}

		void AssertDataObjectSubShipmentHierarchy(Shipment dataObject)
		{
			var consolDataSource = dataObject.GetMatchingDataSource(DataContextType.ForwardingConsol);
			AssertNotNull("Should contain consol data source in top level consol data object", consolDataSource);
			AssertEquals("Data source key", "TestConsol", consolDataSource.Key);

			var shipmentDataSource = dataObject.GetMatchingDataSource(DataContextType.ForwardingShipment);
			AssertNotNull("Should contain shipment data source in top level consol data object", shipmentDataSource);
			AssertEquals("Data source key", "TestHVLShipment", shipmentDataSource.Key);

			AssertEquals("Top level consol data object should contain 1 sub-shipment", 1, dataObject.SubShipmentCollection.Count);

			var shipmentData = dataObject.SubShipmentCollection.Single();

			var subShipmentDataSource = shipmentData.GetMatchingDataSource(DataContextType.ForwardingShipment);
			AssertNotNull("Should contain shipment data source in second level shipment data object", subShipmentDataSource);
			AssertEquals("Data source key", "TestHVLShipment", subShipmentDataSource.Key);
			AssertEquals("Second level shipment data object should contain 2 sub-shipments", 2, shipmentData.SubShipmentCollection.Count);
		}

		HVLVBookingHeader PopulateAndGetBookingHeader()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			#region orgs
			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			billToParty.MainAddress.Address1 = "45 Bill To Street";
			billToParty.Contacts.AddNew().OC_ContactName = "Bill";

			var destinationDepotOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			destinationDepotOrg1.MainAddress.Address1 = "12 Road Nanjing";
			var lastMileDelivery = Factory.NewWithValidTestData<OrgHeader>();
			lastMileDelivery.MainAddress.Address1 = "99 Road Shanghai";
			var lastMileCarrierBookingAgent = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrierBookingAgent.MainAddress.Address1 = "66 Road Beijing";

			var destinationDepotOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			destinationDepotOrg2.MainAddress.Address1 = "13 Destination St";

			var cusclass = Factory.New<BaseCusClassification>();
			cusclass.CC_LookupCode = "1234";
			cusclass.CC_ClassificationType = Customs.Common.ClassificationType.EXP;
			cusclass.CC_TariffNum = "4321";

			Factory.Save();
			#endregion

			#region booking header
			var bookingHeader = Factory.New<HVLVBookingHeader>();

			bookingHeader.HVH_OA_BillToParty = billToParty.MainAddress.PK;
			bookingHeader.HVH_OC_BillToPartyContact = billToParty.Contacts[0].PK;

			#region consignments
			#region consignment1
			var consignment1 = bookingHeader.Consignments.AddNew();
			#region shipment in consignment1 setup(set consignment.DirectionOfTrade = Directions.Import)
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_UniqueConsignRef = "SHIP000001";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			#endregion
			consignment1.HVC_ConsignmentId = "CONSIFIRST";
			consignment1.HVC_WaybillNumber = "XA13DQ";
			consignment1.HVC_ShipperReference = "123ABC";
			consignment1.HVC_ItemCount = 0;
			consignment1.HVC_GoodsValue = 200;
			consignment1.HVC_RX_NKGoodsValueCurrency = "BGN";
			consignment1.HVC_WeightUQ = Weight.Kilograms;
			consignment1.HVC_VolumeUQ = Volume.CubicMetres;
			consignment1.HVC_GoodsDescription = "GUNS";
			consignment1.HVC_ConsigneeInstructions = "Leave at front";
			consignment1.HVC_INCO = IncoTerms.DeliveredAtFrontier;
			consignment1.HVC_IsHazardous = true;
			consignment1.HVC_IsSignatureRequired = true;
			consignment1.HVC_AuthorityToLeave = true;
			consignment1.HVC_RequiresFumigation = true;
			consignment1.HVC_IsPersonalEffects = true;
			consignment1.HVC_IsTimber = true;
			consignment1.HVC_IsPerishable = true;
			consignment1.HVC_UndgClass = "3.3D";
			consignment1.HVC_PL_NKLastMileCarrierServiceLevel = "EXP";
			consignment1.HVC_IsTaxPrePaid = true;

			consignment1.HVC_OA_DestinationDepot = destinationDepotOrg1.MainAddress.PK;

			consignment1.HVC_OH_LastMileCarrier = lastMileDelivery.PK;

			consignment1.HVC_OH_LastMileCarrierBookingAgent = lastMileCarrierBookingAgent.PK;

			consignment1.HVC_ConsigneeName = "Murray";
			consignment1.HVC_ConsigneeAddress1 = "99 Consignee Road";
			consignment1.HVC_ConsigneeAddress2 = "Downtown";
			consignment1.HVC_ConsigneeCity = "New York";
			consignment1.HVC_ConsigneeState = "NY";
			consignment1.HVC_ConsigneePostcode = "12345";
			consignment1.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment1.HVC_ConsigneeContact = "Moo ray";
			consignment1.HVC_ConsigneeEmail = "murray.hewitt@usconsulate.gov.nz";
			consignment1.HVC_ConsigneePhone = "7";
			consignment1.HVC_ConsigneeMobile = "+1234567890";
			consignment1.HVC_ConsigneeFax = "+0987654321";

			consignment1.HVC_ShipperName = "Some Company";
			consignment1.HVC_ShipperAddress1 = "22 Shipper Street";
			consignment1.HVC_ShipperCity = "Wellington";
			consignment1.HVC_ShipperState = "WLG";
			consignment1.HVC_ShipperPostcode = "54321";
			consignment1.HVC_RN_NKShipperCountryCode = "NZ";
			consignment1.HVC_ShipperContact = "Randy";
			consignment1.HVC_ShipperEmail = "randy@randysdomain.com";
			consignment1.HVC_ShipperPhone = "01189998819991197253";
			consignment1.HVC_ShipperMobile = "+555 5555";
			consignment1.HVC_ShipperFax = "8";

			consignment1.HVC_VendorIdentifier = "VID0001";
			consignment1.HVC_ImportCustomsClearanceStatus = "&&&";
			consignment1.HVC_ExportReleaseStatus = "CLR";
			consignment1.HVC_ImportReleaseStatus = "NON";
			#endregion

			#region consignment2
			var consignment2 = bookingHeader.Consignments.AddNew();
			#region shipment in consignment2 setup(set consignment.DirectionOfTrade = Directions.Export)
			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "NZAKL";

			var departureConsol2 = shipment2.Consols.AddNew();
			departureConsol2.JK_RL_NKLoadPort = "AUSYD";
			departureConsol2.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol2 = shipment2.Consols.AddNew();
			arrivalConsol2.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol2.JK_RL_NKDischargePort = "NZAKL";

			consignment2.HVC_JS_ManifestedOnShipment = shipment2.PK;
			#endregion
			consignment2.HVC_ConsignmentId = "SECONDCONSIGN";
			consignment2.HVC_ShipperReference = "HJK678";
			consignment2.HVC_ItemCount = 0;
			consignment2.HVC_GoodsValue = 0;
			consignment2.HVC_RX_NKGoodsValueCurrency = "USD";
			consignment2.HVC_WeightUQ = Weight.Kilograms;
			consignment2.HVC_VolumeUQ = Volume.CubicMetres;
			consignment2.HVC_GoodsDescription = "Toys";
			consignment2.HVC_ConsigneeInstructions = "Leave at back";
			consignment2.HVC_INCO = IncoTerms.DeliveredAtPlace;
			consignment2.HVC_IsHazardous = false;
			consignment2.HVC_IsSignatureRequired = false;
			consignment2.HVC_AuthorityToLeave = false;
			consignment2.HVC_RequiresFumigation = false;
			consignment2.HVC_IsPersonalEffects = false;
			consignment2.HVC_IsTimber = false;
			consignment2.HVC_IsPerishable = false;
			consignment2.HVC_IsTaxPrePaid = false;

			consignment2.HVC_OA_DestinationDepot = destinationDepotOrg2.MainAddress.PK;

			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			consignment2.HVC_OA_ConsigneeAddress = orgAddress1.PK;
			consignment2.HVC_OA_ShipperAddress = orgAddress2.PK;
			orgAddress1.CompanyName = "Bobs Company";
			orgAddress1.Address1 = "12 Something St";
			orgAddress1.City = "Sydney";
			orgAddress1.State = "NSW";
			orgAddress1.Postcode = "2000";
			orgAddress1.OA_RN_NKCountryCode = "US";
			orgAddress1.OA_Email = "bob@bob.com";

			orgAddress2.CompanyName = "Another Company";
			orgAddress2.Address1 = "23 Another St";
			orgAddress2.City = "Melbourne";
			orgAddress2.State = "VIC";
			orgAddress2.Postcode = "3000";
			orgAddress2.OA_RN_NKCountryCode = "AU";
			orgAddress2.OA_Email = "victor@fakedomain.com";

			var customsDeclaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			consignment2.HVC_JE_ExportDeclaration = customsDeclaration2.PK;

			consignment2.HVC_VendorIdentifier = "VID0002";

			consignment2.HVC_ExportCustomsClearanceStatus = "&&&";
			consignment2.HVC_ImportReleaseStatus = "CLR";
			consignment2.HVC_ExportReleaseStatus = "HLD";

			var consignment3 = bookingHeader.Consignments.AddNew();
			consignment3.HVC_ConsignmentId = "NODATACONS";

			#endregion
			#endregion

			#region Items

			var item1_1 = consignment1.Items.AddNew();
			item1_1.HVI_ShipperReference = "D9901239028";
			item1_1.HVI_CurrentBarcode = "12345678901234567890123456789012345678901234567890";
			item1_1.HVI_F3_NKPackType = "BOX";
			item1_1.HVI_Height = 3m;
			item1_1.HVI_Length = 4m;
			item1_1.HVI_Width = 5m;
			item1_1.HVI_UnitOfDimension = Length.Metres;
			item1_1.HVI_ManifestedWeight = 5;
			item1_1.HVI_ActualWeight = 10;
			item1_1.HVI_ManifestedVolume = 0.5;
			item1_1.HVI_ActualVolume = 0.5;
			item1_1.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;
			item1_1.HVI_IsDamaged = true;
			item1_1.HVI_IsPillaged = true;

			var item1_2 = consignment1.Items.AddNew();
			item1_2.HVI_ShipperReference = "D5465421481";
			item1_2.HVI_F3_NKPackType = "BOX";
			item1_2.HVI_ManifestedWeight = 10;
			item1_2.HVI_ActualWeight = 10;
			item1_2.HVI_ManifestedVolume = 0.5;
			item1_2.HVI_ActualVolume = 0.5;
			item1_2.HVI_Status = HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot;
			item1_2.HVI_IsDamaged = true;
			item1_2.HVI_IsPillaged = false;

			var item2_1 = consignment2.Items.AddNew();
			item2_1.HVI_ShipperReference = "X0012931292";
			item2_1.HVI_F3_NKPackType = "PKG";
			item2_1.HVI_ManifestedWeight = 10;
			item2_1.HVI_ActualWeight = 10;
			item2_1.HVI_ManifestedVolume = 0.5;
			item2_1.HVI_ActualVolume = 1;
			item2_1.HVI_Status = HVLVItemStatus.Codes.ReadyForLastMileDelivery;
			item2_1.HVI_IsDamaged = false;
			item2_1.HVI_IsPillaged = true;
			item2_1.HVI_ContainerNumber = "CTNR654321";

			#endregion

			#region Item Lines

			var line1_1_1 = item1_1.Lines.AddNew();
			line1_1_1.HVS_OriginTariff = "123456";
			line1_1_1.HVS_RN_NKOriginCountryCode = "AU";
			line1_1_1.HVS_CustomsValue = 12.34;
			line1_1_1.HVS_IntrinsicValue = 54.321;
			line1_1_1.HVS_DestinationTariff = "654321";
			line1_1_1.HVS_GoodsDescription = "Keyboard";
			line1_1_1.HVS_GrossWeight = 1.23;
			line1_1_1.HVS_ItemURL = "www.google.com/keyboard";
			line1_1_1.HVS_NetWeight = 3.21;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "KBD001";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = bookingHeader.BillToParty.OA_OH;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			line1_1_1.HVS_ProductCode = "KBD001";
			line1_1_1.HVS_CC_Lookup = cusclass.PK;
			line1_1_1.HVS_Quantity = 2;
			line1_1_1.HVS_WeightUnit = "KG";

			var line1_1_2 = item1_1.Lines.AddNew();
			line1_1_2.HVS_OriginTariff = "12345678";
			line1_1_2.HVS_RN_NKOriginCountryCode = "NZ";
			line1_1_2.HVS_DestinationTariff = "87654321";
			line1_1_2.HVS_GrossWeight = 2.34;
			line1_1_2.HVS_NetWeight = 4.32;
			line1_1_2.HVS_Quantity = 3;
			line1_1_2.HVS_WeightUnit = "T";
			line1_1_2.HVS_GoodsDescription = "Screen";

			var line1_2_1 = item1_2.Lines.AddNew();
			line1_2_1.HVS_OriginTariff = "123123";
			line1_2_1.HVS_RN_NKOriginCountryCode = "FR";
			line1_2_1.HVS_DestinationTariff = "321321";
			line1_2_1.HVS_GrossWeight = 1.2;
			line1_2_1.HVS_NetWeight = 3.4;
			line1_2_1.HVS_Quantity = 4;
			line1_2_1.HVS_WeightUnit = "KG";
			line1_2_1.HVS_GoodsDescription = "Mouse";

			var line2_1_1 = item2_1.Lines.AddNew();
			line2_1_1.HVS_OriginTariff = "123123";
			line2_1_1.HVS_RN_NKOriginCountryCode = "DE";
			line2_1_1.HVS_DestinationTariff = "321321";
			line2_1_1.HVS_GrossWeight = 2.34;
			line2_1_1.HVS_NetWeight = 4.32;
			line2_1_1.HVS_Quantity = 5;
			line2_1_1.HVS_WeightUnit = "T";
			line2_1_1.HVS_GoodsDescription = "Earphone";
			line2_1_1.HVS_CustomsValue = 1.23;

			var line2_1_2 = item2_1.Lines.AddNew();
			line2_1_2.HVS_OriginTariff = "12341234";
			line2_1_2.HVS_RN_NKOriginCountryCode = "CN";
			line2_1_2.HVS_DestinationTariff = "43214321";
			line2_1_2.HVS_GrossWeight = 5.67;
			line2_1_2.HVS_NetWeight = 7.89;
			line2_1_2.HVS_Quantity = 7;
			line2_1_2.HVS_WeightUnit = "T";
			line2_1_2.HVS_GoodsDescription = "Iphone";
			line2_1_2.HVS_CustomsValue = 4.56;

			#endregion
			#endregion

			return bookingHeader;
		}

		protected override IHVLVConsignmentDataExportStrategy DataExportStrategy => HVLVConsignmentToCargoReportDataExportStrategy.Instance;
	}

	public class HVLVConsignmentToCargoReportWithAdditionalReferencesDataExportStrategyTest : HVLVConsignmentToCargoReportDataExportStrategyTest
	{
		public void TestNotesNotExported()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.Items.AddNew();

			var writer = GetDataObjectWriterForTest(consignment);

			var dataObject = writer.GetDataObject(consignment);
			AssertNull("No notes", dataObject.NoteCollection);

			consignment.Notes.AddNew(false, "Description", "qwerty");
			dataObject = writer.GetDataObject(consignment);
			AssertNull("Still no notes", dataObject.NoteCollection);
		}

		protected override bool ShouldAdditionalReferencesBeAdded => true;
		protected override IHVLVConsignmentDataExportStrategy DataExportStrategy => HVLVConsignmentToCargoReportWithAdditionalReferencesDataExportStrategy.Instance;
	}

	public class HVLVConsignmentToRTUSDataExportStrategyTest : HVLVConsignmentDataObjectWriterStrategyTest
	{
		public void TestPackingLineCollectionNotExported()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.Items.AddNew();

			var writer = GetDataObjectWriterForTest(consignment);
			var dataObject = writer.GetDataObject(consignment);
			AssertNull("No PackingLineCollection written", dataObject.PackingLineCollection);
		}

		protected override IHVLVConsignmentDataExportStrategy DataExportStrategy => HVLVConsignmentToRTUSDataExportStrategy.Instance;
	}

	public class HVLVConsignmentToDeclarationDataExportStrategyTest : HVLVConsignmentDataObjectWriterStrategyTest
	{
		public void TestContainersAreWrittenToShipmentLevelDataObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			consol.Shipments.Add(shipment);

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE11223344";

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();

			var writer = GetDataObjectWriter(consignment);
			var consolDataObject = writer.GetDataObject(consignment) as Shipment;

			AssertNull("ContainerCollection is moved to shipmentDataObject", consolDataObject.ContainerCollection);

			var shipmentDataObject = consolDataObject.SubShipmentCollection.Single();
			AssertNotNull("consolDataObject.ContainerCollection is moved here", shipmentDataObject.ContainerCollection);

			var containerDataObject = shipmentDataObject.ContainerCollection.Single();
			AssertEquals("FAKE11223344", containerDataObject.ContainerNumber);
		}

		public void TestGetFromObjectFactory()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var writer = GetDataObjectWriter(consignment);
			AssertType<HVLVConsignmentDataObjectWriter>(writer);

			var consignmentWriter = writer as HVLVConsignmentDataObjectWriter;
			AssertType<HVLVConsignmentToDeclarationDataExportStrategy>(consignmentWriter.DataExportStrategy);
		}

		public void TestContainOneSingleConsignmentAsSubShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			consol.Shipments.Add(shipment);

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_ShipmentType = "STD";
			consol.Shipments.Add(shipment2);

			AssertEquals(2, consol.ShipmentCount);

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();

			var writer = GetDataObjectWriter(consignment);
			var dataObject = writer.GetDataObject(consignment) as Shipment;
			AssertEquals("Should contain one single sub shipment", 1, dataObject.SubShipmentCollection.Count);
			AssertEquals("Should contain consignment data", consignment.HVC_GoodsDescription, dataObject.SubShipmentCollection[0].GoodsDescription);
		}

		public void TestGoodsDescription_FallsBackToItems()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			consol.Shipments.Add(shipment);

			var consignmentA = Factory.New<HVLVConsignment>();
			consignmentA.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignmentA.HVC_GoodsDescription = ZString.Empty;

			var consignmentB = Factory.New<HVLVConsignment>();
			consignmentB.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignmentB.HVC_GoodsDescription = ZString.Empty;

			var itemA1 = consignmentA.Items.AddNew();
			var itemB1 = consignmentB.Items.AddNew();
			var itemB2 = consignmentB.Items.AddNew();

			itemA1.HVI_GoodsDescription = "A1";
			itemB1.HVI_GoodsDescription = ZString.Empty;
			itemB2.HVI_GoodsDescription = "B2";

			var expectedDescriptionA = consignmentA.Items.OfType<HVLVItem>().Select(item => item.HVI_GoodsDescription).FirstOrDefault(description => !description.IsDefault);
			var expectedDescriptionB = consignmentB.Items.OfType<HVLVItem>().Select(item => item.HVI_GoodsDescription).FirstOrDefault(description => !description.IsDefault);

			CombineAssertions("Precondition - Expected Goods Description should be correctly determined from Items", () =>
			{
				AssertEquals("Consignment A", "A1", expectedDescriptionA);
				AssertEquals("Consignment B", "B2", expectedDescriptionB);
			});

			var writerA = GetDataObjectWriter(consignmentA);
			var writerB = GetDataObjectWriter(consignmentB);
			var dataObjectA = writerA.GetDataObject(consignmentA) as Shipment;
			var dataObjectB = writerB.GetDataObject(consignmentB) as Shipment;

			var dataObjectConsignmentA = dataObjectA.SubShipmentCollection[0];
			var dataObjectConsignmentB = dataObjectB.SubShipmentCollection[0];

			CombineAssertions("Goods Description should have fallen-back to Item", () =>
			{
				AssertEquals("A1", dataObjectConsignmentA.GoodsDescription);
				AssertEquals("B2", dataObjectConsignmentB.GoodsDescription);
			});
		}

		public void TestGoodsDescription_FallsBackToItemLines()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			consol.Shipments.Add(shipment);

			var consignmentA = Factory.New<HVLVConsignment>();
			consignmentA.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignmentA.HVC_GoodsDescription = ZString.Empty;

			var consignmentB = Factory.New<HVLVConsignment>();
			consignmentB.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignmentB.HVC_GoodsDescription = ZString.Empty;

			var itemA = consignmentA.Items.AddNew();
			var itemB = consignmentB.Items.AddNew();
			itemA.HVI_GoodsDescription = ZString.Empty;
			itemB.HVI_GoodsDescription = ZString.Empty;

			var lineA1 = itemA.Lines.AddNew();
			var lineB1 = itemB.Lines.AddNew();
			var lineB2 = itemB.Lines.AddNew();

			lineA1.HVS_GoodsDescription = "A1";
			lineB1.HVS_GoodsDescription = ZString.Empty;
			lineB2.HVS_GoodsDescription = "B2";

			var expectedDescriptionA = itemA.Lines.OfType<HVLVItemLine>().Select(line => line.HVS_GoodsDescription).FirstOrDefault(description => !description.IsDefault);
			var expectedDescriptionB = itemB.Lines.OfType<HVLVItemLine>().Select(line => line.HVS_GoodsDescription).FirstOrDefault(description => !description.IsDefault);

			CombineAssertions("Precondition - Expected Goods Description should be correctly determined from Item Lines", () =>
			{
				AssertEquals("Consignment A", "A1", expectedDescriptionA);
				AssertEquals("Consignment B", "B2", expectedDescriptionB);
			});

			var writerA = GetDataObjectWriter(consignmentA);
			var writerB = GetDataObjectWriter(consignmentB);
			var dataObjectA = writerA.GetDataObject(consignmentA) as Shipment;
			var dataObjectB = writerB.GetDataObject(consignmentB) as Shipment;

			var dataObjectConsignmentA = dataObjectA.SubShipmentCollection[0];
			var dataObjectConsignmentB = dataObjectB.SubShipmentCollection[0];

			CombineAssertions("Goods Description should have fallen-back to Item Lines", () =>
			{
				AssertEquals("A1", dataObjectConsignmentA.GoodsDescription);
				AssertEquals("B2", dataObjectConsignmentB.GoodsDescription);
			});
		}

		public void TestCommercialInvoiceHeaderHasSupplier()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			consol.Shipments.Add(shipment);

			var bookingHeader = Factory.New<HVLVBookingHeader>();

			var consignment = bookingHeader.Consignments.AddNew();
			consignment.Items.AddNew().Lines.AddNew();

			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_WaybillNumber = "Test Waybill 1";
			consignment.HVC_ShipperAddress1 = "Test Address 1";
			consignment.HVC_ShipperAddress2 = "Test Address 2";
			consignment.HVC_ShipperCity = "Sydney";
			consignment.HVC_ShipperContact = "Test Contact";
			consignment.HVC_ShipperEmail = "Test Email";
			consignment.HVC_ShipperFax = "Test Fax";
			consignment.HVC_ShipperMobile = "Test Mobile";
			consignment.HVC_ShipperName = "Test Shipper";
			consignment.HVC_ShipperPhone = "Test Phone";
			consignment.HVC_ShipperPostcode = "2000";
			consignment.HVC_ShipperState = "NSW";
			consignment.HVC_RN_NKShipperCountryCode = "AU";

			var writer = GetDataObjectWriter(consignment);
			var dataObject = writer.GetDataObject(consignment) as Shipment;
			var consignmentDataObject = dataObject.SubShipmentCollection[0];
			var invoiceHeader = consignmentDataObject.CommercialInfo.CommercialInvoiceCollection[0];

			CombineAssertions("Consignment shipper should be populated to invoice header's supplier", () =>
			{
				AssertEquals("Invoice Number", "Test Waybill 1", invoiceHeader.InvoiceNumber);

				var supplier = invoiceHeader.Supplier;
				AssertNotNull(supplier);

				AssertEquals("Address1", "Test Address 1", supplier.Address1);
				AssertEquals("Address2", "Test Address 2", supplier.Address2);
				AssertEquals("City", "Sydney", supplier.City);
				AssertEquals("Contact", "Test Contact", supplier.Contact);
				AssertEquals("Email", "Test Email", supplier.Email);
				AssertEquals("Fax", "Test Fax", supplier.Fax);
				AssertEquals("Mobile", "Test Mobile", supplier.Mobile);
				AssertEquals("Company Name", "Test Shipper", supplier.CompanyName);
				AssertEquals("Phone", "Test Phone", supplier.Phone);
				AssertEquals("Postcode", "2000", supplier.Postcode);
				AssertEquals("State", "NSW", supplier.State);
				AssertEquals("Country Code", "AU", supplier.Country.Code);
			});
		}

		public void TestMergeConsignments_ShouldCombineMeasures()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			consol.Shipments.Add(shipment);

			var bookingHeader = Factory.New<HVLVBookingHeader>();

			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "Test Waybill 1";
			consignment1.HVC_GoodsDescription = "Test Goods 1";
			consignment1.HVC_ShipperReference = "Test Shipper 1";
			consignment1.HVC_WeightUQ = "G";
			consignment1.HVC_VolumeUQ = "M3";

			var item1 = consignment1.Items.AddNew();
			item1.HVI_ActualWeight = 100m;
			item1.HVI_ActualVolume = 1;

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_WaybillNumber = "Test Waybill 2";
			consignment2.HVC_GoodsDescription = "Test Goods 2";
			consignment2.HVC_ShipperReference = "Test Shipper 2";
			consignment2.HVC_WeightUQ = "KG";
			consignment2.HVC_VolumeUQ = "D3";

			var item2 = consignment2.Items.AddNew();
			item2.HVI_ActualWeight = 10m;
			item2.HVI_ActualVolume = 2;

			var consignment3 = bookingHeader.Consignments.AddNew();
			consignment3.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment3.HVC_WaybillNumber = "Test Waybill 3";
			consignment3.HVC_GoodsDescription = "Test Goods 3";
			consignment3.HVC_ShipperReference = "Test Shipper 3";
			consignment3.HVC_WeightUQ = "T";
			consignment3.HVC_VolumeUQ = "L";

			var item3 = consignment3.Items.AddNew();
			item3.HVI_ActualWeight = 1m;
			item3.HVI_ActualVolume = 3;

			var writer = GetDataObjectWriter(consignment1, new[] { consignment2, consignment3 });
			var dataObject = writer.GetDataObject(consignment1) as Shipment;
			var consignmentDataObject = dataObject.SubShipmentCollection[0];

			CombineAssertions("Should combine measures", () =>
			{
				AssertEquals("Waybill Number", "Test Waybill 1", consignmentDataObject.WayBillNumber);
				AssertEquals("Goods Description", "Test Goods 1", consignmentDataObject.GoodsDescription);
				AssertEquals("Owner Ref.", "Test Shipper 1", consignmentDataObject.OwnerRef);

				AssertEquals("Total Weight", 1010.1m, consignmentDataObject.TotalWeight);
				AssertEquals("Total Weight Unit", "KG", consignmentDataObject.TotalWeightUnit.Code);
				AssertEquals("Total Volume", 1.005m, consignmentDataObject.TotalVolume);
				AssertEquals("Total Volume Unit", "M3", consignmentDataObject.TotalVolumeUnit.Code);
				AssertEquals("Total No. Of Pieces", 3, consignmentDataObject.TotalNoOfPieces);
				AssertEquals("Outer Packs", 3, consignmentDataObject.OuterPacks);
			});
		}

		public void TestMergeConsignments_ShouldCombineAdditionalBills()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			consol.Shipments.Add(shipment);

			var bookingHeader = Factory.New<HVLVBookingHeader>();

			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "Test Waybill 1";
			consignment1.Items.AddNew();

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_WaybillNumber = "Test Waybill 2";
			consignment2.Items.AddNew();

			var writer = GetDataObjectWriter(consignment1, new[] { consignment2 });
			var dataObject = writer.GetDataObject(consignment1) as Shipment;
			var consignmentDataObject = dataObject.SubShipmentCollection[0];

			CombineAssertions("Should combine additional bills", () =>
			{
				AssertEquals("Should contain 1 additional bill", 1, consignmentDataObject.AdditionalBillCollection.Count);
				AssertEquals("Additional bill should be populated from consignment2", "Test Waybill 2", consignmentDataObject.AdditionalBillCollection[0].BillNumber);
				AssertEquals("Additional bill should be house bill", "HB", consignmentDataObject.AdditionalBillCollection[0].BillType.Code);
			});
		}

		public void TestMergeConsignments_ShouldCombinePackings()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			consol.Shipments.Add(shipment);

			var bookingHeader = Factory.New<HVLVBookingHeader>();

			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "Test Waybill 1";

			var item1 = consignment1.Items.AddNew();
			item1.HVI_F3_NKPackType = "PKG";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_WaybillNumber = "Test Waybill 2";

			var item2 = consignment2.Items.AddNew();
			item2.HVI_F3_NKPackType = "PKG";
			var item3 = consignment2.Items.AddNew();
			item3.HVI_F3_NKPackType = "PKG";

			var writer = GetDataObjectWriter(consignment1, new[] { consignment2 });
			var dataObject = writer.GetDataObject(consignment1) as Shipment;
			var consignmentDataObject = dataObject.SubShipmentCollection[0];

			CombineAssertions("Should combine packing lines", () =>
			{
				AssertEquals("Should contain 3 packing lines", 3, consignmentDataObject.PackingLineCollection.Count);
				AssertContainsExactElementsInAnyOrder("Should contain correct bills", new ZString[] { "Test Waybill 1", "Test Waybill 2", "Test Waybill 2" }, consignmentDataObject.PackingLineCollection.Select(x => x.BillNumber));
			});
		}

		public void TestMergeConsignments_ShouldCombineInvoiceLines()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			consol.Shipments.Add(shipment);

			var bookingHeader = Factory.New<HVLVBookingHeader>();

			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "Test Waybill 1";
			var line1 = consignment1.Items.AddNew().Lines.AddNew();
			line1.HVS_GoodsDescription = "Test Line 1";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_WaybillNumber = "Test Waybill 2";
			var item = consignment2.Items.AddNew();
			var line2 = item.Lines.AddNew();
			line2.HVS_GoodsDescription = "Test Line 2";
			var line3 = item.Lines.AddNew();
			line3.HVS_GoodsDescription = "Test Line 3";

			var writer = GetDataObjectWriter(consignment1, new[] { consignment2 });
			var dataObject = writer.GetDataObject(consignment1) as Shipment;
			var consignmentDataObject = dataObject.SubShipmentCollection[0];

			var commercialInvoiceCollection = consignmentDataObject.CommercialInfo.CommercialInvoiceCollection;
			CombineAssertions("Should combine commercial invoices", () =>
			{
				AssertEquals("Should contain 2 invoice headers", 2, commercialInvoiceCollection.Count);
				AssertContainsExactElementsInAnyOrder("Should contain correct invoice numbers", new ZString[] { "Test Waybill 1", "Test Waybill 2" }, commercialInvoiceCollection.Select(x => x.InvoiceNumber));

				AssertEquals("Should contain 3 invoice lines", 3, commercialInvoiceCollection.SelectMany(x => x.CommercialInvoiceLineCollection).Count());
				AssertContainsExactElementsInAnyOrder("Should contain correct invoice line descriptions", new ZString[] { "Test Line 1", "Test Line 2", "Test Line 3" }, commercialInvoiceCollection.SelectMany(x => x.CommercialInvoiceLineCollection).Select(x => x.Description));
			});
		}

		public void TestNZDeclarationMapping()
		{
			using (Factory.AddDisposableService())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				GlbDepartment.CurrentDepartment.GE_Sea = true;
				var hvlShipment = PopulateAndGetHVLShipment();

				var consignment1 = hvlShipment.HVLVConsignments.Cast<HVLVConsignment>().Single(x => x.HVC_ConsignmentId == "CONSIFIRST");
				var consol = consignment1.ManifestedOnShipment.ArrivalConsol;
				var writer = GetDataObjectWriter(consignment1);

				PublishUniversalXmlResult events;
				var factory = new BusinessObjectFactory();
				using (factory.AddDisposableService())
				{
					var dataObject = writer.GetDataObject(consignment1);
					dataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
					dataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
					events = UniversalXmlWorkflowProcessor.PublishUniversalXMLInternally(factory, consignment1, dataObject, EDIMessageSubTypeList.Codes.XmlUniversalShipment);
					factory.Save();
				}

				var dimEvent = events.SingleOrDefault(e => e.EventType.Value == AutoEvents.DataImportCode);
				var dataSource = dimEvent?.DataContext?.DataSourceCollection?.SingleOrDefault(s => s.Type.Value == nameof(DataContextType.CustomsDeclaration));
				var thisDeclarationReference = dataSource.Key;
				var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, thisDeclarationReference));

				#region Assert declaration properties
				AssertEquals(consignment1.HVC_WaybillNumber, declaration.JE_HouseBill);
				AssertEquals(consignment1.HVC_GoodsDescription, declaration.JE_GoodsDescription);
				AssertEquals(consignment1.HVC_ActualWeight, declaration.JE_TotalWeight);
				AssertEquals(consignment1.HVC_WeightUQ, declaration.JE_TotalWeightUnit);
				AssertEquals(consignment1.HVC_ActualVolume, declaration.JE_TotalVolume);
				AssertEquals(consignment1.HVC_VolumeUQ, declaration.JE_TotalVolumeUnit);
				AssertEquals(consignment1.HVC_ItemCount, declaration.JE_TotalNoOfPacks);
				AssertEquals("BX", declaration.JE_TotalNoOfPacksPackType);
				AssertEquals(consignment1.HVC_INCO, declaration.JE_ShipmentIncoTerm);
				AssertEquals(consignment1.HVC_OH_LastMileCarrier, declaration.JE_OA_DeliveryOrPickupCartageCoAddr_ZAddress.OrgPK);
				#endregion

				AssertEquals(consol.JK_JX_JV_NKVessel, declaration.JE_VesselName);
				AssertEquals(consol.JK_JX_JV_VoyageFlight, declaration.JE_VoyageFlightNo);
				AssertEquals(consol.JK_RL_NKLoadPort, declaration.JE_RL_NKPortOfLoading);
				AssertEquals(consol.JK_RL_NKDischargePort, declaration.JE_RL_NKPortOfArrival);

				AssertEquals(consol.JK_RL_NKLoadPort, declaration.JE_RL_NKOrigin);
				AssertEquals(consol.JK_RL_NKDischargePort, declaration.JE_RL_NKFinalDestination);

				AssertEquals(new ZDateTime(2020, 5, 12), declaration.JE_ExportDate);
				AssertEquals(new ZDateTime(2020, 5, 13), declaration.JE_DateOfArrival);
				AssertEquals(ZDateTime.Empty, declaration.JE_DateAtOrigin);
				AssertEquals(ZDateTime.Empty, declaration.JE_DateAtFinalDestination);

				#region Assert invoice header properties
				var jobComInvoiceHeader = declaration.AllGroupHeaders[0].AllJobComInvoiceHeaders.Single() as IJobComInvoiceHeader;
				AssertEquals(consignment1.HVC_GoodsValue, jobComInvoiceHeader.JZ_InvoiceAmount);
				AssertEquals(consignment1.HVC_RX_NKGoodsValueCurrency, jobComInvoiceHeader.JZ_RX_NKInvoice_Currency);
				AssertEquals(consignment1.TotalLineGrossWeight, jobComInvoiceHeader.JZ_Weight);
				AssertEquals(consignment1.TotalLineNetWeight, jobComInvoiceHeader.JZ_NetWeight);
				AssertEquals(consignment1.FirstLineWeightUnit, jobComInvoiceHeader.JZ_NetWeightUQ);
				AssertEquals(consignment1.HVC_IsTaxPrePaid.ToString(), jobComInvoiceHeader.JZ_IsGSTPrePaid);
				AssertEquals(consignment1.HVC_VendorIdentifier, jobComInvoiceHeader.JZ_SupplierGSTNumber);
				#endregion

				#region Assert invoice lines properties
				AssertEquals(3, jobComInvoiceHeader.JobComInvoiceLines.Count);

				var itemline1 = consignment1.Items[0].Lines[0];
				var jobComInvoiceLine1 = jobComInvoiceHeader.JobComInvoiceLines[0];
				AssertEquals(itemline1.HVS_ProductCode, jobComInvoiceLine1.JI_PartNo);
				AssertEquals(itemline1.HVS_CC_Lookup, jobComInvoiceLine1.JI_CC);
				AssertEquals(itemline1.HVS_FormattedDestinationTariff, jobComInvoiceLine1.JI_Tariff);
				AssertEquals(itemline1.HVS_GoodsDescription.ToUpper(), jobComInvoiceLine1.JI_Description);
				AssertEquals((ZDecimal)(itemline1.HVS_Quantity), jobComInvoiceLine1.JI_InvoiceQuantity);
				AssertEquals(itemline1.HVS_GrossWeight, jobComInvoiceLine1.JI_Weight);
				AssertEquals(itemline1.HVS_WeightUnit, jobComInvoiceLine1.JI_WeightUQ);
				AssertEquals(itemline1.HVS_NetWeight, jobComInvoiceLine1.JI_NetWeight);
				AssertEquals(itemline1.HVS_WeightUnit, jobComInvoiceLine1.JI_NetWeightUQ);
				AssertEquals(itemline1.HVS_RN_NKOriginCountryCode, jobComInvoiceLine1.JI_CountryOfOrigin);
				AssertEquals(itemline1.HVS_CustomsValue, jobComInvoiceLine1.JI_LinePrice);
				AssertEquals(consignment1.HVC_RX_NKGoodsValueCurrency, jobComInvoiceLine1.JI_RX_NKLinePriceCurr);

				var itemline2 = consignment1.Items[0].Lines[1];
				var jobComInvoiceLine2 = jobComInvoiceHeader.JobComInvoiceLines[1];
				AssertEquals(itemline2.HVS_ProductCode, jobComInvoiceLine2.JI_PartNo);
				AssertEquals(itemline2.HVS_CC_Lookup, jobComInvoiceLine2.JI_CC);
				AssertEquals(itemline2.HVS_FormattedDestinationTariff, jobComInvoiceLine2.JI_Tariff);
				AssertEquals(itemline2.HVS_GoodsDescription.ToUpper(), jobComInvoiceLine2.JI_Description);
				AssertEquals((ZDecimal)(itemline2.HVS_Quantity), jobComInvoiceLine2.JI_InvoiceQuantity);
				AssertEquals(itemline2.HVS_GrossWeight, jobComInvoiceLine2.JI_Weight);
				AssertEquals(itemline2.HVS_WeightUnit, jobComInvoiceLine2.JI_WeightUQ);
				AssertEquals(itemline2.HVS_NetWeight, jobComInvoiceLine2.JI_NetWeight);
				AssertEquals(itemline2.HVS_WeightUnit, jobComInvoiceLine2.JI_NetWeightUQ);
				AssertEquals(itemline2.HVS_RN_NKOriginCountryCode, jobComInvoiceLine2.JI_CountryOfOrigin);
				AssertEquals(itemline2.HVS_CustomsValue, jobComInvoiceLine2.JI_LinePrice);
				AssertEquals(consignment1.HVC_RX_NKGoodsValueCurrency, jobComInvoiceLine2.JI_RX_NKLinePriceCurr);

				var itemline3 = consignment1.Items[1].Lines[0];
				var jobComInvoiceLine3 = jobComInvoiceHeader.JobComInvoiceLines[2];
				AssertEquals(itemline3.HVS_ProductCode, jobComInvoiceLine3.JI_PartNo);
				AssertEquals(itemline3.HVS_CC_Lookup, jobComInvoiceLine3.JI_CC);
				AssertEquals(itemline3.HVS_FormattedDestinationTariff, jobComInvoiceLine3.JI_Tariff);
				AssertEquals(itemline3.HVS_GoodsDescription.ToUpper(), jobComInvoiceLine3.JI_Description);
				AssertEquals((ZDecimal)(itemline3.HVS_Quantity), jobComInvoiceLine3.JI_InvoiceQuantity);
				AssertEquals(itemline3.HVS_GrossWeight, jobComInvoiceLine3.JI_Weight);
				AssertEquals(itemline3.HVS_WeightUnit, jobComInvoiceLine3.JI_WeightUQ);
				AssertEquals(itemline3.HVS_NetWeight, jobComInvoiceLine3.JI_NetWeight);
				AssertEquals(itemline3.HVS_WeightUnit, jobComInvoiceLine3.JI_NetWeightUQ);
				AssertEquals(itemline3.HVS_RN_NKOriginCountryCode, jobComInvoiceLine3.JI_CountryOfOrigin);
				AssertEquals(itemline3.HVS_CustomsValue, jobComInvoiceLine3.JI_LinePrice);
				AssertEquals(consignment1.HVC_RX_NKGoodsValueCurrency, jobComInvoiceLine3.JI_RX_NKLinePriceCurr);
				#endregion

				var consignment2 = consignment1;
				var shipment2 = Factory.New<ForwardingShipment>();
				shipment2.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment2.JS_RL_NKOrigin = "AUSYD";
				shipment2.JS_RL_NKDestination = "NZAKL";
				shipment2.JS_UniqueConsignRef = "SHIP000002";

				var arrivalConsol2 = shipment2.Consols.AddNew();
				arrivalConsol2.JK_RL_NKLoadPort = "AUBNE";
				arrivalConsol2.JK_RL_NKDischargePort = "NZAKL";

				consignment2.HVC_JS_ManifestedOnShipment = shipment2.PK;

				consignment2.Items[0].HVI_ActualWeight = 0;
				consignment2.Items[0].HVI_ActualVolume = 0;
				consignment2.Items[0].HVI_ManifestedVolume = 1;
				consignment2.Items[1].HVI_ActualWeight = 0;
				consignment2.Items[1].HVI_ActualVolume = 0;
				consignment2.Items[1].HVI_ManifestedVolume = 2;
				consignment2.Items[0].HVI_F3_NKPackType = "PFA";
				consignment2.HVC_GoodsValue = 0.0;
				consignment2.HVC_RN_NKConsigneeCountryCode = "NZ";
				consignment2.HVC_RN_NKShipperCountryCode = "AU";
				consignment2.Items[0].Lines[0].HVS_CC_Lookup = ZGuid.Empty;
				consignment2.Items[0].Lines[0].HVS_FormattedOriginTariff = "111";
				consignment2.Items[0].Lines[0].HVS_FormattedDestinationTariff = "222";
				consignment2.Items[0].Lines[1].HVS_FormattedOriginTariff = "333";
				consignment2.Items[0].Lines[1].HVS_FormattedDestinationTariff = "444";
				consignment2.Items[1].Lines[0].HVS_FormattedOriginTariff = "555";
				consignment2.Items[1].Lines[0].HVS_FormattedDestinationTariff = "666";

				var consol2 = consignment2.ManifestedOnShipment.ArrivalConsol;
				var writer2 = GetDataObjectWriter(consignment2);

				var dataObject2 = writer.GetDataObject(consignment2);
				dataObject2.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				dataObject2.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				factory = new BusinessObjectFactory();
				PublishUniversalXmlResult events2;
				using (factory.AddDisposableService())
				{
					events2 = UniversalXmlWorkflowProcessor.PublishUniversalXMLInternally(factory, consignment2, dataObject2, EDIMessageSubTypeList.Codes.XmlUniversalShipment);
					factory.Save();
				}

				var dimEvent2 = events2.SingleOrDefault(e => e.EventType.Value == AutoEvents.DataImportCode);
				var dataSource2 = dimEvent2?.DataContext?.DataSourceCollection?.SingleOrDefault(s => s.Type.Value == nameof(DataContextType.CustomsDeclaration));
				var thisDeclarationReference2 = dataSource2.Key;
				var declaration2 = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, thisDeclarationReference2));

				#region Assert when some properties are empty
				AssertEquals(consignment2.HVC_ManifestedWeight, declaration2.JE_TotalWeight);
				AssertEquals(consignment2.HVC_ManifestedVolume, declaration2.JE_TotalVolume);
				AssertEquals(consignment2.Items.OfType<HVLVItem>().FirstOrDefault()?.HVI_F3_NKPackType, declaration2.JE_TotalNoOfPacksPackType);
				AssertEquals(consignment2.HVC_OH_LastMileCarrier, declaration2.JE_OA_DeliveryOrPickupCartageCoAddr_ZAddress.OrgPK);

				var jobComInvoiceHeader2 = declaration2.AllGroupHeaders[0].AllJobComInvoiceHeaders.Single();
				AssertEquals(consignment2.TotalLineCustomsValues, jobComInvoiceHeader2.JZ_InvoiceAmount);

				AssertEquals(consignment2.Items[0].Lines[0].HVS_FormattedOriginTariff, jobComInvoiceHeader2.InvoiceLines[0].JI_Tariff);
				AssertEquals(consignment2.Items[0].Lines[1].HVS_FormattedOriginTariff, jobComInvoiceHeader2.InvoiceLines[1].JI_Tariff);
				AssertEquals(consignment2.Items[1].Lines[0].HVS_FormattedOriginTariff, jobComInvoiceHeader2.InvoiceLines[2].JI_Tariff);
				#endregion

				var consignment3 = consignment2;
				consignment3.HVC_HCH_Header = ZGuid.Empty;
				var writer3 = GetDataObjectWriter(consignment3);

				var dataObject3 = writer.GetDataObject(consignment3);

				#region Assert when manifestedshipment is null
				AssertEquals(1, dataObject3.DataContext.DataSourceCollection.Count());
				Assert(dataObject3.GetMatchingDataSource(DataContextType.ForwardingConsol) == null);
				Assert(dataObject3.GetMatchingDataSource(DataContextType.ForwardingShipment) == null);
				Assert(dataObject3.GetMatchingDataSource(DataContextType.HVLVConsignment) != null);
				#endregion
			}
		}

		public ForwardingShipment PopulateAndGetHVLShipment()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			#region orgs
			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			billToParty.MainAddress.Address1 = "45 Bill To Street";
			billToParty.Contacts.AddNew().OC_ContactName = "Bill";

			var destinationDepotOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			destinationDepotOrg1.MainAddress.Address1 = "12 Road Nanjing";
			var lastMileDelivery = Factory.NewWithValidTestData<OrgHeader>();
			lastMileDelivery.MainAddress.Address1 = "99 Road Shanghai";
			var lastMileCarrierBookingAgent = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrierBookingAgent.MainAddress.Address1 = "66 Road Beijing";

			var destinationDepotOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			destinationDepotOrg2.MainAddress.Address1 = "13 Destination St";

			var cusclass = Factory.New<BaseCusClassification>();
			cusclass.CC_LookupCode = "1234";
			cusclass.CC_ClassificationType = Customs.Common.ClassificationType.EXP;
			cusclass.CC_TariffNum = "4321";
			#region shipment in consignment1 setup(set consignment.DirectionOfTrade = Directions.Import)
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_UniqueConsignRef = "SHIP000001";

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "HighWind";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "SSS111222";
			voyage.JV_AirSeaRoad = "SEA";
			voyage.JV_VoyageType = "MAI";

			var origin = voyage.Origins.AddNew();
			origin.JA_A_DEP = new ZDateTime(2020, 5, 12);
			origin.JA_RL_NKPortOfLoading = "AUBNE";

			var destination = voyage.Destinations.AddNew();
			destination.JB_A_ARV = new ZDateTime(2020, 5, 13);
			destination.JB_RL_NKPortOfDischarge = "AUSYD";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var leg = arrivalConsol.Transports[0];
			leg.JW_ETD = new ZDateTime(2020, 5, 12);
			leg.JW_ETA = new ZDateTime(2020, 5, 13);

			leg.JW_Vessel = vessel.RV_Code;
			leg.JW_VoyageFlight = "SSS111222";
			#endregion

			Factory.Save();
			#endregion

			#region set up
			#region header property
			var bookingHeader = Factory.New<HVLVBookingHeader>();

			bookingHeader.HVH_OA_BillToParty = billToParty.MainAddress.PK;
			bookingHeader.HVH_OC_BillToPartyContact = billToParty.Contacts[0].PK;
			#endregion

			#region consignments
			#region consignment1
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment1 = consignmentHeader.Consignments.AddNew();

			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_ConsignmentId = "CONSIFIRST";
			consignment1.HVC_WaybillNumber = "XA13DQ";
			consignment1.HVC_ShipperReference = "123ABC";
			consignment1.HVC_ItemCount = 0;
			consignment1.HVC_GoodsValue = 200;
			consignment1.HVC_RX_NKGoodsValueCurrency = "BGN";
			consignment1.HVC_WeightUQ = Weight.Kilograms;
			consignment1.HVC_VolumeUQ = Volume.CubicMetres;
			consignment1.HVC_GoodsDescription = "GUNS";
			consignment1.HVC_ConsigneeInstructions = "Leave at front";
			consignment1.HVC_INCO = IncoTerms.DeliveredAtFrontier;
			consignment1.HVC_IsHazardous = true;
			consignment1.HVC_IsSignatureRequired = true;
			consignment1.HVC_AuthorityToLeave = true;
			consignment1.HVC_RequiresFumigation = true;
			consignment1.HVC_IsPersonalEffects = true;
			consignment1.HVC_IsTimber = true;
			consignment1.HVC_IsPerishable = true;
			consignment1.HVC_UndgClass = "3.3D";
			consignment1.HVC_PL_NKLastMileCarrierServiceLevel = "EXP";
			consignment1.HVC_IsTaxPrePaid = true;

			consignment1.HVC_OA_DestinationDepot = destinationDepotOrg1.MainAddress.PK;

			consignment1.HVC_OH_LastMileCarrier = lastMileDelivery.PK;

			consignment1.HVC_OH_LastMileCarrierBookingAgent = lastMileCarrierBookingAgent.PK;

			consignment1.HVC_ConsigneeName = "Murray";
			consignment1.HVC_ConsigneeAddress1 = "99 Consignee Road";
			consignment1.HVC_ConsigneeAddress2 = "Downtown";
			consignment1.HVC_ConsigneeCity = "New York";
			consignment1.HVC_ConsigneeState = "NY";
			consignment1.HVC_ConsigneePostcode = "12345";
			consignment1.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment1.HVC_ConsigneeContact = "Moo ray";
			consignment1.HVC_ConsigneeEmail = "murray.hewitt@usconsulate.gov.nz";
			consignment1.HVC_ConsigneePhone = "7";
			consignment1.HVC_ConsigneeMobile = "+1234567890";
			consignment1.HVC_ConsigneeFax = "+0987654321";

			consignment1.HVC_ShipperName = "Some Company";
			consignment1.HVC_ShipperAddress1 = "22 Shipper Street";
			consignment1.HVC_ShipperCity = "Wellington";
			consignment1.HVC_ShipperState = "WLG";
			consignment1.HVC_ShipperPostcode = "54321";
			consignment1.HVC_RN_NKShipperCountryCode = "NZ";
			consignment1.HVC_ShipperContact = "Randy";
			consignment1.HVC_ShipperEmail = "randy@randysdomain.com";
			consignment1.HVC_ShipperPhone = "01189998819991197253";
			consignment1.HVC_ShipperMobile = "+555 5555";
			consignment1.HVC_ShipperFax = "8";

			consignment1.HVC_VendorIdentifier = "VID0001";
			#endregion
			#endregion

			#region Items

			var item1_1 = consignment1.Items.AddNew();
			item1_1.HVI_ItemId = "D9901239028";
			item1_1.HVI_ShipperReference = "S999887";
			item1_1.HVI_CurrentBarcode = "12345678901234567890123456789012345678901234567890";
			item1_1.HVI_F3_NKPackType = "BOX";
			item1_1.HVI_Height = 3m;
			item1_1.HVI_Length = 4m;
			item1_1.HVI_Width = 5m;
			item1_1.HVI_UnitOfDimension = Length.Metres;
			item1_1.HVI_ManifestedWeight = 5;
			item1_1.HVI_ActualWeight = 10;
			item1_1.HVI_ManifestedVolume = 0.5;
			item1_1.HVI_ActualVolume = 0.5;
			item1_1.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;
			item1_1.HVI_IsDamaged = true;
			item1_1.HVI_IsPillaged = true;

			var item1_2 = consignment1.Items.AddNew();
			item1_2.HVI_ItemId = "D5465421481";
			item1_2.HVI_F3_NKPackType = "BOX";
			item1_2.HVI_ManifestedWeight = 10;
			item1_2.HVI_ActualWeight = 10;
			item1_2.HVI_ManifestedVolume = 0.5;
			item1_2.HVI_ActualVolume = 0.5;
			item1_2.HVI_Status = HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot;
			item1_2.HVI_IsDamaged = true;
			item1_2.HVI_IsPillaged = false;
			#endregion

			#region Item Lines

			var line1_1_1 = item1_1.Lines.AddNew();
			line1_1_1.HVS_OriginTariff = "123456";
			line1_1_1.HVS_RN_NKOriginCountryCode = "AU";
			line1_1_1.HVS_CustomsValue = 12.34;
			line1_1_1.HVS_IntrinsicValue = 54.321;
			line1_1_1.HVS_DestinationTariff = "654321";
			line1_1_1.HVS_GoodsDescription = "Keyboard";
			line1_1_1.HVS_GrossWeight = 1.23;
			line1_1_1.HVS_ItemURL = "www.google.com/keyboard";
			line1_1_1.HVS_NetWeight = 3.21;
			var product = Factory.New<Customs.Business.OrgSupplierPart>();
			product.OP_PartNum = "KBD001";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = bookingHeader.BillToParty.OA_OH;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			line1_1_1.HVS_ProductCode = "KBD001";
			line1_1_1.HVS_CC_Lookup = cusclass.PK;
			line1_1_1.HVS_Quantity = 2;
			line1_1_1.HVS_WeightUnit = "KG";

			var line1_1_2 = item1_1.Lines.AddNew();
			line1_1_2.HVS_OriginTariff = "12345678";
			line1_1_2.HVS_RN_NKOriginCountryCode = "NZ";
			line1_1_2.HVS_DestinationTariff = "87654321";
			line1_1_2.HVS_GrossWeight = 2.34;
			line1_1_2.HVS_NetWeight = 4.32;
			line1_1_2.HVS_Quantity = 3;
			line1_1_2.HVS_WeightUnit = "T";
			line1_1_2.HVS_GoodsDescription = "Screen";

			var line1_2_1 = item1_2.Lines.AddNew();
			line1_2_1.HVS_OriginTariff = "123123";
			line1_2_1.HVS_RN_NKOriginCountryCode = "FR";
			line1_2_1.HVS_DestinationTariff = "321321";
			line1_2_1.HVS_GrossWeight = 1.2;
			line1_2_1.HVS_NetWeight = 3.4;
			line1_2_1.HVS_Quantity = 4;
			line1_2_1.HVS_WeightUnit = "KG";
			line1_2_1.HVS_GoodsDescription = "Mouse";
			#endregion
			#endregion

			return shipment;
		}

		ITopLevelDataObjectWriter GetDataObjectWriter(HVLVConsignment consignment, IEnumerable<HVLVConsignment> consignmentsToMerge = null)
		{
			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.HSA, consignment));
			var dataExportStrategy = ObjectFactory.Get<IHVLVConsignmentDataExportStrategy>("HVLVConsignmentToDeclarationDataExportStrategy");
			var writer = ObjectFactory.Get<IHVLVConsignmentDataObjectWriter>("HVLVConsignmentDataObjectWriter", writeManager, dataExportStrategy);
			writer.SetConsignmentsToMerge(consignmentsToMerge);
			return writer as ITopLevelDataObjectWriter;
		}

		protected override IHVLVConsignmentDataExportStrategy DataExportStrategy => HVLVConsignmentToDeclarationDataExportStrategy.Instance;
	}
}
