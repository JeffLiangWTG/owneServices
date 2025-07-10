using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.Rating.GUI.RateSelector.Services;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public sealed class CostBreakdownChargeViewModel : ChargeViewModel
	{
		public CostBreakdownChargeViewModel()
		{
		}

		public CostBreakdownChargeViewModel(BusinessObjectFactory factory, ICurrencyConverter currencyConverter) : base(factory, currencyConverter)
		{
		}

		public static CostBreakdownChargeViewModel New(BusinessObjectFactory factory, IBookingCostBreakdownCharge charge, bool displayCostInOriginalCurrency)
		{
			return New(factory, charge, displayCostInOriginalCurrency, new RefCurrenciesCurrencyConverter(factory));
		}

		public static CostBreakdownChargeViewModel New(BusinessObjectFactory factory, IBookingCostBreakdownCharge charge, bool displayCostInOriginalCurrency, ICurrencyConverter currencyConverter)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(charge, nameof(charge));

			var result = new CostBreakdownChargeViewModel(factory, currencyConverter)
			{
				Amount = charge.Amount,
				Currency = Argument.NotNullOrEmpty(charge.Currency, nameof(charge.Currency)),
				ChargeCode = charge.ChargeCode,
				ChargeCodeDescription = charge.ChargeCodeDescription
			};

			result.PopulateLocalAmounts(result.ChargeCode, result.Amount, result.Currency);
			result.displayCostInOriginalCurrency = displayCostInOriginalCurrency;

			return result;
		}

		public string CostString
		{
			get
			{
				if (displayCostInOriginalCurrency)
				{
					return Res.GetString("cbd25cac-2bb0-47aa-8262-a82c3849b048", "{0} {1}", Amount.ToString("N2"), Currency);
				}

				return Res.GetString("524428b9-ec78-45a5-a911-feb2ca999fe8", "{0} {1}", LocalAmount.ToString("N2"), LocalCurrency);
			}
		}

		bool displayCostInOriginalCurrency;
	}
}
