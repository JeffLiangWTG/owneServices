using Enterprise.Registry.GUI;

namespace Enterprise.Recruiter.GUI
{
	public partial class OnlineApplicationDocTypesControl : RegistryZUserControl
	{
		public OnlineApplicationDocTypesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			this.DocTypesGrid.ReadOnly = readOnly;
		}
	}
}
