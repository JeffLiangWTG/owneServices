using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors.Testing
{
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
	using Enterprise.Environment;
	class NotificationUpdaterCancellationTest : NotificationUpdaterTestCase
	{
		public void TestProcessError()
		{
			var declaration = GetDeclarationWithSentMessage();
			var mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
			mafMessaging.ZX_ReceiptNumber = "PrevRecpt";
			mafMessaging.ZX_ConsignmentNumber = "PrevConsNo";
			var message = GetReceivedMessage(ResponseMessages.CancellationXML);

			var logger = new LoggerForTesting();
			var processor = new NotificationMessageProcessor(logger.OldLogger);
			processor.ProcessMessage(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_ApplicationCode", NZMMessage.ApplicationCodes.NewZealandMAFeBACCa, message.EM_ApplicationCode);
				AssertEquals("message.EM_ApplicationReference", "DJ34IKXS9F", message.EM_ApplicationReference);
				AssertEquals("message.EM_MessageNum", "1", message.EM_MessageNum);
				AssertEquals("message.EM_MessageType", NZMMessage.MessageTypes.Receive.Notification, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", NZMMessage.MessageTypes.Receive.MessageSubTypes.Cancellation, message.EM_MessageSubType);
				AssertEquals("message.EM_Status", NZMMessage.Status.Received, message.EM_Status);
				AssertEquals("message.EM_LinkUniqueID", declaration.PK, message.EM_LinkUniqueID);
				AssertEquals("message.EM_GB", Branch.PK, message.EM_GB);

				AssertEquals("declaration.MAFMessaging.ZX_ReceiptNumber", "", mafMessaging.ZX_ReceiptNumber);
				AssertEquals("declaration.MAFMessaging.ZX_MessagingStatus", MessagingStatusList.Codes.CancelledByMpi, mafMessaging.ZX_MessagingStatus);
				AssertEquals("declaration.MAFMessaging.ZX_ConsignmentNumber", "", mafMessaging.ZX_ConsignmentNumber);
			});

			AssertMultilineASCIIEquals("logger.ToString()", @"
Information:- 	Processing Notification Receipt # [DJ34IKXS9F]...
Information:- 	Updating Job [B00001000]...
".Trim(), logger.ToString());

			AssertEquals("Env.OutgoingMailManager.EmailsCreated.Count", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			CombineAssertions(delegate
			{
				AssertEquals("email.Subject", "[Cancellation] Response for eBACCa sent from: B00001000", email.Subject);
				AssertMultilineASCIIEquals("email.Body", @"
eBACCa was Cancelled by MPI.

eBACCa Job Status now set to [Entry Cancelled].

Cancellation Receipt # [DJ34IKXS9F].
Response to Message # [1].
".Trim(), email.Body);

				AssertEquals("email.CCRecipients", "impediments@nowhere.com", email.CCRecipients.RecipientsAsDelimitedString());
			});

			AssertMultilineASCIIEquals("message.EM_MessageInterpretation", @"
[Cancellation] Response for eBACCa sent from: B00001000
-------------------------------------------------------
eBACCa was Cancelled by MPI.

eBACCa Job Status now set to [Entry Cancelled].

Cancellation Receipt # [DJ34IKXS9F].
Response to Message # [1].
".Trim(), message.EM_MessageInterpretation);
		}
	}
}
