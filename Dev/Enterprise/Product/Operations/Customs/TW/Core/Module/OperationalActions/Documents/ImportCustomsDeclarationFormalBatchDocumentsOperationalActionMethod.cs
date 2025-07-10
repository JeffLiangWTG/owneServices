using CargoWise.Types;

namespace Enterprise.Customs.TW.Module.OperationalActions
{
	public class ImportCustomsDeclarationFormalBatchDocumentsOperationalActionMethod : BatchDocumentsOperationalActionMethod
	{
		public ImportCustomsDeclarationFormalBatchDocumentsOperationalActionMethod() : base(new ZGuid("CFD8E09D-AF2A-4696-B699-B1798E951AE9"))
		{
		}

		public override string Name => Res.GetString("6488AEF0-BAB0-4515-A8E1-0F4CE48DF1DB", "Import Customs Declaration (Formal)");

		public override string Description => OperationalActionLogAndUserNotificationWrapper.Constants.TaiwanImportCustomsDeclaration_Formal;
	}
}
