using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class TradeLaneOrDetailsControlForTest : ZUserControl
	{
		public TradeLaneOrDetailsControlForTest()
		{
			Grid = new ZGrid();
			Controls.Add(Grid);
		}

		public ZGrid Grid;
	}
}
