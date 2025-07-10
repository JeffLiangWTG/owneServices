using System;
using System.Collections.Generic;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	[Serializable]
	public class WhsGroupedInventoryLineInfoCollection : WhsInventoryLineBaseInfoCollection<WhsGroupedInventoryInfo>
	{
		public WhsGroupedInventoryLineInfoCollection()
		{ }

		public override List<WhsGroupedInventoryInfo> InventoryLineInfos
		{
			get { return inventoryLineInfos ?? (inventoryLineInfos = new List<WhsGroupedInventoryInfo>()); }
			set { inventoryLineInfos = value; }
		}
		List<WhsGroupedInventoryInfo> inventoryLineInfos;
	}
}
