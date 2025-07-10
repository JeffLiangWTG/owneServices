using CargoWise.Customs.PL.MessageContracts.MessageBuilders.AIS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class AISMessageSender : CusEntryHeaderMessageSender
{
	public AISMessageSender(BusinessObjectFactory factory, BaseMessageSendingObject sendingObject, BaseMessageSendingObjectParent sendingObjectParent)
		: base(factory, sendingObject, sendingObjectParent)
	{
	}

	protected override IXmlMessageBuilder GetXmlMessageBuilder()
	{
		switch (sendingObject.Action)
		{
			case ImportMessageSendingObjectActionList.Codes.ZC415:
				return new CC415CMessageBuilder(new CC415RootProvider(sendingObject));
			default:
				return null;
		}
	}

	protected override ZString GetMessageSubType() => (string)sendingObject.Action switch
	{
		ImportMessageSendingObjectActionList.Codes.ZC415 => AISMessageCodes.Descriptions.ZC415,
		_ => ZString.Empty
	};
}
