using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.GUI
{
	public partial class ForwardingShipmentTradeLanesControl : JobCommonTradeLanesControl
	{
		public ForwardingShipmentTradeLanesControl(OrgSalesProduct salesProduct)
			: base(salesProduct)
		{
			InitializeComponent();
		}
	}
}
