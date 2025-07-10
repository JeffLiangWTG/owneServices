using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI.Protest;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Protest.Testing
{
	[TestedType(typeof(ProtestController))]
	sealed class ProtestControllerTest : ZControllerBasherTest
	{
		public void TestGetForm()
		{
			var controller = new ProtestController();
			Assert(controller.ShowNewForm() is ProtestForm);
			controller.LastShownForm.Dispose();
		}

		public void TestIDs()
		{
			var controller = new ProtestController();
			AssertEquals(ModuleIDs.Customs.US.Protest, controller.ModuleID);
			AssertEquals(ControllerIDs.Customs.US.Protest, controller.ID);
		}

		public void TestTopLevelBusinessObject()
		{
			var controller = new ProtestController();
			AssertEquals(typeof(JobDeclaration), controller.TypeOfTopLevelBusinessObject);
		}

		public void TestCheckPoints()
		{
			var declaration = Factory.New<JobDeclaration>();
			var controller = new ProtestController();
			AssertEquals(Env.Security.USProtestModify, controller.GetCheckPointForDelete(declaration));
			AssertEquals(Env.Security.USProtestView, controller.GetCheckPointForView(declaration));
			AssertEquals(Env.Security.USProtestModify, controller.GetCheckPointForNew(declaration));
			AssertEquals(Env.Security.USProtestModify, controller.GetCheckPointForEdit(declaration));
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.MoreCodes.Protest;
			Factory.Save();
			return declaration;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.Protest;

		protected override Type GetBusinessObjectType() => typeof(JobDeclaration);

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;
	}
}
