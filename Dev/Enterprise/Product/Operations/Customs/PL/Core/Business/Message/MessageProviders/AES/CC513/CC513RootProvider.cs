using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;

namespace Enterprise.Customs.PL.Business;

public class CC513RootProvider(BaseMessageSendingObject sendingObject)
	: RootProviderBase_CC515_CC513<ICC513CExportOperation>(sendingObject)
	, ICC513CRoot
{
	public override string MessageType => Constants.MessageType.AES.CC513C;

	protected override ICC513CExportOperation CreateExportOperationCore() => new CC513ExportOperationProvider(SendingObject);
}
