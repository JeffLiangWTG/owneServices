using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class TradeDetailCloneItemCollection : NonPersistentBusinessObjectCollection<TradeDetailCloneItem>
	{
		public TradeDetailCloneItem AddNew(OrgTradeDetail tradeDetail)
		{
			var result = new TradeDetailCloneItem(tradeDetail);
			Add(result);
			return result;
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			// Should not be called
			throw new NotImplementedException();
		}
	}
}
