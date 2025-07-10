using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class TradeDetailSelectionItemCollection : NonPersistentBusinessObjectCollection<TradeDetailSelectionItem>
	{
		public TradeDetailSelectionItem AddNew(OrgTradeDetail tradeDetail)
		{
			var result = new TradeDetailSelectionItem(tradeDetail);
			Add(result);
			return result;
		}

		public void SelectAll()
		{
			foreach (TradeDetailSelectionItem item in this)
			{
				item.Selected = true;
			}
		}

		public void DeselectAll()
		{
			foreach (TradeDetailSelectionItem item in this)
			{
				item.Selected = false;
			}
		}

		#region Allowed Actions

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			// Should not be called
			throw new NotImplementedException();
		}

		#endregion
	}
}
