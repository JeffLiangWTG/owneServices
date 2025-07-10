using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class RatingTabControl : BaseTabControl
	{
		public RatingTabControl()
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
