using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class LastestCustomsResponseParserTest : TestCaseWithFactory
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

			message1.EM_MessageText =
				"B018888XJ5SN                                               ~150000              " +
				"SF30MF MAINFREIGHT INTERNATIONAL             65007252333                        " +
				"SF401042334668US                                                                " +
				"SF90  404INVALID HTS CODE                                                       " +
				"SF9001   SECURITY FILING REJECTED                                               " +
				"Y  8888XJ5SN0004";

			var message2 = (US.Business.MQEDIMessage)header.Messages.AddNew(typeof(US.Business.MQEDIMessage));
			message2.EM_ApplicationCode = Enterprise.Customs.US.Business.EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = Enterprise.Customs.US.Business.EDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			message2.EM_MessageNum = "~150001";
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2009, 2, 23, 15, 30, 0);

			message2.EM_MessageText =
				"B018888XJ5SN                                               ~150001              " +
				"SF10101A  EI 91-013199000           10XJ5-20089367423    91-013199000        N  " +
				"SF15BMBM12356897                                                                " +
				"SF15BMBM56846559                                                                " +
				"SF20MB MB56846859                                                               " +
				"SF9002   ISF ACCEPTED                                                           " +
				"Y  8888XJ5SN00005";

			var parser = new LastestCustomsResponseParser(header);
			AssertEquals("LastestResponse", message2, parser.LastestResponse);
			AssertEquals("LatestMessageTypeCode", ISFMessageStatus.Codes.Accepted, parser.LatestMessageTypeCode);
		}
	}
}
