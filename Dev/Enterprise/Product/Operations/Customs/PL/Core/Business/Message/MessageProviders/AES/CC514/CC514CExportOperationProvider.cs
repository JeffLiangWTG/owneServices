using System;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class CC514CExportOperationProvider : ICC514CExportOperation
{
	public CC514CExportOperationProvider(BaseMessageSendingObject sendingObject)
	{
		this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		entryHeader = sendingObject.Header;
	}

	readonly CusEntryHeader entryHeader;
	readonly BaseMessageSendingObject sendingObject;

	public string LRN => entryHeader.CH_BGMReference;

	public string MRN => sendingObject.EntryNumber;

	public DateTime? InvalidationRequestDateAndTime => ZDateTime.UtcNow.ToDateTime();

	public string InvalidationReason => sendingObject.AmendmentInvalidationReason;
}
