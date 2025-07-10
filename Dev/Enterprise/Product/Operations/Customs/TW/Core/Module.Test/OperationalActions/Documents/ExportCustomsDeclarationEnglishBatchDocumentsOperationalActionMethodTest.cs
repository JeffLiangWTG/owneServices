using Enterprise.Customs.TW.Module.OperationalActions;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(ExportCustomsDeclarationEnglishBatchDocumentsOperationalActionMethod))]
	sealed class ExportCustomsDeclarationEnglishBatchDocumentsOperationalActionMethodTest : BatchDocumentsOperationalActionMethodTest
	{
		protected override string ExpectedName => "Export Customs Declaration (English)";

		protected override string ExpectedDescription => "Export Customs Declaration (English)";

		protected override BatchDocumentsOperationalActionMethod CreateBatchDocumentsOperationalActionMethod()
		{
			return new ExportCustomsDeclarationEnglishBatchDocumentsOperationalActionMethod();
		}
	}
}
