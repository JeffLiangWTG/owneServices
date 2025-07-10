using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class MessageProcessorFactoryTest : TestCaseWithFactory
	{
		public void TestProcessingGoodMessage()
		{
			var message = Factory.New<CBPMessageForTesting>();
			message.EM_MessageText = new ZZZB() { StringB = "B1" }.Serialise() + new ZZZC() { StringC = "JOEY" }.Serialise() + new ZZZY() { StringY = "Y1" }.Serialise();
			message.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting1;
			message.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			new MessageProcessorFactoryForTesting(new LoggingInformation(), CBPEDIInterchange.ApplicationCodeForTesting).ProcessMessage(message);
			AssertEquals(CBPEDIMessage.Status.Received, message.EM_Status);
			Factory.Save();

			var mailQueryText = $"SELECT * FROM dbo.MailDBItems WHERE MI_Subject = 'Joey''s Test'";
			var collection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			collection.Load(mailQueryText);
			AssertEquals(1, collection.Count);
		}

		public void TestProcessingUnknownSegmentInKnownMessage()
		{
			string messageText =
				new ZZZB() { StringB = "B1" }.Serialise() +
				@"F1070918711234567890NARRATIVEMESSAGE".PadRight(80) +
				@"F107091971         0MARKETWASCLOSEDTHISDATE".PadRight(80) +
				@"M".PadRight(80) +
				new ZZZY() { StringY = "Y1" }.Serialise();

			var message = Factory.New<CBPMessageForTesting>();
			message.EM_MessageText = messageText;
			message.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting1;
			message.EM_Status = CBPEDIMessage.Status.Queued;
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			ErrorReporter.Clear();
			new HookedMessageProcessorFactoryForTesting(1).ProcessMessage(message);
			AssertEquals(CBPEDIMessage.Status.Failed, message.EM_Status);
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "Message Response (Failure)"));
			AssertEquals(3, email.Attachments.Count);
		}

		public void TestProcessingUnknownMessage()
		{
			var message = Factory.New<CBPMessageForTesting>();
			message.EM_MessageText = "F1070918711234567890NARRATIVEMESSAGE".PadRight(80);
			message.EM_MessageType = "XX";
			message.EM_Status = CBPEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new MessageProcessorFactoryForTesting(new LoggingInformation(), CBPEDIInterchange.ApplicationCodeForTesting).ProcessMessage(message);
			AssertEquals(CBPEDIMessage.Status.Failed, message.EM_Status);
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "Message Response (Failure)"));
			AssertEquals(3, email.Attachments.Count);
		}

		public void TestProcessingERMessageWithoutMessageNumber()
		{
			var message = Factory.New<CBPMessageForTesting>();
			message.EM_MessageText = new ZZZB() { StringB = "B1" }.Serialise() + new ZZZC() { StringC = "C1" }.Serialise() + new ZZZY() { StringY = "Y1" }.Serialise();
			message.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting1;
			var x0 = new AABIX0()
			{
				ReferenceDataTypeCode = "BLOCK",
				ReferenceDataText = " 3001 101    EQ KNALNMPR1_6662938"
			};
			message.MessageBlock.MessageBlocks.Add(x0);
			message.EM_Status = CBPEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new MessageProcessorFactoryForTesting(new LoggingInformation(), CBPEDIInterchange.ApplicationCodeForTesting).ProcessMessage(message);
			AssertEquals(CBPEDIMessage.Status.Received, message.EM_Status);
			AssertEquals("KNALNMPR1_6662938", message.EM_MessageNum);
		}

		public void TestSupportsMultipleTopLevelMessageBlocksSplitsMessageTrue()
		{
			var message = Factory.New<CBPMessageForTesting>();
			message.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting2;
			message.EM_MessageText = new ZZZB2() { StringB = "B1" }.Serialise() + new ZZZC2() { StringC = "C1" }.Serialise() + new ZZZC2() { StringC = "C2" }.Serialise() + new ZZZC2() { StringC = "C3" }.Serialise() + new ZZZY2() { StringY = "Y2" }.Serialise();
			message.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			HookedMessageProcessorFactoryForTesting processorFactory = new HookedMessageProcessorFactoryForTesting(10);
			processorFactory.ProcessMessage(message);
			AssertEquals(1, processorFactory.hookedProcessor.ProcessCount);
		}

		public void TestSupportsMultipleTopLevelMessageBlocksSplitsMessageFalse()
		{
			var message = Factory.New<CBPMessageForTesting>();
			message.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting2;
			message.EM_MessageText = new ZZZB2() { StringB = "B1" }.Serialise() + new ZZZC2() { StringC = "C1" }.Serialise() + new ZZZC2() { StringC = "C2" }.Serialise() + new ZZZC2() { StringC = "C3" }.Serialise() + new ZZZY2() { StringY = "Y2" }.Serialise();
			message.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			var processorFactory = new HookedMessageProcessorFactoryForTesting(1);
			processorFactory.ProcessMessage(message);
			AssertEquals(3, processorFactory.hookedProcessor.ProcessCount);
			AssertNotNull(processorFactory.hookedProcessor.Logger);
		}

		public void TestMultipleInnerMessagesAreProcessed()
		{
			var message = Factory.New<CBPMessageForTesting>();
			message.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting2;
			message.EM_MessageText = new ZZZB2() { StringB = "B1" }.Serialise() + new ZZZC2() { StringC = "C1" }.Serialise() + new ZZZC2() { StringC = "C2" }.Serialise() + new ZZZC2() { StringC = "C3" }.Serialise() + new ZZZY2() { StringY = "Y2" }.Serialise();
			message.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			HookedMessageProcessorFactoryForTesting.HookedProcessor.LastMessageBlocks.Clear();
			new MessageProcessorFactoryForTesting(new LoggingInformation(), CBPEDIInterchange.ApplicationCodeForTesting).ProcessMessage(message);

			AssertEquals(3, HookedMessageProcessorFactoryForTesting.HookedProcessor.LastMessageBlocks.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "dummy@where.com";
			var group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			group.Staff.Add(currentStaff);
			Factory.Save();
		}
	}
}
