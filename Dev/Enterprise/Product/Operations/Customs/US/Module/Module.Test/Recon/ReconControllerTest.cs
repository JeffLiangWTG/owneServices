using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(ReconController))]
	sealed class ReconControllerTest : ZControllerBasherTest
	{
		public void TestGetForm()
		{
			var controller = new ReconController();
			using (var form = controller.ShowNewForm())
			{
				Assert(form is ReconDeclarationForm);
			}
		}

		public void TestTopLevelBusinessObject()
		{
			var controller = new ReconController();
			AssertEquals(typeof(JobDeclaration), controller.TypeOfTopLevelBusinessObject);
		}

		public void TestCheckPoints()
		{
			var declaration = Factory.New<JobDeclaration>();
			var controller = new ReconController();
			AssertEquals(Env.Security.USReconModify, controller.GetCheckPointForDelete(declaration));
			AssertEquals(Env.Security.USReconView, controller.GetCheckPointForView(declaration));
			AssertEquals(Env.Security.USReconModify, controller.GetCheckPointForNew(declaration));
			AssertEquals(Env.Security.USReconModify, controller.GetCheckPointForEdit(declaration));
		}

		public void TestIDs()
		{
			var controller = new ReconController();
			AssertEquals(ModuleIDs.Customs.US.Reconciliation, controller.ModuleID);
			AssertEquals(ControllerIDs.Customs.US.Recon, controller.ID);
		}

		public void TestDeleteMultiple()
		{
			var formBusinessEntity = GetBusinessObjectThatIsInTheDatabase();
			var controller = new ReconController();
			controller.DeleteMultiple(new BusinessObject[] { formBusinessEntity });
			AssertEquals("Reconciliation doesn't support multiple deactivation", ReconController.MultipleDeactivationNotSupported, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconEntry;
			Factory.Save();
			return declaration;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.Recon;

		protected override Type GetBusinessObjectType() => typeof(JobDeclaration);

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;
	}
}
