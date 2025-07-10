using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class ProcessTaskHelperTest : WhsTestCaseWithFactory
	{
		#region TestDeleteProcessTasksAndReleatedProcessHeader

		public void TestDeleteProcessTasksAndReleatedProcessHeader()
		{
			var header1 = Factory.New<IProcessHeader>();
			header1.FH_WorkflowType = "ZZ1";
			var task11 = Factory.New<IProcessTask>();
			task11.P9_ParentID = ZGuid.NewZGuid();
			task11.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_FormFlowType = WarehouseTaskFormFlowTypes.UnloadJob;
			var task12 = Factory.New<IProcessTask>();
			task12.P9_ParentID = ZGuid.NewZGuid();
			task12.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task12.P9_FH_ProcessHeader = header1.PK;
			task12.P9_FormFlowType = WarehouseTaskFormFlowTypes.UnloadJob;

			var header2 = Factory.New<IProcessHeader>();
			header2.FH_WorkflowType = "ZZ2";
			var task21 = Factory.New<IProcessTask>();
			task21.P9_ParentID = ZGuid.NewZGuid();
			task21.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task21.P9_FH_ProcessHeader = header2.PK;
			task21.P9_FormFlowType = WarehouseTaskFormFlowTypes.UnloadJob;
			var task22 = Factory.New<IProcessTask>();
			task22.P9_ParentID = ZGuid.NewZGuid();
			task22.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task22.P9_FH_ProcessHeader = header2.PK;
			task22.P9_FormFlowType = WarehouseTaskFormFlowTypes.UnloadJob;
			Factory.Save();

			var processTasksToDelete = new IProcessTask[] { task11, task12, task21 };
			var processTasksNotDelete = new IProcessTask[] { task22 };
			ProcessTaskHelper.DeleteProcessTasksAndRelatedProcessHeader(Factory, processTasksToDelete);

			AssertEquals("tasks should already deleted", 0, Factory.Load<IProcessTask>(new ZQuery(ProcessTasksSchema.PK, processTasksToDelete.Select(t => t.PK))).Length);
			AssertEquals("tasks should not being deleted", task22.PK, Factory.Load<IProcessTask>(new ZQuery(ProcessTasksSchema.PK, processTasksNotDelete.Select(t => t.PK))).Single().PK);

			AssertEquals("header1 should being deleted", true, header1.IsDeleted);
			AssertEquals("header2 should not being deleted because", false, header2.IsDeleted);
		}

		#endregion
	}
}
