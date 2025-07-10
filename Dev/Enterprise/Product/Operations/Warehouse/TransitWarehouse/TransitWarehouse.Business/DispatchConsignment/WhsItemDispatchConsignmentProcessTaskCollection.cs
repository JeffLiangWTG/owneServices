using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemDispatchConsignmentProcessTaskCollection : ProcessTaskCollection
	{
		public WhsItemDispatchConsignmentProcessTaskCollection(WhsItemDispatchConsignment consignment)
			: base(consignment)
		{
		}

		public new WhsItemDispatchConsignmentProcessTask this[int index]
		{
			get { return (WhsItemDispatchConsignmentProcessTask)Elements[index]; }
		}

		public new WhsItemDispatchConsignmentProcessTask AddNew()
		{
			return (WhsItemDispatchConsignmentProcessTask)base.AddNew();
		}
	}
}