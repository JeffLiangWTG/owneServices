using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ExternalRequestInfoTemplateController))]
	internal class ExternalRequestInfoTemplateControllerBasherTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.ExternalRequestInfoTemplate;

		public void TestGetCheckPointForView()
		{
			var controller = new ExternalRequestInfoTemplateController();

			AssertEquals(Env.Security.ExternalRequestInfoTemplateView, controller.GetCheckPointForView(null));
			AssertEquals(Env.Security.ExternalRequestInfoTemplateView, controller.GetCheckPointForView(Factory.New<ExternalRequestInfoTemplate>()));
		}

		public void TestGetCheckPointForNew()
		{
			var controller = new ExternalRequestInfoTemplateController();

			AssertEquals(Env.Security.ExternalRequestInfoTemplateNew, controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.ExternalRequestInfoTemplateNew, controller.GetCheckPointForNew(Factory.New<ExternalRequestInfoTemplate>()));
		}

		public void TestGetCheckPointForEdit()
		{
			var controller = new ExternalRequestInfoTemplateController();

			AssertEquals(Env.Security.ExternalRequestInfoTemplateEdit, controller.GetCheckPointForEdit(null));
			AssertEquals(Env.Security.ExternalRequestInfoTemplateEdit, controller.GetCheckPointForEdit(Factory.New<ExternalRequestInfoTemplate>()));
		}

		public void TestGetCheckPointForDelete()
		{
			var controller = new ExternalRequestInfoTemplateController();

			AssertEquals(Env.Security.ExternalRequestInfoTemplateDeactivate, controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.ExternalRequestInfoTemplateDeactivate, controller.GetCheckPointForDelete(Factory.New<ExternalRequestInfoTemplate>()));
		}

		public void TestShowForm()
		{
			AssertEquals(typeof(ExternalRequestInfoTemplateForm), this.GetEditFormToShow().GetType());
		}
	}
}
