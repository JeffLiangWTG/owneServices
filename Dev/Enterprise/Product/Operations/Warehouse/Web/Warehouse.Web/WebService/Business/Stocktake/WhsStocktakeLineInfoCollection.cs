using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	public class WhsStocktakeLineInfoCollection : DataObjectInfoCollection<WhsStocktakeLineInfo>
	{
		public WhsStocktakeLineInfoCollection(WhsStocktakeInfo stocktakeInfo, WhsStocktakeLineCollectionND stocktakeLines)
		{
			foreach (WhsStocktakeLine stocktakeLine in stocktakeLines)
			{
				Add(new WhsStocktakeLineInfo(stocktakeInfo, stocktakeLine));
			}
		}

		public WhsStocktakeLineInfoCollection()
			: base()
		{
		}
	}
}
