using System.Linq;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Services;
using Moq;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models
{
	public class CW1ChargeViewModelTest : RatingTestCase
	{
		public void TestIsValid()
		{
			var chargeViewModel = new CW1ChargeViewModel();
			AssertIsValid(chargeViewModel, ErrorLevel.Error, false);
			AssertIsValid(chargeViewModel, ErrorLevel.Warning, true);
			AssertIsValid(chargeViewModel, ErrorLevel.None, true);

			Assert(true);
		}

		void AssertIsValid(CW1ChargeViewModel chargeViewModel, ErrorLevel errorLevel, bool expectedResult)
		{
			chargeViewModel.ChargeCodeErrorLevel = errorLevel;
			AssertEquals("Expected IsValid to match the expected result", expectedResult, chargeViewModel.IsValid);
			AssertEquals("Expected ErrorLevel to match the assigned error level", errorLevel, chargeViewModel.ErrorLevel);
		}

		public void TestChargeCodeError_WhenRateInfoHasResult_ShouldNotHaveWarnings()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR);
			var rateLine = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, Constants.PkgUnit.Spool, "AUD");

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var mockedCurrencyConverter = new Mock<ICurrencyConverter>();
			mockedCurrencyConverter
				.Setup(s => s.Convert(It.IsAny<Money>(), It.IsAny<RefCurrency>()))
				.Returns(new Money(-1, GlbCompany.CurrentCompany.LocalCurrency, false));

			var context = new RateSelectorContext
			{
				Factory = Factory,
				CurrencyConverter = mockedCurrencyConverter.Object,
				Logger = new MemoryLogger()
			};

			var calculationResult = CalculationResult.CreateForTest(rateLine, 10, 20, 30, criteria);
			var rateInfo = new AutoRateInfo(calculationResult, parameters, Factory);

			var chargeViewModel = new CW1ChargeViewModel(rateInfo, context);
			AssertChargeCodeError(chargeViewModel, ErrorLevel.None, null);

			AssertEquals("The context logger should not have any logs when rate info has result.", 0, context.Logger.Logs.Count());
		}

		public void TestChargeCodeError_WhenRateInfoHasNoResult_ShouldHaveWarnings()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR);
			var rateLine = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, Constants.PkgUnit.Spool, "AUD");

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var mockedCurrencyConverter = new Mock<ICurrencyConverter>();
			mockedCurrencyConverter
				.Setup(s => s.Convert(It.IsAny<Money>(), It.IsAny<RefCurrency>()))
				.Returns(new Money(-1, GlbCompany.CurrentCompany.LocalCurrency, false));

			var context = new RateSelectorContext()
			{
				Factory = Factory,
				CurrencyConverter = mockedCurrencyConverter.Object,
				Logger = new MemoryLogger()
			};

			var rateInfo = new AutoRateInfo("Reason for calculation error", rateLine, parameters, Factory);

			var chargeViewModel = new CW1ChargeViewModel(rateInfo, context);
			AssertChargeCodeError(chargeViewModel, ErrorLevel.Warning, "Calculation failed due to Reason for calculation error");

			AssertGreaterThan("Logging should contain at least one entry.", context.Logger.Logs.Count(), 0);

			var logMessage = context.Logger.Logs.Single().Message;
			AssertEquals(
				"The log message should match the reason for calculation failure.",
				"Calculation failed due to Reason for calculation error",
				logMessage
			);
		}

		void AssertChargeCodeError(CW1ChargeViewModel chargeViewModel, ErrorLevel expectedErrorLevel, string expectedErrorString)
		{
			AssertEquals("Charge code error level mismatch", expectedErrorLevel, chargeViewModel.ChargeCodeErrorLevel);
			AssertEquals("Charge code error string mismatch", expectedErrorString, chargeViewModel.ChargeCodeError);
		}
	}
}
