using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AESMessageProcessorFactoryTest : TestCaseWithFactory
	{
		public void TestProcessingAESMessage()
		{
			DeclarationTestHelper.SetupForSendMessage();
			AESTIREDIMessage message = Factory.New<AESTIREDIMessage>();
			message.EM_MessageText = "B  60612456712E          US EXPORTER NAME                                       " +
"SC1N11AUCAUNKNS40002509        ABAI YUN HE               60267270420081101 N    " +
"ES1066  F FILING OPT IND MUST BE 2 OR 4                                         " +
"ES1970 RF SHIPMENT REJECTED; RESOLVE & RETRANSMIT                               " +
"Y  60612456712E          US EXPORTER NAME";
			message.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipmentResponse;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			AESMessageProcessorFactory messageFactory = new AESMessageProcessorFactory(new LoggingInformation());
			messageFactory.ProcessMessage(message);
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			message = Factory.New<AESTIREDIMessage>();
			message.EM_MessageText = "B  60612456712E          US EXPORTER NAME                                       " +
"SC1N11AUCAUNKNS40002509        ABAI YUN HE               60267270420081101 N    " +
"ES1066  F FILING OPT IND MUST BE 2 OR 4                                         " +
"ES1970 RF SHIPMENT REJECTED; RESOLVE & RETRANSMIT                               " +
"Y  60612456712E          US EXPORTER NAME";
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			messageFactory.ProcessMessage(message);
			AssertEquals(MQEDIMessage.Status.Failed, message.EM_Status);
		}
	}
}
