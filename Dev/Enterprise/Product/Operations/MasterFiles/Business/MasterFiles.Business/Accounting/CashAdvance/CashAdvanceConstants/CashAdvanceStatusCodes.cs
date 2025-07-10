using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class CashAdvanceStatusCodes
	{
		public static class RequestHeader
		{
			public const string Pending = "PEN";
			public const string Requested = "REQ";
			public const string Paid = "PAI";
			public const string PartiallyPaid = "PPA";
			public const string Invoiced = "INV";
			public const string PartiallyInvoiced = "PIN";
			public const string Cancelled = "CAN";

			public static string PendingDescription => ResString.GetMultilingualString("84564259-DF7D-4307-B93A-DFAE04D1D458", "Pending");
			public static string RequestedDescription => ResString.GetMultilingualString("7FD7001F-33E3-4253-812D-95A5007B38CE", "Requested");
			public static string PaidDescription => ResString.GetMultilingualString("42CE5D9A-7CE8-4B2F-982F-B285F9692C6C", "Paid in Full");
			public static string PartiallyPaidDescription => ResString.GetMultilingualString("7CB4B861-AAAF-4D37-B551-D71D9009E4FD", "Partially Paid");
			public static string InvoicedDescription => ResString.GetMultilingualString("2B1F2E32-129A-46EE-8F16-93A5578349EA", "Invoiced");
			public static string PartiallyInvoicedDescription => ResString.GetMultilingualString("2817CF4E-39A2-4916-A248-B217BCE2A9DB", "Partially Invoiced");
			public static string CancelledDescription => ResString.GetMultilingualString("78AD4F2D-AD37-47EE-A24B-356A1600C342", "Canceled");

			public static CodeDescriptionPairList CodesList
			{
				get
				{
					var codes = new CodeDescriptionPairList();
					codes.AddPair(Pending, PendingDescription);
					codes.AddPair(Requested, RequestedDescription);
					codes.AddPair(Paid, PaidDescription);
					codes.AddPair(PartiallyPaid, PartiallyPaidDescription);
					codes.AddPair(Invoiced, InvoicedDescription);
					codes.AddPair(PartiallyInvoiced, PartiallyInvoicedDescription);
					codes.AddPair(Cancelled, CancelledDescription);
					return codes;
				}
			}
		}

		public static class RequestLine
		{
			public const string Requested = "REQ";
			public const string Paid = "PAI";
			public const string Invoiced = "INV";
			public const string Cancelled = "CAN";

			public static string RequestedDescription => ResString.GetMultilingualString("60E698F7-5664-47CD-8A77-D440E5B3DFB7", "Requested");
			public static string PaidDescription => ResString.GetMultilingualString("65DF1FA0-E600-4E1F-96F9-0DCD4236B509", "Paid in Full");
			public static string InvoicedDescription => ResString.GetMultilingualString("7515FDBF-C3F0-4A99-8885-F614010BE9A0", "Invoiced");
			public static string CancelledDescription => ResString.GetMultilingualString("13E702F3-0FB7-406F-9AED-B86579957555", "Canceled");

			public static CodeDescriptionPairList CodesList
			{
				get
				{
					var codes = new CodeDescriptionPairList();
					codes.AddPair(Requested, RequestedDescription);
					codes.AddPair(Paid, PaidDescription);
					codes.AddPair(Invoiced, InvoicedDescription);
					codes.AddPair(Cancelled, CancelledDescription);
					return codes;
				}
			}
		}
	}
}
