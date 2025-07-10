using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public class TradedSalesTreeModelView : ZTreeModelView<TradePeriodGrouping>
	{
		public TradedSalesTreeModelView(TradedSalesTreeModel inner)
			: base(inner)
		{
		}
	}
}
