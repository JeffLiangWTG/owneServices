using Enterprise.Registry.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class CusGoodsLocationRegistryItemUserControl : RegistryZUserControl
	{
		public CusGoodsLocationRegistryItemUserControl()
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
