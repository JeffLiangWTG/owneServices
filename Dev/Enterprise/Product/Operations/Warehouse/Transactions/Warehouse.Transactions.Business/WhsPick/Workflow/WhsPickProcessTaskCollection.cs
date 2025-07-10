using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickProcessTaskCollection : ProcessTaskCollection
	{
		public WhsPickProcessTaskCollection(WhsPick pick)
			: base(pick)
		{
		}

		public new WhsPickProcessTask this[int index]
		{
			get { return (WhsPickProcessTask)Elements[index]; }
		}

		public new WhsPickProcessTask AddNew()
		{
			return (WhsPickProcessTask)base.AddNew();
		}
	}
}
