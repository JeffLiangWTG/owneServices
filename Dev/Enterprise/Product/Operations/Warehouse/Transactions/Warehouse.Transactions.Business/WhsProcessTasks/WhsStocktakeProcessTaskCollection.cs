using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsStocktakeProcessTaskCollection : ProcessTaskCollection
	{
		public WhsStocktakeProcessTaskCollection(WhsStocktake stocktake)
			: base(stocktake)
		{
		}

		public new WhsStocktakeProcessTask this[int index]
		{
			get { return (WhsStocktakeProcessTask)Elements[index]; }
		}

		public new WhsStocktakeProcessTask AddNew()
		{
			return (WhsStocktakeProcessTask)base.AddNew();
		}
	}
}
