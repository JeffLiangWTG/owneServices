using Enterprise.Customs.GUI;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(ImportInvoiceLineDetailsLayoutBuilder))]
	sealed class ImportInvoiceLineDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ImportInvoiceLineDetailsLayoutBuilder, JobComInvoiceLine, CommonInvoiceLineDetailsControlBag>
	{
		protected override ImportInvoiceLineDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new ImportInvoiceLineDetailsLayoutBuilder();

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int ExpectedMaxColumns => 3;

		protected override bool ExpectedNarrowColumnForMediumControls => true;
	}
}
