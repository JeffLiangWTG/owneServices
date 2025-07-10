using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public class InvoiceLineDetailsLayoutBuilder<T> : EU.GUI.InvoiceLineDetailsLayoutBuilder<T>
	where T : JobComInvoiceLine
{
	public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
}
