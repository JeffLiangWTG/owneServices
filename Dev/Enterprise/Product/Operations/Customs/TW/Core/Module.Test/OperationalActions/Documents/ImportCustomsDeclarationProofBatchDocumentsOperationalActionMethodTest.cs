using Enterprise.Customs.TW.Module.OperationalActions;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(ImportCustomsDeclarationProofBatchDocumentsOperationalActionMethod))]
	sealed class ImportCustomsDeclarationProofBatchDocumentsOperationalActionMethodTest : BatchDocumentsOperationalActionMethodTest
	{
		protected override string ExpectedName => "Import Customs Declaration (Proof)";

		protected override string ExpectedDescription => "Import Customs Declaration (Proof)";

		protected override BatchDocumentsOperationalActionMethod CreateBatchDocumentsOperationalActionMethod()
		{
			return new ImportCustomsDeclarationProofBatchDocumentsOperationalActionMethod();
		}
	}
}
