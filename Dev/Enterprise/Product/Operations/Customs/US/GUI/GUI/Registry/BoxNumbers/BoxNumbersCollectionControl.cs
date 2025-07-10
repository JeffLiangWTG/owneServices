using Enterprise.Registry.GUI;

namespace Enterprise.Customs.US.DataRegistry.GUI
{
	public partial class BoxNumbersCollectionControl : RegistryZUserControl
	{
		public BoxNumbersCollectionControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			BoxNumbersGrid.ReadOnly = readOnly;
		}
	}
}
