using System.Windows.Forms;

namespace Enterprise.Customs.GUI
{
	public partial class ExWarehouseFrontPageUserControl : FrontPageUserControl
	{
		public ExWarehouseFrontPageUserControl()
		{
			InitializeComponent();
		}

		protected override Control ControlWithFocusWhenFormIsOpened
		{
			get { return BondedWarehouseDocAddressControl; }
		}

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

