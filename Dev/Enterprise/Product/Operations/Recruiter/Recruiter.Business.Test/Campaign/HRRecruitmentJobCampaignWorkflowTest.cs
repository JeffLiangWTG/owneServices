using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRRecruitmentJobCampaign))]
	sealed class HRRecruitmentJobCampaignWorkflowTest : WorkflowProviderTest<HRRecruitmentJobCampaign, HRRecruitmentJobCampaignProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
			=> new HRRecruitmentJobCampaignWorkflowDescriptor().Code;

		public override void TestProcessTasksCreatedOnSave()
		{
			BusinessObject.HasChanges = true;
			Factory.Save();
			AssertEquals("HRRecruitmentJobCampaign doesn't support Tasks & Milestones.", true, ((IWorkflowProvider)BusinessObject).WorkflowItems.Count == 0);
		}

		protected override bool WorkflowProviderDoesNotApplyTemplatesWhenSavingFactory => true;
	}
}
