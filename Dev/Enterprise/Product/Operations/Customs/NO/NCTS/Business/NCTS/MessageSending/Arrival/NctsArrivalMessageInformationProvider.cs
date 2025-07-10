using CargoWise.Customs.NO.MessageContracts.NCTS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.NO.NCTS.Business.MessageSending.Arrival;

namespace Enterprise.Customs.NO.NCTS.Business;

class NctsArrivalMessageInformationProvider : NctsMessageInformationProvider
{
	public NctsArrivalMessageInformationProvider(NctsHeaderMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
	}

	protected override IXmlMessageBuilder CreateMessageBuilder()
	{
		string messageType = MessageSendingObject.MessageType;
		return messageType switch
		{
			NctsArrivalMessageTypeCodeList.Codes.ArrivalNotification => CreateCC07MessageBuilder(),
			_ => null
		};
	}

	protected override BusinessObject GetParentCore() => Header;

	IXmlMessageBuilder CreateCC07MessageBuilder()
	{
		var dataProvider = CC007CTypeDataProvider.CreateProvider(MessageSendingObject, new CC007CTypeAdditionalDataProvider());
		return new CC007CTypeMessageBuilder(dataProvider);
	}
}
