using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	class TradeDetailAssociationPivotCollection : SalesValueAssociationPivotCollection
	{
		public TradeDetailAssociationPivotCollection(OrgTradeDetail detail, bool onlyIncludeForCurrentCompany, string salesProductCode = null) : base(detail, detail.Factory, GetRelationship(detail, onlyIncludeForCurrentCompany, salesProductCode))
		{
		}

		static ICollectionRelationship GetRelationship(OrgTradeDetail detail, bool onlyIncludeForCurrentCompany, string salesProductCode)
		{
			if (onlyIncludeForCurrentCompany)
			{
				return new CompanyTradeDetailAssociationPivotCollectionRelationship(detail, salesProductCode);
			}
			else
			{
				return new GlobalTradeDetailAssociationPivotCollectionRelationship(detail, salesProductCode);
			}
		}
	}

	class GlobalTradeDetailAssociationPivotCollectionRelationship : GlobalSalesValueAssociationPivotCollectionRelationship
	{
		public GlobalTradeDetailAssociationPivotCollectionRelationship(OrgTradeDetail detail, string salesProductCode)
			: base(GetFilter(detail, salesProductCode))
		{
		}

		static ZQuery GetFilter(OrgTradeDetail detail, string salesProductCode)
		{
			return BaseFilter(detail, salesProductCode);
		}

		public static ZDBOnlyQuery BaseFilter(OrgTradeDetail detail, string salesProductCode)
		{
			var query = new ZDBOnlyQuery(typeof(OrgSalesValueAssociationPivot));

			var tradeDetailSubQuery = new ZDBOnlySubQuery(typeof(OrgTradeDetail), OrgTradeDetailSchema.PK);
			tradeDetailSubQuery.AddToFilter(OrgTradeDetailSchema.PA_OW, detail.PA_OW);

			if (salesProductCode == SystemDefinedSalesProductList.Codes.Warehouse)
			{
				if (detail.PA_OP != ZGuid.Empty)
				{
					tradeDetailSubQuery.AddToFilter(OrgTradeDetailSchema.PA_OP, detail.PA_OP);
				}
				else
				{
					tradeDetailSubQuery.AddToFilter(OrgTradeDetailSchema.PA_OP, DBNull.Value);
				}
			}

			if (salesProductCode == SystemDefinedSalesProductList.Codes.CustomsBrokerage ||
				salesProductCode == SystemDefinedSalesProductList.Codes.LinerAgency ||
				salesProductCode == SystemDefinedSalesProductList.Codes.ForwardingShipment
				)
			{
				tradeDetailSubQuery.AddToFilter(OrgTradeDetailSchema.PA_TradeMode, detail.PA_TradeMode);
				tradeDetailSubQuery.AddToFilter(OrgTradeDetailSchema.PA_TradeType, detail.PA_TradeType);
			}

			query.AddSubQuery(OrgSalesValueAssociationPivotSchema.SVP_TradeId, OrgTradeDetailSchema.PK, tradeDetailSubQuery, JoinCondition.And);
			query.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_TradeTableCode, detail.TablePrefix);
			return query;
		}
	}

	class CompanyTradeDetailAssociationPivotCollectionRelationship : CompanySalesValueAssociationPivotCollectionRelationship
	{
		public CompanyTradeDetailAssociationPivotCollectionRelationship(OrgTradeDetail detail, string salesProductCode)
			: base(detail, GetFilter(detail, salesProductCode))
		{
		}

		static ZQuery GetFilter(OrgTradeDetail detail, string salesProductCode)
		{
			var query = GlobalTradeDetailAssociationPivotCollectionRelationship.BaseFilter(detail, salesProductCode);

			var rateEntryType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(RateEntrySchema.Constants.Prefix);
			var rateEntryCurrentCompanySubquery = new ZDBOnlySubQuery(rateEntryType, OrgSalesValueAssociationPivotSchema.SVP_ActivityId);
			var rateHeaderCurrentCompanySubquery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Rating.IRatingHeader), RateEntrySchema.TI_TH);
			rateHeaderCurrentCompanySubquery.AddToFilter(RatingHeaderSchema.TH_GC, GlbCompany.CurrentCompany.PK);
			rateEntryCurrentCompanySubquery.AddSubQuery(rateHeaderCurrentCompanySubquery, JoinCondition.And);

			var rateCurrentCompanyFilter = new ZDBOnlyQuery(typeof(OrgSalesValueAssociationPivot));
			rateCurrentCompanyFilter.AddToFilter(new ZQuery(OrgSalesValueAssociationPivotSchema.SVP_ActivityTableCode, SQLComparisonOperator.NotEqual, RateEntrySchema.Constants.Prefix));
			rateCurrentCompanyFilter.AddSubQuery(rateEntryCurrentCompanySubquery, JoinCondition.Or);
			query.AddToFilter(rateCurrentCompanyFilter);

			return query;
		}
	}
}
