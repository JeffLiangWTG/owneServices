using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CommonInvoiceLineCalculationsUserControlTest : TestCaseWithFactory
	{
		public void TesCurrentInvoiceLabel()
		{
			using var control = new CommonInvoiceLineCalculationsUserControl();
			CombineAssertions("CurrentInvoiceLabel assertions", () =>
			{
				control.AssertContainsControl<ZLabel>(nameof(CommonInvoiceLineCalculationsUserControl.CurrentInvoiceLabel), x => x.WithCaption("Current Invoice"));
			});
		}

		public void TestBalanceConvertToLocalCurrencyControl()
		{
			using var control = new CommonInvoiceLineCalculationsUserControl();
			CombineAssertions("BalanceConvertToLocalCurrencyControl assertions", () =>
			{
				control.AssertContainsControl<ConvertToLocalCurrencyControl>(nameof(CommonInvoiceLineCalculationsUserControl.BalanceConvertToLocalCurrencyControl), x => x
					.WithBindToAmount(nameof(BaseJobComInvoiceLine.JI_Calc_Balance))
					.WithBindToUnit(nameof(BaseJobComInvoiceLine.JI_RX_NKLinePriceCurr))
					.WithCaption("Balance")
					.WithFullDescription("Remaining balance for entered lines on current invoice.")
				);
			});
		}

		public void TestLinesEnteredConvertToLocalCurrencyControl()
		{
			using var control = new CommonInvoiceLineCalculationsUserControl();
			CombineAssertions("LinesEnteredConvertToLocalCurrencyControl assertions", () =>
			{
				control.AssertContainsControl<ConvertToLocalCurrencyControl>(nameof(CommonInvoiceLineCalculationsUserControl.LinesEnteredConvertToLocalCurrencyControl), x => x
					.WithBindToAmount(nameof(BaseJobComInvoiceLine.JI_Calc_LinesEntered))
					.WithBindToUnit(nameof(BaseJobComInvoiceLine.JI_RX_NKLinePriceCurr))
					.WithCaption("Lines Entered")
					.WithFullDescription("The total amount of lines entered so far.")
				);
			});
		}

		public void TestLinesTotalConvertToLocalCurrencyControl()
		{
			using var control = new CommonInvoiceLineCalculationsUserControl();
			CombineAssertions("LinesTotalConvertToLocalCurrencyControl assertions", () =>
			{
				control.AssertContainsControl<ConvertToLocalCurrencyControl>(nameof(CommonInvoiceLineCalculationsUserControl.LinesTotalConvertToLocalCurrencyControl), x => x
					.WithBindToAmount(nameof(BaseJobComInvoiceLine.JI_Calc_LinesTotal))
					.WithBindToUnit(nameof(BaseJobComInvoiceLine.JI_RX_NKLinePriceCurr))
					.WithCaption("Total Expected")
					.WithShortCaption("Expected")
					.WithFullDescription("The expected line total amount.")
				);
			});
		}

		public void TesSummaryLabel()
		{
			using var control = new CommonInvoiceLineCalculationsUserControl();
			CombineAssertions("TitleLabel assertions", () =>
			{
				control.AssertContainsControl<ZLabel>(nameof(CommonInvoiceLineCalculationsUserControl.SummaryLabel), x => x.WithCaption("Line Calculations"));
			});
		}

		public void TestCIFConvertToLocalCurrencyControl()
		{
			using var control = new CommonInvoiceLineCalculationsUserControl();
			CombineAssertions("CIFConvertToLocalCurrencyControl assertions", () =>
			{
				control.AssertContainsControl<ConvertToLocalCurrencyControl>(nameof(CommonInvoiceLineCalculationsUserControl.CIFConvertToLocalCurrencyControl), x => x
					.WithBindToAmount(nameof(BaseJobComInvoiceLine.JI_Calc_CIF))
					.WithBindToUnit(nameof(BaseJobComInvoiceLine.JI_RX_NKLinePriceCurr))
					.WithCaption("CIF Value")
					.WithShortCaption("CIF")
					.WithFullDescription("CIF value for current line item.")
				);
			});
		}

		public void TestInsuranceInInvoiceCurrConvertToLocalCurrencyControl()
		{
			using var control = new CommonInvoiceLineCalculationsUserControl();
			CombineAssertions("InsuranceInInvoiceCurrConvertToLocalCurrencyControl assertions", () =>
			{
				control.AssertContainsControl<ConvertToLocalCurrencyControl>(nameof(CommonInvoiceLineCalculationsUserControl.InsuranceInInvoiceCurrConvertToLocalCurrencyControl), x => x
					.WithBindToAmount(nameof(BaseJobComInvoiceLine.JI_Calc_InsuranceInInvoiceCurr))
					.WithBindToUnit(nameof(BaseJobComInvoiceLine.JI_RX_NKLinePriceCurr))
					.WithCaption("Insurance Amount")
					.WithShortCaption("Insurance")
					.WithFullDescription("Insurance charges for current line item.")
				);
			});
		}

		public void TestFreightInInvoiceCurrConvertToLocalCurrencyControl()
		{
			using var control = new CommonInvoiceLineCalculationsUserControl();
			CombineAssertions("FreightInInvoiceCurrConvertToLocalCurrencyControl assertions", () =>
			{
				control.AssertContainsControl<ConvertToLocalCurrencyControl>(nameof(CommonInvoiceLineCalculationsUserControl.FreightInInvoiceCurrConvertToLocalCurrencyControl), x => x
					.WithBindToAmount(nameof(BaseJobComInvoiceLine.JI_Calc_FreightInInvoiceCurr))
					.WithBindToUnit(nameof(BaseJobComInvoiceLine.JI_RX_NKLinePriceCurr))
					.WithCaption("Freight")
					.WithFullDescription("Freight Charges for current line item.")
				);
			});
		}

		public void TestFOBConvertToLocalCurrencyControl()
		{
			using var control = new CommonInvoiceLineCalculationsUserControl();
			CombineAssertions("FOBConvertToLocalCurrencyControl assertions", () =>
			{
				control.AssertContainsControl<ConvertToLocalCurrencyControl>(nameof(CommonInvoiceLineCalculationsUserControl.FOBConvertToLocalCurrencyControl), x => x
					.WithBindToAmount(nameof(BaseJobComInvoiceLine.JI_Calc_FOB))
					.WithBindToUnit(nameof(BaseJobComInvoiceLine.JI_RX_NKLinePriceCurr))
					.WithCaption("FOB Value")
					.WithFullDescription("FOB value for current line item.")
				);
			});
		}

		public void TestGSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl()
		{
			using var control = new CommonInvoiceLineCalculationsUserControl();
			CombineAssertions("GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl assertions", () =>
			{
				control.AssertContainsControl<ConvertToLocalCurrencyControl>(nameof(CommonInvoiceLineCalculationsUserControl.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl), x => x
					.WithBindToAmount(nameof(BaseJobComInvoiceLine.JI_Calc_GSTVATAmountIncludingWHEstimate))
					.WithBindToUnit(nameof(BaseJobComInvoiceLine.JI_RX_LocalCurrency))
					.WithCaption("{0} Amount")
					.WithFullDescription("{0} value for current line item")
				);
			});
		}

		public void TestDutyAmountIncludingWHEstimateConvertToLocalCurrencyControl()
		{
			using var control = new CommonInvoiceLineCalculationsUserControl();
			CombineAssertions("DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl assertions", () =>
			{
				control.AssertContainsControl<ConvertToLocalCurrencyControl>(nameof(CommonInvoiceLineCalculationsUserControl.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl), x => x
					.WithBindToAmount(nameof(BaseJobComInvoiceLine.JI_Calc_DutyAmountIncludingWHEstimate))
					.WithBindToUnit(nameof(BaseJobComInvoiceLine.JI_RX_LocalCurrency))
					.WithCaption("Duty")
					.WithFullDescription("Duty value for current line item.")
				);
			});
		}
	}
}
