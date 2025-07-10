using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefExchangeRateCalculatorValidation))]
	sealed class RefExchangeRateCalculatorValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBaseCurrencyValue()
		{
			AssertPositive(Calculator.BaseCurrencyValueInfo);
		}

		public void TestCheckQuoteCurrencyValue()
		{
			AssertPositive(Calculator.QuoteCurrencyValueInfo);
		}

		void AssertPositive(ZPropertyInfo targetInfo)
		{
			var valueCannotBeNegativeMessage = MandatoryValidation.ValueCannotBeNegativeMessage(targetInfo.HumanReadableName);
			var valueCannotBeZeroMessage = MandatoryValidation.ValueCannotBeZeroMessage(targetInfo.HumanReadableName);

			CombineAssertions(() =>
			{
				targetInfo.Value = ZDecimal.Zero;
				AssertHasError(targetInfo, valueCannotBeZeroMessage);
				targetInfo.Value = new ZDecimal(-1);
				AssertHasError(targetInfo, valueCannotBeNegativeMessage);
				targetInfo.Value = new ZDecimal(1);
				AssertNoErrors(targetInfo);
			});
		}

		RefExchangeRateCalculator Calculator
		{
			get
			{
				if (calculator == null)
				{
					var exchangeRate = RefExchangeRate.New(Factory);
					calculator = new RefExchangeRateCalculator(exchangeRate);
				}

				return calculator;
			}
		}
		RefExchangeRateCalculator calculator;
	}
}
