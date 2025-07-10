using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.ServiceTasks.Test
{
	[TestedType(typeof(DataConsistencyCheckServiceTask))]
	sealed class OrphanWIPorACRCheckerTest : DataConsistencyCheckServiceTaskTest
	{
		[SuspendCriticalValidation]
		protected override void PrepareData()
		{
			WIP = Factory.NewWithValidTestData<AccTransactionLines>();
			WIP.AL_GB = GlbBranch.CurrentBranch.PK;
			WIP.AL_GE = GlbDepartment.CurrentDepartment.PK;
			WIP.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			WIP.AL_ReverseDate = ZDateTime.Empty;
			WIP.AL_LineType = Enterprise.ZArchitecture.Core.TransactionLineTypes.WIP;

			LinkedJobCharge = Factory.NewWithValidTestData<JobCharge>();

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;
			jobHeader.JH_JobNum = "S423423";

			WIP.AL_JH = jobHeader.PK;
			WIP.AL_AC = LinkedJobCharge.JR_AC;
			LinkedJobCharge.JR_JH = jobHeader.PK;

			Factory.Save();
		}

		AccTransactionLines? WIP;
		JobCharge? LinkedJobCharge;

		protected override void AssertResult()
		{
			Assert(AccountingMasterFilesRegistry.Instance.OrphanWIPOrACRDetection.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		protected override void AssertLog(string log)
		{
			Assert(log.Contains("Information|Data Consistency Check - Orphan WIPs or Accruals detection - started."));
			Assert(log.Contains(@"Debug|Orphan WIPs or Accruals found for the following company(s) - EDI.
Please use 'Show Only Orphan Transactions' filter option in WIPs or Accruals module to identify the Jobs having these orphan WIPs or Accruals."));
			Assert(log.Contains("Information|Data Consistency Check - Orphan WIPs or Accruals detection - finished."));
		}

		protected override void FixData()
		{
			LinkedJobCharge!.JR_AL_ARLine = WIP!.PK;

			Factory.Save();
		}

		protected override void AssertResultAfterDataFixed()
		{
			Assert(!AccountingMasterFilesRegistry.Instance.OrphanWIPOrACRDetection.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		protected override void AssertLogAfterDataFixed(string log)
		{
			Assert(log.Contains("Information|Data Consistency Check - Orphan WIPs or Accruals detection - started."));
			Assert(log.Contains("Debug|No orphan WIPs or Accruals found for any of the companies."));
			Assert(log.Contains("Debug|Orphan WIPs or Accruals resolved for company - EDI"));
			Assert(log.Contains("Information|Data Consistency Check - Orphan WIPs or Accruals detection - finished."));
		}
	}
}
