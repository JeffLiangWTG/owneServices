namespace Enterprise.Rating.GUI
{
	public partial class ProfitShareRebateControl : RateCalculatorUserControl
	{
		public ProfitShareRebateControl()
		{
			InitializeComponent();
		}

		internal override ZArchitecture.ZGrid GetRateLineItemsGrid()
		{
			return Controls["ApplyToRateLineItemsGrid"] as ZArchitecture.ZGrid;
		}

		#region Binding

		protected override void SetBindings()
		{
			MinimumCalcEdit.DecimalPlaces = DecimalPlaces;
			MinimumCalcEdit.Decimals = DecimalPlaces;
			BasePriceCalcEdit.DecimalPlaces = DecimalPlaces;
			BasePriceCalcEdit.Decimals = DecimalPlaces;
			MaximumCalcEdit.DecimalPlaces = DecimalPlaces;
			MaximumCalcEdit.Decimals = DecimalPlaces;
			PercentCalcEdit.DecimalPlaces = DecimalPlaces;
			PercentCalcEdit.Decimals = DecimalPlaces;

			SetGridBinding(ApplyToRateLineItemsGrid, "ApplyToRateLineItems");

			SetBinding(PercentCalcEdit, "Decimal1");
			SetBinding(MinimumCalcEdit, "Decimal2");
			SetBinding(BasePriceCalcEdit, "Decimal3");
			SetBinding(MaximumCalcEdit, "Decimal4");
			SetBinding(ZeroWhenLoss, "Bool1");
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
