using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	[Serializable]
	public class WhsGroupedUnloadLineInfo : DataObjectInfo
	{
		#region Constructors

		public WhsGroupedUnloadLineInfo()
			: base()
		{
			Attribute1 = "";
			Attribute2 = "";
			Attribute3 = "";
			SerialNumber = "";
			ExpiryDate = new DateTime();
			PackingDate = new DateTime();
			Qty = 0m;
			QtyUQ = "";
			PalletID = "";
			Product = new WhsProductInfo();
			PartAttributes = new WhsProductPartAttributesInfo();
		}

		public WhsGroupedUnloadLineInfo(OrgHeader client, OrgSupplierPart part, WhsWarehouse whs)
			: this()
		{
			if (client == null)
			{
				throw new ArgumentNullException(nameof(client), "Client is required");
			}
			if (part == null)
			{
				throw new ArgumentNullException(nameof(part), "Product is required");
			}
			Product = WhsProductInfo.GetInfo(part, GoodsHandlingInstructionsType.Unload);
			PartAttributes = WhsProductPartAttributesInfo.GetInfo(client, part, whs);
			QtyUQ = part.OP_StockKeepingUnit;
		}

		#endregion

		#region Related Property Infos

		#region Product

		public WhsProductInfo Product { get; set; }

		#endregion

		#region PartAttributes

		public WhsProductPartAttributesInfo PartAttributes { get; set; }

		#endregion

		#endregion

		#region Properties

		public string PalletID { get; set; }

		public decimal Qty { get; set; }

		public string QtyUQ { get; set; }

		public DateTime ExpiryDate { get; set; }

		public DateTime PackingDate { get; set; }

		public string Attribute1 { get; set; }

		public string Attribute2 { get; set; }

		public string Attribute3 { get; set; }

		public string SerialNumber { get; set; }

		#endregion
	}
}
