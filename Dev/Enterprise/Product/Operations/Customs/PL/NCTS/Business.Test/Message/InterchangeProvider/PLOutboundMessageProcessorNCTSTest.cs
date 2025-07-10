using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business.Testing;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class PLOutboundMessageProcessorNCTSTest : OutboundMessageProcessorTestBase
{
	public void TestProcessMessageNctsDeparture() => TestProcessMessageNCTS(EUJobMessageTypeList.Codes.NctsDeparture);

	public void TestProcessMessageNctsArrival() => TestProcessMessageNCTS(EUJobMessageTypeList.Codes.NctsArrivalNotification);

	void TestProcessMessageNCTS(ZString messageType)
	{
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(messageType == EUJobMessageTypeList.Codes.NctsDeparture
			? NctsMovementType.Codes.Departure
			: NctsMovementType.Codes.Arrival);

		var message = Factory.CreateNCTSMessage(messageType: messageType, linkedObject: nctsHeader);
		message.EM_MessageSubType = "001";
		Factory.Save();

		AssertProcessMessage($"Correct {messageType} Message", expectedToBeProcessed: true, message);
	}

	protected override void SetUp()
	{
		base.SetUp();
		CertificateHelper.SetUpTestCertificate();
	}
}
