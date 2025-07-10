using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.GUI
{
	public partial class PercentageBreaksControl : BaseCombinedCalculatorControl
	{
		public PercentageBreaksControl()
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				IncludeTaxCheckbox.Text = IncludeGST;
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

		#region Binding

		protected override void SetBindings()
		{
			base.SetBindings();

			if (ViewCalculatorForBinding.IsWiseRatesView)
			{
				SetGridBinding(ApplyToRateLineItemsGrid, "ApplyToIRateLineItems");
				SetGridBinding(RateLineItemsGrid, "IRateLineItems");
			}
			else
			{
				SetGridBinding(ApplyToRateLineItemsGrid, "ApplyToRateLineItems");
				SetGridBinding(RateLineItemsGrid, "RateLineItems");
			}

			SetBinding(BreaksBasedOnValuesCheckBox, "Bool4");
			SetBinding(IncludeTaxCheckbox, "Bool5");
		}

		internal override ZGrid GetRateLineItemsGrid()
		{
			return RateLineItemsGrid;
		}

		#endregion

		#region Grid Remove Action

		public override RemoveAction RemoveAction
		{
			get { return base.RemoveAction; }
			set
			{
				base.RemoveAction = value;

				if (ApplyToRateLineItemsGrid != null)
				{
					ApplyToRateLineItemsGrid.RemoveAction = value;
				}
			}
		}

		#endregion
	}
}
