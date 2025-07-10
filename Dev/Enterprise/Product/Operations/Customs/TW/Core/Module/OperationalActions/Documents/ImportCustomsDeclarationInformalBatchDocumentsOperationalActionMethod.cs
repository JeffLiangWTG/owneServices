using CargoWise.Types;

namespace Enterprise.Customs.TW.Module.OperationalActions
{
	public class ImportCustomsDeclarationInformalBatchDocumentsOperationalActionMethod : BatchDocumentsOperationalActionMethod
	{
		public ImportCustomsDeclarationInformalBatchDocumentsOperationalActionMethod() : base(new ZGuid("38DDEF9E-229A-41B9-A1BF-B4CA7412D49A"))
		{
		}

		public override string Name => Res.GetString("FAC2648A-661A-4BDC-BC03-44DF3F54D88D", "Import Customs Declaration (Informal)");

		public override string Description => OperationalActionLogAndUserNotificationWrapper.Constants.TaiwanImportCustomsDeclaration_Informal;
	}
}
