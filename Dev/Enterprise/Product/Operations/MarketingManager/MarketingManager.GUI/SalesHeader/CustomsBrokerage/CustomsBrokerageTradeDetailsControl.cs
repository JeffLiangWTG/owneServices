using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.GUI
{
	public partial class CustomsBrokerageTradeDetailsControl : JobCommonTradeDetailsControl
	{
		public CustomsBrokerageTradeDetailsControl(OrgSalesProduct salesProduct)
			: base(salesProduct)
		{
			InitializeComponent();
		}

		protected override IJobCommonTradeDetailsInnerControl GetNewInnerControl()
		{
			var innerControl = base.GetNewInnerControl();
			innerControl.GroupingGrid.ReOrderColumns([OrgTradeDetailJobCommonGrouping.Schema.TradeType, OrgTradeDetailJobCommonGrouping.Schema.TradeMode]);
			return innerControl;
		}
	}
}
