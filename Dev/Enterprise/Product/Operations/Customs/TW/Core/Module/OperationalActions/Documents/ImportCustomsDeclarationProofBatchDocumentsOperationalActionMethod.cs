using CargoWise.Types;

namespace Enterprise.Customs.TW.Module.OperationalActions
{
	public class ImportCustomsDeclarationProofBatchDocumentsOperationalActionMethod : BatchDocumentsOperationalActionMethod
	{
		public ImportCustomsDeclarationProofBatchDocumentsOperationalActionMethod() : base(new ZGuid("7DF74034-520A-4BDC-AE64-136BA23D8F3C"))
		{
		}

		public override string Name => Res.GetString("831C5B0F-C004-41FC-9215-1AD9E22BD0B7", "Import Customs Declaration (Proof)");

		public override string Description => OperationalActionLogAndUserNotificationWrapper.Constants.TaiwanImportCustomsDeclaration_Proof;
	}
}
