using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.Rating.GUI.RateSelector.Services;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public sealed class BookingEngineChargeViewModel : ChargeViewModel
	{
		public BookingEngineChargeViewModel()
		{
		}

		public BookingEngineChargeViewModel(BusinessObjectFactory factory, ICurrencyConverter currencyConverter)
			: base(factory, currencyConverter)
		{
		}

		public static BookingEngineChargeViewModel New(BusinessObjectFactory factory, IBookingRate rate)
		{
			return New(factory, rate, new RefCurrenciesCurrencyConverter(factory));
		}

		public static BookingEngineChargeViewModel New(BusinessObjectFactory factory, IBookingRate rate, ICurrencyConverter currencyConverter)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(rate, nameof(rate));

			var result = new BookingEngineChargeViewModel(factory, currencyConverter)
			{
				Amount = rate.Amount,
				Currency = Argument.NotNullOrEmpty(rate.Currency, nameof(rate.Currency)),
				ChargeCode = rate.ChargeCode,
				ChargeCodeDescription = rate.ChargeCodeDescription,
				IsSelected = true,
				IsIncluded = true
			};

			result.PopulateLocalAmounts(result.ChargeCode, result.Amount, result.Currency);

			return result;
		}

		public override ErrorLevel ErrorLevel => ErrorLevel.None;

		public override bool DisplayPrice => true;
	}
}
