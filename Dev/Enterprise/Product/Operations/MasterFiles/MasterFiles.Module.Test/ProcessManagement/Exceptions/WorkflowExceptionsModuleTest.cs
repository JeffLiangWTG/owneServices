using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(WorkflowExceptionsModule))]
	sealed class WorkflowExceptionsModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WorkflowExceptions;
		}

		[RequiresSTA]
		public void TestFilterControl()
		{
			workflowExceptions = new WorkflowExceptionsModuleForTest();
			IFilterControl controlForTest = workflowExceptions.GetNewFilterControl();
			Assert(controlForTest is WorkflowExceptionsFilterControl);
			controlForTest.Dispose();
			workflowExceptions.Dispose();
		}

		public void TestGridCollection()
		{
			workflowExceptions = new WorkflowExceptionsModuleForTest();
			IBusinessObjectCollection collectionForTest = workflowExceptions.GetNewGridCollection();
			Assert(collectionForTest is BusinessObjectCollection);
			workflowExceptions.Dispose();
		}

		public void TestFilterBusinessObject()
		{
			workflowExceptions = new WorkflowExceptionsModuleForTest();
			FilterBusinessObject businessForTest = workflowExceptions.GetNewFilterBusinessObject();
			Assert(businessForTest is FilterBusinessObject);
			workflowExceptions.Dispose();
		}

		#region Implementation

		WorkflowExceptionsModuleForTest workflowExceptions;

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
