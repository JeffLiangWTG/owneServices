using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.PlugIn;

public partial class LayoutPreviousDocumentsUserControl : EU.GUI.PlugIn.LayoutPreviousDocumentsUserControl
{
	public LayoutPreviousDocumentsUserControl()
	{
		InitializeComponent();
	}

	public new JobDeclaration JobDeclaration
	{
		get => (JobDeclaration)base.JobDeclaration;
		set => base.JobDeclaration = value;
	}

	protected override IGridColumnLayoutProvider GetGridColumnLayoutProvider() => JobDeclaration?.IsImport ?? true
		? new ImportPreviousDocumentGridColumnLayout()
		: IsBoundToInvoiceLines
			? new ExportInvoiceLinePreviousDocumentGridColumnLayout()
			: IsBoundToInvoiceHeaders
				? new ExportInvoiceHeaderPreviousDocumentGridColumnLayout()
				: new ExportPreviousDocumentGridColumnLayout();
}
