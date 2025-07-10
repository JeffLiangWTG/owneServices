using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class JobHeaderStatusList : CodeDescriptionPairList, IJobHeaderStatusList
	{
		public JobHeaderStatusList()
		{
			Add(JobHeaderStatus.Working);
			Add(JobHeaderStatus.WorkOnHold);
			Add(JobHeaderStatus.InvoiceOnHold);
			Add(JobHeaderStatus.CustomsProcessActive);
			Add(JobHeaderStatus.JobReadyForRevenuePosting);
			Add(JobHeaderStatus.JobReadyForCostPosting);
			Add(JobHeaderStatus.JobReadyForRevenueAndCostPosting);
			Add(JobHeaderStatus.JobInvoiced);
			Add(JobHeaderStatus.JobReadyForDelivery);
			Add(JobHeaderStatus.Complete);
			Add(JobHeaderStatus.JobReadyForFinancialClosure);
			Add(JobHeaderStatus.Closed);
			Add(JobHeaderStatus.ScheduledForArchive);
		}
	}
}
