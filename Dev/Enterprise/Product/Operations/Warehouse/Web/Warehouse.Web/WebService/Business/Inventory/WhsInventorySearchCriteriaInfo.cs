using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	[Serializable]
	public class WhsInventorySearchCriteriaInfo
	{
		public WhsInventorySearchCriteriaInfo()
		{
			JoinCondition = SearchJoinCondition.And;
			ClientCode = "";
			ProductPK = Guid.Empty;
			ProductCode = "";
			Location = "";
			PalletID = "";
			Attribute1 = "";
			Attribute2 = "";
			Attribute3 = "";
			SerialNumber = "";
			ExpiryDate = new DateTime();
			PackingDate = new DateTime();
			DestinationInventoryLevel = WhsInventoryLevel.Level1;
		}

		public WhsInventorySearchCriteriaInfo(string clientCode, string productCode, string location, string palletID,
			string attribute1 = "", string attribute2 = "", string attribute3 = "", string serialNumber = "",
			DateTime expiryDate = new DateTime(), DateTime packingDate = new DateTime(), WhsInventoryLevel destLevel = WhsInventoryLevel.Level1,
			SearchJoinCondition joinCondition = SearchJoinCondition.And, SearchJoinCondition palletIDAndLocationJoinCondition = SearchJoinCondition.And)
		{
			ClientCode = clientCode;
			ProductCode = productCode;
			ProductPK = Guid.Empty;
			Location = location;
			PalletID = palletID;
			Attribute1 = attribute1;
			Attribute2 = attribute2;
			Attribute3 = attribute3;
			SerialNumber = serialNumber;
			ExpiryDate = expiryDate;
			PackingDate = packingDate;
			DestinationInventoryLevel = destLevel;
			JoinCondition = joinCondition;
			PalletIDAndLocationJoinCondition = palletIDAndLocationJoinCondition;
		}

		public string ClientCode { get; set; }
		public Guid ProductPK { get; set; }
		public string ProductCode { get; set; }
		public string Location { get; set; }
		public string PalletID { get; set; }
		public string Attribute1 { get; set; }
		public string Attribute2 { get; set; }
		public string Attribute3 { get; set; }
		public string SerialNumber { get; set; }
		public DateTime ExpiryDate { get; set; }
		public DateTime PackingDate { get; set; }

		public SearchJoinCondition JoinCondition { get; set; }

		public SearchJoinCondition PalletIDAndLocationJoinCondition { get; set; }

		public WhsInventoryLevel DestinationInventoryLevel { get; set; }

		#region UpdateInventoryLevel

		public void UpdateInventoryLevel(WhsInventoryView inventory)
		{
			if (DestinationInventoryLevel == WhsInventoryLevel.Level2)
			{
				if (!IsAnyAttributesUsedButNotSerialNumberOrReleaseCaptured(inventory.Product, inventory.Client))
				{
					DestinationInventoryLevel = WhsInventoryLevel.Level3;
				}
			}
		}

		#endregion

		#region IsAnyAttributesUsedButNotSerialNumberOrReleaseCaptured

		/// <summary>
		/// a) Non-SN with RC --> Level 3
		/// b) SN with RC --> Level 3
		/// c) Non-SN Non-RC --> Level 2
		/// Any of a, b has other attribute satisfy c) we go to level 2 
		/// </summary>
		public static bool IsAnyAttributesUsedButNotSerialNumberOrReleaseCaptured(WhsProduct product, OrgHeader client)
		{
			return !GetIsSerialNumberUsedButNotReleaseCaptured(product, client)
				&& GetIsAnyAttributeUsedButNotReleaseCaptured(product, client);
		}

		static bool GetIsSerialNumberUsedButNotReleaseCaptured(WhsProduct product, OrgHeader client)
		{
			return product.IsSerialNumberUsedAndNotReleaseCaptured(client);
		}

		static bool GetIsAnyAttributeUsedButNotReleaseCaptured(WhsProduct product, OrgHeader client)
		{
			return (product.IsPartAttributeUsed(client, 1) && !product.IsPartAttribReleaseCaptured(client, 1))
				|| (product.IsPartAttributeUsed(client, 2) && !product.IsPartAttribReleaseCaptured(client, 2))
				|| (product.IsPartAttributeUsed(client, 3) && !product.IsPartAttribReleaseCaptured(client, 3))
				|| (product.IsExpiryDateUsed(client))
				|| (product.IsPackingDateUsed(client));
		}

		#endregion
	}

	public enum SearchJoinCondition
	{
		And,
		Or
	}

	public enum WhsInventoryLevel
	{
		Level1,
		Level2,
		Level3
	}
}
