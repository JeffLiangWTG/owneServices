using Enterprise.Customs.GUI;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(ExportInvoiceLineDetailsLayoutBuilder))]
	sealed class ExportInvoiceLineDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ExportInvoiceLineDetailsLayoutBuilder, JobComInvoiceLine, CommonInvoiceLineDetailsControlBag>
	{
		protected override ExportInvoiceLineDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new ExportInvoiceLineDetailsLayoutBuilder();

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int ExpectedMaxColumns => 3;

		protected override bool ExpectedNarrowColumnForMediumControls => true;
	}
}
