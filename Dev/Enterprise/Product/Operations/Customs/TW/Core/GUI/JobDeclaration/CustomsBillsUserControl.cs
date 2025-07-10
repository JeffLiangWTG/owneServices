using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.GUI
{
	public partial class CustomsBillsUserControl : BaseCustomsEntryUserControl, IBasePackingControl
	{
		public CustomsBillsUserControl()
		{
			InitializeComponent();
		}

		protected override void InitLayout()
		{
			base.InitLayout();
			if (TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.Value)
			{
				PackingDetailsGroupBox.Visible = false;
				HouseBillsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			}
		}
	}
}
