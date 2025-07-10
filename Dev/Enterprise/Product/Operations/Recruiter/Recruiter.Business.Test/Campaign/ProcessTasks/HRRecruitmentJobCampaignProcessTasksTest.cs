using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRRecruitmentJobCampaignProcessTask))]
	sealed class HRRecruitmentJobCampaignProcessTasksTest : ProcessTaskTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var jobOpening = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			return ((IWorkflowProvider)jobOpening).WorkflowItems
				.AddNew();
		}
	}
}
