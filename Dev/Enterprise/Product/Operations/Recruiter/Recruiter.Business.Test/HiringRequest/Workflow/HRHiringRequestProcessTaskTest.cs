using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRHiringRequestProcessTask))]
	sealed class HRHiringRequestProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentControllerID()
		{
			AssertEquals(ControllerIDs.HRHiringRequest, Factory.New<HRHiringRequestProcessTask>().ParentControllerID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var attempt = Factory.New<HRHiringRequest>();
			return attempt.WorkflowItems.AddNew();
		}
	}
}
