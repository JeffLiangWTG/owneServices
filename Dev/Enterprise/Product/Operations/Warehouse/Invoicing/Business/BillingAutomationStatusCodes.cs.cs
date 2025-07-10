using System.Collections.Generic;

namespace Enterprise.Warehouse.Invoicing.Business
{
	#region BillingAutomationStatusCodes

	public static class BillingAutomationStatusCodes
	{
		public const string BillingAutomationAutorated = "RTD";
		public const string BillingAutomationPosted = "PST";
		public const string BillingAutomationDelivered = "DLV";

		public static IEnumerable<string> AllStatuses => new[] { BillingAutomationAutorated, BillingAutomationPosted, BillingAutomationDelivered };
	}

	#endregion
}
