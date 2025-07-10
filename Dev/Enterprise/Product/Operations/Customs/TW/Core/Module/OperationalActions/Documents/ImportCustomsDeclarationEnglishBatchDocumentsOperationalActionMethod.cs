using CargoWise.Types;

namespace Enterprise.Customs.TW.Module.OperationalActions
{
	public class ImportCustomsDeclarationEnglishBatchDocumentsOperationalActionMethod : BatchDocumentsOperationalActionMethod
	{
		public ImportCustomsDeclarationEnglishBatchDocumentsOperationalActionMethod() : base(new ZGuid("79B136A1-3C83-465E-9A46-795FDE23CAA0"))
		{
		}

		public override string Name => Res.GetString("86604B4C-EED5-4E05-B63A-15BB1B3A613F", "Import Customs Declaration (English)");

		public override string Description => OperationalActionLogAndUserNotificationWrapper.Constants.TaiwanImportCustomsDeclaration_English;
	}
}
