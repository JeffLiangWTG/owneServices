using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.GUI
{
	public partial class ForwardingShipmentTradeDetailsControl : JobCommonTradeDetailsControl
	{
		public ForwardingShipmentTradeDetailsControl(OrgSalesProduct salesProduct)
			: base(salesProduct)
		{
			InitializeComponent();
		}

		protected override IJobCommonTradeDetailsInnerControl GetNewInnerControl()
		{
			return new ForwardingShipmentTradeDetailsInnerControl();
		}
	}
}
