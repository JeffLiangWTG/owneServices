using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors.Testing
{
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
	using Enterprise.Environment;
	class NotificationUpdaterRequestMoreInfoTest : NotificationUpdaterTestCase
	{
		public void TestProcessRequestMoreInfo()
		{
			var declaration = GetDeclarationWithSentMessage();
			var message = GetReceivedMessage(ResponseMessages.RequestMoreInfoXML);

			var logger = new LoggerForTesting();
			var processor = new NotificationMessageProcessor(logger.OldLogger);
			processor.ProcessMessage(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_ApplicationCode", NZMMessage.ApplicationCodes.NewZealandMAFeBACCa, message.EM_ApplicationCode);
				AssertEquals("message.EM_ApplicationReference", "EB4P9WXXL9", message.EM_ApplicationReference);
				AssertEquals("message.EM_MessageNum", "1", message.EM_MessageNum);
				AssertEquals("message.EM_MessageType", NZMMessage.MessageTypes.Receive.Notification, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", NZMMessage.MessageTypes.Receive.MessageSubTypes.RequestMoreInfo, message.EM_MessageSubType);
				AssertEquals("message.EM_Status", NZMMessage.Status.Received, message.EM_Status);
				AssertEquals("message.EM_LinkUniqueID", declaration.PK, message.EM_LinkUniqueID);
				AssertEquals("message.EM_GB", Branch.PK, message.EM_GB);

				var mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
				AssertEquals("declaration.MAFMessaging.ZX_ReceiptNumber", "EB4P9WXXL9", mafMessaging.ZX_ReceiptNumber);
				AssertEquals("declaration.MAFMessaging.ZX_MessagingStatus", MessagingStatusList.Codes.MoreInformationRequired, mafMessaging.ZX_MessagingStatus);
				AssertEquals("declaration.MAFMessaging.ZX_ConsignmentNumber", "B2008/278199", mafMessaging.ZX_ConsignmentNumber);
			});

			AssertMultilineASCIIEquals("logger.ToString()", @"
Information:- 	Processing Notification Receipt # [EB4P9WXXL9]...
Information:- 	Updating Job [B00001000]...
".Trim(), logger.ToString());

			AssertEquals("Env.OutgoingMailManager.EmailsCreated.Count", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			CombineAssertions(delegate
			{
				AssertEquals("email.Subject", "[Request More Info] Response for eBACCa sent from: B00001000", email.Subject);
				AssertMultilineASCIIEquals("email.Body", @"
MPI require more information / documentation to process this eBACCa.

eBACCa Job Status now set to [More Information Required].

Request More Info Receipt # [EB4P9WXXL9].
Response to Message # [1].
MPI Consignment # [B2008/278199].
MPI Comment: Please provide more info on exactly what is in the parcel. What is the colon cleanser?
".Trim(), email.Body);
				AssertEquals("email.CCRecipients", "impediments@nowhere.com", email.CCRecipients.RecipientsAsDelimitedString());
			});

			AssertMultilineASCIIEquals("message.EM_MessageInterpretation", @"
[Request More Info] Response for eBACCa sent from: B00001000
------------------------------------------------------------
MPI require more information / documentation to process this eBACCa.

eBACCa Job Status now set to [More Information Required].

Request More Info Receipt # [EB4P9WXXL9].
Response to Message # [1].
MPI Consignment # [B2008/278199].
MPI Comment: Please provide more info on exactly what is in the parcel. What is the colon cleanser?
".Trim(), message.EM_MessageInterpretation);
		}
	}
}
