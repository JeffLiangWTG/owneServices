using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.GUI.Stocktake
{
	public class WhsStocktakeProductFilterAttacher : ZRecordAttacher
	{
		public WhsStocktakeProductFilterAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
			: base(destinationCollection, findBoxList, moduleID)
		{
			Collection = (WhsStocktakeProductFilterCollection)destinationCollection;
			UnableToAttachText = Res.GetString("9da4d96d-5dd2-4a87-ad2a-d602d7784e5c", "as they are already in the list.");
		}

		readonly WhsStocktakeProductFilterCollection Collection;

		protected override bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			var pk = bizO.PK;
			var result = false;
			if (!Collection.Any(wsp => wsp.WSP_OP_Product == pk))
			{
				var whsStocktakeProductFilter = Collection.AddNew();
				whsStocktakeProductFilter.WSP_OP_Product = pk;
				result = true;
			}
			return result;
		}
	}
}
