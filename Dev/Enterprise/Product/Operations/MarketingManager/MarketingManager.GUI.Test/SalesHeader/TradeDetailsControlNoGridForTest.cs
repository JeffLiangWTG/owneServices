using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class TradeDetailsControlNoGridForTest : TradeDetailsControl
	{
		public TradeDetailsControlNoGridForTest(OrgSalesProduct salesProduct)
	: base(salesProduct)
		{
			Controls.Add(TradeDetailsGrid);
		}

		public override ZGrid TradeDetailsGrid
		{
			get { return null; }
		}
	}
}
