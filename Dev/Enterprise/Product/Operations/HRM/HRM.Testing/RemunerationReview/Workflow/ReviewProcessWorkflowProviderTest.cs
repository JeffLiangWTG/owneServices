using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.HRM.Common.Testing
{
	[TestedType(typeof(ReviewProcess))]
	sealed class ReviewProcessWorkflowProviderTest : WorkflowProviderTest<ReviewProcess, ReviewProcessProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
			=> WorkflowDescriptors.ReviewProcessWorkflowDescriptorCode;

		public override void TestProcessTasksCreatedOnSave()
		{
			BusinessObject.HasChanges = true;
			Factory.Save();
			AssertEquals("ReviewProcess doesn't support Tasks & Milestones.", true, ((IWorkflowProvider)BusinessObject).WorkflowItems.Count == 0);
		}

		protected override bool WorkflowProviderDoesNotApplyTemplatesWhenSavingFactory => true;
	}
}
