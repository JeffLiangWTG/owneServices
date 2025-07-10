using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NZ;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.NZ.Testing
{
	sealed class ExportPreAdviceNotificationBuilderTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			var consol = CreateConsol();
			var exportPreAdviceNotification = new ExportPreAdviceNotificationBuilder(consol, parameters).Build();

			AssertNotNull(exportPreAdviceNotification);
			AssertEquals("BookingConfirmationReference", consol.JK_BookingReference, exportPreAdviceNotification.BookingConfirmationReference);
		}

		#region Population Test Cases

		public void TestPopulateGeneral()
		{
			var consol = CreateConsol();
			var exportPreAdviceNotification = new ExportPreAdviceNotificationBuilder(consol, parameters).Build();

			AssertEquals("Carrier.CompanyName", consol.ShippingLineAddress.CompanyName, exportPreAdviceNotification.Carrier.CompanyName);
			AssertEquals("CarrierCode", "PSN003", exportPreAdviceNotification.CarrierCode);
			AssertEquals("CarrierBookingReference", consol.JK_BookingReference, exportPreAdviceNotification.CarrierBookingReference);
			AssertEquals("FreightForwardersReference", consol.JK_UniqueConsignRef, exportPreAdviceNotification.FreightForwardersReference);

			AssertEquals("Vessel.Name", "Stoomboot van Zwarte Piet", exportPreAdviceNotification.Vessel.Name);
			AssertEquals("Voyage", "CC789", exportPreAdviceNotification.Voyage);

			AssertEquals("PortOfLoad.Code", "NZTRG", exportPreAdviceNotification.PortOfLoad.Code);
			AssertEquals("PortOfDischarge.Code", "NZAKL", exportPreAdviceNotification.PortOfDischarge.Code);

			AssertEquals("LoadPortFacility", "PSN002", exportPreAdviceNotification.LoadPortFacility);
			AssertEquals("OperationalPort.Code", "BEANR", exportPreAdviceNotification.OperationalPort.Code);
		}

		public void TestPopulateGeneral_Shipper_DirectConsol()
		{
			var consol = CreateConsol();
			var exportPreAdviceNotification = new ExportPreAdviceNotificationBuilder(consol, parameters).Build();

			AssertEquals(true, consol.IsDirect);
			AssertEquals("Shipper.CompanyName", "Bangladesh Consignor", exportPreAdviceNotification.Shipper.CompanyName);
		}

		public void TestPopulateGeneral_Shipper_NonDirectConsol()
		{
			var consol = CreateConsol(false);
			var exportPreAdviceNotification = new ExportPreAdviceNotificationBuilder(consol, parameters).Build();

			AssertEquals(false, consol.IsDirect);
			AssertEquals("Shipper.CompanyName", "Sending Forwarder", exportPreAdviceNotification.Shipper.CompanyName);
		}

		public void TestPopulateGeneral_ShipperCode_DirectConsol_WithPSN()
		{
			var consol = CreateConsol();
			var exportPreAdviceNotification = new ExportPreAdviceNotificationBuilder(consol, parameters).Build();

			AssertEquals(true, consol.IsDirect);
			AssertEquals("ShipperCode", "PSN000", exportPreAdviceNotification.ShipperCode);
		}

		public void TestPopulateGeneral_ShipperCode_DirectConsol_WithoutPSN()
		{
			var consol = CreateConsol(true, false);
			var exportPreAdviceNotification = new ExportPreAdviceNotificationBuilder(consol, parameters).Build();

			AssertEquals(true, consol.IsDirect);
			AssertEquals("ShipperCode", "Bangladesh Consignor", exportPreAdviceNotification.ShipperCode);
			AssertEquals("CarrierCode", ZString.Empty, exportPreAdviceNotification.CarrierCode);
		}

		public void TestPopulateGeneral_ShipperCode_NonDirectConsol_WithPSN()
		{
			var consol = CreateConsol(false);
			var exportPreAdviceNotification = new ExportPreAdviceNotificationBuilder(consol, parameters).Build();

			AssertEquals(false, consol.IsDirect);
			AssertEquals("ShipperCode", "PSN001", exportPreAdviceNotification.ShipperCode);
		}

		public void TestPopulateGeneral_ShipperCode_NonDirectConsol_WithoutPSN()
		{
			var consol = CreateConsol(false, false);
			var exportPreAdviceNotification = new ExportPreAdviceNotificationBuilder(consol, parameters).Build();

			AssertEquals(false, consol.IsDirect);
			AssertEquals("ShipperCode", "Sending Forwarder", exportPreAdviceNotification.ShipperCode);
		}

		public void TestPopulateGeneral_PreCarriageMode_PreLegIsRoad()
		{
			var consol = CreateConsol(false, false, Constants.TransportModes.Road);
			var exportPreAdviceNotification = new ExportPreAdviceNotificationBuilder(consol, parameters).Build();

			AssertEquals(Constants.TransportModes.Road, exportPreAdviceNotification.PreCarriageMode.Code);
		}

		public void TestPopulateGeneral_PreCarriageMode_PreLegIsRail()
		{
			var consol = CreateConsol(false, false, Constants.TransportModes.Rail);
			var exportPreAdviceNotification = new ExportPreAdviceNotificationBuilder(consol, parameters).Build();

			AssertEquals(Constants.TransportModes.Rail, exportPreAdviceNotification.PreCarriageMode.Code);
		}

		public void TestPopulateGeneral_PreCarriageMode_PreLegIsOtherType()
		{
			var consol = CreateConsol(false, false, Constants.TransportModes.Courier);
			var exportPreAdviceNotification = new ExportPreAdviceNotificationBuilder(consol, parameters).Build();

			AssertEquals(ZString.Empty, exportPreAdviceNotification.PreCarriageMode.Code);
		}

		public void TestPopulateGeneral_PreCarriageMode_PreLegNotExist()
		{
			var consol = CreateConsol();
			var exportPreAdviceNotification = new ExportPreAdviceNotificationBuilder(consol, parameters).Build();

			AssertEquals(ZString.Empty, exportPreAdviceNotification.PreCarriageMode.Code);
		}

		public void TestPopulateGeneral_Origin_SingleShipment()
		{
			var consol = CreateConsol();
			var exportPreAdviceNotification = new ExportPreAdviceNotificationBuilder(consol, parameters).Build();

			AssertEquals("Origin.Code", "AUSYD", exportPreAdviceNotification.Origin.Code);
		}

		public void TestPopulateGeneral_Origin_MultipleShipmentsWithSameOrigin()
		{
			var consol = CreateConsol(false);
			CreatePackingLinesAndContainers(consol);
			var exportPreAdviceNotification = new ExportPreAdviceNotificationBuilder(consol, parameters).Build();

			AssertEquals("Origin.Code", "NZTRG", exportPreAdviceNotification.Origin.Code);
		}

		public void TestPopulateGeneral_Origin_MultipleShipmentsWithDifferentOrigin()
		{
			var consol = CreateConsol(false);
			CreatePackingLinesAndContainers(consol, false);
			var exportPreAdviceNotification = new ExportPreAdviceNotificationBuilder(consol, parameters).Build();

			AssertEquals("Origin.Code", "NZTIU", exportPreAdviceNotification.Origin.Code);
		}

		public void TestPopulateContainers()
		{
			var consol = CreateConsol(false);
			var list = CreatePackingLinesAndContainers(consol);
			var parameter = new DocDataObjectParameters("Test", "Test", list);
			var exportPreAdviceNotification = new ExportPreAdviceNotificationBuilder(consol, parameter).Build();

			AssertEquals("Containers.Count", list.Count, exportPreAdviceNotification.Containers.Count);
		}

		public void TestPopulateContainers_HSCode_EmptyPackLines()
		{
			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			var parameter = new DocDataObjectParameters("Test", "Test", new List<ForwardingContainer> { container });
			var notification = new ExportPreAdviceNotificationBuilder(consol, parameter).Build();

			var actualContainer = notification.Containers.FirstOrDefault();
			AssertNotNull(actualContainer);
			AssertEquals(ZString.Empty, actualContainer.HSCode);
		}

		public void TestPopulateContainers_HSCode_1PackLineWithEmptyHSCode()
		{
			var consol = CreateConsol();
			var shipment = consol.Shipments.AddNew();
			var packingLine = shipment.OuterPackLines.AddNew();
			var container = consol.Containers.AddNew();
			packingLine.SetContainer(consol, container);
			var parameter = new DocDataObjectParameters("Test", "Test", new List<ForwardingContainer> { container });

			var notification = new ExportPreAdviceNotificationBuilder(consol, parameter).Build();

			var actualContainer = notification.Containers.FirstOrDefault();
			AssertNotNull(actualContainer);
			AssertEquals(ZString.Empty, actualContainer.HSCode);
		}

		public void TestPopulateContainers_HSCode_2PackLinesWithDifferentWeightUnit()
		{
			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_ActualWeight = 5;
			packLine1.JL_ActualWeightUQ = "KG";
			packLine1.JL_HarmonisedCode = "Test1";
			packLine1.SetContainer(consol, container);
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_ActualWeight = 10;
			packLine2.JL_ActualWeightUQ = "LB";
			packLine2.JL_HarmonisedCode = "Test2";
			packLine2.SetContainer(consol, container);
			var parameter = new DocDataObjectParameters("Test", "Test", new List<ForwardingContainer> { container });

			var notification = new ExportPreAdviceNotificationBuilder(consol, parameter).Build();

			var actualContainer = notification.Containers.FirstOrDefault();
			AssertEquals(packLine1.JL_HarmonisedCode, actualContainer.HSCode);
			AssertNotEquals(packLine2.JL_HarmonisedCode, actualContainer.HSCode);
		}

		public void TestPopulateContainers_HSCode_MultiplePackLinesWithoutHSCode()
		{
			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_ActualWeight = 1;
			packLine1.JL_ActualWeightUQ = "KG";
			packLine1.SetContainer(consol, container);
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_ActualWeight = 5;
			packLine2.JL_ActualWeightUQ = "KG";
			packLine2.JL_HarmonisedCode = "Test2";
			packLine2.SetContainer(consol, container);
			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_ActualWeight = 6;
			packLine3.JL_ActualWeightUQ = "KG";
			packLine3.JL_HarmonisedCode = "Test3";
			packLine3.SetContainer(consol, container);
			var packLine4 = shipment.OuterPackLines.AddNew();
			packLine4.JL_ActualWeight = 10;
			packLine4.JL_ActualWeightUQ = "KG";
			packLine4.SetContainer(consol, container);
			var parameter = new DocDataObjectParameters("Test", "Test", new List<ForwardingContainer> { container });

			var notification = new ExportPreAdviceNotificationBuilder(consol, parameter).Build();

			var actualContainer = notification.Containers.FirstOrDefault();
			AssertEquals(packLine3.JL_HarmonisedCode, actualContainer.HSCode);
			AssertNotEquals(packLine4.JL_HarmonisedCode, actualContainer.HSCode);
		}

		public void TestPopulateContainers_HSCode_MultiplePackLinesWithoutWeight()
		{
			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container);
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = "Test2";
			packLine2.SetContainer(consol, container);
			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_HarmonisedCode = "Test3";
			packLine3.SetContainer(consol, container);
			var parameter = new DocDataObjectParameters("Test", "Test", new List<ForwardingContainer> { container });

			var notification = new ExportPreAdviceNotificationBuilder(consol, parameter).Build();

			var actualContainer = notification.Containers.FirstOrDefault();
			AssertEquals(packLine2.JL_HarmonisedCode, actualContainer.HSCode);
			AssertNotEquals(packLine3.JL_HarmonisedCode, actualContainer.HSCode);
		}

		#endregion

		#region Validation Test Cases

		public void TestValidateGeneral()
		{
			var consol = Factory.New<ForwardingConsol>();
			var notification = new ExportPreAdviceNotificationBuilder(consol, parameters).Build();

			AssertHasMessageError(notification.Shipper.CompanyNameInfo, "Sending party name and address information is required.");
			AssertHasMessageError(notification.ShipperCodeInfo, "Sending Party Code is required.");
			AssertHasMessageError(notification.Carrier.CompanyNameInfo, "Carrier party name and address information is required.");
			AssertHasMessageError(notification.CarrierCodeInfo, "Carrier Code is required. Please provide in Organization > Config > Registration Numbers / Codes - type 'PSN', Country 'NZ'.");
			AssertHasMessageError(notification.CarrierBookingReferenceInfo, "Carrier Booking Reference is required.");
			AssertHasMessageError(notification.FreightForwardersReferenceInfo, "Freight Forwarders Reference is required.");
			AssertHasMessageError(notification.Vessel.NameInfo, "Vessel Name is required.");
			AssertHasMessageError(notification.VoyageInfo, "Voyage is required.");
			AssertHasMessageError(notification.PortOfLoad.CodeInfo, "Port Of Load Code is required.");
			AssertHasMessageError(notification.PortOfDischarge.CodeInfo, "Port Of Discharge Code is required.");
			AssertHasMessageError(notification.Origin.CodeInfo, "Origin Code is required.");
			AssertHasMessageError(notification.OperationalPort.CodeInfo, "Operational Port Code is required.");
			AssertHasMessageError(notification.LoadPortFacilityInfo, "Load Port Facility is required.");
		}

		public void TestValidateContainers()
		{
			var consol = Factory.New<ForwardingConsol>();
			var departureCTOAddress = Factory.New<OrgHeader>();
			departureCTOAddress.OH_RL_NKClosestPort = "NZLYT";
			consol.JK_OA_DepartureCTOAddress = departureCTOAddress.MainAddress.PK;
			var parameter = new DocDataObjectParameters("Test", "Test", new List<ForwardingContainer> { consol.Containers.AddNew() });
			var notification = new ExportPreAdviceNotificationBuilder(consol, parameter).Build();

			foreach (var container in notification.Containers)
			{
				AssertHasMessageError(container.NumberInfo, "Container number is required.");
				AssertHasMessageError(container.Type.ISOCodeInfo, "ISO code is required.");
				AssertHasMessageError(container.GoodsWeight.ValueInfo, "Net Weight is required.");
				AssertHasMessageError(((Measurement)container.GrossWeight).ValueInfo, "Gross Weight is required.");
				AssertHasMessageError(container.VerifiedDateInfo, "Verified Date is required.");
				AssertHasMessageError(container.VerifiedByAddress.CompanyNameInfo, "Verified By is required.");
				AssertHasMessageError(container.SealInfo, "Seal is required.");
				AssertHasMessageError(((CodeDescription)container.SealPartyType).CodeInfo, "Seal Party Code is required.");
			}

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_ISOType = "22P1";

			var refContainer2 = Factory.New<RefContainer>();
			refContainer2.RC_ISOType = "22p1";

			var refContainer3 = Factory.New<RefContainer>();
			refContainer3.RC_ISOType = "22p";

			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			container1.JC_RC = refContainer.PK;
			container2.JC_RC = refContainer2.PK;

			parameter = new DocDataObjectParameters("Test", "Test", new List<ForwardingContainer> { container1, container2 });
			notification = new ExportPreAdviceNotificationBuilder(consol, parameter).Build();

			foreach (var container in notification.Containers)
			{
				AssertEquals(string.Empty, container.Seal);
				AssertNoMessageError(container.SealInfo, "Seal is required.");

				AssertEquals(string.Empty, container.SealPartyType.Code);
				AssertNoMessageError(((CodeDescription)container.SealPartyType).CodeInfo, "Seal Party Code is required.");
			}

			container1.JC_RC = refContainer3.PK;

			parameter = new DocDataObjectParameters("Test", "Test", new List<ForwardingContainer> { container1 });
			notification = new ExportPreAdviceNotificationBuilder(consol, parameter).Build();

			foreach (var container in notification.Containers)
			{
				AssertEquals(string.Empty, container.Seal);
				AssertHasMessageError(container.SealInfo, "Seal is required.");

				AssertEquals(string.Empty, container.SealPartyType.Code);
				AssertHasMessageError(((CodeDescription)container.SealPartyType).CodeInfo, "Seal Party Code is required.");
			}
		}

		public void TestValidateHSCode()
		{
			var consol = Factory.New<ForwardingConsol>();

			var shipment1 = consol.Shipments.AddNew();
			var packingLine1 = shipment1.OuterPackLines.AddNew();
			packingLine1.JL_HarmonisedCode = "HC001";
			var shipment2 = consol.Shipments.AddNew();
			var packingLine2 = shipment2.OuterPackLines.AddNew();
			packingLine2.JL_HarmonisedCode = "HC002";
			var container = consol.Containers.AddNew();
			packingLine1.SetContainer(consol, container);
			packingLine2.SetContainer(consol, container);

			var parameter = new DocDataObjectParameters("Test", "Test", new List<ForwardingContainer> { container });
			var notification = new ExportPreAdviceNotificationBuilder(consol, parameter).Build();
			foreach (var item in notification.Containers)
			{
				AssertHasWarning(item.HSCodeInfo, "Multiple HS Codes have been found on different Pack Lines in this Container, please validate if the correct one has been selected");
			}
		}

		#endregion

		#region Implement

		readonly DummyDocDataObjectParameters parameters = new DummyDocDataObjectParameters
		{
			DataStoreName = "Export Pre-Advice Notification"
		};

		ForwardingConsol CreateConsol(bool isDirect = true, bool withPSN = true, string preLegMode = Constants.TransportModes.Sea)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00002000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "NZTIU";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL_Reference";

			if (isDirect)
			{
				consol.JK_AgentType = Constants.AgentType.Direct;
			}
			else
			{
				consol.JK_AgentType = Constants.AgentType.Agent;
			}

			CreateAddresses(consol, isDirect, withPSN);
			CreateTransports(consol, preLegMode);

			return consol;
		}

		void CreateAddresses(ForwardingConsol consol, bool isDirect, bool withPSN)
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "9001";
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "BE";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			if (withPSN)
			{
				carrier.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PortSystemNumber, "PSN003", Constants.CountryCodes.NewZealand);
				carrier.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PortSystemNumber, "PSN100", Constants.CountryCodes.NewCaledonia);
			}

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			if (isDirect)
			{
				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

				var consignor = Factory.New<OrgHeader>();
				consignor.OH_FullName = "Bangladesh Consignor";
				consignor.OH_RL_NKClosestPort = "AUSYD";
				consignor.MainAddress.Address1 = "UNIT05";
				consignor.MainAddress.Address2 = "Haha Street";
				consignor.MainAddress.City = "AUCKLAND";
				consignor.MainAddress.Postcode = "1050";
				consignor.MainAddress.OA_RN_NKCountryCode = "NZ";
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				if (withPSN)
				{
					consignor.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PortSystemNumber, "PSN000", Constants.CountryCodes.NewZealand);
				}
			}
			else
			{
				var sendingForwarder = Factory.New<OrgHeader>();
				sendingForwarder.OH_FullName = "Sending Forwarder";
				sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
				sendingForwarder.MainAddress.Address1 = "Unit 13";
				sendingForwarder.MainAddress.Address2 = "4 Lost Lane";
				sendingForwarder.MainAddress.City = "Antwerp";
				sendingForwarder.MainAddress.Postcode = "2000";
				sendingForwarder.MainAddress.OA_RN_NKCountryCode = "BE";
				if (withPSN)
				{
					sendingForwarder.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PortSystemNumber, "PSN001", Constants.CountryCodes.NewZealand);
				}

				consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			}

			var departureCTOAddress = Factory.New<OrgHeader>();
			departureCTOAddress.OH_FullName = "I'm Handling the Stuff to be send";
			departureCTOAddress.OH_RL_NKClosestPort = "BEANR";
			departureCTOAddress.MainAddress.Address1 = "Unit 200";
			departureCTOAddress.MainAddress.Address2 = "55 Why Lane";
			departureCTOAddress.MainAddress.City = "Antwerp";
			departureCTOAddress.MainAddress.Postcode = "2000";
			departureCTOAddress.MainAddress.OA_RN_NKCountryCode = "BE";
			departureCTOAddress.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PortSystemNumber, "PSN002", Constants.CountryCodes.NewZealand);

			consol.JK_OA_DepartureCTOAddress = departureCTOAddress.MainAddress.PK;
		}

		void CreateTransports(ForwardingConsol consol, ZString transportModeToTerminal)
		{
			var preTransport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			preTransport.JW_LegOrder = 1;
			preTransport.JW_TransportMode = transportModeToTerminal;
			preTransport.JW_TransportType = Constants.TransportPlanningType.PreCarriage;
			preTransport.JW_RL_NKLoadPort = "NZTRG";
			preTransport.JW_RL_NKDiscPort = "NZAKL";
			preTransport.JW_ETD = new ZDateTime(2024, 10, 1);
			preTransport.JW_VoyageFlight = "CC789";

			if (transportModeToTerminal == Constants.TransportModes.Sea)
			{
				var preVessel = Factory.New<RefVessel>();
				preVessel.RV_Code = "Stoomboot van Zwarte Piet";

				preVessel.RV_VesselType = Constants.VesselType.Barge;
				preVessel.RV_LloydsNumber = "LYDS456";
				preVessel.RV_RadioCallSign = "Radio456";
				preTransport.JW_Vessel = preVessel.RV_Code;
			}

			var transport = consol.Transports.AddNew();
			transport.JW_LegOrder = 2;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "NZAKL";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_Vessel = "MSC Antwerp";
			transport.JW_VoyageFlight = "V111";
			transport.JW_ETD = new ZDateTime(2024, 10, 15);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Stoomboot van Sinterklaas";
			vessel.RV_VesselType = Constants.VesselType.CargoVessel;
			vessel.RV_LloydsNumber = "LYDS123";
			vessel.RV_RadioCallSign = "Radio123";
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_VoyageFlight = "CC456";
		}

		IReadOnlyCollection<ForwardingContainer> CreatePackingLinesAndContainers(ForwardingConsol consol, bool sameOrigin = true)
		{
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SH0001000";
			shipment1.JS_RL_NKOrigin = "NZTRG";

			var packingLine1 = shipment1.OuterPackLines.AddNew();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "TBNU1111111";
			packingLine1.SetContainer(consol, container1);

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "SH0002000";
			if (sameOrigin)
			{
				shipment2.JS_RL_NKOrigin = "NZTRG";
			}
			else
			{
				shipment2.JS_RL_NKOrigin = "NZAKL";
			}

			var packingLine2 = shipment2.OuterPackLines.AddNew();
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "TBNU2222222";
			packingLine2.SetContainer(consol, container2);

			return new List<ForwardingContainer> { container1, container2 };
		}
		#endregion
	}
}
