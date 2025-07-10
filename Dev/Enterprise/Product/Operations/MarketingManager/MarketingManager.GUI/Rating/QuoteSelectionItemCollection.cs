using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class QuoteSelectionItemCollection : NonPersistentBusinessObjectCollection<QuoteSelectionItem>
	{
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

		#region New

		public QuoteSelectionItem AddNew(IRelatableActivity quote)
		{
			var result = new QuoteSelectionItem(quote);
			Add(result);
			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			// Should not be called
			throw new NotImplementedException();
		}

		#endregion

		#region SelectedItem

		public QuoteSelectionItem SelectedItem
		{
			get
			{
				foreach (QuoteSelectionItem item in this)
				{
					if (item.Selected)
					{
						return item;
					}
				}

				return null;
			}
		}

		public event EventHandler SelectedItemChanged;

		#endregion

		#region Relationship

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			var item = (QuoteSelectionItem)bizOAdded;
			item.SelectedInfo.ValueChanged += SelectedInfo_ValueChanged;
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);

			var item = (QuoteSelectionItem)bizO;
			item.SelectedInfo.ValueChanged -= SelectedInfo_ValueChanged;
		}

		void SelectedInfo_ValueChanged(object sender, EventArgs e)
		{
			var changedItem = (QuoteSelectionItem)sender;
			if (changedItem.Selected)
			{
				foreach (QuoteSelectionItem item in this)
				{
					if (item != changedItem)
					{
						item.Selected = false;
					}
				}
			}

			if (SelectedItemChanged != null)
			{
				SelectedItemChanged(this, EventArgs.Empty);
			}
		}

		#endregion
	}
}
