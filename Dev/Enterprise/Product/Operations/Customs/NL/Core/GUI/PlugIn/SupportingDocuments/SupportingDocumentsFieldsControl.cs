namespace Enterprise.Customs.NL.GUI;

public partial class SupportingDocumentsFieldsControl : EU.GUI.PlugIn.SupportingDocumentsFieldsControl
{
	public SupportingDocumentsFieldsControl()
	{
		InitializeComponent();
		CSI_UnitOfQuantityTextBox.Visible = false;
		CSI_Quantity2CalcEdit.Visible = false;
		CSI_UnitOfQuantity2TextBox.Visible = false;
		CSI_StatusDropEdit.Visible = false;
		CSI_DateOfIssueDateEdit.Visible = false;
	}
}
