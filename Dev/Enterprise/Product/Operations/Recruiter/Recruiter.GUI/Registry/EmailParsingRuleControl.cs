using Enterprise.Registry.GUI;

namespace Enterprise.Recruiter.GUI
{
	public partial class EmailParsingRuleControl : RegistryZUserControl
	{
		public EmailParsingRuleControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			MainGrid.ReadOnly = readOnly;
		}
	}
}
