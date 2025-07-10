using System.Linq;
using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TransportTradeLanesControl : JobCommonTradeLanesControl
	{
		public TransportTradeLanesControl(OrgSalesProduct salesProduct)
			: base(salesProduct)
		{
			InitializeComponent();

			TradeLanesGrid.SetAvailability(true, LocationTypeColumnsInTradeLanesGrid.ToArray());
		}
	}
}
