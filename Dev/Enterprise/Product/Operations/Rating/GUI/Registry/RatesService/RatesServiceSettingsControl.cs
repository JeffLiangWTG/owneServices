using Enterprise.Registry.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class RatesServiceSettingsControl : RegistryZUserControl
	{
		public RatesServiceSettingsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			RatesServiceSettingsGrid.ReadOnly = readOnly;
		}

#if DEBUG

		public bool IsControlOrBusinessEntityReadOnly
		{
			get
			{
				return RatesServiceSettingsGrid.ReadOnly;
			}
		}

#endif

	}
}
