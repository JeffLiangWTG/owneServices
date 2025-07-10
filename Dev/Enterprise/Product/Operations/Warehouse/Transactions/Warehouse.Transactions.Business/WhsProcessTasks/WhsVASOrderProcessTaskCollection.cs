using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsVASOrderProcessTaskCollection : ProcessTaskCollection
	{
		public WhsVASOrderProcessTaskCollection(WhsVASOrder vasOrder)
			: base(vasOrder)
		{
		}

		public new WhsVASOrderProcessTask this[int index]
		{
			get { return (WhsVASOrderProcessTask)Elements[index]; }
		}

		public new WhsVASOrderProcessTask AddNew()
		{
			return (WhsVASOrderProcessTask)base.AddNew();
		}
	}
}
