using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;

namespace Enterprise.Customs.PL.Business;

public class CC514RootProvider(BaseMessageSendingObject sendingObject)
	: AESBaseProviderWithPartiesBase(sendingObject)
	, ICC514CRoot
{
	public ICC514CExportOperation ExportOperation => exportOperation ?? (exportOperation = new CC514CExportOperationProvider(SendingObject));
	ICC514CExportOperation exportOperation;

	public string CustomsOfficeOfExport => Declaration.JE_CustomsOffice;

	public override string MessageType => Constants.MessageType.AES.CC514C;
}
