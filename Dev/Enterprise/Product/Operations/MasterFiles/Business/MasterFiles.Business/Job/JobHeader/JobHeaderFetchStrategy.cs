using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public JobHeaderFetchStrategy(JobHeader header) : base(header)
		{
		}

		JobHeader jobHeader
		{
			get { return BusinessObject as JobHeader; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddGenericJobQueryHint(jobHeader.JH_ParentID, jobHeader.JH_ParentTableCode);

			Factory.AddFetchHint(OrgAddressSchema.Constants.TableName, jobHeader.JH_OA_LocalChargesAddr);
			Factory.AddFetchHint(OrgAddressSchema.Constants.TableName, jobHeader.JH_OA_AgentCollectAddr);

			Factory.AddFetchHint(GlbBranchSchema.Constants.TableName, jobHeader.JH_GB);
			Factory.AddFetchHint(GlbCompanySchema.Constants.TableName, jobHeader.JH_GC);
			Factory.AddFetchHint(GlbDepartmentSchema.Constants.TableName, jobHeader.JH_GE);

			Factory.AddFetchHint(JobChargeRevRecognitionSchema.D3_JH, jobHeader.PK);

			Factory.AddFetchHint(JobExRateSchema.JF_JH, jobHeader.PK);

			Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, jobHeader.PK);
		}
	}
}
