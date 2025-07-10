using CargoWise.Types;

namespace Enterprise.Customs.TW.Module.OperationalActions
{
	public class ExportCustomsDeclarationProofBatchDocumentsOperationalActionMethod : BatchDocumentsOperationalActionMethod
	{
		public ExportCustomsDeclarationProofBatchDocumentsOperationalActionMethod() : base(new ZGuid("8099DE3F-16BF-4127-B50D-1379630FD720"))
		{
		}

		public override string Name => Res.GetString("6D950531-C0D3-422E-85EC-AE71FF2A0CEE", "Export Customs Declaration (Proof)");

		public override string Description => OperationalActionLogAndUserNotificationWrapper.Constants.TaiwanExportCustomsDeclaration_Proof;
	}
}
