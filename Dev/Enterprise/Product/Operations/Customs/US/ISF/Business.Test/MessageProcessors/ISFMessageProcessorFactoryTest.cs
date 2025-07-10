using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ISFMessageProcessorFactoryTest : TestCaseWithFactory
	{
		public void TestMessageFriendlyName()
		{
			var processor = new ISFMessageProcessorFactory(new LoggingInformation());
			AssertEquals("US Customs ISF Message Processor", processor.MessageFriendlyName);
		}

		public void TestApplicationCode()
		{
			var processor = new ISFMessageProcessorFactory(new LoggingInformation());
			AssertEquals(CBPEDIInterchange.ApplicationCodes.USCustomsImport, processor.ApplicationCode);
		}

		public void TestMessageTypesToInclude()
		{
			var expectedList = new[] { ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse, ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingStatusAdvisory, ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling };
			var actualList = new ISFMessageProcessorFactory(new LoggingInformation()).MessageTypesToInclude;
			AssertEquals(expectedList.Length, actualList.Count);
			foreach (var type in expectedList)
			{
				AssertCollectionContains(type, actualList);
			}
		}

		public void TestMessageFilter()
		{
			var message1 = Factory.New<MQEDIMessage>();
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			var message3 = Factory.New<MQEDIMessage>();
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingStatusAdvisory;
			var message4 = Factory.New<MQEDIMessage>();
			message4.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message4.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			var processor = new ISFMessageProcessorFactory(new LoggingInformation());
			var messages = Factory.Load<MQEDIMessage>(processor.MessageFilter);
			AssertEquals("messages.Length", 2, messages.Length);
			var message1Check = messages[0];
			var message2Check = messages[1];
			if (message2Check.EM_MessageType == ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse)
			{
				message1Check = messages[1];
				message2Check = messages[0];
			}

			AssertEquals("message1Check should be message1 (ImporterSecurityFilingResponse)", message1, message1Check);
			AssertEquals("message2Check should be message3 (ImporterSecurityFilingStatusAdvisory)", message3, message2Check);
		}

		public void TestProcessingGoodMessage()
		{
			var currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "bob@where.com";
			var group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			group.Staff.Add(currentStaff);
			var originalMessage = Factory.New<MQEDIMessage>();
			originalMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalMessage.EM_MessageNum = "Test";
			Factory.Save();
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			message.EM_MessageNum = "Test";
			message.EM_MessageText = new Messaging.Business.MessageBuildingBlocks.Common.APLB().Serialise() + new ISFSF10().Serialise();
			new ISFMessageProcessorFactory(new LoggingInformation()).ProcessMessage(message);
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
		}

		public void TestProcessingUnknownMessage()
		{
			var currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "bob@where.com";
			var group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			group.Staff.Add(currentStaff);
			var originalMessage = Factory.New<MQEDIMessage>();
			originalMessage.EM_MessageType = "XX";
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalMessage.EM_MessageNum = "Test";
			Factory.Save();
			var message = Factory.New<MQEDIMessage>();
			message.EM_MessageText = @"XYZ123".PadRight(80);
			message.EM_MessageNum = "Test";
			message.EM_MessageType = "XX";
			message.EM_Status = MQEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ISFMessageProcessorFactory(new LoggingInformation()).ProcessMessage(message);
			AssertEquals(MQEDIMessage.Status.Failed, message.EM_Status);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) =>
			{
				return emailToMatched.Subject == "Message Response (Failure)";
			}));
			AssertEquals(true, email.Recipients.Contains("bob@where.com"));
		}
	}
}
