using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public abstract class WhsInventoryLineBaseInfoCollection<T>
	{
		#region InventoryLineInfos

		public abstract List<T> InventoryLineInfos { get; set; }

		#endregion

		#region ProductInfos

		public List<WhsProductInfo> ProductInfos
		{
			get { return productInfos ?? (productInfos = new List<WhsProductInfo>()); }
			set { productInfos = value; }
		}
		List<WhsProductInfo> productInfos;

		#endregion

		#region ProductPartAttributesInfos

		public List<WhsProductPartAttributesInfo> ProductPartAttributesInfos
		{
			get { return productPartAttributesInfos ?? (productPartAttributesInfos = new List<WhsProductPartAttributesInfo>()); }
			set { productPartAttributesInfos = value; }
		}
		List<WhsProductPartAttributesInfo> productPartAttributesInfos;

		#endregion
	}

	[Serializable]
	public class WhsInventoryLineInfoCollection : WhsInventoryLineBaseInfoCollection<WhsInventoryLineInfo>
	{
		public WhsInventoryLineInfoCollection()
		{ }

		public static WhsInventoryLineInfoCollection GetWhsInventoryLineInfoCollectionWithFetchHints(IEnumerable<WhsInventoryView> lines)
		{
			var inventoryLineCollection = new WhsInventoryLineInfoCollection();

			if (lines.Any())
			{
				var factory = lines.First().Factory;
				var query = new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, lines.Select(line => line.WI_WE_InDocketLine));
				factory.AddFetchHint(typeof(WhsPickLine), query);
			}

			foreach (var line in lines)
			{
				inventoryLineCollection.InventoryLineInfos.Add(new WhsInventoryLineInfo(inventoryLineCollection, line));
			}

			return inventoryLineCollection;
		}

		public override List<WhsInventoryLineInfo> InventoryLineInfos
		{
			get { return inventoryLineInfos ?? (inventoryLineInfos = new List<WhsInventoryLineInfo>()); }
			set { inventoryLineInfos = value; }
		}
		List<WhsInventoryLineInfo> inventoryLineInfos;
	}
}
