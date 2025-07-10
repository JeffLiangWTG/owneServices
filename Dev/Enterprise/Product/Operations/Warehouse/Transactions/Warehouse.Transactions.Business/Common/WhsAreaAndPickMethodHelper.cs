using System;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class WhsAreaAndPickMethodHelper
	{
		public const string AnyCode = "ANY";
		public static bool IsAnyCode(string pickMethodOrAreaToFilter) => pickMethodOrAreaToFilter != null && pickMethodOrAreaToFilter.Equals(AnyCode, StringComparison.OrdinalIgnoreCase);
	}
}
