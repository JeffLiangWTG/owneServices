using CargoWise.Types;

namespace Enterprise.Customs.TW.Module.OperationalActions
{
	public class ExportCustomsDeclarationEnglishBatchDocumentsOperationalActionMethod : BatchDocumentsOperationalActionMethod
	{
		public ExportCustomsDeclarationEnglishBatchDocumentsOperationalActionMethod() : base(new ZGuid("8A5AE298-1CA3-4350-BD2C-90F18E88BBFD"))
		{
		}

		public override string Name => Res.GetString("0A9FE495-D6C4-4485-A030-7E400F147BB9", "Export Customs Declaration (English)");

		public override string Description => OperationalActionLogAndUserNotificationWrapper.Constants.TaiwanExportCustomsDeclaration_English;
	}
}
