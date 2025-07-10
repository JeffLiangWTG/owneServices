using System;
using System.Collections.Generic;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public partial class ExportInvoiceLineUserControl : EU.GUI.EUExportInvoiceLineUserControl
{
	public ExportInvoiceLineUserControl()
	{
		InitializeComponent();

		CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);

		ReorderTabPages();
	}

	protected override ResourceStringData GetAdditionalInfosTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) => EU.GUI.CaptionProvider.AdditionalDocuments;

	protected override ZBool DynamicLayoutApplied => ZBool.True;

	protected override bool IsZG_TransactionNatureVisible => true;

	protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ExportInvoiceLineDetailsLayout();

	protected override Type GetSupportingDocumentsUserControlType() => typeof(NLSupportingDocumentsUserControl);

	protected override Type GetAdditionalInfosUserControlType() => typeof(InvoiceLineAdditionalInfosUserControlWithGrid);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(NLPreviousDocumentsUserControl);

	protected override void AddColumnsToGrid()
	{
		base.AddColumnsToGrid();
		CustomsInvoiceLinesBoundGrid.ColumnStyles.AddRange(new ZGridColumnInfo[1]
		{
			new ZCodeFindBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport,
				IsVisible = true,
				IsUnavailable = false
			}
		});
	}

	protected override string[] GetDefaultColumnsForGrid()
	{
		List<string> list = new List<string>(base.GetDefaultColumnsForGrid());
		list.Add(JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport);

		return list.ToArray();
	}

	protected override ResourceStringData GetPreviousDocumentsTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration)
	{
		return Res.GetData("28FAEB39-C2A5-4F0B-A9C3-3CB44A4F683A", "[UCC 2/1] Previous Documents");
	}

	protected override ResourceStringData GetSupportingDocumentsTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration)
	{
		return Res.GetData("DB58A85C-949A-4DE5-8689-A6BFE46BC360", "[UCC 2/3] Supporting Documents");
	}

	protected override ResourceStringData GetPackagesTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration)
	{
		return Res.GetData("F84BC4C8-526D-46E1-AF94-DD02A4A5A99E", "Packages");
	}

	protected override ResourceStringData GetLineChargesTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration)
	{
		return Res.GetData("985685B6-65C9-4EB8-A86D-C3495E86EBF2", "[UCC 4/9] Charges");
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

		LineDetailTabControl.TabPages.Remove(DangerousGoodsTabPage);
		LineDetailTabControl.TabPages.Insert(DangerousGoodsTabPage, 7);

		LineDetailTabControl.TabPages.Remove(AuthorisationsTabPage);
		LineDetailTabControl.TabPages.Insert(AuthorisationsTabPage, 8);
	}
}
