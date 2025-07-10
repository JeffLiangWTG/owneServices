using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DE;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.Testing.DE
{
	sealed class AdvancedLogisticsPortOrderMessagingExtensionsTest : TestCaseWithFactory
	{
		public void TestContinueWithSendingMessageWithdrawal_ShouldShowMessage_WhenMAAEventHasNotReceived()
		{
			var notifications = new UserNotificationTest();
			var extensions = CreateAdvancedLogisticsPortOrderMessagingExtensions(Events.MessageSent);
			var result = extensions.ContinueWithSendingMessageWithdrawal(notifications);

			AssertEquals("Message can be withdrawn only after you have received an acceptance response to the previous message.", false, result);
			Assert("Notification ShowMessage called", notifications.ShowMessageCalled);
			AssertEquals("Message can be withdrawn only after you have received an acceptance response to the previous message.", notifications.MessageContent);
		}

		public void TestContinueWithSendingMessageWithdrawal_ShouldNotShowMessage_WhenMAAEventHasReceived()
		{
			var notifications = new UserNotificationTest();
			var extensions = CreateAdvancedLogisticsPortOrderMessagingExtensions(Events.MessageAccepted, "CRF123", false, false);
			var result = extensions.ContinueWithSendingMessageWithdrawal(notifications);

			AssertNotNull(result);
			AssertEquals("Message can be withdrawn only after you have received an acceptance response to the previous message.", true, result);
			Assert("Notification ShowMessage called", !notifications.ShowMessageCalled);
		}

		public void TestContinueWithSendingMessageWithdrawal_ShouldShowMessage_WhenMWAEventHasReceived()
		{
			var notifications = new UserNotificationTest();
			var extensions = CreateAdvancedLogisticsPortOrderMessagingExtensions(Events.MessageWithdrawCancelAccepted);
			var result = extensions.ContinueWithSendingMessageWithdrawal(notifications);

			AssertEquals("Message can be withdrawn only after you have received an acceptance response to the previous message.", false, result);
			Assert("Notification ShowMessage called", notifications.ShowMessageCalled);
			AssertEquals("Message can be withdrawn only after you have received an acceptance response to the previous message.", notifications.MessageContent);
		}

		public void TestContinueWithSendingMessageWithdrawal_ShouldShowMessage_SHLEvent()
		{
			var notifications = new UserNotificationTest();
			var extensions = CreateAdvancedLogisticsPortOrderMessagingExtensions(Events.MessageSent, createSHLEvent: true, createSCMEvent: false);
			var result = extensions.ContinueWithSendingMessageWithdrawal(notifications);

			AssertEquals("A Customs Stop Loading (SHL – Held) event has been received for container('s). Please handle this request in ALPO.", false, result);
			Assert("Notification ShowMessage called", notifications.ShowMessageCalled);
			AssertEquals("A Customs Stop Loading (SHL – Held) event has been received for container('s) CONT1111111, CONT2222222. Please handle this request in ALPO.", notifications.MessageContent);
		}

		public void TestContinueWithSendingMessageWithdrawal_ShouldNotShowMessage_SHLEvent_SCMEvent_WhenMAAEventHasReceived()
		{
			var notifications = new UserNotificationTest();
			var extensions = CreateAdvancedLogisticsPortOrderMessagingExtensions(Events.MessageAccepted, "CRF123", true, true);
			var result = extensions.ContinueWithSendingMessageWithdrawal(notifications);

			AssertNotNull(result);
			AssertEquals("Message can be withdrawn only after you have received an acceptance response to the previous message.", true, result);
			Assert("Notification ShowMessage called", !notifications.ShowMessageCalled);
		}

		public void TestMessagingExtensionResult_Amendment_MessageSent()
		{
			AssertMessagingExtensionResult_Amendment(Events.MessageSent);
		}

		public void TestMessagingExtensionResult_Amendment_MessageAccepted()
		{
			AssertMessagingExtensionResult_Amendment(Events.MessageAccepted);
		}

		public void TestMessagingExtensionResult_Amendment_MessageWithdrawCancelAccepted()
		{
			AssertMessagingExtensionResult_Amendment(Events.MessageWithdrawCancelAccepted);
		}

		void AssertMessagingExtensionResult_Amendment(Event eventType)
		{
			var notifications = new UserNotificationTest();
			var extensions = CreateAdvancedLogisticsPortOrderMessagingExtensions(eventType);

			var result = extensions.IsSendingAmendment();
			AssertEquals("Message amendment not available", false, result);

			result = extensions.ContinueWithSendingMessageAmendment(notifications);
			AssertNull("Message can not be amended", result);
			Assert("Notification ShowMessage called", !notifications.ShowMessageCalled);
		}

		#region Implementation

		AdvancedLogisticsPortOrderMessagingExtensions CreateAdvancedLogisticsPortOrderMessagingExtensions(Event eventType, string customsReferenceNumber = "", bool createSHLEvent = false, bool createSCMEvent = false)
		{
			var documentName = "Advanced Logistics Port Order";

			var parameters = new List<KeyValuePair<string, string>>()
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.DBH)
			};
			if (!string.IsNullOrEmpty(customsReferenceNumber))
			{
				parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, customsReferenceNumber));
			}

			var consol = CreateConsolWithEventForALPODocument(eventType, documentName, createSHLEvent, createSCMEvent, parameters.ToArray());
			var advancedLogisticsPortOrder = new AdvancedLogisticsPortOrder("zzz", "zzz");
			advancedLogisticsPortOrder.ALPOReference = customsReferenceNumber;

			CreateLog(consol, eventType, ZDateTimeOffset.Now, parameters.ToArray());

			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();

			dynamicData
				.SetupGet(data => data.Value)
				.Returns(advancedLogisticsPortOrder);

			document
				.SetupGet(doc => doc.Data)
				.Returns(dynamicData.Object);

			return new AdvancedLogisticsPortOrderMessagingExtensions(document.Object, consol);
		}

		ForwardingConsol CreateConsolWithEventForALPODocument(Event eventType, string documentName, bool createSHLEvent = false, bool createSCMEvent = false, params KeyValuePair<string, string>[] parameters)
		{
			var consol = CreateConsol();

			consol.Logs.RemoveAndDeleteAll();

			var visualizerDocumentData = Factory.New<VisualizerDocumentData>();
			visualizerDocumentData.JDD_ParentID = consol.PK;
			visualizerDocumentData.JDD_ParentTableCode = consol.TablePrefix;
			visualizerDocumentData.JDD_Name = DataContext.DEAdvancedLogisticsPortOrder;
			visualizerDocumentData.Logs.CreateOrRecreateEventLog(eventType, EstimateActual.Actual, ZDateTimeOffset.Now, string.Empty, parameters);

			if (createSHLEvent)
			{
				foreach (ForwardingContainer container in consol.Containers)
				{
					CreateLog(container, Events.Held, ZDateTimeOffset.Now);
				}
			}

			if (createSCMEvent)
			{
				foreach (ForwardingContainer container in consol.Containers)
				{
					CreateLog(container, Events.ClearanceCompleted, ZDateTimeOffset.Now.Add(new System.TimeSpan(10, 0, 0, 0)));
				}
			}

			Factory.Save();

			return consol;
		}

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00002000";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "FRPRA";
			consol.JK_RL_NKDischargePort = "BEANR";
			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL_Reference";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1111111";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2222222";

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNYTN";
			transport.JW_RL_NKDiscPort = "BEANR";
			transport.JW_Vessel = "COSCO NEBULA";
			transport.JW_VoyageFlight = "85475";
			transport.JW_ETD = new ZDateTime(2020, 10, 6, 8, 45, 00);
			transport.JW_ETA = new ZDateTime(2020, 10, 22, 12, 15, 00);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;
			transport2.JW_RL_NKLoadPort = "BEANR";
			transport2.JW_RL_NKDiscPort = "BEWJG";
			transport2.JW_ETD = new ZDateTime(2020, 10, 23, 7, 35, 00);
			transport2.JW_ETA = new ZDateTime(2020, 10, 25, 15, 55, 00);
			transport2.JW_Vessel = "TRAILER";
			transport2.JW_VoyageFlight = "1-TRU-CK1";

			return consol;
		}

		void CreateLog(ForwardingConsol consol, Event @event, ZDateTimeOffset time, params KeyValuePair<string, string>[] parameters)
		{
			consol.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, time, string.Empty, parameters);
			Thread.Sleep(1);
			Factory.Save();
		}

		void CreateLog(ForwardingContainer container, Event @event, ZDateTimeOffset time, params KeyValuePair<string, string>[] parameters)
		{
			container.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, time, string.Empty, parameters);
			Thread.Sleep(1);
			Factory.Save();
		}

		class UserNotificationTest : IUserNotifications
		{
			public bool ShowMessageCalled
			{
				get; private set;
			}

			public string MessageContent
			{
				get; private set;
			}

			public bool ShowConfirmation(string message, string caption)
			{
				return false;
			}

			public bool ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString)
			{
				return false;
			}

			public void ShowMessage(string message, string caption)
			{
				MessageContent = message;
				ShowMessageCalled = true;
			}
		}

		#endregion
	}
}
