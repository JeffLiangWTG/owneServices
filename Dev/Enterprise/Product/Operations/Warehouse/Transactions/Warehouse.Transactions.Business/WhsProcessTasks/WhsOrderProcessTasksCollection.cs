namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderProcessTasksCollection : WhsDocketProcessTasksCollection
	{
		#region Constructors

		public WhsOrderProcessTasksCollection(WhsOrder order)
			: base(order)
		{
		}

		#endregion

		#region New Methods

		public new WhsOrderProcessTasks this[int index]
		{
			get { return (WhsOrderProcessTasks)Elements[index]; }
		}

		public new WhsOrderProcessTasks AddNew()
		{
			return (WhsOrderProcessTasks)base.AddNew();
		}

		#endregion
	}
}
