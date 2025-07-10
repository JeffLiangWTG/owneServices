using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDeclarationDocumentSupporterForTest : BaseJobDeclarationDocumentSupporter
	{
		public JobDeclarationDocumentSupporterForTest(BaseJobDeclaration baseJobDeclaration)
			: base(baseJobDeclaration)
		{
		}

		public void DocumentEventSource_DocumentPrintedTestMethod(object sender, DocumentPrintedEventArgs e)
		{
			base.DocumentEventSource_DocumentPrinted(sender, e);
		}
	}
}
