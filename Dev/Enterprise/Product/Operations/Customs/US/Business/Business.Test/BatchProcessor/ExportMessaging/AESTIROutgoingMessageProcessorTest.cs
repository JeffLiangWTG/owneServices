using System.Threading;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AESTIROutgoingMessageProcessorTest : TestCaseWithFactory
	{
		[TestDate(2009, 12, 1)]
		public void TestOutgoingMessgesPackageIntoInterchanges()
		{
			var filer = new Registry.Business.Customs.US.ExportEntryFilerID();
			filer.EntryFilerID = "123456789";
			filer.EntryFilerIDType = "D";
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(MasterFiles.Business.GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, filer);

			var b = new AESCommShipBXP();
			b.USPPIID = "123456789";
			var messageText = b.Serialise();

			var message1 = Factory.New<AESTIREDIMessage>();
			message1.EM_ReceiveTransmit = AESTIREDIMessage.Direction.Transmit;
			message1.EM_Status = AESTIREDIMessage.Status.Queued;
			message1.EM_MessageText = messageText;
			message1.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipment;
			AssertNull(message1.Interchange);

			var message2 = Factory.New<AESTIREDIMessage>();
			message2.EM_ReceiveTransmit = AESTIREDIMessage.Direction.Transmit;
			message2.EM_Status = AESTIREDIMessage.Status.Queued;
			message2.EM_MessageText = messageText;
			message2.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipment;
			AssertNull(message2.Interchange);

			var message3 = Factory.New<AESTIREDIMessage>();
			message3.EM_ReceiveTransmit = AESTIREDIMessage.Direction.Transmit;
			message3.EM_Status = AESTIREDIMessage.Status.Queued;
			message3.EM_MessageText = messageText;
			message3.EM_MessageType = ApplicationIdentifierCodeList.Codes.ElectronicInvoice;
			AssertNull(message3.Interchange);

			var message4 = Factory.New<AESTIREDIMessage>();
			message4.EM_ReceiveTransmit = AESTIREDIMessage.Direction.Transmit;
			message4.EM_Status = AESTIREDIMessage.Status.Queued;
			message4.EM_MessageText = messageText;
			message4.EM_MessageType = ApplicationIdentifierCodeList.Codes.BorderCargoRelease;
			AssertNull(message4.Interchange);

			Factory.Save();

			var processor = new AESTIROutgoingMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);

			AssertMessage(message1);
			AssertMessage(message2);
			AssertNull(message3.Interchange);
			AssertNull(message4.Interchange);
		}

		void AssertMessage(MQEDIMessage message)
		{
			message.Reload();
			var interchange = message.Interchange;
			AssertNotNull(interchange);
			AssertEquals(message, interchange.ContainedMessages[0]);
			AssertEquals(1, interchange.ContainedMessages.Count);
			AssertEquals(message.EM_MessageType, interchange.EI_InterchangeType);
		}
	}
}
