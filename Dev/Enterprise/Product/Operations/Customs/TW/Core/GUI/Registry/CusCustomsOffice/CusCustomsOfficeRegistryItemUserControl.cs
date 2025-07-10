using Enterprise.Registry.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class CusCustomsOfficeRegistryItemUserControl : RegistryZUserControl
	{
		public CusCustomsOfficeRegistryItemUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CustomsOfficeCodeFindBox.ReadOnly = readOnly;
		}
	}
}
