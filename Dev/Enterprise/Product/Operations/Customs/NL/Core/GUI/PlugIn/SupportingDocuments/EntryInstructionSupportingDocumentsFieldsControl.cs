using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public partial class EntryInstructionSupportingDocumentsFieldsControl : EU.GUI.PlugIn.InvoiceLayoutSupportingDocumentsFieldsControl
{
	public EntryInstructionSupportingDocumentsFieldsControl(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
	{
		InitializeComponent();
		SupportingDocumentsGroupBox.CaptionResourceString = Res.GetData("718BB396-ABC1-4F41-A6B3-F59FF70947C8", "Supporting Documents");
	}

	protected override IPanelLayoutProvider GetLayout() => new EntryInstructionSupportingDocumentsFieldsLayout();
}
