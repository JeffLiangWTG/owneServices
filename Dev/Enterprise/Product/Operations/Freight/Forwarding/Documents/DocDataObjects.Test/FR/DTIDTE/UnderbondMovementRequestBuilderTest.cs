using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;
using ConsolDocumentDataStoreNames = Enterprise.Freight.Forwarding.Documents.DocDataObjects.ConsolDocumentDataStoreNames;
using Transport = Enterprise.Freight.Business.Transport;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	public class UnderbondMovementRequestBuilderTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DTI
			};

			var builder = new UnderbondMovementRequestBuilder(consol, parameters);
			var request = builder.Build();

			CombineAssertions(() =>
			{
				AssertEquals("ConsolNumber", "C00001000", request.ConsolNumber);
				AssertEquals("CarrierBookingReference", "B0001100", request.CarrierBookingReference);
				AssertEquals("ContainerMode", "FCL", request.ContainerMode.Code);
				AssertEquals("ShipmentType", "AGT", request.ShipmentType.Code);
				AssertEquals("PortOfOrigin", "AUSYD", request.PortOfOrigin.Code);
				AssertEquals("PortOfTranshipment", "NZAKL", request.PortOfTranshipment.Code);
				AssertEquals("PortOfDestination", "FRBOD", request.PortOfDestination.Code);
				AssertEquals("OperationalPort", "FRBOD", request.OperationalPort.Code);
				AssertEquals("VesselName", "Miranda", request.VesselName);
				AssertEquals("VoyageFlightNo", "333", request.VoyageFlightNo);
				AssertEquals("BillOfLading", "WBN666", request.BillOfLading);
				AssertEquals("Containers.Count", 1, request.Containers.Count);
			});

			AssertAddressData(consol.ShippingLineAddress, request.Carrier);
			AssertAddressData(consol.SendingForwarderAddress, request.SendingForwarder);
			AssertAddressData(consol.ReceivingForwarderAddress, request.ReceivingForwarder);
			AssertAddressData(consol.ArrivalUnpackCFSTransportAddress, request.Transporter);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, request.SendingParty);

			AssertEquals("CARCI5", request.CarrierCI5.Value);
			AssertEquals("CARSON", request.CarrierSON.Value);
			AssertEquals("CARCCC", request.CarrierCCC.Value);
			AssertEquals("SEDCI5", request.SendingForwarderCI5.Value);
			AssertEquals("SEDSON", request.SendingForwarderSON.Value);
			AssertEquals("REVCI5", request.ReceivingForwarderCI5.Value);
			AssertEquals("REVSON", request.ReceivingForwarderSON.Value);
			AssertEquals("DEPCI5", request.TransporterCI5.Value);
			AssertEquals("DEPSON", request.TransporterSON.Value);
			AssertEquals("GLBCI5", request.SendingPartyCI5.Value);
			AssertEquals("GLBSON", request.SendingPartySON.Value);

			var container = request.Containers.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("Number", "AAAA0000007", container.Number);
				AssertEquals("Type", "20GP", container.ContainerType.Code);
				AssertEquals("Mode", "FCL", container.ContainerMode.Code);
				AssertEquals("IsEmptyContainer", true, container.IsEmptyContainer);
				AssertEquals("IsNonOperativeReefer", false, container.IsNonOperativeReefer);
			});
		}

		public void TestValidation()
		{
			var portLocationErrorMessage = "Port Location within Area is required.";
			var portAreaErrorMessage = "Port Area is required.";
			var consol = CreateConsol();
			foreach (ForwardingContainer container in consol.Containers)
			{
				container.JC_ContainerNum = ZString.Empty;
			}

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DTI
			};

			var builder = new UnderbondMovementRequestBuilder(consol, parameters);
			var request = builder.Build();

			request.ReasonNote = "A computer program is a collection of instructions that can be executed by a computer to perform a specific task. A computer program is usually written by a computer programmer in a programming language. ";

			request.ValidateAllIncludingChildren();

			AssertHasMessageError(((CodeDescription)request.PortLocationTo).CodeInfo, portLocationErrorMessage);
			AssertHasMessageError(((CodeDescription)request.PortAreaTo).CodeInfo, portAreaErrorMessage);
			AssertHasMessageError(((CodeDescription)request.PortLocationFrom).CodeInfo, portLocationErrorMessage);
			AssertHasMessageError(((CodeDescription)request.PortAreaFrom).CodeInfo, portAreaErrorMessage);
			AssertHasMessageError(request.ReasonNoteInfo, "A maximum of 35 characters may be sent.");

			foreach (var container in request.Containers)
			{
				AssertHasMessageError(container.NumberInfo, "Container Number is required.");
			}
		}

		public void TestPortLocationAndAreaValidation()
		{
			var portLocationErrorMessage = "Port Location within Area is required.";
			var portAreaErrorMessage = "Port Area is required.";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "FRPRA";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DTI
			};

			var builder = new UnderbondMovementRequestBuilder(consol, parameters);
			var request = builder.Build();

			AssertNoMessageError(((CodeDescription)request.PortLocationTo).CodeInfo, portLocationErrorMessage);
			AssertNoMessageError(((CodeDescription)request.PortAreaTo).CodeInfo, portAreaErrorMessage);
			AssertNoMessageError(((CodeDescription)request.PortLocationFrom).CodeInfo, portLocationErrorMessage);
			AssertNoMessageError(((CodeDescription)request.PortAreaTo).CodeInfo, portAreaErrorMessage);

			request.PortOfOrigin.Code = "FRBOD";
			request.PortOfDestination.Code = "FRBOD";

			request.ValidateAllIncludingChildren();

			AssertHasMessageError(((CodeDescription)request.PortLocationTo).CodeInfo, portLocationErrorMessage);
			AssertHasMessageError(((CodeDescription)request.PortAreaTo).CodeInfo, portAreaErrorMessage);
			AssertHasMessageError(((CodeDescription)request.PortLocationFrom).CodeInfo, portLocationErrorMessage);
			AssertHasMessageError(((CodeDescription)request.PortAreaTo).CodeInfo, portAreaErrorMessage);

			request.PortLocationFrom.Code = "AELUD";
			request.PortAreaFrom.Code = "CNHSI";
			request.PortLocationTo.Code = "AELUD";
			request.PortAreaTo.Code = "CNHSI";

			AssertNoMessageError(((CodeDescription)request.PortLocationTo).CodeInfo, portLocationErrorMessage);
			AssertNoMessageError(((CodeDescription)request.PortAreaTo).CodeInfo, portAreaErrorMessage);
			AssertNoMessageError(((CodeDescription)request.PortLocationFrom).CodeInfo, portLocationErrorMessage);
			AssertNoMessageError(((CodeDescription)request.PortAreaTo).CodeInfo, portAreaErrorMessage);
		}

		public void TestDTIOperationalPortFallBack()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DTI
			};
			var builder = new UnderbondMovementRequestBuilder(consol, parameters);
			var dtiRequest = builder.Build();

			AssertEquals("OperationalPort", "FRBOD", dtiRequest.OperationalPort.Code);

			AddCTOAddresses(consol);
			var dtiRequestWithCTOAddresses = builder.Build();

			AssertEquals("OperationalPort", "AUMEL", dtiRequestWithCTOAddresses.OperationalPort.Code);
		}

		public void TestDTEOperationalPortFallBack()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DTE
			};
			var builder = new UnderbondMovementRequestBuilder(consol, parameters);
			var dtiRequest = builder.Build();

			AssertEquals("OperationalPort", "AUSYD", dtiRequest.OperationalPort.Code);

			AddCTOAddresses(consol);
			var dtiRequestWithCTOAddresses = builder.Build();

			AssertEquals("OperationalPort", "AUBRI", dtiRequestWithCTOAddresses.OperationalPort.Code);
		}

		#region TestContainerMode_ShippersConsol

		public void TestContainerMode_ShippersConsol()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.ShippersConsol;

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DTE
			};

			var data = new UnderbondMovementRequestBuilder(consol, parameters).Build();
			AssertEquals(Constants.ContainerModes.FCL, data.ContainerMode.Code);
			AssertEquals(Constants.ContainerModeDescriptions.FCL, data.ContainerMode.Description);
		}

		#endregion

		#region Implement

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "FRBOD";
			consol.JK_BookingReference = "B0001100";
			consol.JK_MasterBillNum = "WBN666";

			consol.JK_RequiresTemperatureControl = true;
			consol.JK_RequiredTemperatureUnit = "C";
			consol.JK_RequiresTemperatureControl = true;
			consol.JK_RequiredTemperatureMinimum = 15;
			consol.JK_RequiredTemperatureMaximum = 25;

			var carrierBookingRequest = consol.Notes.AddNew();
			carrierBookingRequest.ST_Description = PredefinedNoteTypes.Instance.CarrierBookingRequest.Description;
			carrierBookingRequest.ST_NoteText = "carrier booking request";

			CreateTransports(consol);
			CreatePackingLinesAndContainers(consol);
			CreateAddresses(consol);

			return consol;
		}

		void CreateTransports(ForwardingConsol consol)
		{
			var transport = consol.Transports.OfType<Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "Dragon";
			transport.JW_VoyageFlight = "111";
			transport.JW_ETD = new ZDateTime(2018, 12, 1);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportType = Constants.TransportPlanningType.Other;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_Vessel = "Steven";
			transport2.JW_VoyageFlight = "222";

			var transport3 = consol.Transports.AddNew();
			transport3.JW_LegOrder = 3;
			transport3.JW_TransportMode = Constants.TransportModes.Sea;
			transport3.JW_TransportType = Constants.TransportPlanningType.Other;
			transport3.JW_RL_NKLoadPort = "NZAKL";
			transport3.JW_RL_NKDiscPort = "FRBOD";
			transport3.JW_Vessel = "Miranda";
			transport3.JW_VoyageFlight = "333";
		}

		void CreateAddresses(ForwardingConsol consol)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";
			carrier.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "CARCI5", Core.Constants.CountryCodes.France);
			carrier.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SON, "CARSON", Core.Constants.CountryCodes.France);
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CARCCC", Core.Constants.CountryCodes.France);
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var handlingAgent = Factory.New<OrgHeader>();
			handlingAgent.OH_FullName = "I'm Handling Stuff";
			handlingAgent.OH_RL_NKClosestPort = "AUSYD";
			handlingAgent.MainAddress.Address1 = "Unit 2";
			handlingAgent.MainAddress.Address2 = "60 What Lane";
			handlingAgent.MainAddress.City = "Sydney";
			handlingAgent.MainAddress.Postcode = "2023";
			handlingAgent.MainAddress.OA_RN_NKCountryCode = "AU";
			consol.CarrierHandlingAgentDocumentaryAddress.E2_OA_Address = handlingAgent.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "I'm Sending Stuff";
			sendingForwarder.OH_RL_NKClosestPort = "CNNJI";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Conficious Ave";
			sendingForwarder.MainAddress.Postcode = "10000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";
			sendingForwarder.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "SEDCI5", Core.Constants.CountryCodes.France);
			sendingForwarder.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SON, "SEDSON", Core.Constants.CountryCodes.France);
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Receiving Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "CNNJI";
			receivingForwarder.MainAddress.Address1 = "Unit 210";
			receivingForwarder.MainAddress.Address2 = "56 Why Lane";
			receivingForwarder.MainAddress.City = "Conficious Ave";
			receivingForwarder.MainAddress.Postcode = "10001";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";
			receivingForwarder.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "REVCI5", Core.Constants.CountryCodes.France);
			receivingForwarder.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SON, "REVSON", Core.Constants.CountryCodes.France);
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var arrivalUnpackCFSTransport = Factory.New<OrgHeader>();
			arrivalUnpackCFSTransport.OH_FullName = "I'm Departure Stuff";
			arrivalUnpackCFSTransport.OH_RL_NKClosestPort = "AUSYD";
			arrivalUnpackCFSTransport.MainAddress.Address1 = "Unit 399";
			arrivalUnpackCFSTransport.MainAddress.Address2 = "50 What Lane";
			arrivalUnpackCFSTransport.MainAddress.City = "Sydney";
			arrivalUnpackCFSTransport.MainAddress.Postcode = "5023";
			arrivalUnpackCFSTransport.MainAddress.OA_RN_NKCountryCode = "AU";
			arrivalUnpackCFSTransport.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "DEPCI5", Core.Constants.CountryCodes.France);
			arrivalUnpackCFSTransport.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SON, "DEPSON", Core.Constants.CountryCodes.France);
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = arrivalUnpackCFSTransport.MainAddress.PK;

			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "GLBCI5", Core.Constants.CountryCodes.France);
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SON, "GLBSON", Core.Constants.CountryCodes.France);
		}

		void CreatePackingLinesAndContainers(ForwardingConsol consol)
		{
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CY/CY";
			container.JC_IsEmptyContainer = true;
			container.JC_ContainerCount = 1;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;
			container.JC_OverhangBack = 1.1m;
			container.JC_OverhangRight = 2.1m;
			container.JC_IsNonOperativeReefer = false;

			container.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;
			var handlingNote = container.Notes.AddNew();
			handlingNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			handlingNote.ST_NoteText = "container handling note";

			var fumigationService = container.Services.AddNew();
			fumigationService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceNote = "fumigation note";

			var contractor = Factory.New<OrgHeader>();
			contractor.OH_FullName = "CSHIPING";
			contractor.OH_RL_NKClosestPort = "FRPAR";
			contractor.MainAddress.Address1 = "Unit 18";
			contractor.MainAddress.Address2 = "5 Lost Lane";
			contractor.MainAddress.City = "PARIS";
			contractor.MainAddress.Postcode = "2000";
			contractor.MainAddress.OA_RN_NKCountryCode = "FR";
			fumigationService.ES_OH_Contractor = contractor.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.OuterPackLines.RemoveAndDeleteAll();

			if (UNDGSubstanceLoader.LoadSubstances(Factory, "6666", "E", "IMO").FirstOrDefault() == null)
			{
				var subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "6666";
				subs.DG_Variant = "E";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
		}

		void AddCTOAddresses(ForwardingConsol consol)
		{
			var arrivalCTOAddress = Factory.New<OrgHeader>();
			arrivalCTOAddress.MainAddress.OA_RL_NKRelatedPortCode = "AUMEL";
			consol.JK_OA_ArrivalCTOAddress = arrivalCTOAddress.MainAddress.PK;

			var departureCTOAddress = Factory.New<OrgHeader>();
			departureCTOAddress.MainAddress.OA_RL_NKRelatedPortCode = "AUBRI";
			consol.JK_OA_DepartureCTOAddress = departureCTOAddress.MainAddress.PK;
		}

		#endregion
	}
}
