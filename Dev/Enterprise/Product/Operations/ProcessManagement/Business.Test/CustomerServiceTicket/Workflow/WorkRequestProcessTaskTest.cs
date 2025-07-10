using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkRequestProcessTask))]
	class WorkRequestProcessTaskTest : ProcessTaskTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return ProcessMgmtTestHelper.CreateWorkRequest(Factory).WorkflowItems.Tasks.AddNew();
		}

		#endregion
	}
}
