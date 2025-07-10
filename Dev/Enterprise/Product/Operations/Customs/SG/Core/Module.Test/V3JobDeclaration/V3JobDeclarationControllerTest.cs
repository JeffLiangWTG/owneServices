using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V3.Business;
using Enterprise.Customs.SG.V3.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V3.Module.Testing
{
	[TestedType(typeof(V3JobDeclarationController))]
	sealed class V3JobDeclarationControllerTest : ZControllerBasherTest
	{
		public void TestGetForm()
		{
			var controller = new V3JobDeclarationController();
			Assert(controller.ShowNewForm() is V3JobDeclarationForm);
			controller.LastShownForm.Dispose();
		}

		public void TestControllerID()
		{
			var controller = new V3JobDeclarationController();
			AssertEquals(ControllerIDs.Customs.SGV3JobDeclaration, controller.ID);
		}

		public void TestTopLevelBusinessObject()
		{
			var controller = new V3JobDeclarationController();
			AssertEquals(typeof(V3JobDeclaration), controller.TypeOfTopLevelBusinessObject);
		}

		public void TestCheckPoints()
		{
			var declaration = Factory.New<V3JobDeclaration>();
			var controller = new V3JobDeclarationController();
			AssertEquals(Env.Security.CustomsDeclarationEnquiryDelete, controller.GetCheckPointForDelete(declaration));
			AssertEquals(Env.Security.CustomsDeclarationEnquiry, controller.GetCheckPointForView(declaration));
			AssertEquals(Env.Security.CustomsDeclarationEnquiryNew, controller.GetCheckPointForNew(declaration));
			AssertEquals(Env.Security.CustomsDeclarationEnquiryEdit, controller.GetCheckPointForEdit(declaration));
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var v3JobDeclaration = Factory.New<V3JobDeclaration>();
			Factory.Save();
			return v3JobDeclaration;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.SGV3JobDeclaration;

		protected override Type GetBusinessObjectType() => typeof(V3JobDeclaration);

		protected override string CountryCode => Core.Constants.CountryCodes.Singapore;
	}
}
