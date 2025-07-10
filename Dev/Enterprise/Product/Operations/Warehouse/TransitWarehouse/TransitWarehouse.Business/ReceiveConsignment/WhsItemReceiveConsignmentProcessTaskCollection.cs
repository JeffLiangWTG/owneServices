using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveConsignmentProcessTaskCollection : ProcessTaskCollection
	{
		public WhsItemReceiveConsignmentProcessTaskCollection(WhsItemReceiveConsignment consignment)
			: base(consignment)
		{
		}

		public new WhsItemReceiveConsignmentProcessTask this[int index]
		{
			get { return (WhsItemReceiveConsignmentProcessTask)Elements[index]; }
		}

		public new WhsItemReceiveConsignmentProcessTask AddNew()
		{
			return (WhsItemReceiveConsignmentProcessTask)base.AddNew();
		}
	}
}