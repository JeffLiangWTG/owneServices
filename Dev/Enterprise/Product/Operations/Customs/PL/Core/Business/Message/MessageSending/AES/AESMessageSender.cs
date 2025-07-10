using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class AESMessageSender(BusinessObjectFactory factory, BaseMessageSendingObject sendingObject, BaseMessageSendingObjectParent parent) : CusEntryHeaderMessageSender(factory, sendingObject, parent)
{
	internal static string[] MessageWithEvidences => [ExportMessageSendingObjectActionList.Codes.CC583];

	protected override IXmlMessageBuilder GetXmlMessageBuilder() => (string)sendingObject.Action switch
	{
		ExportMessageSendingObjectActionList.Codes.CC511 => new CC511CMessageBuilder(new CC511RootProvider(sendingObject)),
		ExportMessageSendingObjectActionList.Codes.CC513 => new CC513CMessageBuilder(new CC513RootProvider(sendingObject)),
		ExportMessageSendingObjectActionList.Codes.CC514 => new CC514CMessageBuilder(new CC514RootProvider(sendingObject)),
		ExportMessageSendingObjectActionList.Codes.CC515 => new CC515CMessageBuilder(new CC515RootProvider(sendingObject)),
		ExportMessageSendingObjectActionList.Codes.CC566 => new CC566CMessageBuilder(new CC566RootProvider(sendingObject)),
		ExportMessageSendingObjectActionList.Codes.CC583 => new CC583CMessageBuilder(new CC583RootProvider(sendingObject, SendingObjectParent)),
		_ => null,
	};

	protected override void SendCore(EDIMessage message)
	{
		base.SendCore(message);

		switch (sendingObject.Action)
		{
			case ExportMessageSendingObjectActionList.Codes.CC514:
				CreateOrUpdateNoteForAmendmentInvalidationReason();
				break;
		}
	}

	protected override ZString GetMessageSubType() => (string)sendingObject.Action switch
	{
		ExportMessageSendingObjectActionList.Codes.CC511 => AESMessageCodes.Descriptions.CC511,
		ExportMessageSendingObjectActionList.Codes.CC513 => AESMessageCodes.Descriptions.CC513,
		ExportMessageSendingObjectActionList.Codes.CC514 => AESMessageCodes.Descriptions.CC514,
		ExportMessageSendingObjectActionList.Codes.CC515 => AESMessageCodes.Descriptions.CC515,
		ExportMessageSendingObjectActionList.Codes.CC566 => AESMessageCodes.Descriptions.CC566,
		ExportMessageSendingObjectActionList.Codes.CC583 => AESMessageCodes.Descriptions.CC583,
		_ => ZString.Empty
	};

	void CreateOrUpdateNoteForAmendmentInvalidationReason()
	{
		var noteText = sendingObject.AmendmentInvalidationReason;
		if (!noteText.IsEmpty)
		{
			CusEntryHeader.CH_CustomsMessageRemarks = noteText;
		}
	}
}
