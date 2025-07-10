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
	[TestedType(typeof(WorkflowTriggersModule))]
	sealed class WorkflowTriggersModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WorkflowTriggers;
		}

		[RequiresSTA]
		public void TestFilterControl()
		{
			using (workflowTriggers = new WorkflowTriggersModuleForTest())
			using (var controlForTest = (IDisposable)workflowTriggers.GetNewFilterControl())
			{
				Assert(controlForTest is WorkflowTriggersFilterControl);
			}
		}

		public void TestGridCollection()
		{
			using (workflowTriggers = new WorkflowTriggersModuleForTest())
			{
				var collectionForTest = workflowTriggers.GetNewGridCollection();
				Assert(collectionForTest is BusinessObjectCollection);
			}
		}

		public void TestFilterBusinessObject()
		{
			using (workflowTriggers = new WorkflowTriggersModuleForTest())
			{
				var businessForTest = workflowTriggers.GetNewFilterBusinessObject();
				Assert(businessForTest is FilterBusinessObject);
			}
		}

		#region Implementation

		WorkflowTriggersModuleForTest workflowTriggers;

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
