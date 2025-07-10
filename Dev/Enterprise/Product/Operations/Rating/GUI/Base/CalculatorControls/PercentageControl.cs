using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.GUI
{
	public partial class PercentageControl : RateCalculatorUserControl
	{
		public PercentageControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				IncludeTaxCheckbox.Text = IncludeGST;
			}
		}

		internal override ZGrid GetRateLineItemsGrid()
		{
			return Controls["ApplyToRateLineItemsGrid"] as ZGrid;
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

			if (ViewCalculatorForBinding.IsWiseRatesView)
			{
				SetGridBinding(ApplyToRateLineItemsGrid, "ApplyToIRateLineItems");
			}
			else
			{
				SetGridBinding(ApplyToRateLineItemsGrid, "ApplyToRateLineItems");
			}

			SetBinding(PercentCalcEdit, "Decimal1");
			SetBinding(MinimumCalcEdit, "Decimal2");
			SetBinding(BasePriceCalcEdit, "Decimal3");
			SetBinding(MaximumCalcEdit, "Decimal4");
			SetBinding(RateCalcEdit, "Decimal5");
			SetBinding(ValueOrPartThereOfCalcEdit, "Decimal6");
			SetBinding(IncludeTaxCheckbox, "Bool1");
			SetBinding(GreaterChargeCheckbox, "Bool2");
			SetBinding(PartThereofCheckBox, "Bool3");
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				RatingHeader ratingHeader = dataSource as RatingHeader;
				GlbCompany companyForTax = ratingHeader?.Company ?? GlbCompany.CurrentCompany;
				IncludeTaxCheckbox.Visible = companyForTax.Country.IsGSTRegistered;
				PartThereofCheckBox_CheckedChanged(PartThereofCheckBox, EventArgs.Empty);
			}
		}

		#endregion

		#region Part There Of GUI Show / Hide

		void PartThereofCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			bool isPartThereofCalculation = PartThereofCheckBox.Checked;

			PercentLabel.Visible = !isPartThereofCalculation;
			PercentCalcEdit.Visible = !isPartThereofCalculation;

			RateCalcEdit.Visible = isPartThereofCalculation;
			RateLabel1.Visible = isPartThereofCalculation;
			RateLabel2.Visible = isPartThereofCalculation;
			ValueOrPartThereOfCalcEdit.Visible = isPartThereofCalculation;
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
