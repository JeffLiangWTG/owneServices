using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using RateTypes = Enterprise.Customs.Universal.Constants.RateTypes;

namespace Enterprise.Customs.ZA.Business
{
	public static class UniversalReferenceDataHelper
	{
		public static KeyValuePair<ZString, ZString> GetTradeGroupByPreference(TariffView tariff, params IZZRateSelectionCriteria[] criterias)
		{
			var tradeGroup = new KeyValuePair<ZString, ZString>();
			if (tariff != null)
			{
				var tradeGroupInfo = Customs.Business.UniversalReferenceDataHelper.GetRateSelectionCriteriaInfo(tariff, criterias)
					.FirstOrDefault(x => !x.ZZS_Preference.IsEmpty && criterias.Any(c => x.MatchExcludingTradeGroup(c)));

				if (tradeGroupInfo != null)
				{
					tradeGroup = new KeyValuePair<ZString, ZString>(tradeGroupInfo.ZZA_TradeGroup, tradeGroupInfo.ZZA_Description);
				}
			}
			return tradeGroup;
		}

		public static CodeDescriptionPairList GetDA63PartList(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("DA63AdditionalDutyLookups|PartList", () =>
			{
				var result = new CodeDescriptionPairList();

				var codes = CusRefRateCodeView.Loader.Load(
					factory
					, Core.Constants.CountryCodes.SouthAfrica
					, new RateCodeLoadCriteria()
					{
						RateTypesToInclude = new ZString[]
						{
							Constants.RateTypes.AntiDumping,
							Constants.RateTypes.Levy,
							Constants.RateTypes.AdValoremExcise,
							Constants.RateTypes.Excise,
						}
					});
				result.AddRange(codes);
				result.AddRange(factory.GetFullProvisionalPaymentTypeList());
				result.Sort();

				return result;
			});
		}

		public static CusRefRateCodeView[] GetRateCodesWithRateType(this BusinessObjectFactory factory, string rateType)
		{
			return factory.GetCachedValue("ZA_RateCodesWith" + rateType, () => CusRefRateCodeView.Loader.LoadByRateType(factory
				, Core.Constants.CountryCodes.SouthAfrica
				, rateType))
			;
		}

		public static CusRefRateCodeView[] GetRateCodesWithEX1(this BusinessObjectFactory factory)
		{
			return GetRateCodesWithRateType(factory, Constants.RateTypes.AdValoremExcise);
		}

		public static TariffView GetCusTariff(this BusinessObjectFactory factory, ZString type, ZString tariff, ZDateTime assessmentDate, string relatedTariffCode = null)
		{
			return factory == null ? null : new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, type, tariff, assessmentDate, relatedTariffCode);
		}

		public static TariffView GetCusTariffIncludingCheckDigit(this BusinessObjectFactory factory, ZString type, ZString tariff, ZDateTime assessmentDate, ZString checkDigit, string relatedTariffCode = null)
		{
			return factory == null ? null : new TariffView.Loader(factory).LoadMostRecentCachedTariffWithAttribute(Core.Constants.CountryCodes.SouthAfrica, type, tariff, assessmentDate, UniversalReferenceConstants.TariffAttributes.CheckDigit, checkDigit, relatedTariffCode);
		}

		public static ZString GetTariffCodeWithCheckDigit(this TariffView tariff)
		{
			var result = ZString.Empty;
			if (tariff != null)
			{
				var rateType = tariff.Rates.FirstOrDefault()?.ZZ2_ZZR_RateTypeCode ?? ZString.Empty;
				var checkDigit = tariff.GetAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit)?.ZZ3_Value ?? ZString.Empty;
				if (rateType == RateTypes.Duty)
				{
					result = ZString.Format("{0}{1}", tariff.ZZ1_TariffCode.PadRight(8, '0'), checkDigit);
				}
				else
				{
					result = ZString.Format("{0}{1}", tariff.ZZ1_TariffCode, checkDigit);
				}
			}
			return result;
		}

		public static RateView GetCusRate(this TariffView tariff)
		{
			return tariff?.Rates.OfType<RateView>().FirstOrDefault(); // A Tariff should be linked to only one RateCode
		}

		public static bool IsDutiableCustomsProcedure(this RefCusProcedure procedure)
		{
			return procedure != null && (procedure.ZZ6_CalculateDuty || procedure.ZZ6_LandedCost);
		}

		public static bool IsApplicableForDutyCalculation(this TariffView tariff, RefCusProcedure procedure)
		{
			return procedure != null && tariff.IsApplicableForDutyCalculation(procedure.IsDutiableCustomsProcedure(), procedure.Concessions);
		}

		public static bool IsApplicableForDutyCalculation(this TariffView tariff, bool isDutiableCustomsProcedure, IEnumerable<ZString> concessions)
		{
			var result = false;

			if (tariff != null)
			{
				var tariffType = tariff.CusTariffType?.ZZI_TariffType ?? ZString.Empty;
				if (tariff.IsPayableDuty())
				{
					result = isDutiableCustomsProcedure && (!concessions.ContainsSpecificTariffType(tariff.Factory) || concessions.Contains(tariffType));
				}
				else
				{
					result = concessions.Contains(tariffType.Left(1));
				}
			}

			return result;
		}

		public static bool ContainsSpecificTariffType(this IEnumerable<ZString> concessions, BusinessObjectFactory factory)
		{
			var list = RefCusTariffTypeList.GetCachedList(factory, Core.Constants.CountryCodes.SouthAfrica);
			return concessions.Any(x => list.ContainsCode(x));
		}

		public static ZString FirstNonSpecificTariffType(this IEnumerable<ZString> concessions, BusinessObjectFactory factory)
		{
			var list = RefCusTariffTypeList.GetCachedList(factory, Core.Constants.CountryCodes.SouthAfrica);
			return concessions.FirstOrDefault(x => !list.ContainsCode(x));
		}

		public static bool IsPayableDuty(this TariffView tariff)
		{
			return tariff?.Factory.GetCachedValue(FormattableString.Invariant($"ZA_IsPayableDuty_{tariff.PK}"), () =>
			{
				var result = false;
				var rate = tariff?.GetCusRate();
				if (rate != null)
				{
					result = rate.CusRateType?.ZZR_IsPayable ?? ZBool.False;
				}
				return result;
			}) ?? false;
		}

		public static bool IsRefund(this RefCusTariffType tariffType)
		{
			return tariffType?.Factory.GetCachedValue(FormattableString.Invariant($"ZA_IsRefund_{tariffType.PK}"), () => LoadCusRateTypeFromTariffTypeAndRateType(tariffType, RateTypes.Refund) != null) ?? false;
		}

		static RefCusRateType LoadCusRateTypeFromTariffTypeAndRateType(RefCusTariffType tariffType, ZString rateType)
		{
			RefCusRateType result = null;
			if (tariffType != null)
			{
				var rateCodes = CusRefRateCodeView.Loader.LoadByRateCode(tariffType.Factory, Core.Constants.CountryCodes.SouthAfrica, tariffType.ZZI_TariffType);
				if (rateCodes.Where(x => x.ZY1_RateType == rateType).Any())
				{
					result = RefCusRateType.Loader.Load(tariffType.Factory, Core.Constants.CountryCodes.SouthAfrica, rateType);
				}
			}
			return result;
		}

		static TariffView LoadCusTariffFromTariffType(RefCusTariffType tariffType, BusinessObjectFactory factory)
		{
			TariffView result = null;
			if (tariffType != null)
			{
				var query = new ZQuery(TariffViewSchema.ZZ1_ZZI_TariffType, tariffType.PK);
				query.AddToFilter(TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.SouthAfrica);
				result = factory.LoadTop1<TariffView>(query);
			}
			return result;
		}

		public static bool IsPayableDutyExcludingAntiDumping(this RefCusTariffType tariffType)
		{
			var result = false;
			if (tariffType != null)
			{
				var tariff = LoadCusTariffFromTariffType(tariffType, tariffType.Factory);
				var rateType = tariff?.Rates.FirstOrDefault()?.CusRateType;
				result = rateType != null && rateType.ZZR_IsPayable && rateType.ZZR_RateType != Constants.RateTypes.AntiDumping;
			}
			return result;
		}

		public static bool HasEntryStatusGotAttribute(this ZString entryStatus, BusinessObjectFactory factory, ZDateTime assessmentDate, ZString attributeName)
		{
			var result = false;
			if (factory != null)
			{
				result = CustomsStatusAttributeHelper.HasAttribute(factory, attributeName, entryStatus, Core.Constants.CountryCodes.SouthAfrica, assessmentDate);
			}
			return result;
		}

		public static IDictionary<RefCusTariffType, List<TariffView>> GetValidRefCusTariffSortedDictionary(this RefCusProcedure cusProcedure, ZString tariffType, ZString tariff, ZDateTime assessmentDate)
		{
			var dictionary = new SortedDictionary<RefCusTariffType, List<TariffView>>(new RefCusTariffTypeComparer());
			if (cusProcedure != null && !tariff.IsEmpty && !tariffType.IsEmpty && assessmentDate.IsValid)
			{
				var isDutiableCustomsProcedure = cusProcedure.IsDutiableCustomsProcedure();
				var concession = cusProcedure.Concessions;
				foreach (var relatedTariff in new TariffView.Loader(cusProcedure.Factory).GetEffectiveChildTariffs(Core.Constants.CountryCodes.SouthAfrica, tariffType, tariff, assessmentDate))
				{
					if (relatedTariff.IsApplicableForDutyCalculation(isDutiableCustomsProcedure, concession))
					{
						List<TariffView> list = null;
						var relatedTariffFromType = relatedTariff.CusTariffType;
						if (!dictionary.TryGetValue(relatedTariffFromType, out list))
						{
							list = new List<TariffView>();
							dictionary.Add(relatedTariffFromType, list);
						}
						list.Add(relatedTariff);
					}
				}
			}
			return dictionary;
		}

		public static CodeDescriptionPairList GetTaxTypeList(this BusinessObjectFactory factory, ZDateTime assessmentDate)
		{
			return RefCusTaxOrFee.Loader.GetList(factory, Core.Constants.CountryCodes.SouthAfrica, assessmentDate);
		}

		class RefCusTariffTypeComparer : Comparer<RefCusTariffType>
		{
			public override int Compare(RefCusTariffType x, RefCusTariffType y)
			{
				return string.Compare(x.ZZI_TariffType, y.ZZI_TariffType, StringComparison.Ordinal);
			}
		}
	}
}
