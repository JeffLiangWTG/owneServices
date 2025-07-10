namespace Enterprise.Rating.GUI
{
	public partial class MinimumOrPerUnitControl : RateCalculatorUserControl
	{
		public MinimumOrPerUnitControl()
		{
			InitializeComponent();
		}

		#region Binding

		protected override void SetBindings()
		{
			PerUnitPriceCalcEdit.Decimals = DecimalPlaces;
			MinPriceCalcEdit.Decimals = DecimalPlaces;

			base.SetBindings();

			SetBinding(MinPriceCalcEdit, "Decimal1");
			SetBinding(PerUnitPriceCalcEdit, "Decimal2");
		}

		#endregion

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
