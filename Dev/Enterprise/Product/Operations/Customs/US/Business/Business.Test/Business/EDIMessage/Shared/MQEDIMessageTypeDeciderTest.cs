using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class MQEDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var amsMessage = Factory.New<MQEDIMessage>();
			amsMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.BrokerManifestDownload;
			amsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var lrMessage = Factory.New<MQEDIMessage>();
			lrMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			lrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var ensMessage = Factory.New<MQEDIMessage>();
			ensMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			ensMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			AssertEquals(typeof(AMSBrokerDownloadMQEDIMessage), factory2.Load<MQEDIMessage>(amsMessage.PK).GetType());
			AssertEquals(typeof(LineReleaseMQEDIMessage), factory2.Load<MQEDIMessage>(lrMessage.PK).GetType());
			AssertEquals(typeof(MQEDIMessage), factory2.Load<MQEDIMessage>(ensMessage.PK).GetType());
		}
	}
}
