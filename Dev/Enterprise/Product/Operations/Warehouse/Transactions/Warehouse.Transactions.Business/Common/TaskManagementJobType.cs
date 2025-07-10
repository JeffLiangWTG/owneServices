namespace Enterprise.Warehouse.Transactions.Business
{
	public static class TaskManagementJobType
	{
		public const string WhsReceive = nameof(WhsReceive);
		public const string WhsTransfer = nameof(WhsTransfer);
		public const string WhsPick = nameof(WhsPick);
		public const string WhsCycleCountLocation = nameof(WhsCycleCountLocation);
		public const string WhsLoad = nameof(WhsLoad);
	}
}
