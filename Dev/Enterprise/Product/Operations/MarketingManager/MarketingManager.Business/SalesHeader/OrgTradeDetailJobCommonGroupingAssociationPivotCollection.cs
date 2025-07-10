using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class OrgTradeDetailJobCommonGroupingAssociationPivotCollection : SalesValueAssociationPivotCollection
	{
		public OrgTradeDetailJobCommonGroupingAssociationPivotCollection(OrgTradeDetailJobCommonGrouping tradeDetailGrouping, bool applyCompanyFilter)
			: base(tradeDetailGrouping, tradeDetailGrouping.Product.Factory, new TradeDetailGroupingAssociationPivotCollectionRelationship(tradeDetailGrouping))
		{
			this.tradeDetailGrouping = tradeDetailGrouping;
			this.applyCompanyFilter = applyCompanyFilter;
		}
		readonly OrgTradeDetailJobCommonGrouping tradeDetailGrouping;
		readonly bool applyCompanyFilter;

		protected override bool MatchesFilterCore(OrgSalesValueAssociationPivot element, bool fetchOnlyFromLocalCache)
		{
			bool result;
			if (element.IsInDatabase)
			{
				result = base.MatchesFilterCore(element, fetchOnlyFromLocalCache);
			}
			else
			{
				result = element.SVP_TradeTableCode == OrgTradeDetailSchema.Constants.Prefix
					&& tradeDetailGrouping.Elements.Any(x => x.PK == element.SVP_TradeId);
			}

			if (result && applyCompanyFilter)
			{
				var companyPk = tradeDetailGrouping.EntityTradeDetails.EntitySales.CompanyFilter;
				var entity = element.AssociatedEntity;
				if (companyPk.IsEmpty || entity == null)
				{
					return true;
				}
				else
				{
					return entity.CompanyPk == null || entity.CompanyPk == companyPk;
				}
			}

			return result;
		}

		public void SetAdditionalFilter(OrgTradeDetailJobCommonGrouping grouping)
		{
			var query = new ZDBOnlyQuery(typeof(OrgSalesValueAssociationPivot));
			var tradeDetailSubQuery = new ZDBOnlySubQuery(typeof(OrgTradeDetail), OrgTradeDetailSchema.PK);
			tradeDetailSubQuery.AddToFilter(OrgTradeDetailSchema.PA_OW, grouping.Elements.Select(x => x.PA_OW).ToArray());
			tradeDetailSubQuery.AddToFilter(OrgTradeDetailSchema.PA_TradeMode, grouping.TradeMode);
			tradeDetailSubQuery.AddToFilter(OrgTradeDetailSchema.PA_TradeType, grouping.TradeType);

			query.AddSubQuery(OrgSalesValueAssociationPivotSchema.SVP_TradeId, OrgTradeDetailSchema.PK, tradeDetailSubQuery, JoinCondition.And);
			AdditionalFilter = query;
		}
	}

	class TradeDetailGroupingAssociationPivotCollectionRelationship : CollectionRelationship
	{
		public TradeDetailGroupingAssociationPivotCollectionRelationship(OrgTradeDetailJobCommonGrouping grouping)
			: base(typeof(OrgSalesValueAssociationPivot), GetFilter(grouping))
		{
			this.grouping = grouping;
		}
		readonly OrgTradeDetailJobCommonGrouping grouping;

		static ZQuery GetFilter(OrgTradeDetailJobCommonGrouping grouping)
		{
			var tradeDetailSubQuery = new ZDBOnlySubQuery(typeof(OrgTradeDetail), OrgTradeDetailSchema.PK);
			tradeDetailSubQuery.AddToFilter(OrgTradeDetailSchema.PA_OW, grouping.Elements.Select(x => x.PA_OW).ToArray());

			var query = new ZDBOnlyQuery(typeof(OrgSalesValueAssociationPivot));

			query.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_TradeTableCode, OrgTradeDetailSchema.Constants.Prefix);
			query.AddSubQuery(OrgSalesValueAssociationPivotSchema.SVP_TradeId, OrgTradeDetailSchema.PK, tradeDetailSubQuery, JoinCondition.And);

			return query;
		}

		protected override bool MatchesRelationshipFilterCore(BusinessObject businessObject, bool ignoreActiveFilter, bool fetchOnlyFromLocalCache)
		{
			if (businessObject.IsInDatabase)
			{
				return base.MatchesRelationshipFilterCore(businessObject, ignoreActiveFilter, fetchOnlyFromLocalCache);
			}
			else
			{
				var pivot = (OrgSalesValueAssociationPivot)businessObject;
				return pivot.SVP_TradeTableCode == OrgTradeDetailSchema.Constants.Prefix
					&& grouping.Elements.Any(x => x.PK == pivot.SVP_TradeId);
			}
		}
	}
}
