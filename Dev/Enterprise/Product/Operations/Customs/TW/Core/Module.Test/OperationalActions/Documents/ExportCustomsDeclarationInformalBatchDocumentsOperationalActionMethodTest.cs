using Enterprise.Customs.TW.Module.OperationalActions;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(ExportCustomsDeclarationInformalBatchDocumentsOperationalActionMethod))]
	sealed class ExportCustomsDeclarationInformalBatchDocumentsOperationalActionMethodTest : BatchDocumentsOperationalActionMethodTest
	{
		protected override string ExpectedName => "Export Customs Declaration (Informal)";

		protected override string ExpectedDescription => "Export Customs Declaration (Informal)";

		protected override BatchDocumentsOperationalActionMethod CreateBatchDocumentsOperationalActionMethod()
		{
			return new ExportCustomsDeclarationInformalBatchDocumentsOperationalActionMethod();
		}
	}
}
