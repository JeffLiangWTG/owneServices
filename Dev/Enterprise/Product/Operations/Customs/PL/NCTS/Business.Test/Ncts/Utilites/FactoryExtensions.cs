using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

static class FactoryExtensions
{
	const string TestMessageText =
@"<ns1:IsztarHistoryRequest xmlns:ns1=""http://www.mf.gov.pl/schematy/isztar/ecipSeapInpParams/2017/01"">
   <ns1:akceptuje_zobowiazanie>true</ns1:akceptuje_zobowiazanie>
   <ns1:startDate>2022-06-23T00:00:00</ns1:startDate>
   <ns1:endDate>2022-06-23T23:59:59</ns1:endDate>
</ns1:IsztarHistoryRequest>";

	public static EDIMessage CreateNCTSMessage(
		this BusinessObjectFactory factory,
		string messageText = TestMessageText,
		string applicationCode = ApplicationCodeList.Codes.PLCustomsNCTS,
		string messageType = EUJobMessageTypeList.Codes.NctsDeparture,
		string direction = EDIInterchange.Direction.Transmit,
		string status = EDIMessageStatusList.Codes.Queued,
		BusinessObject linkedObject = null,
		BusinessObject password = null,
		bool isTestMessage = true
		)
	{
		var message = factory.New<EDIMessage>();
		message.EM_IsTestMessage = isTestMessage;
		message.EM_ApplicationCode = applicationCode;
		message.EM_GB = GlbBranch.CurrentBranch.PK;
		message.EM_MessageText = messageText;
		message.EM_MessageType = messageType;
		message.EM_ReceiveTransmit = direction;
		message.EM_Status = status;
		message.EM_LinkedObject = linkedObject;
		message.EM_GP = password?.PK ?? ZGuid.Empty;
		return message;
	}
}
