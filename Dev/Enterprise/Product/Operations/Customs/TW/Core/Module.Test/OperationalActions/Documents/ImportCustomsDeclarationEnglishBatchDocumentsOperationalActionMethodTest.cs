using Enterprise.Customs.TW.Module.OperationalActions;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(ImportCustomsDeclarationEnglishBatchDocumentsOperationalActionMethod))]
	sealed class ImportCustomsDeclarationEnglishBatchDocumentsOperationalActionMethodTest : BatchDocumentsOperationalActionMethodTest
	{
		protected override string ExpectedName => "Import Customs Declaration (English)";

		protected override string ExpectedDescription => "Import Customs Declaration (English)";

		protected override BatchDocumentsOperationalActionMethod CreateBatchDocumentsOperationalActionMethod()
		{
			return new ImportCustomsDeclarationEnglishBatchDocumentsOperationalActionMethod();
		}
	}
}
