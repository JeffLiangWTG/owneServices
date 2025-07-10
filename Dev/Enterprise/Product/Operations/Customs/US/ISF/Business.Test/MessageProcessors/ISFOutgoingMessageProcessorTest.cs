using System;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ISFOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		[TestDate(2009, 12, 1)]
		public void TestOutgoingMessagesPackageIntoInterchanges()
		{
			var filer = new EntryFiler();
			filer.EntryFilerCode = "XJ5";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);
			var b = new APLB();
			b.UserData = MQEDIMessage.MessageNumberPlaceHolder;
			ZString messageText = b.Serialise() + new ISFSF10()
			{ ISFImporterNumber = "789012" }.Serialise();
			var message1 = Factory.New<MQEDIMessage>();
			message1.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message1.EM_Status = MQEDIMessage.Status.Queued;
			message1.EM_MessageText = messageText;
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling;
			AssertNull(message1.Interchange);
			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message2.EM_Status = MQEDIMessage.Status.Queued;
			message2.EM_MessageText = messageText;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling;
			AssertNull(message2.Interchange);
			var message3 = Factory.New<MQEDIMessage>();
			message3.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message3.EM_Status = MQEDIMessage.Status.Queued;
			message3.EM_MessageText = messageText;
			message3.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			AssertNull(message3.Interchange);
			Factory.Save();
			var processor = new ISFOutgoingMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);
			AssertMessage(message1, "1", "XJ5");
			AssertMessage(message2, "2", "XJ5");
			message3.Reload();
			AssertEquals(MQEDIMessage.Status.Queued, message3.EM_Status);
			AssertNull(message3.Interchange);
			filer.EntryFilerCode = "";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);
			var message4 = Factory.New<MQEDIMessage>();
			message4.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message4.EM_Status = MQEDIMessage.Status.Queued;
			message4.EM_MessageText = messageText;
			message4.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling;
			AssertNull(message4.Interchange);
			Factory.Save();
			processor.ProcessMessage(CancellationToken.None);
			AssertMessage(message4, "1", "ISF" + GlbCompany.CurrentCompany.GC_Code);
		}

		void AssertMessage(MQEDIMessage message, ZString interchangeNum, ZString from)
		{
			message.Reload();
			var interchange = message.Interchange;
			AssertNotNull(interchange);
			AssertEquals(message, interchange.ContainedMessages[0]);
			AssertEquals(1, interchange.ContainedMessages.Count);
			AssertEquals(message.EM_MessageType, interchange.EI_InterchangeType);
			AssertEquals(interchangeNum, interchange.EI_InterchangeNum);
			AssertEquals(from, interchange.EI_From);
		}
	}
}
