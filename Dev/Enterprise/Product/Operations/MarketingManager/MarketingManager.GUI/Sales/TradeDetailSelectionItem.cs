using CargoWise.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class TradeDetailSelectionItem : AutoTradeDetailSelectionItem
	{
		public TradeDetailSelectionItem(OrgTradeDetail tradeDetail)
			: base(tradeDetail.Factory)
		{
			Argument.NotNull(tradeDetail, nameof(tradeDetail));
			this.tradeDetail = tradeDetail;
		}

		#region TradeDetail

		public OrgTradeDetail TradeDetail
		{
			get { return tradeDetail; }
		}
		protected readonly OrgTradeDetail tradeDetail;

		#endregion
	}
}
