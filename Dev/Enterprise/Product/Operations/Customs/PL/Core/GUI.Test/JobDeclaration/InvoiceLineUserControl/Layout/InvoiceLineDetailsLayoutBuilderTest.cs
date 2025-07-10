using Enterprise.Customs.GUI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(InvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>))]
class InvoiceLineDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<InvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>, JobComInvoiceLine, CommonInvoiceLineDetailsControlBag>
{
	protected override InvoiceLineDetailsLayoutBuilder<JobComInvoiceLine> GetColumnLayoutBuilderForTesting() => new InvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();

	protected override int ExpectedMaxColumns => 3;

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

	protected override bool ExpectedNarrowColumnForMediumControls => true;
}
