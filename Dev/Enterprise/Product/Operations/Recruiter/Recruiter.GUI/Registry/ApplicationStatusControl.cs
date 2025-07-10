using Enterprise.Registry.GUI;

namespace Enterprise.Recruiter.GUI
{
	public partial class ApplicationStatusControl : RegistryZUserControl
	{
		public ApplicationStatusControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			StatusesGrid.ReadOnly = readOnly;
			notificationEmailTemplateControl1.Enabled = !readOnly;
		}
	}
}
