using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class CustomsResponseParserTest : TestCaseWithFactory
	{
		public void TestParser()
		{
			var header = Factory.New<CusISFHeader>();
			var message1 = (US.Business.MQEDIMessage)header.Messages.AddNew(typeof(US.Business.MQEDIMessage));
			message1.EM_ApplicationCode = Enterprise.Customs.US.Business.EDIMessage.ApplicationCodes.USCustomsImport;
			message1.EM_ReceiveTransmit = Enterprise.Customs.US.Business.EDIMessage.Direction.Receive;
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			message1.EM_MessageNum = "~150000";
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2009, 2, 23, 10, 30, 0);
			message1.EM_MessageText = "B018888XJ5SN                                               ~150000              " +
				"SF30MF MAINFREIGHT INTERNATIONAL             65007252333                        " +
				"SF401042334668US                                                                " +
				"SF90  404INVALID HTS CODE                                                       " +
				"SF9001   SECURITY FILING REJECTED                                               " +
				"Y  8888XJ5SN0004";
			var parser = new CustomsResponseParser(message1);
			AssertEquals("MessageTypeCode", ISFMessageStatus.Codes.Rejected, parser.MessageTypeCode);
		}
	}
}
