using System;

namespace Enterprise.Customs.PL.GUI;

public partial class ExportSupplierHeaderUserControl : EU.GUI.EUExportSupplierHeaderUserControl
{
	public ExportSupplierHeaderUserControl()
	{
		InitializeComponent();
	}

	protected override Type GetPreviousDocumentsUserControlType() => typeof(PlugIn.LayoutPreviousDocumentsUserControl);

	protected override Type GetAdditionalInfosUserControlType() => typeof(PlugIn.AdditionalInfosUserControlWithGrid);

	protected override Type GetSupportingDocumentsUserControlType() => typeof(PlugIn.LayoutSupportingDocumentsUserControl);
}
