using Enterprise.Customs.GUI;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public sealed class ImportInvoiceLineDetailsLayoutBuilder : CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>
	{
		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
