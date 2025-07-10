using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;

namespace Enterprise.Rating.GUI
{
	public partial class DisbursementInterestControl : RateCalculatorUserControl
	{
		public DisbursementInterestControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				IncludeTaxCheckbox.Text = IncludeGST;
				var hint = IncludeTaxCheckbox.Extensions.Get<IHintExtension>();
				hint.Description = Res.GetString("0edd58ca-76e3-48fc-986f-9da42c3401e8"
					, "Use this flag to calculate Disbursement Interest on charges {0} inclusive"
					, GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			}
		}

		#region Binding

		internal override ZGrid GetRateLineItemsGrid()
		{
			return Controls["ApplyToRateLineItemsGrid"] as ZGrid;
		}

		protected override void SetBindings()
		{
			CurrentPrimeRateCalcEdit.DecimalPlaces = DecimalPlaces;
			CurrentPrimeRateCalcEdit.Decimals = DecimalPlaces;
			UpliftCalcEdit.DecimalPlaces = DecimalPlaces;
			UpliftCalcEdit.Decimals = DecimalPlaces;
			EffectiveRateCalcEdit.DecimalPlaces = DecimalPlaces;
			EffectiveRateCalcEdit.Decimals = DecimalPlaces;

			if (ViewCalculatorForBinding.IsWiseRatesView)
			{
				SetGridBinding(ApplyToRateLineItemsGrid, "ApplyToIRateLineItems");
			}
			else
			{
				SetGridBinding(ApplyToRateLineItemsGrid, "ApplyToRateLineItems");
			}

			SetBinding(CurrentPrimeRateCalcEdit, "Decimal1");
			SetBinding(UpliftCalcEdit, "Decimal2");
			SetBinding(EffectiveRateCalcEdit, "Decimal3");
			SetBinding(AdjustmentDaysCalcEdit, "Decimal4");
			SetBinding(CreditTermsLabel, "String1");
			SetBinding(OutstandingDaysCheckBox, "Bool1");
			SetBinding(IncludeTaxCheckbox, "Bool2");
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
