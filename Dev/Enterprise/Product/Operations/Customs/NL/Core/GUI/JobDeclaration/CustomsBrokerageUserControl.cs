using Enterprise.Customs.GUI;

namespace Enterprise.Customs.NL.GUI;

public partial class CustomsBrokerageUserControl : EU.GUI.CustomsBrokerageUserControl
{
	public CustomsBrokerageUserControl()
	{
		InitializeComponent();
	}

	protected override BaseCustomsEntryUserControl GetDeclarationUserControl() => new JobDeclarationUserControl();

	protected override BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
	{
		return JobDeclaration.IsImport
			? new ImportInvoiceLineUserControl()
			: new ExportInvoiceLineUserControl();
	}

	protected override BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
	{
		return JobDeclaration.IsImport
			? new NLImportSupplierHeaderUserControl()
			: new NLExportSupplierHeaderUserControl();
	}

	protected override BaseCustomsCusContainersUserControl GetContainerUserControl()
	{
		return new ContainerUserControl();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			components?.Dispose();
		}
		base.Dispose(disposing);
	}

	protected override BaseCustomsEntryUserControl GetMessageUserControl() => new EntryMessageUserControl();

	protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl() => new EntryInstructionDetailsUserControl();
}
