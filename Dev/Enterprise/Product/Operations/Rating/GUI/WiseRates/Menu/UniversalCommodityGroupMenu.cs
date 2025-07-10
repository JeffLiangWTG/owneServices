using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public static class UniversalCommodityGroupMenu
	{
		public static void Add(ZGrid grid)
		{
			var menuItem = new ZMenuItem(RatingConstants.WiseRatesViewContextMenu.ShowCommodityGroups, delegate
			{ ShowCommodityGroups(grid); });
			var contextMenu = grid.ContextMenu;
			contextMenu.MenuItems.Add(menuItem);
		}

		static void ShowCommodityGroups(ZGrid grid)
		{
			var currentItem = grid?.ListManager?.GetCurrent();
			if (currentItem != null && currentItem is WiseEntryView wiseEntryView)
			{
				var commodityGroupViewModelCollection = new CommodityGroupViewModelCollection();
				LoadCommodityGroupsViewModelCollection(wiseEntryView, commodityGroupViewModelCollection);
				ZFormModaliser.ShowDialogAndDispose(new CommodityGroupForm(commodityGroupViewModelCollection));
			}
		}

		static void LoadCommodityGroupsViewModelCollection(WiseEntryView wiseEntryView, CommodityGroupViewModelCollection commodityGroupViewModelCollection)
		{
			if (!wiseEntryView.CommodityGroup.IsEmpty)
			{
				var universalCommodityGroups = RefCommodityCodeLookups.GetUniversalCommodityGroupMapUnfilteredList();

				var commodities = RefCommodityCode.GetCommodities(wiseEntryView.Factory, wiseEntryView.CommodityGroup);
				foreach (var commodity in commodities)
				{
					commodityGroupViewModelCollection.Add(
						universalGroup: wiseEntryView.CommodityGroup,
						universalGroupDescription: universalCommodityGroups.GetDescriptionFromCode(wiseEntryView.CommodityGroup),
						commodityCode: commodity.RH_Code,
						commodityDescription: commodity.RH_DescriptionMultilingual);
				}
			}
		}
	}
}
