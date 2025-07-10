namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveProcessTasksCollection : WhsDocketProcessTasksCollection
	{
		#region Constructors

		public WhsReceiveProcessTasksCollection(WhsReceive receive)
			: base(receive)
		{
		}

		#endregion

		#region New Methods

		public new WhsReceiveProcessTasks this[int index]
		{
			get { return (WhsReceiveProcessTasks)Elements[index]; }
		}

		public new WhsReceiveProcessTasks AddNew()
		{
			return (WhsReceiveProcessTasks)base.AddNew();
		}

		#endregion
	}
}
