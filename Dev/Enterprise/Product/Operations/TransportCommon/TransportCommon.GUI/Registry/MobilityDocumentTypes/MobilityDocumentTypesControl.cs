using Enterprise.Registry.GUI;

namespace Enterprise.TransportCommon.GUI.Registry
{
	public partial class MobilityDocumentTypesControl : RegistryZUserControl
	{
		public MobilityDocumentTypesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			this.DocTypesGrid.ReadOnly = readOnly;
		}

		void DocTypesGrid_Navigate(object sender, System.Windows.Forms.NavigateEventArgs ne)
		{
		}
	}
}
