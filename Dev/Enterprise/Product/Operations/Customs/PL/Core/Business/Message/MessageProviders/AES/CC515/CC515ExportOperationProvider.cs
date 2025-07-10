using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;

namespace Enterprise.Customs.PL.Business;

sealed class CC515ExportOperationProvider(BaseMessageSendingObject sendingObject)
	: ExportOperationProvider_CC515_CC513(sendingObject)
	, ICC515CExportOperation
{
	public string ReferenceNumber => EntryHeader.CH_BGMReference;
}
