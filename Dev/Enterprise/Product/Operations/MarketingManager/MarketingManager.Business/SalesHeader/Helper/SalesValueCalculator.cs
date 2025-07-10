using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class SalesValueCalculator
	{
		public SalesValueCalculator(BusinessObjectFactory factory, ZGuid viewingOrgPk, OrgSalesRevenueCalculator revenueCalculator)
		{
			this.factory = factory;
			this.viewingOrgPk = viewingOrgPk;
			this.revenueCalculator = revenueCalculator;
			this.CalculatedValueCache = new Dictionary<CacheKey, (ZDecimal, ZDecimal)>(10);
		}

		readonly BusinessObjectFactory factory;
		readonly ZGuid viewingOrgPk;
		readonly OrgSalesRevenueCalculator revenueCalculator;
		readonly Dictionary<CacheKey, (ZDecimal, ZDecimal)> CalculatedValueCache;

		public enum PeriodRange
		{
			CurrentFinancialYearToDate,
			CurrentFinancialYear,
			NextFinancialYear,
			Trailing12Months,
			Next12Months,
			None
		}

		public enum ValueType
		{
			Committed,
			Forecast,
			Pipeline,
			Unsuccessful,
			Traded
		}

		enum TradeLaneRange
		{
			SalesProduct,
			SelectionBased
		}

		struct CacheKey
		{
			public ValueType ValueType;
			public PeriodRange PeriodRange;
			public TradeLaneRange TradeLaneType;
			public ZGuid CompanyPk;
			public ZGuid SalesProductPk;
			public ZString TradeLanePks;
		}

		public void InvalidateCache()
		{
			CalculatedValueCache.Clear();
		}

		public (ZDecimal TotalRevenue, ZDecimal TEUQuantity) GetProductValue(ValueType valueType, PeriodRange periodRange, ZGuid companyPk, OrgSalesProduct salesProduct)
		{
			return GetCachedValue(valueType, periodRange, TradeLaneRange.SalesProduct, companyPk, salesProduct, null);
		}

		public (ZDecimal TotalRevenue, ZDecimal TEUQuantity) GetTradeLaneValue(ValueType valueType, PeriodRange periodRange, ZGuid companyPk, OrgSales[] tradeLanes)
		{
			return GetCachedValue(valueType, periodRange, TradeLaneRange.SelectionBased, companyPk, null, tradeLanes);
		}

		public (ZDecimal TotalRevenue, ZDecimal TEUQuantity) GetTradeLaneValue(ValueType valueType, PeriodRange periodRange, ZGuid companyPk, OrgSales tradeLane)
		{
			return GetCachedValue(valueType, periodRange, TradeLaneRange.SelectionBased, companyPk, null, new OrgSales[] { tradeLane });
		}

		(ZDecimal TotalRevenue, ZDecimal TEUQuantity) GetCachedValue(ValueType valueType, PeriodRange periodRange, TradeLaneRange tradeLaneRange, ZGuid companyPk, OrgSalesProduct salesProduct, OrgSales[] tradeLanes)
		{
			var key = new CacheKey()
			{
				ValueType = valueType,
				PeriodRange = periodRange,
				TradeLaneType = tradeLaneRange,
				CompanyPk = companyPk,
				SalesProductPk = salesProduct?.PK ?? ZGuid.Empty,
				TradeLanePks = String.Join("|", tradeLanes?.Select(x => x.PK.ToString()).OrderBy(x => x) ?? Enumerable.Empty<string>()),
			};

			if (CalculatedValueCache.TryGetValue(key, out (ZDecimal, ZDecimal) cachedValue))
			{
				return cachedValue;
			}
			else
			{
				var calculatdValue = CalculateValue(valueType, periodRange, tradeLaneRange, companyPk, salesProduct, tradeLanes);
				CalculatedValueCache[key] = calculatdValue;
				return calculatdValue;
			}
		}

		(ZDecimal TotalRevenue, ZDecimal TEUQuantity) CalculateValue(ValueType valueType, PeriodRange periodRange, TradeLaneRange tradeLaneRange, ZGuid companyPk, OrgSalesProduct salesProduct, OrgSales[] tradeLanes)
		{
			switch (valueType)
			{
				case ValueType.Traded:
					return CalculateTradedValue(periodRange, tradeLaneRange, companyPk, salesProduct, tradeLanes);
				case ValueType.Committed:
				case ValueType.Forecast:
					return CalculateSuccessfulValue(valueType, periodRange, tradeLaneRange, companyPk, salesProduct, tradeLanes);
				case ValueType.Pipeline:
				case ValueType.Unsuccessful:
					return CalculatePipelineOrUnsuccessfulValue(valueType, tradeLaneRange, companyPk, salesProduct, tradeLanes);
				default:
					return (ZDecimal.Zero, ZDecimal.Zero);
			}
		}

		(ZDecimal TotalRevenue, ZDecimal TEUQuantity) CalculateSuccessfulValue(ValueType valueType, PeriodRange periodRange, TradeLaneRange tradeLaneRange, ZGuid companyPk, OrgSalesProduct salesProduct, OrgSales[] tradeLanes)
		{
			bool isForecast = valueType == ValueType.Forecast;
			var periodsFromToInclusive = GetPeriodFromToRange(periodRange);
			IEnumerable<OrgTradePeriod> prospectValuesToInclude = null;

			var context = new SalesLoader.LoadingContext()
			{
				OrgPk = viewingOrgPk,
				CompanyPk = companyPk,
				PeriodFrom = periodsFromToInclusive.From,
				PeriodTo = periodsFromToInclusive.To,
				IsPeriodToInclusive = true,
				IsTraded = false,
				IsForecast = isForecast,
				TradeStatus = OpportunityTradeStatus.Codes.Successful
			};

			if (tradeLaneRange == TradeLaneRange.SalesProduct)
			{
				context.SalesProduct = salesProduct;
				context.LoadByOption = SalesLoader.LoadingContext.LoadBy.Product;
				prospectValuesToInclude = SalesLoader.LoadPeriods(factory, context);
			}
			else if (tradeLaneRange == TradeLaneRange.SelectionBased)
			{
				context.SelectedSalesPks = tradeLanes?.Select(x => x.PK);
				context.LoadByOption = SalesLoader.LoadingContext.LoadBy.SelectedSales;
				prospectValuesToInclude = SalesLoader.LoadPeriods(factory, context);
			}

			if (prospectValuesToInclude != null && prospectValuesToInclude.Any())
			{
				var teuQuantity = prospectValuesToInclude.Sum(x => x.PAS_TEUQuantity);
				var totalRevenue = revenueCalculator.GetTotalInEntityCurrency(prospectValuesToInclude, x => x.PAS_RX_NKCurrency, x => x.PAS_EstimatedProfit);

				return (totalRevenue, teuQuantity);
			}
			else
			{
				return (ZDecimal.Zero, ZDecimal.Zero);
			}
		}

		(ZDecimal TotalRevenue, ZDecimal TEUQuantity) CalculatePipelineOrUnsuccessfulValue(ValueType valueType, TradeLaneRange tradeLaneRange, ZGuid companyPk, OrgSalesProduct salesProduct, OrgSales[] tradeLanes)
		{
			var context = new SalesLoader.LoadingContext()
			{
				OrgPk = viewingOrgPk,
				CompanyPk = companyPk,
				IsTraded = false,
				IsForecast = false
			};

			if (valueType == ValueType.Pipeline)
			{
				context.TradeStatus = OpportunityTradeStatus.Codes.Active;
			}
			else if (valueType == ValueType.Unsuccessful)
			{
				context.TradeStatus = OpportunityTradeStatus.Codes.Unsuccessful;
			}

			IEnumerable<OrgTradeDetail> detailsToInclude = null;

			if (tradeLaneRange == TradeLaneRange.SalesProduct)
			{
				context.LoadByOption = SalesLoader.LoadingContext.LoadBy.Product;
				context.SalesProduct = salesProduct;
				detailsToInclude = SalesLoader.LoadTradeDetails(factory, context);
			}
			else if (tradeLaneRange == TradeLaneRange.SelectionBased)
			{
				context.LoadByOption = SalesLoader.LoadingContext.LoadBy.SelectedSales;
				context.SelectedSalesPks = tradeLanes?.Select(x => x.PK);
				detailsToInclude = SalesLoader.LoadTradeDetails(factory, context);
			}

			if (detailsToInclude.Any())
			{
				foreach (var detail in detailsToInclude)
				{
					factory.AddFetchHint(OrgTradePeriodSchema.PAS_PA, detail.PK);
					factory.AddFetchHint(OrgTradeProspectSchema.PAP_PA, detail.PK);
				}

				var teuQuantity = detailsToInclude.Sum(x => x.PA_Calc_EstimatedPipelineAnnualTEUQuantity);
				var totalRevenue = revenueCalculator.GetTotalInEntityCurrency(detailsToInclude, x => x.CurrentProspectPeriod.PAS_RX_NKCurrency, x => x.PA_Calc_EstimatedPipelineAnnualValue);

				return (totalRevenue, teuQuantity);
			}
			else
			{
				return (ZDecimal.Zero, ZDecimal.Zero);
			}
		}

		(ZDecimal TotalRevenue, ZDecimal TEUQuantity) CalculateTradedValue(PeriodRange periodRange, TradeLaneRange tradeLaneRange, ZGuid companyPk, OrgSalesProduct salesProduct, OrgSales[] tradeLanes)
		{
			IEnumerable<OrgTradeValue> tradedValuesToInclude = null;
			var periodsFromToInclusive = GetPeriodFromToRange(periodRange);

			var context = new SalesLoader.LoadingContext()
			{
				OrgPk = viewingOrgPk,
				PeriodFrom = periodsFromToInclusive.From,
				PeriodTo = periodsFromToInclusive.To,
				IsPeriodToInclusive = true,
				IsTraded = true,
				IsJobValue = false,
				CompanyPk = companyPk
			};

			if (tradeLaneRange == TradeLaneRange.SalesProduct)
			{
				context.SalesProduct = salesProduct;
				context.LoadByOption = SalesLoader.LoadingContext.LoadBy.Product;
				tradedValuesToInclude = SalesLoader.LoadTradedValues(factory, context);
			}
			else if (tradeLaneRange == TradeLaneRange.SelectionBased)
			{
				context.SelectedSalesPks = tradeLanes?.Select(x => x.PK);
				context.LoadByOption = SalesLoader.LoadingContext.LoadBy.SelectedSales;
				tradedValuesToInclude = SalesLoader.LoadTradedValues(factory, context);
			}

			if (tradedValuesToInclude != null && tradedValuesToInclude.Any())
			{
				var teuQuantity = tradedValuesToInclude.Sum(x => x.TradePeriod.PAS_TEUQuantity);
				var totalRevenue = revenueCalculator.GetTotalInEntityCurrency(tradedValuesToInclude, x => x.PAV_RX_NKCurrency, x => x.PAV_Revenue);

				return (totalRevenue, teuQuantity);
			}
			else
			{
				return (ZDecimal.Zero, ZDecimal.Zero);
			}
		}

		(ZDate From, ZDate To) GetPeriodFromToRange(PeriodRange periodRange)
		{
			switch (periodRange)
			{
				case PeriodRange.CurrentFinancialYearToDate:
					return GetFinancialYearPeriodRange(isYTD: true, isNextFinancialYear: false);
				case PeriodRange.CurrentFinancialYear:
					return GetFinancialYearPeriodRange(isYTD: false, isNextFinancialYear: false);
				case PeriodRange.NextFinancialYear:
					return GetFinancialYearPeriodRange(isYTD: false, isNextFinancialYear: true);
				case PeriodRange.Trailing12Months:
					return Get12MonthsPeriodRange(isNextYear: false);
				case PeriodRange.Next12Months:
					return Get12MonthsPeriodRange(isNextYear: true);
				default:
					return (ZDate.Empty, ZDate.Empty);
			}
		}

		(ZDate From, ZDate To) Get12MonthsPeriodRange(bool isNextYear)
		{
			var today = ZDate.Today;
			var currentPeriod = new ZDate(today.Year, today.Month, 1);

			ZDate periodFrom = currentPeriod.AddMonths(-12);
			ZDate periodToInclusive = currentPeriod.AddMonths(-1);

			if (isNextYear)
			{
				periodFrom = periodFrom.AddYears(1);
				periodToInclusive = periodToInclusive.AddYears(1);
			}

			return (periodFrom, periodToInclusive);
		}

		(ZDate From, ZDate To) GetFinancialYearPeriodRange(bool isYTD, bool isNextFinancialYear)
		{
			var today = ZDate.Today;
			var currentPeriod = new ZDate(today.Year, today.Month, 1);

			ZDate periodToInclusive = ZDate.Empty;
			ZDate periodFrom = ZDate.Empty;
			var periodCalculator = new AccountingPeriodCalculator(factory);
			var currentAccountingPeriod = periodCalculator.GetPeriodFromDate(today);
			if (currentAccountingPeriod > 0)
			{
				periodFrom = currentPeriod.AddMonths(1 - currentAccountingPeriod % 100);
				periodToInclusive = isYTD ? currentPeriod : periodFrom.AddMonths(11);

				if (isNextFinancialYear)
				{
					periodFrom = periodFrom.AddYears(1);
					periodToInclusive = periodToInclusive.AddYears(1);
				}
			}

			return (periodFrom, periodToInclusive);
		}
	}
}
