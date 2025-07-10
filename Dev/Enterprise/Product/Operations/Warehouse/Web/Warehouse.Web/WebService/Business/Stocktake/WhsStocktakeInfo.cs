using System;
using System.Collections.Generic;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsStocktakeInfo : DataObjectInfo
	{
		public WhsStocktakeInfo()
		{
		}

		public WhsStocktakeInfo(WhsStocktake stocktake)
			: this()
		{
			if (stocktake != null)
			{
				PK = stocktake.PK.ToGuid();
				Number = stocktake.WS_StocktakeNumber;
				var client = stocktake.Client;
				if (client != null)
				{
					ClientCode = client.OH_Code;
					ClientPK = client.PK.ToGuid();
				}
				WarehouseCode = stocktake.Warehouse.WW_WarehouseCode;
				Filter = new WhsStocktakeFilterInfo(stocktake);
			}
		}

		#region Properties

		public Guid PK { get; set; }
		public string Number { get; set; }
		public string ClientCode { get; set; }
		public Guid ClientPK { get; set; }
		public string WarehouseCode { get; set; }
		public WhsStocktakeFilterInfo Filter { get; set; }

		public List<WhsProductInfo> ProductInfos
		{
			get { return productInfos ?? (productInfos = new List<WhsProductInfo>()); }
			set { productInfos = value; }
		}

		public List<WhsProductPartAttributesInfo> ProductPartAttributesInfos
		{
			get { return productPartAttributesInfos ?? (productPartAttributesInfos = new List<WhsProductPartAttributesInfo>()); }
			set { productPartAttributesInfos = value; }
		}
		List<WhsProductInfo> productInfos;
		List<WhsProductPartAttributesInfo> productPartAttributesInfos;

		#endregion
	}
}
