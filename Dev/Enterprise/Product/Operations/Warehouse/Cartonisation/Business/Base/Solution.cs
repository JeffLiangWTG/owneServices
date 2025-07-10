using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Warehouse.Cartonisation.Business
{
	class Solution
	{
		public List<ItemsGroup> AssignedItemGroups => assignedItems ??= new List<ItemsGroup>();
		List<ItemsGroup> assignedItems;

		public List<ItemsGroup> UnassignedItemGroups => unassignedRoutes ??= new List<ItemsGroup>();
		List<ItemsGroup> unassignedRoutes;

		public List<CartonItems> FilledCartons => filledCartons ??= new List<CartonItems>();
		List<CartonItems> filledCartons;

		public decimal SolutionCost { get; set; }

		internal int UnassignedItemsCount => UnassignedItemGroups.Sum(o => o.ItemNodes.Count);
	}
}
