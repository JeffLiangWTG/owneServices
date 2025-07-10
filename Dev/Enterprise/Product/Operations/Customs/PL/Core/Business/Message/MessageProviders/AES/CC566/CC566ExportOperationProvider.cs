using System;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

sealed class CC566ExportOperationProvider(BaseMessageSendingObject sendingObject) : ICC566CExportOperation
{
	readonly BaseMessageSendingObject sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));

	readonly CusEntryHeader entryHeader = sendingObject.Header;

	public string Lrn => entryHeader.CH_BGMReference;

	public string Mrn => sendingObject.EntryNumber;

	public DateTime ControlNotificationDateAndTime => default;

	public string NotificationType => null;

	public DateTime? AnticipatedControlDate => null;

	public string Text => null;
}
