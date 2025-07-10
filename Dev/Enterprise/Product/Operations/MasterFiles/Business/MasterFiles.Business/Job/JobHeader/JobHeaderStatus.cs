using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class JobHeaderStatus : CodeDescriptionPair
	{
		JobHeaderStatus(object code, MultilingualString description) : base(code, description) { }

		public static class Codes
		{
			public const string Working = "WRK";
			public const string WorkOnHold = "WHL";
			public const string InvoiceOnHold = "IHL";
			public const string CustomsProcessActive = "CUS";
			public const string JobInvoiced = "INV";
			public const string JobReadyForRevenuePosting = "JRB";
			public const string JobReadyForCostPosting = "JRC";
			public const string JobReadyForRevenueAndCostPosting = "JRA";
			public const string JobReadyForDelivery = "RDD";
			public const string Complete = "CMP";
			public const string Closed = "CLS";
			public const string ScheduledForArchive = "ARC";
			public const string JobReadyForFinancialClosure = "JFC";
		}

		public static readonly JobHeaderStatus Working = new JobHeaderStatus(Codes.Working, ResString.GetMultilingualString("MasterFiles.JobHeaderStatus.Working", "Working"));
		public static readonly JobHeaderStatus WorkOnHold = new JobHeaderStatus(Codes.WorkOnHold, ResString.GetMultilingualString("MasterFiles.JobHeaderStatus.WorkOnHold", "Work on Hold"));
		public static readonly JobHeaderStatus InvoiceOnHold = new JobHeaderStatus(Codes.InvoiceOnHold, ResString.GetMultilingualString("MasterFiles.JobHeaderStatus.InvoiceOnHold", "Invoice On Hold"));
		public static readonly JobHeaderStatus CustomsProcessActive = new JobHeaderStatus(Codes.CustomsProcessActive, ResString.GetMultilingualString("MasterFiles.JobHeaderStatus.CustomsProcessActive", "Customs Processing Active"));
		public static readonly JobHeaderStatus JobInvoiced = new JobHeaderStatus(Codes.JobInvoiced, ResString.GetMultilingualString("MasterFiles.JobHeaderStatus.JobInvoiced", "Job Invoiced"));
		public static readonly JobHeaderStatus JobReadyForRevenuePosting = new JobHeaderStatus(Codes.JobReadyForRevenuePosting, ResString.GetMultilingualString("MasterFiles.JobHeaderStatus.JobReadyForRevenuePosting", "Job Ready for Revenue Posting"));
		public static readonly JobHeaderStatus JobReadyForCostPosting = new JobHeaderStatus(Codes.JobReadyForCostPosting, ResString.GetMultilingualString("MasterFiles.JobHeaderStatus.JobReadyForCostPosting", "Job Ready for Cost Posting"));
		public static readonly JobHeaderStatus JobReadyForRevenueAndCostPosting = new JobHeaderStatus(Codes.JobReadyForRevenueAndCostPosting, ResString.GetMultilingualString("MasterFiles.JobHeaderStatus.JobReadyForRevenueAndCostPosting", "Job Ready for Revenue and Cost Posting"));
		public static readonly JobHeaderStatus JobReadyForDelivery = new JobHeaderStatus(Codes.JobReadyForDelivery, ResString.GetMultilingualString("MasterFiles.JobHeaderStatus.JobReadyForDelivery", "Job Ready for Delivery"));
		public static readonly JobHeaderStatus Complete = new JobHeaderStatus(Codes.Complete, ResString.GetMultilingualString("MasterFiles.JobHeaderStatus.Complete", "Complete"));
		public static readonly JobHeaderStatus Closed = new JobHeaderStatus(Codes.Closed, ResString.GetMultilingualString("MasterFiles.JobHeaderStatus.Closed", "Closed"));
		public static readonly JobHeaderStatus ScheduledForArchive = new JobHeaderStatus(Codes.ScheduledForArchive, ResString.GetMultilingualString("MasterFiles.JobHeaderStatus.ScheduledForArchive", "Schedule for Archive"));
		public static readonly JobHeaderStatus JobReadyForFinancialClosure = new JobHeaderStatus(Codes.JobReadyForFinancialClosure, ResString.GetMultilingualString("MasterFiles.JobHeaderStatus.JobReadyForFinancialClosure", "Job Ready for Financial Closure"));
	}
}
