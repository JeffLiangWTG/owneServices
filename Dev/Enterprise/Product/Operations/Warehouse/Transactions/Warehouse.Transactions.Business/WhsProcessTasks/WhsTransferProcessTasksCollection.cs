namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsTransferProcessTasksCollection : WhsDocketProcessTasksCollection
	{
		#region Constructors

		public WhsTransferProcessTasksCollection(WhsTransfer transfer)
			: base(transfer)
		{
		}

		#endregion

		#region New Methods

		public new WhsTransferProcessTasks this[int index]
		{
			get { return (WhsTransferProcessTasks)Elements[index]; }
		}

		public new WhsTransferProcessTasks AddNew()
		{
			return (WhsTransferProcessTasks)base.AddNew();
		}

		#endregion
	}
}
