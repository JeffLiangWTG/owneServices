namespace Enterprise.Rating.GUI
{
	public partial class FlatControl : RateCalculatorUserControl
	{
		public FlatControl()
		{
			InitializeComponent();
		}

		#region Binding

		protected override void SetBindings()
		{
			PriceCalcEdit.Decimals = DecimalPlaces;
			base.SetBindings();
			SetBinding(PriceCalcEdit, "Decimal1");
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
