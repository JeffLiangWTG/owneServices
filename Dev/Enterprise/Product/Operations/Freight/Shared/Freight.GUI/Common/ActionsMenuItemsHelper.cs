using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public static class ActionsMenuItemsHelper
	{
		public static void DisableActionMenuItemsExcludingDefaultsInViewMode(ZForm form)
		{
			ZFormMenuStrategy.DisableActionMenuItemsExcludingDefaultsInViewMode(form);
		}
	}
}
