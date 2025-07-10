using Enterprise.Registry.GUI;

namespace Enterprise.TransportCommon.GUI.Registry
{
	public partial class ServiceTaskCreatorOptionControl : RegistryZUserControl
	{
		public ServiceTaskCreatorOptionControl()
		{
			InitializeComponent();
		}

		#region ReadOnly

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ServiceTaskCreatorOptionGrid.ReadOnly = readOnly;
		}

		#endregion
	}
}

