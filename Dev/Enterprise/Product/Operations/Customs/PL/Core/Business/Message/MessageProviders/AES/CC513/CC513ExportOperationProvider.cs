using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;

namespace Enterprise.Customs.PL.Business;

sealed class CC513ExportOperationProvider(BaseMessageSendingObject sendingObject)
	: ExportOperationProvider_CC515_CC513(sendingObject)
	, ICC513CExportOperation
{
	public string LRN => EntryHeader.CH_BGMReference;

	public string MRN => EntryHeader.EntryNumber.IsEmpty
		? SendingObject.EntryNumber
		: EntryHeader.EntryNumber;
}
