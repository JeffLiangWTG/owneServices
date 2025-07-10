using CargoWise.Types;

namespace Enterprise.Customs.TW.Module.OperationalActions
{
	public class ExportCustomsDeclarationInformalBatchDocumentsOperationalActionMethod : BatchDocumentsOperationalActionMethod
	{
		public ExportCustomsDeclarationInformalBatchDocumentsOperationalActionMethod() : base(new ZGuid("8E3BC91D-AEC5-4884-A391-B60C5E57D31B"))
		{
		}

		public override string Name => Res.GetString("E1BA0371-1B6C-4B06-9EF6-E51B9CAE7CC0", "Export Customs Declaration (Informal)");

		public override string Description => OperationalActionLogAndUserNotificationWrapper.Constants.TaiwanExportCustomsDeclaration_Informal;
	}
}
