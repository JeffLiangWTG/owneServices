using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;

namespace Enterprise.Customs.PL.Business;

public class CC515RootProvider(BaseMessageSendingObject sendingObject)
	: RootProviderBase_CC515_CC513<ICC515CExportOperation>(sendingObject)
	, ICC515CRoot
{
	public override string MessageType => Constants.MessageType.AES.CC515C;

	protected override ICC515CExportOperation CreateExportOperationCore() => new CC515ExportOperationProvider(SendingObject);
}
