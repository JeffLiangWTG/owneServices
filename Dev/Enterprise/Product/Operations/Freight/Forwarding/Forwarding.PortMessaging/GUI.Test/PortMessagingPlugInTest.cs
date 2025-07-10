using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.PortMessaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.PortMessaging.GUI.Testing
{
	sealed class PortMessagingPlugInTest : ZPlugInGenericTest
	{
		[TestDate(2021, 6, 1)]
		public void TestSendMessage_Shipment()
		{
			var consol = GetSavedConsol();
			var shipment = consol.Shipments[0];
			var portMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);

			foreach (var menuItem in OrderMenuItems)
			{
				shipment.Logs.CancelAll();
				var isCancellation = menuItem.MenuPath == hdsMenu + "|Forwarding to Another Port" || menuItem.MenuPath == hdsMenu + "|Cancellation of Export on Exit" || menuItem.MenuPath == hdsMenu + "|Cancellation Because of Errors";
				shipment.JS_SZB = isCancellation ? "FOO" : "";
				portMessaging.JSM_MovementReferenceNumberComplete = isCancellation;
				Factory.Save();

				AssertMessageSending(shipment, menuItem.MenuPath, menuItem.EventReference, menuItem.MessageType);
			}

			foreach (var menuItem in OrderMenuItems)
			{
				shipment.Logs.CancelAll();
				var isCancellation = menuItem.MenuPath == hdsMenu + "|Forwarding to Another Port" || menuItem.MenuPath == hdsMenu + "|Cancellation of Export on Exit" || menuItem.MenuPath == hdsMenu + "|Cancellation Because of Errors";
				shipment.JS_SZB = isCancellation ? "FOO" : "";
				portMessaging.JSM_MovementReferenceNumberComplete = isCancellation;
				Factory.Save();

				AssertMessageSending(shipment, menuItem.MenuPath, menuItem.EventReference, menuItem.MessageType, true, DialogResult.Yes);
			}

			foreach (var menuItem in OrderMenuItems)
			{
				shipment.Logs.CancelAll();
				var isCancellation = menuItem.MenuPath == hdsMenu + "|Forwarding to Another Port" || menuItem.MenuPath == hdsMenu + "|Cancellation of Export on Exit" || menuItem.MenuPath == hdsMenu + "|Cancellation Because of Errors";
				shipment.JS_SZB = isCancellation ? "FOO" : "";
				portMessaging.JSM_MovementReferenceNumberComplete = isCancellation;
				Factory.Save();

				AssertMessageSending(shipment, menuItem.MenuPath, menuItem.EventReference, menuItem.MessageType, true, DialogResult.No);
			}
		}

		[TestDate(2021, 6, 1)]
		public void TestSendMessage_Consol()
		{
			var consol = GetSavedConsol();
			consol.Shipments[0].JS_SZB = "123456";
			Factory.Save();

			foreach (var menuItem in OrderMenuItems)
			{
				consol.Logs.CancelAll();
				Factory.Save();
				AssertMessageSending(consol, menuItem.MenuPath, menuItem.EventReference, menuItem.MessageType);
			}
		}

		[TestDate(2021, 6, 1)]
		public void TestSendMessageSequence_Consol()
		{
			const string expectedNoReplyError = "A reply has not been received for the last message to Dakosy.\r\nReplies must be received from Dakosy before you can send additional messages.";
			const string expectedPendingError = "Dakosy status is Pending.\r\nAn Acceptance or Rejection must be received before you can send additional messages to Dakosy.";
			const string expectedCancellationAbortedError = "Cancellation message aborted.";
			const string expectedSzbError = "Not all sea shipments on this Consol have received their release SZB number. Send the Port Order on the Shipments to receive release number(s) from Dakosy.";

			var consol = GetSavedConsol();
			var manager = new ConsolPortMessagingManager(consol);

			AssertEquals(expectedSzbError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
			AssertEquals(expectedSzbError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSCancellationBecauseOfErrors));

			consol.Shipments[0].JS_SZB = "123456";
			Factory.Save();

			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			addLog(Events.MessageSent, "Port Order with HDS Message Sent to Dakosy|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals(expectedNoReplyError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderInbound));
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.GatePass));
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.RequestForPortServices));
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.CertificateOfObligation));
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.RequestForRailDischarge));
			AssertEquals(expectedNoReplyError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSCancellationBecauseOfErrors, true));
			AssertEquals(expectedCancellationAbortedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.GatePass, true, false));

			addLog(Events.MessageRejected, "Message Rejected|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
			AssertEquals(expectedCancellationAbortedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSCancellationBecauseOfErrors, true, false));

			addLog(Events.MessageSent, "Port Order with HDS Message Sent to Dakosy|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();

			addLog(Events.InterchangeRejected, "Interchange Rejected|DEP=eHub|RES=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
			AssertEquals(expectedCancellationAbortedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSCancellationBecauseOfErrors, true, false));

			addLog(Events.MessageSent, "Port Order with HDS Message Sent to Dakosy|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();

			addLog(Events.MessageAccepted, "Message Accepted|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSCancellationBecauseOfErrors, true));

			addLog(Events.MessageWithdrawCancelRequest, "Port Order with HDS Cancellation Because of Errors Message Withdraw/Cancel Request sent to Dakosy|DEP=Dakosy|MST=Port Order with HDS Cancellation Because of Errors");
			Factory.Save();
			AssertEquals(expectedNoReplyError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
			AssertEquals(expectedNoReplyError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSCancellationBecauseOfErrors, true));

			addLog(Events.InterchangeRejected, "Interchange Rejected|DEP=eHub|RES=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSCancellationBecauseOfErrors, true));

			addLog(Events.MessageWithdrawCancelRequest, "Port Order with HDS Cancellation Because of Errors Message Withdraw/Cancel Request sent to Dakosy|DEP=Dakosy|MST=Port Order with HDS Cancellation Because of Errors");
			Factory.Save();

			addLog(Events.MessageRejected, "Message Rejected|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSCancellationBecauseOfErrors, true));

			addLog(Events.MessageWithdrawCancelRequest, "Port Order with HDS Cancellation Because of Errors Message Withdraw/Cancel Request sent to Dakosy|DEP=Dakosy|MST=Port Order with HDS Cancellation Because of Errors");
			Factory.Save();

			addLog(Events.MessageWithdrawCancelAccepted, "Message Withdraw/Cancel Accepted|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
			AssertEquals(expectedCancellationAbortedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSCancellationBecauseOfErrors, true, false));
			AssertEquals(expectedCancellationAbortedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSCancellationOnExit, true, false));
			AssertEquals(expectedCancellationAbortedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.GatePass, true, false));
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSCancellationBecauseOfErrors, false));
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSCancellationOnExit, false));
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.GatePass, false));

			addLog(Events.MessageSent, "Port Order with HDS Message Sent to Dakosy|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();

			addLog(Events.InterchangeReceiptAcknowledged, "Interchange Receipt Acknowledged|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals(expectedPendingError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			addLog(Events.MessagePendingProcessing, "Message Pending Processing|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals(expectedPendingError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderInbound));
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.GatePass));
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.RequestForPortServices));
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.CertificateOfObligation));
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.RequestForRailDischarge));
			AssertEquals(expectedCancellationAbortedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.GatePass, true, false));
			AssertEquals(expectedCancellationAbortedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.RequestForPortServices, true, false));
			AssertEquals(expectedCancellationAbortedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.CertificateOfObligation, true, false));
			AssertEquals(expectedCancellationAbortedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.RequestForRailDischarge, true, false));

			addLog(Events.MessageAccepted, "Message Accepted|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
			AssertEquals(ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSCancellationBecauseOfErrors, true));

			addLog(Events.MessageWithdrawCancelRequest, "Port Order with HDS Cancellation Because of Errors Message Withdraw/Cancel Request sent to Dakosy|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();

			consol.JK_SZB = "123456";
			addLog(Events.MessagePendingProcessing, "Message Pending Processing|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals(expectedPendingError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
			AssertEquals(expectedPendingError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSCancellationBecauseOfErrors, true));

			void addLog(Event logEvent, ZString reference)
			{
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
				consol.GetLogs().AddNew(logEvent, reference);
			}
		}

		[TestDate(2021, 6, 1)]
		public void TestSendMessageSequenceWithPropagatedEvent_Consol()
		{
			const string expectedPendingError = "Dakosy status is Pending.\r\nAn Acceptance or Rejection must be received before you can send additional messages to Dakosy.";
			const string expectedNoReplyError = "A reply has not been received for the last message to Dakosy.\r\nReplies must be received from Dakosy before you can send additional messages.";

			var consol = GetSavedConsol();
			var manager = new ConsolPortMessagingManager(consol);

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			shipment2.JS_RL_NKOrigin = "DEHAM";
			shipment2.JS_RL_NKDestination = "PLGDY";

			consol.Shipments[0].JS_SZB = "123456";
			shipment2.JS_SZB = "123457";
			Factory.Save();

			addLog(consol, Events.MessageSent, "Port Order with HDS Message Sent to Dakosy|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();

			addLog(consol, Events.MessageAccepted, "Message Accepted|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();

			var log = addLog(consol, Events.MessagePendingProcessing, "Message Pending Processing|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals("Precondition: Error should be returned after MPP event", expectedPendingError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			log.Cancel();
			Factory.Save();
			AssertEquals("Precondition: Cancelled event should be ignored in validation", ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			addLog(shipment2, Events.MessagePendingProcessing, "Message Pending Processing|DEP=Dakosy|MST=Port Order with HDS");
			addLog(consol.Shipments[0], Events.MessagePendingProcessing, "Message Pending Processing|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals("Precondition: Most recent consol log should be a propagated MPP", "Propagated: All Shipments|DEP=Dakosy|MST=Port Order with HDS", consol.Logs.MostRecentLogByPostedDate.SL_Reference);
			AssertEquals("Precondition: Most recent consol log should be a propagated MPP", Events.MessagePendingProcessingCode, consol.Logs.MostRecentLogByPostedDate.SL_SE_NKEvent);
			AssertEquals("MPP event in shipment should prevent message sending in consol", expectedPendingError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			addLog(consol.Shipments[0], Events.MessageAccepted, "Message Accepted|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals("MPP event in one of the shipments should still prevent message sending in consol", expectedPendingError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			addLog(shipment2, Events.MessageAccepted, "Message Accepted|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals("MAA event in all shipments should allow message sending in consol", ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			addLog(consol.Shipments[0], Events.MessageWithdrawCancelRequest, "Message Withdrawl/Cancel Request|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals("MWR event in shipment should prevent message sending in consol", expectedNoReplyError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			StmALog addLog(BusinessObject bizo, Event logEvent, ZString reference)
			{
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
				return bizo.GetLogs().AddNew(logEvent, reference);
			}
		}

		[TestDate(2021, 6, 1)]
		public void TestSendMessageSequenceWithPropagatedEvent_Shipment()
		{
			const string expectedPendingError = "Dakosy status is Pending.\r\nAn Acceptance or Rejection must be received before you can send additional messages to Dakosy.";
			const string expectedNoReplyError = "A reply has not been received for the last message to Dakosy.\r\nReplies must be received from Dakosy before you can send additional messages.";

			var consol1 = GetSavedConsol();
			var consol2 = GetSecondSavedConsol();

			var shipment = consol1.Shipments.AddNew();
			consol2.Shipments.Add(shipment);

			var manager = new ShipmentPortMessagingManager(shipment);
			Factory.Save();

			addLog(shipment, Events.MessageSent, "Port Order with HDS Message Sent to Dakosy|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();

			addLog(shipment, Events.MessageAccepted, "Message Accepted|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();

			var log = addLog(shipment, Events.MessagePendingProcessing, "Message Pending Processing|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals("Precondition: Error should be returned after MPP event", expectedPendingError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			log.Cancel();
			Factory.Save();
			AssertEquals("Precondition: Cancelled event should be ignored in validation", ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			addLog(consol1, Events.MessagePendingProcessing, "Message Pending Processing|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals("MPP event in consol should prevent message sending in shipment", expectedPendingError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			addLog(consol2, Events.MessagePendingProcessing, "Message Pending Processing|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();

			addLog(consol1, Events.MessageAccepted, "Message Accepted|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals("MPP event in one of the consols should prevent message sending in shipment", expectedPendingError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			addLog(consol2, Events.MessageAccepted, "Message Accepted|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals("MAA event in all consols should allow message sending in shipment", ZString.Empty, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			addLog(consol1, Events.MessageWithdrawCancelRequest, "Message Withdrawal/Cancel Request|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();
			AssertEquals("MWR event in consol should prevent message sending in shipment", expectedNoReplyError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			StmALog addLog(BusinessObject bizo, Event logEvent, ZString reference)
			{
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
				return bizo.GetLogs().AddNew(logEvent, reference);
			}
		}

		[TestDate(2021, 6, 1)]
		public void TestResendMessageAfterCancellation_Shipment()
		{
			var consol = GetSavedConsol();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_SZB = "1245253";

			shipment.GetLogs().AddNew(Events.MessageWithdrawCancelRequest, "Cancel me|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			shipment.GetLogs().AddNew(Events.MessagePendingProcessing, "Processing Cancellation|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();

			AssertResendAfterCancellation(shipment);
		}

		[TestDate(2021, 6, 1)]
		public void TestResendMessageAfterCancellation_Consol()
		{
			var consol = GetSavedConsol();
			consol.Shipments[0].JS_SZB = "123456";
			consol.JK_SZB = "123512";

			consol.GetLogs().AddNew(Events.MessageWithdrawCancelRequest, "Cancel me|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			consol.GetLogs().AddNew(Events.MessagePendingProcessing, "Processing Cancellation|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();

			AssertResendAfterCancellation(consol);
		}

		public void TestMenuItemVisibility()
		{
			const string portOrderPath = "Port Messaging|Port Order (DEHAM)";
			const string exportOnlyItem = portOrderPath + "|Send Port Order for Inbound Delivery";
			const string importOnlyItem = portOrderPath + "|Send Port Order for Outbound Delivery";
			const string exportImportItem = portOrderPath + "|Send Gate Pass";
			const string noMessagesAvailableItem = "Port Messaging|No Messages Available";

			var consol = GetSavedConsol();

			AssertEquals(true, ConsolPortMessagingManager.IsExportConsolForDakosy(consol));
			AssertEquals(false, ConsolPortMessagingManager.IsImportConsolForDakosy(consol));
			AssertMenuItemVisible(consol, portOrderPath, true);
			AssertMenuItemVisible(consol, exportOnlyItem, true);
			AssertMenuItemVisible(consol, importOnlyItem, false);
			AssertMenuItemVisible(consol, exportImportItem, true);
			AssertMenuItemVisible(consol, noMessagesAvailableItem, false);

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEHAM";
			consol.Transports[0].JW_RL_NKLoadPort = "NZAKL";
			consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";

			AssertEquals(false, ConsolPortMessagingManager.IsExportConsolForDakosy(consol));
			AssertEquals(true, ConsolPortMessagingManager.IsImportConsolForDakosy(consol));
			AssertMenuItemVisible(consol, portOrderPath, true);
			AssertMenuItemVisible(consol, exportOnlyItem, false);
			AssertMenuItemVisible(consol, importOnlyItem, true);
			AssertMenuItemVisible(consol, exportImportItem, true);
			AssertMenuItemVisible(consol, noMessagesAvailableItem, false);

			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.Transports[0].JW_RL_NKDiscPort = "SGSIN";

			AssertEquals(false, ConsolPortMessagingManager.IsExportConsolForDakosy(consol));
			AssertEquals(false, ConsolPortMessagingManager.IsImportConsolForDakosy(consol));
			AssertMenuItemIsNull(consol, portOrderPath);
			AssertMenuItemIsNull(consol, exportOnlyItem);
			AssertMenuItemIsNull(consol, importOnlyItem);
			AssertMenuItemIsNull(consol, exportImportItem);
			AssertMenuItemVisible(consol, noMessagesAvailableItem, true);
		}

		public void TestMenuItemVisibilityWithSecurityRestrictions()
		{
			Env.Security.PortMessaging.IsAllowed = false;
			var consol = GetSavedConsol();

			Factory.Save();

			AssertSecurityMessageMenuItem(consol);

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "GBLON";

			AssertSecurityMessageMenuItemIsNull(consol);

			consol.JK_RL_NKDischargePort = "DEHAM";

			AssertSecurityMessageMenuItem(consol);
		}

		#region Forms Menus

		public void TestFormsMenus_NLPortBase_Export()
		{
			AssertFormsMenus_ExportNotification();
		}

		public void TestFormsMenus_NLPortBase_Import()
		{
			AssertFormsMenus_ImportNotification();
		}

		void AssertFormsMenus_ExportNotification()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Netherlands))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "NLHRB";
				consol.JK_RL_NKDischargePort = "AUSYD";

				var transport = consol.Transports.Cast<Transport>().First();
				transport.JW_TransportType = Constants.TransportModes.Sea;
				transport.JW_RL_NKLoadPort = "NLHRB";
				transport.JW_RL_NKDiscPort = "AUSYD";

				using (var form = new FormForTest(consol))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Port Messaging");

					AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertMultilineASCIIEquals("Electronic Menu Items for sea consol",
@"Port Messaging
   Portbase (NL)
      Export Notification",
						electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		void AssertFormsMenus_ImportNotification()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Netherlands))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "NLHRB";

				var transport = consol.Transports.Cast<Transport>().First();
				transport.JW_TransportType = Constants.TransportModes.Sea;
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "NLHRB";

				using (var form = new FormForTest(consol))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Port Messaging");

					AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertMultilineASCIIEquals("Electronic Menu Items for sea consol",
@"Port Messaging
   Portbase (NL)
      Import Notification",
						electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestFormsMenus_CargoDuesMenuItems_Export()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "ZA2WC";
				consol.JK_RL_NKDischargePort = "AUSYD";

				var transport1 = consol.Transports.Cast<Transport>().First();
				transport1.JW_TransportType = Constants.TransportModes.Sea;
				transport1.JW_RL_NKLoadPort = "ZA2WC";
				transport1.JW_RL_NKDiscPort = "ZA3WC";

				var transport2 = consol.Transports.AddNew();
				transport2.JW_TransportType = Constants.TransportModes.Sea;
				transport2.JW_RL_NKLoadPort = "ZA3WC";
				transport2.JW_RL_NKDiscPort = "AUSYD";

				using (var form = new FormForTest(consol))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Port Messaging");

					AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertMultilineASCIIEquals("Electronic Menu Items for sea consol",
@"Port Messaging
   Cargo Dues (ZA)
      Cargo Dues Order
         Export
      Cargo Dues Quotation
         Export",
						electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestFormsMenus_CargoDuesMenuItems_Import()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "ZA3WC";

				var transport1 = consol.Transports.Cast<Transport>().First();
				transport1.JW_TransportType = Constants.TransportModes.Sea;
				transport1.JW_RL_NKLoadPort = "AUSYD";
				transport1.JW_RL_NKDiscPort = "ZA2WC";

				var transport2 = consol.Transports.AddNew();
				transport2.JW_TransportType = Constants.TransportModes.Sea;
				transport2.JW_RL_NKLoadPort = "ZA2WC";
				transport2.JW_RL_NKDiscPort = "ZA3WC";

				using (var form = new FormForTest(consol))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Port Messaging");

					AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertMultilineASCIIEquals("Electronic Menu Items for sea consol",
@"Port Messaging
   Cargo Dues (ZA)
      Cargo Dues Order
         Import
      Cargo Dues Quotation
         Import", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestFormsMenus_CargoDuesMenuItems_Coastwise()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "ZA3WC";
				consol.JK_RL_NKDischargePort = "ZA2WC";

				var transport = consol.Transports.Cast<Transport>().First();
				transport.JW_TransportType = Constants.TransportModes.Sea;
				transport.JW_RL_NKLoadPort = "ZA3WC";
				transport.JW_RL_NKDiscPort = "ZA2WC";

				using (var form = new FormForTest(consol))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Port Messaging");

					AssertNotNull("Port Messaging menu item exists", electronicMessagingMenuItem);

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertMultilineASCIIEquals("Port Menu Items for sea consol",
@"Port Messaging
   Cargo Dues (ZA)
      Cargo Dues Order
         Load Coastwise
         Discharge Coastwise
      Cargo Dues Quotation
         Load Coastwise
         Discharge Coastwise", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestFormsMenus_China()
		{
			var overrides = new AdditionalHouseBillOfLadingTypeCollection();
			var dhl = new AdditionalHouseBillOfLadingType
			{
				Code = Constants.AddtionalHouseBillTypeMenu.Code.DHLHBL,
				Description = (NoResString)"DHL HBL",
			};
			overrides.Add(dhl);
			dhl.Enable = true;

			using (FreightDataRegistry.Instance.AddtionalHouseBillOfLadingTypes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, overrides))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "CNNGB";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.Containers.AddNew().FillWithValidTestData();

				var transport = consol.Transports.Cast<Transport>().First();
				transport.JW_TransportType = Constants.TransportModes.Sea;
				transport.JW_RL_NKLoadPort = "CNNGB";
				transport.JW_RL_NKDiscPort = "AUSYD";

				using (var form = new FormForTest(consol))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Port Messaging");

					AssertNotNull("Port Messaging menu item exists", electronicMessagingMenuItem);

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertMultilineASCIIEquals("Port Menu Items for sea consol",
	@"Port Messaging
   eTerminal Release Manifest
   Container Load Plan",
						electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestFormsMenus_TPTMenuItems_Scenario1()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "ZA2TP";

			var transport1 = consol.Transports.Cast<Transport>().First();
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "ZADUR";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = "ZADUR";
			transport2.JW_RL_NKDiscPort = "ZACPT";

			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Constants.TransportModes.Sea;
			transport3.JW_LegOrder = 3;
			transport3.JW_RL_NKLoadPort = "ZACPT";
			transport3.JW_RL_NKDiscPort = "ZAESL";

			var transport4 = consol.Transports.AddNew();
			transport4.JW_TransportMode = Constants.TransportModes.Road;
			transport4.JW_LegOrder = 4;
			transport4.JW_RL_NKLoadPort = "ZAESL";
			transport4.JW_RL_NKDiscPort = "ZA2TP";

			Factory.Save();

			AssertTPTMenuItems(consol,
@"Port Messaging
   Cargo Dues (ZA)
      Cargo Dues Order
         Import
      Cargo Dues Quotation
         Import
   Service Instruction (ZA)
      Landing Order (Imports)
      Transhipment Order (ZADUR)
      Transhipment Order (ZACPT)");
		}

		public void TestFormsMenus_TPTMenuItems_Scenario2()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
			consol.JK_RL_NKLoadPort = "ZA2WC";
			consol.JK_RL_NKDischargePort = "USSFO";

			var transport1 = consol.Transports.Cast<Transport>().First();
			transport1.JW_TransportMode = Constants.TransportModes.Rail;
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = "ZA2WC";
			transport1.JW_RL_NKDiscPort = "ZAELS";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = "ZAELS";
			transport2.JW_RL_NKDiscPort = "ZADUR";

			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Constants.TransportModes.Sea;
			transport3.JW_LegOrder = 3;
			transport3.JW_RL_NKLoadPort = "ZADUR";
			transport3.JW_RL_NKDiscPort = "ZACPT";

			var transport4 = consol.Transports.AddNew();
			transport4.JW_TransportMode = Constants.TransportModes.Sea;
			transport4.JW_LegOrder = 4;
			transport4.JW_RL_NKLoadPort = "ZACPT";
			transport4.JW_RL_NKDiscPort = "USSFO";

			Factory.Save();

			AssertTPTMenuItems(consol,
@"Port Messaging
   Cargo Dues (ZA)
      Cargo Dues Order
         Export
      Cargo Dues Quotation
         Export
   Service Instruction (ZA)
      Shipping Order (Exports)
      Transhipment Order (ZADUR)
      Transhipment Order (ZACPT)");
		}

		public void TestFormsMenus_TPTMenuItems_Scenario3()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
			consol.JK_RL_NKLoadPort = "USSFO";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var transport1 = consol.Transports.Cast<Transport>().First();
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = "USSFO";
			transport1.JW_RL_NKDiscPort = "ZACPT";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = "ZACPT";
			transport2.JW_RL_NKDiscPort = "AUSYD";

			Factory.Save();

			AssertTPTMenuItems(consol,
@"Port Messaging
   Service Instruction (ZA)
      Transhipment Order (ZACPT)");
		}

		void AssertTPTMenuItems(ForwardingConsol consol, string expectedElectronicMessagingMenuItems)
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				using (var form = new FormForTest(consol))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Port Messaging");

					AssertNotNull("Port Messaging menu item exists", electronicMessagingMenuItem);

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertMultilineASCIIEquals("Port Menu Items for sea consol",
						expectedElectronicMessagingMenuItems,
						electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestFormsMenus_ZARelatedMenuItems_Import()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
				consol.JK_RL_NKLoadPort = "ZA2WC";
				consol.JK_RL_NKDischargePort = "AUSYD";

				var transport = consol.Transports.Cast<Transport>().First();
				transport.JW_TransportType = Constants.TransportModes.Sea;
				transport.JW_RL_NKLoadPort = "ZA2WC";
				transport.JW_RL_NKDiscPort = "AUSYD";

				using (var form = new FormForTest(consol))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Port Messaging");

					AssertNotNull("Port Messaging menu item exists", electronicMessagingMenuItem);

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertMultilineASCIIEquals("Port Menu Items for sea consol",
@"Port Messaging
   Cargo Dues (ZA)
      Cargo Dues Order
         Export
      Cargo Dues Quotation
         Export
   Service Instruction (ZA)
      Shipping Order (Exports)",
						electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestFormsMenus_ZARelatedMenuItems_Export()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
				consol.JK_RL_NKLoadPort = "ZA2WC";
				consol.JK_RL_NKDischargePort = "AUSYD";

				var transport = consol.Transports.Cast<Transport>().First();
				transport.JW_TransportType = Constants.TransportModes.Sea;
				transport.JW_RL_NKLoadPort = "ZA2WC";
				transport.JW_RL_NKDiscPort = "AUSYD";

				using (var form = new FormForTest(consol))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Port Messaging");

					AssertNotNull("Port Messaging menu item exists", electronicMessagingMenuItem);

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertMultilineASCIIEquals("Port Menu Items for sea consol",
@"Port Messaging
   Cargo Dues (ZA)
      Cargo Dues Order
         Export
      Cargo Dues Quotation
         Export
   Service Instruction (ZA)
      Shipping Order (Exports)",
						electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestElectronicMenuItems_FR_DTIDTE()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
				consol.JK_RL_NKLoadPort = "FR223";
				consol.JK_RL_NKDischargePort = "FR123";

				var shipment = consol.Shipments.AddNew();
				shipment.OuterPackLines.AddNew();

				consol.Containers.AddNew();

				using (var form = new FormForTest(consol))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Port Messaging");

					AssertNotNull("Port Messaging menu item exists", electronicMessagingMenuItem);

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertContains("Underbond Movement Request (DTI) (FR)", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
					AssertContains("Underbond Movement Request (DTE) (FR)", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestFormsMenusCombineDakosyMenus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
				consol.JK_RL_NKLoadPort = "DEHAM";
				consol.JK_RL_NKDischargePort = "ZADUR";

				using (var form = new FormForTest(consol))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Port Messaging");

					AssertNotNull("Port Messaging menu item exists", electronicMessagingMenuItem);

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertMultilineASCIIEquals("Port Menu Items for sea consol",
@"Port Messaging
   Port Order with HDS (DEHAM)
      Send Port Order With HDS
      Cancellation Because of Errors
      Cancellation of Export on Exit
      Forwarding to Another Port
   Port Order (DEHAM)
      Send Port Order for Inbound Delivery
      Send Port Order for Inbound Delivery Cancellation
      Send Stop Request
      Send Gate Pass
      Send Gate Pass Cancellation
      Send Request for Port Services
      Send Request for Port Services Cancellation
      Send Certificate of Obligation
      Send Certificate of Obligation Cancellation
      Send Request for Rail Discharge
      Send Request for Rail Discharge Cancellation
   Cargo Dues (ZA)
      Cargo Dues Order
         Import
      Cargo Dues Quotation
         Import
   Service Instruction (ZA)
      Landing Order (Imports)", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestElectronicMenuItems_France_DocumentAndFormMenusMerged()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_ConsolMode = Constants.ContainerModes.LCL;
				consol.JK_AgentsReference = "";
				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "FRPAR";
				consol.JK_RL_NKDischargePort = "FRSXB";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "FRSXB";

				var transportLeg = consol.Transports.Cast<Transport>().Single();
				transportLeg.JW_TransportMode = Constants.TransportModes.Sea;
				transportLeg.JW_RL_NKLoadPort = "FRPAR";
				transportLeg.JW_RL_NKDiscPort = "FRSXB";

				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "AA";
				var packline = shipment.OuterPackLines.AddNew();
				container.AddPackLine(packline);

				Factory.Save();

				using (var form = new FormForTest(consol))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Port Messaging");

					AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertMultilineASCIIEquals("Electronic Menu Items",
@"Port Messaging
   Import
      Underbond Movement Request (DTI) (FR)
      Import Manifest (LPD) (FR)
      File Creation Request (DOS) (FR)
      Tracing Request (TRC) (FR)
   Export
      Container Advice to Booking (AMQ) (FR)
      Underbond Movement Request (DTE) (FR)
      File Creation Request (DOS) (FR)
      Tracing Request (TRC) (FR)
", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}

				consol.JK_ConsolMode = Constants.ContainerModes.FCL;
				Factory.Save();

				using (var form = new FormForTest(consol))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Port Messaging");

					AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertMultilineASCIIEquals("Electronic Menu Items",
@"Port Messaging
   Import
      Underbond Movement Request (DTI) (FR)
      File Creation Request (DOS) (FR)
      Tracing Request (TRC) (FR)
   Export
      Container Advice to Booking (AMQ) (FR)
      Underbond Movement Request (DTE) (FR)
      File Creation Request (DOS) (FR)
      Tracing Request (TRC) (FR)
", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestElectronicMenuItems_PackAndUnPackDepotAddressIsFrance()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_ConsolMode = Constants.ContainerModes.LCL;
				consol.JK_AgentsReference = "";
				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "FRPAR";
				consol.JK_RL_NKDischargePort = "AUSYD";

				var depotAddress = Factory.New<OrgHeader>();
				depotAddress.OH_FullName = "FromFR";
				depotAddress.OH_RL_NKClosestPort = "FRCAL";
				depotAddress.MainAddress.Address1 = "Unit 13";
				depotAddress.MainAddress.Address2 = "4 Lost Lane";
				depotAddress.MainAddress.City = "French";
				depotAddress.MainAddress.Postcode = "2000";
				depotAddress.MainAddress.OA_RN_NKCountryCode = "FR";

				consol.JK_OA_UnpackDepotAddress = depotAddress.MainAddress.PK;
				consol.JK_OA_PackDepotAddress = depotAddress.MainAddress.PK;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "FRSXB";

				var transportLeg = consol.Transports.Cast<Transport>().Single();
				transportLeg.JW_TransportMode = Constants.TransportModes.Sea;
				transportLeg.JW_RL_NKLoadPort = "FRPAR";
				transportLeg.JW_RL_NKDiscPort = "FRSXB";

				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "AA";
				var packline = shipment.OuterPackLines.AddNew();
				container.AddPackLine(packline);

				Factory.Save();

				using (var form = new FormForTest(consol))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Port Messaging");

					AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertMultilineASCIIEquals("Electronic Menu Items",
@"Port Messaging
   Import
      Outturn Report (CDM) (FR)
   Export
      Container Advice to Booking (AMQ) (FR)
      Underbond Movement Request (DTE) (FR)
      Final Container Manifest (LDE) (FR)
      File Creation Request (DOS) (FR)
      Tracing Request (TRC) (FR)
", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestElectronicMenuItems_Belgium_DocumentAndFormMenusMerged()
		{
			AssertElectronicMenuItems_Belgium_Export();
			AssertElectronicMenuItems_Belgium_Import();
			AssertElectronicMenuItems_Belgium_TransShip();
		}

		void AssertElectronicMenuItems_Belgium_Export()
		{
			var consolExport = Factory.New<ForwardingConsol>();
			consolExport.JK_TransportMode = Constants.TransportModes.Sea;
			consolExport.JK_AgentsReference = "";
			consolExport.JK_AgentType = Constants.AgentType.Agent;
			consolExport.JK_RL_NKLoadPort = "BEANR";
			consolExport.JK_RL_NKDischargePort = "AUSYD";

			var shipmentExport = consolExport.Shipments.AddNew();
			shipmentExport.JS_TransportMode = Constants.TransportModes.Sea;
			shipmentExport.JS_RL_NKOrigin = "BEANR";
			shipmentExport.JS_RL_NKDestination = "AUSYD";

			var transportLegExport = consolExport.Transports.Cast<Transport>().Single();
			transportLegExport.JW_TransportMode = Constants.TransportModes.Sea;
			transportLegExport.JW_RL_NKLoadPort = "BEANR";
			transportLegExport.JW_RL_NKDiscPort = "AUSYD";

			var containerExport = consolExport.Containers.AddNew();
			containerExport.JC_ContainerNum = "AA";
			var packlineExport = shipmentExport.OuterPackLines.AddNew();
			containerExport.AddPackLine(packlineExport);

			var eventParameters = new[] { new KeyValuePair<string, string>("MST", "Secure Container Release") };
			containerExport.Logs.AddNew(Events.Authorised, eventParameters);
			eventParameters = new[] { new KeyValuePair<string, string>("MST", "Secure Container Release - Transfer") };
			containerExport.Logs.AddNew(Events.MessageAccepted, eventParameters);

			Factory.Save();

			using (var form = new FormForTest(consolExport))
			{
				form.Show();

				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.Cast<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Port Messaging");

				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				AssertMultilineASCIIEquals("Electronic Menu Items",
@"Port Messaging
   Export
      Export Notification (BE)
      Dangerous Goods Notification (BE)
", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
			}

			containerExport.Logs.AddNew(Events.Authorised
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, "AA")
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerRelease));

			var container2 = consolExport.Containers.AddNew();
			container2.JC_ContainerNum = "BB";
			container2.Logs.AddNew(Events.MessageAccepted
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, "BB")
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseTransfer));

			using (var form = new FormForTest(consolExport))
			{
				form.Show();

				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.Cast<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Port Messaging");

				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				AssertMultilineASCIIEquals("Electronic Menu Items",
@"Port Messaging
   Import
      Secure Cont. Release
         Transfer
         Revoke
   Export
      Export Notification (BE)
      Dangerous Goods Notification (BE)
", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
			}
		}

		void AssertElectronicMenuItems_Belgium_Import()
		{
			var consolImport = Factory.New<ForwardingConsol>();
			consolImport.JK_TransportMode = Constants.TransportModes.Sea;
			consolImport.JK_AgentsReference = "";
			consolImport.JK_AgentType = Constants.AgentType.Agent;
			consolImport.JK_RL_NKLoadPort = "AUSYD";
			consolImport.JK_RL_NKDischargePort = "BEZEE";

			var shipmentImport = consolImport.Shipments.AddNew();
			shipmentImport.JS_TransportMode = Constants.TransportModes.Sea;
			shipmentImport.JS_RL_NKOrigin = "AUSYD";
			shipmentImport.JS_RL_NKDestination = "BEZEE";

			var transportLegImport = consolImport.Transports.Cast<Transport>().Single();
			transportLegImport.JW_TransportMode = Constants.TransportModes.Sea;
			transportLegImport.JW_RL_NKLoadPort = "AUSYD";
			transportLegImport.JW_RL_NKDiscPort = "BEZEE";

			var containerImport = consolImport.Containers.AddNew();
			containerImport.JC_ContainerNum = "AA";
			var packlineImport = shipmentImport.OuterPackLines.AddNew();
			containerImport.AddPackLine(packlineImport);

			var eventParameters = new[] { new KeyValuePair<string, string>("MST", "Secure Container Release") };
			containerImport.Logs.AddNew(Events.Authorised, eventParameters);
			eventParameters = new[] { new KeyValuePair<string, string>("MST", "Secure Container Release - Transfer") };
			containerImport.Logs.AddNew(Events.MessageAccepted, eventParameters);

			Factory.Save();

			using (var form = new FormForTest(consolImport))
			{
				form.Show();

				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.Cast<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Port Messaging");

				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				AssertMultilineASCIIEquals("Electronic Menu Items",
@"Port Messaging
   Import
      Dangerous Goods Notification (BE)
", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
			}

			containerImport.Logs.AddNew(Events.Authorised
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, "AA")
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, CertifiedPickupConstants.ParameterTypes.ContainerRelease)
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.ReleaseRight));

			var container2 = consolImport.Containers.AddNew();
			container2.JC_ContainerNum = "BB";
			container2.Logs.AddNew(Events.MessageRejected
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, "BB")
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.Transfer)
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Status, CertifiedPickupConstants.EventStatus.DeclinedByNextParty));

			var container3 = consolImport.Containers.AddNew();
			container3.JC_ContainerNum = "CC";
			container3.Logs.AddNew(Events.MessagePendingProcessing
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, "CC")
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.Transfer)
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Status, CertifiedPickupConstants.EventStatus.Transferred));

			Factory.Save();

			using (var form = new FormForTest(consolImport))
			{
				form.Show();

				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.Cast<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Port Messaging");

				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				AssertMultilineASCIIEquals("Electronic Menu Items",
@"Port Messaging
   Import
      Dangerous Goods Notification (BE)
      Certified Pick up (BE)
         Accept/Decline
         Transfer
         Revoke
", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
			}

			containerImport.Logs.AddNew(Events.Authorised
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, "AA")
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerRelease));

			container2.Logs.AddNew(Events.MessageAccepted
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, "BB")
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseTransfer));

			Factory.Save();

			using (var form = new FormForTest(consolImport))
			{
				form.Show();

				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.Cast<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Port Messaging");

				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				AssertMultilineASCIIEquals("Electronic Menu Items",
@"Port Messaging
   Import
      Dangerous Goods Notification (BE)
      Certified Pick up (BE)
         Accept/Decline
         Transfer
         Revoke
      Secure Cont. Release
         Transfer
         Revoke
", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
			}
		}

		void AssertElectronicMenuItems_Belgium_TransShip()
		{
			var consolTransShip = Factory.New<ForwardingConsol>();
			consolTransShip.JK_TransportMode = Constants.TransportModes.Sea;
			consolTransShip.JK_AgentsReference = "";
			consolTransShip.JK_AgentType = Constants.AgentType.Agent;
			consolTransShip.JK_RL_NKLoadPort = "AUSYD";
			consolTransShip.JK_RL_NKDischargePort = "BEZEE";

			var shipmentTransShip = consolTransShip.Shipments.AddNew();
			shipmentTransShip.JS_TransportMode = Constants.TransportModes.Sea;
			shipmentTransShip.JS_RL_NKOrigin = "AUSYD";
			shipmentTransShip.JS_RL_NKDestination = "BEZEE";

			var transportLeg1 = consolTransShip.Transports.Cast<Transport>().Single();
			transportLeg1.JW_TransportMode = Constants.TransportModes.Sea;
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "BEZEE";

			var transportLeg2 = consolTransShip.Transports.AddNew();
			transportLeg2.JW_TransportMode = Constants.TransportModes.Sea;
			transportLeg2.JW_LegOrder = 2;
			transportLeg2.JW_RL_NKLoadPort = "BEZEE";
			transportLeg2.JW_RL_NKDiscPort = "HKHKG";

			var containerTransShip = consolTransShip.Containers.AddNew();
			containerTransShip.JC_ContainerNum = "AA";
			var packlineTransShip = shipmentTransShip.OuterPackLines.AddNew();
			containerTransShip.AddPackLine(packlineTransShip);

			var eventParameters = new[] { new KeyValuePair<string, string>("MST", "Secure Container Release") };
			containerTransShip.Logs.AddNew(Events.Authorised, eventParameters);
			eventParameters = new[] { new KeyValuePair<string, string>("MST", "Secure Container Release - Transfer") };
			containerTransShip.Logs.AddNew(Events.MessageAccepted, eventParameters);

			Factory.Save();

			using (var form = new FormForTest(consolTransShip))
			{
				form.Show();

				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.Cast<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Port Messaging");

				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				AssertMultilineASCIIEquals("Electronic Menu Items",
@"Port Messaging
   Export
      Export Notification (BE)
", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
			}

			containerTransShip.Logs.AddNew(Events.Authorised
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, "AA")
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerRelease));

			var container2 = consolTransShip.Containers.AddNew();
			container2.JC_ContainerNum = "BB";
			container2.Logs.AddNew(Events.MessageAccepted
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, "BB")
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseTransfer));

			Factory.Save();

			using (var form = new FormForTest(consolTransShip))
			{
				form.Show();

				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.Cast<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Port Messaging");

				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				AssertMultilineASCIIEquals("Electronic Menu Items",
@"Port Messaging
   Import
      Secure Cont. Release
         Transfer
         Revoke
   Export
      Export Notification (BE)
", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
			}
		}

		public void TestElectronicMenuItems_Germany_DocumentAndFormMenus()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentsReference = "";
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "DEBRE";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "DEBRE";
			shipment.JS_RL_NKDestination = "AUSYD";

			var transportLeg = consol.Transports.Cast<Transport>().Single();
			transportLeg.JW_TransportMode = Constants.TransportModes.Sea;
			transportLeg.JW_RL_NKLoadPort = "DEBRE";
			transportLeg.JW_RL_NKDiscPort = "AUSYD";

			var transportLeg2 = consol.Transports.AddNew();
			transportLeg2.JW_TransportMode = Constants.TransportModes.Sea;
			transportLeg2.JW_LegOrder = 2;
			transportLeg2.JW_RL_NKLoadPort = "AUSYD";
			transportLeg2.JW_RL_NKDiscPort = "DEBRV";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AA";
			var packline = shipment.OuterPackLines.AddNew();
			container.AddPackLine(packline);

			Factory.Save();

			using (var form = new FormForTest(consol))
			{
				form.Show();

				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.Cast<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Port Messaging");

				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				AssertMultilineASCIIEquals("Electronic Menu Items",
@"Port Messaging
   Import
      Advanced Logistics Port Order (DE)
   Export
      Advanced Logistics Port Order (DE)
", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
			}
		}

		public void TestElectronicMenuItems_TMining_SecureContainerRelease()
		{
			var consolImport = Factory.New<ForwardingConsol>();
			consolImport.JK_TransportMode = Constants.TransportModes.Sea;
			consolImport.JK_AgentsReference = "";
			consolImport.JK_AgentType = Constants.AgentType.Agent;
			consolImport.JK_RL_NKLoadPort = "AUSYD";
			consolImport.JK_RL_NKDischargePort = "BEZEE";

			var containerImport = consolImport.Containers.AddNew();
			containerImport.JC_ContainerNum = "AA";

			var eventParameters = new[] { new KeyValuePair<string, string>("MST", "Secure Container Release") };
			containerImport.Logs.AddNew(Events.Authorised, eventParameters);
			eventParameters = new[] { new KeyValuePair<string, string>("MST", "Secure Container Release - Transfer") };
			containerImport.Logs.AddNew(Events.MessageAccepted, eventParameters);

			Factory.Save();

			using (var form = new FormForTest(consolImport))
			{
				form.Show();

				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.Cast<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Port Messaging");

				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				AssertMultilineASCIIEquals("Electronic Menu Items",
@"Port Messaging
   Import
      Dangerous Goods Notification (BE)
", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
			}

			containerImport.Logs.AddNew(Events.Authorised
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, "AA")
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, CertifiedPickupConstants.ParameterTypes.ContainerRelease)
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.ReleaseRight));

			var container2 = consolImport.Containers.AddNew();
			container2.JC_ContainerNum = "BB";
			container2.Logs.AddNew(Events.MessageRejected
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, "BB")
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.Transfer)
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Status, CertifiedPickupConstants.EventStatus.DeclinedByNextParty));

			var container3 = consolImport.Containers.AddNew();
			container3.JC_ContainerNum = "CC";
			container3.Logs.AddNew(Events.MessagePendingProcessing
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, "CC")
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.Transfer)
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Status, CertifiedPickupConstants.EventStatus.Transferred));

			Factory.Save();

			using (var form = new FormForTest(consolImport))
			{
				form.Show();

				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.Cast<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Port Messaging");

				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				AssertMultilineASCIIEquals("Electronic Menu Items",
@"Port Messaging
   Import
      Dangerous Goods Notification (BE)
      Certified Pick up (BE)
         Accept/Decline
         Transfer
         Revoke
", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
			}

			containerImport.Logs.AddNew(Events.Authorised
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, "AA")
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerRelease));

			container2.Logs.AddNew(Events.MessageAccepted
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, "BB")
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseTransfer));

			Factory.Save();

			using (var form = new FormForTest(consolImport))
			{
				form.Show();

				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.Cast<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Port Messaging");

				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				AssertMultilineASCIIEquals("Electronic Menu Items",
@"Port Messaging
   Import
      Dangerous Goods Notification (BE)
      Certified Pick up (BE)
         Accept/Decline
         Transfer
         Revoke
      Secure Cont. Release
         Transfer
         Revoke
", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
			}
		}

		public void TestElectronicMenuItems_Export_DocumentAndFormMenusMerged()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.NewZealand))
			{
				var departureCTO = Factory.NewWithValidTestData<OrgHeader>();
				departureCTO.OH_FullName = "YUMMY";
				departureCTO.MainAddress.OA_RL_NKRelatedPortCode = "NZAKL";
				departureCTO.MainAddress.Address1 = "Unit 200";
				departureCTO.MainAddress.Address2 = "55 Why Lane";
				departureCTO.MainAddress.City = "Antwerp";
				departureCTO.MainAddress.Postcode = "2000";
				departureCTO.MainAddress.OA_RN_NKCountryCode = "NZ";

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_ConsolMode = Constants.ContainerModes.LCL;
				consol.JK_AgentsReference = "";
				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "FRSXB";
				consol.JK_OA_DepartureCTOAddress = departureCTO.MainAddress.PK;

				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "TESU1234567";
				container.JC_GrossWeight = 1000;
				container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
				container.JC_GrossWeightVerificationDateTime = new ZDateTime(2024, 3, 1);
				container.JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
				container.JC_ContainerCount = 2;

				Factory.Save();

				using (var form = new FormForTest(consol))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Port Messaging");

					AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertMultilineASCIIEquals("Electronic Menu Items",
						@"Port Messaging
   Export
      Export Pre-Advice Notification (NZ)
", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestElectronicMenuItems_Netherlands_Cargonaut()
		{
			using (PortMessagingRegistry.Instance.AllowToSendExportNotificationToCargonaut.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Netherlands))
			{
				var carrier = Factory.New<OrgHeader>();
				carrier.OH_IsShippingLine = true;

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
				consol.JK_MasterBillNum = "12345678901";
				consol.JK_RL_NKLoadPort = "NLAMS";

				var number1 = Factory.New<CusEntryNumber>();
				number1.CE_RN_NKCountryCode = Constants.CountryCodes.Netherlands;
				number1.CE_EntryType = "MRN";
				number1.CE_EntryNum = "MRN01";

				var shipment = consol.Shipments.AddNew();
				shipment.Numbers.Add(number1);

				using (var form = new FormForTest(consol))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Port Messaging");

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertMultilineASCIIEquals("Electronic Menu Items for air consol",
						@"Port Messaging
   Export
      Export Notification Cargonaut (NL)",
						electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		#endregion

		#region Implementation

		class PortMessageMenuItemWrapper
		{
			public PortMessageMenuItemWrapper(string menuPath, string eventReference, PortMessagingManager.MessageType messageType)
			{
				this.menuPath = menuPath;
				this.eventReference = eventReference;
				this.messageType = messageType;
			}

			readonly string menuPath;
			readonly string eventReference;
			readonly PortMessagingManager.MessageType messageType;

			public string MenuPath { get { return menuPath; } }
			public string EventReference { get { return eventReference; } }
			public PortMessagingManager.MessageType MessageType { get { return messageType; } }
		}

		const string hdsMenu = "Port Messaging|Port Order with HDS (DEHAM)";
		const string portOrderMenu = "Port Messaging|Port Order (DEHAM)";
		const string accessDeniedMenu = "Port Messaging|DASOKY Access denied, click this menu for details.";

		static List<PortMessageMenuItemWrapper> OrderMenuItems
		{
			get
			{
				if (orderMenuItems == null)
				{
					orderMenuItems = new List<PortMessageMenuItemWrapper>();
					orderMenuItems.Add(new PortMessageMenuItemWrapper(hdsMenu + "|Send Port Order With HDS", "Port Order with HDS", PortMessagingManager.MessageType.PortOrderWithHDS));
					orderMenuItems.Add(new PortMessageMenuItemWrapper(hdsMenu + "|Cancellation Because of Errors", "Port Order with HDS Cancellation Because of Errors", PortMessagingManager.MessageType.PortOrderWithHDSCancellationBecauseOfErrors));
					orderMenuItems.Add(new PortMessageMenuItemWrapper(hdsMenu + "|Cancellation of Export on Exit", "Port Order with HDS Export on Exit Cancellation", PortMessagingManager.MessageType.PortOrderWithHDSCancellationOnExit));
					orderMenuItems.Add(new PortMessageMenuItemWrapper(hdsMenu + "|Forwarding to Another Port", "Port Order with HDS Cancellation as Forwarding to Another Port", PortMessagingManager.MessageType.PortOrderWithHDSForwardingCancellation));
					orderMenuItems.Add(new PortMessageMenuItemWrapper(portOrderMenu + "|Send Port Order for Inbound Delivery", "Port Order for Inbound Delivery", PortMessagingManager.MessageType.PortOrderInbound));
					orderMenuItems.Add(new PortMessageMenuItemWrapper(portOrderMenu + "|Send Port Order for Inbound Delivery Cancellation", "Port Order for Inbound Delivery Cancellation", PortMessagingManager.MessageType.PortOrderInbound));
					orderMenuItems.Add(new PortMessageMenuItemWrapper(portOrderMenu + "|Send Port Order for Outbound Delivery", "Port Order for Outbound Delivery", PortMessagingManager.MessageType.PortOrderOutbound));
					orderMenuItems.Add(new PortMessageMenuItemWrapper(portOrderMenu + "|Send Port Order for Outbound Delivery Cancellation", "Port Order for Outbound Delivery Cancellation", PortMessagingManager.MessageType.PortOrderOutbound));
					orderMenuItems.Add(new PortMessageMenuItemWrapper(portOrderMenu + "|Send Stop Request", "Stop Request", PortMessagingManager.MessageType.StopRequest));
					orderMenuItems.Add(new PortMessageMenuItemWrapper(portOrderMenu + "|Send Gate Pass", "Gate Pass", PortMessagingManager.MessageType.GatePass));
					orderMenuItems.Add(new PortMessageMenuItemWrapper(portOrderMenu + "|Send Gate Pass Cancellation", "Gate Pass Cancellation", PortMessagingManager.MessageType.GatePass));
					orderMenuItems.Add(new PortMessageMenuItemWrapper(portOrderMenu + "|Send Request for Port Services", "Request for Port Services", PortMessagingManager.MessageType.RequestForPortServices));
					orderMenuItems.Add(new PortMessageMenuItemWrapper(portOrderMenu + "|Send Request for Port Services Cancellation", "Request for Port Services Cancellation", PortMessagingManager.MessageType.RequestForPortServices));
					orderMenuItems.Add(new PortMessageMenuItemWrapper(portOrderMenu + "|Send Certificate of Obligation", "Certificate of Obligation", PortMessagingManager.MessageType.CertificateOfObligation));
					orderMenuItems.Add(new PortMessageMenuItemWrapper(portOrderMenu + "|Send Certificate of Obligation Cancellation", "Certificate of Obligation Cancellation", PortMessagingManager.MessageType.CertificateOfObligation));
					orderMenuItems.Add(new PortMessageMenuItemWrapper(portOrderMenu + "|Send Request for Rail Discharge", "Request for Rail Discharge", PortMessagingManager.MessageType.RequestForRailDischarge));
					orderMenuItems.Add(new PortMessageMenuItemWrapper(portOrderMenu + "|Send Request for Rail Discharge Cancellation", "Request for Rail Discharge Cancellation", PortMessagingManager.MessageType.RequestForRailDischarge));
				}

				return orderMenuItems;
			}
		}

		static List<PortMessageMenuItemWrapper> orderMenuItems;

		void AssertMessageSending(BusinessObject parent, string menuItemText, string expectedMessage, PortMessagingManager.MessageType messageType, bool scenarioHasNoMessageToCancel = false, DialogResult userInput = DialogResult.Yes)
		{
			var isOrder = !expectedMessage.Contains(" Cancellation");

			if (!isOrder)
			{
				parent.GetLogs().AddNew(Events.MessageSent, "Message Sent|DEP=Dakosy|MST=" + PortMessagingStatusRetriever.GetLogReferenceFromMessageType(messageType));
				parent.Factory.Save();

				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

				parent.GetLogs().AddNew(Events.MessageAccepted, "Message Accepted|DEP=Dakosy|MST=" + PortMessagingStatusRetriever.GetLogReferenceFromMessageType(messageType));
				parent.Factory.Save();

				if (scenarioHasNoMessageToCancel)
				{
					TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

					parent.GetLogs().AddNew(Events.MessageRejected, "Message Rejected|DEP=Dakosy|MST=" + PortMessagingStatusRetriever.GetLogReferenceFromMessageType(messageType));
					parent.Factory.Save();
				}
			}

			using (var form = new FormForTest(parent))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(userInput);

				var sendMenu = FindMenuItem(form, menuItemText);
				sendMenu.PerformClick();

				var expectedNotification = isOrder
					? "Message queued for sending to DAKOSY."
					: userInput == DialogResult.No
						? "Cancellation message aborted."
						: "Message Cancellation queued for sending to DAKOSY.";

				AssertEquals(expectedNotification, UnitTestUserNotification.Instance.LastMessage.Text);

				var expectedEventCode = isOrder
					? Events.MessageSent.Code
					: Events.MessageWithdrawCancelRequest.Code;

				Action<BusinessObject> assertLogExists = bizO
					=> Assert(bizO.GetLogs().Find(log => HasMatchingLog(log, expectedEventCode, expectedMessage)).Any());

				assertLogExists(parent);
				assertLogExists(new BusinessObjectFactory().Load(parent.GetType(), parent.PK));
			}
		}

		bool HasMatchingLog(StmALog log, string expectedEventCode, string expectedMessage)
		{
			return log.SL_SE_NKEvent == expectedEventCode
				&& log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType].Equals(expectedMessage)
				&& log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department].Equals("DAKOSY", StringComparison.OrdinalIgnoreCase);
		}

		void AssertMenuItemVisible(BusinessObject parent, string menuItemName, bool isVisible)
		{
			using (var form = new FormForTest(parent))
			{
				form.Show();
				var portMessagingItem = FindMenuItem(form, "Port Messaging");
				portMessagingItem.OnPopup(EventArgs.Empty);
				var menuItem = FindMenuItem(form, menuItemName);
				AssertEquals(isVisible, menuItem.Visible);
			}
		}

		void AssertMenuItemIsNull(BusinessObject parent, string menuItemName)
		{
			using (var form = new FormForTest(parent))
			{
				form.Show();
				var portMessagingItem = FindMenuItem(form, "Port Messaging");
				portMessagingItem.OnPopup(EventArgs.Empty);
				var menuItem = FindMenuItem(form, menuItemName);
				AssertNull(menuItem);
			}
		}

		void AssertSecurityMessageMenuItem(BusinessObject parent)
		{
			using (var form = new FormForTest(parent))
			{
				form.Show();
				var menuItem = FindMenuItem(form, accessDeniedMenu);
				AssertNotNull(menuItem);

				menuItem.PerformClick();
				AssertContains(Env.Security.PortMessaging.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		void AssertSecurityMessageMenuItemIsNull(BusinessObject parent)
		{
			using (var form = new FormForTest(parent))
			{
				form.Show();
				var menuItem = FindMenuItem(form, accessDeniedMenu);
				AssertNull(menuItem);
			}
		}

		void AssertResendAfterCancellation(BusinessObject parent)
		{
			using (var form = new FormForTest(parent))
			{
				form.Show();

				var sendMenu = FindMenuItem(form, "Port Messaging|Port Order with HDS (DEHAM)|Send Port Order With HDS");
				sendMenu.PerformClick();

				var expectedError = "Dakosy status is Pending.\r\nAn Acceptance or Rejection must be received before you can send additional messages to Dakosy.";
				AssertEquals(expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			return new PortMessagingPlugIn(Factory.New<ForwardingConsol>());
		}

		protected override void SetUp()
		{
			base.SetUp();

			isPortMessagingAllowedDefault = Env.Security.PortMessaging.IsAllowed;
			Env.Security.PortMessaging.IsAllowed = true;
			countryOverride = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany);
		}

		protected override void TearDown()
		{
			base.TearDown();

			Env.Security.PortMessaging.IsAllowed = isPortMessagingAllowedDefault;
			countryOverride.Dispose();
		}

		IDisposable countryOverride;
		bool isPortMessagingAllowedDefault;

		ForwardingConsol GetSecondSavedConsol()
		{
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_Code = "maersk2";
			shippingLine.OH_FullName = "shipping line";

			var forwarder = Factory.New<OrgHeader>();
			forwarder.OH_Code = "kermit2";
			forwarder.OH_FullName = "shipper";

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());

			var cfs = Factory.New<OrgHeader>();
			cfs.OH_Code = "cfs2";

			var cto = Factory.New<OrgHeader>();
			cto.OH_Code = "cto2";

			forwarder.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "dakosysendercode", Constants.CountryCodes.Germany);
			forwarder.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ZAP, "zapsendercode", Constants.CountryCodes.Germany);
			cfs.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "dakosysfc", Constants.CountryCodes.Germany);
			cto.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "BRT", Constants.CountryCodes.Germany);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKDischargePort = "PLGDY";
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = forwarder.MainAddress.PK;
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = cto.MainAddress.PK;
			consol.JK_MasterBillNum = "billno0000-00112";

			consol.ShippingLine.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ZAP, "ZZZ", Constants.CountryCodes.Germany);
			consol.ShippingLine.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "DDD", Constants.CountryCodes.Germany);

			var transport = consol.Transports[0];
			transport.JW_JX = CreateNewSailing(vessel, "voyageNo", "DEHAM", "AUSYD", ZDateTime.Today.AddDays(-7), ZDateTime.Today.AddDays(1)).PK;
			transport.JW_Vessel = vessel.RV_FK;
			transport.JW_ETD = ZDateTime.BrettsBirthday;

			var origin1 = consol.Voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "UAODT";
			origin1.JA_Berth = "AAA";
			var origin2 = consol.Voyage.Origins.OfType<VoyageOrigin>().First(o => o.JA_RL_NKPortOfLoading.Equals("DEHAM"));
			origin2.JA_RL_NKPortOfLoading = "DEHAM";
			origin2.JA_Berth = "BBB";
			origin2.JA_DepartReference = "REF111";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "DEHAM";
			shipment.JS_RL_NKDestination = "PLGDY";

			var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);
			shipmentPortMessaging.JSM_MovementReferenceNumber = "15DE333444455555E2";
			shipmentPortMessaging.JSM_ForwardingCustomsOfficeCode = "83031478";

			Factory.Save();

			return consol;
		}

		ForwardingConsol GetSavedConsol()
		{
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_Code = "maersk";
			shippingLine.OH_FullName = "shipping line";

			var forwarder = Factory.New<OrgHeader>();
			forwarder.OH_Code = "kermit";
			forwarder.OH_FullName = "shipper";

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());

			var cfs = Factory.New<OrgHeader>();
			cfs.OH_Code = "cfs";

			var cto = Factory.New<OrgHeader>();
			cto.OH_Code = "cto";

			forwarder.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "dakosysendercode", Constants.CountryCodes.Germany);
			forwarder.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ZAP, "zapsendercode", Constants.CountryCodes.Germany);
			cfs.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "dakosysfc", Constants.CountryCodes.Germany);
			cto.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "BRT", Constants.CountryCodes.Germany);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKDischargePort = "PLGDY";
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = forwarder.MainAddress.PK;
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = cto.MainAddress.PK;
			consol.JK_MasterBillNum = "billno0000-00111";

			consol.ShippingLine.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ZAP, "ZZZ", Constants.CountryCodes.Germany);
			consol.ShippingLine.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "DDD", Constants.CountryCodes.Germany);

			var transport = consol.Transports[0];
			transport.JW_JX = CreateNewSailing(vessel, "voyageNo", "DEHAM", "AUSYD", ZDateTime.Today.AddDays(-7), ZDateTime.Today.AddDays(1)).PK;
			transport.JW_Vessel = vessel.RV_FK;
			transport.JW_ETD = ZDateTime.BrettsBirthday;

			var origin1 = consol.Voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "UAODS";
			origin1.JA_Berth = "AAA";
			var origin2 = consol.Voyage.Origins.OfType<VoyageOrigin>().First(o => o.JA_RL_NKPortOfLoading.Equals("DEHAM"));
			origin2.JA_RL_NKPortOfLoading = "DEHAM";
			origin2.JA_Berth = "BBB";
			origin2.JA_DepartReference = "REF111";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "DEHAM";
			shipment.JS_RL_NKDestination = "PLGDY";

			var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);
			shipmentPortMessaging.JSM_MovementReferenceNumber = "15DE333444455555E2";
			shipmentPortMessaging.JSM_ForwardingCustomsOfficeCode = "83031478";

			Factory.Save();

			return consol;
		}

		JobSailing CreateNewSailing(RefVessel vessel, ZString voyageNo, ZString loadPort, ZString dischargePort, ZDateTime departureTime, ZDateTime arrivalTime)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = voyageNo;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = loadPort;
			origin.JA_E_DEP = departureTime;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = dischargePort;
			destination.JB_E_ARV = arrivalTime;
			voyage.GenerateSailings();

			var result = voyage.Sailings.AddNew();
			result.JX_JB = destination.PK;
			result.JX_JA = origin.PK;
			return result;
		}

		static MenuItem FindMenuItem(ZForm form, string path)
		{
			return !string.IsNullOrEmpty(path)
				? FindMenuItem(form.Menu.MenuItems, path.Split('|'))
				: null;
		}

		static MenuItem FindMenuItem(Menu.MenuItemCollection items, string[] path)
		{
			if (path == null || path.Length == 0)
			{
				return null;
			}

			MenuItem menuItem = null;

			foreach (MenuItem item in items)
			{
				var itemText = item.Text.Replace("&", "");

				if (itemText == path[0])
				{
					menuItem = item;
					break;
				}
			}

			return path.Length == 1 || menuItem == null
				? menuItem
				: FindMenuItem(menuItem.MenuItems, path.Skip(1).ToArray());
		}

		class FormForTest : ZTemplateForm
		{
			public FormForTest(BusinessObject businessEntity)
				: base(businessEntity)
			{
				PlugIns.Add(ControllerIDs.PortMessaging);
			}
		}

		#endregion
	}
}
