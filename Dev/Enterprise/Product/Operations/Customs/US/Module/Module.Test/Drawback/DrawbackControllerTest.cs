using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(DrawbackController))]
	sealed class DrawbackControllerTest : ZControllerBasherTest
	{
		public void TestGetForm()
		{
			var controller = new DrawbackController();
			using (var form = controller.ShowNewForm())
			{
				Assert(form is JobDeclarationForm);
			}
		}

		public void TestTopLevelBusinessObject()
		{
			var controller = new DrawbackController();
			AssertEquals(typeof(JobDeclaration), controller.TypeOfTopLevelBusinessObject);
		}

		public void TestIDs()
		{
			var controller = new DrawbackController();
			AssertEquals(ModuleIDs.Customs.US.Drawback, controller.ModuleID);
			AssertEquals(ControllerIDs.Customs.US.Drawback, controller.ID);
		}

		public void TestCheckPoints()
		{
			var declaration = Factory.New<JobDeclaration>();
			var controller = new DrawbackController();
			AssertEquals(Env.Security.USDrawbackEdit, controller.GetCheckPointForDelete(declaration));
			AssertEquals(Env.Security.USDrawbackView, controller.GetCheckPointForView(declaration));
			AssertEquals(Env.Security.USDrawbackNew, controller.GetCheckPointForNew(declaration));
			AssertEquals(Env.Security.USDrawbackEdit, controller.GetCheckPointForEdit(declaration));
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Factory.Save();
			return declaration;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.Drawback;

		protected override Type GetBusinessObjectType() => typeof(JobDeclaration);

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;
	}
}
