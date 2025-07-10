using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class OrgTradeDetailJobCommonGroupingElements : BusinessObjectCollection<EntityTradeDetailWrapper>
	{
		public OrgTradeDetailJobCommonGroupingElements(OrgTradeDetailJobCommonGrouping grouping)
			: base(grouping.Factory)
		{
			this.tradeDetailJobCommonGrouping = grouping;
		}

		readonly OrgTradeDetailJobCommonGrouping tradeDetailJobCommonGrouping;

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			ILegacyBusinessObjectCollectionInternals tradeDetailCollectionInternals = tradeDetailJobCommonGrouping.EntityTradeDetails;
			tradeDetailCollectionInternals.SetCollectionRelationships(child);
		}

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var entityTradeDetail = (EntityTradeDetailWrapper)child;
			using (entityTradeDetail.GetValidationSuspender())
			{
				entityTradeDetail.PA_TradeMode = tradeDetailJobCommonGrouping.TradeMode;
				entityTradeDetail.PA_TradeType = tradeDetailJobCommonGrouping.TradeType;
			}

			if (!IsLoading)
			{
				var entity = tradeDetailJobCommonGrouping.EntityTradeDetails.EntitySales.Entity;
				EntityTradeDetailWrapperCollection.SetOpportunityTradeDetailStatus(entity, entityTradeDetail);
			}
		}

		#endregion

		#region Add / Remove

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			tradeDetailJobCommonGrouping.EntityTradeDetails.Add((EntityTradeDetailWrapper)bizOAdded);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			tradeDetailJobCommonGrouping.EntityTradeDetails.Remove((EntityTradeDetailWrapper)bizO);
			base.OnRemoved(bizO);
		}

		#endregion
	}
}
