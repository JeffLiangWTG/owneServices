using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class MultipleTradeDetailSelectionControl : ZUserControl
	{
		public MultipleTradeDetailSelectionControl()
		{
			InitializeComponent();
		}

		public TradeDetailSelectionItemCollection TradeDetailSelectionItemCollection
		{
			get { return (TradeDetailSelectionItemCollection)BindingSource.Current; }
		}

		#region Select All

		void SelectAllButton_Click(object sender, EventArgs e)
		{
			TradeDetailSelectionItemCollection.SelectAll();
		}

		#endregion

		#region Deselect All

		void DeselectAllButton_Click(object sender, EventArgs e)
		{
			TradeDetailSelectionItemCollection.DeselectAll();
		}

		#endregion
	}
}
