using System.IO;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.PL.Business;

public abstract class CusEntryHeaderMessageSender : PLMessageSender
{
	protected CusEntryHeaderMessageSender(BusinessObjectFactory factory, BaseMessageSendingObject sendingObject, BaseMessageSendingObjectParent sendingObjectParent)
		: base(factory, sendingObjectParent)
	{
		this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		var header = sendingObject.Header;
		CusEntryHeader = factory.Load<CusEntryHeader>(header.PK);
	}

	protected readonly BaseMessageSendingObject sendingObject;

	protected abstract IXmlMessageBuilder GetXmlMessageBuilder();

	protected CusEntryHeader CusEntryHeader { get; }

	protected override ZString ApplicationCode => EDIMessage.ApplicationCodes.PLCustoms;

	protected override ZString ApplicationReference => string.Empty;

	protected override ZString MessageType => CusEntryHeader?.Declaration.JE_MessageType ?? ZString.Empty;

	protected override void SendCore(EDIMessage message)
	{
		base.SendCore(message);

		message.EM_MessageSubType = GetMessageSubType();
		message.EM_IsActive = false;
		message.EM_LinkedObject = CusEntryHeader;
		message.EM_LinkUniqueID = CusEntryHeader.PK;

		if (GetXmlMessageBuilder()?.GenerateXmlMessage().GetSerializedStream() is Stream serializedMessage)
		{
			message.SetEM_MessageTextOrDataSource(serializedMessage);
		}

		CusEntryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.Sent;
		CusEntryHeader.Messages.Add(message);
	}

	protected abstract ZString GetMessageSubType();
}
