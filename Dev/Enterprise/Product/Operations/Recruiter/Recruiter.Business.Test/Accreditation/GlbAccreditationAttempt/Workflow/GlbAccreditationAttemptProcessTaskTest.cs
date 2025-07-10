using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(GlbAccreditationAttemptProcessTask))]
	sealed class GlbAccreditationAttemptProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentControllerID()
		{
			AssertEquals(ControllerIDs.GlbAccreditationAttempt, Factory.New<GlbAccreditationAttemptProcessTask>().ParentControllerID);
		}

		public void TestParent()
		{
			var attempt = Factory.New<GlbAccreditationAttempt>();
			var task = attempt.WorkflowItems.AddNew();
			AssertEquals(attempt, task.Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var attempt = Factory.New<GlbAccreditationAttempt>();
			return attempt.WorkflowItems.AddNew();
		}
	}
}
