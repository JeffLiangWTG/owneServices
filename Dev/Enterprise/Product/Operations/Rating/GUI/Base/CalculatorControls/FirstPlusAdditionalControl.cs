namespace Enterprise.Rating.GUI
{
	public partial class FirstPlusAdditionalControl : RateCalculatorUserControl
	{
		public FirstPlusAdditionalControl()
		{
			InitializeComponent();
		}

		#region Binding

		protected override void SetBindings()
		{
			AddlItemPriceCalcEdit.Decimals = DecimalPlaces;
			FirstItemPriceCalcEdit.Decimals = DecimalPlaces;

			base.SetBindings();

			SetBinding(FirstItemPriceCalcEdit, "Decimal1");
			SetBinding(AddlItemPriceCalcEdit, "Decimal2");
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
