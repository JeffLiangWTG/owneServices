using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRRecruitmentJobCampaignWorkflowDescriptor))]
	sealed class HRRecruitmentJobCampaignWorkflowDescriptorTest : WorkflowDescriptorTestCase<HRRecruitmentJobCampaignWorkflowDescriptor>
	{
		public override void TestID()
			=> AssertEquals("HRJ", WorkflowDescriptor.Code);

		public override void TestDescription()
			=> AssertEquals("Correct Desc", "Human Resources Job Opening", WorkflowDescriptor.Description);

		public override void TestSubTypes()
			=> AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
			=> AssertEquals(false, WorkflowDescriptor.RequiresClient);

		public override void TestRequiresDepartment()
			=> AssertEquals(false, WorkflowDescriptor.RequiresDepartment);

		public override void TestRequiresBranch()
			=> AssertEquals(false, WorkflowDescriptor.RequiresBranch);

		public override void TestSupportsEventTracking()
			=> AssertEquals(false, WorkflowDescriptor.SupportsEventTracking);

		protected override bool ExpectingTasksToBeCompanySpecific
			=> false;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
			=> new IWorkflowProvider[]
			{
				(IWorkflowProvider)Factory.New<IHRRecruitmentJobCampaign>()
			};
	}
}
