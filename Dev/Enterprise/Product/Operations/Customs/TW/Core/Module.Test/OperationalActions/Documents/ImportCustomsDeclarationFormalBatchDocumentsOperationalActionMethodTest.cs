using Enterprise.Customs.TW.Module.OperationalActions;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(ImportCustomsDeclarationFormalBatchDocumentsOperationalActionMethod))]
	sealed class ImportCustomsDeclarationFormalBatchDocumentsOperationalActionMethodTest : BatchDocumentsOperationalActionMethodTest
	{
		protected override string ExpectedName => "Import Customs Declaration (Formal)";

		protected override string ExpectedDescription => "Import Customs Declaration (Formal)";

		protected override BatchDocumentsOperationalActionMethod CreateBatchDocumentsOperationalActionMethod()
		{
			return new ImportCustomsDeclarationFormalBatchDocumentsOperationalActionMethod();
		}
	}
}
