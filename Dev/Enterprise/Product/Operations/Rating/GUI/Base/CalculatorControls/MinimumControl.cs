namespace Enterprise.Rating.GUI
{
	public partial class MinimumControl : RateCalculatorUserControl
	{
		public MinimumControl()
		{
			InitializeComponent();
		}

		#region Binding

		protected override void SetBindings()
		{
			PriceCalcEdit.Decimals = DecimalPlaces;

			base.SetBindings();

			SetBinding(PriceCalcEdit, "Decimal1");
			SetBinding(zJobRadioButton, "Bool1");
			SetBinding(zChargeCodeRadioButton, "Bool2");
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
