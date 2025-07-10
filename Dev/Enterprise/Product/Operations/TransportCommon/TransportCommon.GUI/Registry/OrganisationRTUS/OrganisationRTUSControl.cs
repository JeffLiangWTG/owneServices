using Enterprise.Registry.GUI;

namespace Enterprise.TransportCommon.GUI.Registry
{
	public partial class OrganisationRTUSControl : RegistryZUserControl
	{
		public OrganisationRTUSControl()
		{
			InitializeComponent();
		}

		#region ReadOnly

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			OrganisationRTUSGrid.ReadOnly = readOnly;
		}

		public bool GridReadonly { get => OrganisationRTUSGrid.ReadOnly; }

		#endregion
	}
}

