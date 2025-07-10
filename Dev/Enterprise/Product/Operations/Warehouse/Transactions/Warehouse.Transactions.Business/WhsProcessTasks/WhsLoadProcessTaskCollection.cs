using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsLoadProcessTaskCollection : ProcessTaskCollection
	{
		public WhsLoadProcessTaskCollection(WhsLoad load)
			: base(load)
		{
		}

		public new WhsLoadProcessTask this[int index] => (WhsLoadProcessTask)Elements[index];

		public new WhsLoadProcessTask AddNew()
		{
			return (WhsLoadProcessTask)base.AddNew();
		}
	}
}
