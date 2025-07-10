using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.Registry
{
	[SuppressFormsLocalizedTest]
	public partial class AutoratingIntercompanyTariffsForGatewayJobConfigurationControl : RegistryZUserControl
	{
		#region Controls

		internal ZArchitecture.ZGrid AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid;

		#endregion

		public AutoratingIntercompanyTariffsForGatewayJobConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.ReadOnly = readOnly;
		}
	}
}

