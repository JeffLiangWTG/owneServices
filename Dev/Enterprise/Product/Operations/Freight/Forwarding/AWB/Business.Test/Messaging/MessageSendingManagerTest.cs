using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Forwarding.AWB.Messaging.Testing
{
	sealed class MessageSendingManagerTest : TestCaseWithFactory
	{
		public void TestDuplicateHAWBsCannotBeSent()
		{
			var mawbHeader = Factory.New<ExportAWBHeader>();
			mawbHeader.EH_WayBillNumber = "081-56789023";

			mawbHeader.ChildBills.AddNew().EH_WayBillNumber = "1234567-1";
			mawbHeader.ChildBills.AddNew().EH_WayBillNumber = "1234567-3";
			mawbHeader.ChildBills.AddNew().EH_WayBillNumber = "1234567-5";
			mawbHeader.ChildBills.AddNew().EH_WayBillNumber = "1234567-7";
			mawbHeader.ChildBills.AddNew().EH_WayBillNumber = "1234567-9";
			mawbHeader.ChildBills.AddNew().EH_WayBillNumber = "1234567-8";
			mawbHeader.ChildBills.AddNew().EH_WayBillNumber = "1234567-7";
			mawbHeader.ChildBills.AddNew().EH_WayBillNumber = "1234567-6";
			mawbHeader.ChildBills.AddNew().EH_WayBillNumber = "1234567-5";
			mawbHeader.ChildBills.AddNew().EH_WayBillNumber = "1234567-4";
			mawbHeader.ChildBills.AddNew().EH_WayBillNumber = "1234567-6";
			mawbHeader.ChildBills.AddNew().EH_WayBillNumber = "1234567-2";
			mawbHeader.ChildBills.AddNew().EH_WayBillNumber = "1234567-6";
			mawbHeader.ChildBills.AddNew().EH_WayBillNumber = "1234568-1";
			mawbHeader.ChildBills.AddNew().EH_WayBillNumber = "1234568-1";
			mawbHeader.ChildBills.AddNew().EH_WayBillNumber = "1234568-5";
			mawbHeader.ChildBills.AddNew().EH_WayBillNumber = "1234568-2";
			mawbHeader.ChildBills.AddNew().EH_WayBillNumber = "1234568-2";
			mawbHeader.ChildBills.AddNew().EH_WayBillNumber = "1234568-2";

			string message;
			var result = new MessageSendingManager(mawbHeader).SendAll(out message);
			AssertEquals("Message send should fail and return false result", false, result);
			AssertMultilineASCIIEquals("Failure Message", @"
ERROR: Cannot send a MAWB with duplicate HAWB numbers on it.

The following HAWBs occurred multiple times on this MAWB:
1234567-5 (x2), 1234567-6 (x3), 1234567-7 (x2), 1234568-1 (x2),
1234568-2 (x3).
".Trim(), message);
		}

		public void TestSentWithOrgContactAddsAMessageWithTheContactsEmailAddressAttached()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "HANBARLAX";

			var contact = organisation.Contacts.AddNew();
			contact.OC_ContactName = "Top Cat";
			contact.OC_Email = "top.cat@hanna-barbera.com";

			var dataCreator = new AWBTestDataCreator(Factory);
			var mawbHeader = dataCreator.MAWBHeader;

			AssertEquals("Precondition: mawbHeader.EH_AWBStatus", AWBMessagingStatusList.Codes.NotSent, mawbHeader.EH_AWBStatus);

			string message;
			var result = new MessageSendingManager(mawbHeader, contact).SendAll(out message);
			AssertEquals("manager.ReSendThisOnly(out message) Result", true, result);
			AssertEquals("manager.ReSendThisOnly(out message) Message", "FWB sent for MAWB: 006-96667465", message);
			AssertEquals("mawbHeader.EH_AWBStatus", AWBMessagingStatusList.Codes.SentToAirlines, mawbHeader.EH_AWBStatus);
			AssertMessageSent(mawbHeader, "mawbHeader", CIMEDIMessage.MessageTypes.Sent.FWB, AWBTestDataCreator.MAWBSampleFWB);

			ZString sendersEmailAddress = new MessageSenderManager(mawbHeader.Messages[0]).SendersEmailAddress;
			AssertEquals("Senders Email Address", "top.cat@hanna-barbera.com", sendersEmailAddress);
		}

		public void TestFailureResentsAllSentStatuses()
		{
			var dataCreator = new AWBTestDataCreator(Factory);
			var mawbHeader = dataCreator.MAWBHeader;
			var hawbHeader1 = dataCreator.SetupHAWBHeader1();
			var hawbHeader2 = dataCreator.SetupHAWBHeader2();

			AssertEquals("Precondition: mawbHeader.EH_AWBStatus", AWBMessagingStatusList.Codes.NotSent, mawbHeader.EH_AWBStatus);
			AssertEquals("Precondition: hawbHeader1.EH_AWBStatus", AWBMessagingStatusList.Codes.NotSent, hawbHeader1.EH_AWBStatus);
			AssertEquals("Precondition: hawbHeader2.EH_AWBStatus", AWBMessagingStatusList.Codes.NotSent, hawbHeader2.EH_AWBStatus);

			hawbHeader2.EH_AWBType = AWBTypeList.Codes.UndefinedMaster; // This should cause it to blow up on the last sent AWB.

			string message;
			var result = new MessageSendingManager(mawbHeader).SendAll(out message);
			AssertEquals("manager.ReSendThisOnly(out message) Result", false, result);
			AssertEquals("manager.ReSendThisOnly(out message) Message", MessageSendingManager.FunctionalityNotImplementedYet, message);
			AssertEquals("mawbHeader.EH_AWBStatus", AWBMessagingStatusList.Codes.NotSent, mawbHeader.EH_AWBStatus);
			AssertEquals("hawbHeader1.EH_AWBStatus", AWBMessagingStatusList.Codes.NotSent, hawbHeader1.EH_AWBStatus);
			AssertEquals("hawbHeader2.EH_AWBStatus", AWBMessagingStatusList.Codes.NotSent, hawbHeader2.EH_AWBStatus);

			AssertEquals("mawbHeader.Messages.Count", 0, mawbHeader.Messages.Count);
			AssertEquals("hawbHeader1.Messages.Count", 0, hawbHeader1.Messages.Count);
			AssertEquals("hawbHeader2.Messages.Count", 0, hawbHeader2.Messages.Count);
		}

		public void TestSendAllWhenAllShouldSend()
		{
			var dataCreator = new AWBTestDataCreator(Factory);
			var mawbHeader = dataCreator.MAWBHeader;
			var hawbHeader1 = dataCreator.SetupHAWBHeader1();
			var hawbHeader2 = dataCreator.SetupHAWBHeader2();

			AssertEquals("Precondition: mawbHeader.EH_AWBStatus", AWBMessagingStatusList.Codes.NotSent, mawbHeader.EH_AWBStatus);
			AssertEquals("Precondition: hawbHeader1.EH_AWBStatus", AWBMessagingStatusList.Codes.NotSent, hawbHeader1.EH_AWBStatus);
			AssertEquals("Precondition: hawbHeader2.EH_AWBStatus", AWBMessagingStatusList.Codes.NotSent, hawbHeader2.EH_AWBStatus);

			string message;
			var result = new MessageSendingManager(mawbHeader).SendAll(out message);
			AssertEquals("manager.ReSendThisOnly(out message) Result", true, result);
			AssertEquals("manager.ReSendThisOnly(out message) Message", "FWB + 2 FHLs sent for MAWB: 006-96667465", message);
			AssertEquals("mawbHeader.EH_AWBStatus", AWBMessagingStatusList.Codes.SentToAirlines, mawbHeader.EH_AWBStatus);
			AssertEquals("hawbHeader1.EH_AWBStatus", AWBMessagingStatusList.Codes.SentToAirlines, hawbHeader1.EH_AWBStatus);
			AssertEquals("hawbHeader2.EH_AWBStatus", AWBMessagingStatusList.Codes.SentToAirlines, hawbHeader2.EH_AWBStatus);

			AssertMessageSent(mawbHeader, "mawbHeader", CIMEDIMessage.MessageTypes.Sent.FWB, AWBTestDataCreator.MAWBSampleFWB);
			AssertMessageSent(hawbHeader1, "hawbHeader1", CIMEDIMessage.MessageTypes.Sent.FHL, AWBTestDataCreator.HAWBSampleFHL1);
			AssertMessageSent(hawbHeader2, "hawbHeader2", CIMEDIMessage.MessageTypes.Sent.FHL, AWBTestDataCreator.HAWBSampleFHL2);
		}

		public void TestSendAllWhenOnlySomeShouldSend()
		{
			var dataCreator = new AWBTestDataCreator(Factory);
			var mawbHeader = dataCreator.MAWBHeader;
			var hawbHeader1 = dataCreator.SetupHAWBHeader1();
			var hawbHeader2 = dataCreator.SetupHAWBHeader2();

			mawbHeader.EH_AWBStatus = AWBMessagingStatusList.Codes.NotSent;
			hawbHeader1.EH_AWBStatus = AWBMessagingStatusList.Codes.ErrorRejected;
			hawbHeader2.EH_AWBStatus = AWBMessagingStatusList.Codes.SentToAirlines;

			string message;
			var result = new MessageSendingManager(mawbHeader).SendAll(out message);
			AssertEquals("manager.ReSendThisOnly(out message) Result", true, result);
			AssertEquals("manager.ReSendThisOnly(out message) Message", "FWB + 1 FHL sent for MAWB: 006-96667465", message);
			AssertEquals("mawbHeader.EH_AWBStatus", AWBMessagingStatusList.Codes.SentToAirlines, mawbHeader.EH_AWBStatus);
			AssertEquals("hawbHeader1.EH_AWBStatus", AWBMessagingStatusList.Codes.SentToAirlines, hawbHeader1.EH_AWBStatus);
			AssertEquals("hawbHeader2.EH_AWBStatus", AWBMessagingStatusList.Codes.SentToAirlines, hawbHeader2.EH_AWBStatus);

			AssertMessageSent(mawbHeader, "mawbHeader", CIMEDIMessage.MessageTypes.Sent.FWB, AWBTestDataCreator.MAWBSampleFWB);
			AssertMessageSent(hawbHeader1, "hawbHeader1", CIMEDIMessage.MessageTypes.Sent.FHL, AWBTestDataCreator.HAWBSampleFHL1);
			AssertEquals("hawbHeader2.Messages.Count", 0, hawbHeader2.Messages.Count);
		}

		public void TestReSendAll()
		{
			var dataCreator = new AWBTestDataCreator(Factory);
			var mawbHeader = dataCreator.MAWBHeader;
			var hawbHeader1 = dataCreator.SetupHAWBHeader1();
			var hawbHeader2 = dataCreator.SetupHAWBHeader2();

			mawbHeader.EH_AWBStatus = AWBMessagingStatusList.Codes.NotSent;
			hawbHeader1.EH_AWBStatus = AWBMessagingStatusList.Codes.ErrorRejected;
			hawbHeader2.EH_AWBStatus = AWBMessagingStatusList.Codes.SentToAirlines;

			string message;
			var result = new MessageSendingManager(mawbHeader).ReSendAll(out message);
			AssertEquals("manager.ReSendThisOnly(out message) Result", true, result);
			AssertEquals("manager.ReSendThisOnly(out message) Message", "FWB + 2 FHLs sent for MAWB: 006-96667465", message);
			AssertEquals("mawbHeader.EH_AWBStatus", AWBMessagingStatusList.Codes.SentToAirlines, mawbHeader.EH_AWBStatus);
			AssertEquals("hawbHeader1.EH_AWBStatus", AWBMessagingStatusList.Codes.SentToAirlines, hawbHeader1.EH_AWBStatus);
			AssertEquals("hawbHeader2.EH_AWBStatus", AWBMessagingStatusList.Codes.SentToAirlines, hawbHeader2.EH_AWBStatus);

			AssertMessageSent(mawbHeader, "mawbHeader", CIMEDIMessage.MessageTypes.Sent.FWB, AWBTestDataCreator.MAWBSampleFWB);
			AssertMessageSent(hawbHeader1, "hawbHeader1", CIMEDIMessage.MessageTypes.Sent.FHL, AWBTestDataCreator.HAWBSampleFHL1);
			AssertMessageSent(hawbHeader2, "hawbHeader2", CIMEDIMessage.MessageTypes.Sent.FHL, AWBTestDataCreator.HAWBSampleFHL2);
		}

		public void TestReSendThisHAWBOnly()
		{
			var dataCreator = new AWBTestDataCreator(Factory);
			dataCreator.SetupHAWBHeader1();
			var hawbHeader = dataCreator.HAWBHeader1;
			AssertEquals("Precondition: hawbHeader.EH_AWBStatus", AWBMessagingStatusList.Codes.NotSent, hawbHeader.EH_AWBStatus);

			string message;
			var result = new MessageSendingManager(hawbHeader).ReSendThisOnly(out message);
			AssertEquals("manager.ReSendThisOnly(out message) Result", true, result);
			AssertEquals("manager.ReSendThisOnly(out message) Message", "1 FHL sent for MAWB: 006-96667465", message);
			AssertEquals("hawbHeader.EH_AWBStatus", AWBMessagingStatusList.Codes.SentToAirlines, hawbHeader.EH_AWBStatus);

			AssertEquals(1, hawbHeader.Messages.Count);
			var sentMessage = hawbHeader.Messages[0];

			AssertEquals("sentMessage.EM_ApplicationCode", CIMEDIMessage.ApplicationCodes.CIM, sentMessage.EM_ApplicationCode);
			AssertEquals("sentMessage.EM_LinkTable", hawbHeader.TableName, sentMessage.EM_LinkTable);
			AssertEquals("sentMessage.EM_LinkUniqueID", hawbHeader.PK, sentMessage.EM_LinkUniqueID);
			AssertEquals("sentMessage.EM_MessageType", CIMEDIMessage.MessageTypes.Sent.FHL, sentMessage.EM_MessageType);
			AssertEquals("sentMessage.EM_MessageSubType", CIMEDIMessage.MessageSubTypes.Standalone, sentMessage.EM_MessageSubType);
			AssertEquals("sentMessage.EM_ReceiveTransmit", CIMEDIMessage.Direction.Transmit, sentMessage.EM_ReceiveTransmit);
			AssertEquals("sentMessage.EM_MessageNum", "00000001", sentMessage.EM_MessageNum);
			AssertEquals("sentMessage.EM_ApplicationReference", "00696667465", sentMessage.EM_ApplicationReference);
			AssertEquals("sentMessage.EM_Status", CIMEDIMessage.Status.Queued, sentMessage.EM_Status);
			AssertEquals("sentMessage.EM_GB", GlbBranch.CurrentBranch.PK, sentMessage.EM_GB);
			AssertMultilineASCIIEquals("sentMessage.EM_MessageText", AWBTestDataCreator.HAWBSampleFHL1.Trim(), sentMessage.EM_MessageText.Trim());

			var sentInterchange = sentMessage.Interchange;
			AssertNotNull("sentMessage.Interchange", sentInterchange);
			AssertEquals("sentInterchange.EI_HeaderText", ZString.Empty, sentInterchange.EI_HeaderText);
			AssertEquals("sentInterchange.EI_BodyText", sentMessage.EM_MessageText, sentInterchange.EI_BodyText);
			AssertEquals("sentInterchange.EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, sentInterchange.EI_ReceiveTransmit);
			AssertEquals("sentInterchange.EI_Status", EDIInterchange.Status.Queued, sentInterchange.EI_Status);
			AssertEquals("sentInterchange.EI_From", "EDIDAT", sentInterchange.EI_From);
			AssertEquals("sentInterchange.EI_To", "EDI CCN", sentInterchange.EI_To);
		}

		public void TestReSendThisMAWBOnly()
		{
			var dataCreator = new AWBTestDataCreator(Factory);
			var mawbHeader = dataCreator.MAWBHeader;
			AssertEquals("Precondition: mawbHeader.EH_AWBStatus", AWBMessagingStatusList.Codes.NotSent, mawbHeader.EH_AWBStatus);

			string message;
			var result = new MessageSendingManager(mawbHeader).ReSendThisOnly(out message);
			AssertEquals("manager.ReSendThisOnly(out message) Result", true, result);
			AssertEquals("manager.ReSendThisOnly(out message) Message", "FWB sent for MAWB: 006-96667465", message);
			AssertEquals("mawbHeader.EH_AWBStatus", AWBMessagingStatusList.Codes.SentToAirlines, mawbHeader.EH_AWBStatus);

			AssertEquals(1, mawbHeader.Messages.Count);
			var sentMessage = mawbHeader.Messages[0];

			AssertEquals("sentMessage.EM_ApplicationCode", CIMEDIMessage.ApplicationCodes.CIM, sentMessage.EM_ApplicationCode);
			AssertEquals("sentMessage.EM_LinkTable", mawbHeader.TableName, sentMessage.EM_LinkTable);
			AssertEquals("sentMessage.EM_LinkUniqueID", mawbHeader.PK, sentMessage.EM_LinkUniqueID);
			AssertEquals("sentMessage.EM_MessageType", CIMEDIMessage.MessageTypes.Sent.FWB, sentMessage.EM_MessageType);
			AssertEquals("sentMessage.EM_MessageSubType", CIMEDIMessage.MessageSubTypes.Standalone, sentMessage.EM_MessageSubType);
			AssertEquals("sentMessage.EM_ReceiveTransmit", CIMEDIMessage.Direction.Transmit, sentMessage.EM_ReceiveTransmit);
			AssertEquals("sentMessage.EM_MessageNum", "00000001", sentMessage.EM_MessageNum);
			AssertEquals("sentMessage.EM_ApplicationReference", "00696667465", sentMessage.EM_ApplicationReference);
			AssertEquals("sentMessage.EM_Status", CIMEDIMessage.Status.Queued, sentMessage.EM_Status);
			AssertEquals("sentMessage.EM_GB", GlbBranch.CurrentBranch.PK, sentMessage.EM_GB);
			AssertMultilineASCIIEquals("sentMessage.EM_MessageText", AWBTestDataCreator.MAWBSampleFWB.Trim(), sentMessage.EM_MessageText.Trim());

			var sentInterchange = sentMessage.Interchange;
			AssertNotNull("sentMessage.Interchange", sentInterchange);
			AssertEquals("sentInterchange.EI_HeaderText", ZString.Empty, sentInterchange.EI_HeaderText);
			AssertEquals("sentInterchange.EI_BodyText", sentMessage.EM_MessageText, sentInterchange.EI_BodyText);
			AssertEquals("sentInterchange.EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, sentInterchange.EI_ReceiveTransmit);
			AssertEquals("sentInterchange.EI_Status", EDIInterchange.Status.Queued, sentInterchange.EI_Status);
			AssertEquals("sentInterchange.EI_From", "EDIDAT", sentInterchange.EI_From);
			AssertEquals("sentInterchange.EI_To", "EDI CCN", sentInterchange.EI_To);
		}

		#region Implementation

		static void AssertMessageSent(ExportAWBHeader mawbHeader, string headerName, string messageType, string messageContent)
		{
			AssertEquals(headerName + ".Messages.Count", 1, mawbHeader.Messages.Count);
			var sentMessage = mawbHeader.Messages[0];
			CombineAssertions(delegate
			{
				AssertEquals(headerName + ".Messages[0].EM_MessageType", messageType, sentMessage.EM_MessageType);
				AssertEquals(headerName + ".Messages[0].EM_ApplicationReference", "00696667465", sentMessage.EM_ApplicationReference);
				AssertEquals(headerName + ".Messages[0].EM_Status", CIMEDIMessage.Status.Queued, sentMessage.EM_Status);
				AssertMultilineASCIIEquals(headerName + ".Messages[0].EM_MessageText", messageContent.Trim(), sentMessage.EM_MessageText.Trim());
			});
		}

		#endregion
	}
}
