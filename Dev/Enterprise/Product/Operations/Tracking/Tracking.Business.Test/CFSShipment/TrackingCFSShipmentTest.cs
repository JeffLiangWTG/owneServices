using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.CFS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[HttpContextEnabledTest]
	sealed class TrackingCFSShipmentTest : CFSShipmentTest
	{
		#region Setup & Control Overrides

		protected override CommonShipment GetShipment()
		{
			return Factory.New<TrackingCFSShipment>();
		}

		TrackingCFSShipment TestShipment
		{
			get
			{
				if (testShipment == null)
				{
					testShipment = (TrackingCFSShipment)GetShipment();
				}
				return testShipment;
			}
		}
		TrackingCFSShipment testShipment;

		OrgHeader TestOrg
		{
			get
			{
				if (testOrg == null)
				{
					testOrg = Factory.NewWithValidTestData<OrgHeader>();
				}
				return testOrg;
			}
		}
		OrgHeader testOrg;

		OrgAddress TestOrgAddress
		{
			get
			{
				if (testOrgAddress == null)
				{
					testOrgAddress = Factory.New<OrgAddress>();
					testOrgAddress.OA_Address1 = "Test Address Line 1";
					testOrgAddress.OA_Address2 = "Test Address Line 2";
					testOrgAddress.OA_City = "Test Address City";
				}
				return testOrgAddress;
			}
		}

		OrgAddress testOrgAddress;

		JobDocAddress TestJobDocAddress
		{
			get
			{
				if (testJobDocAddress == null)
				{
					testJobDocAddress = Factory.New<JobDocAddress>();
					testJobDocAddress.E2_OA_Address = TestOrgAddress.PK;
				}
				return testJobDocAddress;
			}
		}

		JobDocAddress testJobDocAddress;

		#endregion Setup & Control Overrides

		#region Related BizO Tests

		public void TestRelatedBizOReturnCorrectType()
		{
			AssertEquals("", typeof(CFSLoadListConsol), TestShipment.Consols.TypeOfElements);
			AssertEquals("", typeof(TrackingCFSShipment), TestShipment.CoLoadShipments.TypeOfElements);
			AssertEquals("", typeof(CFSPackLine), TestShipment.OuterPackLines.TypeOfElements);
		}

		#endregion Related BizO Tests

		#region TestVolumeWithUnits

		public void TestVolumeWithUnits()
		{
			var shipment = Factory.New<TrackingCFSShipment>();
			shipment.JS_ActualVolume = 5;
			shipment.JS_UnitOfVolume = "M3";
			AssertEquals("5.000 M3", shipment.VolumeWithUnits);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_SeaVolume = collection.AddNew();
			defaultNumberOfDecimals_SeaVolume.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_SeaVolume.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaVolume.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaVolume.RoundingMode = RoundingModes.Down;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			shipment.JS_ActualVolume = 12.128;
			shipment.JS_UnitOfVolume = "M3";
			AssertEquals("12.12 M3", shipment.VolumeWithUnits);
		}

		#endregion

		#region TestWeightWithUnits

		public void TestWeightWithUnits()
		{
			var shipment = Factory.New<TrackingCFSShipment>();
			shipment.JS_ActualWeight = 5;
			shipment.JS_UnitOfWeight = "KG";
			AssertEquals("5.000 KG", shipment.WeightWithUnits);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_SeaWeight = collection.AddNew();
			defaultNumberOfDecimals_SeaWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals_SeaWeight.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaWeight.RoundingMode = RoundingModes.Up;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			shipment.JS_ActualWeight = 12.251;
			shipment.JS_UnitOfWeight = "KG";
			AssertEquals("12.26 KG", shipment.WeightWithUnits);
		}

		#endregion

		#region TestCurrentLoadPort

		public void TestCurrentLoadPort()
		{
			var port = Factory.NewWithValidTestData<RefUNLOCO>();

			var shipment = GetNewTrackingCFSShipment();
			var transport = shipment.Consols[0].Transports[0];
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			transport.JW_JX = sailing.PK;
			sailing.Origin.JA_RL_NKPortOfLoading = port.RL_Code;

			Factory.Save();

			AssertEquals(port.RL_Code, shipment.CurrentLoadPort);
		}

		#endregion

		#region TestCurrentDischargePort

		public void TestCurrentDischargePort()
		{
			RefUNLOCO port = Factory.NewWithValidTestData<RefUNLOCO>();

			TrackingCFSShipment shipment = GetNewTrackingCFSShipment();
			Transport transport = shipment.Consols[0].Transports[0];
			JobSailing sailing = Factory.NewWithValidTestData<JobSailing>();
			transport.JW_JX = sailing.PK;
			sailing.Destination.JB_RL_NKPortOfDischarge = port.RL_Code;

			Factory.Save();

			AssertEquals(port.RL_Code, shipment.CurrentDischargePort);
		}

		DummyShipment GetNewTrackingCFSShipment()
		{
			var result = Factory.New<DummyShipment>();

			var consol = result.Consols.AddNew();
			AssertEquals("Should be one consol", 1, result.Consols.Count);
			AssertEquals("Should be one transport on consol", 1, result.Consols[0].Transports.Count);

			return result;
		}

		class DummyShipment : TrackingCFSShipment
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

		#region TestConsignorProperties

		public void TestConsignorProperties()
		{
			var shipment = Factory.New<TrackingCFSShipment>();

			var consignorAddress = shipment.ConsignorDocumentaryAddress;
			consignorAddress.E2_OA_Address = TestOrg.Addresses[0].PK;
			var formatter = new WebAddressFormatter(consignorAddress);

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
			var shipment = Factory.New<TrackingCFSShipment>();

			var deliveryAddress = shipment.ConsigneeDeliveryAddress;
			deliveryAddress.E2_OA_Address = TestOrg.Addresses[0].PK;
			var formatter = new WebAddressFormatter(deliveryAddress);

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
			var shipment = Factory.New<TrackingCFSShipment>();

			var pickupAddress = shipment.ConsignorPickupAddress;
			pickupAddress.E2_OA_Address = TestOrg.Addresses[0].PK;
			var formatter = new WebAddressFormatter(pickupAddress);

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

		public void TestConsigneeProperties()
		{
			var shipment = Factory.New<TrackingCFSShipment>();

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

		#region TestTransports

		public void TestTransports()
		{
			var shipment = GetNewTrackingCFSShipment();

			var mainTransport = shipment.Consols[0].Transports[0];
			mainTransport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			mainTransport.JW_Vessel = "mainVessel";

			Factory.Save();
			AssertEquals("mainVessel", shipment.GetCurrentTransportForTest().JV_RV_NKVessel);
			AssertEquals("mainVessel", shipment.GetMainTransportForTest().JV_RV_NKVessel);

			var transport2 = shipment.Consols[0].Transports.AddNew();
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
			transport2.JW_ATA = ZDateTime.Now;
			transport2.JW_Vessel = "vessel2";

			Factory.Save();
			shipment.ResetTransports();

			AssertEquals("vessel2", shipment.GetCurrentTransportForTest().JV_RV_NKVessel);
			AssertEquals("mainVessel", shipment.GetMainTransportForTest().JV_RV_NKVessel);

			mainTransport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			Transport transport3 = shipment.Consols[0].Transports.AddNew();
			transport3.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport3.JW_ATD = ZDateTime.Now;
			transport3.JW_Vessel = "vessel3";

			Factory.Save();
			shipment.ResetTransports();

			AssertEquals("vessel3", shipment.GetCurrentTransportForTest().JV_RV_NKVessel);
			AssertNull(shipment.GetMainTransportForTest());
		}

		#endregion

		#region TestDocsAndCartageProperties

		public void TestDocsAndCartageProperties()
		{
			var shipment = GetNewTrackingCFSShipment();
			AssertNotNull("DocsAndCartage", shipment.DocsAndCartage);

			var testDate = new ZDateTime(2009, 01, 19);

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

		#region TestMasterBill

		public void TestMasterBill()
		{
			var shipment = GetNewTrackingCFSShipment();
			var consol = shipment.Consols[0];
			consol.JK_MasterBillNum = "M001";

			Factory.Save();

			AssertEquals("M001", shipment.MasterBill);
		}

		#endregion

		#region TestPacksWithUnits

		public void TestPacksWithUnits()
		{
			var shipment = Factory.New<TrackingCFSShipment>();
			shipment.JS_OuterPacks = 5;
			shipment.JS_F3_NKPackType = "PLT";
			AssertEquals("5 PLT", shipment.PacksWithUnits);
		}

		#endregion

		#region TestDeliveredLegProperties

		public void TestDeliveredLegProperties()
		{
			var shipment = GetNewTrackingCFSShipment();

			AssertEquals("ReceivedDate", ZDateTime.Empty, shipment.ReceivedDate);
			AssertEquals("ReceivedBy", ZString.Empty, shipment.ReceivedBy);
			AssertEquals("PiecesReceived", ZInt.Zero, shipment.PiecesReceived);

			var packline1 = shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm leg1 = shipment.DeliveryConfirms.AddNew();
			CommonPickupDeliveryConfirm leg2 = shipment.DeliveryConfirms.AddNew();

			leg1.EU_PickupDeliveryTime = new ZDateTime(2008, 03, 8);
			leg1.EU_GoodsSignForBy = "First";
			leg2.EU_PickupDeliveryTime = new ZDateTime(2008, 03, 19);
			leg2.EU_GoodsSignForBy = "Second";

			var divot = leg2.GetDivot(packline1);
			divot.J8_PackagesDelivered = 5;

			AssertEquals("ReceivedDate", new ZDateTime(2008, 03, 19), shipment.ReceivedDate);
			AssertEquals("ReceivedBy", "Second", shipment.ReceivedBy);
			AssertEquals("PiecesReceived", 5, shipment.PiecesReceived);
		}

		#endregion

		#region TestBookedOnline

		public void TestBookedOnline()
		{
			var shipment = Factory.New<TrackingCFSShipment>();

			Assert("Not BookedOnline", !shipment.BookedOnline);

			shipment.JS_SystemCreateUser = "ZZ";
			Assert("BookedOnline", shipment.BookedOnline);
		}

		#endregion

		public void TestJobDocsAndCartage()
		{
			var shipment = Factory.New<CFSShipment>();
			AssertNotNull(shipment.DocsAndCartage);
		}

		#region Test additional points

		public void TestPickupAddressAsTest()
		{
			var shipmentWithoutPickupAddress = Factory.New<TrackingCFSShipment>();
			AssertEquals("PickupAddressAsText is empty", ZString.Empty, shipmentWithoutPickupAddress.PickupAddressAsText);

			shipmentWithoutPickupAddress.DocAddresses.FindOrCreateWithRequirement(shipmentWithoutPickupAddress.ConsignorPickupDeliveryAddressRequirement).E2_OA_Address = TestOrgAddress.PK;
			AssertEquals("PickupAddressAsText", TestJobDocAddress.AddressAsASingleLine, shipmentWithoutPickupAddress.PickupAddressAsText);
			AssertNotEquals("PickupAddressAsText should not be empty now", ZString.Empty, shipmentWithoutPickupAddress.PickupAddressAsText);
		}

		public void TestDeliveryAddressAsTest()
		{
			var shipmentWithoutDeliveryAddress = Factory.New<TrackingCFSShipment>();
			AssertEquals("DeliveryAddressAsText is empty", ZString.Empty, shipmentWithoutDeliveryAddress.DeliveryAddressAsText);

			shipmentWithoutDeliveryAddress.DocAddresses.FindOrCreateWithRequirement(shipmentWithoutDeliveryAddress.ConsigneePickupDeliveryAddressRequirement).E2_OA_Address = TestOrgAddress.PK;
			AssertEquals("DeliveryAddressAsText", TestJobDocAddress.AddressAsASingleLine, shipmentWithoutDeliveryAddress.DeliveryAddressAsText);
			AssertNotEquals("DeliveryAddressAsText should not be empty now", ZString.Empty, shipmentWithoutDeliveryAddress.DeliveryAddressAsText);
		}

		#endregion

		#region DischargeETA and LoadETD

		public void TestLoadETD()
		{
			Globals.IsWeb = true;
			var testDate = new ZDateTime(2006, 7, 28);
			var shipment = (TrackingCFSShipment)GetShipmentWithAttachedConsolAndTransport("AUSYD", "USLAX", "ABC", "ORIENTAL PHOENIX", testDate,
																										 Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL);
			AssertNotNull("Shipment should have a departure consol", shipment.DepartureConsol);
			AssertEquals("Shipment LoadETD should be the ETD of the departure transport on the departure consol", testDate.ToShortDateString(),
				shipment.LoadETDWithSuppression.ToShortDateString());
		}

		#endregion

		#region Suppressed Fields

		CFSShipment GetShipmentWithAttachedConsolAndTransport(ZString loadPort, ZString destPort, ZString voyageFlight, ZString vessel, ZDateTime depDate, ZString transportMode, ZString containerMode)
		{
			var result = (CFSShipment)GetShipment();
			result.JS_E_DEP = depDate;
			result.JS_TransportMode = transportMode;
			result.JS_PackingMode = containerMode;

			var consol = result.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = transportMode;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = loadPort;
			transport.JW_ETD = depDate;

			return result;
		}

		#endregion

		#region DischargeETAWithSuppression

		public void TestDischargeETAWithSuppression()
		{
			Globals.IsWeb = true;
			AssertEquals(ZDateTime.Empty, TestShipment.DischargeETAWithSuppression);

			TestShipment.JS_RL_NKOrigin = "AUSYD";
			TestShipment.JS_RL_NKDestination = "USLAX";
			TestShipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var consol = Factory.New<CFSLoadListConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport = consol.Transports[0];
			transport.JW_VoyageFlight = "ABC";
			transport.JW_Vessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_Code;
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			TestShipment.Consols.Add(consol);

			var testDate = ZDateTime.Now.AddDays(1);
			transport.JW_ETA = testDate;

			AssertEquals(testDate.ToShortDateString(), TestShipment.DischargeETAWithSuppression.ToShortDateString());

			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			TestShipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			AssertEquals(testDate, TestShipment.DischargeETAWithSuppression);

			SuppressionForTest.CacheObjectClear();
			SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForExport, true);

			AssertEquals(Suppression.SuppressedDate, TestShipment.DischargeETAWithSuppression);
		}

		#endregion

		#region FirstTransportLoadPort

		public void TestFirstTransportLoadPort()
		{
			TestShipment.JS_RL_NKOrigin = "AUSYD";
			TestShipment.JS_RL_NKDestination = "USLAX";

			var consol = Factory.New<CFSLoadListConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUADL";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_VoyageFlight = "ABC";
			transport.JW_Vessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_Code;
			TestShipment.Consols.Add(consol);

			TestShipment.JS_PackingMode = Core.Constants.ContainerModes.AIR;

			AssertNotNull(TestShipment.DepartureConsol);
		}

		#endregion

		#region LastTransportDischargePort

		public void TestLastTransportDischargePort()
		{
			TestShipment.JS_RL_NKOrigin = "AUSYD";
			TestShipment.JS_RL_NKDestination = "USLAX";

			var consol = Factory.New<CFSLoadListConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUADL";
			consol.JK_RL_NKDischargePort = "USNYC";

			var transport = consol.Transports[0];
			transport.JW_VoyageFlight = "ABC";
			transport.JW_Vessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_Code;
			TestShipment.Consols.Add(consol);

			TestShipment.JS_PackingMode = Core.Constants.ContainerModes.AIR;
			AssertNotNull(TestShipment.ArrivalConsol);
		}

		#endregion

		#region TestDirectConsolPKAdded

		public void TestDirectConsolPKAdded()
		{
			var shipment = Factory.NewWithValidTestData<TrackingCFSShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AssertEquals(0, shipment.DocRelatedPKs.Count);
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals(1, shipment.DocRelatedPKs.Count);
			AssertEquals(consol.PK, shipment.DocRelatedPKs[0]);
		}

		#endregion

	}
}
