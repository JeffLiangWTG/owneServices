using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgContactsStmALogController))]
	sealed class OrgContactsStmALogControllerTest : ZControllerBasherTest
	{
		public override void TestDeleteForm()
		{
			Assert(true);
		}

		public override void TestEditForm()
		{
			Assert(true);
		}

		public override void TestViewForm()
		{
			Assert(true);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return null;
		}

		public void TestModuleID()
		{
			ZController controller = ZControllerFactory.Create(ControllerIDs.OrgContactsStmALog);
			AssertEquals(ModuleIDs.OrgContactsStmALog, controller.ModuleID);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.OrgContactsStmALog;
		}

		public override void TestNewForm()
		{
			Assert(true); // The ZControllerBasherTest creates a new ZController without setting collection and relationship defaults, which ends in a bad way if the developer creates a new orgcontact
						  // without going through an organisation. This is not possible in the application from a user point of view, so this test is invalid for this controller.
						  //
						  // see ZFilterGridModule.GetNewControllerInternal() to see how it is done in the application for users (and works)
		}

		[ExpectNoExceptions()]
		public void TestShowNewForm()
		{
			ZController controller = ZControllerFactory.Create(ControllerIDs.OrgContactsStmALog);
			using (IZForm form = controller.ShowNewForm())
			{
				AssertNull("Should return null as Contact does not have a Header", form);
			}
		}

		public override void TestSaveFormWithCustomsPlugIns()
		{
			Assert(true);
		}
	}
}
