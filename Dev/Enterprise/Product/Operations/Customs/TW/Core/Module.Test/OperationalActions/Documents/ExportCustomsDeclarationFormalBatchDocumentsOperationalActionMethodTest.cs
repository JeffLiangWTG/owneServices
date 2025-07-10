using Enterprise.Customs.TW.Module.OperationalActions;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(ExportCustomsDeclarationFormalBatchDocumentsOperationalActionMethod))]
	sealed class ExportCustomsDeclarationFormalBatchDocumentsOperationalActionMethodTest : BatchDocumentsOperationalActionMethodTest
	{
		protected override string ExpectedName => "Export Customs Declaration (Formal)";

		protected override string ExpectedDescription => "Export Customs Declaration (Formal)";

		protected override BatchDocumentsOperationalActionMethod CreateBatchDocumentsOperationalActionMethod()
		{
			return new ExportCustomsDeclarationFormalBatchDocumentsOperationalActionMethod();
		}
	}
}
