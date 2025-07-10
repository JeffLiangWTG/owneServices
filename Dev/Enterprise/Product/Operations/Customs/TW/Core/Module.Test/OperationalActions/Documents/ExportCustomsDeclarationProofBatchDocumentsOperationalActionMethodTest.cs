using Enterprise.Customs.TW.Module.OperationalActions;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(ExportCustomsDeclarationProofBatchDocumentsOperationalActionMethod))]
	sealed class ExportCustomsDeclarationProofBatchDocumentsOperationalActionMethodTest : BatchDocumentsOperationalActionMethodTest
	{
		protected override string ExpectedName => "Export Customs Declaration (Proof)";

		protected override string ExpectedDescription => "Export Customs Declaration (Proof)";

		protected override BatchDocumentsOperationalActionMethod CreateBatchDocumentsOperationalActionMethod()
		{
			return new ExportCustomsDeclarationProofBatchDocumentsOperationalActionMethod();
		}
	}
}
