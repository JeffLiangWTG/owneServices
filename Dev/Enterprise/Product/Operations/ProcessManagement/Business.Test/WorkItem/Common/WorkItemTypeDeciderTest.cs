using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ProcessManagement.Business.Test
{
	class WorkItemTypeDeciderTest : TestCaseWithFactory
	{
		#region TestGetTypeForBinding

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(WorkItemCommon), new WorkItemTypeDecider().GetTypeForBinding());
		}

		#endregion

		#region TestGetTypeForNew

		public void TestGetTypeForNew()
		{
			AssertExceptionThrown(typeof(NotSupportedException), "Abstract type.", () => new WorkItemTypeDecider().GetTypeForNew());
		}

		#endregion

		#region TestGetTypeForLoad

		public void TestGetTypeForLoad()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			Factory.Save();

			AssertEquals(typeof(WorkItem), new WorkItemTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)workItem).Row, new BusinessObjectFactory()));
		}

		#endregion
	}
}
