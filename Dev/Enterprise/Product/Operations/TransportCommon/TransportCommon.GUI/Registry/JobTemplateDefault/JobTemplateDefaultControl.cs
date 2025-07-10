using Enterprise.Registry.GUI;

namespace Enterprise.TransportCommon.GUI.Registry
{
	public partial class JobTemplateDefaultControl : RegistryZUserControl
	{
		public JobTemplateDefaultControl()
		{
			InitializeComponent();
		}

		#region ReadOnly

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			JobTemplateDefaultGrid.ReadOnly = readOnly;
		}

		#endregion
	}
}

