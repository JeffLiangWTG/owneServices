using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	class TaskCreationWorkflowInfoTest : TestCaseWithFactory
	{
		public void TestConstructor_NullThrows()
		{
			var branchPK = ZGuid.NewZGuid();
			var warehousePK = ZGuid.NewZGuid();
			var releaseGroupPK = ZGuid.NewZGuid();
			var workflowProvider = Mock.Of<IWorkflowProvider>();
			AssertExceptionThrown<ArgumentNullException>(() => new TaskCreationWorkflowInfo(null, branchPK, warehousePK, releaseGroupPK, workflowProvider));
			AssertExceptionThrown<ArgumentNullException>(() => new TaskCreationWorkflowInfo("TEST", branchPK, warehousePK, releaseGroupPK, null));
		}

		public void TestConstructor()
		{
			var branchPK = ZGuid.NewZGuid();
			var warehousePK = ZGuid.NewZGuid();
			var releaseGroupPK = ZGuid.NewZGuid();
			var workflowProvider = Mock.Of<IWorkflowProvider>();
			var workflowInfo = new TaskCreationWorkflowInfo("TEST", branchPK, warehousePK, releaseGroupPK, workflowProvider);
			AssertEquals(nameof(workflowInfo.NameForLog), "TEST", workflowInfo.NameForLog);
			AssertEquals(nameof(workflowInfo.BranchPK), branchPK, workflowInfo.BranchPK);
			AssertEquals(nameof(workflowInfo.ReleaseGroupPK), releaseGroupPK, workflowInfo.ReleaseGroupPK);
		}
	}
}
