using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingShipment_IShipmentDeclarationTest : IShipmentDeclarationTest
	{
		#region Milestones

		public void TestMilestones()
		{
			var testShipping = Factory.NewWithValidTestData<TrackingShipment>();
			AssertNotNull(testShipping.Milestones);
			AssertEquals(0, testShipping.Milestones.Count);

			var milestone1 = testShipping.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "lastCompleted1";
			milestone1.P9_IsPublished = true;
			milestone1.SetMilestoneActualDateForTest(DateTime.Now);
			milestone1.P9_Sequence = 1;

			var milestone2 = testShipping.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "lastCompleted2";
			milestone2.P9_IsPublished = true;
			milestone2.SetMilestoneActualDateForTest(DateTime.Now);
			milestone2.P9_Sequence = 1;
			testShipping.Factory.Save();
			AssertEquals(0, testShipping.Milestones.Count);

			testShipping.ReloadMilestones();
			AssertEquals(2, testShipping.Milestones.Count);
		}

		#endregion

		#region TestPersistentBizOPK

		public override void TestPersistentBizOPK()
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			AssertEquals(shipment.PK, ((IShipmentDeclaration)shipment).PersistentBizOPK);
		}

		#endregion

		#region TestCurrentLoadPort

		public override void TestCurrentLoadPort()
		{
			RefUNLOCO port = Factory.NewWithValidTestData<RefUNLOCO>();

			TrackingShipment shipment = GetNewTrackingShipment();
			Transport transport = shipment.Consols[0].Transports[0];
			JobSailing sailing = Factory.NewWithValidTestData<JobSailing>();
			transport.JW_JX = sailing.PK;
			sailing.Origin.JA_RL_NKPortOfLoading = port.RL_Code;

			Factory.Save();

			AssertEquals(port.RL_Code, shipment.CurrentLoadPort);
		}

		#endregion

		#region TestCurrentDischargePort

		public override void TestCurrentDischargePort()
		{
			RefUNLOCO port = Factory.NewWithValidTestData<RefUNLOCO>();

			TrackingShipment shipment = GetNewTrackingShipment();
			Transport transport = shipment.Consols[0].Transports[0];
			JobSailing sailing = Factory.NewWithValidTestData<JobSailing>();
			transport.JW_JX = sailing.PK;
			sailing.Destination.JB_RL_NKPortOfDischarge = port.RL_Code;

			Factory.Save();

			AssertEquals(port.RL_Code, shipment.CurrentDischargePort);
		}

		#endregion

		#region TestNumber

		public override void TestNumber()
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			shipment.JS_UniqueConsignRef = "001";

			AssertEquals("001", shipment.Number);
		}

		#endregion

		#region TestHouseBill

		public override void TestHouseBill()
		{
			TrackingShipment trackingShipment = GetNewTrackingShipment();
			trackingShipment.JS_HouseBill = "H001";
			TrackingConsol consol = trackingShipment.Consols[0];
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_MasterBillNum = "M001";

			Factory.Save();

			AssertEquals("M001", trackingShipment.HouseBill);

			TrackingShipment trackingShipment2 = GetNewTrackingShipment();
			trackingShipment2.JS_HouseBill = "H002";
			TrackingConsol consol2 = trackingShipment.Consols[0];
			consol2.JK_AgentType = Constants.AgentType.Agent;
			consol2.JK_MasterBillNum = "M002";

			Factory.Save();

			AssertEquals("H002", trackingShipment2.HouseBill);
		}

		#endregion

		#region TestMasterBill

		public override void TestMasterBill()
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			TrackingConsol consol = shipment.Consols[0];
			consol.JK_MasterBillNum = "M001";

			Factory.Save();

			AssertEquals("M001", shipment.MasterBill);
		}

		#endregion

		#region TestOriginPortCode

		public override void TestOriginPortCode()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			AssertEquals("AUSYD", shipment.OriginPortCode);
		}

		#endregion

		#region TestDestinationPortCode

		public override void TestDestinationPortCode()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals("AUSYD", shipment.DestinationPortCode);
		}

		#endregion

		#region TestConsignorProperties

		public override void TestConsignorProperties()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();

			JobDocAddress consignorAddress = shipment.ConsignorDocumentaryAddress;
			consignorAddress.E2_OA_Address = TestOrg.Addresses[0].PK;
			WebAddressFormatter formatter = new WebAddressFormatter(consignorAddress);

			AssertEquals("DocAddress should not be overriden", false, consignorAddress.E2_AddressOverride);

			AssertEquals("Company Name", TestOrg.OH_FullName, shipment.ConsignorName);
			AssertEquals("Full Address", formatter.FormattedAddressWithCompanyName(consignorAddress.Organisation.OH_FullName), shipment.ConsignorFullAddress);
			AssertEquals("Address1", consignorAddress.Address.OA_Address1, shipment.ConsignorAddress);
			AssertEquals("City", consignorAddress.Address.OA_City, shipment.ConsignorCity);
			AssertEquals("State", consignorAddress.Address.OA_State, shipment.ConsignorState);
			AssertEquals("Post Code", consignorAddress.Address.OA_PostCode, shipment.ConsignorPostCode);

			consignorAddress.E2_AddressOverride = true;
			consignorAddress.E2_CompanyName = "";
			consignorAddress.E2_Address1 = "Test Address";
			consignorAddress.E2_City = "Test City";
			consignorAddress.E2_State = "Test";
			consignorAddress.E2_Postcode = "1111";

			AssertEquals("Company Name should be empty", "", shipment.ConsignorName);
			consignorAddress.E2_CompanyName = "Test";
			AssertEquals("Company Name", consignorAddress.E2_CompanyName, shipment.ConsignorName);
			AssertEquals("Full Address", formatter.FormattedAddressWithCompanyName("Test"), shipment.ConsignorFullAddress);
			AssertEquals("Address1", consignorAddress.E2_Address1, shipment.ConsignorAddress);
			AssertEquals("City", consignorAddress.E2_City, shipment.ConsignorCity);
			AssertEquals("State", consignorAddress.E2_State, shipment.ConsignorState);
			AssertEquals("Post Code", consignorAddress.E2_Postcode, shipment.ConsignorPostCode);
		}

		#endregion

		#region TestDeliverToProperties

		public void TestDeliverToProperties()
		{
			var shipment = Factory.New<TrackingShipment>();

			var deliveryAddress = shipment.ConsigneeDeliveryAddress;
			deliveryAddress.E2_OA_Address = TestOrg.Addresses[0].PK;
			WebAddressFormatter formatter = new WebAddressFormatter(deliveryAddress);

			AssertEquals("DocAddress should not be overriden", false, deliveryAddress.E2_AddressOverride);

			AssertEquals("Company Name", TestOrg.OH_FullName, shipment.DeliverToName);
			AssertEquals("Full Address", formatter.FormattedAddressWithCompanyName(deliveryAddress.Organisation.OH_FullName), shipment.DeliverToFullAddress);

			deliveryAddress.E2_AddressOverride = true;
			deliveryAddress.E2_CompanyName = "";
			deliveryAddress.E2_Address1 = "Test Address";
			deliveryAddress.E2_City = "Test City";
			deliveryAddress.E2_State = "Test";
			deliveryAddress.E2_Postcode = "1111";

			AssertEquals("Company Name should be empty", "", shipment.DeliverToName);
			deliveryAddress.E2_CompanyName = "Test";
			AssertEquals("Company Name", deliveryAddress.E2_CompanyName, shipment.DeliverToName);
			AssertEquals("Full Address", formatter.FormattedAddressWithCompanyName("Test"), shipment.DeliverToFullAddress);
		}

		#endregion

		#region TestPickupFromProperties

		public void TestPickupFromProperties()
		{
			var shipment = Factory.New<TrackingShipment>();

			var pickupAddress = shipment.ConsignorPickupAddress;
			pickupAddress.E2_OA_Address = TestOrg.Addresses[0].PK;
			WebAddressFormatter formatter = new WebAddressFormatter(pickupAddress);

			AssertEquals("DocAddress should not be overriden", false, pickupAddress.E2_AddressOverride);

			AssertEquals("Company Name", TestOrg.OH_FullName, shipment.PickupFromName);
			AssertEquals("Full Address", formatter.FormattedAddressWithCompanyName(pickupAddress.Organisation.OH_FullName), shipment.PickupFromFullAddress);

			pickupAddress.E2_AddressOverride = true;
			pickupAddress.E2_CompanyName = "";
			pickupAddress.E2_Address1 = "Test Address";
			pickupAddress.E2_City = "Test City";
			pickupAddress.E2_State = "Test";
			pickupAddress.E2_Postcode = "1111";

			AssertEquals("Company Name should be empty", "", shipment.PickupFromName);
			pickupAddress.E2_CompanyName = "Test";
			AssertEquals("Company Name", pickupAddress.E2_CompanyName, shipment.PickupFromName);
			AssertEquals("Full Address", formatter.FormattedAddressWithCompanyName("Test"), shipment.PickupFromFullAddress);
		}

		#endregion

		#region TestConsigneeProperties

		public override void TestConsigneeProperties()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();

			JobDocAddress consigneeAddress = shipment.ConsigneeDocumentaryAddress;
			consigneeAddress.E2_OA_Address = TestOrg.Addresses[0].PK;
			WebAddressFormatter formatter = new WebAddressFormatter(consigneeAddress);

			AssertEquals("DocAddress should not be overriden", false, consigneeAddress.E2_AddressOverride);

			AssertEquals("Company Name", TestOrg.OH_FullName, shipment.ConsigneeName);
			AssertEquals("Full Address", formatter.FormattedAddressWithCompanyName(consigneeAddress.Organisation.OH_FullName), shipment.ConsigneeFullAddress);
			AssertEquals("Address1", consigneeAddress.Address.OA_Address1, shipment.ConsigneeAddress);
			AssertEquals("City", consigneeAddress.Address.OA_City, shipment.ConsigneeCity);
			AssertEquals("State", consigneeAddress.Address.OA_State, shipment.ConsigneeState);
			AssertEquals("Post Code", consigneeAddress.Address.OA_PostCode, shipment.ConsigneePostCode);

			consigneeAddress.E2_AddressOverride = true;
			consigneeAddress.E2_CompanyName = "";
			consigneeAddress.E2_Address1 = "Test Address";
			consigneeAddress.E2_City = "Test City";
			consigneeAddress.E2_State = "Test";
			consigneeAddress.E2_Postcode = "1111";

			AssertEquals("Company Name should be empty", "", shipment.ConsigneeName);
			consigneeAddress.E2_CompanyName = "Test";
			AssertEquals("Company Name", consigneeAddress.E2_CompanyName, shipment.ConsigneeName);
			AssertEquals("Full Address", formatter.FormattedAddressWithCompanyName("Test"), shipment.ConsigneeFullAddress);
			AssertEquals("Address1", consigneeAddress.E2_Address1, shipment.ConsigneeAddress);
			AssertEquals("City", consigneeAddress.E2_City, shipment.ConsigneeCity);
			AssertEquals("State", consigneeAddress.E2_State, shipment.ConsigneeState);
			AssertEquals("Post Code", consigneeAddress.E2_Postcode, shipment.ConsigneePostCode);
		}

		#endregion

		#region TestETA

		public override void TestETA()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();

			DateTime testDate = new DateTime(2009, 01, 01);
			shipment.JS_E_ARV = testDate;
			AssertEquals(testDate, shipment.ETA);
		}

		#endregion

		#region TestBookingReference

		public override void TestBookingReference()
		{
			var testShipment = Factory.New<TrackingShipment>();
			testShipment.JS_BookingReference = "BR2000";
			AssertEquals("Booking Reference", "BR2000", testShipment.BookingReference);
		}

		#endregion

		#region TestOwnerReference

		public override void TestOwnerReference()
		{
			var testShipment = Factory.New<TrackingShipment>();
			var expDeclaration = Factory.New<BaseJobDeclaration>();
			expDeclaration.JE_JS = testShipment.PK;
			expDeclaration.JE_OwnerRef = "EXP";
			AssertEquals("if OwnerRef on dec are set, display", "EXP", testShipment.OwnerReference);
		}

		#endregion

		#region TestPacksWithUnits

		public override void TestPacksWithUnits()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.JS_OuterPacks = 5;
			shipment.JS_F3_NKPackType = "PLT";
			AssertEquals("5 PLT", shipment.PacksWithUnits);
		}

		#endregion

		#region TestVolumeWithUnits

		public override void TestVolumeWithUnits()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.JS_ActualVolume = 5;
			shipment.JS_UnitOfVolume = "M3";
			AssertEquals("5.000 M3", shipment.VolumeWithUnits);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_SeaVolume = collection.AddNew();
			defaultNumberOfDecimals_SeaVolume.UnitOfMeasure = Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_SeaVolume.TransportMode = Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaVolume.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaVolume.RoundingMode = RoundingModes.Down;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			shipment.JS_ActualVolume = 12.128;
			shipment.JS_UnitOfVolume = "M3";
			AssertEquals("12.12 M3", shipment.VolumeWithUnits);
		}

		#endregion

		#region TestWeightWithUnits

		public override void TestWeightWithUnits()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.JS_ActualWeight = 5;
			shipment.JS_UnitOfWeight = "KG";
			AssertEquals("5.000 KG", shipment.WeightWithUnits);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_SeaWeight = collection.AddNew();
			defaultNumberOfDecimals_SeaWeight.UnitOfMeasure = Constants.Weight.Kilograms;
			defaultNumberOfDecimals_SeaWeight.TransportMode = Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaWeight.RoundingMode = RoundingModes.Up;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			shipment.JS_ActualWeight = 12.251;
			shipment.JS_UnitOfWeight = "KG";
			AssertEquals("12.26 KG", shipment.WeightWithUnits);
		}

		#endregion

		#region TestGoodsProperties

		public override void TestGoodsProperties()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.JS_GoodsValue = 100;
			shipment.JS_RX_NKGoodsValueCurr = "USD";
			shipment.JS_GoodsDescription = "Drugs";
			AssertEquals(100m, shipment.GoodsValue);
			AssertEquals("USD", shipment.GoodsValueCurrency);
			AssertEquals("Drugs", shipment.GoodsDescription);
		}

		#endregion

		#region TestDocsAndCartageProperties

		public override void TestDocsAndCartageProperties()
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			AssertNotNull("DocsAndCartage", shipment.DocsAndCartage);

			ZDateTime testDate = new ZDateTime(2009, 01, 19);

			shipment.DocsAndCartage.JP_EstimatedPickup = testDate;
			shipment.DocsAndCartage.JP_PickupRequiredBy = testDate.AddDays(1);
			shipment.DocsAndCartage.JP_EstimatedDelivery = testDate.AddDays(2);
			shipment.DocsAndCartage.JP_DeliveryRequiredBy = testDate.AddDays(3);
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = testDate.AddDays(4);
			shipment.DocsAndCartage.JP_PickupCartageCompleted = testDate.AddDays(5);

			AssertEquals("EstimatedPickupDate", testDate, shipment.EstimatedPickupDate);
			AssertEquals("PickupDateRequiredBy", testDate.AddDays(1), shipment.PickupDateRequiredBy);
			AssertEquals("EstimatedDeliveryDate", testDate.AddDays(2), shipment.EstimatedDeliveryDate);
			AssertEquals("DeliveryDateRequiredBy", testDate.AddDays(3), shipment.DeliveryDateRequiredBy);
			AssertEquals("DeliveryDate", testDate.AddDays(4), shipment.DeliveryDate);
			AssertEquals("ActualPickupDate", testDate.AddDays(5), shipment.ActualPickupDate);
		}

		#endregion

		#region TestServiceLevelCode

		public override void TestServiceLevelCode()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.JS_RS_NKServiceLevel = "D2D";
			AssertEquals("D2D", shipment.ServiceLevelCode);
		}

		#endregion

		#region TestDeliveredLegProperties

		public override void TestDeliveredLegProperties()
		{
			TrackingShipment shipment = GetNewTrackingShipment();

			AssertEquals("ReceivedDate", ZDateTime.Empty, shipment.ReceivedDate);
			AssertEquals("ReceivedBy", ZString.Empty, shipment.ReceivedBy);
			AssertEquals("PiecesReceived", ZInt.Zero, shipment.PiecesReceived);

			TrackingPackLine packline1 = shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm leg1 = shipment.DeliveryConfirms.AddNew();
			CommonPickupDeliveryConfirm leg2 = shipment.DeliveryConfirms.AddNew();

			leg1.EU_PickupDeliveryTime = new ZDateTime(2008, 03, 8);
			leg1.EU_GoodsSignForBy = "First";
			leg2.EU_PickupDeliveryTime = new ZDateTime(2008, 03, 19);
			leg2.EU_GoodsSignForBy = "Second";
			CommonConfirmDivot divot = leg2.GetDivot(packline1);
			divot.J8_PackagesDelivered = 5;

			AssertEquals("ReceivedDate", new ZDateTime(2008, 03, 19), shipment.ReceivedDate);
			AssertEquals("ReceivedBy", "Second", shipment.ReceivedBy);
			AssertEquals("PiecesReceived", 5, shipment.PiecesReceived);
		}

		#endregion

		#region TestBookedOnline

		public override void TestBookedOnline()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();

			Assert("Not BookedOnline", !shipment.BookedOnline);

			shipment.JS_SystemCreateUser = "ZZ";
			Assert("BookedOnline", shipment.BookedOnline);
		}

		#endregion

		#region TestTop3Containers

		public override void TestTop3Containers()
		{
			new BusinessObjectFactory().New<TrackingPackLine>();

			TrackingShipment shipment = Factory.New<TrackingShipment>();

			var testContainer1 = Factory.New<TrackingContainer>();
			testContainer1.JC_ContainerNum = "1";
			var testContainer2 = Factory.New<TrackingContainer>();
			testContainer2.JC_ContainerNum = "2";
			var testContainer3 = Factory.New<TrackingContainer>();
			testContainer3.JC_ContainerNum = "3";
			var testContainer4 = Factory.New<TrackingContainer>();
			testContainer4.JC_ContainerNum = "";

			var testConsol = shipment.Consols.AddNew();

			testConsol.Containers.Add(testContainer1);
			testConsol.Containers.Add(testContainer2);
			testConsol.Containers.Add(testContainer3);
			testConsol.Containers.Add(testContainer4);

			TrackingPackLine line1 = AddLineToShipment(shipment);
			TrackingPackLine line2 = AddLineToShipment(shipment);
			TrackingPackLine line3 = AddLineToShipment(shipment);
			TrackingPackLine line4 = AddLineToShipment(shipment);

			AssertEquals("Should be no Containers because they weren't attached to PackLines", ZString.Empty, shipment.Top3Containers);

			line1.Containers.Add(testContainer1);
			line2.Containers.Add(testContainer2);

			AssertTop3Containers("Should be 2 containers", shipment.Top3Containers, new string[] { "1", "2" });

			line3.Containers.Add(testContainer3);
			line4.Containers.Add(testContainer4);

			AssertTop3Containers("Should be 3 containers and no empty one", shipment.Top3Containers, new string[] { "1", "2", "3" });

			line4.Containers.RemoveAll();
			line4.Containers.Add(testContainer3);

			AssertTop3Containers("Should be 3 containers and no duplicates", shipment.Top3Containers, new string[] { "1", "2", "3" });

			testContainer4.JC_ContainerNum = "4";
			line4.Containers.RemoveAll();
			line4.Containers.Add(testContainer4);

			AssertTop3Containers("Should be 3 containers and dots", shipment.Top3Containers, new string[] { "1", "2", "3", "..." });

			shipment.Consols.Remove(testConsol);

			AssertEquals("Should be no Containers because Consol was detached", ZString.Empty, shipment.Top3Containers);
		}

		void AssertTop3Containers(string message, string top3Containers, string[] containerNames)
		{
			Assert(message, top3Containers.Split(new char[] { ',' }).Length == containerNames.Length);

			foreach (string containerName in containerNames)
			{
				Assert("Should contain container " + containerName, top3Containers.Contains(containerName));
			}
		}

		TrackingPackLine AddLineToShipment(TrackingShipment shipment)
		{
			TrackingPackLine line = shipment.OuterPackLines.AddNew();
			line.Containers.RemoveAll();
			line.JL_FreightMode = FreightConstants.OuterPackType;
			return line;
		}

		#endregion

		#region TestOrderReference

		public override void TestOrderReference()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.DocsAndCartage.JP_OrderItemsAsString = "test";
			AssertEquals("test", shipment.OrderReference);
		}

		#endregion

		#region TestForwarders

		public override void TestForwarders()
		{
			TrackingShipment shipment = GetNewTrackingShipment();

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			shipment.Consols[0].SetDefaultSendingForwarderAddress(org1);
			shipment.Consols[0].SetDefaultReceivingForwarderAddress(org2);

			Factory.Save();

			AssertEquals("SendingForwarder", org1.PK, shipment.SendingForwarderPK);
			AssertEquals("ReceivingForwarder", org2.PK, shipment.ReceivingForwarderPK);
		}

		#endregion

		#region TestIsShipping

		public void TestIsShippingBillOfLading()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.JS_IsShipping = true;
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.WebFwdInstruction;
			AssertEquals(true, shipment.IsShippingBillOfLading);

			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			AssertEquals(true, shipment.IsShippingBillOfLading);

			shipment.JS_ShipmentStatus = "CN";
			AssertEquals(false, shipment.IsShippingBillOfLading);

			shipment.JS_ShipmentStatus = "NF";
			AssertEquals(false, shipment.IsShippingBillOfLading);

			shipment.JS_IsShipping = false;
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			AssertEquals(false, shipment.IsShippingBillOfLading);
		}

		public void TestIsShippingBooking()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.JS_IsShipping = true;
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.WebBooking;
			AssertEquals(true, shipment.IsShippingBooking);

			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			AssertEquals(true, shipment.IsShippingBooking);

			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;
			AssertEquals(true, shipment.IsShippingBooking);

			shipment.JS_ShipmentStatus = "WT";
			AssertEquals(false, shipment.IsShippingBooking);

			shipment.JS_ShipmentStatus = "TL";
			AssertEquals(false, shipment.IsShippingBooking);

			shipment.JS_IsShipping = false;
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;
			AssertEquals(false, shipment.IsShippingBooking);
		}

		#endregion

		#region TestTransports

		public void TestTransports()
		{
			DummyShipment shipment = GetNewTrackingShipment();

			Transport mainTransport = shipment.Consols[0].Transports[0];
			mainTransport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			mainTransport.JW_Vessel = "mainVessel";

			Factory.Save();
			AssertEquals("mainVessel", shipment.GetCurrentTransportForTest().JV_RV_NKVessel);
			AssertEquals("mainVessel", shipment.GetMainTransportForTest().JV_RV_NKVessel);

			Transport transport2 = shipment.Consols[0].Transports.AddNew();
			transport2.JW_TransportType = Constants.TransportPlanningType.PreCarriage;
			transport2.JW_ATA = ZDateTime.Now;
			transport2.JW_Vessel = "vessel2";

			Factory.Save();
			shipment.ResetTransports();

			AssertEquals("vessel2", shipment.GetCurrentTransportForTest().JV_RV_NKVessel);
			AssertEquals("mainVessel", shipment.GetMainTransportForTest().JV_RV_NKVessel);

			mainTransport.JW_TransportType = Constants.TransportPlanningType.Other;
			Transport transport3 = shipment.Consols[0].Transports.AddNew();
			transport3.JW_TransportType = Constants.TransportPlanningType.Other;
			transport3.JW_ATD = ZDateTime.Now;
			transport3.JW_Vessel = "vessel3";

			Factory.Save();
			shipment.ResetTransports();

			AssertEquals("vessel3", shipment.GetCurrentTransportForTest().JV_RV_NKVessel);
			AssertNull(shipment.GetMainTransportForTest());
		}

		#endregion

		#region TestLoadingMeters

		public override void TestLoadingMeters()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.JS_LoadingMeters = 10;
			AssertEquals("LoadingMeters", new ZDecimal(10), shipment.LoadingMeters);
		}

		#endregion

		#region TestTEUCount

		public void TestTEUCount()
		{
			var shipment = Factory.NewWithValidTestData<TrackingShipment>();
			var line1 = shipment.OuterPackLines.AddNew();
			var line2 = shipment.OuterPackLines.AddNew();
			AssertEquals(0m, shipment.TEUCount);

			var consol = shipment.Consols.AddNew();
			var container = consol.Containers.AddNew();
			container.JC_RC = Factory.NewWithValidTestData<RefContainer>().PK;
			container.JC_ContainerCount = 5;
			container.RefContainer.RC_TEU = 5;
			container.PackLines.Add(line1);

			container = consol.Containers.AddNew();
			container.JC_RC = Factory.NewWithValidTestData<RefContainer>().PK;
			container.JC_ContainerCount = 10;
			container.RefContainer.RC_TEU = 10;
			container.PackLines.Add(line2);

			AssertEquals(125m, shipment.TEUCount);
		}

		#endregion

		public void TestWorkflowItems()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();
			var workflow = shipment.WorkflowItems.AddNew();
			AssertEquals(typeof(TrackingShipmentProcessTask), workflow.GetType());
		}

		#region TestTop3JobNotes

		[HttpContextEnabledTest]
		public void TestTop3JobNotes()
		{
			var helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			var shipment = helper.CreateShipment();

			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.AgentNotes.Description, "NotJobNotes");
			AssertEquals(string.Empty, shipment.Top3JobNotes);

			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.ClientVisibleJobNotes.Description, "1");
			AssertEquals("1", shipment.Top3JobNotes);

			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.ClientVisibleJobNotes.Description, $"2A{System.Environment.NewLine}2B");
			AssertEquals($"1{System.Environment.NewLine}2A 2B", shipment.Top3JobNotes);

			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.ClientVisibleJobNotes.Description, "3");
			AssertEquals($"1{System.Environment.NewLine}2A 2B{System.Environment.NewLine}3", shipment.Top3JobNotes);

			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.ClientVisibleJobNotes.Description, "4");
			AssertEquals($"1{System.Environment.NewLine}2A 2B{System.Environment.NewLine}3{System.Environment.NewLine}...", shipment.Top3JobNotes);
		}

		#endregion

		[SetGlobalsIsWeb]
		public void TestFirstAndLastLegDates()
		{
			var shipment = Factory.New<TrackingShipment>();
			shipment.JS_RL_NKOrigin = "USORD";
			shipment.JS_RL_NKDestination = "AUSYD";
			var firstConsol = shipment.Consols.AddNew();
			firstConsol.JK_RL_NKLoadPort = "USORD";
			firstConsol.JK_RL_NKDischargePort = "USLAX";
			var lastConsol = shipment.Consols.AddNew();
			lastConsol.JK_RL_NKLoadPort = "USLAX";
			lastConsol.JK_RL_NKDischargePort = "AUSYD";

			AssertEquals("Precondition", firstConsol, shipment.FirstLoadConsol);
			AssertEquals("Precondition", lastConsol, shipment.LastDischargeConsol);

			var now = ZDateTime.Now;
			var etd = now.AddDays(1);
			var atd = now.AddDays(2);
			var eta = now.AddDays(4);
			var ata = now.AddDays(5);

			var firstLeg = firstConsol.Transports[0];
			firstLeg.JW_TransportMode = "AIR";
			firstLeg.JW_RL_NKLoadPort = "USORD";
			firstLeg.JW_RL_NKDiscPort = "USLAX";
			firstLeg.JW_ETD = etd;
			firstLeg.JW_ATD = atd;
			var lastLeg = lastConsol.Transports.AddNew();
			lastLeg.JW_TransportMode = "AIR";
			lastLeg.JW_RL_NKLoadPort = "USLAX";
			lastLeg.JW_RL_NKDiscPort = "AUSYD";
			lastLeg.JW_ETA = eta;
			lastLeg.JW_ATA = ata;
			AssertEquals("Precondition", firstLeg, shipment.FirstLoadConsol.Transports.DepartureTransport);
			AssertEquals("Precondition", lastLeg, shipment.LastDischargeConsol.Transports.ArrivalTransport);

			SuppressionForTest.CacheObjectClear();
			SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForDomestic, false);
			SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForImport, false);

			AssertEquals(etd, shipment.FirstLegLoadETD);
			AssertEquals(atd, shipment.FirstLegLoadATD);
			AssertEquals(eta, shipment.LastLegDischargeETA);
			AssertEquals(ata, shipment.LastLegDischargeATA);

			SuppressionForTest.CacheObjectClear();
			SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForDomestic, true);
			SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForImport, true);

			AssertEquals(Suppression.SuppressedDate, shipment.FirstLegLoadETD);
			AssertEquals(Suppression.SuppressedDate, shipment.FirstLegLoadATD);
			AssertEquals(Suppression.SuppressedDate, shipment.LastLegDischargeETA);
			AssertEquals(Suppression.SuppressedDate, shipment.LastLegDischargeATA);
		}

		#region Implementation

		protected override IShipmentDeclaration GetNewBizOForChargesTest()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.SiteUser = TestSiteUser;
			shipment.JS_UniqueConsignRef = "001";
			return shipment;
		}

		protected override CodeDescriptionBoolRegistryItem RegistryItem
		{
			get { return WebDataRegistry.Instance.SuppressFlightDetailsForDomestic; }
		}

		protected override IShipmentDeclaration GetNewBizOForSuppressedFieldsTest(ZDateTime date, ZString vessel, ZString voyageFlight, ZString transportMode)
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			shipment.JS_E_DEP = date;
			shipment.JS_E_ARV = date;

			Transport transport = shipment.Consols[0].Transports[0];
			transport.JW_Vessel = vessel;
			transport.JW_VoyageFlight = voyageFlight;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			shipment.JS_TransportMode = transportMode;

			Factory.Save();

			return shipment;
		}

		DummyShipment GetNewTrackingShipment()
		{
			DummyShipment result = Factory.New<DummyShipment>();

			TrackingConsol consol = result.Consols.AddNew();
			AssertEquals("Should be one consol", 1, result.Consols.Count);
			AssertEquals("Should be one transport on consol", 1, result.Consols[0].Transports.Count);

			return result;
		}

		class DummyShipment : TrackingShipment
		{
			public DummyShipment(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public void ResetTransports()
			{
				shipmentTransports = null;
			}

			public WebShipmentTransport GetCurrentTransportForTest()
			{
				return CurrentTransport;
			}

			public WebShipmentTransport GetMainTransportForTest()
			{
				return MainTransport;
			}
		}

		#endregion
	}
}
