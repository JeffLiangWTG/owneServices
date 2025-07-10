using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSalesCallProcessTask))]
	class OrgSalesCallProcessTaskTest : ProcessTaskTest
	{
		public void TestProcessTask()
		{
			var call = Factory.NewWithValidTestData<OrgSalesCall>();
			var processTask = call.WorkflowItems.AddNew();

			AssertEquals(typeof(OrgSalesCallProcessTask), processTask.GetType());
			AssertEquals(call, processTask.Parent);
			AssertEquals(ControllerIDs.Communication, processTask.ParentControllerID);
		}

		public void TestSubclassOfCRMProcessTask()
		{
			Assert(GetExpectedBusinessObjectType().IsSubclassOf(typeof(CRMProcessTask)));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<OrgSalesCall>().WorkflowItems.AddNew();
		}

		#endregion
	}
}
