using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class JobCommonTradeDetailFieldsControl : ZUserControl
	{
		public JobCommonTradeDetailFieldsControl()
		{
			InitializeComponent();
		}

		void ViewProspectPeriodsButton_Click(object sender, System.EventArgs e)
		{
			var detail = this.CurrentDataItem as OrgTradeDetail;
			if (detail != null)
			{
				var timelineCollection = new TradeDetailTimelineCollection(detail);
				timelineCollection.Load();
				timelineCollection.Sort("Period");
				ZFormModaliser.ShowDialogAndDispose(new TradeDetailTimelineForm(timelineCollection));
			}
		}
	}
}
