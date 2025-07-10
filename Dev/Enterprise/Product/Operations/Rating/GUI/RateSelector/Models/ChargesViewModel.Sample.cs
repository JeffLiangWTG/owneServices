#region SuppressResourceStringsCheckRegion

using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	[CodeAlive("Used in xaml designer")]
	public class ChargesViewModelSample : ChargesViewModel
	{
		public ChargesViewModelSample()
			: base("Freight")
		{
			var displayAmount = true;

			Add(new ChargeViewModelSample
			{
				ChargeCode = "FRT",
				Amount = 666,
				Currency = "USD",
				LocalAmount = 1000,
				LocalCurrency = "AUD",
				ChargeCodeDescription = "Freight",
				IsSelected = true,
				DisplayPrice = displayAmount
			});

			Add(new ChargeViewModelSample
			{
				ChargeCode = "XXX",
				ChargeCodeDescription = "XXX Description Looooooooonggg Description",
				IsSelected = true,
				IsIncluded = true,
				DisplayPrice = displayAmount
			});

			Add(new ChargeViewModelSample
			{
				ChargeCode = "YYY",
				ChargeCodeDescription = "YYY Description",
				IsSelected = true,
				IsIncluded = true,
				DisplayPrice = displayAmount
			});

			Add(new ChargeViewModelSample
			{
				ChargeCode = "ABCD",
				ChargeCodeError = "No mapping to universal code",
				ChargeCodeErrorLevel = ErrorLevel.Error,
				Amount = 100,
				Currency = "BTC",
				LocalAmount = 0,
				LocalCurrency = "AUD",
				LocalAmountError = "No exchange rate",
				ChargeCodeDescription = "Some carrier charge code",
				IsSelected = true,
				DisplayPrice = displayAmount
			});

			Add(new ChargeViewModelSample
			{
				ChargeCode = "WAR",
				Amount = 100,
				Currency = "EUR",
				LocalAmount = 150,
				LocalCurrency = "AUD",
				ChargeCodeDescription = "War",
				IsOptional = true,
				IsSelected = false,
				DisplayPrice = displayAmount
			});

			TotalPriceString = "2,499.00 BTC";
			TotalPriceError = "Error";
		}

		public new string TotalPriceString { get; set; }
		public new string TotalPriceError { get; set; }
	}
}

#endregion
