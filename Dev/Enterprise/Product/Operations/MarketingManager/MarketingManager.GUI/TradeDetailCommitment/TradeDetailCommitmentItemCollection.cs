using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class TradeDetailCommitmentItemCollection : NonPersistentBusinessObjectCollection<TradeDetailCommitmentItem>
	{
		public TradeDetailCommitmentItem AddNew(OrgTradeDetail tradeDetail, ZString opportunityStatus)
		{
			var result = new TradeDetailCommitmentItem(tradeDetail, opportunityStatus);
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
