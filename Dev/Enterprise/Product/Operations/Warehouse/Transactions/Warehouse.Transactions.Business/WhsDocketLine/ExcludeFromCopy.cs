using System;

namespace Enterprise.Warehouse.Transactions.Business
{
	[Flags]
	public enum ExcludeFromCopy
	{
		None = 0,
		CustomAttribs = 1,
		PackType = 2,
		Qty = 4,
	}
}
