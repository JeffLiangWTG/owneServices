using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class TradeDetailTimelineCollection : NonPersistentBusinessObjectCollection<TradeDetailTimeline>
	{
		public TradeDetailTimelineCollection(OrgTradeDetail tradeDetail)
			: base(tradeDetail.Factory)
		{
			this.tradeDetail = tradeDetail;
		}
		readonly OrgTradeDetail tradeDetail;

		public override void Load()
		{
			if (tradeDetail.Parent == null)
			{
				return;
			}

			var periodQuery = new ZDBOnlyQuery(typeof(OrgTradePeriod));
			periodQuery.AddToFilter(OrgTradePeriodSchema.PAS_IsTraded, ZBool.False);
			periodQuery.AddToFilter(OrgTradePeriodSchema.PAS_OH_Client, tradeDetail.Parent.OW_OH_Primary);
			periodQuery.AddToFilter(OrgTradePeriodSchema.PAS_Period, SQLComparisonOperator.NotEqual, null);

			var detailSubQuery = new ZDBOnlySubQuery(typeof(OrgTradeDetail), OrgTradePeriodSchema.PAS_PA);
			detailSubQuery.AddToFilter(OrgTradeDetailSchema.PA_Status, OpportunityTradeStatus.Codes.Successful);
			detailSubQuery.AddToFilter(OrgTradeDetailSchema.PA_TradeMode, tradeDetail.PA_TradeMode);
			detailSubQuery.AddToFilter(OrgTradeDetailSchema.PA_TradeType, tradeDetail.PA_TradeType);
			if (!tradeDetail.PA_OP.IsEmpty)
			{
				detailSubQuery.AddToFilter(OrgTradeDetailSchema.PA_OP, tradeDetail.PA_OP);
			}
			else
			{
				detailSubQuery.AddToFilter(OrgTradeDetailSchema.PA_OP, null);
			}

			var salesSubQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgTradeDetailSchema.PA_OW);
			salesSubQuery.AddToFilter(OrgSalesSchema.OW_IsTraded, ZBool.False);
			salesSubQuery.AddToFilter(OrgSalesSchema.OW_OH_Primary, tradeDetail.Parent.OW_OH_Primary);
			salesSubQuery.AddToFilter(OrgSalesSchema.OW_MP_Product, tradeDetail.Parent.OW_MP_Product);
			salesSubQuery.AddToFilter(OrgSalesSchema.OW_Service, tradeDetail.Parent.OW_Service);

			if (!tradeDetail.Parent.OW_OriginID.IsEmpty)
			{
				salesSubQuery.AddToFilter(OrgSalesSchema.OW_OriginID, tradeDetail.Parent.OW_OriginID);
			}
			else
			{
				salesSubQuery.AddToFilter(OrgSalesSchema.OW_OriginID, null);
			}

			if (!tradeDetail.Parent.OW_DestinationID.IsEmpty)
			{
				salesSubQuery.AddToFilter(OrgSalesSchema.OW_DestinationID, tradeDetail.Parent.OW_DestinationID);
			}
			else
			{
				salesSubQuery.AddToFilter(OrgSalesSchema.OW_DestinationID, null);
			}

			if (!tradeDetail.Parent.OW_WW.IsEmpty)
			{
				salesSubQuery.AddToFilter(OrgSalesSchema.OW_WW, tradeDetail.Parent.OW_DestinationID);
			}
			else
			{
				salesSubQuery.AddToFilter(OrgSalesSchema.OW_WW, null);
			}

			detailSubQuery.AddSubQuery(salesSubQuery, JoinCondition.And);
			periodQuery.AddSubQuery(detailSubQuery, JoinCondition.And);

			var periods = Factory.Load<OrgTradePeriod>(periodQuery);

			foreach (var period in periods)
			{
				Add(new TradeDetailTimeline(Factory, period));
			}
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
	}
}
