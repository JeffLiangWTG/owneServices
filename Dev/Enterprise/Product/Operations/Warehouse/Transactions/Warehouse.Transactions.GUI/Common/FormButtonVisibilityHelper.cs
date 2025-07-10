using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.GUI.Common
{
	static class FormButtonVisibilityHelper
	{
		public static bool ShowAttachButton(WhsPick pick) => ShowOrderModuleButtons(pick);
		public static bool ShowDetachButton(WhsPick pick) => !pick.IsFinalisedOrCancelled && !pick.IsReadyForPlanningOrPlanned;
		public static bool ShowEditButton(WhsPick pick) => ShowOrderModuleButtons(pick);
		public static bool ShowNewButton(WhsPick pick) => ShowOrderModuleButtons(pick);
		public static bool ShowAutoPickButton(WhsPick pick) => ShowOrderModuleButtons(pick);
		public static bool ShowCancelPickButton(WhsPick pick) => ShowOrderModuleButtons(pick);
		public static bool ShowSelectAllButton(WhsPick pick) => ShowOrderModuleButtons(pick);
		public static bool ShowClearAllButton(WhsPick pick) => ShowOrderModuleButtons(pick);
		public static bool ShowReleaseButton(WhsPick pick) => !pick.IsWorkOrderPick;
		internal static bool ShowOrderModuleButtons(WhsPick pick) => !pick.IsFinalisedOrCancelled && !pick.WP_IsCartonised && !pick.IsCartonising && !pick.IsReadyForPlanningOrPlanned;
	}
}
