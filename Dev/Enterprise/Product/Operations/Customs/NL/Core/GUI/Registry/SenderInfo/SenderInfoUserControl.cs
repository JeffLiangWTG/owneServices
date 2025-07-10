using Enterprise.Registry.GUI;

namespace Enterprise.Customs.NL.GUI;

public partial class SenderInfoUserControl : RegistryZUserControl
{
	public SenderInfoUserControl()
	{
		InitializeComponent();
	}

	protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
	{
		base.SetControlOrBusinessEntityReadOnly(readOnly);

		MainGrid.ReadOnly = readOnly;
	}
}
