using Enterprise.Registry.GUI;

namespace Enterprise.Freight.GUI
{
	public partial class DefaultContainerModesControl : RegistryZUserControl
	{
		#region Controls

		protected internal ZArchitecture.ZGrid DefaultContainerModesGrid;

		#endregion

		public DefaultContainerModesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			DefaultContainerModesGrid.ReadOnly = readOnly;
		}
	}
}
