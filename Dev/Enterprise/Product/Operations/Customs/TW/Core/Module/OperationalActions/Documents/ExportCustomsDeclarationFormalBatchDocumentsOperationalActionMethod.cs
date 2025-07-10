using CargoWise.Types;

namespace Enterprise.Customs.TW.Module.OperationalActions
{
	public class ExportCustomsDeclarationFormalBatchDocumentsOperationalActionMethod : BatchDocumentsOperationalActionMethod
	{
		public ExportCustomsDeclarationFormalBatchDocumentsOperationalActionMethod() : base(new ZGuid("D51C5F85-3533-4D11-A3EA-585FB5A0F469"))
		{
		}

		public override string Name => Res.GetString("0ADDF6C8-7834-406A-B46C-1A160D7BF10D", "Export Customs Declaration (Formal)");

		public override string Description => OperationalActionLogAndUserNotificationWrapper.Constants.TaiwanExportCustomsDeclaration_Formal;
	}
}
