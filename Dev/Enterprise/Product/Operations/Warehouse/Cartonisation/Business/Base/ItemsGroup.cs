using System.Collections.Generic;

namespace Enterprise.Warehouse.Cartonisation.Business
{
	public class ItemsGroup
	{
		public List<CartonisableItem> ItemNodes => itemNodes ??= new List<CartonisableItem>();
		List<CartonisableItem> itemNodes;
	}
}
