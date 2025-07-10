using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.US.Business
{
	public class FeeCalculationHelper
	{
		public FeeCalculationHelper(BusinessObjectFactory factory, ZDateTime dateForCalculation)
		{
			this.dateForCalculation = dateForCalculation;
			this.factory = factory;
		}
		readonly ZDateTime dateForCalculation;
		readonly BusinessObjectFactory factory;

		public decimal ManualSurchargeAmount
		{
			get { return GetFeeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge); }
		}

		public decimal MailFee
		{
			get { return GetFeeAmount(Core.Constants.USCustoms.FeeCodes.DutiableMail); }
		}

		public decimal InformalFeeAmount
		{
			get { return GetFeeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal); }
		}

		public static decimal GetDeminimus(BusinessObjectFactory factory)
		{
			var date = ZDateTime.Today;
			return factory?.GetCachedValue("FeeCalculationHelper|Deminimus" + ZDateTime.Today, () => new FeeCalculationHelper(factory, date).GetFeeAmount(RateTypes.Deminimus)) ?? decimal.Zero;
		}

		decimal GetFeeAmount(string feeCode)
		{
			var result = decimal.Zero;
			var feeSetting = GetCurrentRate(feeCode);
			if (feeSetting != null)
			{
				result = feeSetting.ZZF_Value;
			}
			return result;
		}

		RefCusTaxOrFee CurrentHMFRate
		{
			get
			{
				if (currentHMFRate == null)
				{
					currentHMFRate = GetCurrentRate(Core.Constants.USCustoms.FeeCodes.HMF);
				}
				return currentHMFRate;
			}
		}
		RefCusTaxOrFee currentHMFRate;

		public decimal HMFThresholdAmount
		{
			get
			{
				var result = decimal.Zero;
				if (CurrentHMFRate != null)
				{
					result = CurrentHMFRate.ZZF_Minimum;
				}
				return result;
			}
		}

		public decimal HMFRatePercentage
		{
			get
			{
				var result = decimal.Zero;
				if (CurrentHMFRate != null)
				{
					result = CurrentHMFRate.ZZF_Value;
				}
				return result;
			}
		}

		public RefCusTaxOrFee GetCurrentRate(ZString taxOrFeeCode)
		{
			return factory.GetCachedValue(taxOrFeeCode + dateForCalculation.ToString(), () =>
			{
				var loader = new RefCusTaxOrFee.Loader(factory);
				return loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.UnitedStates, taxOrFeeCode, dateForCalculation);
			});
		}

		public bool IsAdValorem(ZString taxOrFeeType)
		{
			return taxOrFeeType == "AVL";
		}

		public bool IsFlat(ZString taxOrFeeType)
		{
			return taxOrFeeType == "FLA";
		}

		public ZDecimal GetAmount(ZString taxOrFeeCode, ZDecimal customsValue, FeeCalculationInternalData ddpData)
		{
			var result = ZDecimal.Zero;
			var taxOrFee = GetCurrentRate(taxOrFeeCode);
			if (taxOrFee != null)
			{
				var taxOrFeeType = taxOrFee.TaxOrFeeType;
				if (taxOrFeeType != null)
				{
					if (IsFlat(taxOrFeeType.ZX0_TaxOrFeeType))
					{
						result = taxOrFee.ZZF_Value;
						ddpData.NoneCustomsValueAmount = result;
					}
					else if (IsAdValorem(taxOrFeeType.ZX0_TaxOrFeeType))
					{
						var rate = taxOrFee.ZZF_Value / 100m;
						result = (ZDecimal)(customsValue * rate);
						ddpData.PercentOfRate = taxOrFee.ZZF_Value;
					}
					else
					{
						result = ZDecimal.Zero;
					}
				}
			}
			return result;
		}
	}
}
