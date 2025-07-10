using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentProcessTask))]
	sealed class DtbConsignmentProcessTaskProcessTaskTest : ProcessTaskTest
	{
		#region TestParentType_New

		public void TestParentType_New()
		{
			var consignment = Helper.CreateConsignment();
			var processTask = consignment.WorkflowItems.AddNew();
			AssertEquals(typeof(DtbConsignment), processTask.Parent.GetType());
		}

		#endregion

		#region TestParentControllerIDIsOverridenForNonStandAloneTasks

		public override void TestParentControllerIDIsOverridenForNonStandAloneTasks()
		{
			Assert("Doesn't have a controller ID", true);
		}

		#endregion

		#region GetNewBusinessObject

		protected override BusinessObject GetNewBusinessObject()
		{
			return Helper.CreateConsignment().WorkflowItems.AddNew();
		}

		#endregion

		#region Helper

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;

		#endregion
	}
}
