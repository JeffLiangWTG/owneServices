using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ZA.Business
{
	public class WhsInventoryDutyAndTaxCalculator : Customs.Business.WhsInventoryDutyAndTaxCalculator
	{
		public WhsInventoryDutyAndTaxCalculator(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override void CalculateDutyAndTax(Dictionary<ZString, IZType> data, ZDecimal ratio, ZDateTime arrivalDate, ZDateTime valuationDate, ZString tariff, ZString countryOfOrigin, ZDecimal customsValue, ZDecimal customsQty1, ZString customsUQ1, ZDecimal customsQty2, ZString customsUQ2, ZDecimal customsQty3, ZString customsUQ3)
		{
			ZDecimal? dutyStandard = null;
			ZDecimal? duty12B = null;
			ZDecimal? allDuties = null;
			ZDecimal? vat = null;
			if (!tariff.IsEmpty)
			{
				customsValue = customsValue.Round(2);
				var loader = new TariffView.Loader(Factory);
				var tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, tariff, valuationDate);
				if (tariffView == null)
				{
					valuationDate = arrivalDate;
					tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, tariff, valuationDate);
				}
				if (tariffView != null)
				{
					var universalRateData = new WhsInventoryUniversalRateCalcData(Factory, valuationDate, countryOfOrigin, customsValue, customsQty1, customsUQ1, customsQty2, customsUQ2, customsQty3, customsUQ3);
					var dutyCalculator = new DutyCalculator(false, false, 2);
					dutyCalculator.CalculateAndPopulateDuty(universalRateData, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, tariff, ZString.Empty, ZDecimal.Zero);
					dutyCalculator.CalculateOtherDuties(universalRateData, GetRelatedTariffs(tariff, valuationDate), tariff);
					var duty12BValue = ZDecimal.Zero;
					var dutyStandardValue = ZDecimal.Zero;
					foreach (var pair in universalRateData.CountrySpecificValueList.Where(x => universalRateData.CountrySpecificTypeList.Contains(x.Key)))
					{
						if (pair.Key == Duty12B)
						{
							duty12BValue += pair.Value;
						}
						else
						{
							dutyStandardValue += pair.Value;
						}
					}
					dutyStandard = dutyStandardValue.Round(2);
					duty12B = duty12BValue.Round(2);
					allDuties = dutyStandard + duty12B;

					var country = MasterFiles.Business.RefCountry.LoadFromCountryCode(Factory, countryOfOrigin);
					vat = new ZDecimal(DutyCalculatorStrategy.CalculateTaxValue(false, country, GetTaxRate(), customsValue, dutyStandardValue + duty12BValue, ZDateTime.Today)).Round(2);
				}
			}
			data.Add(UniversalReferenceConstants.CusEntryPayTypes.Duty, GetValueToDisplay(dutyStandard, ratio));
			data.Add(Duty12B, GetValueToDisplay(duty12B, ratio));
			data.Add(UniversalReferenceConstants.CusEntryPayTypes.AllDuties, GetValueToDisplay(allDuties, ratio));
			data.Add(UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax, GetValueToDisplay(vat, ratio));
		}

		IZType GetValueToDisplay(ZDecimal? value, ZDecimal ratio)
		{
			if (value.HasValue)
			{
				return new ZDecimal(value.Value * ratio).Round(2);
			}
			else
			{
				return new ZString(NoRateAvailable);
			}
		}

		const string NoRateAvailable = "NO RATE AVAILABLE";
		const string Duty12B = "12B";

		ZDecimal GetTaxRate()
		{
			return Factory.GetCachedValue("ZATaxRate", () =>
			{
				var loader = new RefCusTaxOrFee.Loader(Factory);
				return loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax, ZDateTime.Today)?.ZZF_Value ?? ZDecimal.Zero;
			});
		}

		class WhsInventoryUniversalRateCalcData : UniversalRateCalcData
		{
			public WhsInventoryUniversalRateCalcData(BusinessObjectFactory factory, ZDateTime dateOfValuation, ZString countryOfOrigin, ZDecimal customsValue, ZDecimal customsQty1, ZString customsUQ1, ZDecimal customsQty2, ZString customsUQ2, ZDecimal customsQty3, ZString customsUQ3)
				: base(factory, dateOfValuation, customsValue, countryOfOrigin, UniversalReferenceConstants.PrimaryPreference.Standard)
			{
				UpdateDictionaryOrAddNew(OriginalUnitOfMeasureValueList, customsUQ1, customsQty1);
				UpdateDictionaryOrAddNew(OriginalUnitOfMeasureValueList, customsUQ2, customsQty2);
				UpdateDictionaryOrAddNew(OriginalUnitOfMeasureValueList, customsUQ3, customsQty3);
			}
		}

		class TariffDetail : ITariffDetail
		{
			public TariffDetail(TariffView cusTariff)
			{
				UniversalTariff = Argument.NotNull(cusTariff, nameof(cusTariff));
				UniversalTariffType = UniversalTariff.CusTariffType;
			}

			public TariffView UniversalTariff { get; }

			public RefCusTariffType UniversalTariffType { get; }

			public ZString FormulaSpecificValue => ZString.Empty;

			public ZString FormulaSpecificQuestion => ZString.Empty;

			public int FormulaSpecificValueScale => 0;

			public ZString Tariff => UniversalTariff.ZZ1_TariffCode;

			public ZString Type => UniversalTariffType.ZZI_TariffType;

			public ZBool ShouldBeExcluded => false;
		}

		IEnumerable<ITariffDetail> GetRelatedTariffs(ZString tariff, ZDateTime valuationDate)
		{
			foreach (var relatedTariff in new TariffView.Loader(Factory).GetEffectiveChildTariffs(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, tariff, valuationDate))
			{
				if (relatedTariff != null)
				{
					var relatedTariffRateType = relatedTariff.Rates.FirstOrDefault()?.ZZ2_ZZR_RateTypeCode ?? ZString.Empty;
					if (!relatedTariffRateType.IsEmpty && applicableRateTypes.Contains(relatedTariffRateType))
					{
						yield return new TariffDetail(relatedTariff);
					}
				}
			}
		}

		readonly IImmutableList<ZString> applicableRateTypes = new ZString[] {
			Universal.Constants.RateTypes.AdValoremExcise,
			Universal.Constants.RateTypes.Excise,
			Universal.Constants.RateTypes.Levy
		}.ToImmutableList();
	}
}
