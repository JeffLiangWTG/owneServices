using System;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(WorkflowMilestonesModule))]
	sealed class WorkflowMilestonesModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WorkflowMilestones;
		}

		[RequiresSTA]
		public void TestFilterControl()
		{
			using (workflowMilestones = new WorkflowMilestonesModuleForTest())
			using (var controlForTest = (IDisposable)workflowMilestones.GetNewFilterControl())
			{
				Assert(controlForTest is WorkflowMilestonesFilterControl);
			}
		}

		public void TestGridCollection()
		{
			using (workflowMilestones = new WorkflowMilestonesModuleForTest())
			{
				var collectionForTest = workflowMilestones.GetNewGridCollection();
				Assert(collectionForTest is BusinessObjectCollection);
			}
		}

		public void TestFilterBusinessObject()
		{
			using (workflowMilestones = new WorkflowMilestonesModuleForTest())
			{
				var businessForTest = workflowMilestones.GetNewFilterBusinessObject();
				Assert(businessForTest is FilterBusinessObject);
			}
		}

		#region Implementation

		WorkflowMilestonesModuleForTest workflowMilestones;

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			base.AddTestObjects(collection);

			var task = Factory.NewWithValidTestData<ProcessTask>();
			var workflow = Factory.New<IProcessHeader>();
			task.P9_FH_ProcessHeader = workflow.PK;

			collection.Add(task);
		}

		#endregion
	}
}
