using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.Business.Test
{
	public class WorkTaskRelatedItemModuleInfoTest : TestCaseWithFactory
	{
		public void TestWorkItem()
		{
			WorkTaskRelatedItemModuleInfo workItemInfo = WorkTaskRelatedItemModuleInfo.WorkItem(Factory);
			AssertEquals("Work Item", workItemInfo.Caption);
			AssertEquals(WorkTaskRelatedItemTypes.WorkItem, workItemInfo.Type);
			AssertEquals(true, workItemInfo.AllowNew);
			AssertEquals(true, workItemInfo.AllowAttach);
			AssertEquals(ControllerIDs.WorkItem, workItemInfo.ControllerID);
			AssertEquals(ModuleIDs.WorkItem, workItemInfo.ModuleID);
			AssertNotNull(workItemInfo.FindBoxList);

			workItemInfo = WorkTaskRelatedItemModuleInfo.WorkItem(Factory, false);
			AssertEquals("Work Item", workItemInfo.Caption);
			AssertEquals(WorkTaskRelatedItemTypes.WorkItem, workItemInfo.Type);
			AssertEquals(false, workItemInfo.AllowNew);
			AssertEquals(true, workItemInfo.AllowAttach);
			AssertEquals(ControllerIDs.WorkItem, workItemInfo.ControllerID);
			AssertEquals(ModuleIDs.WorkItem, workItemInfo.ModuleID);
			AssertNotNull(workItemInfo.FindBoxList);
		}

		public void TestProject()
		{
			WorkTaskRelatedItemModuleInfo projectInfo = WorkTaskRelatedItemModuleInfo.Project(Factory);
			AssertEquals("Project", projectInfo.Caption);
			AssertEquals(WorkTaskRelatedItemTypes.Project, projectInfo.Type);
			AssertEquals(true, projectInfo.AllowNew);
			AssertEquals(true, projectInfo.AllowAttach);
			AssertEquals(ControllerIDs.Project, projectInfo.ControllerID);
			AssertEquals(ModuleIDs.Project, projectInfo.ModuleID);
			AssertNotNull(projectInfo.FindBoxList);

			projectInfo = WorkTaskRelatedItemModuleInfo.Project(Factory, false);
			AssertEquals("Project", projectInfo.Caption);
			AssertEquals(WorkTaskRelatedItemTypes.Project, projectInfo.Type);
			AssertEquals(false, projectInfo.AllowNew);
			AssertEquals(true, projectInfo.AllowAttach);
			AssertEquals(ControllerIDs.Project, projectInfo.ControllerID);
			AssertEquals(ModuleIDs.Project, projectInfo.ModuleID);
			AssertNotNull(projectInfo.FindBoxList);
		}

		public void TestCustomerServiceTicket()
		{
			var info = WorkTaskRelatedItemModuleInfo.CustomerServiceTicket(Factory);
			AssertEquals("Customer Service Ticket", info.Caption);
			AssertEquals(WorkTaskRelatedItemTypes.CustomerServiceTicket, info.Type);
			AssertEquals(false, info.AllowNew);
			AssertEquals(true, info.AllowAttach);
			AssertEquals(ControllerIDs.CustomerServiceTicket, info.ControllerID);
			AssertEquals(ModuleIDs.CustomerServiceTicket, info.ModuleID);
			AssertNotNull(info.FindBoxList);
		}
	}
}
