using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NO.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.NCTS.Business.MessageSending.Arrival.Testing;

[TestedType(typeof(ArrivalOutboundMessageInformationUpdater))]
sealed class ArrivalOutboundMessageInformationUpdaterTest : TestCaseWithFactory
{
	public void TestUpdateInformation_Parameters() => CombineAssertions(() =>
	{
		IMessageInformationUpdater informationUpdater = new ArrivalOutboundMessageInformationUpdater();
		AssertExceptionThrown<ArgumentNullException>("When header is null", () => informationUpdater.UpdateInformation(null, Factory.NewMoq<OutboundEDIMessage>().Object));
		AssertExceptionThrown<ArgumentNullException>("When message is null", () => informationUpdater.UpdateInformation(Factory.NewMoq<NctsHeader>().Object, null));
	});

	public void TestUpdateInformation_EffectiveMessageStatus() => CombineAssertions(() =>
	{
		IMessageInformationUpdater informationUpdater = new ArrivalOutboundMessageInformationUpdater();
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);

		var message = Factory.NewMoq<OutboundEDIMessage>();

		message.Setup(m => m.EM_MessageType).Returns(NctsArrivalMessageTypeCodeList.Codes.ArrivalNotification);
		informationUpdater.UpdateInformation(header, message.Object);
		AssertEquals("When EM_MessageType = 007", NctsMovementHeaderTransactionStatusList.Codes.ArrivalNotificationSent, header.EffectiveMessageStatus);

		message.Setup(m => m.EM_MessageType).Returns(NctsArrivalMessageTypeCodeList.Codes.UnloadingRemarks);
		informationUpdater.UpdateInformation(header, message.Object);
		AssertEquals("When EM_MessageType = 044", NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarksSent, header.EffectiveMessageStatus);
	});
}
