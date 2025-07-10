using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ExternalRequestTypesController))]
	internal class ExternalRequestTypesControllerBasherTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.ExternalRequestTypes;

		public void TestGetCheckPointForView()
		{
			var controller = new ExternalRequestTypesController();

			AssertEquals(Env.Security.ExternalRequestTypesView, controller.GetCheckPointForView(null));
			AssertEquals(Env.Security.ExternalRequestTypesView, controller.GetCheckPointForView(Factory.New<ExternalRequestType>()));
		}

		public void TestGetCheckPointForNew()
		{
			var controller = new ExternalRequestTypesController();

			AssertEquals(Env.Security.ExternalRequestTypesNew, controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.ExternalRequestTypesNew, controller.GetCheckPointForNew(Factory.New<ExternalRequestType>()));
		}

		public void TestGetCheckPointForEdit()
		{
			var controller = new ExternalRequestTypesController();

			AssertEquals(Env.Security.ExternalRequestTypesEdit, controller.GetCheckPointForEdit(null));
			AssertEquals(Env.Security.ExternalRequestTypesEdit, controller.GetCheckPointForEdit(Factory.New<ExternalRequestType>()));
		}

		public void TestGetCheckPointForDelete()
		{
			var controller = new ExternalRequestTypesController();

			AssertEquals(Env.Security.ExternalRequestTypesDeactivate, controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.ExternalRequestTypesDeactivate, controller.GetCheckPointForDelete(Factory.New<ExternalRequestType>()));
		}

		[RequiresSTA]
		public void TestShowForm()
		{
			AssertEquals(typeof(ExternalRequestTypeForm), this.GetEditFormToShow().GetType());
		}
	}
}
