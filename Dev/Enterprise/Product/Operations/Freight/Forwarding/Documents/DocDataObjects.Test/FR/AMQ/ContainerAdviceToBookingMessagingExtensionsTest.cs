using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Constants = Enterprise.Core.Constants;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;
using Transport = Enterprise.Freight.Business.Transport;

namespace Enterprise.Freight.Forwarding.Documents.FR.Testing
{
	sealed class ContainerAdviceToBookingMessagingExtensionsTest : TestCaseWithFactory
	{
		public void TestContinueWithSendingMessage_DOSNotAccepted()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerAdviceToBookingBuilder(consol, parameters);
			var data = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			dynamicData.SetupGet(d => d.Value).Returns(data);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ);

			var extensions = new ContainerAdviceToBookingMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("AMQ cannot be sent because DOS is not sent yet.", false, res);
			notifications.Verify(x => x.ShowMessage("The Container Advice to Booking (AMQ) message cannot be submitted until the \"File Creation Request\" (DOS) message has been accepted by the Terminal.", "Warning"), Times.Once);
		}

		public void TestContinueWithSendingMessage_DOSAccepted()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerAdviceToBookingBuilder(consol, parameters);
			var data = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(consol,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.DOSExport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(data);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ);

			var extensions = new ContainerAdviceToBookingMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("AMQ cannot be sent because DOS is not sent yet.", null, res);
			notifications.Verify(x => x.ShowMessage("The Container Advice to Booking (AMQ) message cannot be submitted until the \"File Creation Request\" (DOS) message has been accepted by the Terminal.", "Warning"), Times.Never);
		}

		public void TestContinueWithSendingMessageAmendment_NotAccepted()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerAdviceToBookingBuilder(consol, parameters);
			var data = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(data);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ);

			var extensions = new ContainerAdviceToBookingMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertEquals("Message can be amended only when MAA is received", false, res);
			notifications.Verify(x => x.ShowMessage("Message can be amended only after you have received an accepted response to the previous message.", "Warning"), Times.Once);
		}

		public void TestContinueWithSendingMessageAmendment_Accepted()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerAdviceToBookingBuilder(consol, parameters);
			var data = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(data);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ);

			var extensions = new ContainerAdviceToBookingMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertEquals("Message can be amended as MAA is received.", null, res);
			notifications.Verify(x => x.ShowMessage("Message can be amended only after you have received an accepted response to the previous message.", "Warning"), Times.Never);
		}

		public void TestContinueWithSendingMessageAmendment_HasBeenRecepted()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerAdviceToBookingBuilder(consol, parameters);
			var data = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 21),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.MessageRejected,
				new ZDateTime(2021, 05, 22),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(data);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ);

			var extensions = new ContainerAdviceToBookingMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertEquals("Message can be amended as MAA is received.", null, res);
			notifications.Verify(x => x.ShowMessage("Message can be amended only after you have received an accepted response to the previous message.", "Warning"), Times.Never);
		}

		public void TestContinueWithSendingMessageWithdrawal_NotAccepted()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerAdviceToBookingBuilder(consol, parameters);
			var data = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(data);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ);

			var extensions = new ContainerAdviceToBookingMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertEquals("Message can be withdrawn only when MAA is received", false, res);
			notifications.Verify(x => x.ShowMessage("Message can be withdrawn only after you have received an accepted response to the previous message.", "Sending Withdraw/Cancel Request"), Times.Once);
		}

		public void TestContinueWithSendingMessageWithdrawal_Accepted()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerAdviceToBookingBuilder(consol, parameters);
			var data = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(data);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ);

			var extensions = new ContainerAdviceToBookingMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertEquals("Message can be amended as MAA is received.", null, res);
			notifications.Verify(x => x.ShowMessage("Message can be withdrawn only after you have received an accepted response to the previous message.", "Sending Withdraw/Cancel Request"), Times.Never);
		}

		public void TestContinueWithResetToOriginal_NotAccepted()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerAdviceToBookingBuilder(consol, parameters);
			var data = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(data);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ);

			var extensions = new ContainerAdviceToBookingMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithResetToOriginal(notifications.Object);

			AssertEquals("Message can be reset as no response message(MAA) received", null, res);
			notifications.Verify(x => x.ShowMessage("You cannot change the status to \"Reset to Original\" as the original message has already been accepted.", "Information"), Times.Never);
		}

		public void TestContinueWithResetToOriginal_Accepted()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerAdviceToBookingBuilder(consol, parameters);
			var data = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(data);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ);

			var extensions = new ContainerAdviceToBookingMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithResetToOriginal(notifications.Object);

			AssertEquals("Message can not reset when MAA event received.", false, res);
			notifications.Verify(x => x.ShowMessage("You cannot change the status to \"Reset to Original\" as the original message has already been accepted.", "Information"), Times.Once);
		}

		#region implementation

		IVisualizerDocumentData CreateDocumentData(ForwardingConsol consol)
		{
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobConsolSchema.Constants.Prefix;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_Name = "FR_AMQ";

			return documentData;
		}

		void CreateLog(IStmALogProvider logParent, Event @event, ZDateTime time, params KeyValuePair<string, string>[] parameters)
		{
			logParent.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, time.ToOffset(), string.Empty, parameters);
			Thread.Sleep(1);
			Factory.Save();
		}

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "FRPRA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_BookingReference = "B0001100";

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
			transport.JW_RL_NKLoadPort = "FRPRA";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "Dragon";
			transport.JW_VoyageFlight = "111";
			transport.JW_ETD = new ZDateTime(2018, 12, 1);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Random Vesel";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "111";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "FRPRA";
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_E_ARV = new ZDateTime(2018, 12, 1);
			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			sailing.Origin.JA_DepartReference = "AOEUI1234";
			sailing.Destination.JB_ArrivalReference = "QWERTY123";
			transport.JW_JX = sailing.PK;

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
			transport3.JW_RL_NKDiscPort = "AUSYD";
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

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var bookingAgent = Factory.New<OrgHeader>();
			bookingAgent.OH_FullName = "I'm Handling Stuff";
			bookingAgent.OH_RL_NKClosestPort = "AUSYD";
			bookingAgent.MainAddress.Address1 = "Unit 2";
			bookingAgent.MainAddress.Address2 = "60 What Lane";
			bookingAgent.MainAddress.City = "Sydney";
			bookingAgent.MainAddress.Postcode = "2023";
			bookingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = bookingAgent.MainAddress.PK;

			var departureCTOAddress = Factory.New<OrgHeader>();
			departureCTOAddress.OH_FullName = "I'm Sending Stuff";
			departureCTOAddress.OH_RL_NKClosestPort = "CNNJI";
			departureCTOAddress.MainAddress.Address1 = "Unit 200";
			departureCTOAddress.MainAddress.Address2 = "55 Why Lane";
			departureCTOAddress.MainAddress.City = "Conficious Ave";
			departureCTOAddress.MainAddress.Postcode = "10000";
			departureCTOAddress.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_DepartureCTOAddress = departureCTOAddress.MainAddress.PK;

			var departurePackCFSTransportAddress = Factory.New<OrgHeader>();
			departurePackCFSTransportAddress.OH_FullName = "I'm Receiving Stuff";
			departurePackCFSTransportAddress.OH_RL_NKClosestPort = "AUSYD";
			departurePackCFSTransportAddress.MainAddress.Address1 = "Unit 399";
			departurePackCFSTransportAddress.MainAddress.Address2 = "50 What Lane";
			departurePackCFSTransportAddress.MainAddress.City = "Sydney";
			departurePackCFSTransportAddress.MainAddress.Postcode = "5023";
			departurePackCFSTransportAddress.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_DeparturePackCFSTransportAddress = departurePackCFSTransportAddress.MainAddress.PK;
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
			container.JC_ExportDepotCustomsReference = "ExportReference";

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

			var packingLine = shipment.OuterPackLines.AddNew();
			packingLine.JL_PackageCount = 2;
			packingLine.JL_F3_NKPackType = "PLT";
			packingLine.JL_ActualWeight = 200;
			packingLine.JL_ActualWeightUQ = "KG";
			packingLine.JL_ActualVolume = 300;
			packingLine.JL_ActualVolumeUQ = "M3";
			packingLine.JL_HarmonisedCode = "ABCDE";
			packingLine.JL_ExportRefNumber = "REF001";
			packingLine.JL_DetailedDescription = "pack1";
			packingLine.JL_ContainerPackingOrder = 1;

			packingLine.JL_RequiresTemperatureControl = true;
			packingLine.JL_RequiredTemperatureUnit = "C";
			packingLine.JL_RequiresTemperatureControl = true;
			packingLine.JL_RequiredTemperatureMinimum = 10;
			packingLine.JL_RequiredTemperatureMaximum = 30;

			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "6666", "E", "IMO").FirstOrDefault();
			if (subs == null)
			{
				subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "6666";
				subs.DG_Variant = "E";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}

			var undg = packingLine.UNDGs.AddNew();
			undg.LinkDefault(subs);
			undg.Substance.DG_PSN = "DG SHIPPER NAME";
			undg.Substance.DG_State = "L";
			undg.DI_TechnicalName = "WHATEVER";
			undg.DI_IMOClass = "CLAS";
			undg.Substance.DG_PG = "GRO";
			undg.Substance.DG_SubLabel1 = "sub1";
			undg.Substance.DG_SubLabel2 = "su2";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 15.0m;
			undg.DI_MPMarinePollutant = "N";
			undg.DI_DGVolume = 2m;
			undg.DI_UnitOfVolume = "M3";
			undg.DI_DGWeight = 200m;
			undg.DI_UnitOfWeight = "KG";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_PackageCount = 5;
			undg.DI_F3_NKPackType = "BAG";

			container.PackLines.Add(packingLine);
		}

		#endregion
	}
}
