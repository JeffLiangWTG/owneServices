namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDynamicWorkOrderProcessTasksCollection : WhsDocketProcessTasksCollection
	{
		public WhsDynamicWorkOrderProcessTasksCollection(WhsDynamicWorkOrder order)
			: base(order)
		{
		}

		public new WhsDynamicWorkOrderProcessTasks this[int index] => (WhsDynamicWorkOrderProcessTasks)Elements[index];

		public new WhsDynamicWorkOrderProcessTasks AddNew() => (WhsDynamicWorkOrderProcessTasks)base.AddNew();
	}
}
