namespace Enterprise.Customs.NL.GUI;

public partial class CustomsOfficesUserControl : EU.GUI.CustomsOfficesUserControl
{
	public CustomsOfficesUserControl()
	{
		InitializeComponent();
	}

	public override void HandleDeclarationControlVisibilityChanged()
	{
		base.HandleDeclarationControlVisibilityChanged();
		CustomsOfficeFindBox.Visible = false;
		CustomsOfficeFindBox.Enabled = false;
	}
}
