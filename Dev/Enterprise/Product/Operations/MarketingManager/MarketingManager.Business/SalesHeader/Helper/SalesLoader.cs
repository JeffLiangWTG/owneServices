using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public static class SalesLoader
	{
		public static IEnumerable<OrgTradePeriod> LoadPeriods(BusinessObjectFactory factory, LoadingContext context)
		{
			var orgPeriodQuery = new ZDBOnlyQuery(typeof(OrgTradePeriod));

			if (context.LoadByOption == LoadingContext.LoadBy.SelectedSales && context.SelectedSalesPks != null && !context.SelectedSalesPks.Any())
			{
				orgPeriodQuery.IsNoResultQuery = true;
			}
			else
			{
				PopulatePeriodQuery(orgPeriodQuery, context);
			}

			return factory.Load<OrgTradePeriod>(orgPeriodQuery);
		}

		public static IEnumerable<OrgPeriodTradedValue> LoadTradedValuesByProduct(BusinessObjectFactory factory, LoadingContext context)
		{
			var valueQuery = new ZDBOnlyQuery(typeof(OrgPeriodTradedValue));
			valueQuery.AddToFilter(ViewOrgPeriodTradedValueSchema.VPT_OH_Client, context.OrgPk);

			if (!context.PeriodFrom.IsEmpty)
			{
				valueQuery.AddToFilter(ViewOrgPeriodTradedValueSchema.VPT_Period, SQLComparisonOperator.GreaterThanOrEqualTo, context.PeriodFrom);
			}

			if (!context.PeriodTo.IsEmpty)
			{
				valueQuery.AddToFilter(ViewOrgPeriodTradedValueSchema.VPT_Period, SQLComparisonOperator.LessThanOrEqualTo, context.PeriodTo);
			}

			if (context.SalesProduct != null)
			{
				valueQuery.AddToFilter(ViewOrgPeriodTradedValueSchema.VPT_ProductCode, context.SalesProduct.MP_Code);
			}

			if (!context.CompanyPk.IsEmpty)
			{
				valueQuery.AddToFilter(ViewOrgPeriodTradedValueSchema.VPT_GC, context.CompanyPk);
			}

			var tradedValues = factory.Load<OrgPeriodTradedValue>(valueQuery);

			return tradedValues;
		}

		public static IEnumerable<OrgTradeValue> LoadTradedValues(BusinessObjectFactory factory, LoadingContext context)
		{
			var valueQuery = new ZDBOnlyQuery(typeof(OrgTradeValue));

			if (context.SelectedSalesPks != null && !context.SelectedSalesPks.Any())
			{
				valueQuery.IsNoResultQuery = true;
			}
			else
			{
				context.IsTraded = true;
				context.IsForecast = false;
				context.TradeStatus = ZString.Empty;

				var periodSubQuery = new ZDBOnlySubQuery(typeof(OrgTradePeriod), OrgTradeValueSchema.PAV_PAS);
				PopulatePeriodQuery(periodSubQuery, context);

				if (!context.CompanyPk.IsEmpty)
				{
					valueQuery.AddToFilter(OrgTradeValueSchema.PAV_GC, context.CompanyPk);
				}

				valueQuery.AddSubQuery(periodSubQuery, JoinCondition.And);
			}

			var tradedValues = factory.Load<OrgTradeValue>(valueQuery);

			return tradedValues;
		}

		public static IEnumerable<OrgTradeDetail> LoadTradeDetails(BusinessObjectFactory factory, LoadingContext context)
		{
			var detailQuery = new ZDBOnlyQuery(typeof(OrgTradeDetail));

			if (context.LoadByOption == LoadingContext.LoadBy.SelectedSales && context.SelectedSalesPks != null && !context.SelectedSalesPks.Any())
			{
				detailQuery.IsNoResultQuery = true;
			}
			else
			{
				PopulateDetailQuery(detailQuery, context);
			}

			return factory.Load<OrgTradeDetail>(detailQuery);
		}

		static void PopulatePeriodQuery(ZDBOnlyQuery periodQuery, LoadingContext context)
		{
			periodQuery.AddToFilter(OrgTradePeriodSchema.PAS_OH_Client, context.OrgPk);
			periodQuery.AddToFilter(OrgTradePeriodSchema.PAS_IsTraded, context.IsTraded);
			periodQuery.AddToFilter(OrgTradePeriodSchema.PAS_IsForecast, context.IsForecast);
			periodQuery.AddToFilter(OrgTradePeriodSchema.PAS_IsSuperseded, ZBool.False);
			periodQuery.AddToFilter(OrgTradePeriodSchema.PAS_IsExpired, ZBool.False);

			if (context.IsJobValue.HasValue)
			{
				periodQuery.AddToFilter(OrgTradePeriodSchema.PAS_IsJobValue, context.IsJobValue.Value);
			}

			if (!context.PeriodFrom.IsEmpty)
			{
				periodQuery.AddToFilter(OrgTradePeriodSchema.PAS_Period, SQLComparisonOperator.GreaterThanOrEqualTo, context.PeriodFrom);
			}

			if (!context.PeriodTo.IsEmpty)
			{
				var periodToComparisonOperator = context.IsPeriodToInclusive ? SQLComparisonOperator.LessThanOrEqualTo : SQLComparisonOperator.LessThan;
				periodQuery.AddToFilter(OrgTradePeriodSchema.PAS_Period, periodToComparisonOperator, context.PeriodTo);
			}

			var detailSubQuery = new ZDBOnlySubQuery(typeof(OrgTradeDetail), OrgTradePeriodSchema.PAS_PA);
			PopulateDetailQuery(detailSubQuery, context);

			periodQuery.AddSubQuery(detailSubQuery, JoinCondition.And);
		}

		static void PopulateDetailQuery(ZDBOnlyQuery detailQuery, LoadingContext context)
		{
			if (!context.TradeStatus.IsEmpty)
			{
				detailQuery.AddToFilter(OrgTradeDetailSchema.PA_Status, context.TradeStatus);
			}

			if (!context.IsTraded && !context.CompanyPk.IsEmpty)
			{
				var pivotSubQuery = new ZDBOnlySubQuery(typeof(OrgSalesValueAssociationPivot), OrgSalesValueAssociationPivotSchema.SVP_TradeId);

				var opportunitySubQuery = new ZDBOnlySubQuery(typeof(OrgOpportunity), OrgOpportunitySchema.PK);
				opportunitySubQuery.AddToFilter(OrgOpportunitySchema.P8_GC, context.CompanyPk);

				var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);

				var rateEntrySubQuery = new ZDBOnlySubQuery(typeof(RateEntry), RateEntrySchema.PK);
				var ratingHeaderSubQuery = new ZDBOnlySubQuery(typeof(RatingHeader), RateEntrySchema.TI_TH);
				ratingHeaderSubQuery.AddToFilter(RatingHeaderSchema.TH_GC, context.CompanyPk);
				rateEntrySubQuery.AddSubQuery(ratingHeaderSubQuery, JoinCondition.And);

				pivotSubQuery.AddSubQuery(OrgSalesValueAssociationPivotSchema.SVP_ActivityId, OrgOpportunitySchema.PK, opportunitySubQuery, JoinCondition.Or);
				pivotSubQuery.AddSubQuery(OrgSalesValueAssociationPivotSchema.SVP_ActivityId, OrgHeaderSchema.PK, orgSubQuery, JoinCondition.Or);
				pivotSubQuery.AddSubQuery(OrgSalesValueAssociationPivotSchema.SVP_ActivityId, RateEntrySchema.PK, rateEntrySubQuery, JoinCondition.Or);

				detailQuery.AddSubQuery(OrgTradeDetailSchema.PK, OrgSalesValueAssociationPivotSchema.SVP_TradeId, pivotSubQuery, JoinCondition.And);
			}

			var salesSubQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgTradeDetailSchema.PA_OW);
			salesSubQuery.AddToFilter(OrgSalesSchema.OW_IsTraded, context.IsTraded);

			if (!context.IsTraded && !context.OrgPk.IsEmpty)
			{
				salesSubQuery.AddToFilter(OrgSalesSchema.OW_OH_Primary, context.OrgPk);
			}

			if (context.SelectedSalesPks != null && context.SelectedSalesPks.Any())
			{
				salesSubQuery.AddToFilter(OrgSalesSchema.PK, context.SelectedSalesPks);
			}

			if (context.SalesProduct != null)
			{
				salesSubQuery.AddToFilter(OrgSalesSchema.OW_MP_Product, context.SalesProduct.PK);
			}

			detailQuery.AddSubQuery(salesSubQuery, JoinCondition.And);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815: Override equals and operator equals on value types", Justification = "It is not used for comparison.")]
		public struct LoadingContext
		{
			public ZGuid OrgPk;
			public ZGuid CompanyPk;
			public ZDateTime PeriodFrom;
			public ZDateTime PeriodTo;
			public bool IsPeriodToInclusive;
			public bool IsTraded;
			public bool IsForecast;
			public bool? IsJobValue;
			public ZString TradeStatus;
			public OrgSalesProduct SalesProduct;
			public IEnumerable<ZGuid> SelectedSalesPks;
			public LoadBy LoadByOption;

			public enum LoadBy
			{
				Product,
				SelectedSales
			}
		}
	}
}
