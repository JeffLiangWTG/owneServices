namespace Enterprise.Rating.GUI
{
	public partial class FlatPlusPerUnitControl : RateCalculatorUserControl
	{
		public FlatPlusPerUnitControl()
		{
			InitializeComponent();
		}

		#region Binding

		protected override void SetBindings()
		{
			FlatPriceCalcEdit.Decimals = DecimalPlaces;
			PerUnitPriceCalcEdit.Decimals = DecimalPlaces;
			base.SetBindings();

			SetBinding(FlatPriceCalcEdit, "Decimal1");
			SetBinding(PerUnitPriceCalcEdit, "Decimal2");
		}

		#endregion

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
