using Enterprise.Registry.GUI;

namespace Enterprise.Customs.NL.GUI
{
	public partial class FallbackControl : RegistryZUserControl
	{
		public FallbackControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			zfallbackGroupbox.Enabled = !readOnly;
		}
	}
}
