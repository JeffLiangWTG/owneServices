using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(WorkflowExceptionTypesController))]
	sealed class WorkflowExceptionTypesControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WorkflowExceptionTypes;
		}

		public void TestGetCheckPointForView()
		{
			var controller = new WorkflowExceptionTypesController();

			AssertEquals(Env.Security.WorkflowExceptionTypesView, controller.GetCheckPointForView(null));
			AssertEquals(Env.Security.WorkflowExceptionTypesView, controller.GetCheckPointForView(Factory.New<ProcessWorkflowExceptionType>()));
		}

		public void TestGetCheckPointForNew()
		{
			var controller = new WorkflowExceptionTypesController();

			AssertEquals(Env.Security.WorkflowExceptionTypesNew, controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.WorkflowExceptionTypesNew, controller.GetCheckPointForNew(Factory.New<ProcessWorkflowExceptionType>()));
		}

		public void TestGetCheckPointForEdit()
		{
			var controller = new WorkflowExceptionTypesController();

			AssertEquals(Env.Security.WorkflowExceptionTypesEdit, controller.GetCheckPointForEdit(null));
			AssertEquals(Env.Security.WorkflowExceptionTypesEdit, controller.GetCheckPointForEdit(Factory.New<ProcessWorkflowExceptionType>()));
		}

		public void TestGetCheckPointForDelete()
		{
			var controller = new WorkflowExceptionTypesController();

			AssertEquals(Env.Security.WorkflowExceptionTypesDeactivate, controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.WorkflowExceptionTypesDeactivate, controller.GetCheckPointForDelete(Factory.New<ProcessWorkflowExceptionType>()));
		}
	}
}
