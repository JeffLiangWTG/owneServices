using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class TradeDetailsControlForTest : TradeDetailsControl
	{
		public TradeDetailsControlForTest(OrgSalesProduct salesProduct)
			: base(salesProduct)
		{
			grid = new ZGrid();
			this.BindingSource.SetBindingMember(grid, "EntityTradeDetailsCollection");
			zDropEditColumnStyleInfo1.ColumnName = "PA_TradeMode";
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);

			Controls.Add(grid);
		}

		public override ZGrid TradeDetailsGrid
		{
			get { return grid; }
		}

		readonly ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
		readonly ZGrid grid;
	}
}
