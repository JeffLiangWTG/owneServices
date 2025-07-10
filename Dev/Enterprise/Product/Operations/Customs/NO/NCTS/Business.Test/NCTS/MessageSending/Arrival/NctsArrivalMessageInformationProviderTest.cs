using CargoWise.Customs.NO.MessageContracts.NCTS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NO.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.NCTS.Business.MessageSending.Arrival.Testing;

[TestedType(typeof(NctsArrivalMessageInformationProvider))]
sealed class NctsArrivalMessageInformationProviderTest : TestCaseWithFactory
{
	public void TestParent()
	{
		var messageSendingObject = CreateMessageSendingObject();
		var provider = CreateMessageInformationProvider(messageSendingObject);
		AssertSame("Parent", messageSendingObject.NctsHeader, provider.Parent);
	}

	public void TestMessageBuilder()
	{
		var messageSendingObject = CreateMessageSendingObject();
		var provider = new NctsArrivalMessageInformationProviderForTest(messageSendingObject);

		CombineAssertions(() =>
		{
			messageSendingObject.MessageType = NctsArrivalMessageTypeCodeList.Codes.ArrivalNotification;
			AssertType<CC007CTypeMessageBuilder>("When MessageType = ArrivalNotification", provider.CreateMessageBuilderExposed());

			messageSendingObject.MessageType = NctsArrivalMessageTypeCodeList.Codes.UnloadingRemarks;
			AssertNull("When MessageType = UnloadingRemarks", provider.CreateMessageBuilderExposed());
		});
	}

	IMessageInformationProvider CreateMessageInformationProvider(NctsHeaderMessageSendingObject messageSendingObject)
		=> new NctsArrivalMessageInformationProvider(messageSendingObject);

	NctsHeaderMessageSendingObject CreateMessageSendingObject()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		return new NctsHeaderMessageSendingObject(nctsHeader);
	}

	class NctsArrivalMessageInformationProviderForTest : NctsArrivalMessageInformationProvider
	{
		public NctsArrivalMessageInformationProviderForTest(NctsHeaderMessageSendingObject messageSendingObject) : base(messageSendingObject)
		{
		}

		public IXmlMessageBuilder CreateMessageBuilderExposed() => CreateMessageBuilder();
	}
}
