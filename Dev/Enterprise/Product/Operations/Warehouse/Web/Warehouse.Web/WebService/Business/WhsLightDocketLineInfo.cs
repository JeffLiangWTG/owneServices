using System;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	[Serializable]
	public class WhsLightDocketLineInfo : DataObjectInfo
	{
		#region Constructors

		public WhsLightDocketLineInfo()
			: base()
		{
			PK = Guid.Empty;
			DocketPK = Guid.Empty;
			ProductCode = "";
			Location = "";
			Location_UserFriendly = "";
			PalletID = "";
			InventoryHeldCode = "";
			Packs = 0m;
			PackUQ = "";
			Attribute1 = "";
			Attribute2 = "";
			Attribute3 = "";
			SerialNumber = "";
			ExpiryDate = new DateTime();
			PackingDate = new DateTime();
		}

		#endregion

		#region Properties

		public Guid DocketPK { get; set; }
		public Guid PK { get; set; }
		public string ProductCode { get; set; }
		public string Location { get; set; }
		public string Location_UserFriendly { get; set; }
		public string PalletID { get; set; }
		public Guid DockDoorLocationPK { get; set; }
		public string InventoryHeldCode { get; set; }
		public decimal Packs { get; set; }
		public string PackUQ { get; set; }
		public string Attribute1 { get; set; }
		public string Attribute2 { get; set; }
		public string Attribute3 { get; set; }
		public string SerialNumber { get; set; }
		public DateTime ExpiryDate { get; set; }
		public DateTime PackingDate { get; set; }

		#endregion
	}
}
