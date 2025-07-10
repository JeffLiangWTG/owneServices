using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class TradeDetailsControlFormForTest : ZForm
	{
		public TradeDetailsControlFormForTest(EntitySalesWrapper sales)
			: base(sales)
		{
		}
	}
}
