using Enterprise.Registry.GUI;

namespace Enterprise.Freight.GUI
{
	public partial class TNPAAccountNumberControl : RegistryZUserControl
	{
		public TNPAAccountNumberControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			TNPAAccountNumberGrid.ReadOnly = readOnly;
		}
	}
}
