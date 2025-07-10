using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public partial class ImportInvoiceLineUserControl : EU.GUI.EUImportInvoiceLineUserControl
{
	public ImportInvoiceLineUserControl()
	{
		InitializeComponent();

		CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);

		ReorderTabPages();
	}

	protected override ZBool DynamicLayoutApplied => ZBool.True;

	protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ImportInvoiceLineDetailsLayout();

	protected override bool IsZG_TransactionNatureVisible => true;

	protected override ResourceStringData GetPreviousDocumentsTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration)
	{
		return Res.GetData("06ACDF9A-F068-432C-BB40-17A005034E32", "[UCC 2/1] Previous Documents");
	}

	protected override ResourceStringData GetSupportingDocumentsTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration)
	{
		return Res.GetData("AD6A5F64-6C30-4555-9C53-360D42B6356B", "[UCC 2/3] Supporting Documents");
	}

	protected override ResourceStringData GetPackagesTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration)
	{
		return Res.GetData("AAE6C9C4-6B74-4AC2-B01A-B79C836B783F", "Packages");
	}

	protected override ResourceStringData GetLineChargesTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration)
	{
		return Res.GetData("6557AD60-4451-493E-BD4A-A4EFD16D96B1", "[UCC 4/9] Charges");
	}

	protected override Type GetSupportingDocumentsUserControlType() => typeof(NLSupportingDocumentsUserControl);

	protected override Type GetAdditionalInfosUserControlType() => typeof(InvoiceLineAdditionalInfosUserControlWithGrid);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(NLPreviousDocumentsUserControl);

	protected override Type GetOrganizationsUserControlType() => typeof(ImportInvoiceLineOrganizationsUserControl);

	protected override ResourceStringData GetAdditionalInfosTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) => EU.GUI.CaptionProvider.AdditionalDocuments;

	protected override void HookInvoiceLineEvents(BaseJobComInvoiceLine invoiceLine)
	{
		base.HookInvoiceLineEvents(invoiceLine);

		var jobComInvoiceLine = (JobComInvoiceLine)invoiceLine;
		if (jobComInvoiceLine != null)
		{
			jobComInvoiceLine.JI_CEIInfo.ValueChanged += EntryInstruction_CEI_StyleInfo_ValueChanged;
			if (jobComInvoiceLine.EntryInstruction != null)
			{
				jobComInvoiceLine.EntryInstruction.ZG_IsHighValueOvrdInfo.ValueChanged += EntryInstruction_CEI_StyleInfo_ValueChanged;
			}
			EntryInstruction_CEI_StyleInfo_ValueChanged(null, null);
		}
	}

	protected override void UnHookInvoiceLineEvents(BaseJobComInvoiceLine invoiceLine)
	{
		base.UnHookInvoiceLineEvents(invoiceLine);

		var jobComInvoiceLine = (JobComInvoiceLine)invoiceLine;
		if (jobComInvoiceLine != null)
		{
			jobComInvoiceLine.JI_CEIInfo.ValueChanged -= EntryInstruction_CEI_StyleInfo_ValueChanged;
			if (jobComInvoiceLine.EntryInstruction != null)
			{
				jobComInvoiceLine.EntryInstruction.ZG_IsHighValueOvrdInfo.ValueChanged -= EntryInstruction_CEI_StyleInfo_ValueChanged;
			}
		}
	}

	protected override void CustomsInvoiceLinesBoundGrid_ListManager_CurrentItemChanged(object sender, EventArgs e)
	{
		base.CustomsInvoiceLinesBoundGrid_ListManager_CurrentItemChanged(sender, e);
		SetValueIndicatorsTabPageVisibles();
	}

	void EntryInstruction_CEI_StyleInfo_ValueChanged(object sender, EventArgs e)
	{
		SetValueIndicatorsTabPageVisibles();
	}

	void SetValueIndicatorsTabPageVisibles()
	{
		var invoiceLine = (JobComInvoiceLine)CurrentInvoiceLine;
		ValueIndicatorsTabPage.TabVisible = invoiceLine?.EntryInstruction?.ZG_IsHighValueOvrd ?? false;
	}

	void ReorderTabPages()
	{
		LineDetailTabControl.TabPages.Remove(NewLineDetailsTabPage);
		LineDetailTabControl.TabPages.Insert(NewLineDetailsTabPage, 0);

		LineDetailTabControl.TabPages.Remove(OrganizationsTabPage);
		LineDetailTabControl.TabPages.Insert(OrganizationsTabPage, 1);

		LineDetailTabControl.TabPages.Remove(LineChargesTabPage);
		LineDetailTabControl.TabPages.Insert(LineChargesTabPage, 2);

		LineDetailTabControl.TabPages.Remove(SupportingDocumentsTabPage);
		LineDetailTabControl.TabPages.Insert(SupportingDocumentsTabPage, 3);

		LineDetailTabControl.TabPages.Remove(AdditionalInfosTabPage);
		LineDetailTabControl.TabPages.Insert(AdditionalInfosTabPage, 4);

		LineDetailTabControl.TabPages.Remove(PreviousDocumentsTabPage);
		LineDetailTabControl.TabPages.Insert(PreviousDocumentsTabPage, 5);

		LineDetailTabControl.TabPages.Remove(PackagesPivotTabPage);
		LineDetailTabControl.TabPages.Insert(PackagesPivotTabPage, 6);

		LineDetailTabControl.TabPages.Remove(ValueIndicatorsTabPage);
		LineDetailTabControl.TabPages.Insert(ValueIndicatorsTabPage, 7);

		LineDetailTabControl.TabPages.Remove(AuthorisationsTabPage);
		LineDetailTabControl.TabPages.Insert(AuthorisationsTabPage, 8);
	}

	protected override ResourceStringData GetValueIndicatorsTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) => Res.GetData("87618775-00DA-4F89-9E15-CCE0EDAE70A1", "[UCC 4/13] Value Indicators");
}
