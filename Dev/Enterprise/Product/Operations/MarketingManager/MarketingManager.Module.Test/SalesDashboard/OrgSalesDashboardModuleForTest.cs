using System.Windows.Forms;

namespace Enterprise.MarketingManager.Module.Testing
{
	class OrgSalesDashboardModuleForTest : OrgSalesDashboardModule
	{
		public bool ShowRecentExposed
		{
			get { return ShowRecentItems; }
		}

		public new MenuItem[] GetNewAdditionalContextMenuItems()
		{
			return base.GetNewAdditionalContextMenuItems();
		}
	}
}
