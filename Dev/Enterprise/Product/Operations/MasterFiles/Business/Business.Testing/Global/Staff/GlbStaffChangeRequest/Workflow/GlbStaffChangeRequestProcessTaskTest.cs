using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffChangeRequestProcessTask))]
	class GlbStaffChangeRequestProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentControllerID()
		{
			AssertEquals(null, Factory.New<GlbStaffChangeRequestProcessTask>().ParentControllerID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var attempt = Factory.New<GlbStaffChangeRequest>();
			return attempt.WorkflowItems.AddNew();
		}
	}
}
