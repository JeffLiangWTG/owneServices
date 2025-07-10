using Enterprise.Registry.GUI;

namespace Enterprise.Customs.NL.GUI;

public partial class MessageVersionRegistryUserControl : RegistryZUserControl
{
	public MessageVersionRegistryUserControl()
	{
		InitializeComponent();
	}

	protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
	{
		base.SetControlOrBusinessEntityReadOnly(readOnly);
		MessageVersionGrid.ReadOnly = readOnly;
	}
}
