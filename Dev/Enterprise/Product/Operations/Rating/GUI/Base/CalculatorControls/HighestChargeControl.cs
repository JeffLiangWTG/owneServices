using Enterprise.ZArchitecture;

namespace Enterprise.Rating.GUI
{
	public partial class HighestChargeControl : RateCalculatorUserControl
	{
		public HighestChargeControl()
		{
			InitializeComponent();
		}

		internal override ZGrid GetRateLineItemsGrid()
		{
			return Controls["ApplyToRateLineItemsGrid"] as ZGrid;
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
