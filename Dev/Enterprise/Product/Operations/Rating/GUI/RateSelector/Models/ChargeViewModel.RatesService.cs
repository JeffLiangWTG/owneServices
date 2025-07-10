using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using WiseRates.Api.Model;
using WiseRates.Constants;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public class RatesServiceChargeViewModel : ChargeViewModel
	{
		public RatesServiceChargeViewModel()
		{
			// For tests
		}

		public RatesServiceChargeViewModel(Charge charge, AutoRateInfo autoRateInfo, RateSelectorContext context, OrgHeader carrier)
			: base(context?.Factory, context?.CurrencyConverter)
		{
			Context = Argument.NotNull(context, nameof(context));
			Argument.NotNull(context.RatesServiceResponse, nameof(context), "The context has no response from the Rates Service");
			RawCharge = Argument.NotNull(charge, nameof(charge));
			AutoRateInfo = autoRateInfo;
			this.carrier = carrier;

			// Populate ViewModel
			Currency = autoRateInfo?.Currency;
			Amount = autoRateInfo?.Amount ?? 0;
			IsIncluded = charge.ChargeType.HasFlag(ChargeType.Included);
			IsOptional = charge.ChargeType.HasFlag(ChargeType.Optional);
			CalculationDescription = autoRateInfo?.CalculationDescription;

			PopulateChargeCodeInfo();
			PopulateLocalAmounts(ChargeCode, Amount, Currency);
		}

		public Charge RawCharge { get; }
		readonly OrgHeader carrier;

		protected virtual void PopulateChargeCodeInfo(IDictionary<string, AccChargeCode> cache = null)
		{
			if (string.IsNullOrEmpty(RawCharge.ChargeCode))
			{
				// Carrier charge code
				AccChargeCode = null;
				ChargeCode = RawCharge.CarrierChargeCodeInfo?.Code;
				ChargeCodeDescription = RawCharge.CarrierChargeCodeInfo?.Description;
				ChargeCodeErrorLevel = ErrorLevel.Error;
				ChargeCodeError = Res.GetString("cc5dc2d1-2258-4fca-8638-35c475363ed6", "The charge code has NO mapping with any Universal Charge Code.");
			}
			else
			{
				if (cache == null || !cache.TryGetValue(RawCharge.ChargeCode, out var chargeCode))
				{
					string error;

					var chargeCodeInfo = RawCharge.ChargeCodeInfo ?? Context.RatesServiceResponse?.ChargeCodes.FirstOrDefault(r => r.Code == RawCharge.ChargeCode);

					(chargeCode, error) = WiseRatesConverter.ConvertChargeCode(RawCharge.ChargeCode, Factory, RawCharge.CarrierChargeCodeInfo?.Code, carrier, WRConstants.TransportModes.AIR);
					if (chargeCode == null)
					{
						// Universal charge code
						AccChargeCode = null;
						ChargeCode = chargeCodeInfo?.Code;
						ChargeCodeDescription = chargeCodeInfo?.Description;
						ChargeCodeErrorLevel = ErrorLevel.Warning;
						ChargeCodeError = error;

						return;
					}
				}

				// Local charge code
				AccChargeCode = chargeCode;
				ChargeCode = chargeCode.AC_Code;
				ChargeCodeDescription = chargeCode.AC_DescMultilingual;
				ChargeCodeErrorLevel = ErrorLevel.None;
				ChargeCodeError = null;

				if (cache != null)
				{
					cache[RawCharge.ChargeCode] = chargeCode;
				}

				if (RawCharge.Restricted.HasValue && RawCharge.Restricted.Value && RawCharge.ChargeCode == FRTChargeCodeMark)
				{
					ChargeCodeErrorLevel = ErrorLevel.Warning;
					ChargeCodeError = RawCharge.Applicability;
					return;
				}

				if (AutoRateInfo != null && RawCharge.ChargeCode == FRTChargeCodeMark)
				{
					// Chargeable Description may contain some notes about how those chargeable was calculated, as it may not be obvious.
					// For example, Higher Break Lower Rate may be applied. So, it makes sense to display some warning next to the charge.
					var descriptions = AutoRateInfo.Bases
						.Select(b => b.Chargeable.Description)
						.Where(b => !string.IsNullOrEmpty(b))
						.ToList();

					if (descriptions.Count > 0)
					{
						ChargeCodeErrorLevel = ErrorLevel.Warning;
						ChargeCodeError = string.Join(System.Environment.NewLine, descriptions);
					}
				}
			}
		}

		RateSelectorContext Context { get; }
		const string FRTChargeCodeMark = "FRT";
	}
}
