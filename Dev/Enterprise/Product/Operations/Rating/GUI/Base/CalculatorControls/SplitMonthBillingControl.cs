namespace Enterprise.Rating.GUI
{
	public partial class SplitMonthBillingControl : RateCalculatorUserControl
	{
		public SplitMonthBillingControl()
		{
			InitializeComponent();
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
