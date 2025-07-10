using Enterprise.Registry.GUI;

namespace Enterprise.Customs.DataRegistry.GUI
{
	public partial class UCMEDIMessageTestTypeUserControl : RegistryZUserControl
	{
		public UCMEDIMessageTestTypeUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ApplicationCodesToTestGrid.ReadOnly = readOnly;
		}
	}
}
