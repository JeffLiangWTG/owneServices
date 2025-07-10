using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors.Testing
{
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
	using Enterprise.Environment;
	class NotificationUpdaterErrorTest : NotificationUpdaterTestCase
	{
		public void TestProcessDuplicateError()
		{
			var declaration = GetDeclarationWithSentMessage();
			var message = GetReceivedMessage(ResponseMessages.ErrorDuplicateXML);

			var logger = new LoggerForTesting();
			var processor = new NotificationMessageProcessor(logger.OldLogger);
			processor.ProcessMessage(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_ApplicationCode", NZMMessage.ApplicationCodes.NewZealandMAFeBACCa, message.EM_ApplicationCode);
				AssertEquals("message.EM_ApplicationReference", "E95HBQPPOJ", message.EM_ApplicationReference);
				AssertEquals("message.EM_MessageNum", "1", message.EM_MessageNum);
				AssertEquals("message.EM_MessageType", NZMMessage.MessageTypes.Receive.Notification, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", NZMMessage.MessageTypes.Receive.MessageSubTypes.ErrorMessage, message.EM_MessageSubType);
				AssertEquals("message.EM_Status", NZMMessage.Status.Received, message.EM_Status);
				AssertEquals("message.EM_LinkUniqueID", declaration.PK, message.EM_LinkUniqueID);
				AssertEquals("message.EM_GB", Branch.PK, message.EM_GB);

				var mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
				AssertEquals("declaration.MAFMessaging.ZX_ReceiptNumber", "E95HBQPPOJ", mafMessaging.ZX_ReceiptNumber);
				AssertEquals("declaration.MAFMessaging.ZX_MessagingStatus", MessagingStatusList.Codes.SentAndAcknowledged, mafMessaging.ZX_MessagingStatus);
				AssertEquals("declaration.MAFMessaging.ZX_ConsignmentNumber", "", mafMessaging.ZX_ConsignmentNumber);
			});

			AssertMultilineASCIIEquals("logger.ToString()", @"
Information:- 	Processing Notification Receipt # [E95HBQPPOJ]...
Information:- 	Updating Job [B00001000]...
".Trim(), logger.ToString());

			AssertEquals("Env.OutgoingMailManager.EmailsCreated.Count", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			CombineAssertions(delegate
			{
				AssertEquals("email.Subject", "[Error Message] Response for eBACCa sent from: B00001000", email.Subject);
				AssertMultilineASCIIEquals("email.Body", @"
Duplicate eBACCa messages were received by MPI with the same Receipt #. The second copy sent has been ignored.

eBACCa Job Status remains as [Sent And Acknowledged].

Error Message Receipt # [E95HBQPPOJ].
Response to Message # [1].
MPI Comment: Duplicate application detected. Original application receipt number: LX9BBGSF7O
".Trim(), email.Body);
				AssertEquals("email.CCRecipients", "errors@nowhere.com", email.CCRecipients.RecipientsAsDelimitedString());
			});

			AssertMultilineASCIIEquals("message.EM_MessageInterpretation", @"
[Error Message] Response for eBACCa sent from: B00001000
--------------------------------------------------------
Duplicate eBACCa messages were received by MPI with the same Receipt #. The second copy sent has been ignored.

eBACCa Job Status remains as [Sent And Acknowledged].

Error Message Receipt # [E95HBQPPOJ].
Response to Message # [1].
MPI Comment: Duplicate application detected. Original application receipt number: LX9BBGSF7O
".Trim(), message.EM_MessageInterpretation);
		}

		public void TestProcessError()
		{
			var declaration = GetDeclarationWithSentMessage();
			var message = GetReceivedMessage(ResponseMessages.ErrorResupplyXML);

			var logger = new LoggerForTesting();
			var processor = new NotificationMessageProcessor(logger.OldLogger);
			processor.ProcessMessage(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_ApplicationCode", NZMMessage.ApplicationCodes.NewZealandMAFeBACCa, message.EM_ApplicationCode);
				AssertEquals("message.EM_ApplicationReference", "3KSKDXS92D", message.EM_ApplicationReference);
				AssertEquals("message.EM_MessageNum", "1", message.EM_MessageNum);
				AssertEquals("message.EM_MessageType", NZMMessage.MessageTypes.Receive.Notification, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", NZMMessage.MessageTypes.Receive.MessageSubTypes.ErrorMessage, message.EM_MessageSubType);
				AssertEquals("message.EM_Status", NZMMessage.Status.Received, message.EM_Status);
				AssertEquals("message.EM_LinkUniqueID", declaration.PK, message.EM_LinkUniqueID);
				AssertEquals("message.EM_GB", Branch.PK, message.EM_GB);

				var mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
				AssertEquals("declaration.MAFMessaging.ZX_ReceiptNumber", "3KSKDXS92D", mafMessaging.ZX_ReceiptNumber);
				AssertEquals("declaration.MAFMessaging.ZX_MessagingStatus", MessagingStatusList.Codes.Error, mafMessaging.ZX_MessagingStatus);
				AssertEquals("declaration.MAFMessaging.ZX_ConsignmentNumber", "", mafMessaging.ZX_ConsignmentNumber);
			});

			AssertMultilineASCIIEquals("logger.ToString()", @"
Information:- 	Processing Notification Receipt # [3KSKDXS92D]...
Information:- 	Updating Job [B00001000]...
".Trim(), logger.ToString());

			AssertEquals("Env.OutgoingMailManager.EmailsCreated.Count", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			CombineAssertions(delegate
			{
				AssertEquals("email.Subject", "[Error Message] Response for eBACCa sent from: B00001000", email.Subject);
				AssertMultilineASCIIEquals("email.Body", @"
Errors were found by MPI processing sent eBACCa message.

eBACCa Job Status now set to [Error].

Error Message Receipt # [3KSKDXS92D].
Response to Message # [1].
MPI Comment: The Receipt number included in this eBACCa MessageRequest TL21SK2MED, refers to an errored version on this application. Please resupply this as a new application.
".Trim(), email.Body);
				AssertEquals("email.CCRecipients", "errors@nowhere.com", email.CCRecipients.RecipientsAsDelimitedString());
			});

			AssertMultilineASCIIEquals("message.EM_MessageInterpretation", @"
[Error Message] Response for eBACCa sent from: B00001000
--------------------------------------------------------
Errors were found by MPI processing sent eBACCa message.

eBACCa Job Status now set to [Error].

Error Message Receipt # [3KSKDXS92D].
Response to Message # [1].
MPI Comment: The Receipt number included in this eBACCa MessageRequest TL21SK2MED, refers to an errored version on this application. Please resupply this as a new application.
".Trim(), message.EM_MessageInterpretation);
		}
	}
}
