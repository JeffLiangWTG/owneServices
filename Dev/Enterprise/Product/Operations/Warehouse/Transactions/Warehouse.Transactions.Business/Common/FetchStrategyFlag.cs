using System;

namespace Enterprise.Warehouse.Transactions.Business.Common
{
	[Flags]
	public enum FetchStrategyFlag
	{
		None = 0,
		RequireClient = 1,
		RequireWarehouse = 2,
		RequireContainer = 4,
		RequireLines = 8,
		RequireJobDocAddress = 16,
		RequireWorkflowItems = 32,
		RequireOrgSupplierPart = 64,
		RequireOrgAddress = 128
	}
}
