using System.Collections.Generic;
using Aga.Controls.Tree;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class TradedSalesAnalysisTreeControlForTest : TradedSalesAnalysisTreeControl
	{
		public IEnumerable<TreeColumn> TreeColumns_Exposed
		{
			get { return TreeColumns; }
		}

		public TreeColumn JobRevenueColumn_Exposed
		{
			get { return JobRevenueColumn; }
		}
		public TreeColumn JobCostColumn_Exposed
		{
			get { return JobCostColumn; }
		}
		public TreeColumn JobProfitColumn_Exposed
		{
			get { return JobProfitColumn; }
		}
		public TreeColumn TEUQuantityColumn_Exposed
		{
			get { return TEUQuantityColumn; }
		}
	}
}
