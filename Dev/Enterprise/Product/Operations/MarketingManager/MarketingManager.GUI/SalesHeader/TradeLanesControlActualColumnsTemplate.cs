using Enterprise.Core.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TradeLanesControlActualColumnsTemplate : ZUserControl
	{
		public TradeLanesControlActualColumnsTemplate()
		{
			InitializeComponent();
		}

		public static void AddActualColumnStyles(ZGrid grid)
		{
			foreach (ZGridColumnInfo columnInfo in grid.ColumnStyles.ToArray())
			{
				if (columnInfo.ColumnName == EntitySalesWrapper.Schema.CommittedAnnualValue
					|| columnInfo.ColumnName == EntitySalesWrapper.Schema.CommittedMonthlyValue)
				{
					grid.ColumnStyles.Remove(columnInfo);
				}
			}

			using (var template = new TradeLanesControlActualColumnsTemplate())
			{
				grid.ColumnStyles.InsertRange(0, template.PreColumnsGrid.ColumnStyles);
				grid.ColumnStyles.AddRange(template.PostColumnsGrid.ColumnStyles);
			}
		}
	}
}
