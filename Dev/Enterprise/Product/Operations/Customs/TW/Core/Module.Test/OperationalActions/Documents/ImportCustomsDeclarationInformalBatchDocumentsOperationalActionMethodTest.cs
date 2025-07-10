using Enterprise.Customs.TW.Module.OperationalActions;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(ImportCustomsDeclarationInformalBatchDocumentsOperationalActionMethod))]
	sealed class ImportCustomsDeclarationInformalBatchDocumentsOperationalActionMethodTest : BatchDocumentsOperationalActionMethodTest
	{
		protected override string ExpectedName => "Import Customs Declaration (Informal)";

		protected override string ExpectedDescription => "Import Customs Declaration (Informal)";

		protected override BatchDocumentsOperationalActionMethod CreateBatchDocumentsOperationalActionMethod()
		{
			return new ImportCustomsDeclarationInformalBatchDocumentsOperationalActionMethod();
		}
	}
}
