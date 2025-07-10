using Enterprise.Customs.GUI;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.NL.GUI;

public partial class NLFiscalReferencesUserControl : BaseCustomsEntryUserControl, EU.GUI.PlugIn.ISupportingInfoUserControls
{
	public NLFiscalReferencesUserControl()
	{
		InitializeComponent();
	}

	public ZGrid Grid => FiscalReferencesGrid;

	public string GridBindingMember => "JobDeclaration";

	public void ShowHideEntryInstruction(bool show)
	{
		FiscalReferencesCusEntryInstructionDropEdit.Visible = show;

		var col = FiscalReferencesGrid.GetColumnStyle(FiscalReference.Schema.EntryInstructionID);
		col.IsVisible = show;
		col.IsUnavailable = !show;
	}
}
