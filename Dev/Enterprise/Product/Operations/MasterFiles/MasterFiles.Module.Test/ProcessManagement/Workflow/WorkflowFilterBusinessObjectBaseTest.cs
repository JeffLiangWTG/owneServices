using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class WorkflowFilterBusinessObjectBaseTest : TestCaseWithFactory
	{
		public void TestProcessTaskFilterSecurityLoad()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWorkflow = ((IWorkflowProvider)shipment);

			for (var i = 0; i < 10; i++)
			{
				shipmentWorkflow.WorkflowItems.Milestones.AddNew();
				shipmentWorkflow.WorkflowItems.Tasks.AddNew();
				shipmentWorkflow.WorkflowItems.Exceptions.AddNew();
				shipmentWorkflow.WorkflowItems.Triggers.AddNew();
			}

			for (var i = 0; i < 10; i++)
			{
				Dummy.WorkflowItems.Milestones.AddNew();
				Dummy.WorkflowItems.Tasks.AddNew();
				Dummy.WorkflowItems.Exceptions.AddNew();
				Dummy.WorkflowItems.Triggers.AddNew();
			}

			Factory.Save();

			foreach (var filter in Filters)
			{
				filter.ParentType = Dummy.GetType();
				filter.QueryObjectType = Factory.New<IProcessTask>().GetType();

				AssertContains("P9_ParentTableCode = 'Z0'", filter.Filter.LiteralTextADO);

				var processTasks = Factory.Load<IProcessTask>(filter.Filter);
				AssertEquals(10, processTasks.Length);
			}
		}

		public void TestProcessHeaderDoesNotContainSecurityFilter()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessHeader))
			{
				var jobWorkflowFilterBusinessObject = module.FilterBusinessObject;
				var tasksModuleFilter = jobWorkflowFilterBusinessObject.AddFilterStrip<TasksModuleFilter>("Tasks");

				AssertNotContains("P9_ParentTableCode =", jobWorkflowFilterBusinessObject.Filter.LiteralTextADO);
			}
		}

		List<WorkflowFilterBusinessObjectBase> Filters
		{
			get
			{
				if (filters == null)
				{
					filters = new List<WorkflowFilterBusinessObjectBase>()
					{
						new WorkflowExceptionsFilterBusinessObject(),
						new WorkflowMilestonesFilterBusinessObject(),
						new ProcessTaskFilterBusinessObject(),
						new WorkflowTriggersFilterBusinessObject(),
					};
				}

				return filters;
			}
		}
		List<WorkflowFilterBusinessObjectBase> filters;

		#region Implementation

		DummyWithWorkflow Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyWithWorkflow>()); }
		}
		DummyWithWorkflow dummy;

		#endregion
	}
}
