using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.HRM.Common.Testing
{
	[TestedType(typeof(ReviewProcessProcessTask))]
	class ReviewProcessProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentControllerID()
		{
			AssertEquals(null, Factory.New<ReviewProcessProcessTask>().ParentControllerID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var attempt = Factory.New<ReviewProcess>();
			return attempt.WorkflowItems.AddNew();
		}
	}
}
