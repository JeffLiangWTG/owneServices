using System;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsStocktakeFilterInfo : DataObjectInfo
	{
		public WhsStocktakeFilterInfo()
		{
		}

		public WhsStocktakeFilterInfo(WhsStocktake stocktake)
			: this()
		{
			if (stocktake != null)
			{
				Cycle = stocktake.WS_StocktakeCycle;
				Commodity = stocktake.CommodityCode != null ? stocktake.CommodityCode.RH_Code : null;
				PickMethod = stocktake.WS_PickMethod;
				Row = stocktake.SelectedRow != null ? stocktake.SelectedRow.WR_Name : null;
				Area = stocktake.SelectedArea != null ? stocktake.SelectedArea.WA_NameMultilingual : null;
				ABCAnalysisCategory = stocktake.WS_ABCAnalysisCategory;
				Location = stocktake.LocationString;
				StocktakeType = stocktake.WS_StocktakeType;
				if (stocktake.ProductFilterCollection.Count == 1)
				{
					Product = stocktake.ProductFilterCollection[0].ProductCode;
				}
				else if (stocktake.ProductFilterCollection.Count > 1)
				{
					Product = Res.GetString("89df4a33-f3b1-44d7-a954-f85358188b2a", "Many");
				}
			}
		}

		#region Properties

		public string Cycle { get; set; }
		public string Product { get; set; }
		public string Commodity { get; set; }
		public string PickMethod { get; set; }
		public string Row { get; set; }
		public string Area { get; set; }
		public string ABCAnalysisCategory { get; set; }
		public string Location { get; set; }
		public string StocktakeType { get; set; }

		#endregion
	}
}
