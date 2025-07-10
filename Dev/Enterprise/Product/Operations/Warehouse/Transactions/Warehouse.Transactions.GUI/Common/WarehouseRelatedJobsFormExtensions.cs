using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.GUI
{
	static class WarehouseToForwardingFormExtensions
	{
		public static void HookCartageClickEvent<T>(this T form, ZMenuItem relatedJobsMenuItem)
			where T : ZForm
		{
			var plugin = form.PlugIns.GetPlugIn(ControllerIDs.CartagePlugin);
			if (relatedJobsMenuItem != null)
			{
				relatedJobsMenuItem.Popup += (sender, e) => plugin.TopLevelMenu.PerformClick();
			}
		}
	}
}
