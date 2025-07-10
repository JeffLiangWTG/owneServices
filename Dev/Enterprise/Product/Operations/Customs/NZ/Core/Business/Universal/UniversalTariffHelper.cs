using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using RateCodeLoader = Enterprise.Customs.Universal.CusRefRateCodeView.Loader;
using TariffLoader = Enterprise.Customs.Universal.TariffView.Loader;
using TradeGroupLoader = Enterprise.Customs.Universal.CusRefTradeGroupView.Loader;

namespace Enterprise.Customs.NZ.Business
{
	public static class UniversalTariffHelper
	{
		public static ZString GetDescription(BusinessObjectFactory factory, ZString tariffCode)
		{
			if (UseRefDatabaseData)
			{
				var tariff = GetTariff(factory, tariffCode) as TariffView;
				return tariff?.ZZ1_Description ?? ZString.Empty;
			}
			else
			{
				return NZCClassification.GetDescriptionForCompleteCode(factory, tariffCode);
			}
		}

		public static ITariff GetTariff(BusinessObjectFactory factory, ZString tariffCode)
		{
			if (UseRefDatabaseData)
			{
				return GetTariffLoader(factory).LoadLatestCachedTariff(Core.Constants.CountryCodes.NewZealand, Constants.TariffTypes.HarmonizedSystem, tariffCode);
			}
			else
			{
				return NZCClassification.GetClassForCompleteCode(factory, tariffCode);
			}
		}

		public static ITariff GetTariff(BusinessObjectFactory factory, ZString tariffCode, ZDateTime valuationDate)
		{
			if (UseRefDatabaseData)
			{
				return GetTariffLoader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.NewZealand, Constants.TariffTypes.HarmonizedSystem, tariffCode, valuationDate);
			}
			else
			{
				return NZCClassification.GetClassForCompleteCode(factory, tariffCode, valuationDate);
			}
		}

		public static ZString GetStatisticalUnit(ITariff tariff)
		{
			if (UseRefDatabaseData)
			{
				return (tariff as TariffView)?.ZZ1_ZZ8_UQ1 ?? ZString.Empty;
			}
			else
			{
				return (tariff as NZCClassification)?.U0_StatisticalUnit ?? ZString.Empty;
			}
		}

		public static ZString GetSupplementaryUnitForInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			if (UseRefDatabaseData)
			{
				return invoiceLine.UniversalTariff?.ZZ1_ZZ8_UQ2 ?? ZString.Empty;
			}
			else
			{
				return (invoiceLine.Tariff as NZCClassification)?.U0_SupplementaryUnitForInvoiceLine ?? ZString.Empty;
			}
		}

		public static CusRefTradeGroupView[] GetTradeGroupsFromCountry(ZString countryOfOrigin, ZDateTime dateForDutyRate, BusinessObjectFactory factory, string preferenceCode = "")
		{
			if (countryOfOrigin.IsEmpty || preferenceCode == UniversalReferenceConstants.TariffCodes.NotQualifies)
			{
				return Array.Empty<CusRefTradeGroupView>();
			}
			else
			{
				var loader = GetTradeGroupLoader(factory);
				return loader.Load(Core.Constants.CountryCodes.NewZealand, dateForDutyRate, countryOfOrigin);
			}
		}

		[SuppressWeaklyTypedCollectionMessage]
		public static IList GetConcessionList(BusinessObjectFactory factory, ZString tariffNum, ZDateTime valuationDate, string goodsOrigin = "", string preferenceCode = "")
		{
			if (UseRefDatabaseData)
			{
				if (preferenceCode == UniversalReferenceConstants.TariffCodes.NotQualifies)
				{
					return factory.GetCachedValue(ConcessionListKeyString + "EmptyList", () => new CodeDescriptionPairList());
				}
				else
				{
					return factory.GetCachedValue(ConcessionListKeyString + tariffNum + goodsOrigin + valuationDate.ToString(), () =>
					{
						var tradeGroups = GetTradeGroupsFromCountry(goodsOrigin, ZDateTime.Today, factory);
						var tariff = GetTariff(factory, tariffNum) as TariffView;
						var concessions = tariff?.Rates?.SelectMany(r => r.RateApplicabilities.ToArray());
						var codeList = new CodeDescriptionPairList();
						var hashSet = new HashSet<string>();
						concessions?.ForEach(c =>
						{
							if (!c.ZZT_OrderNumber.IsEmpty
								&& c.ZZT_StartDate <= valuationDate && c.ZZT_EndDate >= valuationDate
								&& (string.IsNullOrEmpty(goodsOrigin) || tradeGroups.Any(g => g.ZZA_TradeGroup == c.TradeGroupCode))
								&& !hashSet.Contains(c.ZZT_OrderNumber))
							{
								codeList.AddPair(c.ZZT_OrderNumber);
								hashSet.Add(c.ZZT_OrderNumber);
							}
						});
						codeList.Sort();
						return codeList;
					});
				}
			}
			else
			{
				var filter = NonDependentNZCConcessionCollection.ConcessionLinkQuery(tariffNum);
				var result = new NonDependentNZCConcessionCollection(factory, filter);
				result.DefaultModuleFilterFields(tariffNum);
				return result;
			}
		}

		public static bool ApplicabilityPredict(CusRefApplicabilityView applicability, ZString orderNumber, ZString tradeGroup, ZDateTime valuationDate)
			=> applicability.ZZT_OrderNumber == orderNumber && applicability.TradeGroupCode == tradeGroup
				&& applicability.ZZT_StartDate <= valuationDate && applicability.ZZT_EndDate >= valuationDate;

		public static (ITableSchema table, ZQuery query) GetTariffFetchHint(BusinessObjectFactory factory, ZString tariffCode)
			=> GetTariffFetchHint(factory, tariffCode, ZDateTime.Empty);

		public static (ITableSchema table, ZQuery query) GetTariffFetchHint(BusinessObjectFactory factory, ZString tariffCode, ZDateTime valuationDate)
		{
			if (UseRefDatabaseData)
			{
				var filter = TariffLoader.GetEffectiveTariffFilter(factory, Core.Constants.CountryCodes.NewZealand, Constants.TariffTypes.HarmonizedSystem, tariffCode, valuationDate);
				return (TariffViewSchema.Instance, filter);
			}
			else
			{
				var filter = valuationDate.IsValid ? NZCClassification.GetDateForDutyRateClassificationFilter(tariffCode, valuationDate) : new ZQuery(NZCClassificationSchema.U0_Tariff, tariffCode);
				return (NZCClassificationSchema.Instance, filter);
			}
		}

		public static (ITableSchema table, ZQuery query) GetLevyRateFetchHint(BusinessObjectFactory factory, ZGuid tariffPK, ZDateTime valuationDate)
		{
			var rateCodePk = RateCodeLoader.LoadByRateType(factory, Core.Constants.CountryCodes.NewZealand, Constants.RateTypes.Levy)?.FirstOrDefault()?.PK ?? ZGuid.Empty;
			ZQuery filter = null;
			if (!rateCodePk.IsEmpty)
			{
				filter = new ZQuery();
				filter.AddToFilter(RateViewSchema.ZZ2_ZZ1_ParentTariffOrNationalCode, tariffPK);
				filter.AddToFilter(RateViewSchema.ZZ2_StartDate, SQLComparisonOperator.LessThanOrEqualTo, valuationDate);
				filter.AddToFilter(RateViewSchema.ZZ2_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, valuationDate);
				filter.AddToFilter(RateViewSchema.ZZ2_ZY1_RateCode, rateCodePk);
			}
			return (RateViewSchema.Instance, filter);
		}

		public static (ITableSchema table, ZQuery query) GetRateFetchHint(ZGuid tariffPK)
		{
			var filter = new ZQuery();
			filter.AddToFilter(RateViewSchema.ZZ2_ZZ1_ParentTariffOrNationalCode, tariffPK);
			return (RateViewSchema.Instance, filter);
		}

		public static (ITableSchema table, ZQuery query) GetConcessionFetchHint(ZGuid ratePK)
		{
			var filter = new ZQuery();
			filter.AddToFilter(CusRefApplicabilityViewSchema.ZZT_ZZ2_Rate, ratePK);
			return (CusRefApplicabilityViewSchema.Instance, filter);
		}

		public static bool UseRefDatabaseData => NZCustomsDataRegistry.Instance.UseRefDatabaseData.Value;

		public static IMultilingualString GetInvalidConcessionCodeWarningMessage(ZString code) => ResString.GetMultilingualString("NZUniversalTariffHelper|59DA13AC-6270-4FD4-9D68-8C26DBE7D35B", "Concession Code [{0}] not recognized - This either means you're using an invalid concession code, the concession code is unpublished, or your Tariff Data is out of date.", code);

		static TariffLoader GetTariffLoader(BusinessObjectFactory factory) => factory.GetCachedValue(TariffLoaderKeyString, () => new TariffLoader(factory));

		static TradeGroupLoader GetTradeGroupLoader(BusinessObjectFactory factory) => factory.GetCachedValue(GroupLoaderKeyString, () => new TradeGroupLoader(factory));

		const string GroupLoaderKeyString = "NZUniversalTariffHelper|CusRefTradeGroupLoader";
		const string TariffLoaderKeyString = "NZUniversalTariffHelper|TariffLoader";
		const string ConcessionListKeyString = "NZUniversalTariffHelper|ConcessionList";
		public const string StandardDutyRateGroupCode = "STD";
	}
}
