using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.NL.GUI;

public partial class EntryInstructionDetailsUserControl : EU.GUI.EntryInstructionDetailsUserControl
{
	public EntryInstructionDetailsUserControl() : base()
	{
		InitializeComponent();
	}

	protected override Type GetGridUserControl() => typeof(EntryInstructionGridUserControl);

	protected override Type GetDetailsUserControlType() => typeof(LayoutEntryInstructionDetailBasicUserControl);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(NLEntryInstructionPreviousDocumentsUserControl);

	protected override Type GetSupportingDocumentsUserControlType() => typeof(EntryInstructionSupportingDocumentsUserControl);

	protected override Type GetGuaranteesUserControlType() => typeof(EntryInstructionGuaranteesUserControl);

	protected override ResourceStringData GetAdditionalInfosTabCaption() => Res.GetData("7E5B6B54-59AE-4C7D-931C-34ACF6A761A0", "Additional Documents");

	protected override void SetTabPagesVisibilityCore()
	{
		FixTabPageOrderAndVisibility();
	}

	void FixTabPageOrderAndVisibility()
	{
		EntryInstructionTabControl.TabPages.Remove(SupportingDocumentsTabPage);
		EntryInstructionTabControl.TabPages.Remove(AdditionalInfoTabPage);
		EntryInstructionTabControl.TabPages.Remove(PreviousDocumentsTabPage);
		EntryInstructionTabControl.TabPages.Remove(GuaranteesTabPage);
		EntryInstructionTabControl.TabPages.Remove(AuthorisationsTabPage);
		EntryInstructionTabControl.TabPages.Remove(SupplyChainActorTabPage);
		EntryInstructionTabControl.TabPages.Remove(FiscalReferencesTabPage);

		EntryInstructionTabControl.TabPages.Add(SupportingDocumentsTabPage);
		EntryInstructionTabControl.TabPages.Add(AdditionalInfoTabPage);
		EntryInstructionTabControl.TabPages.Add(PreviousDocumentsTabPage);
		EntryInstructionTabControl.TabPages.Add(FiscalReferencesTabPage);
		EntryInstructionTabControl.TabPages.Add(AuthorisationsTabPage);
		EntryInstructionTabControl.TabPages.Add(SupplyChainActorTabPage);
		EntryInstructionTabControl.TabPages.Add(GuaranteesTabPage);

		base.SetTabPagesVisibilityCore();
	}
}

