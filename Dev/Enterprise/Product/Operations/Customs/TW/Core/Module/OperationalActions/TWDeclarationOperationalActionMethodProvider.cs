using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.TW.Module.OperationalActions
{
	public class TWDeclarationOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			return new OperationalActionMethod[]
			{
				new DeclarationMessageOperationalActionMethod(),
				new ExportCustomsDeclarationInformalBatchDocumentsOperationalActionMethod(),
				new ExportCustomsDeclarationFormalBatchDocumentsOperationalActionMethod(),
				new ExportCustomsDeclarationProofBatchDocumentsOperationalActionMethod(),
				new ExportCustomsDeclarationEnglishBatchDocumentsOperationalActionMethod(),
				new ImportCustomsDeclarationInformalBatchDocumentsOperationalActionMethod(),
				new ImportCustomsDeclarationFormalBatchDocumentsOperationalActionMethod(),
				new ImportCustomsDeclarationProofBatchDocumentsOperationalActionMethod(),
				new ImportCustomsDeclarationEnglishBatchDocumentsOperationalActionMethod()
			};
		}
	}
}
