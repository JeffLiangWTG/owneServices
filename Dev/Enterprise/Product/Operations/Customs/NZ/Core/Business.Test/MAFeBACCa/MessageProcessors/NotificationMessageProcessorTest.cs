using System.Text;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors.Testing
{
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	class NotificationMessageProcessorTest : MessageProcessorTest
	{
		public void TestMailSendToOriginalSender()
		{
			var broker1 = Factory.New<GlbStaff>();
			broker1.GS_Code = "UR1";
			broker1.GS_LoginName = "User1";
			broker1.GS_EmailAddress = "user1@test.com";

			var broker2 = Factory.New<GlbStaff>();
			broker2.GS_Code = "UR2";
			broker2.GS_LoginName = "User2";
			broker2.GS_EmailAddress = "user2@test.com";
			Factory.Save();

			var declaration = GetDeclarationWithSentMessage("1", "UR1");
			var mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
			var sendMsg2 = CreateSendMessage(mafMessaging, "2", "UR2");
			sendMsg2.EM_ReceiveTransmit = NZMMessage.Direction.Transmit;

			var message = GetReceivedMessage(ResponseMessages.RequestMoreInfoXML, "1");
			message.DocManagerInfo.AddFileOrDocument(new byte[] { 42 }, "Crazy Ivan.pdf", "MCD");

			var logger = new LoggerForTesting();
			var processor = new NotificationMessageProcessor(logger.OldLogger);
			processor.ProcessMessage(message);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("email.Recipients", "user1@test.com", email.Recipients.RecipientsAsDelimitedString());

			message = GetReceivedMessage(ResponseMessages.RequestMoreInfoXMLNo2, "2");
			message.DocManagerInfo.AddFileOrDocument(new byte[] { 42 }, "Crazy Ivan.pdf", "MCD");
			processor = new NotificationMessageProcessor(logger.OldLogger);
			processor.ProcessMessage(message);

			email = Env.OutgoingMailManager.EmailsCreated[1];
			AssertEquals("email.Recipients", "user2@test.com", email.Recipients.RecipientsAsDelimitedString());
		}

		public override void TestCanProcess()
		{
			var logger = new LoggerForTesting();
			var processor = new NotificationMessageProcessor(logger.OldLogger);
			AssertEquals(true, processor.CanProcess("EBACCANotificationType"));
			AssertEquals(false, processor.CanProcess("MessagingResponse"));
			AssertEquals(false, processor.CanProcess("Anything Else"));
		}

		public void TestProcessMessageUpdatesMessageAndDeclaration()
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
				AssertEquals("message.EM_Status", NZMMessage.Status.Received, message.EM_Status);
				AssertEquals("message.EM_LinkUniqueID", declaration.PK, message.EM_LinkUniqueID);
				AssertEquals("message.EM_GB", Branch.PK, message.EM_GB);

				var mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
				AssertEquals("declaration.MAFMessaging.ZX_ReceiptNumber", "EB4P9WXXL9", mafMessaging.ZX_ReceiptNumber);
				AssertEquals("declaration.MAFMessaging.ZX_MessagingStatus", MessagingStatusList.Codes.MoreInformationRequired, mafMessaging.ZX_MessagingStatus);
			});

			AssertMultilineASCIIEquals("logger.ToString()", @"
Information:- 	Processing Notification Receipt # [EB4P9WXXL9]...
Information:- 	Updating Job [B00001000]...
".Trim(), logger.ToString());
		}

		public void DudMessageThrowsMessageProcessingException()
		{
			var message = GetReceivedMessage("This load of bollocks should never process");
			var logger = new LoggerForTesting();
			var processor = new NotificationMessageProcessor(logger.OldLogger);
			AssertExceptionThrown(typeof(MessageProcessingException), delegate
			{ processor.ProcessMessage(message); });
		}

		public void TestAllEDocsAttachmentsOnIncomingMessageGetAttachedToStatusUpdateEmail()
		{
			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_EmailAddress = "nowhere@noplace.com";

			var declaration = GetDeclarationWithSentMessage();
			var message = GetReceivedMessage(ResponseMessages.RequestMoreInfoXML);
			message.DocManagerInfo.AddFileOrDocument(new byte[] { 42 }, "Crazy Ivan.pdf", "MCD");

			var logger = new LoggerForTesting();
			var processor = new NotificationMessageProcessor(logger.OldLogger);
			processor.ProcessMessage(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", NZMMessage.Status.Received, message.EM_Status);
				AssertEquals("declaration.MAFMessaging.ZX_MessagingStatus", MessagingStatusList.Codes.MoreInformationRequired, TestDataBuilder.GetMAFMessaging(declaration).ZX_MessagingStatus);

				AssertEquals("Env.OutgoingMailManager.EmailsCreated.Count", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("email.Recipients", "nowhere@noplace.com", email.Recipients.RecipientsAsDelimitedString());
				AssertEquals("email.Attachments.Count", 1, email.Attachments.Count);
				var attachment = email.Attachments[0];
				AssertEquals("attachment.DisplayName", "Crazy Ivan.pdf", attachment.DisplayName);
				AssertEquals("attachment.Data", Encoding.ASCII.GetString(new byte[] { 42 }), Encoding.ASCII.GetString(attachment.Data));
			});
		}

		public void TestXMLAllOnOneLineGetsFormattedBackOutWithLeadingTabsAndCarriageReturns()
		{
			var declaration = GetDeclarationWithSentMessage();
			var message = GetReceivedMessage(ResponseMessages.ErrorDuplicateXMLAllOnOneLine);

			var logger = new LoggerForTesting();
			var processor = new NotificationMessageProcessor(logger.OldLogger);
			processor.ProcessMessage(message);

			CombineAssertions(delegate
			{
				AssertEquals("Precondition: message.EM_Status", NZMMessage.Status.Received, message.EM_Status);
				AssertMultilineASCIIEquals("message.EM_MessageText", ResponseMessages.ErrorDuplicateXML, message.EM_MessageText);
			});
		}
	}

	class NotificationMessageProcessor : NotificationMessageProcessor<EBACCANotificationTypeType>
	{
		public NotificationMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}
	}
}
