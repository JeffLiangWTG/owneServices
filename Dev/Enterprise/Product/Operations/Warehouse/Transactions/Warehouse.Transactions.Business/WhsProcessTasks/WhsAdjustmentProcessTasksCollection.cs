namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAdjustmentProcessTasksCollection : WhsDocketProcessTasksCollection
	{
		#region Constructors

		public WhsAdjustmentProcessTasksCollection(WhsAdjustment adjustment)
			: base(adjustment)
		{
		}

		#endregion

		#region New Methods

		public new WhsAdjustmentProcessTasks this[int index]
		{
			get { return (WhsAdjustmentProcessTasks)Elements[index]; }
		}

		public new WhsAdjustmentProcessTasks AddNew()
		{
			return (WhsAdjustmentProcessTasks)base.AddNew();
		}

		#endregion
	}
}
