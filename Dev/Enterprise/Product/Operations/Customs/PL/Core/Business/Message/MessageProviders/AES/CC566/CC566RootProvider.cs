using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;

namespace Enterprise.Customs.PL.Business;

sealed class CC566RootProvider(BaseMessageSendingObject sendingObject) : AESBaseProvider(sendingObject), ICC566CRoot
{
	public ICC566CExportOperation ExportOperation => exportOperation ??= new CC566ExportOperationProvider(SendingObject);
	ICC566CExportOperation exportOperation;

	public IPdWResponse PdWResponse => pdWResponse ??= new PdWResponseProvider(SendingObject);
	IPdWResponse pdWResponse;

	public override string MessageType => Constants.MessageType.AES.CC566C;

	protected override string GetCorrelationIdentifier() => SendingObject.ResponseMessage.IsEmpty ? (string)null : SendingObject.ResponseMessage;
}
