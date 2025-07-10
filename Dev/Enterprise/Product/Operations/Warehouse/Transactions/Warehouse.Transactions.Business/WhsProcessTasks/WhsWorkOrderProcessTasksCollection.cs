namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsWorkOrderProcessTasksCollection : WhsDocketProcessTasksCollection
	{
		public WhsWorkOrderProcessTasksCollection(WhsWorkOrder order)
			: base(order)
		{
		}

		public new WhsWorkOrderProcessTasks this[int index] => (WhsWorkOrderProcessTasks)Elements[index];

		public new WhsWorkOrderProcessTasks AddNew() => (WhsWorkOrderProcessTasks)base.AddNew();
	}
}
