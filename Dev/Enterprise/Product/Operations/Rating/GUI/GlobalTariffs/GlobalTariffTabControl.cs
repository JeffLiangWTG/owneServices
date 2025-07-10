using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class GlobalTariffTabControl : BaseTabControl
	{
		public GlobalTariffTabControl()
		{
			InitializeComponent();
		}

		public override ZTabControl TopLevelTabControl
		{
			get { return TabControl; }
		}

		#region IDisposable Members

		System.ComponentModel.IContainer components;

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
