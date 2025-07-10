using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ModuleID(ModuleId.SupplierPart)]
	public class WhsStocktakeProductFilterCollection : ActiveBusinessObjectCollection<WhsStocktakeProductFilter>
	{
		public WhsStocktakeProductFilterCollection(BusinessObjectFactory factory, BusinessObject stocktake)
			: base(factory, stocktake, null, WhsStocktakeProductFilterSchema.WSP_WS_Stocktake)
		{
			Argument.NotNull(stocktake, "stocktake");
			WhsStocktakePK = stocktake.PK;
		}

		readonly ZGuid WhsStocktakePK;

		protected override void SetDefaultsForNewElementCore(WhsStocktakeProductFilter newElement)
		{
			newElement.WSP_WS_Stocktake = WhsStocktakePK;
			base.SetDefaultsForNewElementCore(newElement);
		}
	}
}
