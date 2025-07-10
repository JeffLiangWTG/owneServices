using Enterprise.Registry.GUI;

namespace Enterprise.Customs.NL.GUI;

public partial class CalCalculationMethodRegistryUserControl : RegistryZUserControl
{
	public CalCalculationMethodRegistryUserControl()
	{
		InitializeComponent();
	}

	protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
	{
		base.SetControlOrBusinessEntityReadOnly(readOnly);
		CalCalculationMethodGrid.ReadOnly = readOnly;
	}
}
