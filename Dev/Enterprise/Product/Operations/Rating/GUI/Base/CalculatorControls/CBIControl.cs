using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public partial class CBIControl : BaseCombinedCalculatorControl
	{
		CombinedBreaksWithIncrementCalculator currentCalculator;
		public CBIControl()
		{
			InitializeComponent();
		}

		internal override void OnSwitched(Calculator viewCalculator)
		{
			base.OnSwitched(viewCalculator);
			if (currentCalculator != null)
			{
				currentCalculator.RateSelected -= LabourRateSelected;
			}

			if (viewCalculator is CombinedBreaksWithIncrementCalculator calc)
			{
				currentCalculator = calc;
				LabourRateSelected(currentCalculator.LabourHourCalculationEnabled);
				// Subscribe to RateSelected action in the CombinedBreaksWithIncrementCalculator to update the UI layout
				currentCalculator.RateSelected += LabourRateSelected;
			}
		}

		void LabourRateSelected(bool value)
		{
			if (value)
			{
				ShowLabourRateComponent();
			}
			else
			{
				ShowMaterialRateComponent();
			}
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

