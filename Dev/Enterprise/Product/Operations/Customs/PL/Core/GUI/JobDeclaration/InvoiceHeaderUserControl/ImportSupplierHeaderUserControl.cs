using System;

namespace Enterprise.Customs.PL.GUI;

public partial class ImportSupplierHeaderUserControl : EU.GUI.EUImportSupplierHeaderUserControl
{
	public ImportSupplierHeaderUserControl()
	{
		InitializeComponent();
		JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayoutJyz3gLm/B1me8goeJBGk7Q==";
	}

	protected override Type GetAdditionalInfosUserControlType() => typeof(PlugIn.AdditionalInfosUserControlWithGrid);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(PlugIn.LayoutPreviousDocumentsUserControl);

	protected override Type GetSupportingDocumentsUserControlType() => typeof(PlugIn.LayoutSupportingDocumentsUserControl);
}
