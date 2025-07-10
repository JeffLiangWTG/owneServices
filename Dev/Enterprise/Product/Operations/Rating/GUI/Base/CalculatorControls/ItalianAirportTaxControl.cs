namespace Enterprise.Rating.GUI
{
	public partial class ItalianAirportTaxControl : RateCalculatorUserControl
	{
		public ItalianAirportTaxControl()
		{
			InitializeComponent();
		}

		#region Binding

		protected override void SetBindings()
		{
			FlatRateCalcEdit.Decimals = DecimalPlaces;
			BasicChargeCalcEdit.Decimals = DecimalPlaces;
			AdditionalPackageCalcEdit.Decimals = DecimalPlaces;
			FirstPackageCalcEdit.Decimals = DecimalPlaces;

			base.SetBindings();

			SetBinding(BasicChargeCalcEdit, "Decimal1");
			SetBinding(FlatRateCalcEdit, "Decimal2");
			SetBinding(FirstPackageCalcEdit, "Decimal3");
			SetBinding(AdditionalPackageCalcEdit, "Decimal4");
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
