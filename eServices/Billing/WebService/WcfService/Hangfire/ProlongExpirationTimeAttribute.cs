using System;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace CargoWise.eServices.Billing.WcfService.Hangfire
{
	public class ProlongExpirationTimeAttribute : JobFilterAttribute, IApplyStateFilter
	{
		public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
		{
			context.JobExpirationTimeout = TimeSpan.FromDays(Days);
		}

		public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
		{
			context.JobExpirationTimeout = TimeSpan.FromDays(Days);
		}

		public int Days { get; set; } = 1;
	}
}
