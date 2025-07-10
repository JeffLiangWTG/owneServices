using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.PlugIn;

public class ExportInvoiceHeaderPreviousDocumentGridColumnLayout : IGridColumnLayoutProvider
{
	public IGridColumnLayout Layout => layout ??= CreateLayout();
	IGridColumnLayout layout;

	IGridColumnLayout CreateLayout()
	{
		var euGridColumnBag = PreviousDocumentsGridColumnBag.Instance;
		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(euGridColumnBag.CodeDropEditColumn);
		builder.AddColumn(euGridColumnBag.ReferenceNumberColumn);
		builder.AddColumn(euGridColumnBag.LineNoColumn);

		return builder.Build();
	}
}
