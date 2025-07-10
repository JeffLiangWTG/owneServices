using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class CostingTabControl : BaseTabControl
	{
		public CostingTabControl()
		{
			InitializeComponent();
		}

		public override ZTabControl TopLevelTabControl
		{
			get { return TabControl; }
		}

		protected override string[] GetInapplicableCategories()
		{
			return new[]
			{
				RatingConstants.RateCategory.CYD,
				RatingConstants.RateCategory.CYU
			};
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
