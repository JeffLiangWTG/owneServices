using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors.Testing
{
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
	using Enterprise.Environment;
	class NotificationUpdaterNotifyCRNTest : NotificationUpdaterTestCase
	{
		public void TestProcessNotifyCRN()
		{
			var declaration = GetDeclarationWithSentMessage("121");
			var message = GetReceivedMessage(ResponseMessages.NotifyCRNMessageXML);

			var logger = new LoggerForTesting();
			var processor = new NotificationMessageProcessor(logger.OldLogger);
			processor.ProcessMessage(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_ApplicationCode", NZMMessage.ApplicationCodes.NewZealandMAFeBACCa, message.EM_ApplicationCode);
				AssertEquals("message.EM_ApplicationReference", "GPEFGSBQDB", message.EM_ApplicationReference);
				AssertEquals("message.EM_MessageNum", "121", message.EM_MessageNum);
				AssertEquals("message.EM_MessageType", NZMMessage.MessageTypes.Receive.Notification, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", NZMMessage.MessageTypes.Receive.MessageSubTypes.NotifyCRN, message.EM_MessageSubType);
				AssertEquals("message.EM_Status", NZMMessage.Status.Received, message.EM_Status);
				AssertEquals("message.EM_LinkUniqueID", declaration.PK, message.EM_LinkUniqueID);
				AssertEquals("message.EM_GB", Branch.PK, message.EM_GB);

				var mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
				AssertEquals("declaration.MAFMessaging.ZX_ReceiptNumber", "GPEFGSBQDB", mafMessaging.ZX_ReceiptNumber);
				AssertEquals("declaration.MAFMessaging.ZX_MessagingStatus", MessagingStatusList.Codes.CrnReceived, mafMessaging.ZX_MessagingStatus);
				AssertEquals("declaration.MAFMessaging.ZX_ConsignmentNumber", "B2008/279885", mafMessaging.ZX_ConsignmentNumber);
			});

			AssertMultilineASCIIEquals("logger.ToString()", @"
Information:- 	Processing Notification Receipt # [GPEFGSBQDB]...
Information:- 	Updating Job [B00001000]...
".Trim(), logger.ToString());

			AssertEquals("Env.OutgoingMailManager.EmailsCreated.Count", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			CombineAssertions(delegate
			{
				AssertEquals("email.Subject", "[Notify CRN] Response for eBACCa sent from: B00001000", email.Subject);
				AssertMultilineASCIIEquals("email.Body", @"
CRN issued for eBACCa by MPI.

eBACCa Job Status now set to [CRN Received].

Notify CRN Receipt # [GPEFGSBQDB].
Response to Message # [121].
MPI Consignment # [B2008/279885].
".Trim(), email.Body);

				AssertEquals("email.CCRecipients", "acknowledgement@nowhere.com", email.CCRecipients.RecipientsAsDelimitedString());
			});

			AssertMultilineASCIIEquals("message.EM_MessageInterpretation", @"
[Notify CRN] Response for eBACCa sent from: B00001000
-----------------------------------------------------
CRN issued for eBACCa by MPI.

eBACCa Job Status now set to [CRN Received].

Notify CRN Receipt # [GPEFGSBQDB].
Response to Message # [121].
MPI Consignment # [B2008/279885].
".Trim(), message.EM_MessageInterpretation);
		}
	}
}
