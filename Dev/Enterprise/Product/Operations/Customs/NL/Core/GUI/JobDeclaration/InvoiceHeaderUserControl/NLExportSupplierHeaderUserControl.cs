using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.NL.GUI;

public partial class NLExportSupplierHeaderUserControl : EU.GUI.EUExportSupplierHeaderUserControl
{
	public NLExportSupplierHeaderUserControl()
	{
		InitializeComponent();
		ChargesGroupBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("NLExportSupplierHeaderUserControl|3da4b650-6e2d-48cb-9eda-996f20c4b003", "[UCC 4/9] Invoice Charges");
		BaseGroupChargesGroupBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("NLExportportSupplierHeaderUserControl|52ef45c3-f115-4258-b47e-a8cb8ffc5ae4", "[UCC 4/9] Group Charges (All Invoices)");
	}

	protected override ResourceStringData GetAdditionalInfoTabPageCaption(JobDeclaration declaration) => Enterprise.Customs.NL.GUI.Res.GetData("4B0CE250-3CC4-4940-A644-B96ECD5C58FD", "[44] Additional Documents");

	protected override ResourceStringData GetSupportingDocumentsTabPageCaption(JobDeclaration declaration) => EU.GUI.CaptionProvider.SupportingDocuments;

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();
		AddColumns();
	}

	void AddColumns()
	{
		JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			ColumnName = Business.Declaration.JobComInvoiceHeader.Schema.JZ_UCR,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
		});
	}

	protected override Type GetSupportingDocumentsUserControlType() => typeof(NLSupportingDocumentsUserControl);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(NLPreviousDocumentsUserControl);

	protected override Type GetAdditionalInfosUserControlType() => typeof(InvoiceLineAdditionalInfosUserControlWithGrid);
}
