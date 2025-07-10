using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NZ;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.NZ
{
	sealed class ExportPreAdviceNotificationExtensionsTest : TestCaseWithFactory
	{
		public void TestContinueWithSendingMessageWithdrawal_NotAccepted()
		{
			var consol = CreateConsol();
			var containers = CreatePackingLinesAndContainers(consol).ToArray();

			AddMessageAcceptedEvent(containers[0]);
			AddMessageSentEvent(containers[1]);

			var extensions = CreateExportPreAdviceNotificationExtensions(consol, containers);
			var notifications = new Mock<IUserNotifications>();
			var result = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertEquals(
				"Message can be withdrawn only after you have received an acceptance response to the previous message",
				false, result);

			notifications.Verify(
				n => n.ShowMessage(
					"Message can be withdrawn only after you have received an acceptance response to the previous message." +
					System.Environment.NewLine +
					"Message Acceptance has not been received for container(s): TBNU2222222", "Information"),
				Times.Once);
		}

		public void TestContinueWithSendingMessageWithdrawal_AllAccepted()
		{
			var consol = CreateConsol();
			var containers = CreatePackingLinesAndContainers(consol).ToArray();

			AddMessageAcceptedEvent(containers[0]);
			AddMessageAcceptedEvent(containers[1]);

			var extensions = CreateExportPreAdviceNotificationExtensions(consol, containers);
			var notifications = new Mock<IUserNotifications>();
			var result = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);
			AssertEquals("Message can be withdrawn only after you have received an acceptance response to the previous message", true, result);
		}

		public void TestIsSendingAmendment_Original()
		{
			var consol = CreateConsol();
			var containers = CreatePackingLinesAndContainers(consol).ToArray();

			AddMessageRejectedEvent(containers[0]);
			var extensions = CreateExportPreAdviceNotificationExtensions(consol, containers);
			AssertEquals("Message purpose is ORG", false, extensions.IsSendingAmendment());

			AddMessageWithdrawalAcceptedEvent(containers[1]);
			extensions = CreateExportPreAdviceNotificationExtensions(consol, containers);
			AssertEquals("Message purpose is ORG", false, extensions.IsSendingAmendment());
		}

		public void TestIsSendingAmendment_Amendment()
		{
			var consol = CreateConsol();
			var containers = CreatePackingLinesAndContainers(consol).ToArray();

			AddMessageAcceptedEvent(containers[0]);

			var extensions = CreateExportPreAdviceNotificationExtensions(consol, containers);
			AssertEquals("Message purpose is AMD", true, extensions.IsSendingAmendment());

			AddMessageWithdrawalAcceptedEvent(containers[0]);
			AddMessageSentEvent(containers[1]);
			extensions = CreateExportPreAdviceNotificationExtensions(consol, containers);
			AssertEquals("Message purpose is AMD", true, extensions.IsSendingAmendment());
		}

		public void TestGetMessageStatus()
		{
			var consol = CreateConsol();
			var containers = CreatePackingLinesAndContainers(consol).ToArray();

			var extensions = CreateExportPreAdviceNotificationExtensions(consol, containers);
			var result = extensions.GetMessageStatus();
			AssertEquals("Message status - No events created", "No Messages have been Sent", result);

			var documentData = CreateDocumentData(consol) as IStmALogProvider;

			AddMessageSentEvent(documentData);
			extensions = CreateExportPreAdviceNotificationExtensions(consol, containers);
			result = extensions.GetMessageStatus();
			AssertEquals("Message status - MSN", "Message Sent", result);

			AddInterchangeRejectedEvent(documentData);
			extensions = CreateExportPreAdviceNotificationExtensions(consol, containers);
			result = extensions.GetMessageStatus();
			AssertEquals("Message status - IRJ", "Interchange Rejected because Reason of interchange rejection", result);

			AddInterchangeSentEvent(documentData);
			extensions = CreateExportPreAdviceNotificationExtensions(consol, containers);
			result = extensions.GetMessageStatus();
			AssertEquals("Message status - ISN", "Interchange Sent", result);

			AddMessageAcceptedEvent(documentData);
			extensions = CreateExportPreAdviceNotificationExtensions(consol, containers);
			result = extensions.GetMessageStatus();
			AssertEquals("Message status - MAA", "Message Accepted", result);

			AddMessageRejectedEvent(documentData);
			extensions = CreateExportPreAdviceNotificationExtensions(consol, containers);
			result = extensions.GetMessageStatus();
			AssertEquals("Message status - MRJ", "Message Rejected", result);

			AddStatusUpdatedEvent(documentData);
			extensions = CreateExportPreAdviceNotificationExtensions(consol, containers);
			result = extensions.GetMessageStatus();
			AssertEquals("Message status - STU", "Status Updated", result);

			AddMessageWithdrawCancelRequestEvent(documentData);
			extensions = CreateExportPreAdviceNotificationExtensions(consol, containers);
			result = extensions.GetMessageStatus();
			AssertEquals("Message status - MWR", "Message Withdraw/Cancel Sent", result);

			AddMessageWithdrawalAcceptedEvent(documentData);
			extensions = CreateExportPreAdviceNotificationExtensions(consol, containers);
			result = extensions.GetMessageStatus();
			AssertEquals("Message status - MWA", "Withdrawal Accepted", result);

			var eventParameters = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, Constants.EventReferenceMessageTypes.ExportPreAdviceNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"),
			};
			CreateLog(documentData, Events.MessageReceived, new ZDateTime(2024, 10, 15), eventParameters);
			extensions = CreateExportPreAdviceNotificationExtensions(consol, containers);
			result = extensions.GetMessageStatus();
			AssertEquals("Message status - Unknown Message Status", "Unknown Message Status", result);
		}

		#region Implementation

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00002000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "NZTIU";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL_Reference";
			consol.JK_AgentType = Constants.AgentType.Agent;

			CreateAddresses(consol);
			CreateTransports(consol);

			return consol;
		}

		ExportPreAdviceNotificationExtensions CreateExportPreAdviceNotificationExtensions(ForwardingConsol consol, ICollection<ForwardingContainer> containers)
		{
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "Export Pre-Advice Notification"
			};
			parameters.Data = containers;
			var builder = new ExportPreAdviceNotificationBuilder(consol, parameters);
			var data = builder.Build();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var messageInstructions = new Mock<IMessageInstructions>();
			dynamicData.SetupGet(d => d.Value).Returns(data);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.Setup(m => m.DocumentName).Returns(ConsolDocumentDataStoreNames.ExportPreAdviceNotification);
			return new ExportPreAdviceNotificationExtensions(document.Object, consol);
		}

		IReadOnlyCollection<ForwardingContainer> CreatePackingLinesAndContainers(ForwardingConsol consol)
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
			shipment2.JS_RL_NKOrigin = "NZAKL";

			var packingLine2 = shipment2.OuterPackLines.AddNew();
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "TBNU2222222";
			packingLine2.SetContainer(consol, container2);

			return new List<ForwardingContainer> { container1, container2 };
		}

		void CreateAddresses(ForwardingConsol consol)
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
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "Sending Forwarder";
			sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			sendingForwarder.MainAddress.Address1 = "Unit 13";
			sendingForwarder.MainAddress.Address2 = "4 Lost Lane";
			sendingForwarder.MainAddress.City = "Antwerp";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "BE";
			sendingForwarder.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PortSystemNumber, "PSN001", Constants.CountryCodes.NewZealand);

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

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

		void CreateTransports(ForwardingConsol consol)
		{
			var preTransport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			preTransport.JW_LegOrder = 1;
			preTransport.JW_TransportMode = Constants.TransportModes.Sea;
			preTransport.JW_TransportType = Constants.TransportPlanningType.PreCarriage;
			preTransport.JW_RL_NKLoadPort = "NZTRG";
			preTransport.JW_RL_NKDiscPort = "NZAKL";
			preTransport.JW_ETD = new ZDateTime(2024, 10, 1);
			preTransport.JW_VoyageFlight = "CC789";

			var preVessel = Factory.New<RefVessel>();
			preVessel.RV_Code = "Stoomboot van Zwarte Piet";
			preVessel.RV_VesselType = Constants.VesselType.Barge;
			preVessel.RV_LloydsNumber = "LYDS456";
			preVessel.RV_RadioCallSign = "Radio456";
			preTransport.JW_Vessel = preVessel.RV_Code;

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

		IVisualizerDocumentData CreateDocumentData(ForwardingConsol consol)
		{
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobConsolSchema.Constants.Prefix;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_Name = ConsolDocumentDataStoreNames.ExportPreAdviceNotification;
			return documentData;
		}

		void CreateLog(IStmALogProvider logParent, Event @event, ZDateTime time, params KeyValuePair<string, string>[] parameters)
		{
			logParent.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, time.ToOffset(), string.Empty, parameters);
			Thread.Sleep(1);
			Factory.Save();
		}

		void AddInterchangeRejectedEvent(IStmALogProvider logParent)
		{
			var eventParameters = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, Constants.EventReferenceMessageTypes.ExportPreAdviceNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "WiseTech Global"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, "Reason of interchange rejection"),
			};
			CreateLog(logParent, Events.InterchangeRejected, new ZDateTime(2024, 10, 15), eventParameters);
		}

		void AddInterchangeSentEvent(IStmALogProvider logParent)
		{
			var eventParameters = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, Constants.EventReferenceMessageTypes.ExportPreAdviceNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "WiseTech Global"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, "NZAKL"),
			};
			CreateLog(logParent, Events.InterchangeSent, new ZDateTime(2024, 10, 15), eventParameters);
		}

		void AddMessageSentEvent(IStmALogProvider logParent)
		{
			var eventParameters = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, Constants.EventReferenceMessageTypes.ExportPreAdviceNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"),
			};
			CreateLog(logParent, Events.MessageSent, new ZDateTime(2024, 10, 15), eventParameters);
		}

		void AddMessageAcceptedEvent(IStmALogProvider logParent)
		{
			var eventParameters = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, Constants.EventReferenceMessageTypes.ExportPreAdviceNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, "NZAKL"),
			};
			CreateLog(logParent, Events.MessageAccepted, new ZDateTime(2024, 10, 15), eventParameters);
		}

		void AddMessageRejectedEvent(IStmALogProvider logParent)
		{
			var eventParameters = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, Constants.EventReferenceMessageTypes.ExportPreAdviceNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, "NZAKL"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, "Reason of message rejected"),
			};
			CreateLog(logParent, Events.MessageRejected, new ZDateTime(2024, 10, 15), eventParameters);
		}

		void AddStatusUpdatedEvent(IStmALogProvider logParent)
		{
			var eventParameters = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, Constants.EventReferenceMessageTypes.ExportPreAdviceNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"),
			};
			CreateLog(logParent, Events.StatusUpdated, new ZDateTime(2024, 10, 15), eventParameters);
		}

		void AddMessageWithdrawCancelRequestEvent(IStmALogProvider logParent)
		{
			var eventParameters = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, Constants.EventReferenceMessageTypes.ExportPreAdviceNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, "NZAKL"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, "Reason of withdraw accepted"),
			};
			CreateLog(logParent, Events.MessageWithdrawCancelRequest, new ZDateTime(2024, 10, 15), eventParameters);
		}

		void AddMessageWithdrawalAcceptedEvent(IStmALogProvider logParent)
		{
			var eventParameters = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, Constants.EventReferenceMessageTypes.ExportPreAdviceNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, "NZAKL"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, "Reason of withdraw accepted"),
			};
			CreateLog(logParent, Events.MessageWithdrawCancelAccepted, new ZDateTime(2024, 10, 15), eventParameters);
		}

		#endregion
	}
}
