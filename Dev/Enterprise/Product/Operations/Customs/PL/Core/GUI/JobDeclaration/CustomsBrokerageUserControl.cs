using Enterprise.Customs.GUI;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.GUI;

public partial class CustomsBrokerageUserControl : EU.GUI.CustomsBrokerageUserControl
{
	public CustomsBrokerageUserControl()
	{
		InitializeComponent();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			components?.Dispose();
		}
		base.Dispose(disposing);
	}

	protected override BaseCustomsEntryUserControl GetDeclarationUserControl() => new JobDeclarationUserControl();

	protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl() => JobDeclaration.IsImport
		? new ImportEntryInstructionDetailsUserControl()
		: new ExportEntryInstructionDetailsUserControl();

	protected override BaseMiscOptionsUserControl GetMiscOptionsUserControl() => new MiscOptionsUserControl();

	protected override BaseCustomsEntryUserControl GetMessageUserControl() => new PLEntryMessageUserControl();

	protected override BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl() => JobDeclaration.IsImport
		? new ImportSupplierHeaderUserControl()
		: new ExportSupplierHeaderUserControl();

	protected override BaseInvoiceLineUserControl GetInvoiceLinesUserControl() => JobDeclaration.IsImport
		? new ImportInvoiceLineUserControl()
		: new ExportInvoiceLineUserControl();

	protected new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

	protected override EU.GUI.DV1UserControl GetDV1UserControl() => new DV1UserControl();

	protected override void RemoveUserControlOfEachTabPage()
	{
		base.RemoveUserControlOfEachTabPage();
		RemoveControl(EntryInstructionDetailsTabPage);
	}
}
