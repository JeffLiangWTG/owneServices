using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class TradeLanesControlForTest : TradeLanesControl
	{
		public TradeLanesControlForTest(OrgSalesProduct product)
			: base(product)
		{
			grid = new ZGrid();
			this.BindingSource.SetBindingMember(grid, "FilterableEntitySalesCollection");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OW_OriginID";
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);

			Controls.Add(grid);
		}

		public override ZGrid TradeLanesGrid
		{
			get { return grid; }
		}

		readonly ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
		readonly ZGrid grid;
	}
}
