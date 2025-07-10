using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HROnBoardingProcessTask))]
	class HROnBoardingProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentControllerID()
		{
			AssertEquals(ControllerIDs.HROnBoarding, Factory.New<HROnBoardingProcessTask>().ParentControllerID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var attempt = Factory.New<HROnBoarding>();
			return attempt.WorkflowItems.AddNew();
		}
	}
}
