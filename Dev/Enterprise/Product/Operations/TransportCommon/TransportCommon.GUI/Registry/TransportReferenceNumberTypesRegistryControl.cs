using Enterprise.Registry.GUI;

namespace Enterprise.TransportCommon.GUI.Registry
{
	public partial class TransportReferenceNumberTypesRegistryControl : RegistryZUserControl
	{
		public TransportReferenceNumberTypesRegistryControl()
		{
			InitializeComponent();
		}

		public object Data
		{
			get { return DataSource; }
			set { SetDataBinding(value, null); }
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			transportReferenceNumbersGrid.ReadOnly = readOnly;
		}
	}
}
