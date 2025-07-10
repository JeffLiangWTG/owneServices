namespace Enterprise.Rating.GUI
{
	public partial class WarehouseLocationTypeCalculatorControl : RateCalculatorUserControl
	{
		public WarehouseLocationTypeCalculatorControl()
		{
			InitializeComponent();
		}

		#region Dispose

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
